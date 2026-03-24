/*
   GENERATED FORM FOR THE EVENTNOTIFICATIONSUBSCRIPTIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventNotificationSubscriptionChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-notification-subscription-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventNotificationSubscriptionChangeHistoryService, EventNotificationSubscriptionChangeHistoryData, EventNotificationSubscriptionChangeHistorySubmitData } from '../../../scheduler-data-services/event-notification-subscription-change-history.service';
import { EventNotificationSubscriptionService } from '../../../scheduler-data-services/event-notification-subscription.service';
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
interface EventNotificationSubscriptionChangeHistoryFormValues {
  eventNotificationSubscriptionId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-event-notification-subscription-change-history-detail',
  templateUrl: './event-notification-subscription-change-history-detail.component.html',
  styleUrls: ['./event-notification-subscription-change-history-detail.component.scss']
})

export class EventNotificationSubscriptionChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventNotificationSubscriptionChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventNotificationSubscriptionChangeHistoryForm: FormGroup = this.fb.group({
        eventNotificationSubscriptionId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public eventNotificationSubscriptionChangeHistoryId: string | null = null;
  public eventNotificationSubscriptionChangeHistoryData: EventNotificationSubscriptionChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  eventNotificationSubscriptionChangeHistories$ = this.eventNotificationSubscriptionChangeHistoryService.GetEventNotificationSubscriptionChangeHistoryList();
  public eventNotificationSubscriptions$ = this.eventNotificationSubscriptionService.GetEventNotificationSubscriptionList();

  private destroy$ = new Subject<void>();

  constructor(
    public eventNotificationSubscriptionChangeHistoryService: EventNotificationSubscriptionChangeHistoryService,
    public eventNotificationSubscriptionService: EventNotificationSubscriptionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the eventNotificationSubscriptionChangeHistoryId from the route parameters
    this.eventNotificationSubscriptionChangeHistoryId = this.route.snapshot.paramMap.get('eventNotificationSubscriptionChangeHistoryId');

    if (this.eventNotificationSubscriptionChangeHistoryId === 'new' ||
        this.eventNotificationSubscriptionChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.eventNotificationSubscriptionChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.eventNotificationSubscriptionChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventNotificationSubscriptionChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Event Notification Subscription Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Event Notification Subscription Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.eventNotificationSubscriptionChangeHistoryForm.dirty) {
      return confirm('You have unsaved Event Notification Subscription Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.eventNotificationSubscriptionChangeHistoryId != null && this.eventNotificationSubscriptionChangeHistoryId !== 'new') {

      const id = parseInt(this.eventNotificationSubscriptionChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { eventNotificationSubscriptionChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the EventNotificationSubscriptionChangeHistory data for the current eventNotificationSubscriptionChangeHistoryId.
  *
  * Fully respects the EventNotificationSubscriptionChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.eventNotificationSubscriptionChangeHistoryService.userIsSchedulerEventNotificationSubscriptionChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EventNotificationSubscriptionChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate eventNotificationSubscriptionChangeHistoryId
    //
    if (!this.eventNotificationSubscriptionChangeHistoryId) {

      this.alertService.showMessage('No EventNotificationSubscriptionChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const eventNotificationSubscriptionChangeHistoryId = Number(this.eventNotificationSubscriptionChangeHistoryId);

    if (isNaN(eventNotificationSubscriptionChangeHistoryId) || eventNotificationSubscriptionChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Event Notification Subscription Change History ID: "${this.eventNotificationSubscriptionChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this EventNotificationSubscriptionChangeHistory + relations

      this.eventNotificationSubscriptionChangeHistoryService.ClearRecordCache(eventNotificationSubscriptionChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.eventNotificationSubscriptionChangeHistoryService.GetEventNotificationSubscriptionChangeHistory(eventNotificationSubscriptionChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (eventNotificationSubscriptionChangeHistoryData) => {

        //
        // Success path — eventNotificationSubscriptionChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!eventNotificationSubscriptionChangeHistoryData) {

          this.handleEventNotificationSubscriptionChangeHistoryNotFound(eventNotificationSubscriptionChangeHistoryId);

        } else {

          this.eventNotificationSubscriptionChangeHistoryData = eventNotificationSubscriptionChangeHistoryData;
          this.buildFormValues(this.eventNotificationSubscriptionChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EventNotificationSubscriptionChangeHistory loaded successfully',
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
        this.handleEventNotificationSubscriptionChangeHistoryLoadError(error, eventNotificationSubscriptionChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEventNotificationSubscriptionChangeHistoryNotFound(eventNotificationSubscriptionChangeHistoryId: number): void {

    this.eventNotificationSubscriptionChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EventNotificationSubscriptionChangeHistory #${eventNotificationSubscriptionChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEventNotificationSubscriptionChangeHistoryLoadError(error: any, eventNotificationSubscriptionChangeHistoryId: number): void {

    let message = 'Failed to load Event Notification Subscription Change History.';
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
          message = 'You do not have permission to view this Event Notification Subscription Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Event Notification Subscription Change History #${eventNotificationSubscriptionChangeHistoryId} was not found.`;
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

    console.error(`Event Notification Subscription Change History load failed (ID: ${eventNotificationSubscriptionChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.eventNotificationSubscriptionChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(eventNotificationSubscriptionChangeHistoryData: EventNotificationSubscriptionChangeHistoryData | null) {

    if (eventNotificationSubscriptionChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventNotificationSubscriptionChangeHistoryForm.reset({
        eventNotificationSubscriptionId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventNotificationSubscriptionChangeHistoryForm.reset({
        eventNotificationSubscriptionId: eventNotificationSubscriptionChangeHistoryData.eventNotificationSubscriptionId,
        versionNumber: eventNotificationSubscriptionChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(eventNotificationSubscriptionChangeHistoryData.timeStamp) ?? '',
        userId: eventNotificationSubscriptionChangeHistoryData.userId?.toString() ?? '',
        data: eventNotificationSubscriptionChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.eventNotificationSubscriptionChangeHistoryForm.markAsPristine();
    this.eventNotificationSubscriptionChangeHistoryForm.markAsUntouched();
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

    if (this.eventNotificationSubscriptionChangeHistoryService.userIsSchedulerEventNotificationSubscriptionChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Event Notification Subscription Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.eventNotificationSubscriptionChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventNotificationSubscriptionChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventNotificationSubscriptionChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventNotificationSubscriptionChangeHistorySubmitData: EventNotificationSubscriptionChangeHistorySubmitData = {
        id: this.eventNotificationSubscriptionChangeHistoryData?.id || 0,
        eventNotificationSubscriptionId: Number(formValue.eventNotificationSubscriptionId),
        versionNumber: this.eventNotificationSubscriptionChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.eventNotificationSubscriptionChangeHistoryService.PutEventNotificationSubscriptionChangeHistory(eventNotificationSubscriptionChangeHistorySubmitData.id, eventNotificationSubscriptionChangeHistorySubmitData)
      : this.eventNotificationSubscriptionChangeHistoryService.PostEventNotificationSubscriptionChangeHistory(eventNotificationSubscriptionChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEventNotificationSubscriptionChangeHistoryData) => {

        this.eventNotificationSubscriptionChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Event Notification Subscription Change History's detail page
          //
          this.eventNotificationSubscriptionChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.eventNotificationSubscriptionChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/eventnotificationsubscriptionchangehistories', savedEventNotificationSubscriptionChangeHistoryData.id]);
          this.alertService.showMessage('Event Notification Subscription Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.eventNotificationSubscriptionChangeHistoryData = savedEventNotificationSubscriptionChangeHistoryData;
          this.buildFormValues(this.eventNotificationSubscriptionChangeHistoryData);

          this.alertService.showMessage("Event Notification Subscription Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Event Notification Subscription Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Notification Subscription Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Notification Subscription Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerEventNotificationSubscriptionChangeHistoryReader(): boolean {
    return this.eventNotificationSubscriptionChangeHistoryService.userIsSchedulerEventNotificationSubscriptionChangeHistoryReader();
  }

  public userIsSchedulerEventNotificationSubscriptionChangeHistoryWriter(): boolean {
    return this.eventNotificationSubscriptionChangeHistoryService.userIsSchedulerEventNotificationSubscriptionChangeHistoryWriter();
  }
}
