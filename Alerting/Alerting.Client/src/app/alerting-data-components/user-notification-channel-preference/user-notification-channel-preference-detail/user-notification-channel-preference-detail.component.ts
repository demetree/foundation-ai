/*
   GENERATED FORM FOR THE USERNOTIFICATIONCHANNELPREFERENCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserNotificationChannelPreference table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-notification-channel-preference-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserNotificationChannelPreferenceService, UserNotificationChannelPreferenceData, UserNotificationChannelPreferenceSubmitData } from '../../../alerting-data-services/user-notification-channel-preference.service';
import { UserNotificationPreferenceService } from '../../../alerting-data-services/user-notification-preference.service';
import { NotificationChannelTypeService } from '../../../alerting-data-services/notification-channel-type.service';
import { UserNotificationChannelPreferenceChangeHistoryService } from '../../../alerting-data-services/user-notification-channel-preference-change-history.service';
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
interface UserNotificationChannelPreferenceFormValues {
  userNotificationPreferenceId: number | bigint,       // For FK link number
  notificationChannelTypeId: number | bigint,       // For FK link number
  isEnabled: boolean,
  priorityOverride: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-notification-channel-preference-detail',
  templateUrl: './user-notification-channel-preference-detail.component.html',
  styleUrls: ['./user-notification-channel-preference-detail.component.scss']
})

export class UserNotificationChannelPreferenceDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserNotificationChannelPreferenceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userNotificationChannelPreferenceForm: FormGroup = this.fb.group({
        userNotificationPreferenceId: [null, Validators.required],
        notificationChannelTypeId: [null, Validators.required],
        isEnabled: [false],
        priorityOverride: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public userNotificationChannelPreferenceId: string | null = null;
  public userNotificationChannelPreferenceData: UserNotificationChannelPreferenceData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userNotificationChannelPreferences$ = this.userNotificationChannelPreferenceService.GetUserNotificationChannelPreferenceList();
  public userNotificationPreferences$ = this.userNotificationPreferenceService.GetUserNotificationPreferenceList();
  public notificationChannelTypes$ = this.notificationChannelTypeService.GetNotificationChannelTypeList();
  public userNotificationChannelPreferenceChangeHistories$ = this.userNotificationChannelPreferenceChangeHistoryService.GetUserNotificationChannelPreferenceChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public userNotificationChannelPreferenceService: UserNotificationChannelPreferenceService,
    public userNotificationPreferenceService: UserNotificationPreferenceService,
    public notificationChannelTypeService: NotificationChannelTypeService,
    public userNotificationChannelPreferenceChangeHistoryService: UserNotificationChannelPreferenceChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userNotificationChannelPreferenceId from the route parameters
    this.userNotificationChannelPreferenceId = this.route.snapshot.paramMap.get('userNotificationChannelPreferenceId');

    if (this.userNotificationChannelPreferenceId === 'new' ||
        this.userNotificationChannelPreferenceId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userNotificationChannelPreferenceData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userNotificationChannelPreferenceForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userNotificationChannelPreferenceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Notification Channel Preference';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Notification Channel Preference';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userNotificationChannelPreferenceForm.dirty) {
      return confirm('You have unsaved User Notification Channel Preference changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userNotificationChannelPreferenceId != null && this.userNotificationChannelPreferenceId !== 'new') {

      const id = parseInt(this.userNotificationChannelPreferenceId, 10);

      if (!isNaN(id)) {
        return { userNotificationChannelPreferenceId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserNotificationChannelPreference data for the current userNotificationChannelPreferenceId.
  *
  * Fully respects the UserNotificationChannelPreferenceService caching strategy and error handling strategy.
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
    if (!this.userNotificationChannelPreferenceService.userIsAlertingUserNotificationChannelPreferenceReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserNotificationChannelPreferences.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userNotificationChannelPreferenceId
    //
    if (!this.userNotificationChannelPreferenceId) {

      this.alertService.showMessage('No UserNotificationChannelPreference ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userNotificationChannelPreferenceId = Number(this.userNotificationChannelPreferenceId);

    if (isNaN(userNotificationChannelPreferenceId) || userNotificationChannelPreferenceId <= 0) {

      this.alertService.showMessage(`Invalid User Notification Channel Preference ID: "${this.userNotificationChannelPreferenceId}"`,
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
      // This is the most targeted way: clear only this UserNotificationChannelPreference + relations

      this.userNotificationChannelPreferenceService.ClearRecordCache(userNotificationChannelPreferenceId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userNotificationChannelPreferenceService.GetUserNotificationChannelPreference(userNotificationChannelPreferenceId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userNotificationChannelPreferenceData) => {

        //
        // Success path — userNotificationChannelPreferenceData can legitimately be null if 404'd but request succeeded
        //
        if (!userNotificationChannelPreferenceData) {

          this.handleUserNotificationChannelPreferenceNotFound(userNotificationChannelPreferenceId);

        } else {

          this.userNotificationChannelPreferenceData = userNotificationChannelPreferenceData;
          this.buildFormValues(this.userNotificationChannelPreferenceData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserNotificationChannelPreference loaded successfully',
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
        this.handleUserNotificationChannelPreferenceLoadError(error, userNotificationChannelPreferenceId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserNotificationChannelPreferenceNotFound(userNotificationChannelPreferenceId: number): void {

    this.userNotificationChannelPreferenceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserNotificationChannelPreference #${userNotificationChannelPreferenceId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserNotificationChannelPreferenceLoadError(error: any, userNotificationChannelPreferenceId: number): void {

    let message = 'Failed to load User Notification Channel Preference.';
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
          message = 'You do not have permission to view this User Notification Channel Preference.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Notification Channel Preference #${userNotificationChannelPreferenceId} was not found.`;
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

    console.error(`User Notification Channel Preference load failed (ID: ${userNotificationChannelPreferenceId})`, error);

    //
    // Reset UI to safe state
    //
    this.userNotificationChannelPreferenceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userNotificationChannelPreferenceData: UserNotificationChannelPreferenceData | null) {

    if (userNotificationChannelPreferenceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userNotificationChannelPreferenceForm.reset({
        userNotificationPreferenceId: null,
        notificationChannelTypeId: null,
        isEnabled: false,
        priorityOverride: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userNotificationChannelPreferenceForm.reset({
        userNotificationPreferenceId: userNotificationChannelPreferenceData.userNotificationPreferenceId,
        notificationChannelTypeId: userNotificationChannelPreferenceData.notificationChannelTypeId,
        isEnabled: userNotificationChannelPreferenceData.isEnabled ?? false,
        priorityOverride: userNotificationChannelPreferenceData.priorityOverride?.toString() ?? '',
        versionNumber: userNotificationChannelPreferenceData.versionNumber?.toString() ?? '',
        active: userNotificationChannelPreferenceData.active ?? true,
        deleted: userNotificationChannelPreferenceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userNotificationChannelPreferenceForm.markAsPristine();
    this.userNotificationChannelPreferenceForm.markAsUntouched();
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

    if (this.userNotificationChannelPreferenceService.userIsAlertingUserNotificationChannelPreferenceWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Notification Channel Preferences", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userNotificationChannelPreferenceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userNotificationChannelPreferenceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userNotificationChannelPreferenceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userNotificationChannelPreferenceSubmitData: UserNotificationChannelPreferenceSubmitData = {
        id: this.userNotificationChannelPreferenceData?.id || 0,
        userNotificationPreferenceId: Number(formValue.userNotificationPreferenceId),
        notificationChannelTypeId: Number(formValue.notificationChannelTypeId),
        isEnabled: !!formValue.isEnabled,
        priorityOverride: formValue.priorityOverride ? Number(formValue.priorityOverride) : null,
        versionNumber: this.userNotificationChannelPreferenceData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userNotificationChannelPreferenceService.PutUserNotificationChannelPreference(userNotificationChannelPreferenceSubmitData.id, userNotificationChannelPreferenceSubmitData)
      : this.userNotificationChannelPreferenceService.PostUserNotificationChannelPreference(userNotificationChannelPreferenceSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserNotificationChannelPreferenceData) => {

        this.userNotificationChannelPreferenceService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Notification Channel Preference's detail page
          //
          this.userNotificationChannelPreferenceForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userNotificationChannelPreferenceForm.markAsUntouched();

          this.router.navigate(['/usernotificationchannelpreferences', savedUserNotificationChannelPreferenceData.id]);
          this.alertService.showMessage('User Notification Channel Preference added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userNotificationChannelPreferenceData = savedUserNotificationChannelPreferenceData;
          this.buildFormValues(this.userNotificationChannelPreferenceData);

          this.alertService.showMessage("User Notification Channel Preference saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Notification Channel Preference.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Notification Channel Preference.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Notification Channel Preference could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingUserNotificationChannelPreferenceReader(): boolean {
    return this.userNotificationChannelPreferenceService.userIsAlertingUserNotificationChannelPreferenceReader();
  }

  public userIsAlertingUserNotificationChannelPreferenceWriter(): boolean {
    return this.userNotificationChannelPreferenceService.userIsAlertingUserNotificationChannelPreferenceWriter();
  }
}
