/*
   GENERATED FORM FOR THE NOTIFICATIONDISTRIBUTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from NotificationDistribution table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to notification-distribution-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NotificationDistributionService, NotificationDistributionData, NotificationDistributionSubmitData } from '../../../scheduler-data-services/notification-distribution.service';
import { NotificationService } from '../../../scheduler-data-services/notification.service';
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
interface NotificationDistributionFormValues {
  notificationId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  acknowledged: boolean,
  dateTimeAcknowledged: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-notification-distribution-detail',
  templateUrl: './notification-distribution-detail.component.html',
  styleUrls: ['./notification-distribution-detail.component.scss']
})

export class NotificationDistributionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<NotificationDistributionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public notificationDistributionForm: FormGroup = this.fb.group({
        notificationId: [null, Validators.required],
        userId: ['', Validators.required],
        acknowledged: [false],
        dateTimeAcknowledged: [''],
        active: [true],
        deleted: [false],
      });


  public notificationDistributionId: string | null = null;
  public notificationDistributionData: NotificationDistributionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  notificationDistributions$ = this.notificationDistributionService.GetNotificationDistributionList();
  public notifications$ = this.notificationService.GetNotificationList();

  private destroy$ = new Subject<void>();

  constructor(
    public notificationDistributionService: NotificationDistributionService,
    public notificationService: NotificationService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the notificationDistributionId from the route parameters
    this.notificationDistributionId = this.route.snapshot.paramMap.get('notificationDistributionId');

    if (this.notificationDistributionId === 'new' ||
        this.notificationDistributionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.notificationDistributionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.notificationDistributionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.notificationDistributionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Notification Distribution';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Notification Distribution';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.notificationDistributionForm.dirty) {
      return confirm('You have unsaved Notification Distribution changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.notificationDistributionId != null && this.notificationDistributionId !== 'new') {

      const id = parseInt(this.notificationDistributionId, 10);

      if (!isNaN(id)) {
        return { notificationDistributionId: id };
      }
    }

    return null;
  }


/*
  * Loads the NotificationDistribution data for the current notificationDistributionId.
  *
  * Fully respects the NotificationDistributionService caching strategy and error handling strategy.
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
    if (!this.notificationDistributionService.userIsSchedulerNotificationDistributionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read NotificationDistributions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate notificationDistributionId
    //
    if (!this.notificationDistributionId) {

      this.alertService.showMessage('No NotificationDistribution ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const notificationDistributionId = Number(this.notificationDistributionId);

    if (isNaN(notificationDistributionId) || notificationDistributionId <= 0) {

      this.alertService.showMessage(`Invalid Notification Distribution ID: "${this.notificationDistributionId}"`,
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
      // This is the most targeted way: clear only this NotificationDistribution + relations

      this.notificationDistributionService.ClearRecordCache(notificationDistributionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.notificationDistributionService.GetNotificationDistribution(notificationDistributionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (notificationDistributionData) => {

        //
        // Success path — notificationDistributionData can legitimately be null if 404'd but request succeeded
        //
        if (!notificationDistributionData) {

          this.handleNotificationDistributionNotFound(notificationDistributionId);

        } else {

          this.notificationDistributionData = notificationDistributionData;
          this.buildFormValues(this.notificationDistributionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'NotificationDistribution loaded successfully',
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
        this.handleNotificationDistributionLoadError(error, notificationDistributionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleNotificationDistributionNotFound(notificationDistributionId: number): void {

    this.notificationDistributionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `NotificationDistribution #${notificationDistributionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleNotificationDistributionLoadError(error: any, notificationDistributionId: number): void {

    let message = 'Failed to load Notification Distribution.';
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
          message = 'You do not have permission to view this Notification Distribution.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Notification Distribution #${notificationDistributionId} was not found.`;
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

    console.error(`Notification Distribution load failed (ID: ${notificationDistributionId})`, error);

    //
    // Reset UI to safe state
    //
    this.notificationDistributionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(notificationDistributionData: NotificationDistributionData | null) {

    if (notificationDistributionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.notificationDistributionForm.reset({
        notificationId: null,
        userId: '',
        acknowledged: false,
        dateTimeAcknowledged: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.notificationDistributionForm.reset({
        notificationId: notificationDistributionData.notificationId,
        userId: notificationDistributionData.userId?.toString() ?? '',
        acknowledged: notificationDistributionData.acknowledged ?? false,
        dateTimeAcknowledged: isoUtcStringToDateTimeLocal(notificationDistributionData.dateTimeAcknowledged) ?? '',
        active: notificationDistributionData.active ?? true,
        deleted: notificationDistributionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.notificationDistributionForm.markAsPristine();
    this.notificationDistributionForm.markAsUntouched();
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

    if (this.notificationDistributionService.userIsSchedulerNotificationDistributionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Notification Distributions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.notificationDistributionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.notificationDistributionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.notificationDistributionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const notificationDistributionSubmitData: NotificationDistributionSubmitData = {
        id: this.notificationDistributionData?.id || 0,
        notificationId: Number(formValue.notificationId),
        userId: Number(formValue.userId),
        acknowledged: !!formValue.acknowledged,
        dateTimeAcknowledged: formValue.dateTimeAcknowledged ? dateTimeLocalToIsoUtc(formValue.dateTimeAcknowledged.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.notificationDistributionService.PutNotificationDistribution(notificationDistributionSubmitData.id, notificationDistributionSubmitData)
      : this.notificationDistributionService.PostNotificationDistribution(notificationDistributionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedNotificationDistributionData) => {

        this.notificationDistributionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Notification Distribution's detail page
          //
          this.notificationDistributionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.notificationDistributionForm.markAsUntouched();

          this.router.navigate(['/notificationdistributions', savedNotificationDistributionData.id]);
          this.alertService.showMessage('Notification Distribution added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.notificationDistributionData = savedNotificationDistributionData;
          this.buildFormValues(this.notificationDistributionData);

          this.alertService.showMessage("Notification Distribution saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Notification Distribution.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Notification Distribution.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Notification Distribution could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerNotificationDistributionReader(): boolean {
    return this.notificationDistributionService.userIsSchedulerNotificationDistributionReader();
  }

  public userIsSchedulerNotificationDistributionWriter(): boolean {
    return this.notificationDistributionService.userIsSchedulerNotificationDistributionWriter();
  }
}
