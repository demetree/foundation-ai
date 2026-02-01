/*
   GENERATED FORM FOR THE NOTIFICATIONDELIVERYATTEMPT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from NotificationDeliveryAttempt table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to notification-delivery-attempt-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NotificationDeliveryAttemptService, NotificationDeliveryAttemptData, NotificationDeliveryAttemptSubmitData } from '../../../alerting-data-services/notification-delivery-attempt.service';
import { IncidentNotificationService } from '../../../alerting-data-services/incident-notification.service';
import { NotificationChannelTypeService } from '../../../alerting-data-services/notification-channel-type.service';
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
interface NotificationDeliveryAttemptFormValues {
  incidentNotificationId: number | bigint,       // For FK link number
  notificationChannelTypeId: number | bigint,       // For FK link number
  attemptNumber: string,     // Stored as string for form input, converted to number on submit.
  attemptedAt: string,
  status: string,
  errorMessage: string | null,
  response: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-notification-delivery-attempt-detail',
  templateUrl: './notification-delivery-attempt-detail.component.html',
  styleUrls: ['./notification-delivery-attempt-detail.component.scss']
})

export class NotificationDeliveryAttemptDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<NotificationDeliveryAttemptFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public notificationDeliveryAttemptForm: FormGroup = this.fb.group({
        incidentNotificationId: [null, Validators.required],
        notificationChannelTypeId: [null, Validators.required],
        attemptNumber: ['', Validators.required],
        attemptedAt: ['', Validators.required],
        status: ['', Validators.required],
        errorMessage: [''],
        response: [''],
        active: [true],
        deleted: [false],
      });


  public notificationDeliveryAttemptId: string | null = null;
  public notificationDeliveryAttemptData: NotificationDeliveryAttemptData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  notificationDeliveryAttempts$ = this.notificationDeliveryAttemptService.GetNotificationDeliveryAttemptList();
  public incidentNotifications$ = this.incidentNotificationService.GetIncidentNotificationList();
  public notificationChannelTypes$ = this.notificationChannelTypeService.GetNotificationChannelTypeList();

  private destroy$ = new Subject<void>();

  constructor(
    public notificationDeliveryAttemptService: NotificationDeliveryAttemptService,
    public incidentNotificationService: IncidentNotificationService,
    public notificationChannelTypeService: NotificationChannelTypeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the notificationDeliveryAttemptId from the route parameters
    this.notificationDeliveryAttemptId = this.route.snapshot.paramMap.get('notificationDeliveryAttemptId');

    if (this.notificationDeliveryAttemptId === 'new' ||
        this.notificationDeliveryAttemptId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.notificationDeliveryAttemptData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.notificationDeliveryAttemptForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.notificationDeliveryAttemptForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Notification Delivery Attempt';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Notification Delivery Attempt';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.notificationDeliveryAttemptForm.dirty) {
      return confirm('You have unsaved Notification Delivery Attempt changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.notificationDeliveryAttemptId != null && this.notificationDeliveryAttemptId !== 'new') {

      const id = parseInt(this.notificationDeliveryAttemptId, 10);

      if (!isNaN(id)) {
        return { notificationDeliveryAttemptId: id };
      }
    }

    return null;
  }


/*
  * Loads the NotificationDeliveryAttempt data for the current notificationDeliveryAttemptId.
  *
  * Fully respects the NotificationDeliveryAttemptService caching strategy and error handling strategy.
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
    if (!this.notificationDeliveryAttemptService.userIsAlertingNotificationDeliveryAttemptReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read NotificationDeliveryAttempts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate notificationDeliveryAttemptId
    //
    if (!this.notificationDeliveryAttemptId) {

      this.alertService.showMessage('No NotificationDeliveryAttempt ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const notificationDeliveryAttemptId = Number(this.notificationDeliveryAttemptId);

    if (isNaN(notificationDeliveryAttemptId) || notificationDeliveryAttemptId <= 0) {

      this.alertService.showMessage(`Invalid Notification Delivery Attempt ID: "${this.notificationDeliveryAttemptId}"`,
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
      // This is the most targeted way: clear only this NotificationDeliveryAttempt + relations

      this.notificationDeliveryAttemptService.ClearRecordCache(notificationDeliveryAttemptId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.notificationDeliveryAttemptService.GetNotificationDeliveryAttempt(notificationDeliveryAttemptId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (notificationDeliveryAttemptData) => {

        //
        // Success path — notificationDeliveryAttemptData can legitimately be null if 404'd but request succeeded
        //
        if (!notificationDeliveryAttemptData) {

          this.handleNotificationDeliveryAttemptNotFound(notificationDeliveryAttemptId);

        } else {

          this.notificationDeliveryAttemptData = notificationDeliveryAttemptData;
          this.buildFormValues(this.notificationDeliveryAttemptData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'NotificationDeliveryAttempt loaded successfully',
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
        this.handleNotificationDeliveryAttemptLoadError(error, notificationDeliveryAttemptId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleNotificationDeliveryAttemptNotFound(notificationDeliveryAttemptId: number): void {

    this.notificationDeliveryAttemptData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `NotificationDeliveryAttempt #${notificationDeliveryAttemptId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleNotificationDeliveryAttemptLoadError(error: any, notificationDeliveryAttemptId: number): void {

    let message = 'Failed to load Notification Delivery Attempt.';
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
          message = 'You do not have permission to view this Notification Delivery Attempt.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Notification Delivery Attempt #${notificationDeliveryAttemptId} was not found.`;
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

    console.error(`Notification Delivery Attempt load failed (ID: ${notificationDeliveryAttemptId})`, error);

    //
    // Reset UI to safe state
    //
    this.notificationDeliveryAttemptData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(notificationDeliveryAttemptData: NotificationDeliveryAttemptData | null) {

    if (notificationDeliveryAttemptData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.notificationDeliveryAttemptForm.reset({
        incidentNotificationId: null,
        notificationChannelTypeId: null,
        attemptNumber: '',
        attemptedAt: '',
        status: '',
        errorMessage: '',
        response: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.notificationDeliveryAttemptForm.reset({
        incidentNotificationId: notificationDeliveryAttemptData.incidentNotificationId,
        notificationChannelTypeId: notificationDeliveryAttemptData.notificationChannelTypeId,
        attemptNumber: notificationDeliveryAttemptData.attemptNumber?.toString() ?? '',
        attemptedAt: isoUtcStringToDateTimeLocal(notificationDeliveryAttemptData.attemptedAt) ?? '',
        status: notificationDeliveryAttemptData.status ?? '',
        errorMessage: notificationDeliveryAttemptData.errorMessage ?? '',
        response: notificationDeliveryAttemptData.response ?? '',
        active: notificationDeliveryAttemptData.active ?? true,
        deleted: notificationDeliveryAttemptData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.notificationDeliveryAttemptForm.markAsPristine();
    this.notificationDeliveryAttemptForm.markAsUntouched();
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

    if (this.notificationDeliveryAttemptService.userIsAlertingNotificationDeliveryAttemptWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Notification Delivery Attempts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.notificationDeliveryAttemptForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.notificationDeliveryAttemptForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.notificationDeliveryAttemptForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const notificationDeliveryAttemptSubmitData: NotificationDeliveryAttemptSubmitData = {
        id: this.notificationDeliveryAttemptData?.id || 0,
        incidentNotificationId: Number(formValue.incidentNotificationId),
        notificationChannelTypeId: Number(formValue.notificationChannelTypeId),
        attemptNumber: Number(formValue.attemptNumber),
        attemptedAt: dateTimeLocalToIsoUtc(formValue.attemptedAt!.trim())!,
        status: formValue.status!.trim(),
        errorMessage: formValue.errorMessage?.trim() || null,
        response: formValue.response?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.notificationDeliveryAttemptService.PutNotificationDeliveryAttempt(notificationDeliveryAttemptSubmitData.id, notificationDeliveryAttemptSubmitData)
      : this.notificationDeliveryAttemptService.PostNotificationDeliveryAttempt(notificationDeliveryAttemptSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedNotificationDeliveryAttemptData) => {

        this.notificationDeliveryAttemptService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Notification Delivery Attempt's detail page
          //
          this.notificationDeliveryAttemptForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.notificationDeliveryAttemptForm.markAsUntouched();

          this.router.navigate(['/notificationdeliveryattempts', savedNotificationDeliveryAttemptData.id]);
          this.alertService.showMessage('Notification Delivery Attempt added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.notificationDeliveryAttemptData = savedNotificationDeliveryAttemptData;
          this.buildFormValues(this.notificationDeliveryAttemptData);

          this.alertService.showMessage("Notification Delivery Attempt saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Notification Delivery Attempt.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Notification Delivery Attempt.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Notification Delivery Attempt could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingNotificationDeliveryAttemptReader(): boolean {
    return this.notificationDeliveryAttemptService.userIsAlertingNotificationDeliveryAttemptReader();
  }

  public userIsAlertingNotificationDeliveryAttemptWriter(): boolean {
    return this.notificationDeliveryAttemptService.userIsAlertingNotificationDeliveryAttemptWriter();
  }
}
