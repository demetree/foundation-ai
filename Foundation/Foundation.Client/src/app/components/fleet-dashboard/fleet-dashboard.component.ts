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

    // Real-time app selector
    selectedRealtimeApp: { name: string; url?: string; isSelf: boolean } | null = null;
    selectedSnapshot: {
        applicationName: string;
        collectedAt: Date;
        isOnline: boolean;
        uptimeSeconds?: number;
        memoryWorkingSetMB?: number;
        memoryGcHeapMB?: number;
        cpuPercent?: number;
        threadPoolWorkerThreads?: number;
        threadPoolPendingWorkItems?: number;
        machineName?: string;
    } | null = null;
    realtimeLoading = false;

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
    autoRefreshInterval = 30;  // Interval in seconds (10, 30, 60, 120)
    autoRefreshCountdown = 30;

    // Historical sub-tabs
    activeHistoricalTab: 'overview' | 'snapshots' | 'runs' = 'overview';


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
        this.loading = false;
        this.error = null;
        this.healthStatus = null;
        this.authenticatedUsers = null;
        this.selectedSnapshot = null;

        // If this is 'self' (current server), use local SystemHealthService
        if (this.selectedRealtimeApp.isSelf) {
            this.loadLocalHealth();
        } else {
            // For remote apps, use the server proxy to get real-time health
            this.loadRemoteHealth(this.selectedRealtimeApp.name);
        }
    }

    private loadLocalHealth(): void {
        this.systemHealthService.getStatus()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (status: SystemHealthStatus) => {
                    this.healthStatus = status;
                    this.realtimeLoading = false;
                    this.lastUpdated = new Date();
                },
                error: (err: Error) => {
                    this.error = 'Failed to load real-time data';
                    this.realtimeLoading = false;
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

        // Also load application business metrics (filtered to the selected 'self' app)
        this.systemHealthService.getApplicationMetrics()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (metrics: ApplicationMetricsResponse) => {
                    // Filter to only show metrics for the currently selected app (the 'self' app's logical name)
                    // The aggregated endpoint returns ALL apps, but in the Real-Time view we only want THIS app's metrics
                    if (this.selectedRealtimeApp && metrics.applications) {
                        const appName = this.selectedRealtimeApp.name;
                        const filteredApps = metrics.applications.filter(app =>
                            app.applicationName.toLowerCase() === appName.toLowerCase());
                        this.appMetrics = {
                            ...metrics,
                            applications: filteredApps
                        };
                    } else {
                        this.appMetrics = metrics;
                    }
                },
                error: (err: Error) => {
                    console.error('Failed to load application metrics:', err);
                }
            });
    }

    private loadRemoteHealth(appName: string): void {
        // Use the server proxy to fetch real-time health from the remote app
        // This uses SystemHealthController which proxies via MonitoredApplicationService (with proper auth)
        this.systemHealthService.getRemoteStatus(appName)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response: any) => {
                    // Check if proxy returned isSelf (shouldn't happen, but handle it)
                    if (response?.isSelf) {
                        this.loadLocalHealth();
                        return;
                    }
                    // The response is the SystemHealthStatus from the remote app
                    this.healthStatus = response;
                    this.realtimeLoading = false;
                    this.lastUpdated = new Date();
                },
                error: (err: any) => {
                    // On error, fall back to showing the telemetry snapshot
                    console.warn('Remote health proxy failed, falling back to snapshot:', err);
                    this.loadFallbackSnapshot(appName);
                }
            });

        // Also try to get authenticated users from remote
        this.systemHealthService.getRemoteUsers(appName)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response: any) => {
                    if (!response?.isSelf) {
                        this.authenticatedUsers = response;
                    }
                },
                error: () => {
                    // Silent fail for users - not critical
                }
            });

        // Also load application business metrics (filtered to the remote app)
        this.systemHealthService.getApplicationMetrics()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (metrics: ApplicationMetricsResponse) => {
                    // Filter to only show metrics for the remote app
                    if (metrics.applications) {
                        const filteredApps = metrics.applications.filter(app =>
                            app.applicationName.toLowerCase() === appName.toLowerCase());
                        this.appMetrics = {
                            ...metrics,
                            applications: filteredApps
                        };
                    } else {
                        this.appMetrics = metrics;
                    }
                },
                error: (err: Error) => {
                    console.error('Failed to load application metrics:', err);
                }
            });
    }

    private loadFallbackSnapshot(appName: string): void {
        // Fallback: Use telemetry snapshot if real-time proxy fails
        if (this.summary?.latestSnapshots) {
            const snapshot = this.summary.latestSnapshots.find(s => s.applicationName === appName);
            if (snapshot) {
                this.selectedSnapshot = snapshot;
                this.lastUpdated = new Date(snapshot.collectedAt);
                this.error = 'Real-time connection unavailable - showing last collected data';
            } else {
                this.error = `Cannot connect to ${appName} - no data available`;
            }
        } else {
            this.error = `Cannot connect to ${appName}`;
        }
        this.realtimeLoading = false;
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
                error: (err: Error) => {
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
        if (this.autoRefreshEnabled) {
            this.startAutoRefresh();
        } else {
            this.stopAutoRefresh();
        }
    }

    setAutoRefreshInterval(seconds: number): void {
        this.autoRefreshInterval = seconds;
        this.autoRefreshCountdown = seconds;
        // If auto-refresh is already running, restart with new interval
        if (this.autoRefreshEnabled) {
            this.stopAutoRefresh();
            this.startAutoRefresh();
        }
    }

    private startAutoRefresh(): void {
        this.autoRefreshCountdown = this.autoRefreshInterval;
        this.autoRefreshSubscription = interval(1000)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
                this.autoRefreshCountdown--;
                if (this.autoRefreshCountdown <= 0) {
                    this.refresh();
                    this.autoRefreshCountdown = this.autoRefreshInterval;
                }
            });
    }

    private stopAutoRefresh(): void {
        if (this.autoRefreshSubscription) {
            this.autoRefreshSubscription.unsubscribe();
            this.autoRefreshSubscription = null;
        }
    }

    refresh(): void {
        if (this.activeMainTab === 'overview') {
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


    // ========================================
    // Health Card Helpers (from system-health)
    // ========================================

    getStatusClass(status: string): string {
        const s = (status || '').toLowerCase();
        if (s === 'healthy' || s === 'connected' || s === 'ok') return 'bg-success';
        if (s === 'warning' || s === 'degraded') return 'bg-warning';
        if (s === 'critical' || s === 'unavailable' || s === 'error') return 'bg-danger';
        return 'bg-secondary';
    }

    getDriveStatusClass(drive: any): string {
        if (!drive) return 'bg-secondary';
        const percent = drive.usedPercent || 0;
        if (percent >= 90) return 'bg-danger';
        if (percent >= 75) return 'bg-warning';
        return 'bg-success';
    }

    getMetricStateClass(state: string | undefined): string {
        if (!state) return '';
        const s = state.toLowerCase();
        if (s === 'healthy' || s === 'ok' || s === 'good') return 'metric-healthy';
        if (s === 'warning' || s === 'degraded') return 'metric-warning';
        if (s === 'critical' || s === 'error') return 'metric-critical';
        return 'metric-unknown';
    }


    // ========================================
    // Table Statistics Modal
    // ========================================

    showTableModal = false;
    selectedDatabaseName = '';
    tableStats: any = null;
    tableStatsLoading = false;
    tableSortColumn = 'tableName';
    tableSortDirection: 'asc' | 'desc' = 'asc';

    openTableModal(databaseName: string): void {
        this.selectedDatabaseName = databaseName;
        this.showTableModal = true;
        this.tableStatsLoading = true;
        this.tableStats = null;

        // For remote apps (not self), pass the appName to proxy to that system
        // Otherwise, query local database directly
        const appName = this.selectedRealtimeApp?.isSelf === false
            ? this.selectedRealtimeApp.name
            : undefined;

        this.systemHealthService.getTableStatistics(databaseName, appName)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (stats: any) => {
                    this.tableStats = stats;
                    this.tableStatsLoading = false;
                },
                error: (err: any) => {
                    this.tableStats = { errorMessage: 'Failed to load table statistics' };
                    this.tableStatsLoading = false;
                }
            });
    }

    closeTableModal(): void {
        this.showTableModal = false;
        this.selectedDatabaseName = '';
        this.tableStats = null;
    }

    sortTables(column: string): void {
        if (this.tableSortColumn === column) {
            this.tableSortDirection = this.tableSortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.tableSortColumn = column;
            this.tableSortDirection = 'asc';
        }
    }

    getSortedTables(): any[] {
        if (!this.tableStats?.tables) return [];
        return [...this.tableStats.tables].sort((a, b) => {
            const aVal = a[this.tableSortColumn];
            const bVal = b[this.tableSortColumn];
            const cmp = aVal < bVal ? -1 : aVal > bVal ? 1 : 0;
            return this.tableSortDirection === 'asc' ? cmp : -cmp;
        });
    }

    formatRowCount(count: number): string {
        if (count >= 1000000) return `${(count / 1000000).toFixed(1)}M`;
        if (count >= 1000) return `${(count / 1000).toFixed(1)}K`;
        return count?.toString() || '0';
    }
}
