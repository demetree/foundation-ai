import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Observable, of, combineLatest, Subject } from 'rxjs';
import { map, startWith, takeUntil } from 'rxjs/operators';
import { ClientData } from '../../../scheduler-data-services/client.service';

import { CrewService } from '../../../scheduler-data-services/crew.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { ClientContactService } from '../../../scheduler-data-services/client-contact.service';


/**
 * Overview tab for the Client detail page.
 *
 * Displays core client information and quick stats (counts of assignments, crews, etc.).
 *
 * Child collection counts are loaded imperatively when the client input is available
 * to avoid async pipe infinite loops and performance issues.
 */
@Component({
  selector: 'app-client-overview-tab',
  templateUrl: './client-overview-tab.component.html',
  styleUrls: ['./client-overview-tab.component.scss']
})
export class ClientOverviewTabComponent implements OnChanges {
  /** The client passed from parent detail component */
  @Input() client!: ClientData | null;

  // Individual count observables
  public assignmentCount$: Observable<number> = of(0);
  public targetCount$: Observable<number> = of(0);
  public contactCount$: Observable<number> = of(0);
  public calendarCount$: Observable<number> = of(0);

  // Cleanup
  private destroy$ = new Subject<void>();


  constructor(private crewService: CrewService,
    private clientContactService: ClientContactService,
    private schedulingTargetService: SchedulingTargetService,
    private eventResourceAssignmentService: EventResourceAssignmentService,
    private scheduledEventService: ScheduledEventService) { }

  ngOnChanges(changes: SimpleChanges): void {

    if (changes['client'] && this.client) {

      this.loadQuickStats();
    } else if (!this.client) {
      // Reset when client is cleared
      this.resetStats();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  /**
   * Loads all quick stat counts imperatively using the client's lazy promise getters.
   * Runs once when client becomes available.
   */
  public loadQuickStats(): void {

    if (!this.client) {
      return;
    }

    const clientId = this.client.id;

    // Individual row count observables
    const crewCount$ = this.crewService.GetCrewsRowCount({
      clientId: clientId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const targetCount$ = this.schedulingTargetService.GetSchedulingTargetsRowCount({
      clientId: clientId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const contactCount$ = this.clientContactService.GetClientContactsRowCount({
      clientId: clientId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const assignmentCount$ = this.eventResourceAssignmentService.GetEventResourceAssignmentsRowCount({
      clientId: clientId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );


    // Assign to public observables
    this.targetCount$ = targetCount$;
    this.assignmentCount$ = assignmentCount$;
    this.contactCount$ = contactCount$;
  }

  /**
 * Resets all stats when resource is cleared (e.g., error state).
 */
  private resetStats(): void {
    this.assignmentCount$ = of(0);
    this.targetCount$ = of(0);
    this.contactCount$ = of(0);
  }
}
