//
// Fleet Dashboard Component
//
// Unified operational hub combining real-time system health monitoring
// with historical telemetry data. Provides three views:
// - Fleet Overview: All services at-a-glance
// - This Server: Real-time details for selected application
// - Historical: Trend analysis and historical data
//

import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject, Subscription, interval, forkJoin, of } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { takeUntil } from 'rxjs/operators';
import {
    TelemetryService,
    TelemetrySummaryResponse,
    TelemetrySnapshotDto,
    TelemetryApplicationDto,
    TelemetryCollectionRunDto,
    MemoryTrendPoint
} from '../../services/telemetry.service';
import { SystemHealthService, SystemHealthStatus, AuthenticatedUsersInfo, ApplicationMetricsResponse } from '../../services/system-health.service';


@Component({
    selector: 'app-fleet-dashboard',
    templateUrl: './fleet-dashboard.component.html',
    styleUrls: ['./fleet-dashboard.component.scss']
})
export class FleetDashboardComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private autoRefreshSubscription: Subscription | null = null;

    // Main tab selection
    activeMainTab: 'overview' | 'realtime' | 'historical' = 'overview';

    // Loading states
    loading = true;
    error: string | null = null;

    // Fleet Overview data (from telemetry summary)
    summary: TelemetrySummaryResponse | null = null;

    // Real-time data (from system health)
    healthStatus: SystemHealthStatus | null = null;
    authenticatedUsers: AuthenticatedUsersInfo | null = null;
    appMetrics: ApplicationMetricsResponse | null = null;
    lastUpdated: Date | null = null;

    // Historical data
    recentSnapshots: TelemetrySnapshotDto[] = [];
    collectionRuns: TelemetryCollectionRunDto[] = [];
    memoryTrends: MemoryTrendPoint[] = [];

    // Filters
    selectedAppName: string = '';
    selectedHours: number = 24;
    hourOptions = [1, 6, 12, 24, 48, 72, 168];

    // Auto-refresh
    autoRefreshEnabled = false;
    autoRefreshSeconds = 30;
    autoRefreshCountdown = 30;

    // Historical sub-tabs
    activeHistoricalTab: 'overview' | 'snapshots' | 'runs' = 'overview';

    // Real-time app selector
    selectedRealtimeApp: { name: string; url?: string; isSelf: boolean } | null = null;
    realtimeLoading = false;


    constructor(
        private http: HttpClient,
        private telemetryService: TelemetryService,
        private systemHealthService: SystemHealthService
    ) { }


    ngOnInit(): void {
        this.loadFleetOverview();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.stopAutoRefresh();
    }


    // ========================================
    // Tab Navigation
    // ========================================

    setMainTab(tab: 'overview' | 'realtime' | 'historical'): void {
        this.activeMainTab = tab;
        if (tab === 'overview') {
            this.loadFleetOverview();
        } else if (tab === 'realtime') {
            this.loadRealTimeData();
        } else if (tab === 'historical') {
            this.loadHistoricalData();
        }
    }

    setHistoricalTab(tab: 'overview' | 'snapshots' | 'runs'): void {
        this.activeHistoricalTab = tab;
    }


    // ========================================
    // Fleet Overview (Tab 1)
    // ========================================

    loadFleetOverview(): void {
        this.loading = true;
        this.error = null;

        this.telemetryService.getSummary()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (summary) => {
                    this.summary = summary;
                    this.loading = false;
                    this.lastUpdated = new Date();
                },
                error: (err: Error) => {
                    this.error = 'Failed to load fleet overview';
                    this.loading = false;
                    console.error('Fleet overview error:', err);
                }
            });
    }

    getOnlineCount(): number {
        if (!this.summary?.latestSnapshots) return 0;
        return this.summary.latestSnapshots.filter(s => s.isOnline).length;
    }

    getOfflineCount(): number {
        if (!this.summary?.latestSnapshots) return 0;
        return this.summary.latestSnapshots.filter(s => !s.isOnline).length;
    }

    getTotalApps(): number {
        return this.summary?.applications?.length || 0;
    }

    getAppStatusClass(app: { name: string }): string {
        if (!this.summary?.latestSnapshots) return 'status-unknown';
        const snapshot = this.summary.latestSnapshots.find(s => s.applicationName === app.name);
        if (!snapshot) return 'status-unknown';
        return snapshot.isOnline ? 'status-online' : 'status-offline';
    }

    getStatusIcon(isOnline: boolean): string {
        return isOnline ? 'fa-check-circle' : 'fa-times-circle';
    }


    // ========================================
    // Real-Time Data (Tab 2)
    // ========================================

    selectRealtimeApp(app: { name: string; url?: string; isSelf: boolean }): void {
        this.selectedRealtimeApp = app;
        this.loadRealTimeData();
    }

    loadRealTimeData(): void {
        // If no app selected, try to auto-select from summary
        if (!this.selectedRealtimeApp && this.summary?.applications?.length) {
            // Default to 'self' app or first app
            const selfApp = this.summary.applications.find(a => a.isSelf);
            this.selectedRealtimeApp = selfApp || this.summary.applications[0];
        }

        if (!this.selectedRealtimeApp) {
            this.error = 'No applications available for monitoring';
            return;
        }

        this.realtimeLoading = true;
        this.error = null;
        this.healthStatus = null;
        this.authenticatedUsers = null;

        // If this is 'self' (current server), use the SystemHealthService
        if (this.selectedRealtimeApp.isSelf) {
            this.loadLocalHealth();
        } else if (this.selectedRealtimeApp.url) {
            this.loadRemoteHealth(this.selectedRealtimeApp.url);
        } else {
            this.error = 'No URL configured for this application';
            this.realtimeLoading = false;
        }
    }

    private loadLocalHealth(): void {
        this.systemHealthService.getStatus()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (status: SystemHealthStatus) => {
                    this.healthStatus = status;
                    this.realtimeLoading = false;
                    this.loading = false;
                    this.lastUpdated = new Date();
                },
                error: (err: Error) => {
                    this.error = 'Failed to load real-time data';
                    this.realtimeLoading = false;
                    this.loading = false;
                    console.error('Real-time data error:', err);
                }
            });

        // Also load authenticated users
        this.systemHealthService.getAuthenticatedUsers()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (users: AuthenticatedUsersInfo) => {
                    this.authenticatedUsers = users;
                },
                error: (err: Error) => {
                    console.error('Failed to load authenticated users:', err);
                }
            });
    }

    private loadRemoteHealth(baseUrl: string): void {
        // Call the remote server's /api/SystemHealth endpoint
        const healthUrl = `${baseUrl}/api/SystemHealth`;

        this.http.get<SystemHealthStatus>(healthUrl)
            .pipe(
                takeUntil(this.destroy$),
                catchError(err => {
                    console.error('Remote health error:', err);
                    return of(null);
                })
            )
            .subscribe({
                next: (status) => {
                    if (status) {
                        this.healthStatus = status;
                    } else {
                        this.error = `Could not connect to ${this.selectedRealtimeApp?.name}`;
                    }
                    this.realtimeLoading = false;
                    this.loading = false;
                    this.lastUpdated = new Date();
                }
            });

        // Try to load authenticated users from remote
        const usersUrl = `${baseUrl}/api/SystemHealth/authenticated-users`;
        this.http.get<AuthenticatedUsersInfo>(usersUrl)
            .pipe(
                takeUntil(this.destroy$),
                catchError(() => of(null))
            )
            .subscribe({
                next: (users) => {
                    if (users) {
                        this.authenticatedUsers = users;
                    }
                }
            });
    }
}


