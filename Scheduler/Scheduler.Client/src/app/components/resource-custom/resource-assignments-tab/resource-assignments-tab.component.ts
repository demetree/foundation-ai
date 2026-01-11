import { Component, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { Subject } from 'rxjs'
import { Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { ResourceData } from '../../../scheduler-data-services/resource.service';
import { EventResourceAssignmentData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { ScheduledEventService, ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';

/**
 * Assignments tab for the Resource detail page.
 *
 * Displays all event assignments (past, current, future) for this resource.
 * Includes event name, date/time, role, status, and target (project/patient/client).
 *
 * Data loaded imperatively when tab becomes active.
 */
@Component({
  selector: 'app-resource-assignments-tab',
  templateUrl: './resource-assignments-tab.component.html',
  styleUrls: ['./resource-assignments-tab.component.scss']
})
export class ResourceAssignmentsTabComponent implements OnChanges {

  @Input() resource!: ResourceData | null;

  // Triggers when a resource assignment is changed.  To be implemented by users of this component.
  @Output() resourceAssignmentChanged = new Subject<ScheduledEventData | EventResourceAssignmentData>();

  public allAssignments: (EventResourceAssignmentData | ScheduledEventData | any)[] = [];

  public isLoading = true;
  public error: string | null = null;

  constructor(private router: Router,
              private scheduledEventService: ScheduledEventService) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resource'] && this.resource) {
      this.loadAssignments();

      this.resource.ClearScheduledEventsCache();
      this.resource.ClearEventResourceAssignmentsCache();
    }
  }

  /**
   * Loads all event assignments for this resource using the hybrid pattern.
   * Uses the EventResourceAssignments promise from ResourceData.
   */
  public async loadAssignments(): Promise<void> {

    if (!this.resource) {
      this.allAssignments = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    try {
      // 1. Get detailed assignments
      const detailed = await this.resource.EventResourceAssignments;

      // 2. Get primary assignments (events where this resource is the lead)
      const primaryEvents: ScheduledEventData[] = [];

      //
      // Get the scheduled events where this resource is the primary 
      //
      const primary = await lastValueFrom(
        this.scheduledEventService.GetScheduledEventList({
          resourceId: this.resource.id,
          active: true,
          deleted: false
        })
      );

      // 3. Combine, mark type, sort by start date
      const combined = [
        ...detailed.map(a => ({ ...a, assignmentType: 'detailed' as const })),
        ...primary.map(e => ({
          scheduledEvent: e,
          assignmentType: 'primary' as const,
          assignmentRole: null,
          assignmentStatus: null
        }))
      ];

      this.allAssignments = combined.sort((a, b) =>
        new Date(b.scheduledEvent?.startDateTime!).getTime() -
        new Date(a.scheduledEvent?.startDateTime!).getTime()
      );

      this.isLoading = false;
    } catch (err) {
      console.error('Failed to load assignments', err);
      this.error = 'Unable to load assignments';
      this.allAssignments = [];
      this.isLoading = false;
    }
  }

  /**
   * Navigate to the event detail page
   */
  public navigateToEvent(eventId: number | bigint| null | undefined): void {
    if (eventId) {
      this.router.navigate(['/scheduledevent', eventId]);
    }
  }

  /**
   * Determines status badge class for an assignment
   */
  public getAssignmentStatusBadge(assignment: EventResourceAssignmentData): string {
    const status = assignment.assignmentStatus;
    if (!status) return 'bg-secondary'; // Planned

    switch (status.name?.toLowerCase()) {
      case 'in progress':
        return 'bg-warning text-dark';
      case 'completed':
        return 'bg-success';
      case 'no-show':
      case 'canceled':
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }

  /**
   * Formats datetime for display
   */
  public formatDateTime(dateStr: string | null | undefined): string {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleString(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short'
    });
  }

  /**
   * Gets the target name (project, patient, etc.)
   */
  public getTargetName(assignment: EventResourceAssignmentData): string {
    return assignment.scheduledEvent?.schedulingTarget?.name || 'Unknown';
  }
}
