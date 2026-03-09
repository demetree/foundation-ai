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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventService, ScheduledEventData, ScheduledEventSubmitData } from '../../../scheduler-data-services/scheduled-event.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
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
import { AuthService } from '../../../services/auth.service';

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
  selector: 'app-scheduled-event-add-edit',
  templateUrl: './scheduled-event-add-edit.component.html',
  styleUrls: ['./scheduled-event-add-edit.component.scss']
})
export class ScheduledEventAddEditComponent {
  @ViewChild('scheduledEventModal') scheduledEventModal!: TemplateRef<any>;
  @Output() scheduledEventChanged = new Subject<ScheduledEventData[]>();
  @Input() scheduledEventSubmitData: ScheduledEventSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  offices$ = this.officeService.GetOfficeList();
  clients$ = this.clientService.GetClientList();
  scheduledEventTemplates$ = this.scheduledEventTemplateService.GetScheduledEventTemplateList();
  recurrenceRules$ = this.recurrenceRuleService.GetRecurrenceRuleList();
  schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  timeZones$ = this.timeZoneService.GetTimeZoneList();
  eventStatuses$ = this.eventStatusService.GetEventStatusList();
  resources$ = this.resourceService.GetResourceList();
  crews$ = this.crewService.GetCrewList();
  priorities$ = this.priorityService.GetPriorityList();
  bookingSourceTypes$ = this.bookingSourceTypeService.GetBookingSourceTypeList();

  constructor(
    private modalService: NgbModal,
    private scheduledEventService: ScheduledEventService,
    private officeService: OfficeService,
    private clientService: ClientService,
    private scheduledEventTemplateService: ScheduledEventTemplateService,
    private recurrenceRuleService: RecurrenceRuleService,
    private schedulingTargetService: SchedulingTargetService,
    private timeZoneService: TimeZoneService,
    private eventStatusService: EventStatusService,
    private resourceService: ResourceService,
    private crewService: CrewService,
    private priorityService: PriorityService,
    private bookingSourceTypeService: BookingSourceTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(scheduledEventData?: ScheduledEventData) {

    if (scheduledEventData != null) {

      if (!this.scheduledEventService.userIsSchedulerScheduledEventReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduled Events`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.scheduledEventSubmitData = this.scheduledEventService.ConvertToScheduledEventSubmitData(scheduledEventData);
      this.isEditMode = true;
      this.objectGuid = scheduledEventData.objectGuid;

      this.buildFormValues(scheduledEventData);

    } else {

      if (!this.scheduledEventService.userIsSchedulerScheduledEventWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduled Events`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.scheduledEventForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.scheduledEventModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
  }


  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.scheduledEventService.userIsSchedulerScheduledEventWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduled Events`,
        '',
        MessageSeverity.info
      );
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
        id: this.scheduledEventSubmitData?.id || 0,
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
        notes: formValue.notes?.trim() || null,
        color: formValue.color?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        attributes: formValue.attributes?.trim() || null,
        isOpenForVolunteers: !!formValue.isOpenForVolunteers,
        maxVolunteerSlots: formValue.maxVolunteerSlots ? Number(formValue.maxVolunteerSlots) : null,
        versionNumber: this.scheduledEventSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateScheduledEvent(scheduledEventSubmitData);
      } else {
        this.addScheduledEvent(scheduledEventSubmitData);
      }
  }

  private addScheduledEvent(scheduledEventData: ScheduledEventSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    scheduledEventData.versionNumber = 0;
    scheduledEventData.active = true;
    scheduledEventData.deleted = false;
    this.scheduledEventService.PostScheduledEvent(scheduledEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newScheduledEvent) => {

        this.scheduledEventService.ClearAllCaches();

        this.scheduledEventChanged.next([newScheduledEvent]);

        this.alertService.showMessage("Scheduled Event added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/scheduledevent', newScheduledEvent.id]);
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


  private updateScheduledEvent(scheduledEventData: ScheduledEventSubmitData) {
    this.scheduledEventService.PutScheduledEvent(scheduledEventData.id, scheduledEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedScheduledEvent) => {

        this.scheduledEventService.ClearAllCaches();

        this.scheduledEventChanged.next([updatedScheduledEvent]);

        this.alertService.showMessage("Scheduled Event updated successfully", '', MessageSeverity.success);

        this.closeModal();
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


  public userIsSchedulerScheduledEventReader(): boolean {
    return this.scheduledEventService.userIsSchedulerScheduledEventReader();
  }

  public userIsSchedulerScheduledEventWriter(): boolean {
    return this.scheduledEventService.userIsSchedulerScheduledEventWriter();
  }
}
