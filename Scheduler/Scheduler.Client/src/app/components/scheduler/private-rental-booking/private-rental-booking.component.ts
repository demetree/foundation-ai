/**
 * PrivateRentalBookingComponent
 *
 * AI-Developed — Purpose-built booking flow for private facility rentals.
 *
 * Streamlined wizard-style modal for PHMC rec committee to book:
 *   - Birthday parties, weddings, bridal showers, private events
 *   - Auto-populates fields from the selected EventType
 *   - Handles payment, deposit, rental agreement, bar service
 *   - Client special requests free-form text
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
import { EventChargeService, EventChargeData } from '../../../scheduler-data-services/event-charge.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-private-rental-booking',
  templateUrl: './private-rental-booking.component.html',
  styleUrls: ['./private-rental-booking.component.scss']
})
export class PrivateRentalBookingComponent implements OnInit {

  // -----------------------------------------------------------------------
  // Inputs
  // -----------------------------------------------------------------------
  @Input() initialStart: string | null = null;
  @Input() initialEnd: string | null = null;

  // -----------------------------------------------------------------------
  // State
  // -----------------------------------------------------------------------
  bookingForm!: FormGroup;
  saving = false;

  // Lookup data
  eventTypes: EventTypeData[] = [];
  statuses: EventStatusData[] = [];
  calendars: CalendarData[] = [];
  selectedEventType: EventTypeData | null = null;

  // UI toggles driven by EventType flags (with per-event overrides)
  needsBarService = false;
  rentalAgreementSigned = false;
  markAsPaid = false;


  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private eventTypeService: EventTypeService,
    private scheduledEventService: ScheduledEventService,
    private eventStatusService: EventStatusService,
    private calendarService: CalendarService,
    private eventCalendarService: EventCalendarService,
    private eventChargeService: EventChargeService,
    private alertService: AlertService
  ) {}


  ngOnInit(): void {
    this.buildForm();
    this.loadLookups();
  }


  // -----------------------------------------------------------------------
  // Form
  // -----------------------------------------------------------------------

  private buildForm(): void {
    //
    // Parse the initial date/time values
    //
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

    this.bookingForm = this.fb.group({
      // Event
      eventTypeId: [null, Validators.required],
      eventName: ['', Validators.required],
      eventDate: [startDate, Validators.required],
      startTime: [startTime, Validators.required],
      endTime: [endTime, Validators.required],

      // Contact
      contactName: ['', Validators.required],
      contactEmail: [''],
      contactPhone: [''],
      partySize: [null],

      // Special Requests
      specialRequests: [''],

      // Payment
      price: [null],

      // Bar Service notes
      barNotes: ['']
    });
  }


  private async loadLookups(): Promise<void> {
    //
    // Load event types — private rentals only (isInternalEvent = false)
    //
    try {
      const allTypes = await lastValueFrom(
        this.eventTypeService.GetEventTypeList({ active: true, deleted: false })
      );
      this.eventTypes = allTypes.filter(et => !et.isInternalEvent);
    } catch (err) {
      console.error('Failed to load event types', err);
    }

    //
    // Load statuses
    //
    try {
      this.statuses = await lastValueFrom(
        this.eventStatusService.GetEventStatusList({ active: true, deleted: false })
      );
    } catch (err) {
      console.error('Failed to load event statuses', err);
    }

    //
    // Load calendars
    //
    try {
      this.calendars = await lastValueFrom(
        this.calendarService.GetCalendarList({ active: true, deleted: false })
      );
    } catch (err) {
      console.error('Failed to load calendars', err);
    }
  }


  // -----------------------------------------------------------------------
  // Event Type Selection
  // -----------------------------------------------------------------------

  onEventTypeChange(): void {
    const typeId = this.bookingForm.get('eventTypeId')?.value;
    this.selectedEventType = this.eventTypes.find(et => et.id == typeId) || null;

    if (this.selectedEventType) {
      //
      // Auto-populate event name from type
      //
      const currentName = this.bookingForm.get('eventName')?.value;
      if (!currentName) {
        this.bookingForm.patchValue({ eventName: this.selectedEventType.name });
      }

      //
      // Auto-populate price from default
      //
      if (this.selectedEventType.defaultPrice != null) {
        this.bookingForm.patchValue({ price: this.selectedEventType.defaultPrice });
      }

      //
      // Set bar service default from flag
      //
      this.needsBarService = this.selectedEventType.requiresBarService;
    }
  }


  // -----------------------------------------------------------------------
  // Save
  // -----------------------------------------------------------------------

  async save(): Promise<void> {
    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    this.saving = true;

    try {
      const form = this.bookingForm.value;

      //
      // 1. Build the ScheduledEvent
      //
      const startDT = new Date(`${form.eventDate}T${form.startTime}:00`).toISOString();
      const endDT = new Date(`${form.eventDate}T${form.endTime}:00`).toISOString();

      //
      // Determine status — find "Confirmed" or first available
      //
      const confirmedStatus = this.statuses.find(s =>
        s.name.toLowerCase().includes('confirmed') || s.name.toLowerCase().includes('scheduled')
      ) || this.statuses[0];

      //
      // Build attributes JSON for rental agreement, bar service, special requests
      //
      const attributes: any = {};
      if (this.rentalAgreementSigned) {
        attributes.rentalAgreement = { signed: true, signedDate: new Date().toISOString() };
      }
      if (this.needsBarService) {
        attributes.barService = { needed: true, notes: form.barNotes || '' };
      }
      if (form.specialRequests) {
        attributes.specialRequests = form.specialRequests;
      }

      const eventSubmit = new ScheduledEventSubmitData();
      eventSubmit.name = form.eventName;
      eventSubmit.description = this.selectedEventType?.description || '';
      eventSubmit.startDateTime = startDT;
      eventSubmit.endDateTime = endDT;
      eventSubmit.isAllDay = false;
      eventSubmit.eventStatusId = confirmedStatus?.id as number;
      eventSubmit.eventTypeId = form.eventTypeId;
      eventSubmit.bookingContactName = form.contactName;
      eventSubmit.bookingContactEmail = form.contactEmail || null;
      eventSubmit.bookingContactPhone = form.contactPhone || null;
      eventSubmit.partySize = form.partySize || null;
      eventSubmit.notes = form.specialRequests || null;
      eventSubmit.attributes = Object.keys(attributes).length > 0 ? JSON.stringify(attributes) : null;
      eventSubmit.active = true;
      eventSubmit.deleted = false;

      //
      // Post the event
      //
      const savedEvent = await lastValueFrom(
        this.scheduledEventService.PostScheduledEvent(eventSubmit)
      );

      //
      // 2. Link to the Recreation calendar (if it exists)
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

      //
      // 3. Create EventCharge if payment is needed
      //
      if (this.selectedEventType?.requiresPayment && form.price > 0) {
        const chargeSubmit: any = {
          scheduledEventId: savedEvent.id,
          chargeTypeId: this.selectedEventType.chargeTypeId,
          amount: form.price,
          description: `${this.selectedEventType.name} rental fee`,
          isDeposit: false,
          isPaid: this.markAsPaid,
          paidDate: this.markAsPaid ? new Date().toISOString() : null,
          active: true,
          deleted: false
        };

        await lastValueFrom(
          this.eventChargeService.PostEventCharge(chargeSubmit)
        );

        //
        // 3b. Create deposit charge if needed
        //
        if (this.selectedEventType?.requiresDeposit) {
          const depositSubmit: any = {
            scheduledEventId: savedEvent.id,
            chargeTypeId: this.selectedEventType.chargeTypeId,
            amount: Math.round(form.price * 0.5),  // 50% deposit
            description: `${this.selectedEventType.name} deposit`,
            isDeposit: true,
            isPaid: false,
            active: true,
            deleted: false
          };

          await lastValueFrom(
            this.eventChargeService.PostEventCharge(depositSubmit)
          );
        }
      }

      this.alertService.showMessage('Booking Created',
        `${form.eventName} has been booked for ${form.eventDate}`,
        MessageSeverity.success);

      this.activeModal.close(true);

    } catch (err) {
      console.error('Failed to save booking', err);
      this.alertService.showMessage('Error', 'Failed to create booking. Please try again.', MessageSeverity.error);
    } finally {
      this.saving = false;
    }
  }
}
