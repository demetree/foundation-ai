import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { VolunteerProfileData, VolunteerProfileService } from '../../../scheduler-data-services/volunteer-profile.service';
import { EventResourceAssignmentData, EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { EventAddEditModalComponent } from '../../scheduler/event-add-edit-modal/event-add-edit-modal.component';
import { VolunteerSuggestionService, VolunteerSuggestion, SuggestionContext } from '../../../services/volunteer-suggestion.service';

/**
 * Assignments tab for the Volunteer detail page.
 *
 * Displays all event assignments (past, current, future) where this volunteer
 * is assigned (isVolunteer = true).  Includes event name, date/time, role,
 * status, and reported/approved hours.
 *
 * Data loaded imperatively when the tab becomes active.
 */
@Component({
    selector: 'app-volunteer-assignments-tab',
    templateUrl: './volunteer-assignments-tab.component.html',
    styleUrls: ['./volunteer-assignments-tab.component.scss']
})
export class VolunteerAssignmentsTabComponent implements OnChanges {

    @Input() volunteer!: VolunteerProfileData | null;

    public allAssignments: EventResourceAssignmentData[] = [];
    public isLoading = true;
    public error: string | null = null;

    // Smart Suggestions
    public suggestions: VolunteerSuggestion[] = [];
    public showSuggestions = false;
    public isLoadingSuggestions = false;

    constructor(
        private router: Router,
        private eventResourceAssignmentService: EventResourceAssignmentService,
        private volunteerProfileService: VolunteerProfileService,
        private suggestionService: VolunteerSuggestionService,
        private modalService: NgbModal
    ) { }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['volunteer'] && this.volunteer) {
            this.loadAssignments();
        }
    }

    /**
     * Loads all event assignments for this volunteer's resource where isVolunteer = true.
     */
    public async loadAssignments(): Promise<void> {
        if (!this.volunteer || !this.volunteer.resourceId) {
            this.allAssignments = [];
            this.isLoading = false;
            return;
        }

        this.isLoading = true;
        this.error = null;

        try {
            // Clear cached data so we get fresh results
            this.eventResourceAssignmentService.ClearAllCaches();

            const assignments = await lastValueFrom(
                this.eventResourceAssignmentService.GetEventResourceAssignmentList({
                    resourceId: this.volunteer.resourceId,
                    isVolunteer: true,
                    active: true,
                    deleted: false,
                    includeRelations: true
                })
            );

            // Sort by start date, most recent first
            this.allAssignments = (assignments ?? []).sort((a, b) => {
                const dateA = a.assignmentStartDateTime || a.scheduledEvent?.startDateTime || '';
                const dateB = b.assignmentStartDateTime || b.scheduledEvent?.startDateTime || '';
                return new Date(dateB).getTime() - new Date(dateA).getTime();
            });

            this.isLoading = false;
        } catch (err) {
            console.error('Failed to load volunteer assignments', err);
            this.error = 'Unable to load assignments';
            this.allAssignments = [];
            this.isLoading = false;
        }
    }

    /**
     * Navigate to the event detail page.
     */
    public navigateToEvent(eventId: number | bigint | null | undefined): void {
        if (eventId) {
            this.router.navigate(['/scheduledevent', eventId]);
        }
    }

    /**
     * Determines status badge class for an assignment.
     */
    public getAssignmentStatusBadge(assignment: EventResourceAssignmentData): string {
        const status = assignment.assignmentStatus;
        if (!status) {
            return 'bg-secondary';
        }

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
     * Formats a datetime string for display.
     */
    public formatDateTime(dateStr: string | null | undefined): string {
        if (!dateStr) {
            return '—';
        }
        return new Date(dateStr).toLocaleString(undefined, {
            dateStyle: 'medium',
            timeStyle: 'short'
        });
    }

    /**
     * Gets the target name (project, patient, etc.) from the scheduled event.
     */
    public getTargetName(assignment: EventResourceAssignmentData): string {
        return assignment.scheduledEvent?.schedulingTarget?.name || '—';
    }

    /**
     * Formats a number as hours for display.
     */
    public formatHours(hours: number | null | undefined): string {
        if (hours === null || hours === undefined) {
            return '—';
        }
        return hours.toFixed(1);
    }

    /**
     * Opens the event add/edit modal in create mode with this volunteer pre-assigned.
     */
    public openAddAssignment(): void {
        if (!this.volunteer || !this.volunteer.resourceId) {
            return;
        }

        const modalRef = this.modalService.open(EventAddEditModalComponent, {
            size: 'xl',
            backdrop: 'static',
            keyboard: false
        });

        modalRef.componentInstance.initialResourceId = Number(this.volunteer.resourceId);
        modalRef.componentInstance.initialIsVolunteer = true;

        modalRef.result.then(
            (result) => {
                if (result === true) {
                    this.loadAssignments();
                }
            },
            () => { /* dismissed */ }
        );
    }

    /**
     * Checks if the current user can write volunteer data.
     */
    public userIsVolunteerWriter(): boolean {
        return this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter();
    }

    /**
     * Load smart suggestions — volunteers who'd be good matches for similar events.
     */
    public async toggleSuggestions(): Promise<void> {
        this.showSuggestions = !this.showSuggestions;

        if (!this.showSuggestions || this.suggestions.length > 0) return;

        this.isLoadingSuggestions = true;

        try {
            // Get all active volunteers
            const allVolunteers = await lastValueFrom(
                this.volunteerProfileService.GetVolunteerProfileList({
                    active: true,
                    deleted: false,
                    includeRelations: true
                })
            );

            // Get all active assignments for workload context
            const allAssignments = await lastValueFrom(
                this.eventResourceAssignmentService.GetEventResourceAssignmentList({
                    isVolunteer: true,
                    active: true,
                    deleted: false,
                    includeRelations: true
                })
            );

            // Build context from the volunteer's most recent assignment
            const recentAssignment = this.allAssignments[0]; // sorted newest first
            const context: SuggestionContext = {
                eventName: recentAssignment?.scheduledEvent?.name || '',
                eventDescription: recentAssignment?.scheduledEvent?.description || '',
                alreadyAssignedResourceIds: this.volunteer?.resourceId
                    ? [Number(this.volunteer.resourceId)]
                    : [],
                allAssignments
            };

            // Set day of week from event start
            const startDate = recentAssignment?.assignmentStartDateTime || recentAssignment?.scheduledEvent?.startDateTime;
            if (startDate) {
                context.eventDayOfWeek = new Date(startDate).getDay();
            }

            this.suggestions = this.suggestionService.getSuggestions(allVolunteers, context, 5);
        } catch (err) {
            console.error('Failed to load suggestions', err);
        } finally {
            this.isLoadingSuggestions = false;
        }
    }

    public navigateToVolunteer(volunteerId: bigint | number | undefined): void {
        if (volunteerId) {
            this.router.navigate(['/volunteers', volunteerId]);
        }
    }
}
