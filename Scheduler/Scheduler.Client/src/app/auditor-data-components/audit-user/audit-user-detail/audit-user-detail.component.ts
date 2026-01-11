import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditUserService, AuditUserData, AuditUserSubmitData } from '../../../auditor-data-services/audit-user.service';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
import { ExternalCommunicationService } from '../../../auditor-data-services/external-communication.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-audit-user-detail',
  templateUrl: './audit-user-detail.component.html',
  styleUrls: ['./audit-user-detail.component.scss']
})

export class AuditUserDetailComponent implements OnInit, CanComponentDeactivate {

  auditUserForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });


  public auditUserId: string | null = null;
  public auditUserData: AuditUserData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditUsers$ = this.auditUserService.GetAuditUserList();
  auditEvents$ = this.auditEventService.GetAuditEventList();
  externalCommunications$ = this.externalCommunicationService.GetExternalCommunicationList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditUserService: AuditUserService,
    public auditEventService: AuditEventService,
    public externalCommunicationService: ExternalCommunicationService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditUserId from the route parameters
    this.auditUserId = this.route.snapshot.paramMap.get('auditUserId');

    if (this.auditUserId === 'new' ||
        this.auditUserId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditUserData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit User';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit User';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditUserForm.dirty) {
      return confirm('You have unsaved Audit User changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditUserId != null && this.auditUserId !== 'new') {

      const id = parseInt(this.auditUserId, 10);

      if (!isNaN(id)) {
        return { auditUserId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditUser data for the current auditUserId.
  *
  * Fully respects the AuditUserService caching strategy and error handling strategy.
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
    if (!this.auditUserService.userIsAuditorAuditUserReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditUsers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditUserId
    //
    if (!this.auditUserId) {

      this.alertService.showMessage('No AuditUser ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditUserId = Number(this.auditUserId);

    if (isNaN(auditUserId) || auditUserId <= 0) {

      this.alertService.showMessage(`Invalid Audit User ID: "${this.auditUserId}"`,
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
      // This is the most targeted way: clear only this AuditUser + relations

      this.auditUserService.ClearRecordCache(auditUserId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditUserService.GetAuditUser(auditUserId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditUserData) => {

        //
        // Success path — auditUserData can legitimately be null if 404'd but request succeeded
        //
        if (!auditUserData) {

          this.handleAuditUserNotFound(auditUserId);

        } else {

          this.auditUserData = auditUserData;
          this.buildFormValues(this.auditUserData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditUser loaded successfully',
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
        this.handleAuditUserLoadError(error, auditUserId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditUserNotFound(auditUserId: number): void {

    this.auditUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditUser #${auditUserId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditUserLoadError(error: any, auditUserId: number): void {

    let message = 'Failed to load Audit User.';
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
          message = 'You do not have permission to view this Audit User.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit User #${auditUserId} was not found.`;
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

    console.error(`Audit User load failed (ID: ${auditUserId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditUserData: AuditUserData | null) {

    if (auditUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditUserForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditUserForm.reset({
        name: auditUserData.name ?? '',
        comments: auditUserData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditUserData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditUserForm.markAsPristine();
    this.auditUserForm.markAsUntouched();
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

    if (this.auditUserService.userIsAuditorAuditUserWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Users", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditUserSubmitData: AuditUserSubmitData = {
        id: this.auditUserData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditUserService.PutAuditUser(auditUserSubmitData.id, auditUserSubmitData)
      : this.auditUserService.PostAuditUser(auditUserSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditUserData) => {

        this.auditUserService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit User's detail page
          //
          this.auditUserForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditUserForm.markAsUntouched();

          this.router.navigate(['/auditusers', savedAuditUserData.id]);
          this.alertService.showMessage('Audit User added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditUserData = savedAuditUserData;
          this.buildFormValues(this.auditUserData);

          this.alertService.showMessage("Audit User saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditUserReader(): boolean {
    return this.auditUserService.userIsAuditorAuditUserReader();
  }

  public userIsAuditorAuditUserWriter(): boolean {
    return this.auditUserService.userIsAuditorAuditUserWriter();
  }
}
