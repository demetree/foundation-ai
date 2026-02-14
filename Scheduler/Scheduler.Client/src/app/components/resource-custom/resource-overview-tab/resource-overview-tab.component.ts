import { Component, Input, OnChanges, SimpleChanges, OnDestroy } from '@angular/core';
import { Observable, of, combineLatest, Subject } from 'rxjs';
import { map, startWith, takeUntil } from 'rxjs/operators';

import { ResourceData } from '../../../scheduler-data-services/resource.service';
import { CrewMemberService } from '../../../scheduler-data-services/crew-member.service';
import { ResourceQualificationService } from '../../../scheduler-data-services/resource-qualification.service';
import { ResourceAvailabilityService } from '../../../scheduler-data-services/resource-availability.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { ResourceContactService } from '../../../scheduler-data-services/resource-contact.service';
import { RateSheetService } from '../../../scheduler-data-services/rate-sheet.service';
import { ResourceShiftService } from '../../../scheduler-data-services/resource-shift.service';
import { NotificationSubscriptionService } from '../../../scheduler-data-services/notification-subscription.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';

/**
 * Overview tab for the Resource detail page.
 *
 * Displays core resource information and quick stats (counts of assignments, crews, etc.).
 *
 * Counts are now loaded via dedicated row-count endpoints instead of full lists
 * for better performance and consistency with the main detail page.
 */
@Component({
  selector: 'app-resource-overview-tab',
  templateUrl: './resource-overview-tab.component.html',
  styleUrls: ['./resource-overview-tab.component.scss']
})
export class ResourceOverviewTabComponent implements OnChanges, OnDestroy {
  /** The resource passed from parent detail component */
  @Input() resource!: ResourceData | null;

  // Individual count observables
  public assignmentCount$: Observable<number> = of(0);
  public crewCount$: Observable<number> = of(0);
  public qualificationCount$: Observable<number> = of(0);
  public blackoutCount$: Observable<number> = of(0);
  public contactCount$: Observable<number> = of(0);
  public rateSheetCount$: Observable<number> = of(0);
  public shiftCount$: Observable<number> = of(0);
  public notificationCount$: Observable<number> = of(0);

  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;

  // Cleanup
  private destroy$ = new Subject<void>();

  constructor(
    private crewMemberService: CrewMemberService,
    private resourceQualificationService: ResourceQualificationService,
    private resourceAvailabilityService: ResourceAvailabilityService,
    private eventResourceAssignmentService: EventResourceAssignmentService,
    private resourceContactService: ResourceContactService,
    private rateSheetService: RateSheetService,
    private resourceShiftService: ResourceShiftService,
    private notificationSubscriptionService: NotificationSubscriptionService,
    private schedulerHelperService: SchedulerHelperService,
    private scheduledEventService: ScheduledEventService
  ) { }

  /**
   * Reacts to changes in the input resource.
   * When a valid resource is provided, kicks off row count loading.
   */
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resource'] && this.resource) {
      this.loadQuickStats();
    } else if (!this.resource) {
      // Reset when resource is cleared
      this.resetStats();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Loads all quick stat counts using dedicated row-count service methods.
   *
   * Uses combineLatest to wait for all counts, then derives a single loading state.
   * All counts are active=true, deleted=false (standard for overview badges).
   */
  public loadQuickStats(): void {
    if (!this.resource) {
      return;
    }

    const resourceId = this.resource.id;

    // Individual row count observables
    const crewCount$ = this.crewMemberService.GetCrewMembersRowCount({
      resourceId: resourceId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const qualificationCount$ = this.resourceQualificationService.GetResourceQualificationsRowCount({
      resourceId: resourceId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const blackoutCount$ = this.resourceAvailabilityService.GetResourceAvailabilitiesRowCount({
      resourceId: resourceId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const contactCount$ = this.resourceContactService.GetResourceContactsRowCount({
      resourceId: resourceId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );


    const rateSheetCount$ = this.rateSheetService.GetRateSheetsRowCount({
      resourceId: resourceId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const shiftCount$ = this.resourceShiftService.GetResourceShiftsRowCount({
      resourceId: resourceId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const notificationCount$ = this.notificationSubscriptionService.GetNotificationSubscriptionsRowCount({
      resourceId: resourceId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );


    // Combined assignment count: direct assignments + events where this resource is lead
    const directAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentsRowCount({
      resourceId: resourceId,
      active: true,
      deleted: false
    }).pipe(
      map(count => Number(count ?? 0)),
      startWith(0)
    );

    const leadEvents$ = this.scheduledEventService.GetScheduledEventsRowCount({
      resourceId: resourceId,
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
    this.qualificationCount$ = qualificationCount$;
    this.blackoutCount$ = blackoutCount$;
    this.assignmentCount$ = assignmentCount$;
    this.contactCount$ = contactCount$;
    this.rateSheetCount$ = rateSheetCount$;
    this.shiftCount$ = shiftCount$;
    this.notificationCount$ = notificationCount$;
  }

  /**
   * Resets all stats when resource is cleared (e.g., error state).
   */
  private resetStats(): void {
    this.assignmentCount$ = of(0);
    this.crewCount$ = of(0);
    this.qualificationCount$ = of(0);
    this.blackoutCount$ = of(0);
    this.contactCount$ = of(0);
    this.rateSheetCount$ = of(0);
    this.shiftCount$ = of(0);
    this.notificationCount$ = of(0);
  }
}
