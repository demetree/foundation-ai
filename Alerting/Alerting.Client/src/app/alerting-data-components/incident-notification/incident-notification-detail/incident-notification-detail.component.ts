/*
   GENERATED FORM FOR THE INCIDENTNOTIFICATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IncidentNotification table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-notification-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentNotificationService, IncidentNotificationData, IncidentNotificationSubmitData } from '../../../alerting-data-services/incident-notification.service';
import { IncidentService } from '../../../alerting-data-services/incident.service';
import { EscalationRuleService } from '../../../alerting-data-services/escalation-rule.service';
import { NotificationDeliveryAttemptService } from '../../../alerting-data-services/notification-delivery-attempt.service';
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
interface IncidentNotificationFormValues {
  incidentId: number | bigint,       // For FK link number
  escalationRuleId: number | bigint | null,       // For FK link number
  userObjectGuid: string,
  firstNotifiedAt: string,
  lastNotifiedAt: string | null,
  acknowledgedAt: string | null,
  acknowledgedByObjectGuid: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-incident-notification-detail',
  templateUrl: './incident-notification-detail.component.html',
  styleUrls: ['./incident-notification-detail.component.scss']
})

export class IncidentNotificationDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentNotificationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentNotificationForm: FormGroup = this.fb.group({
        incidentId: [null, Validators.required],
        escalationRuleId: [null],
        userObjectGuid: ['', Validators.required],
        firstNotifiedAt: ['', Validators.required],
        lastNotifiedAt: [''],
        acknowledgedAt: [''],
        acknowledgedByObjectGuid: [''],
        active: [true],
        deleted: [false],
      });


  public incidentNotificationId: string | null = null;
  public incidentNotificationData: IncidentNotificationData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  incidentNotifications$ = this.incidentNotificationService.GetIncidentNotificationList();
  public incidents$ = this.incidentService.GetIncidentList();
  public escalationRules$ = this.escalationRuleService.GetEscalationRuleList();
  public notificationDeliveryAttempts$ = this.notificationDeliveryAttemptService.GetNotificationDeliveryAttemptList();

  private destroy$ = new Subject<void>();

  constructor(
    public incidentNotificationService: IncidentNotificationService,
    public incidentService: IncidentService,
    public escalationRuleService: EscalationRuleService,
    public notificationDeliveryAttemptService: NotificationDeliveryAttemptService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the incidentNotificationId from the route parameters
    this.incidentNotificationId = this.route.snapshot.paramMap.get('incidentNotificationId');

    if (this.incidentNotificationId === 'new' ||
        this.incidentNotificationId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.incidentNotificationData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.incidentNotificationForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentNotificationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Incident Notification';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Incident Notification';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.incidentNotificationForm.dirty) {
      return confirm('You have unsaved Incident Notification changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.incidentNotificationId != null && this.incidentNotificationId !== 'new') {

      const id = parseInt(this.incidentNotificationId, 10);

      if (!isNaN(id)) {
        return { incidentNotificationId: id };
      }
    }

    return null;
  }


/*
  * Loads the IncidentNotification data for the current incidentNotificationId.
  *
  * Fully respects the IncidentNotificationService caching strategy and error handling strategy.
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
    if (!this.incidentNotificationService.userIsAlertingIncidentNotificationReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read IncidentNotifications.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate incidentNotificationId
    //
    if (!this.incidentNotificationId) {

      this.alertService.showMessage('No IncidentNotification ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const incidentNotificationId = Number(this.incidentNotificationId);

    if (isNaN(incidentNotificationId) || incidentNotificationId <= 0) {

      this.alertService.showMessage(`Invalid Incident Notification ID: "${this.incidentNotificationId}"`,
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
      // This is the most targeted way: clear only this IncidentNotification + relations

      this.incidentNotificationService.ClearRecordCache(incidentNotificationId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.incidentNotificationService.GetIncidentNotification(incidentNotificationId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (incidentNotificationData) => {

        //
        // Success path — incidentNotificationData can legitimately be null if 404'd but request succeeded
        //
        if (!incidentNotificationData) {

          this.handleIncidentNotificationNotFound(incidentNotificationId);

        } else {

          this.incidentNotificationData = incidentNotificationData;
          this.buildFormValues(this.incidentNotificationData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'IncidentNotification loaded successfully',
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
        this.handleIncidentNotificationLoadError(error, incidentNotificationId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleIncidentNotificationNotFound(incidentNotificationId: number): void {

    this.incidentNotificationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `IncidentNotification #${incidentNotificationId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleIncidentNotificationLoadError(error: any, incidentNotificationId: number): void {

    let message = 'Failed to load Incident Notification.';
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
          message = 'You do not have permission to view this Incident Notification.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Incident Notification #${incidentNotificationId} was not found.`;
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

    console.error(`Incident Notification load failed (ID: ${incidentNotificationId})`, error);

    //
    // Reset UI to safe state
    //
    this.incidentNotificationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(incidentNotificationData: IncidentNotificationData | null) {

    if (incidentNotificationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentNotificationForm.reset({
        incidentId: null,
        escalationRuleId: null,
        userObjectGuid: '',
        firstNotifiedAt: '',
        lastNotifiedAt: '',
        acknowledgedAt: '',
        acknowledgedByObjectGuid: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.incidentNotificationForm.reset({
        incidentId: incidentNotificationData.incidentId,
        escalationRuleId: incidentNotificationData.escalationRuleId,
        userObjectGuid: incidentNotificationData.userObjectGuid ?? '',
        firstNotifiedAt: isoUtcStringToDateTimeLocal(incidentNotificationData.firstNotifiedAt) ?? '',
        lastNotifiedAt: isoUtcStringToDateTimeLocal(incidentNotificationData.lastNotifiedAt) ?? '',
        acknowledgedAt: isoUtcStringToDateTimeLocal(incidentNotificationData.acknowledgedAt) ?? '',
        acknowledgedByObjectGuid: incidentNotificationData.acknowledgedByObjectGuid ?? '',
        active: incidentNotificationData.active ?? true,
        deleted: incidentNotificationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.incidentNotificationForm.markAsPristine();
    this.incidentNotificationForm.markAsUntouched();
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

    if (this.incidentNotificationService.userIsAlertingIncidentNotificationWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Incident Notifications", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.incidentNotificationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentNotificationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentNotificationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentNotificationSubmitData: IncidentNotificationSubmitData = {
        id: this.incidentNotificationData?.id || 0,
        incidentId: Number(formValue.incidentId),
        escalationRuleId: formValue.escalationRuleId ? Number(formValue.escalationRuleId) : null,
        userObjectGuid: formValue.userObjectGuid!.trim(),
        firstNotifiedAt: dateTimeLocalToIsoUtc(formValue.firstNotifiedAt!.trim())!,
        lastNotifiedAt: formValue.lastNotifiedAt ? dateTimeLocalToIsoUtc(formValue.lastNotifiedAt.trim()) : null,
        acknowledgedAt: formValue.acknowledgedAt ? dateTimeLocalToIsoUtc(formValue.acknowledgedAt.trim()) : null,
        acknowledgedByObjectGuid: formValue.acknowledgedByObjectGuid?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.incidentNotificationService.PutIncidentNotification(incidentNotificationSubmitData.id, incidentNotificationSubmitData)
      : this.incidentNotificationService.PostIncidentNotification(incidentNotificationSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedIncidentNotificationData) => {

        this.incidentNotificationService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Incident Notification's detail page
          //
          this.incidentNotificationForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.incidentNotificationForm.markAsUntouched();

          this.router.navigate(['/incidentnotifications', savedIncidentNotificationData.id]);
          this.alertService.showMessage('Incident Notification added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.incidentNotificationData = savedIncidentNotificationData;
          this.buildFormValues(this.incidentNotificationData);

          this.alertService.showMessage("Incident Notification saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Incident Notification.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Notification.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Notification could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingIncidentNotificationReader(): boolean {
    return this.incidentNotificationService.userIsAlertingIncidentNotificationReader();
  }

  public userIsAlertingIncidentNotificationWriter(): boolean {
    return this.incidentNotificationService.userIsAlertingIncidentNotificationWriter();
  }
}
