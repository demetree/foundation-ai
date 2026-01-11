import { Component, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { Subject } from 'rxjs'
import { Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { OfficeData } from '../../../scheduler-data-services/office.service';
import { EventResourceAssignmentData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { ScheduledEventService, ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';

/**
 * Assignments tab for the Office detail page.
 *
 * Displays all event assignments (past, current, future) for this office.
 * Includes event name, date/time, role, status, and target (project/patient/client).
 *
 * Data loaded imperatively when tab becomes active.
 */
@Component({
  selector: 'app-office-assignments-tab',
  templateUrl: './office-assignments-tab.component.html',
  styleUrls: ['./office-assignments-tab.component.scss']
})
export class OfficeAssignmentsTabComponent implements OnChanges {

  @Input() office!: OfficeData | null;

  // Triggers when an office assignment is changed.  To be implemented by users of this component.
  @Output() officeAssignmentChanged = new Subject<ScheduledEventData | EventResourceAssignmentData>();

  public allAssignments: (EventResourceAssignmentData | ScheduledEventData | any)[] = [];

  public isLoading = true;
  public error: string | null = null;

  constructor(private router: Router,
              private scheduledEventService: ScheduledEventService) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['office'] && this.office) {

      this.office.ClearScheduledEventsCache();
      this.office.ClearEventResourceAssignmentsCache();

      this.loadAssignments();
    }
  }

  /**
   * Loads all event assignments for this office using the hybrid pattern.
   * Uses the EventOfficeAssignments promise from OfficeData.
   */
  public async loadAssignments(): Promise<void> {

    if (!this.office) {
      this.allAssignments = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    try {
      // 1. Get detailed assignments
      const detailed = await this.office.EventResourceAssignments;

      // 2. Get primary assignments (events where this office is the lead)
      const primaryEvents: ScheduledEventData[] = [];

      //
      // Get the scheduled events where this office is the primary 
      //
      const primary = await lastValueFrom(
        this.scheduledEventService.GetScheduledEventList({
          officeId: this.office.id,
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
