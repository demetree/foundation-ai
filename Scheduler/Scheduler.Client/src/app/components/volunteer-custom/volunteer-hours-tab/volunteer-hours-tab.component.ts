import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { VolunteerProfileData, VolunteerProfileService } from '../../../scheduler-data-services/volunteer-profile.service';
import { EventResourceAssignmentData, EventResourceAssignmentService, EventResourceAssignmentSubmitData } from '../../../scheduler-data-services/event-resource-assignment.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CurrentUserService } from '../../../services/current-user.service';
import { ContactData } from '../../../scheduler-data-services/contact.service';

/**
 * Hours dashboard tab for the Volunteer detail page.
 *
 * Shows summary cards (total reported, approved, pending, reimbursement),
 * a date range filter, and a detailed hours table with inline approval.
 */
@Component({
    selector: 'app-volunteer-hours-tab',
    templateUrl: './volunteer-hours-tab.component.html',
    styleUrls: ['./volunteer-hours-tab.component.scss']
})
export class VolunteerHoursTabComponent implements OnChanges {

    @Input() volunteer!: VolunteerProfileData | null;

    public allAssignments: EventResourceAssignmentData[] = [];
    public filteredAssignments: EventResourceAssignmentData[] = [];
    public isLoading = true;
    public error: string | null = null;
    public dateRange: string = 'all';

    // Summary stats
    public totalReportedHours = 0;
    public totalApprovedHours = 0;
    public pendingApprovalCount = 0;
    public totalReimbursement = 0;
    public pendingReimbursementCount = 0;

