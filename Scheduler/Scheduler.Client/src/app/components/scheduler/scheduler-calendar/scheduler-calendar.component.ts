import { Component, OnInit, ViewChild } from '@angular/core';
import { FullCalendarComponent } from '@fullcalendar/angular';
import { CalendarOptions } from '@fullcalendar/core'; // useful for typechecking
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import { ScheduledEventService, ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { EventAddEditModalComponent } from '../event-add-edit-modal/event-add-edit-modal.component';
import { formatISO } from 'date-fns';

@Component({
  selector: 'app-scheduler-calendar',
  templateUrl: './scheduler-calendar.component.html',
  styleUrls: ['./scheduler-calendar.component.scss']
})
export class SchedulerCalendarComponent implements OnInit {
  @ViewChild('calendar') calendarComponent!: FullCalendarComponent;

  calendarOptions: CalendarOptions = {
    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
    initialView: 'timeGridWeek',
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'dayGridMonth,timeGridWeek,timeGridDay'
    },
    editable: true,
    selectable: true,
    selectMirror: true,
    dayMaxEvents: true,
    weekends: true,
    events: [], // Filled by loadEvents
    select: this.handleDateSelect.bind(this),
    eventClick: this.handleEventClick.bind(this),
    eventDrop: this.handleEventDrop.bind(this),
    eventResize: this.handleEventResize.bind(this),
    eventContent: this.renderEventContent.bind(this),
    datesSet: this.handleDatesSet.bind(this)
  };

  constructor(
    private scheduledEventService: ScheduledEventService,
    private modalService: NgbModal
  ) { }

  ngOnInit(): void {
    //
    // Initial load is triggered by FullCalendar's datesSet callback when the view renders.
    //
  }


  /**
   *
   * Called by FullCalendar whenever the visible date range changes (view switch, prev/next navigation, etc).
   *
   * Loads all events for the visible range using the server-side Calendar endpoint,
   * which returns both standalone events and expanded recurring event instances.
   *
   */
  handleDatesSet(dateInfo: any): void {
    this.loadEvents(dateInfo.startStr, dateInfo.endStr);
  }


  private loadEvents(rangeStart?: string, rangeEnd?: string): void {

    //
    // If no range provided, use a default window of 3 months back to 6 months forward
    //
    if (rangeStart == null || rangeEnd == null) {

      const today = new Date();
      const defaultStart = new Date(today.getFullYear(), today.getMonth() - 3, 1);
      const defaultEnd = new Date(today.getFullYear(), today.getMonth() + 6, 0);

      rangeStart = defaultStart.toISOString();
      rangeEnd = defaultEnd.toISOString();
    }

    this.scheduledEventService.GetCalendarEvents(rangeStart, rangeEnd).subscribe(events => {
      this.calendarOptions.events = events.map(event => ({
        id: event.id.toString(),
        title: event.name,
        start: event.startDateTime,
        end: event.endDateTime,
        extendedProps: {
          eventData: event,
          location: event.location,
          notes: event.notes
        },
        backgroundColor: this.getEventColor(event),
        borderColor: this.getEventColor(event)
      }));
    });
  }

  private getEventColor(event: ScheduledEventData): string {
    if (event.color) return event.color;
    if (event.eventStatus?.color) return event.eventStatus.color;
    if (event.schedulingTarget?.color) return event.schedulingTarget.color;
    return '#3788d8'; // Default blue
  }

  handleDateSelect(selectInfo: any): void {
    const modalRef = this.modalService.open(EventAddEditModalComponent, {
      size: 'xl',
      backdrop: 'static',
      keyboard: false
    });

    // Pass initial dates
    modalRef.componentInstance.initialStart = selectInfo.startStr;
    modalRef.componentInstance.initialEnd = selectInfo.endStr || selectInfo.startStr;

    modalRef.result.then(
      (result) => {
        if (result === true) {
          this.loadEvents();
        }
      },
      () => { /* dismissed */ }
    );
  }


  handleEventClick(clickInfo: any): void {
    const eventData = (clickInfo.event.extendedProps as any).eventData as ScheduledEventData;
    const modalRef = this.modalService.open(EventAddEditModalComponent, { size: 'lg' });
    modalRef.componentInstance.event = eventData;

    modalRef.result.then((updated) => {
      if (updated) {
        this.loadEvents();
      }
    });
  }

  handleEventDrop(dropInfo: any): void {
    this.updateEventTime(dropInfo.event);
  }

  handleEventResize(resizeInfo: any): void {
    this.updateEventTime(resizeInfo.event);
  }

  private updateEventTime(fcEvent: any): void {
    const eventData = (fcEvent.extendedProps as any).eventData as ScheduledEventData;

    // Mutate the existing object — no need to create a new instance
    eventData.startDateTime = fcEvent.start.toISOString();
    eventData.endDateTime = fcEvent.end
      ? fcEvent.end.toISOString()
      : fcEvent.start.toISOString();

    // Convert and submit
    const submit = this.scheduledEventService.ConvertToScheduledEventSubmitData(eventData);
    this.scheduledEventService.PutScheduledEvent(submit.id, submit).subscribe({
      next: () => {
        // Success — calendar already reflects change
        // Optional: clear caches if needed
        // eventData.ClearAssignmentsCache(); // example
      },
      error: (err) => {
        console.error('Failed to update event time', err);
        // Optional: revert UI change or show alert
      }
    });
  }

  renderEventContent(eventInfo: any): any {
    const escape = (str: string): string => {
      const div = document.createElement('div');
      div.textContent = str;
      return div.innerHTML;
    };
    const title = escape(eventInfo.event.title || '');
    const time = escape(eventInfo.timeText || '');
    const location = eventInfo.event.extendedProps.location
      ? `<div class="small">${escape(eventInfo.event.extendedProps.location)}</div>`
      : '';
    return {
      html: `
        <div class="fc-event-title">${title}</div>
        <div class="fc-event-time small">${time}</div>
        ${location}
      `
    };
  }
}
