import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Observable, of, combineLatest, Subject } from 'rxjs';
import { map, startWith, takeUntil } from 'rxjs/operators';
import { OfficeData } from '../../../scheduler-data-services/office.service';

import { CrewService } from '../../../scheduler-data-services/crew.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { RateSheetService } from '../../../scheduler-data-services/rate-sheet.service';
import { OfficeContactService } from '../../../scheduler-data-services/office-contact.service';


/**
 * Overview tab for the Office detail page.
 *
 * Displays core office information and quick stats (counts of assignments, crews, etc.).
 *
 * Child collection counts are loaded imperatively when the office input is available
 * to avoid async pipe infinite loops and performance issues.
 */
@Component({
  selector: 'app-office-overview-tab',
  templateUrl: './office-overview-tab.component.html',
  styleUrls: ['./office-overview-tab.component.scss']
})
export class OfficeOverviewTabComponent implements OnChanges {
  /** The office passed from parent detail component */
  @Input() office!: OfficeData | null;

  // Individual count observables
  public assignmentCount$: Observable<number> = of(0);
  public crewCount$: Observable<number> = of(0);
  public resourceCount$: Observable<number> = of(0);
  public contactCount$: Observable<number> = of(0);
  public rateSheetCount$: Observable<number> = of(0);
  public calendarCount$: Observable<number> = of(0);

  // Cleanup
  private destroy$ = new Subject<void>();


  constructor(private crewService: CrewService,
    private officeContactService: OfficeContactService,
    private resourceService: ResourceService,
    private eventResourceAssignmentService: EventResourceAssignmentService,
    private rateSheetService: RateSheetService,
    private scheduledEventService: ScheduledEventService) { }

  ngOnChanges(changes: SimpleChanges): void {

    if (changes['office'] && this.office) {

      this.loadQuickStats();
    } else if (!this.office) {
      // Reset when office is cleared
      this.resetStats();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  /**
   * Loads all quick stat counts imperatively using the office's lazy promise getters.
   * Runs once when office becomes available.
   */
  public loadQuickStats(): void {

    if (!this.office) {
      return;
    }

    const officeId = this.office.id;

    // Individual row count observables
    const crewCount$ = this.crewService.GetCrewsRowCount({
      officeId: officeId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const resourceCount$ = this.resourceService.GetResourcesRowCount({
      officeId: officeId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const contactCount$ = this.officeContactService.GetOfficeContactsRowCount({
      officeId: officeId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );


    const rateSheetCount$ = this.rateSheetService.GetRateSheetsRowCount({
      officeId: officeId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );


    // Combined assignment count: direct assignments + events where this resource is lead
    const directAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentsRowCount({
      officeId: officeId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const leadEvents$ = this.scheduledEventService.GetScheduledEventsRowCount({
      officeId: officeId,
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
    this.crewCount$ = crewCount$;
    this.resourceCount$ = resourceCount$;
    this.assignmentCount$ = assignmentCount$;
    this.contactCount$ = contactCount$;
    this.rateSheetCount$ = rateSheetCount$;
  }

  /**
 * Resets all stats when resource is cleared (e.g., error state).
 */
  private resetStats(): void {
    this.assignmentCount$ = of(0);
    this.crewCount$ = of(0);
    this.resourceCount$ = of(0);
    this.contactCount$ = of(0);
    this.rateSheetCount$ = of(0);
  }
}
