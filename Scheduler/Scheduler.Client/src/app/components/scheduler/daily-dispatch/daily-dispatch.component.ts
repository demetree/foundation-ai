import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { ScheduledEventService, ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';
import { EventResourceAssignmentService, EventResourceAssignmentData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TerminologyService } from '../../../services/terminology.service';

@Component({
  selector: 'app-daily-dispatch',
  templateUrl: './daily-dispatch.component.html',
  styleUrls: ['./daily-dispatch.component.scss']
})
export class DailyDispatchComponent implements OnInit {
  public today: Date = new Date();
  public isLoading: boolean = true;
  
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
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.loadData();
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
      }),
      assignments: this.assignmentService.GetEventResourceAssignmentList({
        active: true,
        deleted: false
      })
    }).subscribe({
      next: (data) => {
        this.resources = data.resources || [];
        this.allEvents = data.events || [];
        this.assignments = data.assignments || [];

        this.processTimelines();
        this.isLoading = false;
      },
      error: (err) => {
        this.alertService.showMessage('Error loading dispatch data', '', MessageSeverity.error);
        this.isLoading = false;
      }
    });
  }

  private processTimelines(): void {
    // Filter assignments that apply to today's events
    const todayEventIds = new Set(this.allEvents.map(e => Number(e.id)));
    const relevantAssignments = this.assignments.filter(a => a.scheduledEventId && todayEventIds.has(Number(a.scheduledEventId)));
    
    // Set of assigned event IDs
    const assignedIds = new Set(relevantAssignments.map(a => Number(a.scheduledEventId)));

    // Unassigned subset
    this.unassignedEvents = this.allEvents.filter(e => !assignedIds.has(Number(e.id)));

    // Build timeline rows
    this.resourceTimelines = this.resources.map(res => {
      const resAssignments = relevantAssignments.filter(a => Number(a.resourceId) === Number(res.id));
      const resEventIds = new Set(resAssignments.map(a => Number(a.scheduledEventId)));
      const resEvents = this.allEvents.filter(e => resEventIds.has(Number(e.id)));
      
      return {
        resource: res,
        events: resEvents
      };
    });
  }

  // Drag and Drop support
  onDragStart(event: DragEvent, scheduledEvent: ScheduledEventData) {
    if (event.dataTransfer) {
      event.dataTransfer.setData('text/plain', scheduledEvent.id.toString());
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
  }

  onDrop(event: DragEvent, targetResourceId: bigint | number) {
    event.preventDefault();
    const eventIdStr = event.dataTransfer?.getData('text/plain');
    if (!eventIdStr) return;

    const eventId = Number(eventIdStr);
    
    // Check if it's already assigned there (to prevent duplicates)
    if (this.assignments.some(a => Number(a.scheduledEventId) === eventId && Number(a.resourceId) === Number(targetResourceId))) {
      return; 
    }

    this.isLoading = true;

    // Create new assignment
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
        this.loadData(); // Re-fetch all so the view updates cleanly
      },
      error: (err) => {
        this.alertService.showMessage('Failed to assign event', '', MessageSeverity.error);
        this.isLoading = false;
      }
    });

  }
}
