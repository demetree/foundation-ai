import { Component, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { Subject } from 'rxjs'
import { Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { ClientData } from '../../../scheduler-data-services/client.service';
import { EventResourceAssignmentData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { ScheduledEventService, ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';

/**
 * Assignments tab for the Client detail page.
 *
 * Displays all event assignments (past, current, future) for this client.
 * Includes event name, date/time, role, status, and target (project/patient/client).
 *
 * Data loaded imperatively when tab becomes active.
 */
@Component({
  selector: 'app-client-assignments-tab',
  templateUrl: './client-assignments-tab.component.html',
  styleUrls: ['./client-assignments-tab.component.scss']
})
export class ClientAssignmentsTabComponent implements OnChanges {

  @Input() client!: ClientData | null;

  // Triggers when an client assignment is changed.  To be implemented by users of this component.
  @Output() clientAssignmentChanged = new Subject<ScheduledEventData | EventResourceAssignmentData>();

  public allAssignments: (EventResourceAssignmentData | ScheduledEventData | any)[] = [];

  public isLoading = true;
  public error: string | null = null;

  constructor(private router: Router,
              private scheduledEventService: ScheduledEventService) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['client'] && this.client) {

      this.client.ClearScheduledEventsCache();

      this.loadAssignments();
    }
  }

  /**
   * Loads all event assignments for this client using the hybrid pattern.
   * Uses the EventClientAssignments promise from ClientData.
   */
  public async loadAssignments(): Promise<void> {

    if (!this.client) {
      this.allAssignments = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    try {
     
      // Get primary assignments (events where this client is the lead)
      const primaryEvents: ScheduledEventData[] = [];

      //
      // Get the scheduled events where this client is the primary 
      //
      const primary = await lastValueFrom(
        this.scheduledEventService.GetScheduledEventList({
          clientId: this.client.id,
          active: true,
          deleted: false
        })
      );

      // 3. Combine, mark type, sort by start date
      const combined = [
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
