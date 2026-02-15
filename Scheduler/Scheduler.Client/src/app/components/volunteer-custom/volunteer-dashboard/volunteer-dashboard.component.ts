import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, lastValueFrom } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VolunteerProfileService, VolunteerProfileData } from '../../../scheduler-data-services/volunteer-profile.service';
import { VolunteerStatusService, VolunteerStatusData } from '../../../scheduler-data-services/volunteer-status.service';
import { EventResourceAssignmentService, EventResourceAssignmentData } from '../../../scheduler-data-services/event-resource-assignment.service';

interface StatusBreakdown {
    name: string;
    color: string;
    count: number;
    percentage: number;
}

interface RecentActivity {
    volunteerName: string;
    eventName: string;
    date: string;
    hours: number | null;
    status: 'reported' | 'approved' | 'assigned';
}

@Component({
    selector: 'app-volunteer-dashboard',
    templateUrl: './volunteer-dashboard.component.html',
    styleUrls: ['./volunteer-dashboard.component.scss']
})
export class VolunteerDashboardComponent implements OnInit, OnDestroy {

    // KPI values
    public totalActiveVolunteers = 0;
    public totalHoursServed = 0;
    public pendingApprovals = 0;
    public expiringBgChecks = 0;

    // Status breakdown
    public statusBreakdown: StatusBreakdown[] = [];

    // Recent activity
    public recentActivity: RecentActivity[] = [];

    // Loading states
    public isLoading = true;
    public isLoadingActivity = true;

    private destroy$ = new Subject<void>();

    constructor(
        private volunteerProfileService: VolunteerProfileService,
        private volunteerStatusService: VolunteerStatusService,
        private eventResourceAssignmentService: EventResourceAssignmentService
    ) { }

    ngOnInit(): void {
        this.loadDashboardData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private async loadDashboardData(): Promise<void> {
        this.isLoading = true;
        this.isLoadingActivity = true;

        try {
            // Load all volunteer profiles and statuses in parallel
            const [volunteers, statuses] = await Promise.all([
                lastValueFrom(this.volunteerProfileService.GetVolunteerProfileList({ includeRelations: true, active: true, deleted: false })),
                lastValueFrom(this.volunteerStatusService.GetVolunteerStatusList({ active: true, deleted: false }))
            ]);

            // KPI: Total Active
            this.totalActiveVolunteers = volunteers.length;

            // KPI: Total Hours
            this.totalHoursServed = volunteers.reduce((sum, v) => sum + (v.totalHoursServed || 0), 0);

            // KPI: Expiring BG Checks (within 30 days)
            const now = new Date();
            const thirtyDaysFromNow = new Date(now.getTime() + 30 * 24 * 60 * 60 * 1000);
            this.expiringBgChecks = volunteers.filter(v => {
                if (!v.backgroundCheckExpiry) return false;
                const expiry = new Date(v.backgroundCheckExpiry);
                return expiry >= now && expiry <= thirtyDaysFromNow;
            }).length;

            // Status breakdown
            const statusMap = new Map<number, StatusBreakdown>();
            statuses.forEach(s => {
                statusMap.set(Number(s.id), {
                    name: s.name,
                    color: s.color || '#6c757d',
                    count: 0,
                    percentage: 0
                });
            });

            volunteers.forEach(v => {
                const entry = statusMap.get(Number(v.volunteerStatusId));
                if (entry) entry.count++;
            });

            this.statusBreakdown = Array.from(statusMap.values())
                .filter(s => s.count > 0)
                .sort((a, b) => b.count - a.count);

            const totalForPercentage = this.statusBreakdown.reduce((sum, s) => sum + s.count, 0);
            this.statusBreakdown.forEach(s => {
                s.percentage = totalForPercentage > 0 ? Math.round((s.count / totalForPercentage) * 100) : 0;
            });

            this.isLoading = false;

            // Load recent volunteer assignments for activity feed
            this.loadRecentActivity(volunteers);
        } catch (err) {
            console.error('Failed to load dashboard data', err);
            this.isLoading = false;
            this.isLoadingActivity = false;
        }
    }

    private async loadRecentActivity(volunteers: VolunteerProfileData[]): Promise<void> {
        try {
            const assignments = await lastValueFrom(
                this.eventResourceAssignmentService.GetEventResourceAssignmentList({
                    isVolunteer: true,
                    includeRelations: true,
                    active: true,
                    deleted: false,
                    pageSize: 10
                })
            );

            // KPI: Pending approvals
            this.pendingApprovals = assignments.filter(a =>
                a.reportedVolunteerHours != null && a.reportedVolunteerHours > 0 &&
                (a.approvedVolunteerHours == null || a.approvedVolunteerHours === 0)
            ).length;

            // Build recent activity list
            this.recentActivity = assignments
                .sort((a, b) => {
                    const dateA = a.assignmentStartDateTime || a.scheduledEvent?.startDateTime || '';
                    const dateB = b.assignmentStartDateTime || b.scheduledEvent?.startDateTime || '';
                    return dateB.localeCompare(dateA);
                })
                .slice(0, 10)
                .map(a => {
                    let status: 'reported' | 'approved' | 'assigned' = 'assigned';
                    if (a.approvedVolunteerHours != null && a.approvedVolunteerHours > 0) {
                        status = 'approved';
                    } else if (a.reportedVolunteerHours != null && a.reportedVolunteerHours > 0) {
                        status = 'reported';
                    }

                    return {
                        volunteerName: a.resource?.name || 'Unknown',
                        eventName: a.scheduledEvent?.name || 'Untitled Event',
                        date: a.assignmentStartDateTime || a.scheduledEvent?.startDateTime || '',
                        hours: a.reportedVolunteerHours || a.approvedVolunteerHours || null,
                        status
                    };
                });

        } catch (err) {
            console.error('Failed to load recent activity', err);
        } finally {
            this.isLoadingActivity = false;
        }
    }

    public formatHours(value: number | null): string {
        if (value === null || value === undefined) return '0.0';
        return value.toFixed(1);
    }

    public formatDate(dateStr: string): string {
        if (!dateStr) return '—';
        const d = new Date(dateStr);
        return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    }

    public getStatusBadgeClass(status: string): string {
        switch (status) {
            case 'approved': return 'bg-success';
            case 'reported': return 'bg-warning text-dark';
            case 'assigned': return 'bg-secondary';
            default: return 'bg-secondary';
        }
    }
}
