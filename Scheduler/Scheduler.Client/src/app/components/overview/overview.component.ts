//import { Component, OnInit, OnDestroy } from '@angular/core';
//import { Router } from '@angular/router';
//import { Subscription, forkJoin } from 'rxjs';
//import { format, startOfDay, endOfDay, addDays, isWithinInterval } from 'date-fns'; // npm i date-fns

//// Import your generated services
//import { ScheduledEventService, ScheduledEventData } from '../../scheduler-data-services/scheduled-event.service';
//import { EventResourceAssignmentService, EventResourceAssignmentData } from '../../scheduler-data-services/event-resource-assignment.service';
//import { ResourceService, ResourceData } from '../../scheduler-data-services/resource.service';
//// Assume you have this service (same pattern as the others)
//import { ResourceAvailabilityService, ResourceAvailabilityData } from '../../scheduler-data-services/resource-availability.service';

//@Component({
//  selector: 'app-overview',
//  templateUrl: './overview.component.html',
//  styleUrls: ['./overview.component.scss']
//})
//export class OverviewComponent implements OnInit, OnDestroy {
//  // Data containers
//  upcomingEvents: ScheduledEventData[] = [];
//  todaysAssignments: Array<{
//    event: ScheduledEventData;
//    assignments: EventResourceAssignmentData[];
//  }> = [];
//  resourcesOnBlackout: Array<{
//    resource: ResourceData;
//    blackout: ResourceAvailabilityData;
//  }> = [];

//  loading = true;
//  private subscriptions = new Subscription();

//  constructor(
//    private router: Router,
//    private scheduledEventService: ScheduledEventService,
//    private assignmentService: EventResourceAssignmentService,
//    private resourceService: ResourceService,
//    private resourceAvailabilityService: ResourceAvailabilityService
//  ) { }

//  ngOnInit(): void {
//    this.loadOverviewData();
//  }

//  ngOnDestroy(): void {
//    this.subscriptions.unsubscribe();
//  }

//  /**
//   * Load all data needed for the overview in parallel for best performance
//   */
//  private loadOverviewData(): void {
//    this.loading = true;

//    const todayStart = startOfDay(new Date());
//    const todayEnd = endOfDay(new Date());
//    const sevenDaysEnd = endOfDay(addDays(new Date(), 7));

//    // 1. Upcoming events (today + next 7 days)
//    const upcomingQuery = {
//      startDateTime: todayStart.toISOString(),
//      endDateTime: sevenDaysEnd.toISOString(),
//      pageSize: 50,
//      includeRelations: true
//    };

//    // 2. All active blackouts that overlap today
//    const blackoutQuery = {
//      startDateTime: null, // get all active
//      endDateTime: null,
//      active: true,
//      includeRelations: true
//    };

//    const data$ = forkJoin({
//      upcomingEvents: this.scheduledEventService.GetScheduledEventList(upcomingQuery),
//      allAssignments: this.assignmentService.GetEventResourceAssignmentList({ includeRelations: true }),
//      resources: this.resourceService.GetResourceList({ includeRelations: true }),
//      blackouts: this.resourceAvailabilityService.GetResourceAvailabilityList(blackoutQuery)
//    });

//    this.subscriptions.add(
//      data$.subscribe({
//        next: ({ upcomingEvents, allAssignments, resources, blackouts }) => {
//          this.upcomingEvents = upcomingEvents.sort((a, b) =>
//            new Date(a.startDateTime).getTime() - new Date(b.startDateTime).getTime()
//          );

//          // Filter assignments for events happening today
//          const todayEvents = this.upcomingEvents.filter(e =>
//            isWithinInterval(new Date(e.startDateTime), { start: todayStart, end: todayEnd }) ||
//            isWithinInterval(new Date(e.endDateTime), { start: todayStart, end: todayEnd })
//          );

//          this.todaysAssignments = todayEvents.map(event => ({
//            event,
//            assignments: allAssignments.filter(a => a.scheduledEventId === event.id && a.active)
//          }));

//          // Current blackouts
//          const resourceMap = new Map<number, ResourceData>();
//          resources.forEach(r => resourceMap.set(Number(r.id), r));

//          this.resourcesOnBlackout = blackouts
//            .filter(b =>
//              b.startDateTime <= todayEnd.toISOString() &&
//              (!b.endDateTime || b.endDateTime >= todayStart.toISOString())
//            )
//            .map(b => ({
//              resource: resourceMap.get(Number(b.resourceId))!,
//              blackout: b
//            }))
//            .filter(item => item.resource); // safety

//          this.loading = false;
//        },
//        error: (err) => {
//          console.error('Failed to load overview data', err);
//          this.loading = false;
//        }
//      })
//    );
//  }

