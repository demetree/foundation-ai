//
// System Health Component
//
// Displays real-time system health metrics for administrators.
// Supports monitoring multiple Foundation-based applications via tabs.
//

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

import {
    SystemHealthService,
    SystemHealthStatus,
    DriveInfo,
    MonitoredApplicationsService,
    MonitoredApplicationStatus,
    TableStatisticsInfo,
    AuthenticatedUsersInfo,
    ApplicationMetricsResponse,
    ApplicationMetricItem
} from '../../services/system-health.service';
import { AlertService } from '../../services/alert.service';


@Component({
    selector: 'app-system-health',
    templateUrl: './system-health.component.html',
    styleUrls: ['./system-health.component.scss']
})
export class SystemHealthComponent implements OnInit, OnDestroy {

    //
    // Lifecycle management
    //
    private destroy$ = new Subject<void>();

    //
    // Loading state
    //
    loading = true;
    lastUpdated: Date | null = null;
    error: string | null = null;

    //
    // Auto-refresh
    //
    autoRefreshEnabled = true;
    autoRefreshInterval = 30; // seconds
    autoRefreshCountdown = 30;
    private countdownInterval: ReturnType<typeof setInterval> | null = null;

    //
    // Multi-application support
    //
    applications: MonitoredApplicationStatus[] = [];
    selectedAppIndex = 0;
    healthStatus: SystemHealthStatus | null = null;

    //
    // Table statistics modal
    //
    showTableModal = false;
    tableStatsLoading = false;
    tableStats: TableStatisticsInfo | null = null;
    selectedDatabaseName: string | null = null;
    tableSortColumn: 'tableName' | 'rowCount' | 'sizeMB' = 'rowCount';
    tableSortDirection: 'asc' | 'desc' = 'desc';

    //
    // Authenticated Users panel
    //
    authenticatedUsers: AuthenticatedUsersInfo | null = null;
    usersLoading = false;
    showUsersPanel = true;
    revokingSessionId: number | null = null;

    //
    // Application Metrics panel
    //
    appMetrics: ApplicationMetricsResponse | null = null;
    metricsLoading = false;
    showMetricsPanel = true;


    constructor(
        private systemHealthService: SystemHealthService,
        private monitoredAppsService: MonitoredApplicationsService,
        private modalService: NgbModal,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        this.loadAllApplications();
        this.loadAuthenticatedUsers();
        this.loadApplicationMetrics();
        this.startAutoRefresh();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.stopAutoRefresh();
    }


