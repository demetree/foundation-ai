//
// Alerting Command Center Overview Component
//
// Premium dashboard providing a holistic view of the alerting system.
// Serves as the landing page after login.
//
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, timer } from 'rxjs';
import { takeUntil, switchMap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { ConfigurationService } from '../../services/configuration.service';
import { AlertService } from '../../services/alert.service';
import { AuthService } from '../../services/auth.service';
import { NavigationService } from '../../utility-services/navigation.service';


import { MessageSeverity } from '../../services/alert.service';

//
// DTO Types matching backend DashboardSummaryDto
//
export interface DashboardSummary {
    status: OperationalStatus;
    generatedAt: Date;
    incidentMetrics: IncidentMetrics;
    onCallSummary: OnCallScheduleSummary[];
    recentActivity: RecentActivity[];
    configCounts: ConfigurationCounts;
    performance: PerformanceMetrics;
    configurationHealth?: ConfigurationHealth;
}

export enum OperationalStatus {
    Healthy = 0,
    Degraded = 1,
    Critical = 2
}

export interface IncidentMetrics {
    activeCount: number;
    triggeredCount: number;
    acknowledgedCount: number;
    resolvedTodayCount: number;
    criticalCount: number;
    highCount: number;
    mediumCount: number;
    lowCount: number;
    infoCount: number;
}

export interface OnCallScheduleSummary {
    scheduleId: number;
    scheduleObjectGuid: string;
    scheduleName: string;
    timezone: string;
    onCallUsers: OnCallUser[];
}

export interface OnCallUser {
    userObjectGuid: string;
    displayName: string;
    email: string;
    layerName: string;
}

export interface RecentActivity {
    id: number;
    timestamp: Date;
    eventType: string;
    incidentId: number;
    incidentTitle: string;
    actorName: string;
    severityName: string;
}

export interface ConfigurationCounts {
    servicesCount: number;
    integrationsCount: number;
    schedulesCount: number;
    policiesCount: number;
}

export interface PerformanceMetrics {
    mttaMinutes: number | null;
    mttrMinutes: number | null;
    mttaTrend: string;
    mttrTrend: string;
    incidentsResolvedLast7Days: number;
}

export interface ConfigurationHealth {
    overallStatus: string;
    fullyConfiguredCount: number;
    partiallyConfiguredCount: number;
    unconfiguredCount: number;
    issues: ConfigurationIssue[];
}

export interface ConfigurationIssue {
    entityType: string;
    entityId: number;
    entityName: string;
    severity: string;
    description: string;
    quickFixRoute: string;
}

@Component({
    selector: 'app-alerting-overview',
    standalone: false,
    templateUrl: './alerting-overview.component.html',
    styleUrl: './alerting-overview.component.scss'
})
export class AlertingOverviewComponent implements OnInit, OnDestroy {
    private destroy$ = new Subject<void>();

    // Dashboard data
    dashboardSummary: DashboardSummary | null = null;
    loading = true;

    // Auto-refresh interval (30 seconds)
    private readonly REFRESH_INTERVAL = 30000;

    // Navigation cards configuration
    readonly navCards = [
        {
            title: 'Incidents',
            icon: 'bi-exclamation-triangle-fill',
            route: '/data/IncidentListing',
            description: 'View and manage active incidents',
            color: '#ef4444'
        },
        {
            title: 'Schedules',
            icon: 'bi-calendar-week',
            route: '/schedule-management',
            description: 'Manage on-call schedules',
            color: '#3b82f6'
        },
        {
            title: 'Services',
            icon: 'bi-gear-fill',
            route: '/service-management',
            description: 'Configure monitored services',
            color: '#10b981'
        },
        {
            title: 'Integrations',
            icon: 'bi-link-45deg',
            route: '/integration-management',
            description: 'Manage alert integrations',
            color: '#8b5cf6'
        },
        {
            title: 'Policies',
            icon: 'bi-diagram-3-fill',
            route: '/escalation-policy-management',
            description: 'Configure escalation policies',
            color: '#f59e0b'
        },
        {
            title: 'My Shift',
            icon: 'bi-person-badge-fill',
            route: '/my-shift',
            description: 'View your on-call status',
            color: '#06b6d4'
        }
    ];

    constructor(
        private http: HttpClient,
        private config: ConfigurationService,
        private alertService: AlertService,
        private authService: AuthService,
        private navigationService: NavigationService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.loadDashboard();
        this.startAutoRefresh();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    /**
     * Load dashboard data from the API
     */
    loadDashboard(): void {
        this.loading = true;
        const headers = this.authService.GetAuthenticationHeaders();

        this.http.get<DashboardSummary>(
            `${this.config.baseUrl}/api/Dashboard/summary`,
            { headers }
        ).pipe(
            takeUntil(this.destroy$),
            catchError(error => {
                console.error('Failed to load dashboard summary', error);
                this.alertService.showErrorMessage('Dashboard Error', 'Failed to load dashboard data');
                this.loading = false;
                return of(null);
            })
        ).subscribe((summary: DashboardSummary | null) => {
            if (summary) {
                this.dashboardSummary = summary;
            }
            this.loading = false;
        });
    }

    /**
     * Start auto-refresh timer
     */
    private startAutoRefresh(): void {
        timer(this.REFRESH_INTERVAL, this.REFRESH_INTERVAL)
            .pipe(
                takeUntil(this.destroy$),
                switchMap(() => {
                    const headers = this.authService.GetAuthenticationHeaders();
                    return this.http.get<DashboardSummary>(
                        `${this.config.baseUrl}/api/Dashboard/summary`,
                        { headers }
                    ).pipe(
                        catchError(error => {
                            console.warn('Auto-refresh failed', error);
                            return of(null);
                        })
                    );
                })
            )
            .subscribe((summary: DashboardSummary | null) => {
                if (summary) {
                    this.dashboardSummary = summary;
                }
            });
    }

    /**
     * Manual refresh
     */
    refresh(): void {
        this.loadDashboard();
    }

    /**
     * Navigate to a route
     */
    navigateTo(route: string): void {
        this.router.navigate([route]);
    }

    /**
     * Navigate to incident viewer
     */
    viewIncident(incidentId: number): void {
        this.router.navigate(['/incidents', incidentId]);
    }

    /**
     * Get status display properties
     */
    getStatusDisplay(): { label: string; class: string; icon: string } {
        if (!this.dashboardSummary) {
            return { label: 'Loading...', class: 'loading', icon: 'bi-hourglass-split' };
        }

        switch (this.dashboardSummary.status) {
            case OperationalStatus.Healthy:
                return { label: 'All Systems Operational', class: 'healthy', icon: 'bi-check-circle-fill' };
            case OperationalStatus.Degraded:
                return { label: 'Minor Issues Detected', class: 'degraded', icon: 'bi-exclamation-triangle-fill' };
            case OperationalStatus.Critical:
                return { label: 'Critical Incidents Active', class: 'critical', icon: 'bi-x-circle-fill' };
            default:
                return { label: 'Unknown', class: 'unknown', icon: 'bi-question-circle' };
        }
    }

    /**
     * Get severity badge class
     */
    getSeverityClass(severity: string): string {
        switch (severity?.toLowerCase()) {
            case 'critical': return 'severity-critical';
            case 'high': return 'severity-high';
            case 'medium': return 'severity-medium';
            case 'low': return 'severity-low';
            case 'info': return 'severity-info';
            default: return 'severity-unknown';
        }
    }

    /**
     * Get trend icon
     */
    getTrendIcon(trend: string): string {
        switch (trend) {
            case 'improving': return 'bi-arrow-down-circle-fill';
            case 'worsening': return 'bi-arrow-up-circle-fill';
            default: return 'bi-dash-circle';
        }
    }

    /**
     * Get trend class
     */
    getTrendClass(trend: string): string {
        switch (trend) {
            case 'improving': return 'trend-improving';
            case 'worsening': return 'trend-worsening';
            default: return 'trend-stable';
        }
    }

    /**
     * Format time ago
     */
    formatTimeAgo(timestamp: Date): string {
        const now = new Date();
        const date = new Date(timestamp);
        const seconds = Math.floor((now.getTime() - date.getTime()) / 1000);

        if (seconds < 60) return 'just now';
        if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
        if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
        return `${Math.floor(seconds / 86400)}d ago`;
    }

    /**
     * Format minutes to display
     */
    formatMinutes(minutes: number | null): string {
        if (minutes === null) return 'N/A';
        if (minutes < 60) return `${Math.round(minutes)}m`;
        return `${Math.floor(minutes / 60)}h ${Math.round(minutes % 60)}m`;
    }

    /**
     * Go back navigation
     */
    goBack(): void {
        this.navigationService.goBack();
    }

    /**
     * Check if can go back
     */
    canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }
}
