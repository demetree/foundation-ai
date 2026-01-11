/*
   GENERATED FORM FOR THE NOTIFICATIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from NotificationType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to notification-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NotificationTypeService, NotificationTypeData, NotificationTypeSubmitData } from '../../../scheduler-data-services/notification-type.service';
import { NotificationSubscriptionService } from '../../../scheduler-data-services/notification-subscription.service';
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
interface NotificationTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-notification-type-detail',
  templateUrl: './notification-type-detail.component.html',
  styleUrls: ['./notification-type-detail.component.scss']
})

export class NotificationTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<NotificationTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public notificationTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        color: [''],
        active: [true],
        deleted: [false],
      });


  public notificationTypeId: string | null = null;
  public notificationTypeData: NotificationTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  notificationTypes$ = this.notificationTypeService.GetNotificationTypeList();
  public notificationSubscriptions$ = this.notificationSubscriptionService.GetNotificationSubscriptionList();

  private destroy$ = new Subject<void>();

  constructor(
    public notificationTypeService: NotificationTypeService,
    public notificationSubscriptionService: NotificationSubscriptionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the notificationTypeId from the route parameters
    this.notificationTypeId = this.route.snapshot.paramMap.get('notificationTypeId');

    if (this.notificationTypeId === 'new' ||
        this.notificationTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.notificationTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.notificationTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.notificationTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Notification Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Notification Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.notificationTypeForm.dirty) {
      return confirm('You have unsaved Notification Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.notificationTypeId != null && this.notificationTypeId !== 'new') {

      const id = parseInt(this.notificationTypeId, 10);

      if (!isNaN(id)) {
        return { notificationTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the NotificationType data for the current notificationTypeId.
  *
  * Fully respects the NotificationTypeService caching strategy and error handling strategy.
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
    if (!this.notificationTypeService.userIsSchedulerNotificationTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read NotificationTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate notificationTypeId
    //
    if (!this.notificationTypeId) {

      this.alertService.showMessage('No NotificationType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const notificationTypeId = Number(this.notificationTypeId);

    if (isNaN(notificationTypeId) || notificationTypeId <= 0) {

      this.alertService.showMessage(`Invalid Notification Type ID: "${this.notificationTypeId}"`,
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
      // This is the most targeted way: clear only this NotificationType + relations

      this.notificationTypeService.ClearRecordCache(notificationTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.notificationTypeService.GetNotificationType(notificationTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (notificationTypeData) => {

        //
        // Success path — notificationTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!notificationTypeData) {

          this.handleNotificationTypeNotFound(notificationTypeId);

        } else {

          this.notificationTypeData = notificationTypeData;
          this.buildFormValues(this.notificationTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'NotificationType loaded successfully',
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
        this.handleNotificationTypeLoadError(error, notificationTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleNotificationTypeNotFound(notificationTypeId: number): void {

    this.notificationTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `NotificationType #${notificationTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleNotificationTypeLoadError(error: any, notificationTypeId: number): void {

    let message = 'Failed to load Notification Type.';
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
          message = 'You do not have permission to view this Notification Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Notification Type #${notificationTypeId} was not found.`;
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

    console.error(`Notification Type load failed (ID: ${notificationTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.notificationTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(notificationTypeData: NotificationTypeData | null) {

    if (notificationTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.notificationTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.notificationTypeForm.reset({
        name: notificationTypeData.name ?? '',
        description: notificationTypeData.description ?? '',
        sequence: notificationTypeData.sequence?.toString() ?? '',
        color: notificationTypeData.color ?? '',
        active: notificationTypeData.active ?? true,
        deleted: notificationTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.notificationTypeForm.markAsPristine();
    this.notificationTypeForm.markAsUntouched();
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

    if (this.notificationTypeService.userIsSchedulerNotificationTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Notification Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.notificationTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.notificationTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.notificationTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const notificationTypeSubmitData: NotificationTypeSubmitData = {
        id: this.notificationTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.notificationTypeService.PutNotificationType(notificationTypeSubmitData.id, notificationTypeSubmitData)
      : this.notificationTypeService.PostNotificationType(notificationTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedNotificationTypeData) => {

        this.notificationTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Notification Type's detail page
          //
          this.notificationTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.notificationTypeForm.markAsUntouched();

          this.router.navigate(['/notificationtypes', savedNotificationTypeData.id]);
          this.alertService.showMessage('Notification Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.notificationTypeData = savedNotificationTypeData;
          this.buildFormValues(this.notificationTypeData);

          this.alertService.showMessage("Notification Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Notification Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Notification Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Notification Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerNotificationTypeReader(): boolean {
    return this.notificationTypeService.userIsSchedulerNotificationTypeReader();
  }

  public userIsSchedulerNotificationTypeWriter(): boolean {
    return this.notificationTypeService.userIsSchedulerNotificationTypeWriter();
  }
}
