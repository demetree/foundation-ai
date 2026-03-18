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
 *   - Facility resource selection (auto-selected when only one)
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
import { ResourceTypeService, ResourceTypeData } from '../../../scheduler-data-services/resource-type.service';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';
import { DocumentService, DocumentSubmitData } from '../../../scheduler-data-services/document.service';
import { DocumentTypeService, DocumentTypeData } from '../../../scheduler-data-services/document-type.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';

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

  // Facility resource picker
  facilityResources: ResourceData[] = [];
  selectedFacilityId: number | bigint | null = null;
  facilityLoadWarning: string | null = null;  // Warning shown when no facility resources exist

  // UI toggles driven by EventType flags (with per-event overrides)
  needsBarService = false;
  rentalAgreementSigned = false;
  markAsPaid = false;

  // Optional rental agreement file attachment
  pendingAgreementFile: File | null = null;
  pendingAgreementBase64: string | null = null;
  documentTypes: DocumentTypeData[] = [];


  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private eventTypeService: EventTypeService,
    private scheduledEventService: ScheduledEventService,
    private eventStatusService: EventStatusService,
    private calendarService: CalendarService,
    private eventCalendarService: EventCalendarService,
    private eventChargeService: EventChargeService,
    private resourceTypeService: ResourceTypeService,
    private resourceService: ResourceService,
    private documentService: DocumentService,
    private documentTypeService: DocumentTypeService,
    private alertService: AlertService,
    private authService: AuthService
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

    //
    // Load facility resources — find the "Facility" ResourceType, then get its resources
    //
    await this.loadFacilityResources();

    //
    // Load document types (for optional rental agreement attachment)
    //
    try {
      this.documentTypes = await lastValueFrom(
        this.documentTypeService.GetDocumentTypeList({ active: true, deleted: false })
      );
    } catch (err) {
      console.error('Failed to load document types', err);
    }
  }


  /**
   * Finds the "Facility" resource type and loads active resources of that type.
   * Auto-selects if there is exactly one. Shows a warning if there are none.
   */
  private async loadFacilityResources(): Promise<void> {
    try {
      const resourceTypes = await lastValueFrom(
        this.resourceTypeService.GetResourceTypeList({ active: true, deleted: false })
      );

      const facilityType = resourceTypes.find(rt =>
        rt.name.toLowerCase().includes('facility')
      );

      if (!facilityType) {
        this.facilityLoadWarning = 'No "Facility" resource type is configured. The booking will be created without a facility assignment.';
        return;
      }

      const resources = await lastValueFrom(
        this.resourceService.GetResourceList({ resourceTypeId: facilityType.id, active: true, deleted: false })
      );

      this.facilityResources = resources;

      if (resources.length === 0) {
        this.facilityLoadWarning = 'No facility resources are configured. The booking will be created without a facility assignment.';
      } else if (resources.length === 1) {
        // Auto-select the only facility
        this.selectedFacilityId = resources[0].id;
      }
      // If multiple, the user picks from the dropdown

    } catch (err) {
      console.error('Failed to load facility resources', err);
      this.facilityLoadWarning = 'Could not load facility resources.';
    }
  }



  // -----------------------------------------------------------------------
  // Rental Agreement File Handling
  // -----------------------------------------------------------------------

  onAgreementFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    const maxSize = 10 * 1024 * 1024; // 10MB

    if (file.size > maxSize) {
      this.alertService.showMessage('File Too Large', 'Maximum file size is 10MB.', MessageSeverity.warn);
      return;
    }

    this.pendingAgreementFile = file;

    // Read as base64
    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result as string;
      this.pendingAgreementBase64 = result.split(',')[1];
    };
    reader.readAsDataURL(file);
  }

  removeAgreementFile(): void {
    this.pendingAgreementFile = null;
    this.pendingAgreementBase64 = null;
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
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
      eventSubmit.resourceId = this.selectedFacilityId as number || null;
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

      //
      // 4. Upload rental agreement document if one was attached
      //
      if (this.pendingAgreementFile && this.pendingAgreementBase64) {
        try {
          const rentalAgreementType = this.documentTypes.find(dt =>
            dt.name.toLowerCase().includes('rental')
          );

          if (rentalAgreementType) {
            const docSubmit = new DocumentSubmitData();
            docSubmit.name = this.pendingAgreementFile.name.replace(/\.[^/.]+$/, '');
            docSubmit.description = 'Rental agreement attached during booking';
            docSubmit.fileName = this.pendingAgreementFile.name;
            docSubmit.mimeType = this.pendingAgreementFile.type || 'application/octet-stream';
            docSubmit.fileSizeBytes = this.pendingAgreementFile.size;
            docSubmit.fileDataFileName = this.pendingAgreementFile.name;
            docSubmit.fileDataSize = this.pendingAgreementFile.size;
            docSubmit.fileDataData = this.pendingAgreementBase64;
            docSubmit.fileDataMimeType = this.pendingAgreementFile.type || 'application/octet-stream';
            docSubmit.documentTypeId = rentalAgreementType.id as number;
            docSubmit.scheduledEventId = savedEvent.id as number;
            docSubmit.status = 'Uploaded';
            docSubmit.statusDate = new Date().toISOString();
            docSubmit.uploadedDate = new Date().toISOString();
            docSubmit.uploadedBy = this.authService.currentUser?.userName || 'Unknown';
            docSubmit.versionNumber = 0;
            docSubmit.active = true;
            docSubmit.deleted = false;

            await lastValueFrom(
              this.documentService.PostDocument(docSubmit)
            );
          }
        } catch (docErr) {
          console.warn('Failed to upload rental agreement document', docErr);
          // Don't fail the booking, just warn
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