//  // Helper for display
//  formatDateTime(iso: string): string {
//    return format(new Date(iso), 'PPP p'); // e.g., "Dec 22, 2025 2:00 PM"
//  }

//  formatDateRange(start: string, end: string): string {
//    const s = new Date(start);
//    const e = new Date(end);
//    if (s.toDateString() === e.toDateString()) {
//      return `${format(s, 'PPP')} ${format(s, 'p')} – ${format(e, 'p')}`;
//    }
//    return `${format(s, 'PPP p')} – ${format(e, 'PPP p')}`;
//  }

//  // Navigation helpers
//  navigateToEvents(): void {
//    this.router.navigate(['/scheduledevents']);
//  }

//  navigateToResources(): void {
//    this.router.navigate(['/resources']);
//  }

//  navigateToCrews(): void {
//    this.router.navigate(['/crews']);
//  }

//  createNewEvent(): void {
//    this.router.navigate(['/scheduledevents/new']);
//  }
//}




import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { format, startOfDay, endOfDay, addDays, eachDayOfInterval, isWithinInterval } from 'date-fns';

import { ScheduledEventService, ScheduledEventData } from '../../scheduler-data-services/scheduled-event.service';
import { EventResourceAssignmentService, EventResourceAssignmentData } from '../../scheduler-data-services/event-resource-assignment.service';
import { ResourceService, ResourceData } from '../../scheduler-data-services/resource.service';
import { ResourceAvailabilityService, ResourceAvailabilityData } from '../../scheduler-data-services/resource-availability.service';
import { ResourceShiftService, ResourceShiftData } from '../../scheduler-data-services/resource-shift.service';
import { SchedulingTargetService, SchedulingTargetData } from '../../scheduler-data-services/scheduling-target.service';
import { AssignmentStatusService, AssignmentStatusData } from '../../scheduler-data-services/assignment-status.service';
import { ScheduledEventTemplateService } from '../../scheduler-data-services/scheduled-event-template.service';

interface DaySummary {
  date: Date;
  events: number;
  utilization: number; // 0-100%
  conflicts: number;
}

interface ActiveTargetSummary {
  id: number;
  name: string;
  type: string;
  addressCount: number;
  eventCount: number;
}

interface ResourceUtilization {
  id: number;
  name: string;
  hours: number;
}

interface RecentChange {
  action: string;
  entity: string;
  by: string;
  when: Date;
}

@Component({
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss']
})
export class OverviewComponent implements OnInit, OnDestroy {
  // -------------------------------------------------------------------------
  // Core dashboard data
  // -------------------------------------------------------------------------
  loading = true;
  today = new Date();

  // Today's summary numbers
  todaysAssignments: Array<{ event: ScheduledEventData; assignments: EventResourceAssignmentData[] }> = [];
  currentlyRunning: Array<{ event: ScheduledEventData; assignments: EventResourceAssignmentData[] }> = [];
  activeResourcesToday = 0;
  resourcesOnBlackoutToday: Array<{ resource: ResourceData; blackout: ResourceAvailabilityData }> = [];
  potentialConflictsToday = 0;

  // Next 7 days
  next7Days: DaySummary[] = [];

  // Active targets (projects/patients)
  activeTargets: ActiveTargetSummary[] = [];

  // Resource health
  availableResources = 0;
  bookedResources = 0;
  topUtilizedResources: ResourceUtilization[] = [];

  // Recent activity (placeholder — pull from audit log or change history if available)
  recentChanges: RecentChange[] = [];

  constructor(
    private router: Router,
    private scheduledEventService: ScheduledEventService,
    private assignmentService: EventResourceAssignmentService,
    private resourceService: ResourceService,
    private resourceAvailabilityService: ResourceAvailabilityService,
    private resourceShiftService: ResourceShiftService,
    private schedulingTargetService: SchedulingTargetService,
    private assignmentStatusService: AssignmentStatusService,
    private templateService: ScheduledEventTemplateService
  ) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  ngOnDestroy(): void {
    // No subscriptions to clean up (forkJoin completes)
  }