    //
    // Data loading
    //
    loadAllApplications(): void {
        this.loading = true;
        this.monitoredAppsService.getAllStatus()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (apps: MonitoredApplicationStatus[]) => {
                    this.applications = apps;
                    this.lastUpdated = new Date();
                    this.loading = false;
                    this.error = null;

                    //
                    // Set the health data for the selected app
                    //
                    this.updateSelectedAppHealth();
                },
                error: (err: Error) => {
                    console.error('Failed to load applications:', err);
                    this.error = 'Failed to load application health data';
                    this.loading = false;
                }
            });
    }


    updateSelectedAppHealth(): void {
        if (this.applications.length > 0 && this.selectedAppIndex < this.applications.length) {
            const selectedApp = this.applications[this.selectedAppIndex];
            this.healthStatus = selectedApp.healthData || null;
        }
    }


    //
    // Load authenticated users
    //
    loadAuthenticatedUsers(): void {
        this.usersLoading = true;
        this.systemHealthService.getAuthenticatedUsers()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (users: AuthenticatedUsersInfo) => {
                    this.authenticatedUsers = users;
                    this.usersLoading = false;
                },
                error: (err: Error) => {
                    console.error('Failed to load authenticated users:', err);
                    this.usersLoading = false;
                }
            });
    }



    //
    // Load application metrics
    //
    loadApplicationMetrics(): void {
        this.metricsLoading = true;
        this.systemHealthService.getApplicationMetrics()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (metrics: ApplicationMetricsResponse) => {
                    this.appMetrics = metrics;
                    this.metricsLoading = false;
                },
                error: (err: Error) => {
                    console.error('Failed to load application metrics:', err);
                    this.metricsLoading = false;
                }
            });
    }


    //
    // Get CSS class for metric state
    //
    getMetricStateClass(state: string): string {
        switch (state?.toLowerCase()) {
            case 'healthy': return 'metric-healthy';
            case 'warning': return 'metric-warning';
            case 'critical': return 'metric-critical';
            default: return 'metric-unknown';
        }
    }


    selectApplication(index: number): void {
        this.selectedAppIndex = index;
        this.updateSelectedAppHealth();
    }


    refresh(): void {
        this.autoRefreshCountdown = this.autoRefreshInterval;
        this.loadAllApplications();
    }


    //
    // Auto-refresh controls
    //
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

        if (this.autoRefreshEnabled) {
            this.stopAutoRefresh();
            this.startAutoRefresh();
        }
    }


    private startAutoRefresh(): void {
        this.autoRefreshCountdown = this.autoRefreshInterval;

        this.countdownInterval = setInterval(() => {
            if (this.autoRefreshEnabled) {
                this.autoRefreshCountdown--;

                if (this.autoRefreshCountdown <= 0) {
                    this.loadAllApplications();
                    this.autoRefreshCountdown = this.autoRefreshInterval;
                }
            }
        }, 1000);
    }


    private stopAutoRefresh(): void {
        if (this.countdownInterval) {
            clearInterval(this.countdownInterval);
            this.countdownInterval = null;
        }
    }


    //
    // Application status helpers
    //
    getAppStatusClass(app: MonitoredApplicationStatus): string {
        if (!app.isAvailable) return 'text-danger';
        if (app.status === 'Healthy') return 'text-success';
        if (app.status === 'Warning') return 'text-warning';
        return 'text-muted';
    }


    getAppStatusIcon(app: MonitoredApplicationStatus): string {
        if (!app.isAvailable) return 'fa-circle-xmark';
        if (app.status === 'Healthy') return 'fa-circle-check';
        if (app.status === 'Warning') return 'fa-triangle-exclamation';
        return 'fa-circle-question';
    }


    //
    // Health status helpers
    //
    getOverallStatus(): 'healthy' | 'warning' | 'critical' | 'unavailable' {
        const selectedApp = this.applications[this.selectedAppIndex];
        if (!selectedApp?.isAvailable) return 'unavailable';
        if (!this.healthStatus) return 'healthy';

        //
        // Check system-wide memory percentage (warning at 50%, critical at 80%)
        // Falls back to process percent, then MB thresholds
        //
        const systemMemoryPercent = this.healthStatus.application?.memory?.systemPercent;
        const memoryPercent = this.healthStatus.application?.memory?.percent;
        if (systemMemoryPercent !== undefined) {
            if (systemMemoryPercent >= 80) return 'critical';
            if (systemMemoryPercent >= 50) return 'warning';
        } else if (memoryPercent !== undefined) {
            if (memoryPercent >= 80) return 'critical';
            if (memoryPercent >= 50) return 'warning';
        } else {
            // Fallback to MB thresholds
            const memoryMB = this.healthStatus.application?.memory?.workingSetMB || 0;
            if (memoryMB > 2048) return 'critical';
            if (memoryMB > 1024) return 'warning';
        }

        //
        // Check disk
        //
        const drives = this.healthStatus.disk?.drives || [];
        const criticalDrives = drives.filter((d: DriveInfo) => d.status === 'Critical');
        if (criticalDrives.length > 0) return 'critical';

        const warningDrives = drives.filter((d: DriveInfo) => d.status === 'Warning');
        if (warningDrives.length > 0) return 'warning';

        return 'healthy';
    }


    getStatusIcon(status: string): string {
        switch (status?.toLowerCase()) {
            case 'running':
            case 'configured':
            case 'healthy':
                return 'fa-circle-check';
            case 'warning':
                return 'fa-triangle-exclamation';
            case 'critical':
            case 'unavailable':
            case 'not configured':
                return 'fa-circle-xmark';
            default:
                return 'fa-circle-question';
        }
    }


    getStatusClass(status: string): string {
        switch (status?.toLowerCase()) {
            case 'running':
            case 'configured':
            case 'healthy':
                return 'text-success';
            case 'warning':
                return 'text-warning';
            case 'critical':
            case 'unavailable':
            case 'not configured':
                return 'text-danger';
            default:
                return 'text-muted';
        }
    }


    getDriveStatusClass(drive: DriveInfo): string {
        if (drive.status === 'Critical') return 'bg-danger';
        if (drive.status === 'Warning') return 'bg-warning';
        return 'bg-success';
    }


    formatBytes(mb: number): string {
        if (mb >= 1024) {
            return `${(mb / 1024).toFixed(2)} GB`;
        }
        return `${mb.toFixed(0)} MB`;
    }


    formatCpu(percent: number | undefined): string {
        if (percent === undefined || percent === null) return '-';
        return `${percent.toFixed(1)}%`;
    }


    formatPercent(percent: number | undefined): string {
        if (percent === undefined || percent === null) return '-';
        return `${percent.toFixed(1)}%`;
    }


    formatNetworkBytes(bytes: number): string {
        if (bytes >= 1_073_741_824) {
            return `${(bytes / 1_073_741_824).toFixed(2)} GB`;
        }
        if (bytes >= 1_048_576) {
            return `${(bytes / 1_048_576).toFixed(1)} MB`;
        }
        if (bytes >= 1024) {
            return `${(bytes / 1024).toFixed(0)} KB`;
        }
        return `${bytes} B`;
    }

    //
    // Table statistics modal
    //
    openTableModal(databaseName: string): void {
        this.selectedDatabaseName = databaseName;
        this.showTableModal = true;
        this.tableStats = null;
        this.tableStatsLoading = true;

        //
        // Get the selected application name for remote app proxying
        //
        const selectedApp = this.applications[this.selectedAppIndex];
        const appName = selectedApp?.name;

        this.systemHealthService.getTableStatistics(databaseName, appName)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (stats: TableStatisticsInfo) => {
                    this.tableStats = stats;
                    this.tableStatsLoading = false;
                },
                error: (err: Error) => {
                    console.error('Failed to load table statistics:', err);
                    this.tableStatsLoading = false;
                    this.tableStats = {
                        databaseName: databaseName,
                        provider: 'Unknown',
                        tables: [],
                        totalTables: 0,
                        totalRows: 0,
                        totalSizeMB: 0,
                        sizeAvailable: false,
                        errorMessage: 'Failed to load table statistics'
                    };
                }
            });
    }


    closeTableModal(): void {
        this.showTableModal = false;
        this.tableStats = null;
        this.selectedDatabaseName = null;
    }


    sortTables(column: 'tableName' | 'rowCount' | 'sizeMB'): void {
        if (this.tableSortColumn === column) {
            this.tableSortDirection = this.tableSortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.tableSortColumn = column;
            this.tableSortDirection = column === 'tableName' ? 'asc' : 'desc';
        }
    }


    getSortedTables() {
        if (!this.tableStats?.tables) return [];

        return [...this.tableStats.tables].sort((a, b) => {
            let comparison = 0;

            switch (this.tableSortColumn) {
                case 'tableName':
                    comparison = a.tableName.localeCompare(b.tableName);
                    break;
                case 'rowCount':
                    comparison = a.rowCount - b.rowCount;
                    break;
                case 'sizeMB':
                    comparison = a.sizeMB - b.sizeMB;
                    break;
            }

            return this.tableSortDirection === 'asc' ? comparison : -comparison;
        });
    }


    formatRowCount(count: number): string {
        if (count >= 1000000) {
            return `${(count / 1000000).toFixed(1)}M`;
        }
        if (count >= 1000) {
            return `${(count / 1000).toFixed(1)}K`;
        }
        return count.toLocaleString();
    }
}

