/*
   GENERATED FORM FOR THE MESSAGINGAUDITLOG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MessagingAuditLog table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to messaging-audit-log-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MessagingAuditLogService, MessagingAuditLogData, MessagingAuditLogSubmitData } from '../../../scheduler-data-services/messaging-audit-log.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface MessagingAuditLogFormValues {
  performedByUserId: string,     // Stored as string for form input, converted to number on submit.
  action: string,
  entityType: string | null,
  entityId: string | null,     // Stored as string for form input, converted to number on submit.
  details: string | null,
  ipAddress: string | null,
  dateTimeCreated: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-messaging-audit-log-detail',
  templateUrl: './messaging-audit-log-detail.component.html',
  styleUrls: ['./messaging-audit-log-detail.component.scss']
})

export class MessagingAuditLogDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MessagingAuditLogFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public messagingAuditLogForm: FormGroup = this.fb.group({
        performedByUserId: ['', Validators.required],
        action: ['', Validators.required],
        entityType: [''],
        entityId: [''],
        details: [''],
        ipAddress: [''],
        dateTimeCreated: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public messagingAuditLogId: string | null = null;
  public messagingAuditLogData: MessagingAuditLogData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  messagingAuditLogs$ = this.messagingAuditLogService.GetMessagingAuditLogList();

  private destroy$ = new Subject<void>();

  constructor(
    public messagingAuditLogService: MessagingAuditLogService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the messagingAuditLogId from the route parameters
    this.messagingAuditLogId = this.route.snapshot.paramMap.get('messagingAuditLogId');

    if (this.messagingAuditLogId === 'new' ||
        this.messagingAuditLogId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.messagingAuditLogData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.messagingAuditLogForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.messagingAuditLogForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Messaging Audit Log';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Messaging Audit Log';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.messagingAuditLogForm.dirty) {
      return confirm('You have unsaved Messaging Audit Log changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.messagingAuditLogId != null && this.messagingAuditLogId !== 'new') {

      const id = parseInt(this.messagingAuditLogId, 10);

      if (!isNaN(id)) {
        return { messagingAuditLogId: id };
      }
    }

    return null;
  }


/*
  * Loads the MessagingAuditLog data for the current messagingAuditLogId.
  *
  * Fully respects the MessagingAuditLogService caching strategy and error handling strategy.
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
    if (!this.messagingAuditLogService.userIsSchedulerMessagingAuditLogReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read MessagingAuditLogs.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate messagingAuditLogId
    //
    if (!this.messagingAuditLogId) {

      this.alertService.showMessage('No MessagingAuditLog ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const messagingAuditLogId = Number(this.messagingAuditLogId);

    if (isNaN(messagingAuditLogId) || messagingAuditLogId <= 0) {

      this.alertService.showMessage(`Invalid Messaging Audit Log ID: "${this.messagingAuditLogId}"`,
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
      // This is the most targeted way: clear only this MessagingAuditLog + relations

      this.messagingAuditLogService.ClearRecordCache(messagingAuditLogId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.messagingAuditLogService.GetMessagingAuditLog(messagingAuditLogId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (messagingAuditLogData) => {

        //
        // Success path — messagingAuditLogData can legitimately be null if 404'd but request succeeded
        //
        if (!messagingAuditLogData) {

          this.handleMessagingAuditLogNotFound(messagingAuditLogId);

        } else {

          this.messagingAuditLogData = messagingAuditLogData;
          this.buildFormValues(this.messagingAuditLogData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'MessagingAuditLog loaded successfully',
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
        this.handleMessagingAuditLogLoadError(error, messagingAuditLogId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleMessagingAuditLogNotFound(messagingAuditLogId: number): void {

    this.messagingAuditLogData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `MessagingAuditLog #${messagingAuditLogId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleMessagingAuditLogLoadError(error: any, messagingAuditLogId: number): void {

    let message = 'Failed to load Messaging Audit Log.';
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
          message = 'You do not have permission to view this Messaging Audit Log.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Messaging Audit Log #${messagingAuditLogId} was not found.`;
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

    console.error(`Messaging Audit Log load failed (ID: ${messagingAuditLogId})`, error);

    //
    // Reset UI to safe state
    //
    this.messagingAuditLogData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(messagingAuditLogData: MessagingAuditLogData | null) {

    if (messagingAuditLogData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.messagingAuditLogForm.reset({
        performedByUserId: '',
        action: '',
        entityType: '',
        entityId: '',
        details: '',
        ipAddress: '',
        dateTimeCreated: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.messagingAuditLogForm.reset({
        performedByUserId: messagingAuditLogData.performedByUserId?.toString() ?? '',
        action: messagingAuditLogData.action ?? '',
        entityType: messagingAuditLogData.entityType ?? '',
        entityId: messagingAuditLogData.entityId?.toString() ?? '',
        details: messagingAuditLogData.details ?? '',
        ipAddress: messagingAuditLogData.ipAddress ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(messagingAuditLogData.dateTimeCreated) ?? '',
        active: messagingAuditLogData.active ?? true,
        deleted: messagingAuditLogData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.messagingAuditLogForm.markAsPristine();
    this.messagingAuditLogForm.markAsUntouched();
  }

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  //
  // Helper method to determine if a field should be hidden based on the hiddenFields input.
  // Returns true if the field is in the array, false otherwise.
  //
  public isFieldHidden(fieldName: string): boolean {
    // Explicit check for array existence to avoid runtime errors.
    if (this.hiddenFields === null || this.hiddenFields === undefined) {
      return false;
    }
    // Use traditional includes method for clarity.
    return this.hiddenFields.includes(fieldName);
  }


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.messagingAuditLogService.userIsSchedulerMessagingAuditLogWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Messaging Audit Logs", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.messagingAuditLogForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.messagingAuditLogForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.messagingAuditLogForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const messagingAuditLogSubmitData: MessagingAuditLogSubmitData = {
        id: this.messagingAuditLogData?.id || 0,
        performedByUserId: Number(formValue.performedByUserId),
        action: formValue.action!.trim(),
        entityType: formValue.entityType?.trim() || null,
        entityId: formValue.entityId ? Number(formValue.entityId) : null,
        details: formValue.details?.trim() || null,
        ipAddress: formValue.ipAddress?.trim() || null,
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.messagingAuditLogService.PutMessagingAuditLog(messagingAuditLogSubmitData.id, messagingAuditLogSubmitData)
      : this.messagingAuditLogService.PostMessagingAuditLog(messagingAuditLogSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedMessagingAuditLogData) => {

        this.messagingAuditLogService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Messaging Audit Log's detail page
          //
          this.messagingAuditLogForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.messagingAuditLogForm.markAsUntouched();

          this.router.navigate(['/messagingauditlogs', savedMessagingAuditLogData.id]);
          this.alertService.showMessage('Messaging Audit Log added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.messagingAuditLogData = savedMessagingAuditLogData;
          this.buildFormValues(this.messagingAuditLogData);

          this.alertService.showMessage("Messaging Audit Log saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Messaging Audit Log.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Messaging Audit Log.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Messaging Audit Log could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerMessagingAuditLogReader(): boolean {
    return this.messagingAuditLogService.userIsSchedulerMessagingAuditLogReader();
  }

  public userIsSchedulerMessagingAuditLogWriter(): boolean {
    return this.messagingAuditLogService.userIsSchedulerMessagingAuditLogWriter();
  }
}
