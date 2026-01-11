import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditEventErrorMessageService, AuditEventErrorMessageData, AuditEventErrorMessageSubmitData } from '../../../auditor-data-services/audit-event-error-message.service';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-audit-event-error-message-detail',
  templateUrl: './audit-event-error-message-detail.component.html',
  styleUrls: ['./audit-event-error-message-detail.component.scss']
})

export class AuditEventErrorMessageDetailComponent implements OnInit, CanComponentDeactivate {

  auditEventErrorMessageForm: FormGroup = this.fb.group({
        auditEventId: [null, Validators.required],
        errorMessage: ['', Validators.required],
      });


  public auditEventErrorMessageId: string | null = null;
  public auditEventErrorMessageData: AuditEventErrorMessageData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditEventErrorMessages$ = this.auditEventErrorMessageService.GetAuditEventErrorMessageList();
  auditEvents$ = this.auditEventService.GetAuditEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditEventErrorMessageService: AuditEventErrorMessageService,
    public auditEventService: AuditEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditEventErrorMessageId from the route parameters
    this.auditEventErrorMessageId = this.route.snapshot.paramMap.get('auditEventErrorMessageId');

    if (this.auditEventErrorMessageId === 'new' ||
        this.auditEventErrorMessageId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditEventErrorMessageData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Event Error Message';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Event Error Message';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditEventErrorMessageForm.dirty) {
      return confirm('You have unsaved Audit Event Error Message changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditEventErrorMessageId != null && this.auditEventErrorMessageId !== 'new') {

      const id = parseInt(this.auditEventErrorMessageId, 10);

      if (!isNaN(id)) {
        return { auditEventErrorMessageId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditEventErrorMessage data for the current auditEventErrorMessageId.
  *
  * Fully respects the AuditEventErrorMessageService caching strategy and error handling strategy.
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
    if (!this.auditEventErrorMessageService.userIsAuditorAuditEventErrorMessageReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditEventErrorMessages.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditEventErrorMessageId
    //
    if (!this.auditEventErrorMessageId) {

      this.alertService.showMessage('No AuditEventErrorMessage ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditEventErrorMessageId = Number(this.auditEventErrorMessageId);

    if (isNaN(auditEventErrorMessageId) || auditEventErrorMessageId <= 0) {

      this.alertService.showMessage(`Invalid Audit Event Error Message ID: "${this.auditEventErrorMessageId}"`,
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
      // This is the most targeted way: clear only this AuditEventErrorMessage + relations

      this.auditEventErrorMessageService.ClearRecordCache(auditEventErrorMessageId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditEventErrorMessageService.GetAuditEventErrorMessage(auditEventErrorMessageId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditEventErrorMessageData) => {

        //
        // Success path — auditEventErrorMessageData can legitimately be null if 404'd but request succeeded
        //
        if (!auditEventErrorMessageData) {

          this.handleAuditEventErrorMessageNotFound(auditEventErrorMessageId);

        } else {

          this.auditEventErrorMessageData = auditEventErrorMessageData;
          this.buildFormValues(this.auditEventErrorMessageData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditEventErrorMessage loaded successfully',
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
        this.handleAuditEventErrorMessageLoadError(error, auditEventErrorMessageId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditEventErrorMessageNotFound(auditEventErrorMessageId: number): void {

    this.auditEventErrorMessageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditEventErrorMessage #${auditEventErrorMessageId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditEventErrorMessageLoadError(error: any, auditEventErrorMessageId: number): void {

    let message = 'Failed to load Audit Event Error Message.';
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
          message = 'You do not have permission to view this Audit Event Error Message.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Event Error Message #${auditEventErrorMessageId} was not found.`;
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

    console.error(`Audit Event Error Message load failed (ID: ${auditEventErrorMessageId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditEventErrorMessageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditEventErrorMessageData: AuditEventErrorMessageData | null) {

    if (auditEventErrorMessageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditEventErrorMessageForm.reset({
        auditEventId: null,
        errorMessage: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditEventErrorMessageForm.reset({
        auditEventId: auditEventErrorMessageData.auditEventId,
        errorMessage: auditEventErrorMessageData.errorMessage ?? '',
      }, { emitEvent: false});
    }

    this.auditEventErrorMessageForm.markAsPristine();
    this.auditEventErrorMessageForm.markAsUntouched();
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

    if (this.auditEventErrorMessageService.userIsAuditorAuditEventErrorMessageWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Event Error Messages", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditEventErrorMessageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditEventErrorMessageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditEventErrorMessageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditEventErrorMessageSubmitData: AuditEventErrorMessageSubmitData = {
        id: this.auditEventErrorMessageData?.id || 0,
        auditEventId: Number(formValue.auditEventId),
        errorMessage: formValue.errorMessage!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditEventErrorMessageService.PutAuditEventErrorMessage(auditEventErrorMessageSubmitData.id, auditEventErrorMessageSubmitData)
      : this.auditEventErrorMessageService.PostAuditEventErrorMessage(auditEventErrorMessageSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditEventErrorMessageData) => {

        this.auditEventErrorMessageService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Event Error Message's detail page
          //
          this.auditEventErrorMessageForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditEventErrorMessageForm.markAsUntouched();

          this.router.navigate(['/auditeventerrormessages', savedAuditEventErrorMessageData.id]);
          this.alertService.showMessage('Audit Event Error Message added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditEventErrorMessageData = savedAuditEventErrorMessageData;
          this.buildFormValues(this.auditEventErrorMessageData);

          this.alertService.showMessage("Audit Event Error Message saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit Event Error Message.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Event Error Message.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Event Error Message could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditEventErrorMessageReader(): boolean {
    return this.auditEventErrorMessageService.userIsAuditorAuditEventErrorMessageReader();
  }

  public userIsAuditorAuditEventErrorMessageWriter(): boolean {
    return this.auditEventErrorMessageService.userIsAuditorAuditEventErrorMessageWriter();
  }
}
