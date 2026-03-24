/*
   GENERATED FORM FOR THE EVENTNOTIFICATIONSUBSCRIPTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventNotificationSubscription table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-notification-subscription-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventNotificationSubscriptionService, EventNotificationSubscriptionData, EventNotificationSubscriptionSubmitData } from '../../../scheduler-data-services/event-notification-subscription.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { EventNotificationTypeService } from '../../../scheduler-data-services/event-notification-type.service';
import { EventNotificationSubscriptionChangeHistoryService } from '../../../scheduler-data-services/event-notification-subscription-change-history.service';
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
interface EventNotificationSubscriptionFormValues {
  resourceId: number | bigint | null,       // For FK link number
  contactId: number | bigint | null,       // For FK link number
  eventNotificationTypeId: number | bigint,       // For FK link number
  triggerEvents: string,     // Stored as string for form input, converted to number on submit.
  recipientAddress: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-event-notification-subscription-detail',
  templateUrl: './event-notification-subscription-detail.component.html',
  styleUrls: ['./event-notification-subscription-detail.component.scss']
})

export class EventNotificationSubscriptionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventNotificationSubscriptionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventNotificationSubscriptionForm: FormGroup = this.fb.group({
        resourceId: [null],
        contactId: [null],
        eventNotificationTypeId: [null, Validators.required],
        triggerEvents: ['', Validators.required],
        recipientAddress: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public eventNotificationSubscriptionId: string | null = null;
  public eventNotificationSubscriptionData: EventNotificationSubscriptionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  eventNotificationSubscriptions$ = this.eventNotificationSubscriptionService.GetEventNotificationSubscriptionList();
  public resources$ = this.resourceService.GetResourceList();
  public contacts$ = this.contactService.GetContactList();
  public eventNotificationTypes$ = this.eventNotificationTypeService.GetEventNotificationTypeList();
  public eventNotificationSubscriptionChangeHistories$ = this.eventNotificationSubscriptionChangeHistoryService.GetEventNotificationSubscriptionChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public eventNotificationSubscriptionService: EventNotificationSubscriptionService,
    public resourceService: ResourceService,
    public contactService: ContactService,
    public eventNotificationTypeService: EventNotificationTypeService,
    public eventNotificationSubscriptionChangeHistoryService: EventNotificationSubscriptionChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the eventNotificationSubscriptionId from the route parameters
    this.eventNotificationSubscriptionId = this.route.snapshot.paramMap.get('eventNotificationSubscriptionId');

    if (this.eventNotificationSubscriptionId === 'new' ||
        this.eventNotificationSubscriptionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.eventNotificationSubscriptionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.eventNotificationSubscriptionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventNotificationSubscriptionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Event Notification Subscription';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Event Notification Subscription';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.eventNotificationSubscriptionForm.dirty) {
      return confirm('You have unsaved Event Notification Subscription changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.eventNotificationSubscriptionId != null && this.eventNotificationSubscriptionId !== 'new') {

      const id = parseInt(this.eventNotificationSubscriptionId, 10);

      if (!isNaN(id)) {
        return { eventNotificationSubscriptionId: id };
      }
    }

    return null;
  }


/*
  * Loads the EventNotificationSubscription data for the current eventNotificationSubscriptionId.
  *
  * Fully respects the EventNotificationSubscriptionService caching strategy and error handling strategy.
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
    if (!this.eventNotificationSubscriptionService.userIsSchedulerEventNotificationSubscriptionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EventNotificationSubscriptions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate eventNotificationSubscriptionId
    //
    if (!this.eventNotificationSubscriptionId) {

      this.alertService.showMessage('No EventNotificationSubscription ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const eventNotificationSubscriptionId = Number(this.eventNotificationSubscriptionId);

    if (isNaN(eventNotificationSubscriptionId) || eventNotificationSubscriptionId <= 0) {

      this.alertService.showMessage(`Invalid Event Notification Subscription ID: "${this.eventNotificationSubscriptionId}"`,
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
      // This is the most targeted way: clear only this EventNotificationSubscription + relations

      this.eventNotificationSubscriptionService.ClearRecordCache(eventNotificationSubscriptionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.eventNotificationSubscriptionService.GetEventNotificationSubscription(eventNotificationSubscriptionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (eventNotificationSubscriptionData) => {

        //
        // Success path — eventNotificationSubscriptionData can legitimately be null if 404'd but request succeeded
        //
        if (!eventNotificationSubscriptionData) {

          this.handleEventNotificationSubscriptionNotFound(eventNotificationSubscriptionId);

        } else {

          this.eventNotificationSubscriptionData = eventNotificationSubscriptionData;
          this.buildFormValues(this.eventNotificationSubscriptionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EventNotificationSubscription loaded successfully',
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
        this.handleEventNotificationSubscriptionLoadError(error, eventNotificationSubscriptionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEventNotificationSubscriptionNotFound(eventNotificationSubscriptionId: number): void {

    this.eventNotificationSubscriptionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EventNotificationSubscription #${eventNotificationSubscriptionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEventNotificationSubscriptionLoadError(error: any, eventNotificationSubscriptionId: number): void {

    let message = 'Failed to load Event Notification Subscription.';
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
          message = 'You do not have permission to view this Event Notification Subscription.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Event Notification Subscription #${eventNotificationSubscriptionId} was not found.`;
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

    console.error(`Event Notification Subscription load failed (ID: ${eventNotificationSubscriptionId})`, error);

    //
    // Reset UI to safe state
    //
    this.eventNotificationSubscriptionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(eventNotificationSubscriptionData: EventNotificationSubscriptionData | null) {

    if (eventNotificationSubscriptionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventNotificationSubscriptionForm.reset({
        resourceId: null,
        contactId: null,
        eventNotificationTypeId: null,
        triggerEvents: '',
        recipientAddress: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventNotificationSubscriptionForm.reset({
        resourceId: eventNotificationSubscriptionData.resourceId,
        contactId: eventNotificationSubscriptionData.contactId,
        eventNotificationTypeId: eventNotificationSubscriptionData.eventNotificationTypeId,
        triggerEvents: eventNotificationSubscriptionData.triggerEvents?.toString() ?? '',
        recipientAddress: eventNotificationSubscriptionData.recipientAddress ?? '',
        versionNumber: eventNotificationSubscriptionData.versionNumber?.toString() ?? '',
        active: eventNotificationSubscriptionData.active ?? true,
        deleted: eventNotificationSubscriptionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.eventNotificationSubscriptionForm.markAsPristine();
    this.eventNotificationSubscriptionForm.markAsUntouched();
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

    if (this.eventNotificationSubscriptionService.userIsSchedulerEventNotificationSubscriptionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Event Notification Subscriptions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.eventNotificationSubscriptionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventNotificationSubscriptionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventNotificationSubscriptionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventNotificationSubscriptionSubmitData: EventNotificationSubscriptionSubmitData = {
        id: this.eventNotificationSubscriptionData?.id || 0,
        resourceId: formValue.resourceId ? Number(formValue.resourceId) : null,
        contactId: formValue.contactId ? Number(formValue.contactId) : null,
        eventNotificationTypeId: Number(formValue.eventNotificationTypeId),
        triggerEvents: Number(formValue.triggerEvents),
        recipientAddress: formValue.recipientAddress!.trim(),
        versionNumber: this.eventNotificationSubscriptionData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.eventNotificationSubscriptionService.PutEventNotificationSubscription(eventNotificationSubscriptionSubmitData.id, eventNotificationSubscriptionSubmitData)
      : this.eventNotificationSubscriptionService.PostEventNotificationSubscription(eventNotificationSubscriptionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEventNotificationSubscriptionData) => {

        this.eventNotificationSubscriptionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Event Notification Subscription's detail page
          //
          this.eventNotificationSubscriptionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.eventNotificationSubscriptionForm.markAsUntouched();

          this.router.navigate(['/eventnotificationsubscriptions', savedEventNotificationSubscriptionData.id]);
          this.alertService.showMessage('Event Notification Subscription added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.eventNotificationSubscriptionData = savedEventNotificationSubscriptionData;
          this.buildFormValues(this.eventNotificationSubscriptionData);

          this.alertService.showMessage("Event Notification Subscription saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Event Notification Subscription.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Notification Subscription.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Notification Subscription could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerEventNotificationSubscriptionReader(): boolean {
    return this.eventNotificationSubscriptionService.userIsSchedulerEventNotificationSubscriptionReader();
  }

  public userIsSchedulerEventNotificationSubscriptionWriter(): boolean {
    return this.eventNotificationSubscriptionService.userIsSchedulerEventNotificationSubscriptionWriter();
  }
}
