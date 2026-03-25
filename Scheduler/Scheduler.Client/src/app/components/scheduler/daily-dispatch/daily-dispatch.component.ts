//
// Daily Dispatch — drag-and-drop dispatch board for today's jobs.
//
// AI-Developed — This file was significantly developed with AI assistance.
//
import { Component, OnInit, ViewChild } from '@angular/core';
import { forkJoin } from 'rxjs';
import { ScheduledEventService, ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';
import { EventResourceAssignmentService, EventResourceAssignmentData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { TerminologyService } from '../../../services/terminology.service';
import { QuickAddJobModalComponent } from '../quick-add-job-modal/quick-add-job-modal.component';

@Component({
  selector: 'app-daily-dispatch',
  templateUrl: './daily-dispatch.component.html',
  styleUrls: ['./daily-dispatch.component.scss']
})
export class DailyDispatchComponent implements OnInit {
  @ViewChild('quickAddJob') quickAddJobComponent?: QuickAddJobModalComponent;

  public today: Date = new Date();
  public isLoading = true;

  public resources: ResourceData[] = [];
  public allEvents: ScheduledEventData[] = [];
  public assignments: EventResourceAssignmentData[] = [];

  // Derived arrays
  public unassignedEvents: ScheduledEventData[] = [];
  public resourceTimelines: { resource: ResourceData, events: ScheduledEventData[] }[] = [];

  constructor(
    private scheduledEventService: ScheduledEventService,
    private resourceService: ResourceService,
    private assignmentService: EventResourceAssignmentService,
    public terminology: TerminologyService,
    private alertService: AlertService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  /** Whether the current user can create/assign resources. */
  public get canWrite(): boolean {
    return this.authService.isSchedulerReaderWriter;
  }

  loadData(): void {
    this.isLoading = true;

    // Define today's range (00:00:00 to 23:59:59)
    const start = new Date(this.today);
    start.setHours(0, 0, 0, 0);
    const end = new Date(this.today);
    end.setHours(23, 59, 59, 999);

    forkJoin({
      resources: this.resourceService.GetResourceList({ active: true, deleted: false }),
      events: this.scheduledEventService.GetScheduledEventList({
        active: true,
        deleted: false,
        startDateTime: start.toISOString(),
        endDateTime: end.toISOString()
      })
    }).subscribe({
      next: (data) => {
        this.resources = data.resources || [];
        this.allEvents = data.events || [];

        // Load assignments scoped to today's events only (F10: bounded fetch)
        this.loadTodayAssignments();
      },
      error: () => {
        this.alertService.showMessage('Error loading dispatch data', '', MessageSeverity.error);
        this.isLoading = false;
      }
    });
  }

  /** Load assignments relevant to today's events only — avoids unbounded full-table fetch. */
  private loadTodayAssignments(): void {
    if (this.allEvents.length === 0) {
      this.assignments = [];
      this.processTimelines();
      this.isLoading = false;
      return;
    }

    // Fetch assignments filtered to active/non-deleted, then filter client-side
    // to only today's event IDs.
    this.assignmentService.GetEventResourceAssignmentList({
      active: true,
      deleted: false
    }).subscribe({
      next: (allAssignments) => {
        const todayEventIds = new Set(this.allEvents.map(e => Number(e.id)));
        this.assignments = (allAssignments || []).filter(
          a => a.scheduledEventId && todayEventIds.has(Number(a.scheduledEventId))
        );
        this.processTimelines();
        this.isLoading = false;
      },
      error: () => {
        this.assignments = [];
        this.processTimelines();
        this.isLoading = false;
      }
    });
  }

  private processTimelines(): void {
    // Set of assigned event IDs
    const assignedIds = new Set(this.assignments.map(a => Number(a.scheduledEventId)));

    // Unassigned subset
    this.unassignedEvents = this.allEvents.filter(e => !assignedIds.has(Number(e.id)));

    // Build timeline rows
    this.resourceTimelines = this.resources.map(res => {
      const resAssignments = this.assignments.filter(a => Number(a.resourceId) === Number(res.id));
      const resEventIds = new Set(resAssignments.map(a => Number(a.scheduledEventId)));
      const resEvents = this.allEvents.filter(e => resEventIds.has(Number(e.id)));

      return { resource: res, events: resEvents };
    });
  }

  // Drag and Drop support
  onDragStart(event: DragEvent, scheduledEvent: ScheduledEventData): void {
    if (!this.canWrite) { return; }
    if (event.dataTransfer) {
      event.dataTransfer.setData('text/plain', scheduledEvent.id.toString());
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  onDragOver(event: DragEvent): void {
    if (!this.canWrite) { return; }
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
  }

  onDrop(event: DragEvent, targetResourceId: bigint | number): void {
    event.preventDefault();

    // Permission check
    if (!this.canWrite) {
      this.alertService.showMessage('Permission Denied', 'You do not have write access.', MessageSeverity.info);
      return;
    }

    const eventIdStr = event.dataTransfer?.getData('text/plain');
    if (!eventIdStr) { return; }

    const eventId = Number(eventIdStr);

    // Prevent duplicate assignments
    if (this.assignments.some(a => Number(a.scheduledEventId) === eventId && Number(a.resourceId) === Number(targetResourceId))) {
      return;
    }

    this.isLoading = true;

    const newAssignment = {
      id: 0,
      scheduledEventId: eventId,
      resourceId: Number(targetResourceId),
      active: true,
      deleted: false,
      versionNumber: 0
    } as EventResourceAssignmentData;

    this.assignmentService.PostEventResourceAssignment(newAssignment).subscribe({
      next: () => {
        this.alertService.showMessage('Assigned successfully', '', MessageSeverity.success);
        this.loadData();
      },
      error: () => {
        this.alertService.showMessage('Failed to assign event', '', MessageSeverity.error);
        this.isLoading = false;
      }
    });
  }

  public onJobCreated(): void {
    this.loadData();
  }
}
