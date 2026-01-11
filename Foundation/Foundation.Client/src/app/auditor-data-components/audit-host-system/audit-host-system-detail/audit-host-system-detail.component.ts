import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditHostSystemService, AuditHostSystemData, AuditHostSystemSubmitData } from '../../../auditor-data-services/audit-host-system.service';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-audit-host-system-detail',
  templateUrl: './audit-host-system-detail.component.html',
  styleUrls: ['./audit-host-system-detail.component.scss']
})

export class AuditHostSystemDetailComponent implements OnInit, CanComponentDeactivate {

  auditHostSystemForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });


  public auditHostSystemId: string | null = null;
  public auditHostSystemData: AuditHostSystemData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditHostSystems$ = this.auditHostSystemService.GetAuditHostSystemList();
  auditEvents$ = this.auditEventService.GetAuditEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditHostSystemService: AuditHostSystemService,
    public auditEventService: AuditEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditHostSystemId from the route parameters
    this.auditHostSystemId = this.route.snapshot.paramMap.get('auditHostSystemId');

    if (this.auditHostSystemId === 'new' ||
        this.auditHostSystemId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditHostSystemData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Host System';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Host System';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditHostSystemForm.dirty) {
      return confirm('You have unsaved Audit Host System changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditHostSystemId != null && this.auditHostSystemId !== 'new') {

      const id = parseInt(this.auditHostSystemId, 10);

      if (!isNaN(id)) {
        return { auditHostSystemId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditHostSystem data for the current auditHostSystemId.
  *
  * Fully respects the AuditHostSystemService caching strategy and error handling strategy.
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
    if (!this.auditHostSystemService.userIsAuditorAuditHostSystemReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditHostSystems.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditHostSystemId
    //
    if (!this.auditHostSystemId) {

      this.alertService.showMessage('No AuditHostSystem ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditHostSystemId = Number(this.auditHostSystemId);

    if (isNaN(auditHostSystemId) || auditHostSystemId <= 0) {

      this.alertService.showMessage(`Invalid Audit Host System ID: "${this.auditHostSystemId}"`,
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
      // This is the most targeted way: clear only this AuditHostSystem + relations

      this.auditHostSystemService.ClearRecordCache(auditHostSystemId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditHostSystemService.GetAuditHostSystem(auditHostSystemId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditHostSystemData) => {

        //
        // Success path — auditHostSystemData can legitimately be null if 404'd but request succeeded
        //
        if (!auditHostSystemData) {

          this.handleAuditHostSystemNotFound(auditHostSystemId);

        } else {

          this.auditHostSystemData = auditHostSystemData;
          this.buildFormValues(this.auditHostSystemData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditHostSystem loaded successfully',
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
        this.handleAuditHostSystemLoadError(error, auditHostSystemId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditHostSystemNotFound(auditHostSystemId: number): void {

    this.auditHostSystemData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditHostSystem #${auditHostSystemId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditHostSystemLoadError(error: any, auditHostSystemId: number): void {

    let message = 'Failed to load Audit Host System.';
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
          message = 'You do not have permission to view this Audit Host System.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Host System #${auditHostSystemId} was not found.`;
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

    console.error(`Audit Host System load failed (ID: ${auditHostSystemId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditHostSystemData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditHostSystemData: AuditHostSystemData | null) {

    if (auditHostSystemData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditHostSystemForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditHostSystemForm.reset({
        name: auditHostSystemData.name ?? '',
        comments: auditHostSystemData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditHostSystemData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditHostSystemForm.markAsPristine();
    this.auditHostSystemForm.markAsUntouched();
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

    if (this.auditHostSystemService.userIsAuditorAuditHostSystemWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Host Systems", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditHostSystemForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditHostSystemForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditHostSystemForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditHostSystemSubmitData: AuditHostSystemSubmitData = {
        id: this.auditHostSystemData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditHostSystemService.PutAuditHostSystem(auditHostSystemSubmitData.id, auditHostSystemSubmitData)
      : this.auditHostSystemService.PostAuditHostSystem(auditHostSystemSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditHostSystemData) => {

        this.auditHostSystemService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Host System's detail page
          //
          this.auditHostSystemForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditHostSystemForm.markAsUntouched();

          this.router.navigate(['/audithostsystems', savedAuditHostSystemData.id]);
          this.alertService.showMessage('Audit Host System added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditHostSystemData = savedAuditHostSystemData;
          this.buildFormValues(this.auditHostSystemData);

          this.alertService.showMessage("Audit Host System saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit Host System.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Host System.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Host System could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditHostSystemReader(): boolean {
    return this.auditHostSystemService.userIsAuditorAuditHostSystemReader();
  }

  public userIsAuditorAuditHostSystemWriter(): boolean {
    return this.auditHostSystemService.userIsAuditorAuditHostSystemWriter();
  }
}