    constructor(
        private router: Router,
        private eventResourceAssignmentService: EventResourceAssignmentService,
        private volunteerProfileService: VolunteerProfileService,
        private alertService: AlertService,
        private currentUserService: CurrentUserService
    ) { }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['volunteer'] && this.volunteer) {
            this.loadAssignments();
        }
    }

    /**
     * Loads all volunteer assignments and computes summary statistics.
     */
    public async loadAssignments(): Promise<void> {
        if (!this.volunteer || !this.volunteer.resourceId) {
            this.allAssignments = [];
            this.filteredAssignments = [];
            this.isLoading = false;
            return;
        }

        this.isLoading = true;
        this.error = null;

        try {
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

            this.applyDateFilter();
            this.isLoading = false;
        } catch (err) {
            console.error('Failed to load volunteer hours', err);
            this.error = 'Unable to load volunteer hours';
            this.allAssignments = [];
            this.filteredAssignments = [];
            this.isLoading = false;
        }
    }

    /**
     * Applies the selected date range filter and recalculates summary stats.
     */
    public applyDateFilter(): void {
        const now = new Date();
        let cutoff: Date | null = null;

        switch (this.dateRange) {
            case 'week':
                cutoff = new Date(now);
                cutoff.setDate(cutoff.getDate() - 7);
                break;
            case 'month':
                cutoff = new Date(now);
                cutoff.setMonth(cutoff.getMonth() - 1);
                break;
            case 'year':
                cutoff = new Date(now);
                cutoff.setFullYear(cutoff.getFullYear() - 1);
                break;
            default:
                cutoff = null;
                break;
        }

        if (cutoff !== null) {
            this.filteredAssignments = this.allAssignments.filter(a => {
                const dateStr = a.assignmentStartDateTime || a.scheduledEvent?.startDateTime;
                if (!dateStr) {
                    return false;
                }
                return new Date(dateStr).getTime() >= cutoff!.getTime();
            });
        } else {
            this.filteredAssignments = [...this.allAssignments];
        }

        this.computeSummary();
    }

    /**
     * Sets the date range filter and reapplies.
     */
    public setDateRange(range: string): void {
        this.dateRange = range;
        this.applyDateFilter();
    }

    /**
     * Computes summary statistics from the filtered assignments.
     */
    private computeSummary(): void {
        this.totalReportedHours = 0;
        this.totalApprovedHours = 0;
        this.pendingApprovalCount = 0;
        this.totalReimbursement = 0;
        this.pendingReimbursementCount = 0;

        for (const a of this.filteredAssignments) {
            if (a.reportedVolunteerHours !== null && a.reportedVolunteerHours !== undefined) {
                this.totalReportedHours += a.reportedVolunteerHours;
            }
            if (a.approvedVolunteerHours !== null && a.approvedVolunteerHours !== undefined) {
                this.totalApprovedHours += a.approvedVolunteerHours;
            }

            // Pending = has reported hours but no approved hours
            const hasReported = a.reportedVolunteerHours !== null && a.reportedVolunteerHours !== undefined && a.reportedVolunteerHours > 0;
            const hasApproved = a.approvedVolunteerHours !== null && a.approvedVolunteerHours !== undefined && a.approvedVolunteerHours > 0;
            if (hasReported && !hasApproved) {
                this.pendingApprovalCount++;
            }

            if (a.reimbursementAmount !== null && a.reimbursementAmount !== undefined) {
                this.totalReimbursement += a.reimbursementAmount;
            }

            if (a.reimbursementRequested === true && (a.reimbursementAmount === null || a.reimbursementAmount === undefined || a.reimbursementAmount === 0)) {
                this.pendingReimbursementCount++;
            }
        }
    }

    /**
     * Approves the reported hours for an assignment.
     * Sets approvedVolunteerHours = reportedVolunteerHours and records the approval timestamp.
     */
    public async approveHours(assignment: EventResourceAssignmentData): Promise<void> {
        if (!assignment.reportedVolunteerHours) {
            return;
        }

        try {
            const submitData = assignment.ConvertToSubmitData();
            submitData.approvedVolunteerHours = assignment.reportedVolunteerHours;
            submitData.approvedDateTime = new Date().toISOString();

            // Set the approving contact if available
            if (this.currentUserService.contact$) {
                try {
                    const contact = await lastValueFrom(this.currentUserService.contact$);
                    if (contact && contact.id) {
                        submitData.hoursApprovedByContactId = contact.id;
                    }
                } catch { /* proceed without contact id */ }
            }

            const updated = await lastValueFrom(
                this.eventResourceAssignmentService.PutEventResourceAssignment(assignment.id, submitData)
            );

            // Update the local data
            assignment.approvedVolunteerHours = updated.approvedVolunteerHours;
            assignment.approvedDateTime = updated.approvedDateTime;
            assignment.hoursApprovedByContactId = updated.hoursApprovedByContactId;

            this.computeSummary();

            this.alertService.showMessage('Success', 'Hours approved successfully.', MessageSeverity.success);
        } catch (err) {
            console.error('Failed to approve hours', err);
            this.alertService.showMessage('Error', 'Failed to approve hours.', MessageSeverity.error);
        }
    }

    /**
     * Checks if the current user can write volunteer data.
     */
    public userIsVolunteerWriter(): boolean {
        return this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter();
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
     * Formats a date string for display (date only, no time).
     */
    public formatDate(dateStr: string | null | undefined): string {
        if (!dateStr) {
            return '—';
        }
        return new Date(dateStr).toLocaleDateString(undefined, {
            dateStyle: 'medium'
        });
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
     * Formats a currency amount for display.
     */
    public formatCurrency(amount: number | null | undefined): string {
        if (amount === null || amount === undefined) {
            return '—';
        }
        return '$' + amount.toFixed(2);
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
     * Checks if an assignment has pending (unapproved) hours.
     */
    public isPendingApproval(assignment: EventResourceAssignmentData): boolean {
        const hasReported = assignment.reportedVolunteerHours !== null && assignment.reportedVolunteerHours !== undefined && assignment.reportedVolunteerHours > 0;
        const hasApproved = assignment.approvedVolunteerHours !== null && assignment.approvedVolunteerHours !== undefined && assignment.approvedVolunteerHours > 0;
        return hasReported && !hasApproved;
    }
}
