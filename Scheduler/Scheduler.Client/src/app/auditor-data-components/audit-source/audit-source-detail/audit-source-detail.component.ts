import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditSourceService, AuditSourceData, AuditSourceSubmitData } from '../../../auditor-data-services/audit-source.service';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-audit-source-detail',
  templateUrl: './audit-source-detail.component.html',
  styleUrls: ['./audit-source-detail.component.scss']
})

export class AuditSourceDetailComponent implements OnInit, CanComponentDeactivate {

  auditSourceForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });


  public auditSourceId: string | null = null;
  public auditSourceData: AuditSourceData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditSources$ = this.auditSourceService.GetAuditSourceList();
  auditEvents$ = this.auditEventService.GetAuditEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditSourceService: AuditSourceService,
    public auditEventService: AuditEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditSourceId from the route parameters
    this.auditSourceId = this.route.snapshot.paramMap.get('auditSourceId');

    if (this.auditSourceId === 'new' ||
        this.auditSourceId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditSourceData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Source';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Source';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditSourceForm.dirty) {
      return confirm('You have unsaved Audit Source changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditSourceId != null && this.auditSourceId !== 'new') {

      const id = parseInt(this.auditSourceId, 10);

      if (!isNaN(id)) {
        return { auditSourceId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditSource data for the current auditSourceId.
  *
  * Fully respects the AuditSourceService caching strategy and error handling strategy.
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
    if (!this.auditSourceService.userIsAuditorAuditSourceReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditSources.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditSourceId
    //
    if (!this.auditSourceId) {

      this.alertService.showMessage('No AuditSource ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditSourceId = Number(this.auditSourceId);

    if (isNaN(auditSourceId) || auditSourceId <= 0) {

      this.alertService.showMessage(`Invalid Audit Source ID: "${this.auditSourceId}"`,
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
      // This is the most targeted way: clear only this AuditSource + relations

      this.auditSourceService.ClearRecordCache(auditSourceId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditSourceService.GetAuditSource(auditSourceId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditSourceData) => {

        //
        // Success path — auditSourceData can legitimately be null if 404'd but request succeeded
        //
        if (!auditSourceData) {

          this.handleAuditSourceNotFound(auditSourceId);

        } else {

          this.auditSourceData = auditSourceData;
          this.buildFormValues(this.auditSourceData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditSource loaded successfully',
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
        this.handleAuditSourceLoadError(error, auditSourceId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditSourceNotFound(auditSourceId: number): void {

    this.auditSourceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditSource #${auditSourceId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditSourceLoadError(error: any, auditSourceId: number): void {

    let message = 'Failed to load Audit Source.';
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
          message = 'You do not have permission to view this Audit Source.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Source #${auditSourceId} was not found.`;
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

    console.error(`Audit Source load failed (ID: ${auditSourceId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditSourceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditSourceData: AuditSourceData | null) {

    if (auditSourceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditSourceForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditSourceForm.reset({
        name: auditSourceData.name ?? '',
        comments: auditSourceData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditSourceData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditSourceForm.markAsPristine();
    this.auditSourceForm.markAsUntouched();
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

    if (this.auditSourceService.userIsAuditorAuditSourceWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Sources", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditSourceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditSourceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditSourceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditSourceSubmitData: AuditSourceSubmitData = {
        id: this.auditSourceData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditSourceService.PutAuditSource(auditSourceSubmitData.id, auditSourceSubmitData)
      : this.auditSourceService.PostAuditSource(auditSourceSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditSourceData) => {

        this.auditSourceService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Source's detail page
          //
          this.auditSourceForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditSourceForm.markAsUntouched();

          this.router.navigate(['/auditsources', savedAuditSourceData.id]);
          this.alertService.showMessage('Audit Source added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditSourceData = savedAuditSourceData;
          this.buildFormValues(this.auditSourceData);

          this.alertService.showMessage("Audit Source saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit Source.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Source.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Source could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditSourceReader(): boolean {
    return this.auditSourceService.userIsAuditorAuditSourceReader();
  }

  public userIsAuditorAuditSourceWriter(): boolean {
    return this.auditSourceService.userIsAuditorAuditSourceWriter();
  }
}
