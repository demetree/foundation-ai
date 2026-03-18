/**
 * CommitteeEventBookingComponent
 *
 * AI-Developed — Purpose-built booking flow for committee-organized events.
 *
 * Streamlined form for rec committee to schedule:
 *   - Community events, fundraisers, bingo nights
 *   - Optional ticket sales
 *   - Optional bar service override
 */

import { Component, OnInit, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { lastValueFrom } from 'rxjs';
import { EventTypeService, EventTypeData } from '../../../scheduler-data-services/event-type.service';
import { ScheduledEventService, ScheduledEventSubmitData } from '../../../scheduler-data-services/scheduled-event.service';
import { EventStatusService, EventStatusData } from '../../../scheduler-data-services/event-status.service';
import { CalendarService, CalendarData } from '../../../scheduler-data-services/calendar.service';
import { EventCalendarService, EventCalendarSubmitData } from '../../../scheduler-data-services/event-calendar.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-committee-event-booking',
  templateUrl: './committee-event-booking.component.html',
  styleUrls: ['./committee-event-booking.component.scss']
})
export class CommitteeEventBookingComponent implements OnInit {

  // -----------------------------------------------------------------------
  // Inputs
  // -----------------------------------------------------------------------
  @Input() initialStart: string | null = null;
  @Input() initialEnd: string | null = null;

  // -----------------------------------------------------------------------
  // State
  // -----------------------------------------------------------------------
  eventForm!: FormGroup;
  saving = false;

  // Lookup data
  eventTypes: EventTypeData[] = [];
  statuses: EventStatusData[] = [];
  calendars: CalendarData[] = [];
  selectedEventType: EventTypeData | null = null;

  // UI toggles
  needsBarService = false;
  enableTicketSales = false;


  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private eventTypeService: EventTypeService,
    private scheduledEventService: ScheduledEventService,
    private eventStatusService: EventStatusService,
    private calendarService: CalendarService,
    private eventCalendarService: EventCalendarService,
    private alertService: AlertService
  ) {}


  ngOnInit(): void {
    this.buildForm();
    this.loadLookups();
  }


  private buildForm(): void {
    let startDate = '';
    let startTime = '10:00';
    let endTime = '14:00';

    if (this.initialStart) {
      const d = new Date(this.initialStart);
      startDate = d.toISOString().split('T')[0];
      if (d.getHours() !== 0 || d.getMinutes() !== 0) {
        startTime = d.toTimeString().substring(0, 5);
      }
    }
    if (this.initialEnd) {
      const d = new Date(this.initialEnd);
      if (d.getHours() !== 0 || d.getMinutes() !== 0) {
        endTime = d.toTimeString().substring(0, 5);
      }
    }

    this.eventForm = this.fb.group({
      eventTypeId: [null, Validators.required],
      eventName: ['', Validators.required],
      eventDate: [startDate, Validators.required],
      startTime: [startTime, Validators.required],
      endTime: [endTime, Validators.required],
      description: [''],
      notes: [''],

      // Optional ticket sales
      ticketPrice: [null],
      barNotes: ['']
    });
  }


  private async loadLookups(): Promise<void> {
    try {
      const allTypes = await lastValueFrom(
        this.eventTypeService.GetEventTypeList({ active: true, deleted: false })
      );
      // Only internal event types
      this.eventTypes = allTypes.filter(et => et.isInternalEvent);
    } catch (err) {
      console.error('Failed to load event types', err);
    }

    try {
      this.statuses = await lastValueFrom(
        this.eventStatusService.GetEventStatusList({ active: true, deleted: false })
      );
    } catch (err) {
      console.error('Failed to load event statuses', err);
    }

    try {
      this.calendars = await lastValueFrom(
        this.calendarService.GetCalendarList({ active: true, deleted: false })
      );
    } catch (err) {
      console.error('Failed to load calendars', err);
    }
  }


  onEventTypeChange(): void {
    const typeId = this.eventForm.get('eventTypeId')?.value;
    this.selectedEventType = this.eventTypes.find(et => et.id == typeId) || null;

    if (this.selectedEventType) {
      const currentName = this.eventForm.get('eventName')?.value;
      if (!currentName) {
        this.eventForm.patchValue({ eventName: this.selectedEventType.name });
      }

      this.enableTicketSales = this.selectedEventType.allowsTicketSales;
      this.needsBarService = this.selectedEventType.requiresBarService;
    }
  }


  async save(): Promise<void> {
    if (this.eventForm.invalid) {
      this.eventForm.markAllAsTouched();
      return;
    }

    this.saving = true;

    try {
      const form = this.eventForm.value;

      const startDT = new Date(`${form.eventDate}T${form.startTime}:00`).toISOString();
      const endDT = new Date(`${form.eventDate}T${form.endTime}:00`).toISOString();

      const confirmedStatus = this.statuses.find(s =>
        s.name.toLowerCase().includes('confirmed') || s.name.toLowerCase().includes('scheduled')
      ) || this.statuses[0];

      //
      // Build attributes JSON
      //
      const attributes: any = {};
      if (this.needsBarService) {
        attributes.barService = { needed: true, notes: form.barNotes || '' };
      }
      if (this.enableTicketSales && form.ticketPrice) {
        attributes.ticketSales = { enabled: true, price: form.ticketPrice };
      }

      const eventSubmit = new ScheduledEventSubmitData();
      eventSubmit.name = form.eventName;
      eventSubmit.description = form.description || this.selectedEventType?.description || '';
      eventSubmit.startDateTime = startDT;
      eventSubmit.endDateTime = endDT;
      eventSubmit.isAllDay = false;
      eventSubmit.eventStatusId = confirmedStatus?.id as number;
      eventSubmit.eventTypeId = form.eventTypeId;
      eventSubmit.notes = form.notes || null;
      eventSubmit.attributes = Object.keys(attributes).length > 0 ? JSON.stringify(attributes) : null;
      eventSubmit.active = true;
      eventSubmit.deleted = false;

      const savedEvent = await lastValueFrom(
        this.scheduledEventService.PostScheduledEvent(eventSubmit)
      );

      //
      // Link to Recreation calendar
      //
      const recCalendar = this.calendars.find(c =>
        c.name.toLowerCase().includes('recreation') || c.name.toLowerCase().includes('rec')
      );

      if (recCalendar) {
        const calendarLink = new EventCalendarSubmitData();
        calendarLink.scheduledEventId = savedEvent.id as number;
        calendarLink.calendarId = recCalendar.id as number;
        calendarLink.active = true;
        calendarLink.deleted = false;

        await lastValueFrom(
          this.eventCalendarService.PostEventCalendar(calendarLink)
        );
      }

      this.alertService.showMessage('Event Scheduled',
        `${form.eventName} has been scheduled for ${form.eventDate}`,
        MessageSeverity.success);

      this.activeModal.close(true);

    } catch (err) {
      console.error('Failed to save event', err);
      this.alertService.showMessage('Error', 'Failed to schedule event. Please try again.', MessageSeverity.error);
    } finally {
      this.saving = false;
    }
  }
}
