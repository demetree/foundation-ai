//
// Overview Component - Premium Dashboard Command Center
//
// This component serves as the main landing page for users, providing a high-level
// snapshot of the system state and role-based views for different user responsibilities.
//
// Follows the contact-custom-detail pattern with a header + tabbed interface.
//

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { forkJoin, Subject, Observable, of } from 'rxjs';
import { takeUntil, map } from 'rxjs/operators';
import { format, startOfDay, endOfDay, addDays, eachDayOfInterval, isWithinInterval } from 'date-fns';

//
// Data Services
//
import { ScheduledEventService, ScheduledEventData } from '../../scheduler-data-services/scheduled-event.service';
import { EventResourceAssignmentService, EventResourceAssignmentData } from '../../scheduler-data-services/event-resource-assignment.service';
import { ResourceService, ResourceData } from '../../scheduler-data-services/resource.service';
import { ResourceAvailabilityService, ResourceAvailabilityData } from '../../scheduler-data-services/resource-availability.service';
import { SchedulingTargetService, SchedulingTargetData } from '../../scheduler-data-services/scheduling-target.service';
import { ContactData } from '../../scheduler-data-services/contact.service';

//
// Core Services
//
import { AuthService } from '../../services/auth.service';
import { CurrentUserService } from '../../services/current-user.service';
import { UserSettingsService, FavouriteItem, MostRecentItem } from '../../services/user-settings.service';


//
// Interface definitions for dashboard data
//
interface DaySummary {
  date: Date;
  events: number;
  utilization: number;
  conflicts: number;
}

interface ActiveTargetSummary {
  id: number;
  name: string;
  type: string;
  eventCount: number;
}

interface ResourceUtilization {
  id: number;
  name: string;
  hours: number;
}


@Component({
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss']
})
export class OverviewComponent implements OnInit, OnDestroy {

  //
  // User settings key for tab persistence
  //
  private static readonly TAB_SETTING_KEY = 'overview_active_tab';

  //
  // Lifecycle management
  //
  private destroy$ = new Subject<void>();

  //
  // Loading state
  //
  public loading = true;
  public today = new Date();

  //
  // Current user info
  //
  public currentUserContact$: Observable<ContactData | null> = of(null);
  public userGreeting = '';

  //
  // Tab state
  //
  public activeTab = 'system';

  //
  // Dashboard data - System Overview
  //
  public todaysAssignments: Array<{ event: ScheduledEventData; assignments: EventResourceAssignmentData[] }> = [];
  public currentlyRunning: Array<{ event: ScheduledEventData; assignments: EventResourceAssignmentData[] }> = [];
  public activeResourcesToday = 0;
  public resourcesOnBlackoutToday: Array<{ resource: ResourceData; blackout: ResourceAvailabilityData }> = [];
  public potentialConflictsToday = 0;
  public next7Days: DaySummary[] = [];
  public activeTargets: ActiveTargetSummary[] = [];
  public availableResources = 0;
  public bookedResources = 0;
  public topUtilizedResources: ResourceUtilization[] = [];
  public totalResources = 0;

