import { Component, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { Subject } from 'rxjs'
import { Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { CalendarData } from '../../../scheduler-data-services/calendar.service';
import { EventCalendarService, EventCalendarData } from '../../../scheduler-data-services/event-calendar.service';
//import { EventResourceAssignmentData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';


/**
 * Assignments tab for the Resource detail page.
 *
 * Displays all event assignments (past, current, future) for this resource.
 * Includes event name, date/time, role, status, and target (project/patient/client).
 *
 * Data loaded imperatively when tab becomes active.
 */
@Component({
  selector: 'app-calendar-assignments-tab',
  templateUrl: './calendar-assignments-tab.component.html',
  styleUrls: ['./calendar-assignments-tab.component.scss']
})
export class CalendarAssignmentsTabComponent implements OnChanges {

  @Input() calendar!: CalendarData | null;

  // Triggers when an assignment is changed.  To be implemented by users of this component.
  @Output() calendarAssignmentChanged = new Subject<EventCalendarData>();

  public assignments: (EventCalendarData)[] = [];

  public isLoading = true;
  public error: string | null = null;

  constructor(private router: Router,
              private eventCalendarService: EventCalendarService) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['calendar'] && this.calendar) {

      this.calendar.ClearEventCalendarsCache();

      this.loadAssignments();
    }
  }

  public async loadAssignments(): Promise<void> {

    if (!this.calendar) {
      this.assignments = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    try {


      //
      // Get the event calendars
      //
      this.assignments = await lastValueFrom(
        this.eventCalendarService.GetEventCalendarList({
          calendarId: this.calendar.id,
          active: true,
          deleted: false,
          includeRelations: true
        })
      );
      
      this.isLoading = false;
    } catch (err) {
      console.error('Failed to load assignments', err);
      this.error = 'Unable to load assignments';
      this.assignments = [];
      this.isLoading = false;
    }
  }

  /**
   * Navigate to the event detail page
   */
  public navigateToEvent(eventId: number | bigint| null | undefined): void {
    if (eventId) {
      this.router.navigate(['/scheduledevent', eventId]);
    }
  }


  /**
   * Formats datetime for display
   */
  public formatDateTime(dateStr: string | null | undefined): string {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleString(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short'
    });
  }

  /**
   * Gets the target name (project, patient, etc.)
   */
  public getTargetName(event: ScheduledEventData | null | undefined): string {

    if (event == null || event == undefined) {
      return "";
    }

    return event?.schedulingTarget?.name || 'Unknown';
  }
}
