import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Observable, Subject, of, combineLatest } from 'rxjs'
import { map, startWith, takeUntil } from 'rxjs/operators'
import { CrewData } from '../../../scheduler-data-services/crew.service';
import { CrewMemberService } from '../../../scheduler-data-services/crew-member.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';


/**
 * Overview tab for the Crew detail page.
 *
 * Displays core crew information and quick stats (counts of assignments, crews, etc.).
 *
 * Child collection counts are loaded imperatively when the crew input is available
 * to avoid async pipe infinite loops and performance issues.
 */
@Component({
  selector: 'app-crew-overview-tab',
  templateUrl: './crew-overview-tab.component.html',
  styleUrls: ['./crew-overview-tab.component.scss']
})
export class CrewOverviewTabComponent implements OnChanges {
  /** The crew passed from parent detail component */
  @Input() crew!: CrewData | null;

  public assignmentCount$: Observable<number> = of(0);
  public crewMemberCount$: Observable<number> = of(0);

  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;

  // Cleanup
  private destroy$ = new Subject<void>();

  constructor(private crewMemberService: CrewMemberService,
    private scheduledEventService: ScheduledEventService,
    private officeService: OfficeService,
    private schedulerHelperService: SchedulerHelperService,
    private eventResourceAssignmentService: EventResourceAssignmentService) {
  }

  ngOnChanges(changes: SimpleChanges): void {

    if (changes['crew'] && this.crew) {

      this.loadQuickStats();
    }
    else if (!this.crew) {
      // Reset when crew is cleared
      this.resetStats();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Loads all quick stat counts imperatively using the crew's lazy promise getters.
   * Runs once when crew becomes available.
   */
  public loadQuickStats(): void {

    if (!this.crew) {
      return;
    }

    const crewId = this.crew.id;

    // Individual row count observables
    const crewMemberCount$ = this.crewMemberService.GetCrewMembersRowCount({
      crewId: crewId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    // Combined assignment count: direct assignments + events where this resource is lead
    const directAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentsRowCount({
      crewId: crewId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const leadEvents$ = this.scheduledEventService.GetScheduledEventsRowCount({
      crewId: crewId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const assignmentCount$ = combineLatest([directAssignments$, leadEvents$]).pipe(
      map(([direct, lead]) => direct + lead),
      startWith(0)
    );

    // Assign to public observables
    this.crewMemberCount$ = crewMemberCount$;
    this.assignmentCount$ = assignmentCount$;
  }


  /**
   * Resets all stats when resource is cleared (e.g., error state).
   */
  private resetStats(): void {
    this.assignmentCount$ = of(0);
    this.crewMemberCount$ = of(0);
  }
}
