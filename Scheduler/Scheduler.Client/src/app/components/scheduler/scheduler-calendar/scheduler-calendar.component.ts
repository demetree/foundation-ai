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
    events: [], // Filled in ngOnInit
    select: this.handleDateSelect.bind(this),
    eventClick: this.handleEventClick.bind(this),
    eventDrop: this.handleEventDrop.bind(this),
    eventResize: this.handleEventResize.bind(this),
    eventContent: this.renderEventContent.bind(this)
  };

  constructor(
    private scheduledEventService: ScheduledEventService,
    private modalService: NgbModal
  ) { }

  ngOnInit(): void {
    this.loadEvents();
  }

  private loadEvents(): void {
    // Pull last 3 months to next 6 months for smooth scrolling
    const today = new Date();
    const start = new Date(today.getFullYear(), today.getMonth() - 3, 1);
    const end = new Date(today.getFullYear(), today.getMonth() + 6, 0);

    this.scheduledEventService.GetScheduledEventList({
      startDateTime: start.toISOString(),
      endDateTime: end.toISOString(),
      includeRelations: true
    }).subscribe(events => {
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