// ========================================
// Historical Data (Tab 3)
// ========================================

loadHistoricalData(): void {
    this.loading = true;
    this.error = null;

    const startDate = new Date();
    startDate.setHours(startDate.getHours() - this.selectedHours);

    forkJoin({
        summary: this.telemetryService.getSummary(),
    snapshots: this.telemetryService.getSnapshots(
        this.selectedAppName || undefined,
        startDate,
        undefined,
        100
    ),
    runs: this.telemetryService.getCollectionRuns(50)
})
            .pipe(takeUntil(this.destroy$))
    .subscribe({
        next: (result) => {
            this.summary = result.summary;
            this.recentSnapshots = result.snapshots.snapshots;
            this.collectionRuns = result.runs.runs;
            this.loading = false;
            this.lastUpdated = new Date();
        },
        error: (err) => {
            this.error = 'Failed to load historical data';
            this.loading = false;
            console.error('Historical data error:', err);
        }
    });
    }

onAppFilterChange(): void {
    this.loadHistoricalData();
}

onHoursChange(): void {
    this.loadHistoricalData();
}


// ========================================
// Auto-Refresh
// ========================================

toggleAutoRefresh(): void {
    this.autoRefreshEnabled = !this.autoRefreshEnabled;
    if(this.autoRefreshEnabled) {
    this.startAutoRefresh();
} else {
    this.stopAutoRefresh();
}
    }

    private startAutoRefresh(): void {
    this.autoRefreshCountdown = this.autoRefreshSeconds;
    this.autoRefreshSubscription = interval(1000)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
            this.autoRefreshCountdown--;
            if (this.autoRefreshCountdown <= 0) {
                this.refresh();
                this.autoRefreshCountdown = this.autoRefreshSeconds;
            }
        });
}

    private stopAutoRefresh(): void {
    if(this.autoRefreshSubscription) {
    this.autoRefreshSubscription.unsubscribe();
    this.autoRefreshSubscription = null;
}
    }

refresh(): void {
    if(this.activeMainTab === 'overview') {
    this.loadFleetOverview();
} else if (this.activeMainTab === 'realtime') {
    this.loadRealTimeData();
} else if (this.activeMainTab === 'historical') {
    this.loadHistoricalData();
}
    }


// ========================================
// Formatting Helpers
// ========================================

formatUptime(seconds: number | undefined): string {
    if (!seconds) return '-';
    const days = Math.floor(seconds / 86400);
    const hours = Math.floor((seconds % 86400) / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    if (days > 0) return `${days}d ${hours}h`;
    if (hours > 0) return `${hours}h ${mins}m`;
    return `${mins}m`;
}

formatMemory(mb: number | undefined): string {
    if (mb === undefined || mb === null) return '-';
    return `${mb.toFixed(1)} MB`;
}

formatCpu(percent: number | undefined): string {
    if (percent === undefined || percent === null) return '-';
    return `${percent.toFixed(1)}%`;
}

formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleString();
}

formatRelativeTime(date: Date | string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;
    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays}d ago`;
}

formatDuration(ms: number | undefined): string {
    if (!ms) return '-';
    if (ms < 1000) return `${ms}ms`;
    return `${(ms / 1000).toFixed(1)}s`;
}

getSuccessRate(run: TelemetryCollectionRunDto): number {
    if (!run.applicationsPolled) return 0;
    return Math.round((run.applicationsSucceeded / run.applicationsPolled) * 100);
}

getSuccessClass(run: TelemetryCollectionRunDto): string {
    const rate = this.getSuccessRate(run);
    if (rate === 100) return 'text-success';
    if (rate >= 50) return 'text-warning';
    return 'text-danger';
}
}
