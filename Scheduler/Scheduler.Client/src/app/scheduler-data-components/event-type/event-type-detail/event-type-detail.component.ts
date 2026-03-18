/*
   GENERATED FORM FOR THE EVENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventTypeService, EventTypeData, EventTypeSubmitData } from '../../../scheduler-data-services/event-type.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
import { EventTypeChangeHistoryService } from '../../../scheduler-data-services/event-type-change-history.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
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
interface EventTypeFormValues {
  name: string,
  description: string,
  color: string | null,
  iconId: number | bigint | null,       // For FK link number
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  requiresRentalAgreement: boolean,
  requiresExternalContact: boolean,
  requiresPayment: boolean,
  requiresDeposit: boolean,
  requiresBarService: boolean,
  allowsTicketSales: boolean,
  isInternalEvent: boolean,
  defaultPrice: string | null,     // Stored as string for form input, converted to number on submit.
  chargeTypeId: number | bigint | null,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-event-type-detail',
  templateUrl: './event-type-detail.component.html',
  styleUrls: ['./event-type-detail.component.scss']
})

export class EventTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        color: [''],
        iconId: [null],
        sequence: [''],
        requiresRentalAgreement: [false],
        requiresExternalContact: [false],
        requiresPayment: [false],
        requiresDeposit: [false],
        requiresBarService: [false],
        allowsTicketSales: [false],
        isInternalEvent: [false],
        defaultPrice: [''],
        chargeTypeId: [null],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public eventTypeId: string | null = null;
  public eventTypeData: EventTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  eventTypes$ = this.eventTypeService.GetEventTypeList();
  public icons$ = this.iconService.GetIconList();
  public chargeTypes$ = this.chargeTypeService.GetChargeTypeList();
  public eventTypeChangeHistories$ = this.eventTypeChangeHistoryService.GetEventTypeChangeHistoryList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public eventTypeService: EventTypeService,
    public iconService: IconService,
    public chargeTypeService: ChargeTypeService,
    public eventTypeChangeHistoryService: EventTypeChangeHistoryService,
    public scheduledEventService: ScheduledEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the eventTypeId from the route parameters
    this.eventTypeId = this.route.snapshot.paramMap.get('eventTypeId');

    if (this.eventTypeId === 'new' ||
        this.eventTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.eventTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.eventTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Event Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Event Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.eventTypeForm.dirty) {
      return confirm('You have unsaved Event Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.eventTypeId != null && this.eventTypeId !== 'new') {

      const id = parseInt(this.eventTypeId, 10);

      if (!isNaN(id)) {
        return { eventTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the EventType data for the current eventTypeId.
  *
  * Fully respects the EventTypeService caching strategy and error handling strategy.
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
    if (!this.eventTypeService.userIsSchedulerEventTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EventTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate eventTypeId
    //
    if (!this.eventTypeId) {

      this.alertService.showMessage('No EventType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const eventTypeId = Number(this.eventTypeId);

    if (isNaN(eventTypeId) || eventTypeId <= 0) {

      this.alertService.showMessage(`Invalid Event Type ID: "${this.eventTypeId}"`,
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
      // This is the most targeted way: clear only this EventType + relations

      this.eventTypeService.ClearRecordCache(eventTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.eventTypeService.GetEventType(eventTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (eventTypeData) => {

        //
        // Success path — eventTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!eventTypeData) {

          this.handleEventTypeNotFound(eventTypeId);

        } else {

          this.eventTypeData = eventTypeData;
          this.buildFormValues(this.eventTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EventType loaded successfully',
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
        this.handleEventTypeLoadError(error, eventTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEventTypeNotFound(eventTypeId: number): void {

    this.eventTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EventType #${eventTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEventTypeLoadError(error: any, eventTypeId: number): void {

    let message = 'Failed to load Event Type.';
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
          message = 'You do not have permission to view this Event Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Event Type #${eventTypeId} was not found.`;
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

    console.error(`Event Type load failed (ID: ${eventTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.eventTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(eventTypeData: EventTypeData | null) {

    if (eventTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventTypeForm.reset({
        name: '',
        description: '',
        color: '',
        iconId: null,
        sequence: '',
        requiresRentalAgreement: false,
        requiresExternalContact: false,
        requiresPayment: false,
        requiresDeposit: false,
        requiresBarService: false,
        allowsTicketSales: false,
        isInternalEvent: false,
        defaultPrice: '',
        chargeTypeId: null,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventTypeForm.reset({
        name: eventTypeData.name ?? '',
        description: eventTypeData.description ?? '',
        color: eventTypeData.color ?? '',
        iconId: eventTypeData.iconId,
        sequence: eventTypeData.sequence?.toString() ?? '',
        requiresRentalAgreement: eventTypeData.requiresRentalAgreement ?? false,
        requiresExternalContact: eventTypeData.requiresExternalContact ?? false,
        requiresPayment: eventTypeData.requiresPayment ?? false,
        requiresDeposit: eventTypeData.requiresDeposit ?? false,
        requiresBarService: eventTypeData.requiresBarService ?? false,
        allowsTicketSales: eventTypeData.allowsTicketSales ?? false,
        isInternalEvent: eventTypeData.isInternalEvent ?? false,
        defaultPrice: eventTypeData.defaultPrice?.toString() ?? '',
        chargeTypeId: eventTypeData.chargeTypeId,
        versionNumber: eventTypeData.versionNumber?.toString() ?? '',
        active: eventTypeData.active ?? true,
        deleted: eventTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.eventTypeForm.markAsPristine();
    this.eventTypeForm.markAsUntouched();
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

    if (this.eventTypeService.userIsSchedulerEventTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Event Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.eventTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventTypeSubmitData: EventTypeSubmitData = {
        id: this.eventTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        color: formValue.color?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        requiresRentalAgreement: !!formValue.requiresRentalAgreement,
        requiresExternalContact: !!formValue.requiresExternalContact,
        requiresPayment: !!formValue.requiresPayment,
        requiresDeposit: !!formValue.requiresDeposit,
        requiresBarService: !!formValue.requiresBarService,
        allowsTicketSales: !!formValue.allowsTicketSales,
        isInternalEvent: !!formValue.isInternalEvent,
        defaultPrice: formValue.defaultPrice ? Number(formValue.defaultPrice) : null,
        chargeTypeId: formValue.chargeTypeId ? Number(formValue.chargeTypeId) : null,
        versionNumber: this.eventTypeData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.eventTypeService.PutEventType(eventTypeSubmitData.id, eventTypeSubmitData)
      : this.eventTypeService.PostEventType(eventTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEventTypeData) => {

        this.eventTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Event Type's detail page
          //
          this.eventTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.eventTypeForm.markAsUntouched();

          this.router.navigate(['/eventtypes', savedEventTypeData.id]);
          this.alertService.showMessage('Event Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.eventTypeData = savedEventTypeData;
          this.buildFormValues(this.eventTypeData);

          this.alertService.showMessage("Event Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerEventTypeReader(): boolean {
    return this.eventTypeService.userIsSchedulerEventTypeReader();
  }

  public userIsSchedulerEventTypeWriter(): boolean {
    return this.eventTypeService.userIsSchedulerEventTypeWriter();
  }
}
