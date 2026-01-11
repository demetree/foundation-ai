/*
   GENERATED FORM FOR THE EVENTCALENDAR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EventCalendar table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to event-calendar-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EventCalendarService, EventCalendarData, EventCalendarSubmitData } from '../../../scheduler-data-services/event-calendar.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface EventCalendarFormValues {
  scheduledEventId: number | bigint,       // For FK link number
  calendarId: number | bigint,       // For FK link number
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-event-calendar-add-edit',
  templateUrl: './event-calendar-add-edit.component.html',
  styleUrls: ['./event-calendar-add-edit.component.scss']
})
export class EventCalendarAddEditComponent {
  @ViewChild('eventCalendarModal') eventCalendarModal!: TemplateRef<any>;
  @Output() eventCalendarChanged = new Subject<EventCalendarData[]>();
  @Input() eventCalendarSubmitData: EventCalendarSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EventCalendarFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public eventCalendarForm: FormGroup = this.fb.group({
        scheduledEventId: [null, Validators.required],
        calendarId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  eventCalendars$ = this.eventCalendarService.GetEventCalendarList();
  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  calendars$ = this.calendarService.GetCalendarList();

  constructor(
    private modalService: NgbModal,
    private eventCalendarService: EventCalendarService,
    private scheduledEventService: ScheduledEventService,
    private calendarService: CalendarService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(eventCalendarData?: EventCalendarData) {

    if (eventCalendarData != null) {

      if (!this.eventCalendarService.userIsSchedulerEventCalendarReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Event Calendars`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.eventCalendarSubmitData = this.eventCalendarService.ConvertToEventCalendarSubmitData(eventCalendarData);
      this.isEditMode = true;
      this.objectGuid = eventCalendarData.objectGuid;

      this.buildFormValues(eventCalendarData);

    } else {

      if (!this.eventCalendarService.userIsSchedulerEventCalendarWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Event Calendars`,
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
        this.eventCalendarForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.eventCalendarForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.eventCalendarModal, {
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

    if (this.eventCalendarService.userIsSchedulerEventCalendarWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Event Calendars`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.eventCalendarForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.eventCalendarForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.eventCalendarForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const eventCalendarSubmitData: EventCalendarSubmitData = {
        id: this.eventCalendarSubmitData?.id || 0,
        scheduledEventId: Number(formValue.scheduledEventId),
        calendarId: Number(formValue.calendarId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateEventCalendar(eventCalendarSubmitData);
      } else {
        this.addEventCalendar(eventCalendarSubmitData);
      }
  }

  private addEventCalendar(eventCalendarData: EventCalendarSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    eventCalendarData.active = true;
    eventCalendarData.deleted = false;
    this.eventCalendarService.PostEventCalendar(eventCalendarData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newEventCalendar) => {

        this.eventCalendarService.ClearAllCaches();

        this.eventCalendarChanged.next([newEventCalendar]);

        this.alertService.showMessage("Event Calendar added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/eventcalendar', newEventCalendar.id]);
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
                                   'You do not have permission to save this Event Calendar.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Calendar.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Calendar could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateEventCalendar(eventCalendarData: EventCalendarSubmitData) {
    this.eventCalendarService.PutEventCalendar(eventCalendarData.id, eventCalendarData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedEventCalendar) => {

        this.eventCalendarService.ClearAllCaches();

        this.eventCalendarChanged.next([updatedEventCalendar]);

        this.alertService.showMessage("Event Calendar updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Event Calendar.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Event Calendar.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Event Calendar could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(eventCalendarData: EventCalendarData | null) {

    if (eventCalendarData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.eventCalendarForm.reset({
        scheduledEventId: null,
        calendarId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.eventCalendarForm.reset({
        scheduledEventId: eventCalendarData.scheduledEventId,
        calendarId: eventCalendarData.calendarId,
        active: eventCalendarData.active ?? true,
        deleted: eventCalendarData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.eventCalendarForm.markAsPristine();
    this.eventCalendarForm.markAsUntouched();
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


  public userIsSchedulerEventCalendarReader(): boolean {
    return this.eventCalendarService.userIsSchedulerEventCalendarReader();
  }

  public userIsSchedulerEventCalendarWriter(): boolean {
    return this.eventCalendarService.userIsSchedulerEventCalendarWriter();
  }
}
