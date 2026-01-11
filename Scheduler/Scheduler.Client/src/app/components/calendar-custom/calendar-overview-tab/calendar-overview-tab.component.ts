import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Observable, Subject, of, combineLatest } from 'rxjs'
import { map, startWith, takeUntil } from 'rxjs/operators'
import { CalendarData } from '../../../scheduler-data-services/calendar.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';


/**
 * Overview tab for the Calendar detail page.
 *
 * Displays core calendar information and quick stats (counts of assignments, etc.).
 *
 * Child collection counts are loaded imperatively when the calendar input is available
 * to avoid async pipe infinite loops and performance issues.
 */
@Component({
  selector: 'app-calendar-overview-tab',
  templateUrl: './calendar-overview-tab.component.html',
  styleUrls: ['./calendar-overview-tab.component.scss']
})
export class CalendarOverviewTabComponent implements OnChanges {
  /** The calendar passed from parent detail component */
  @Input() calendar!: CalendarData | null;

  public assignmentCount$: Observable<number | bigint | null> = of(null);
  public clientCount$: Observable<number | bigint | null> = of(null);
  public schedulingTargetCount$: Observable<number | bigint | null> = of(null);

  // Cleanup
  private destroy$ = new Subject<void>();

  constructor(private clientService: ClientService,
    private schedulingTargetService: SchedulingTargetService,
    private scheduledEventService: ScheduledEventService,
    private eventResourceAssignmentService: EventResourceAssignmentService) {
  }

  ngOnChanges(changes: SimpleChanges): void {

    if (changes['calendar'] && this.calendar) {

      this.loadQuickStats();
    }
    else if (!this.calendar) {
      // Reset when calendar is cleared
      this.resetStats();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Loads all quick stat counts imperatively using the calendar's lazy promise getters.
   * Runs once when calendar becomes available.
   */
  public loadQuickStats(): void {

    if (!this.calendar) {
      return;
    }

    const calendarId = this.calendar.id;

    this.assignmentCount$ = this.scheduledEventService.GetScheduledEventsRowCount({
      calendarId: calendarId,
      active: true,
      deleted: false
    });

    //.pipe(
    //  map(count => Number(count ?? 0)),
    //  startWith(0)
    //);


    this.clientCount$ = this.clientService.GetClientsRowCount({
      calendarId: calendarId,
      active: true,
      deleted: false
    });

    //.pipe(
    //  map(count => Number(count ?? 0)),
    //  startWith(0)
    //);


    this.schedulingTargetCount$ = this.schedulingTargetService.GetSchedulingTargetsRowCount({
      calendarId: calendarId,
      active: true,
      deleted: false
    });

    //.pipe(
    //  map(count => Number(count ?? 0)),
    //  startWith(0)
    //);
  }

  /**
   * Resets all stats when resource is cleared (e.g., error state).
   */
  private resetStats(): void {
    this.assignmentCount$ = of(null);
    this.clientCount$ = of(null);
    this.schedulingTargetCount$ = of(null);
  }
}
