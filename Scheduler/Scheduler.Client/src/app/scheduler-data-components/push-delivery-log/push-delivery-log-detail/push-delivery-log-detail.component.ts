/*
   GENERATED FORM FOR THE PUSHDELIVERYLOG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PushDeliveryLog table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to push-delivery-log-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PushDeliveryLogService, PushDeliveryLogData, PushDeliveryLogSubmitData } from '../../../scheduler-data-services/push-delivery-log.service';
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
interface PushDeliveryLogFormValues {
  userId: string,     // Stored as string for form input, converted to number on submit.
  providerId: string,
  destination: string | null,
  sourceType: string | null,
  sourceNotificationId: string | null,     // Stored as string for form input, converted to number on submit.
  sourceConversationMessageId: string | null,     // Stored as string for form input, converted to number on submit.
  success: boolean,
  externalId: string | null,
  errorMessage: string | null,
  attemptNumber: string,     // Stored as string for form input, converted to number on submit.
  dateTimeCreated: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-push-delivery-log-detail',
  templateUrl: './push-delivery-log-detail.component.html',
  styleUrls: ['./push-delivery-log-detail.component.scss']
})

export class PushDeliveryLogDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PushDeliveryLogFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public pushDeliveryLogForm: FormGroup = this.fb.group({
        userId: ['', Validators.required],
        providerId: ['', Validators.required],
        destination: [''],
        sourceType: [''],
        sourceNotificationId: [''],
        sourceConversationMessageId: [''],
        success: [false],
        externalId: [''],
        errorMessage: [''],
        attemptNumber: ['', Validators.required],
        dateTimeCreated: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public pushDeliveryLogId: string | null = null;
  public pushDeliveryLogData: PushDeliveryLogData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  pushDeliveryLogs$ = this.pushDeliveryLogService.GetPushDeliveryLogList();

  private destroy$ = new Subject<void>();

  constructor(
    public pushDeliveryLogService: PushDeliveryLogService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the pushDeliveryLogId from the route parameters
    this.pushDeliveryLogId = this.route.snapshot.paramMap.get('pushDeliveryLogId');

    if (this.pushDeliveryLogId === 'new' ||
        this.pushDeliveryLogId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.pushDeliveryLogData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.pushDeliveryLogForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.pushDeliveryLogForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Push Delivery Log';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Push Delivery Log';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.pushDeliveryLogForm.dirty) {
      return confirm('You have unsaved Push Delivery Log changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.pushDeliveryLogId != null && this.pushDeliveryLogId !== 'new') {

      const id = parseInt(this.pushDeliveryLogId, 10);

      if (!isNaN(id)) {
        return { pushDeliveryLogId: id };
      }
    }

    return null;
  }


/*
  * Loads the PushDeliveryLog data for the current pushDeliveryLogId.
  *
  * Fully respects the PushDeliveryLogService caching strategy and error handling strategy.
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
    if (!this.pushDeliveryLogService.userIsSchedulerPushDeliveryLogReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PushDeliveryLogs.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate pushDeliveryLogId
    //
    if (!this.pushDeliveryLogId) {

      this.alertService.showMessage('No PushDeliveryLog ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const pushDeliveryLogId = Number(this.pushDeliveryLogId);

    if (isNaN(pushDeliveryLogId) || pushDeliveryLogId <= 0) {

      this.alertService.showMessage(`Invalid Push Delivery Log ID: "${this.pushDeliveryLogId}"`,
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
      // This is the most targeted way: clear only this PushDeliveryLog + relations

      this.pushDeliveryLogService.ClearRecordCache(pushDeliveryLogId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.pushDeliveryLogService.GetPushDeliveryLog(pushDeliveryLogId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (pushDeliveryLogData) => {

        //
        // Success path — pushDeliveryLogData can legitimately be null if 404'd but request succeeded
        //
        if (!pushDeliveryLogData) {

          this.handlePushDeliveryLogNotFound(pushDeliveryLogId);

        } else {

          this.pushDeliveryLogData = pushDeliveryLogData;
          this.buildFormValues(this.pushDeliveryLogData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PushDeliveryLog loaded successfully',
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
        this.handlePushDeliveryLogLoadError(error, pushDeliveryLogId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePushDeliveryLogNotFound(pushDeliveryLogId: number): void {

    this.pushDeliveryLogData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PushDeliveryLog #${pushDeliveryLogId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePushDeliveryLogLoadError(error: any, pushDeliveryLogId: number): void {

    let message = 'Failed to load Push Delivery Log.';
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
          message = 'You do not have permission to view this Push Delivery Log.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Push Delivery Log #${pushDeliveryLogId} was not found.`;
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

    console.error(`Push Delivery Log load failed (ID: ${pushDeliveryLogId})`, error);

    //
    // Reset UI to safe state
    //
    this.pushDeliveryLogData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(pushDeliveryLogData: PushDeliveryLogData | null) {

    if (pushDeliveryLogData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.pushDeliveryLogForm.reset({
        userId: '',
        providerId: '',
        destination: '',
        sourceType: '',
        sourceNotificationId: '',
        sourceConversationMessageId: '',
        success: false,
        externalId: '',
        errorMessage: '',
        attemptNumber: '',
        dateTimeCreated: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.pushDeliveryLogForm.reset({
        userId: pushDeliveryLogData.userId?.toString() ?? '',
        providerId: pushDeliveryLogData.providerId ?? '',
        destination: pushDeliveryLogData.destination ?? '',
        sourceType: pushDeliveryLogData.sourceType ?? '',
        sourceNotificationId: pushDeliveryLogData.sourceNotificationId?.toString() ?? '',
        sourceConversationMessageId: pushDeliveryLogData.sourceConversationMessageId?.toString() ?? '',
        success: pushDeliveryLogData.success ?? false,
        externalId: pushDeliveryLogData.externalId ?? '',
        errorMessage: pushDeliveryLogData.errorMessage ?? '',
        attemptNumber: pushDeliveryLogData.attemptNumber?.toString() ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(pushDeliveryLogData.dateTimeCreated) ?? '',
        active: pushDeliveryLogData.active ?? true,
        deleted: pushDeliveryLogData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.pushDeliveryLogForm.markAsPristine();
    this.pushDeliveryLogForm.markAsUntouched();
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

    if (this.pushDeliveryLogService.userIsSchedulerPushDeliveryLogWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Push Delivery Logs", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.pushDeliveryLogForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.pushDeliveryLogForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.pushDeliveryLogForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const pushDeliveryLogSubmitData: PushDeliveryLogSubmitData = {
        id: this.pushDeliveryLogData?.id || 0,
        userId: Number(formValue.userId),
        providerId: formValue.providerId!.trim(),
        destination: formValue.destination?.trim() || null,
        sourceType: formValue.sourceType?.trim() || null,
        sourceNotificationId: formValue.sourceNotificationId ? Number(formValue.sourceNotificationId) : null,
        sourceConversationMessageId: formValue.sourceConversationMessageId ? Number(formValue.sourceConversationMessageId) : null,
        success: !!formValue.success,
        externalId: formValue.externalId?.trim() || null,
        errorMessage: formValue.errorMessage?.trim() || null,
        attemptNumber: Number(formValue.attemptNumber),
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.pushDeliveryLogService.PutPushDeliveryLog(pushDeliveryLogSubmitData.id, pushDeliveryLogSubmitData)
      : this.pushDeliveryLogService.PostPushDeliveryLog(pushDeliveryLogSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPushDeliveryLogData) => {

        this.pushDeliveryLogService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Push Delivery Log's detail page
          //
          this.pushDeliveryLogForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.pushDeliveryLogForm.markAsUntouched();

          this.router.navigate(['/pushdeliverylogs', savedPushDeliveryLogData.id]);
          this.alertService.showMessage('Push Delivery Log added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.pushDeliveryLogData = savedPushDeliveryLogData;
          this.buildFormValues(this.pushDeliveryLogData);

          this.alertService.showMessage("Push Delivery Log saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Push Delivery Log.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Push Delivery Log.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Push Delivery Log could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerPushDeliveryLogReader(): boolean {
    return this.pushDeliveryLogService.userIsSchedulerPushDeliveryLogReader();
  }

  public userIsSchedulerPushDeliveryLogWriter(): boolean {
    return this.pushDeliveryLogService.userIsSchedulerPushDeliveryLogWriter();
  }
}
