//
// Telemetry Dashboard Component
//
// Displays historical telemetry data with trend charts and summary statistics.
// Complements the real-time System Health dashboard with historical views.
//

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import {
    TelemetryService,
    TelemetrySummaryResponse,
    TelemetrySnapshotDto,
    TelemetryApplicationDto,
    TelemetryCollectionRunDto,
    MemoryTrendPoint
} from '../../services/telemetry.service';


@Component({
    selector: 'app-telemetry-dashboard',
    templateUrl: './telemetry-dashboard.component.html',
    styleUrls: ['./telemetry-dashboard.component.scss']
})
export class TelemetryDashboardComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // Loading states
    loading = true;
    error: string | null = null;

    // Data
    summary: TelemetrySummaryResponse | null = null;
    applications: TelemetryApplicationDto[] = [];
    recentSnapshots: TelemetrySnapshotDto[] = [];
    collectionRuns: TelemetryCollectionRunDto[] = [];
    memoryTrends: MemoryTrendPoint[] = [];

    // Filters
    selectedAppName: string = '';
    selectedHours: number = 24;
    hourOptions = [1, 6, 12, 24, 48, 72, 168]; // up to 1 week

    // View state
    activeTab: 'overview' | 'snapshots' | 'runs' = 'overview';


    constructor(private telemetryService: TelemetryService) { }


    ngOnInit(): void {
        this.loadData();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    loadData(): void {
        this.loading = true;
        this.error = null;

        // Load summary
        this.telemetryService.getSummary()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (summary) => {
                    this.summary = summary;
                    this.loading = false;
                },
                error: (err) => {
                    this.error = err.message || 'Failed to load telemetry summary';
                    this.loading = false;
                }
            });

        // Load applications
        this.telemetryService.getApplications()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    this.applications = response.applications;
                },
                error: (err) => {
                    console.error('Failed to load applications', err);
                }
            });

        // Load collection runs
        this.telemetryService.getCollectionRuns(20)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    this.collectionRuns = response.runs;
                },
                error: (err) => {
                    console.error('Failed to load collection runs', err);
                }
            });

        // Load memory trends
        this.loadMemoryTrends();
    }


    loadMemoryTrends(): void {
        const appName = this.selectedAppName || undefined;
        this.telemetryService.getMemoryTrends(appName, this.selectedHours)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    this.memoryTrends = response.data;
                },
                error: (err) => {
                    console.error('Failed to load memory trends', err);
                }
            });
    }


    loadSnapshots(): void {
        const appName = this.selectedAppName || undefined;
        this.telemetryService.getSnapshots(appName, undefined, undefined, 50)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    this.recentSnapshots = response.snapshots;
                },
                error: (err) => {
                    console.error('Failed to load snapshots', err);
                }
            });
    }


    refresh(): void {
        this.loadData();
    }


    setTab(tab: 'overview' | 'snapshots' | 'runs'): void {
        this.activeTab = tab;
        if (tab === 'snapshots') {
            this.loadSnapshots();
        }
    }


    onAppFilterChange(): void {
        this.loadMemoryTrends();
        if (this.activeTab === 'snapshots') {
            this.loadSnapshots();
        }
    }


    onHoursChange(): void {
        this.loadMemoryTrends();
    }


    // Helper methods
    getAppStatusClass(app: any): string {
        const snapshot = this.summary?.latestSnapshots?.find(s => s.applicationName === app.name);
        if (!snapshot) return 'status-unknown';
        return snapshot.isOnline ? 'status-online' : 'status-offline';
    }


    getStatusIcon(isOnline: boolean): string {
        return isOnline ? 'fa-check-circle' : 'fa-times-circle';
    }


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


    formatDate(date: Date | string | undefined): string {
        if (!date) return '-';
        return new Date(date).toLocaleString();
    }


    formatDuration(ms: number | undefined): string {
        if (ms === undefined || ms === null) return '-';
        if (ms < 1000) return `${ms.toFixed(0)}ms`;
        return `${(ms / 1000).toFixed(2)}s`;
    }


    getSuccessRate(run: TelemetryCollectionRunDto): number {
        if (!run.applicationsPolled) return 0;
        return Math.round((run.applicationsSucceeded / run.applicationsPolled) * 100);
    }


    getSuccessClass(run: TelemetryCollectionRunDto): string {
        const rate = this.getSuccessRate(run);
        if (rate >= 100) return 'text-success';
        if (rate >= 50) return 'text-warning';
        return 'text-danger';
    }


    getOnlineCount(): number {
        if (!this.summary?.latestSnapshots) return 0;
        return this.summary.latestSnapshots.filter(s => s.isOnline).length;
    }
}