  /**
   * Main data load — parallel requests for performance
   */
  private loadDashboardData(): void {
    this.loading = true;

    const todayStart = startOfDay(this.today);
    const todayEnd = endOfDay(this.today);
    const sevenDaysEnd = endOfDay(addDays(this.today, 7));

    forkJoin({
      // Events in next 7 days
      upcomingEvents: this.scheduledEventService.GetScheduledEventList({
        startDateTime: todayStart.toISOString(),
        endDateTime: sevenDaysEnd.toISOString(),
        includeRelations: true
      }),
      // All active assignments
      allAssignments: this.assignmentService.GetEventResourceAssignmentList({
        includeRelations: true,
        active: true
      }),
      // All resources
      resources: this.resourceService.GetResourceList({ includeRelations: true }),
      // Current blackouts
      blackouts: this.resourceAvailabilityService.GetResourceAvailabilityList({
        includeRelations: true
      }),
      // Shifts (for availability)
      shifts: this.resourceShiftService.GetResourceShiftList({ includeRelations: true }),
      // Active targets
      targets: this.schedulingTargetService.GetSchedulingTargetList({
        active: true,
        includeRelations: true
      })
    }).subscribe({
      next: (data) => {
        this.processDashboardData(data, todayStart, todayEnd, sevenDaysEnd);
        this.loading = false;
      },
      error: (err) => {
        console.error('Dashboard load failed', err);
        this.loading = false;
      }
    });
  }

  /**
   * Process all loaded data into dashboard metrics
   */
  private processDashboardData(
    data: any,
    todayStart: Date,
    todayEnd: Date,
    sevenDaysEnd: Date
  ): void {
    const {
      upcomingEvents,
      allAssignments,
      resources,
      blackouts,
      shifts,
      targets
    } = data;

    // -----------------------------------------------------------------
    // Today's assignments & running events
    // -----------------------------------------------------------------
    const todayEvents = upcomingEvents.filter((e: ScheduledEventData) =>
      isWithinInterval(new Date(e.startDateTime), { start: todayStart, end: todayEnd }) ||
      isWithinInterval(new Date(e.endDateTime), { start: todayStart, end: todayEnd })
    );

    this.todaysAssignments = todayEvents.map((event: ScheduledEventData) => ({
      event,
      assignments: allAssignments.filter((a: EventResourceAssignmentData) => a.scheduledEventId === event.id)
    }));

    // Currently running (now is between start/end)
    const now = new Date();
    this.currentlyRunning = this.todaysAssignments.filter(item =>
      isWithinInterval(now, {
        start: new Date(item.event.startDateTime),
        end: new Date(item.event.endDateTime)
      })
    );

    this.activeResourcesToday = new Set(
      this.todaysAssignments.flatMap(item => item.assignments.map((a: any) => a.resourceId || a.crewId))
    ).size;

    // -----------------------------------------------------------------
    // Blackouts today
    // -----------------------------------------------------------------
    const resourceMap = new Map<number, ResourceData>();
    resources.forEach((r: ResourceData) => resourceMap.set(r.id as number, r));

    const todayBlackoutsWithResources: Array<{
      resource: ResourceData;
      blackout: ResourceAvailabilityData;
    }> = [];

    for (const b of blackouts) {
      if (
        new Date(b.startDateTime) <= todayEnd &&
        (!b.endDateTime || new Date(b.endDateTime) >= todayStart)
      ) {
        const resource = resourceMap.get(b.resourceId as number);
        if (resource) {
          todayBlackoutsWithResources.push({
            resource,
            blackout: b
          });
        }
      }
    }

    this.resourcesOnBlackoutToday = todayBlackoutsWithResources;

    // -----------------------------------------------------------------
    // Next 7 days summary
    // -----------------------------------------------------------------
    const days = eachDayOfInterval({ start: this.today, end: sevenDaysEnd });
    this.next7Days = days.map(day => {
      const dayStart = startOfDay(day);
      const dayEnd = endOfDay(day);

      const dayEvents = upcomingEvents.filter((e: ScheduledEventData) =>
        isWithinInterval(new Date(e.startDateTime), { start: dayStart, end: dayEnd }) ||
        isWithinInterval(new Date(e.endDateTime), { start: dayStart, end: dayEnd })
      );

      // Simple utilization: events on this day / total possible (placeholder)
      const utilization = dayEvents.length > 0 ? Math.min(100, dayEvents.length * 20) : 0;

      // Placeholder conflicts — replace with real check later
      const conflicts = 0;

      return {
        date: day,
        events: dayEvents.length,
        utilization,
        conflicts
      };
    });

    // -----------------------------------------------------------------
    // Active targets (projects/patients)
    // -----------------------------------------------------------------
    this.activeTargets = targets
      .filter((t: SchedulingTargetData) => t.active)
      .map((t: SchedulingTargetData) => ({
        id: t.id,
        name: t.name,
        type: t.schedulingTargetType?.name || 'Unknown',
        //addressCount: t.schedulingTargetAddresses?.length || 0,
        eventCount: upcomingEvents.filter((e: ScheduledEventData) => e.schedulingTargetId === t.id).length
      }))
      .sort((a: ActiveTargetSummary, b: ActiveTargetSummary) => b.eventCount - a.eventCount)
      .slice(0, 10); // Top 10

    // -----------------------------------------------------------------
    // Resource health
    // -----------------------------------------------------------------
    const allResourceIds = new Set(resources.map((r: ResourceData) => r.id));
    const bookedIds = new Set(
      this.todaysAssignments.flatMap(item =>
        item.assignments.map((a: EventResourceAssignmentData) => a.resourceId || a.crewId) // crewId ignored for count
      ).filter((id: number | bigint | null) => id !== null)
    );

    this.availableResources = allResourceIds.size - bookedIds.size - this.resourcesOnBlackoutToday.length;
    this.bookedResources = bookedIds.size;

    // Top utilized (placeholder — hours booked today)
    this.topUtilizedResources = this.calculateTopUtilizedResources(resources, this.todaysAssignments);

    // -----------------------------------------------------------------
    // Recent changes (placeholder — replace with audit log when available)
    // -----------------------------------------------------------------
    this.recentChanges = [
      { action: 'Created', entity: 'Event', by: 'John Smith', when: new Date() },
      { action: 'Updated', entity: 'Crew', by: 'Admin', when: new Date(Date.now() - 3600000) },
      { action: 'Added', entity: 'Blackout', by: 'Sarah Lee', when: new Date(Date.now() - 7200000) }
    ];
  }


