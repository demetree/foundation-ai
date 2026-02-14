//
// System Health Component (BMC)
//
// Displays real-time system health metrics for the BMC server.
// Ported from Scheduler.Client — simplified for single-server use.
//

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import {
    SystemHealthService,
    SystemHealthStatus,
    DriveInfo,
    TableStatisticsInfo,
    AuthenticatedUsersInfo
} from '../../services/system-health.service';

@Component({
    selector: 'app-system-health',
    templateUrl: './system-health.component.html',
    styleUrl: './system-health.component.scss'
})
export class SystemHealthComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    loading = true;
    lastUpdated: Date | null = null;
    error: string | null = null;

    // Auto-refresh
    autoRefreshEnabled = true;
    autoRefreshInterval = 30;
    autoRefreshCountdown = 30;
    private countdownInterval: ReturnType<typeof setInterval> | null = null;

    // Health data
    healthStatus: SystemHealthStatus | null = null;

    // Table statistics modal
    showTableModal = false;
    tableStatsLoading = false;
    tableStats: TableStatisticsInfo | null = null;
    selectedDatabaseName: string | null = null;
    tableSortColumn: 'tableName' | 'rowCount' | 'sizeMB' = 'rowCount';
    tableSortDirection: 'asc' | 'desc' = 'desc';

    // Authenticated Users panel
    authenticatedUsers: AuthenticatedUsersInfo | null = null;
    usersLoading = false;


    constructor(
        private systemHealthService: SystemHealthService
    ) { }


    ngOnInit(): void {
        this.loadStatus();
        this.loadAuthenticatedUsers();
        this.startAutoRefresh();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.stopAutoRefresh();
    }


    loadStatus(): void {
        this.loading = true;
        this.systemHealthService.getStatus()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (status: SystemHealthStatus) => {
                    this.healthStatus = status;
                    this.lastUpdated = new Date();
                    this.loading = false;
                    this.error = null;
                },
                error: (err: Error) => {
                    console.error('Failed to load system health:', err);
                    this.error = 'Failed to load system health data';
                    this.loading = false;
                }
            });
    }


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


    refresh(): void {
        this.autoRefreshCountdown = this.autoRefreshInterval;
        this.loadStatus();
    }


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
                    this.loadStatus();
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


    // Health status helpers
    getOverallStatus(): 'healthy' | 'warning' | 'critical' | 'unavailable' {
        if (!this.healthStatus) return 'unavailable';

        const systemMemoryPercent = this.healthStatus.application?.memory?.systemPercent;
        const memoryPercent = this.healthStatus.application?.memory?.percent;
        if (systemMemoryPercent !== undefined) {
            if (systemMemoryPercent >= 80) return 'critical';
            if (systemMemoryPercent >= 50) return 'warning';
        } else if (memoryPercent !== undefined) {
            if (memoryPercent >= 80) return 'critical';
            if (memoryPercent >= 50) return 'warning';
        } else {
            const memoryMB = this.healthStatus.application?.memory?.workingSetMB || 0;
            if (memoryMB > 2048) return 'critical';
            if (memoryMB > 1024) return 'warning';
        }

        const drives = this.healthStatus.disk?.drives || [];
        if (drives.some((d: DriveInfo) => d.status === 'Critical')) return 'critical';
        if (drives.some((d: DriveInfo) => d.status === 'Warning')) return 'warning';

        return 'healthy';
    }


    getStatusIcon(status: string): string {
        switch (status?.toLowerCase()) {
            case 'running': case 'configured': case 'healthy':
                return 'fa-circle-check';
            case 'warning':
                return 'fa-triangle-exclamation';
            case 'critical': case 'unavailable': case 'not configured':
                return 'fa-circle-xmark';
            default:
                return 'fa-circle-question';
        }
    }


    getStatusClass(status: string): string {
        switch (status?.toLowerCase()) {
            case 'running': case 'configured': case 'healthy':
                return 'status-healthy';
            case 'warning':
                return 'status-warning';
            case 'critical': case 'unavailable': case 'not configured':
                return 'status-critical';
            default:
                return 'status-unknown';
        }
    }


    getDriveStatusClass(drive: DriveInfo): string {
        if (drive.status === 'Critical') return 'bar-critical';
        if (drive.status === 'Warning') return 'bar-warning';
        return 'bar-healthy';
    }


    formatBytes(mb: number): string {
        if (mb >= 1024) return `${(mb / 1024).toFixed(2)} GB`;
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
        if (bytes >= 1_073_741_824) return `${(bytes / 1_073_741_824).toFixed(2)} GB`;
        if (bytes >= 1_048_576) return `${(bytes / 1_048_576).toFixed(1)} MB`;
        if (bytes >= 1024) return `${(bytes / 1024).toFixed(0)} KB`;
        return `${bytes} B`;
    }


    // Table statistics modal
    openTableModal(databaseName: string): void {
        this.selectedDatabaseName = databaseName;
        this.showTableModal = true;
        this.tableStats = null;
        this.tableStatsLoading = true;

        this.systemHealthService.getTableStatistics(databaseName)
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
                        databaseName, provider: 'Unknown',
                        tables: [], totalTables: 0, totalRows: 0, totalSizeMB: 0,
                        sizeAvailable: false, errorMessage: 'Failed to load table statistics'
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
                case 'tableName': comparison = a.tableName.localeCompare(b.tableName); break;
                case 'rowCount': comparison = a.rowCount - b.rowCount; break;
                case 'sizeMB': comparison = a.sizeMB - b.sizeMB; break;
            }
            return this.tableSortDirection === 'asc' ? comparison : -comparison;
        });
    }


    formatRowCount(count: number): string {
        if (count >= 1000000) return `${(count / 1000000).toFixed(1)}M`;
        if (count >= 1000) return `${(count / 1000).toFixed(1)}K`;
        return count.toLocaleString();
    }
}
