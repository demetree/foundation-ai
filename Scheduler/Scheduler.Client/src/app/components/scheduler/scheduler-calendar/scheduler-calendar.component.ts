/**
 *
 * SchedulerCalendarComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Premium calendar view using FullCalendar with:
 *   - Custom gradient header with glassmorphic navigation controls
 *   - Quick Peek hover popover for event details
 *   - Drag-n-Drop guard for recurring instances
 *   - Server-side recurrence expansion via Calendar API endpoint
 *
 */

import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { FullCalendarComponent } from '@fullcalendar/angular';
import { CalendarOptions } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import { ScheduledEventService, ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';
import { CalendarService, CalendarData } from '../../../scheduler-data-services/calendar.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { EventAddEditModalComponent } from '../event-add-edit-modal/event-add-edit-modal.component';
import { format, parseISO } from 'date-fns';
import { forkJoin } from 'rxjs';
import { ConflictDetectionService, ScheduleConflict } from '../../../services/conflict-detection.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { CrewService } from '../../../scheduler-data-services/crew.service';
import { ScheduledEventDependencyService } from '../../../scheduler-data-services/scheduled-event-dependency.service';
import { EventResourceAssignmentService, EventResourceAssignmentData } from '../../../scheduler-data-services/event-resource-assignment.service';

@Component({
  selector: 'app-scheduler-calendar',
  templateUrl: './scheduler-calendar.component.html',
  styleUrls: ['./scheduler-calendar.component.scss']
})
export class SchedulerCalendarComponent implements OnInit, OnDestroy {
  @ViewChild('calendar') calendarComponent!: FullCalendarComponent;


  //
  // Header state
  //
  currentView: string = 'timeGridWeek';
  currentTitle: string = '';


  //
  // Quick Peek state
  //
  hoveredEvent: ScheduledEventData | null = null;
  popoverPosition = { top: 0, left: 0 };
  private popoverShowTimer: any = null;
  private popoverHideTimer: any = null;
  private readonly POPOVER_SHOW_DELAY = 300;   // ms before showing
  private readonly POPOVER_HIDE_DELAY = 200;   // ms before hiding (allows mouse-to-popover)


  //
  // Conflict Detection state
  //
  conflicts: ScheduleConflict[] = [];
  conflictEventIds: Set<number> = new Set();
  conflictPanelVisible: boolean = false;
  dependencyCountMap: Map<number, number> = new Map();
  hoveredAssignments: EventResourceAssignmentData[] = [];
  private assignmentsCache: Map<number, EventResourceAssignmentData[]> = new Map();


  //
  // Calendar Sidebar state
  //
  calendarSidebarVisible: boolean = false;
  availableCalendars: CalendarData[] = [];
  selectedCalendarIds: Set<number> = new Set();
  private readonly CALENDAR_SELECTION_KEY = 'scheduler-selected-calendars';


  //
  // FullCalendar options
  //
  calendarOptions: CalendarOptions = {
    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
    initialView: 'timeGridWeek',
    headerToolbar: false,  // We use our own premium header
    editable: true,
    selectable: true,
    selectMirror: true,
    dayMaxEvents: true,
    weekends: true,
    nowIndicator: true,
    height: 'auto',
    allDaySlot: true,
    slotMinTime: '06:00:00',
    slotMaxTime: '22:00:00',
    expandRows: true,
    stickyHeaderDates: true,
    events: [],
    select: this.handleDateSelect.bind(this),
    eventClick: this.handleEventClick.bind(this),
    eventDrop: this.handleEventDrop.bind(this),
    eventResize: this.handleEventResize.bind(this),
    eventContent: this.renderEventContent.bind(this),
    datesSet: this.handleDatesSet.bind(this),
    eventMouseEnter: this.handleEventMouseEnter.bind(this),
    eventMouseLeave: this.handleEventMouseLeave.bind(this)
  };


  constructor(
    private scheduledEventService: ScheduledEventService,
    private calendarService: CalendarService,
    private modalService: NgbModal,
    private conflictDetectionService: ConflictDetectionService,
    private resourceService: ResourceService,
    private crewService: CrewService,
    private dependencyService: ScheduledEventDependencyService,
    private assignmentService: EventResourceAssignmentService
  ) { }


  ngOnInit(): void {
    //
    // Load available calendars for the sidebar filter
    //
    this.loadCalendars();

    //
    // Initial event load is triggered by FullCalendar's datesSet callback when the view renders.
    //
  }


  ngOnDestroy(): void {
    this.clearPopoverTimers();
  }


  // =========================================================================
  // Custom Header Navigation
  // =========================================================================

  /**
   * Navigate the calendar (prev / next / today).
   */
  navigateCalendar(action: 'prev' | 'next' | 'today'): void {
    const calendarApi = this.calendarComponent?.getApi();

    if (calendarApi == null) {
      return;
    }

    switch (action) {
      case 'prev':
        calendarApi.prev();
        break;
      case 'next':
        calendarApi.next();
        break;
      case 'today':
        calendarApi.today();
        break;
    }

    this.updateHeaderState(calendarApi);
  }


  /**
   * Switch the calendar view.
   */
  changeView(viewName: string): void {
    const calendarApi = this.calendarComponent?.getApi();

    if (calendarApi == null) {
      return;
    }

    calendarApi.changeView(viewName);
    this.currentView = viewName;
    this.updateHeaderState(calendarApi);
  }


  /**
   * Sync the header title with the calendar's current date range.
   */
  private updateHeaderState(calendarApi: any): void {
    this.currentTitle = calendarApi.view.title;
    this.currentView = calendarApi.view.type;
  }


  // =========================================================================
  // Calendar Sidebar
  // =========================================================================

  /**
   * Load available calendars for the sidebar filter and restore selection from localStorage.
   */
  private loadCalendars(): void {
    this.calendarService.GetCalendarList({ active: true, deleted: false }).subscribe(calendars => {
      this.availableCalendars = calendars;

      //
      // Restore persisted selection from localStorage
      //
      const saved = localStorage.getItem(this.CALENDAR_SELECTION_KEY);

      if (saved) {
        try {
          const ids: number[] = JSON.parse(saved);
          // Only keep IDs that still exist in the available list
          const validIds = ids.filter(id => calendars.some(c => Number(c.id) === id));
          this.selectedCalendarIds = new Set(validIds);
        } catch {
          this.selectedCalendarIds = new Set();
        }
      }
    });
  }


  /**
   * Toggle the calendar sidebar panel visibility.
   */
  toggleCalendarSidebar(): void {
    this.calendarSidebarVisible = !this.calendarSidebarVisible;
  }


  /**
   * Toggle a calendar on/off and reload events.
   */
  toggleCalendar(calendarId: bigint | number): void {
    const id = Number(calendarId);

    if (this.selectedCalendarIds.has(id)) {
      this.selectedCalendarIds.delete(id);
    } else {
      this.selectedCalendarIds.add(id);
    }

    this.saveCalendarSelection();
    this.loadEvents();
  }


  /**
   * Check if a calendar is currently selected (for template binding).
   */
  isCalendarSelected(calendarId: bigint | number): boolean {
    return this.selectedCalendarIds.has(Number(calendarId));
  }


  /**
   * Persist the selected calendar IDs to localStorage.
   */
  private saveCalendarSelection(): void {
    localStorage.setItem(
      this.CALENDAR_SELECTION_KEY,
      JSON.stringify(Array.from(this.selectedCalendarIds))
    );
  }


  /**
   * Select all calendars (show everything).
   */
  selectAllCalendars(): void {
    this.selectedCalendarIds = new Set(this.availableCalendars.map(c => Number(c.id)));
    this.saveCalendarSelection();
    this.loadEvents();
  }


  /**
   * Clear all calendar selections (show unfiltered — no calendarIds sent).
   */
  clearCalendarSelection(): void {
    this.selectedCalendarIds.clear();
    this.saveCalendarSelection();
    this.loadEvents();
  }


  // =========================================================================
  // Data Loading
  // =========================================================================

  /**
   * Called by FullCalendar whenever the visible date range changes.
   */
  handleDatesSet(dateInfo: any): void {
    this.currentTitle = dateInfo.view.title;
    this.currentView = dateInfo.view.type;
    this.loadEvents(dateInfo.startStr, dateInfo.endStr);
  }


  private loadEvents(rangeStart?: string, rangeEnd?: string): void {

    //
    // If no range provided, use a default window
    //
    if (rangeStart == null || rangeEnd == null) {
      const today = new Date();
      const defaultStart = new Date(today.getFullYear(), today.getMonth() - 3, 1);
      const defaultEnd = new Date(today.getFullYear(), today.getMonth() + 6, 0);

      rangeStart = defaultStart.toISOString();
      rangeEnd = defaultEnd.toISOString();
    }

    //
    // Build calendarIds filter from sidebar selection
    //
    const calendarIds = this.selectedCalendarIds.size > 0
      ? Array.from(this.selectedCalendarIds)
      : undefined;

    forkJoin({
      events: this.scheduledEventService.GetCalendarEvents(rangeStart, rangeEnd, calendarIds),
      deps: this.dependencyService.GetScheduledEventDependencyList({ active: true, deleted: false })
    }).subscribe(({ events, deps }) => {

      //
      // Build dependency count map
      //
      this.dependencyCountMap = new Map();
      for (const dep of deps) {
        const predId = Number(dep.predecessorEventId);
        const succId = Number(dep.successorEventId);
        this.dependencyCountMap.set(predId, (this.dependencyCountMap.get(predId) || 0) + 1);
        this.dependencyCountMap.set(succId, (this.dependencyCountMap.get(succId) || 0) + 1);
      }

      //
      // Run conflict detection on loaded events
      //
      this.conflicts = this.conflictDetectionService.detectConflicts(events);
      this.conflictEventIds = this.conflictDetectionService.getConflictEventIds(this.conflicts);
      this.enrichConflictNames();

      this.calendarOptions.events = events.map(event => ({
        id: event.id.toString(),
        title: event.name,
        start: event.startDateTime,
        end: event.endDateTime,
        allDay: event.isAllDay === true,
        extendedProps: {
          eventData: event,
          location: event.location,
          notes: event.notes,
          isRecurringInstance: this.isRecurringInstance(event),
          hasConflict: this.conflictEventIds.has(Number(event.id)),
          dependencyCount: this.dependencyCountMap.get(Number(event.id)) || 0
        },
        backgroundColor: this.getEventColor(event),
        borderColor: this.getEventColor(event),
        // Prevent dragging of recurring virtual instances
        editable: !this.isRecurringInstance(event)
      }));
    });
  }


  // =========================================================================
  // Event Color
  // =========================================================================

  getEventColor(event: ScheduledEventData): string {
    if (event.color) return event.color;
    if (event.eventStatus?.color) return event.eventStatus.color;
    if (event.schedulingTarget?.color) return event.schedulingTarget.color;
    return '#667eea';  // Match our accent color
  }


  // =========================================================================
  // Quick Peek Popover
  // =========================================================================

  handleEventMouseEnter(info: any): void {
    const eventData = info.event.extendedProps?.eventData as ScheduledEventData;

    if (eventData == null) {
      return;
    }

    //
    // Clear any pending hide
    //
    this.clearHideTimer();

    //
    // Debounce show to avoid flicker on fast mouse movement
    //
    this.popoverShowTimer = setTimeout(() => {
      this.hoveredEvent = eventData;
      this.positionPopover(info.el);
      this.loadHoveredAssignments(eventData);
    }, this.POPOVER_SHOW_DELAY);
  }


  handleEventMouseLeave(info: any): void {
    //
    // Clear the show timer if user moved away before it fired
    //
    this.clearShowTimer();

    //
    // Delayed hide — allows the user to mouse into the popover itself
    //
    this.popoverHideTimer = setTimeout(() => {
      this.hoveredEvent = null;
      this.hoveredAssignments = [];
    }, this.POPOVER_HIDE_DELAY);
  }


  /**
   * Keep the popover visible while the mouse is inside it.
   */
  onPopoverMouseEnter(): void {
    this.clearHideTimer();
  }


  /**
   * Hide the popover when the mouse leaves it.
   */
  onPopoverMouseLeave(): void {
    this.hoveredEvent = null;
  }


  /**
   * Position the popover near the event element, keeping it within the viewport.
   */
  private positionPopover(eventEl: HTMLElement): void {
    const rect = eventEl.getBoundingClientRect();
    const popoverWidth = 300;
    const popoverEstimatedHeight = 200;
    const offset = 8;

    //
    // Default: position to the right of the event
    //
    let left = rect.right + offset;
    let top = rect.top;

    //
    // If it would go off the right edge, position to the left instead
    //
    if (left + popoverWidth > window.innerWidth - 16) {
      left = rect.left - popoverWidth - offset;
    }

    //
    // If it would go below the viewport, shift up
    //
    if (top + popoverEstimatedHeight > window.innerHeight - 16) {
      top = window.innerHeight - popoverEstimatedHeight - 16;
    }

    //
    // Clamp to positive values
    //
    this.popoverPosition = {
      top: Math.max(8, top),
      left: Math.max(8, left)
    };
  }


  private clearPopoverTimers(): void {
    this.clearShowTimer();
    this.clearHideTimer();
  }

  private clearShowTimer(): void {
    if (this.popoverShowTimer) {
      clearTimeout(this.popoverShowTimer);
      this.popoverShowTimer = null;
    }
  }

  private clearHideTimer(): void {
    if (this.popoverHideTimer) {
      clearTimeout(this.popoverHideTimer);
      this.popoverHideTimer = null;
    }
  }


  // =========================================================================
  // Recurring Instance Helpers
  // =========================================================================

  /**
   * Virtual recurring instances have negative IDs assigned by the server-side expansion service.
   */
  isRecurringInstance(event: ScheduledEventData): boolean {
    return Number(event.id) < 0;
  }


  /**
   * Format a human-readable time range for display in the Quick Peek popover.
   */
  formatTimeRange(event: ScheduledEventData): string {
    try {
      if (event.isAllDay) {
        const startDate = parseISO(event.startDateTime);
        return format(startDate, 'EEE, MMM d, yyyy') + ' (All Day)';
      }

      const start = parseISO(event.startDateTime);
      const end = parseISO(event.endDateTime);

      const startStr = format(start, 'EEE, MMM d · h:mm a');
      const endStr = format(end, 'h:mm a');

      return `${startStr} – ${endStr}`;
    } catch {
      return event.startDateTime || '';
    }
  }


  // =========================================================================
  // Event Interactions
  // =========================================================================

  /**
   * Select a date range on the calendar → open the Add Event modal.
   */
  handleDateSelect(selectInfo: any): void {
    //
    // Hide popover if shown
    //
    this.hoveredEvent = null;

    const modalRef = this.modalService.open(EventAddEditModalComponent, {
      size: 'xl',
      backdrop: 'static',
      keyboard: false
    });

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


  /**
   * Click an event → open the Edit modal.
   *
   * For recurring virtual instances, open the master (parent) event instead.
   */
  handleEventClick(clickInfo: any): void {
    //
    // Hide popover
    //
    this.hoveredEvent = null;

    const eventData = clickInfo.event.extendedProps?.eventData as ScheduledEventData;

    if (eventData == null) {
      return;
    }

    //
    // For recurring instances, we should open the master event.
    // The parentScheduledEventId links back to the series master.
    //
    if (this.isRecurringInstance(eventData) && eventData.parentScheduledEventId) {

      //
      // Fetch the master event from the server to get full details
      //
      this.scheduledEventService.GetScheduledEvent(Number(eventData.parentScheduledEventId)).subscribe({
        next: (masterEvent: ScheduledEventData) => {
          this.openEditModal(masterEvent);
        },
        error: (err: any) => {
          console.error('Failed to load master event for recurring instance', err);
          //
          // Fall back to opening the instance data we have
          //
          this.openEditModal(eventData);
        }
      });

      return;
    }

    this.openEditModal(eventData);
  }


  private openEditModal(eventData: ScheduledEventData): void {
    const modalRef = this.modalService.open(EventAddEditModalComponent, { size: 'lg' });
    modalRef.componentInstance.event = eventData;

    modalRef.result.then((updated) => {
      if (updated) {
        this.loadEvents();
      }
    },
      () => { /* dismissed */ });
  }


  // =========================================================================
  // Drag-n-Drop
  // =========================================================================

  /**
   * Drag an event to a new time slot.
   *
   * Virtual recurring instances cannot be saved individually — revert the drop.
   */
  handleEventDrop(dropInfo: any): void {
    const eventData = dropInfo.event.extendedProps?.eventData as ScheduledEventData;

    if (eventData == null || this.isRecurringInstance(eventData)) {
      dropInfo.revert();
      return;
    }

    this.updateEventTime(dropInfo.event, dropInfo.revert);
  }


  /**
   * Resize an event to change its duration.
   */
  handleEventResize(resizeInfo: any): void {
    const eventData = resizeInfo.event.extendedProps?.eventData as ScheduledEventData;

    if (eventData == null || this.isRecurringInstance(eventData)) {
      resizeInfo.revert();
      return;
    }

    this.updateEventTime(resizeInfo.event, resizeInfo.revert);
  }


  /**
   * Persist a drag/resize update to the server.
   */
  private updateEventTime(fcEvent: any, revertFn: () => void): void {
    const eventData = fcEvent.extendedProps?.eventData as ScheduledEventData;

    if (eventData == null) {
      return;
    }

    //
    // Mutate the data object with the new times
    //
    eventData.startDateTime = fcEvent.start.toISOString();
    eventData.endDateTime = fcEvent.end
      ? fcEvent.end.toISOString()
      : fcEvent.start.toISOString();

    //
    // Submit to server
    //
    const submit = this.scheduledEventService.ConvertToScheduledEventSubmitData(eventData);

    this.scheduledEventService.PutScheduledEvent(submit.id, submit).subscribe({
      next: () => {
        // Success — calendar already reflects change
      },
      error: (err) => {
        console.error('Failed to update event time', err);
        //
        // Revert the visual change on failure
        //
        revertFn();
      }
    });
  }


  // =========================================================================
  // Custom Event Rendering
  // =========================================================================

  renderEventContent(eventInfo: any): any {

    const escape = (str: string): string => {
      const div = document.createElement('div');
      div.textContent = str;
      return div.innerHTML;
    };

    const title = escape(eventInfo.event.title || '');
    const time = escape(eventInfo.timeText || '');
    const location = eventInfo.event.extendedProps?.location;
    const isRecurring = eventInfo.event.extendedProps?.isRecurringInstance === true;
    const hasConflict = eventInfo.event.extendedProps?.hasConflict === true;

    let html = '';

    // Conflict indicator — corner triangle
    if (hasConflict) {
      html += `<div class="fc-event-conflict-badge" title="Scheduling conflict"><i class="bi bi-exclamation-triangle-fill"></i></div>`;
    }

    html += `<div class="fc-event-title-custom">${title}</div>`;

    if (time) {
      html += `<div class="fc-event-time-custom">${time}</div>`;
    }

    if (location) {
      html += `<div class="fc-event-location-custom"><i class="bi bi-geo-alt-fill"></i>${escape(location)}</div>`;
    }

    if (isRecurring) {
      html += `<div class="fc-event-series-badge"><i class="bi bi-arrow-repeat"></i> Series</div>`;
    }

    return { html };
  }


  // =========================================================================
  // Conflict Detection Helpers
  // =========================================================================

  toggleConflictPanel(): void {
    this.conflictPanelVisible = !this.conflictPanelVisible;
  }

  /**
   * Look up resource/crew names and populate entityName on each conflict.
   */
  private enrichConflictNames(): void {
    if (this.conflicts.length === 0) return;

    // Collect unique resource/crew IDs
    const resourceIds = new Set<number>();
    const crewIds = new Set<number>();

    for (const c of this.conflicts) {
      if (c.type === 'resource') resourceIds.add(Number(c.entityId));
      if (c.type === 'crew') crewIds.add(Number(c.entityId));
    }

    // Load resources
    if (resourceIds.size > 0) {
      this.resourceService.GetResourceList({ active: true, deleted: false }).subscribe(resources => {
        const map = new Map(resources.map(r => [Number(r.id), r.name]));
        for (const c of this.conflicts) {
          if (c.type === 'resource') c.entityName = map.get(Number(c.entityId)) || `Resource #${c.entityId}`;
        }
      });
    }

    // Load crews
    if (crewIds.size > 0) {
      this.crewService.GetCrewList({ active: true, deleted: false }).subscribe(crews => {
        const map = new Map(crews.map(cr => [Number(cr.id), cr.name]));
        for (const c of this.conflicts) {
          if (c.type === 'crew') c.entityName = map.get(Number(c.entityId)) || `Crew #${c.entityId}`;
        }
      });
    }
  }


  /**
   * Open the edit modal for the first event of a conflict.
   */
  openConflictEvent(conflict: ScheduleConflict): void {
    this.conflictPanelVisible = false;
    this.openEditModal(conflict.eventA);
  }


  /**
   * Format overlap duration for display.
   */
  formatOverlap(minutes: number): string {
    if (minutes < 60) return `${minutes}m overlap`;
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m > 0 ? `${h}h ${m}m overlap` : `${h}h overlap`;
  }


  /**
   * Get the dependency count for a given event (for Quick Peek display).
   */
  getDependencyCount(event: ScheduledEventData): number {
    return this.dependencyCountMap.get(Number(event.id)) || 0;
  }


  /**
   * Lazy-load resource assignments for the hovered event.
   * Results are cached to avoid re-fetching on subsequent hovers.
   */
  private loadHoveredAssignments(event: ScheduledEventData): void {
    const eventId = Number(event.id);

    // Use cache if available
    if (this.assignmentsCache.has(eventId)) {
      this.hoveredAssignments = this.assignmentsCache.get(eventId) || [];
      return;
    }

    this.hoveredAssignments = [];
    this.assignmentService.GetEventResourceAssignmentList({
      scheduledEventId: eventId,
      active: true,
      deleted: false,
      includeRelations: true
    }).subscribe(assignments => {
      this.assignmentsCache.set(eventId, assignments);

      // Only apply if we're still hovering the same event
      if (this.hoveredEvent && Number(this.hoveredEvent.id) === eventId) {
        this.hoveredAssignments = assignments;
      }
    });
  }
}