  /**
   * Calculate the top 5 most utilized resources based on booked hours today.
   * Uses actual assignment times if available, otherwise falls back to event times.
   */
  private calculateTopUtilizedResources(
    resources: ResourceData[],
    todaysAssignments: Array<{ event: ScheduledEventData; assignments: EventResourceAssignmentData[] }>
  ): ResourceUtilization[] {
    const utilizationList: ResourceUtilization[] = [];

    for (const resource of resources) {
      let totalHours = 0;

      // Look through all of today's assignments
      for (const assignmentGroup of todaysAssignments) {
        for (const assignment of assignmentGroup.assignments) {
          // Only count assignments for this specific resource
          if (assignment.resourceId !== resource.id) {
            continue;
          }

          // Determine start and end times
          // Prefer explicit assignment times, fall back to event times
          let startStr: string | null | undefined = assignment.assignmentStartDateTime;
          if (!startStr) {
            startStr = assignmentGroup.event.startDateTime;
          }

          let endStr: string | null | undefined = assignment.assignmentEndDateTime;
          if (!endStr) {
            endStr = assignmentGroup.event.endDateTime;
          }

          // Safety: skip if we don't have valid dates
          if (!startStr || !endStr) {
            continue;
          }

          const start = new Date(startStr);
          const end = new Date(endStr);

          // Validate dates to avoid NaN
          if (isNaN(start.getTime()) || isNaN(end.getTime())) {
            continue;
          }

          const hours = (end.getTime() - start.getTime()) / (1000 * 60 * 60);
          if (hours > 0) {
            totalHours += hours;
          }
        }
      }

      // Only include resources with actual booked time
      if (totalHours > 0) {
        utilizationList.push({
          id: resource.id as number,
          name: resource.name,
          hours: Math.round(totalHours * 10) / 10  // One decimal place
        });
      }
    }

    // Sort descending and take top 5
    utilizationList.sort((a, b) => b.hours - a.hours);
    return utilizationList.slice(0, 5);
  }


  // -------------------------------------------------------------------------
  // Formatting helpers
  // -------------------------------------------------------------------------
  formatDateTime(iso: string): string {
    return format(new Date(iso), 'PPP p');
  }

  formatDateRange(start: string, end: string): string {
    const s = new Date(start);
    const e = new Date(end);
    if (s.toDateString() === e.toDateString()) {
      return `${format(s, 'PPP')} ${format(s, 'p')} – ${format(e, 'p')}`;
    }
    return `${format(s, 'PPP p')} – ${format(e, 'PPP p')}`;
  }

  // -------------------------------------------------------------------------
  // Navigation
  // -------------------------------------------------------------------------
  createNewEvent(): void {
    this.router.navigate(['/scheduledevents/new']);
  }

  createFromTemplate(): void {
    // Future: open template picker modal
    this.router.navigate(['/scheduledevents/new']);
  }

  navigateToEvents(): void {
    this.router.navigate(['/scheduledevents']);
  }

  navigateToResources(): void {
    this.router.navigate(['/resources']);
  }

  navigateToCrews(): void {
    this.router.navigate(['/crews']);
  }

  navigateToTargets(): void {
    this.router.navigate(['/schedulingtargets']);
  }
}
