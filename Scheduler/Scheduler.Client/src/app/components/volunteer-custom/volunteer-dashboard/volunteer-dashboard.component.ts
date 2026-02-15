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

/**
 * Extensible alert model — designed for future server-side provider.
 * A server endpoint could return DashboardAlert[] which the UI merges
 * with client-computed alerts.
 */
export interface DashboardAlert {
    severity: 'critical' | 'warning' | 'info';
    type: 'bg-expired' | 'bg-expiring' | 'pending-hours' | 'inactive' | 'missing-compliance';
    message: string;
    volunteerName: string;
    volunteerId: number;
    detail?: string;
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

    // Action items / alerts
    public alerts: DashboardAlert[] = [];
    public showAllAlerts = false;
    public readonly ALERTS_PREVIEW_COUNT = 5;

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

            // Compute alerts from volunteer data
            this.computeAlerts(volunteers);

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

    public get visibleAlerts(): DashboardAlert[] {
        return this.showAllAlerts ? this.alerts : this.alerts.slice(0, this.ALERTS_PREVIEW_COUNT);
    }

    public get criticalCount(): number {
        return this.alerts.filter(a => a.severity === 'critical').length;
    }

    public get warningCount(): number {
        return this.alerts.filter(a => a.severity === 'warning').length;
    }

    public getAlertIcon(alert: DashboardAlert): string {
        switch (alert.type) {
            case 'bg-expired': return 'fa-solid fa-shield-xmark';
            case 'bg-expiring': return 'fa-solid fa-shield-halved';
            case 'pending-hours': return 'fa-solid fa-hourglass-half';
            case 'inactive': return 'fa-solid fa-user-clock';
            case 'missing-compliance': return 'fa-solid fa-file-circle-exclamation';
            default: return 'fa-solid fa-circle-info';
        }
    }

    public getAlertColorClass(severity: string): string {
        switch (severity) {
            case 'critical': return 'text-danger';
            case 'warning': return 'text-warning';
            case 'info': return 'text-info';
            default: return 'text-secondary';
        }
    }

    public getAlertBorderClass(severity: string): string {
        switch (severity) {
            case 'critical': return 'alert-item-critical';
            case 'warning': return 'alert-item-warning';
            case 'info': return 'alert-item-info';
            default: return '';
        }
    }

    /**
     * Compute client-side alerts from volunteer data.
     * Future: merge with server-provided alerts from an API endpoint.
     */
    private computeAlerts(volunteers: VolunteerProfileData[]): void {
        const alerts: DashboardAlert[] = [];
        const now = new Date();
        const thirtyDaysFromNow = new Date(now.getTime() + 30 * 24 * 60 * 60 * 1000);
        const ninetyDaysAgo = new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000);

        for (const v of volunteers) {
            const name = v.resource?.name || 'Unknown';
            const id = Number(v.id);

            // Expired BG checks (past due) — critical
            if (v.backgroundCheckExpiry) {
                const expiry = new Date(v.backgroundCheckExpiry);
                if (expiry < now) {
                    alerts.push({
                        severity: 'critical',
                        type: 'bg-expired',
                        message: 'Background check expired',
                        volunteerName: name,
                        volunteerId: id,
                        detail: `Expired ${this.formatDate(v.backgroundCheckExpiry)}`
                    });
                } else if (expiry <= thirtyDaysFromNow) {
                    // Expiring within 30 days — warning
                    alerts.push({
                        severity: 'warning',
                        type: 'bg-expiring',
                        message: 'Background check expiring soon',
                        volunteerName: name,
                        volunteerId: id,
                        detail: `Expires ${this.formatDate(v.backgroundCheckExpiry)}`
                    });
                }
            }

            // Missing compliance — info
            if (v.backgroundCheckCompleted === false && !v.backgroundCheckDate) {
                alerts.push({
                    severity: 'info',
                    type: 'missing-compliance',
                    message: 'Background check not completed',
                    volunteerName: name,
                    volunteerId: id
                });
            }
            if (v.confidentialityAgreementSigned === false && !v.confidentialityAgreementDate) {
                alerts.push({
                    severity: 'info',
                    type: 'missing-compliance',
                    message: 'Confidentiality agreement not signed',
                    volunteerName: name,
                    volunteerId: id
                });
            }

            // Long inactive — info (no activity in 90+ days)
            if (v.lastActivityDate) {
                const lastActivity = new Date(v.lastActivityDate);
                if (lastActivity < ninetyDaysAgo) {
                    alerts.push({
                        severity: 'info',
                        type: 'inactive',
                        message: 'No activity in 90+ days',
                        volunteerName: name,
                        volunteerId: id,
                        detail: `Last active ${this.formatDate(v.lastActivityDate)}`
                    });
                }
            }
        }

        // Sort by severity: critical first, then warning, then info
        const severityOrder: Record<string, number> = { critical: 0, warning: 1, info: 2 };
        alerts.sort((a, b) => (severityOrder[a.severity] ?? 3) - (severityOrder[b.severity] ?? 3));

        this.alerts = alerts;
    }
}