  //
  // Favourites and Recent Items
  //
  public favourites$: Observable<FavouriteItem[]> = of([]);
  public mostRecents$: Observable<MostRecentItem[]> = of([]);


  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    private currentUserService: CurrentUserService,
    private userSettingsService: UserSettingsService,
    private scheduledEventService: ScheduledEventService,
    private assignmentService: EventResourceAssignmentService,
    private resourceService: ResourceService,
    private resourceAvailabilityService: ResourceAvailabilityService,
    private schedulingTargetService: SchedulingTargetService
  ) {
    this.setGreeting();
  }


  ngOnInit(): void {

    //
    // Load current user contact
    //
    this.currentUserContact$ = this.currentUserService.contact$ || of(null);

    //
    // Load favourites and recent items
    //
    this.favourites$ = this.userSettingsService.getFavourites();
    this.mostRecents$ = this.userSettingsService.getMostRecents();

    //
    // Handle tab state from query params first, then user settings
    //
    this.route.queryParams.pipe(
      takeUntil(this.destroy$)
    ).subscribe(params => {
      if (params['tab']) {
        this.activeTab = params['tab'];
      } else {
        //
        // No query param - try to load from user settings
        //
        this.userSettingsService.getStringSetting(OverviewComponent.TAB_SETTING_KEY).pipe(
          takeUntil(this.destroy$)
        ).subscribe(savedTab => {
          if (savedTab != null && savedTab.length > 0) {
            this.activeTab = savedTab;
          }
        });
      }
    });

    //
    // Load dashboard data
    //
    this.loadDashboardData();
  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  /**
   * Sets the greeting based on time of day.
   */
  private setGreeting(): void {

    const hour = new Date().getHours();

    if (hour < 12) {
      this.userGreeting = 'Good morning';
    } else if (hour < 17) {
      this.userGreeting = 'Good afternoon';
    } else {
      this.userGreeting = 'Good evening';
    }
  }


  /**
   * Handles tab change events from ngbNav.
   */
  public onTabChange(event: any): void {

    this.activeTab = event.nextId;

    //
    // Update URL query param
    //
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: this.activeTab },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });

    //
    // Persist to user settings
    //
    this.userSettingsService.setStringSetting(OverviewComponent.TAB_SETTING_KEY, this.activeTab)
      .pipe(takeUntil(this.destroy$))
      .subscribe();
  }


  /**
   * Role-based tab visibility checks.
   */
  public canSeeDispatcherTab(): boolean {
    return this.authService.isSchedulerReaderWriter || this.authService.isSchedulerAdministrator;
  }

  public canSeeSchedulerTab(): boolean {
    return this.authService.isSchedulerReaderWriter || this.authService.isSchedulerAdministrator;
  }

  public canSeeManagerTab(): boolean {
    return this.authService.isSchedulerReader || this.authService.isSchedulerReaderWriter || this.authService.isSchedulerAdministrator;
  }


  /**
   * Main data load - parallel requests for performance.
   */
  private loadDashboardData(): void {

    this.loading = true;

    const todayStart = startOfDay(this.today);
    const todayEnd = endOfDay(this.today);
    const sevenDaysEnd = endOfDay(addDays(this.today, 7));

    forkJoin({
      upcomingEvents: this.scheduledEventService.GetScheduledEventList({
        startDateTime: todayStart.toISOString(),
        endDateTime: sevenDaysEnd.toISOString(),
        includeRelations: true
      }),
      allAssignments: this.assignmentService.GetEventResourceAssignmentList({
        includeRelations: true,
        active: true
      }),
      resources: this.resourceService.GetResourceList({ includeRelations: true }),
      blackouts: this.resourceAvailabilityService.GetResourceAvailabilityList({
        includeRelations: true
      }),
      targets: this.schedulingTargetService.GetSchedulingTargetList({
        active: true,
        includeRelations: true
      })
    }).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
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
   * Process all loaded data into dashboard metrics.
   */
  private processDashboardData(
    data: any,
    todayStart: Date,
    todayEnd: Date,
    sevenDaysEnd: Date
  ): void {

    const { upcomingEvents, allAssignments, resources, blackouts, targets } = data;

    this.totalResources = resources.length;

    //
    // Today's assignments & running events
    //
    const todayEvents = upcomingEvents.filter((e: ScheduledEventData) =>
      isWithinInterval(new Date(e.startDateTime), { start: todayStart, end: todayEnd }) ||
      isWithinInterval(new Date(e.endDateTime), { start: todayStart, end: todayEnd })
    );

    this.todaysAssignments = todayEvents.map((event: ScheduledEventData) => ({
      event,
      assignments: allAssignments.filter((a: EventResourceAssignmentData) => a.scheduledEventId === event.id)
    }));

    //
    // Currently running
    //
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

    //
    // Blackouts today
    //
    const resourceMap = new Map<number, ResourceData>();
    resources.forEach((r: ResourceData) => resourceMap.set(r.id as number, r));

    const todayBlackoutsWithResources: Array<{ resource: ResourceData; blackout: ResourceAvailabilityData }> = [];

    for (const b of blackouts) {
      if (new Date(b.startDateTime) <= todayEnd && (!b.endDateTime || new Date(b.endDateTime) >= todayStart)) {
        const resource = resourceMap.get(b.resourceId as number);
        if (resource) {
          todayBlackoutsWithResources.push({ resource, blackout: b });
        }
      }
    }

    this.resourcesOnBlackoutToday = todayBlackoutsWithResources;

    //
    // Next 7 days summary
    //
    const days = eachDayOfInterval({ start: this.today, end: sevenDaysEnd });
    this.next7Days = days.map(day => {
      const dayStart = startOfDay(day);
      const dayEnd = endOfDay(day);

      const dayEvents = upcomingEvents.filter((e: ScheduledEventData) =>
        isWithinInterval(new Date(e.startDateTime), { start: dayStart, end: dayEnd }) ||
        isWithinInterval(new Date(e.endDateTime), { start: dayStart, end: dayEnd })
      );

      const utilization = dayEvents.length > 0 ? Math.min(100, dayEvents.length * 15) : 0;

      return {
        date: day,
        events: dayEvents.length,
        utilization,
        conflicts: 0
      };
    });

    //
    // Active targets
    //
    this.activeTargets = targets
      .filter((t: SchedulingTargetData) => t.active === true)
      .map((t: SchedulingTargetData) => ({
        id: t.id,
        name: t.name,
        type: t.schedulingTargetType?.name || 'Unknown',
        eventCount: upcomingEvents.filter((e: ScheduledEventData) => e.schedulingTargetId === t.id).length
      }))
      .sort((a: ActiveTargetSummary, b: ActiveTargetSummary) => b.eventCount - a.eventCount)
      .slice(0, 8);

    //
    // Resource health
    //
    const bookedIds = new Set(
      this.todaysAssignments.flatMap(item =>
        item.assignments.map((a: EventResourceAssignmentData) => a.resourceId || a.crewId)
      ).filter((id: number | bigint | null) => id !== null)
    );

    this.availableResources = this.totalResources - bookedIds.size - this.resourcesOnBlackoutToday.length;
    this.bookedResources = bookedIds.size;

    //
    // Top utilized resources
    //
    this.topUtilizedResources = this.calculateTopUtilizedResources(resources, this.todaysAssignments);
  }


  /**
   * Calculate the top 5 most utilized resources based on booked hours today.
   */
  private calculateTopUtilizedResources(
    resources: ResourceData[],
    todaysAssignments: Array<{ event: ScheduledEventData; assignments: EventResourceAssignmentData[] }>
  ): ResourceUtilization[] {

    const utilizationList: ResourceUtilization[] = [];

    for (const resource of resources) {
      let totalHours = 0;

      for (const assignmentGroup of todaysAssignments) {
        for (const assignment of assignmentGroup.assignments) {
          if (assignment.resourceId !== resource.id) {
            continue;
          }

          let startStr: string | null | undefined = assignment.assignmentStartDateTime;
          if (!startStr) {
            startStr = assignmentGroup.event.startDateTime;
          }

          let endStr: string | null | undefined = assignment.assignmentEndDateTime;
          if (!endStr) {
            endStr = assignmentGroup.event.endDateTime;
          }

          if (!startStr || !endStr) {
            continue;
          }

          const start = new Date(startStr);
          const end = new Date(endStr);

          if (isNaN(start.getTime()) || isNaN(end.getTime())) {
            continue;
          }

          const hours = (end.getTime() - start.getTime()) / (1000 * 60 * 60);
          if (hours > 0) {
            totalHours += hours;
          }
        }
      }

      if (totalHours > 0) {
        utilizationList.push({
          id: resource.id as number,
          name: resource.name,
          hours: Math.round(totalHours * 10) / 10
        });
      }
    }

    utilizationList.sort((a, b) => b.hours - a.hours);
    return utilizationList.slice(0, 5);
  }


  //
  // Formatting helpers
  //
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


  //
  // Navigation helpers
  //
  createNewEvent(): void {
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

  navigateToCalendar(): void {
    this.router.navigate(['/calendar']);
  }

  navigateToEntity(entity: string, id: number): void {
    //
    // Route based on entity type
    //
    const routeMap: Record<string, string> = {
      'Contact': '/contacts',
      'ScheduledEvent': '/scheduledevents',
      'Resource': '/resources',
      'Client': '/clients',
      'Office': '/offices',
      'SchedulingTarget': '/schedulingtargets',
      'Crew': '/crews'
    };

    const basePath = routeMap[entity] || '/';
    this.router.navigate([basePath, id]);
  }
}
