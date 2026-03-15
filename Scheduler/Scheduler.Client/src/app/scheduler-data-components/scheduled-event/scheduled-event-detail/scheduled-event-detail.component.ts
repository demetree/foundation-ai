/*
   GENERATED FORM FOR THE SCHEDULEDEVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventService, ScheduledEventData, ScheduledEventSubmitData } from '../../../scheduler-data-services/scheduled-event.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { ScheduledEventTemplateService } from '../../../scheduler-data-services/scheduled-event-template.service';
import { RecurrenceRuleService } from '../../../scheduler-data-services/recurrence-rule.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { EventStatusService } from '../../../scheduler-data-services/event-status.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { CrewService } from '../../../scheduler-data-services/crew.service';
import { PriorityService } from '../../../scheduler-data-services/priority.service';
import { BookingSourceTypeService } from '../../../scheduler-data-services/booking-source-type.service';
import { ScheduledEventChangeHistoryService } from '../../../scheduler-data-services/scheduled-event-change-history.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { PaymentTransactionService } from '../../../scheduler-data-services/payment-transaction.service';
import { InvoiceService } from '../../../scheduler-data-services/invoice.service';
import { DocumentService } from '../../../scheduler-data-services/document.service';
import { ContactInteractionService } from '../../../scheduler-data-services/contact-interaction.service';
import { EventCalendarService } from '../../../scheduler-data-services/event-calendar.service';
import { ScheduledEventDependencyService } from '../../../scheduler-data-services/scheduled-event-dependency.service';
import { ScheduledEventQualificationRequirementService } from '../../../scheduler-data-services/scheduled-event-qualification-requirement.service';
import { RecurrenceExceptionService } from '../../../scheduler-data-services/recurrence-exception.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
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
interface ScheduledEventFormValues {
  officeId: number | bigint | null,       // For FK link number
  clientId: number | bigint | null,       // For FK link number
  scheduledEventTemplateId: number | bigint | null,       // For FK link number
  recurrenceRuleId: number | bigint | null,       // For FK link number
  schedulingTargetId: number | bigint | null,       // For FK link number
  timeZoneId: number | bigint | null,       // For FK link number
  parentScheduledEventId: number | bigint | null,       // For FK link number
  recurrenceInstanceDate: string | null,
  name: string,
  description: string | null,
  isAllDay: boolean | null,
  startDateTime: string,
  endDateTime: string,
  location: string | null,
  eventStatusId: number | bigint,       // For FK link number
  resourceId: number | bigint | null,       // For FK link number
  crewId: number | bigint | null,       // For FK link number
  priorityId: number | bigint | null,       // For FK link number
  bookingSourceTypeId: number | bigint | null,       // For FK link number
  partySize: string | null,     // Stored as string for form input, converted to number on submit.
  bookingContactName: string | null,
  bookingContactEmail: string | null,
  bookingContactPhone: string | null,
  notes: string | null,
  color: string | null,
  externalId: string | null,
  attributes: string | null,
  isOpenForVolunteers: boolean,
  maxVolunteerSlots: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-scheduled-event-detail',
  templateUrl: './scheduled-event-detail.component.html',
  styleUrls: ['./scheduled-event-detail.component.scss']
})

export class ScheduledEventDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventForm: FormGroup = this.fb.group({
        officeId: [null],
        clientId: [null],
        scheduledEventTemplateId: [null],
        recurrenceRuleId: [null],
        schedulingTargetId: [null],
        timeZoneId: [null],
        parentScheduledEventId: [null],
        recurrenceInstanceDate: [''],
        name: ['', Validators.required],
        description: [''],
        isAllDay: [false],
        startDateTime: ['', Validators.required],
        endDateTime: ['', Validators.required],
        location: [''],
        eventStatusId: [null, Validators.required],
        resourceId: [null],
        crewId: [null],
        priorityId: [null],
        bookingSourceTypeId: [null],
        partySize: [''],
        bookingContactName: [''],
        bookingContactEmail: [''],
        bookingContactPhone: [''],
        notes: [''],
        color: [''],
        externalId: [''],
        attributes: [''],
        isOpenForVolunteers: [false],
        maxVolunteerSlots: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public scheduledEventId: string | null = null;
  public scheduledEventData: ScheduledEventData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public offices$ = this.officeService.GetOfficeList();
  public clients$ = this.clientService.GetClientList();
  public scheduledEventTemplates$ = this.scheduledEventTemplateService.GetScheduledEventTemplateList();
  public recurrenceRules$ = this.recurrenceRuleService.GetRecurrenceRuleList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public timeZones$ = this.timeZoneService.GetTimeZoneList();
  public eventStatuses$ = this.eventStatusService.GetEventStatusList();
  public resources$ = this.resourceService.GetResourceList();
  public crews$ = this.crewService.GetCrewList();
  public priorities$ = this.priorityService.GetPriorityList();
  public bookingSourceTypes$ = this.bookingSourceTypeService.GetBookingSourceTypeList();
  public scheduledEventChangeHistories$ = this.scheduledEventChangeHistoryService.GetScheduledEventChangeHistoryList();
  public eventCharges$ = this.eventChargeService.GetEventChargeList();
  public financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  public paymentTransactions$ = this.paymentTransactionService.GetPaymentTransactionList();
  public invoices$ = this.invoiceService.GetInvoiceList();
  public documents$ = this.documentService.GetDocumentList();
  public contactInteractions$ = this.contactInteractionService.GetContactInteractionList();
  public eventCalendars$ = this.eventCalendarService.GetEventCalendarList();
  public scheduledEventDependencies$ = this.scheduledEventDependencyService.GetScheduledEventDependencyList();
  public scheduledEventQualificationRequirements$ = this.scheduledEventQualificationRequirementService.GetScheduledEventQualificationRequirementList();
  public recurrenceExceptions$ = this.recurrenceExceptionService.GetRecurrenceExceptionList();
  public eventResourceAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentList();

  private destroy$ = new Subject<void>();

  constructor(
    public scheduledEventService: ScheduledEventService,
    public officeService: OfficeService,
    public clientService: ClientService,
    public scheduledEventTemplateService: ScheduledEventTemplateService,
    public recurrenceRuleService: RecurrenceRuleService,
    public schedulingTargetService: SchedulingTargetService,
    public timeZoneService: TimeZoneService,
    public eventStatusService: EventStatusService,
    public resourceService: ResourceService,
    public crewService: CrewService,
    public priorityService: PriorityService,
    public bookingSourceTypeService: BookingSourceTypeService,
    public scheduledEventChangeHistoryService: ScheduledEventChangeHistoryService,
    public eventChargeService: EventChargeService,
    public financialTransactionService: FinancialTransactionService,
    public paymentTransactionService: PaymentTransactionService,
    public invoiceService: InvoiceService,
    public documentService: DocumentService,
    public contactInteractionService: ContactInteractionService,
    public eventCalendarService: EventCalendarService,
    public scheduledEventDependencyService: ScheduledEventDependencyService,
    public scheduledEventQualificationRequirementService: ScheduledEventQualificationRequirementService,
    public recurrenceExceptionService: RecurrenceExceptionService,
    public eventResourceAssignmentService: EventResourceAssignmentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the scheduledEventId from the route parameters
    this.scheduledEventId = this.route.snapshot.paramMap.get('scheduledEventId');

    if (this.scheduledEventId === 'new' ||
        this.scheduledEventId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.scheduledEventData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.scheduledEventForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduled Event';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduled Event';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.scheduledEventForm.dirty) {
      return confirm('You have unsaved Scheduled Event changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.scheduledEventId != null && this.scheduledEventId !== 'new') {

      const id = parseInt(this.scheduledEventId, 10);

      if (!isNaN(id)) {
        return { scheduledEventId: id };
      }
    }

    return null;
  }


/*
  * Loads the ScheduledEvent data for the current scheduledEventId.
  *
  * Fully respects the ScheduledEventService caching strategy and error handling strategy.
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
    if (!this.scheduledEventService.userIsSchedulerScheduledEventReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ScheduledEvents.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate scheduledEventId
    //
    if (!this.scheduledEventId) {

      this.alertService.showMessage('No ScheduledEvent ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const scheduledEventId = Number(this.scheduledEventId);

    if (isNaN(scheduledEventId) || scheduledEventId <= 0) {

      this.alertService.showMessage(`Invalid Scheduled Event ID: "${this.scheduledEventId}"`,
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
      // This is the most targeted way: clear only this ScheduledEvent + relations

      this.scheduledEventService.ClearRecordCache(scheduledEventId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.scheduledEventService.GetScheduledEvent(scheduledEventId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (scheduledEventData) => {

        //
        // Success path — scheduledEventData can legitimately be null if 404'd but request succeeded
        //
        if (!scheduledEventData) {

          this.handleScheduledEventNotFound(scheduledEventId);

        } else {

          this.scheduledEventData = scheduledEventData;
          this.buildFormValues(this.scheduledEventData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ScheduledEvent loaded successfully',
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
        this.handleScheduledEventLoadError(error, scheduledEventId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleScheduledEventNotFound(scheduledEventId: number): void {

    this.scheduledEventData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ScheduledEvent #${scheduledEventId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleScheduledEventLoadError(error: any, scheduledEventId: number): void {

    let message = 'Failed to load Scheduled Event.';
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
          message = 'You do not have permission to view this Scheduled Event.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduled Event #${scheduledEventId} was not found.`;
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

    console.error(`Scheduled Event load failed (ID: ${scheduledEventId})`, error);

    //
    // Reset UI to safe state
    //
    this.scheduledEventData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(scheduledEventData: ScheduledEventData | null) {

    if (scheduledEventData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventForm.reset({
        officeId: null,
        clientId: null,
        scheduledEventTemplateId: null,
        recurrenceRuleId: null,
        schedulingTargetId: null,
        timeZoneId: null,
        parentScheduledEventId: null,
        recurrenceInstanceDate: '',
        name: '',
        description: '',
        isAllDay: false,
        startDateTime: '',
        endDateTime: '',
        location: '',
        eventStatusId: null,
        resourceId: null,
        crewId: null,
        priorityId: null,
        bookingSourceTypeId: null,
        partySize: '',
        bookingContactName: '',
        bookingContactEmail: '',
        bookingContactPhone: '',
        notes: '',
        color: '',
        externalId: '',
        attributes: '',
        isOpenForVolunteers: false,
        maxVolunteerSlots: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduledEventForm.reset({
        officeId: scheduledEventData.officeId,
        clientId: scheduledEventData.clientId,
        scheduledEventTemplateId: scheduledEventData.scheduledEventTemplateId,
        recurrenceRuleId: scheduledEventData.recurrenceRuleId,
        schedulingTargetId: scheduledEventData.schedulingTargetId,
        timeZoneId: scheduledEventData.timeZoneId,
        parentScheduledEventId: scheduledEventData.parentScheduledEventId,
        recurrenceInstanceDate: isoUtcStringToDateTimeLocal(scheduledEventData.recurrenceInstanceDate) ?? '',
        name: scheduledEventData.name ?? '',
        description: scheduledEventData.description ?? '',
        isAllDay: scheduledEventData.isAllDay ?? false,
        startDateTime: isoUtcStringToDateTimeLocal(scheduledEventData.startDateTime) ?? '',
        endDateTime: isoUtcStringToDateTimeLocal(scheduledEventData.endDateTime) ?? '',
        location: scheduledEventData.location ?? '',
        eventStatusId: scheduledEventData.eventStatusId,
        resourceId: scheduledEventData.resourceId,
        crewId: scheduledEventData.crewId,
        priorityId: scheduledEventData.priorityId,
        bookingSourceTypeId: scheduledEventData.bookingSourceTypeId,
        partySize: scheduledEventData.partySize?.toString() ?? '',
        bookingContactName: scheduledEventData.bookingContactName ?? '',
        bookingContactEmail: scheduledEventData.bookingContactEmail ?? '',
        bookingContactPhone: scheduledEventData.bookingContactPhone ?? '',
        notes: scheduledEventData.notes ?? '',
        color: scheduledEventData.color ?? '',
        externalId: scheduledEventData.externalId ?? '',
        attributes: scheduledEventData.attributes ?? '',
        isOpenForVolunteers: scheduledEventData.isOpenForVolunteers ?? false,
        maxVolunteerSlots: scheduledEventData.maxVolunteerSlots?.toString() ?? '',
        versionNumber: scheduledEventData.versionNumber?.toString() ?? '',
        active: scheduledEventData.active ?? true,
        deleted: scheduledEventData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduledEventForm.markAsPristine();
    this.scheduledEventForm.markAsUntouched();
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

    if (this.scheduledEventService.userIsSchedulerScheduledEventWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduled Events", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.scheduledEventForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventSubmitData: ScheduledEventSubmitData = {
        id: this.scheduledEventData?.id || 0,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        clientId: formValue.clientId ? Number(formValue.clientId) : null,
        scheduledEventTemplateId: formValue.scheduledEventTemplateId ? Number(formValue.scheduledEventTemplateId) : null,
        recurrenceRuleId: formValue.recurrenceRuleId ? Number(formValue.recurrenceRuleId) : null,
        schedulingTargetId: formValue.schedulingTargetId ? Number(formValue.schedulingTargetId) : null,
        timeZoneId: formValue.timeZoneId ? Number(formValue.timeZoneId) : null,
        parentScheduledEventId: formValue.parentScheduledEventId ? Number(formValue.parentScheduledEventId) : null,
        recurrenceInstanceDate: formValue.recurrenceInstanceDate ? dateTimeLocalToIsoUtc(formValue.recurrenceInstanceDate.trim()) : null,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        isAllDay: formValue.isAllDay == true ? true : formValue.isAllDay == false ? false : null,
        startDateTime: dateTimeLocalToIsoUtc(formValue.startDateTime!.trim())!,
        endDateTime: dateTimeLocalToIsoUtc(formValue.endDateTime!.trim())!,
        location: formValue.location?.trim() || null,
        eventStatusId: Number(formValue.eventStatusId),
        resourceId: formValue.resourceId ? Number(formValue.resourceId) : null,
        crewId: formValue.crewId ? Number(formValue.crewId) : null,
        priorityId: formValue.priorityId ? Number(formValue.priorityId) : null,
        bookingSourceTypeId: formValue.bookingSourceTypeId ? Number(formValue.bookingSourceTypeId) : null,
        partySize: formValue.partySize ? Number(formValue.partySize) : null,
        bookingContactName: formValue.bookingContactName?.trim() || null,
        bookingContactEmail: formValue.bookingContactEmail?.trim() || null,
        bookingContactPhone: formValue.bookingContactPhone?.trim() || null,
        notes: formValue.notes?.trim() || null,
        color: formValue.color?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        attributes: formValue.attributes?.trim() || null,
        isOpenForVolunteers: !!formValue.isOpenForVolunteers,
        maxVolunteerSlots: formValue.maxVolunteerSlots ? Number(formValue.maxVolunteerSlots) : null,
        versionNumber: this.scheduledEventData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.scheduledEventService.PutScheduledEvent(scheduledEventSubmitData.id, scheduledEventSubmitData)
      : this.scheduledEventService.PostScheduledEvent(scheduledEventSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedScheduledEventData) => {

        this.scheduledEventService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduled Event's detail page
          //
          this.scheduledEventForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.scheduledEventForm.markAsUntouched();

          this.router.navigate(['/scheduledevents', savedScheduledEventData.id]);
          this.alertService.showMessage('Scheduled Event added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.scheduledEventData = savedScheduledEventData;
          this.buildFormValues(this.scheduledEventData);

          this.alertService.showMessage("Scheduled Event saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduled Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerScheduledEventReader(): boolean {
    return this.scheduledEventService.userIsSchedulerScheduledEventReader();
  }

  public userIsSchedulerScheduledEventWriter(): boolean {
    return this.scheduledEventService.userIsSchedulerScheduledEventWriter();
  }
}
