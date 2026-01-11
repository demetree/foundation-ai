import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditResourceService, AuditResourceData, AuditResourceSubmitData } from '../../../auditor-data-services/audit-resource.service';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-audit-resource-detail',
  templateUrl: './audit-resource-detail.component.html',
  styleUrls: ['./audit-resource-detail.component.scss']
})

export class AuditResourceDetailComponent implements OnInit, CanComponentDeactivate {

  auditResourceForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });


  public auditResourceId: string | null = null;
  public auditResourceData: AuditResourceData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditResources$ = this.auditResourceService.GetAuditResourceList();
  auditEvents$ = this.auditEventService.GetAuditEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditResourceService: AuditResourceService,
    public auditEventService: AuditEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditResourceId from the route parameters
    this.auditResourceId = this.route.snapshot.paramMap.get('auditResourceId');

    if (this.auditResourceId === 'new' ||
        this.auditResourceId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditResourceData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Resource';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Resource';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditResourceForm.dirty) {
      return confirm('You have unsaved Audit Resource changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditResourceId != null && this.auditResourceId !== 'new') {

      const id = parseInt(this.auditResourceId, 10);

      if (!isNaN(id)) {
        return { auditResourceId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditResource data for the current auditResourceId.
  *
  * Fully respects the AuditResourceService caching strategy and error handling strategy.
  *
  * @param forceLoadAndDisplaySuccessAlert
  *   - true  will bypass cache entirely and show success alert message
  *   - false/null will use cache if available, no alert message
  */
  public loadData(forceLoadAndDisplaySuccessAlert: boolean | null = null): void {

    //
    // Start loading indicator immediately
    //
    this.isLoadingSubject.next(true);


    //
    // Permission Check
    //
    if (!this.auditResourceService.userIsAuditorAuditResourceReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditResources.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditResourceId
    //
    if (!this.auditResourceId) {

      this.alertService.showMessage('No AuditResource ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditResourceId = Number(this.auditResourceId);

    if (isNaN(auditResourceId) || auditResourceId <= 0) {

      this.alertService.showMessage(`Invalid Audit Resource ID: "${this.auditResourceId}"`,
                                    'Invalid ID',
                                    MessageSeverity.error
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {
      // This is the most targeted way: clear only this AuditResource + relations

      this.auditResourceService.ClearRecordCache(auditResourceId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditResourceService.GetAuditResource(auditResourceId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditResourceData) => {

        //
        // Success path — auditResourceData can legitimately be null if 404'd but request succeeded
        //
        if (!auditResourceData) {

          this.handleAuditResourceNotFound(auditResourceId);

        } else {

          this.auditResourceData = auditResourceData;
          this.buildFormValues(this.auditResourceData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditResource loaded successfully',
              '',
              MessageSeverity.success
            );
          }
        }

        this.isLoadingSubject.next(false);
      },

      error: (error: any) => {
        //
        // All HTTP/network/parsing errors flow here
        // The service already stripped sensitive info and re-threw cleanly
        //
        this.handleAuditResourceLoadError(error, auditResourceId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditResourceNotFound(auditResourceId: number): void {

    this.auditResourceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditResource #${auditResourceId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditResourceLoadError(error: any, auditResourceId: number): void {

    let message = 'Failed to load Audit Resource.';
    let title = 'Load Error';
    let severity = MessageSeverity.error;

    //
    // Leverage HTTP status if available
    //
    if (error?.status) {
      switch (error.status) {
        case 401:
          message = 'Your session has expired. Please log in again.';
          title = 'Unauthorized';
          break;
        case 403:
          message = 'You do not have permission to view this Audit Resource.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Resource #${auditResourceId} was not found.`;
          title = 'Not Found';
          severity = MessageSeverity.warn;
          break;
        case 500:
          message = 'Server error. Please try again or contact support.';
          title = 'Server Error';
          break;
        case 0:
          message = 'Cannot reach server. Check your internet connection.';
          title = 'Offline';
          break;
        default:
          message = `Server error ${error.status || 'unknown'}: ${error.statusText || 'Request failed'}`;
      }
    } else {
      message = error?.message || message;
    }

    console.error(`Audit Resource load failed (ID: ${auditResourceId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditResourceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditResourceData: AuditResourceData | null) {

    if (auditResourceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditResourceForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditResourceForm.reset({
        name: auditResourceData.name ?? '',
        comments: auditResourceData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditResourceData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditResourceForm.markAsPristine();
    this.auditResourceForm.markAsUntouched();
  }

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.auditResourceService.userIsAuditorAuditResourceWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Resources", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditResourceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditResourceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditResourceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditResourceSubmitData: AuditResourceSubmitData = {
        id: this.auditResourceData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditResourceService.PutAuditResource(auditResourceSubmitData.id, auditResourceSubmitData)
      : this.auditResourceService.PostAuditResource(auditResourceSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditResourceData) => {

        this.auditResourceService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Resource's detail page
          //
          this.auditResourceForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditResourceForm.markAsUntouched();

          this.router.navigate(['/auditresources', savedAuditResourceData.id]);
          this.alertService.showMessage('Audit Resource added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditResourceData = savedAuditResourceData;
          this.buildFormValues(this.auditResourceData);

          this.alertService.showMessage("Audit Resource saved successfully", '', MessageSeverity.success);
        }
      },
      error: (err) => {

            let errorMessage: string;

            // Check if err is an Error object (e.g., new Error('message'))
            if (err instanceof Error) {
                errorMessage = err.message || 'An unexpected error occurred.';
            }
            // Check if err is a ServerError object with status and error properties
            else if (err.status && err.error)
            {
                if (err.status === 403)
                {
                    errorMessage = err.error?.message ||
                                   'You do not have permission to save this Audit Resource.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Resource.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Resource could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditResourceReader(): boolean {
    return this.auditResourceService.userIsAuditorAuditResourceReader();
  }

  public userIsAuditorAuditResourceWriter(): boolean {
    return this.auditResourceService.userIsAuditorAuditResourceWriter();
  }
}
