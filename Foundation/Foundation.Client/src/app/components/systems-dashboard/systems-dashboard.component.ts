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
import { ChartConfiguration, ChartData } from 'chart.js';
import {
    TelemetryService,
    TelemetrySummaryResponse,
    TelemetrySnapshotDto,
    TelemetryApplicationDto,
    TelemetryCollectionRunDto,
    MemoryTrendPoint,
    CpuTrendPoint,
    NetworkTrendPoint,
    SystemMemoryTrendPoint,
    SystemCpuTrendPoint,
    SnapshotDetailDto,
    FleetMetricsResponse,
    MetricTrendPoint
} from '../../services/telemetry.service';
import { SystemHealthService, SystemHealthStatus, AuthenticatedUsersInfo, ApplicationMetricsResponse } from '../../services/system-health.service';


@Component({
    selector: 'app-systems-dashboard',
    templateUrl: './systems-dashboard.component.html',
    styleUrls: ['./systems-dashboard.component.scss']
})
export class SystemsDashboardComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private autoRefreshSubscription: Subscription | null = null;

    // Main tab selection
    activeMainTab: 'overview' | 'realtime' | 'historical' = 'overview';

    // Loading states
    loading = true;
    initialLoadComplete = false;  // Track if initial load is done (to avoid spinner during refresh)
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
        systemMemoryPercent?: number;
        systemCpuPercent?: number;
    } | null = null;
    realtimeLoading = false;

    // Historical data
    recentSnapshots: TelemetrySnapshotDto[] = [];
    collectionRuns: TelemetryCollectionRunDto[] = [];
    memoryTrends: MemoryTrendPoint[] = [];

    // Fleet aggregates
    fleetMetrics: FleetMetricsResponse | null = null;

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

    // Chart data
    cpuTrends: CpuTrendPoint[] = [];
    networkTrends: NetworkTrendPoint[] = [];
    systemMemoryTrends: SystemMemoryTrendPoint[] = [];
    systemCpuTrends: SystemCpuTrendPoint[] = [];
    trendLoading = false;

    // Memory chart configuration
    memoryChartData: ChartData<'line'> = { labels: [], datasets: [] };
    cpuChartData: ChartData<'line'> = { labels: [], datasets: [] };
    networkChartData: ChartData<'line'> = { labels: [], datasets: [] };
    lineChartType = 'line' as const;
    chartOptions: ChartConfiguration<'line'>['options'] = {
        responsive: true,
        maintainAspectRatio: false,
        animation: false,  // Disable animation on data updates to prevent flicker
        plugins: {
            legend: {
                display: true,
                position: 'bottom'
            },
            tooltip: {
                mode: 'index',
                intersect: false
            }
        },
        scales: {
            x: {
                display: true,
                title: { display: true, text: 'Time' }
            },
            y: {
                display: true,
                beginAtZero: true
            }
        },
        elements: {
            point: { radius: 2 },
            line: { tension: 0.3 }
        }
    };

    // Sparkline config (minimal, inline charts)
    sparklineOptions: ChartConfiguration<'line'>['options'] = {
        responsive: true,
        maintainAspectRatio: false,
        animation: false,  // Disable animation on data updates to prevent flicker
        plugins: { legend: { display: false }, tooltip: { enabled: false } },
        scales: { x: { display: false }, y: { display: false, beginAtZero: true } },
        elements: { point: { radius: 0 }, line: { borderWidth: 2, tension: 0.4 } }
    };

    // Metric trend sparkline data
    metricTrends: Map<string, MetricTrendPoint[]> = new Map();
    metricSparklines: Map<string, ChartData<'line'>> = new Map();

    // System metric sparklines (memory + CPU + disk)
    memorySparkline: ChartData<'line'> | null = null;
    cpuSparkline: ChartData<'line'> | null = null;
    diskSparkline: ChartData<'line'> | null = null;
    networkSparkline: ChartData<'line'> | null = null;
    systemMemorySparkline: ChartData<'line'> | null = null;
    systemCpuSparkline: ChartData<'line'> | null = null;

    // Sparkline time range controls
    sparklineHours = 24;
    sparklineHourOptions = [1, 6, 24];

    // Last refresh tracking
    lastRefreshTime: Date | null = null;
    refreshing = false;

    // Per-app sparklines and health scores
    appSparklines: Map<string, ChartData<'line'>> = new Map();
    appHealthScores: Map<string, number> = new Map();

    // Snapshot detail modal
    selectedSnapshotDetail: SnapshotDetailDto | null = null;
    snapshotDetailLoading = false;
    logErrorFilter = '';
    logErrorLevelFilter = 'ALL';

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
        //
        // Only show loading spinner on initial load, not during refresh
        //
        if (!this.initialLoadComplete) {
            this.loading = true;
        }
        this.error = null;

        this.telemetryService.getSummary()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (summary) => {
                    this.summary = summary;
                    this.loading = false;
                    this.initialLoadComplete = true;
                    this.lastUpdated = new Date();
                    // Load per-app data
                    this.loadAppSparklines();
                    this.initHealthScores();
                },
                error: (err: Error) => {
                    this.error = 'Failed to load fleet overview';
                    this.loading = false;
                    console.error('Fleet overview error:', err);
                }
            });

        // Also load application business metrics for the overview
        this.systemHealthService.getApplicationMetrics()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (metrics: ApplicationMetricsResponse) => {
                    this.appMetrics = metrics;
                },
                error: (err: Error) => {
                    console.error('Failed to load application metrics:', err);
                }
            });

        // Load fleet aggregates
        this.telemetryService.getFleetMetrics()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (metrics) => {
                    this.fleetMetrics = metrics;
                    // Load sparklines for each metric
                    this.loadMetricSparklines();
                },
                error: (err: Error) => {
                    console.error('Failed to load fleet metrics:', err);
                }
            });
    }

    /**
     * Load metric trend data and build sparkline chart data
     */
    loadMetricSparklines(): void {
        if (!this.fleetMetrics?.metrics) return;

        // Load trends for each business metric
        this.fleetMetrics.metrics.forEach(metric => {
            this.telemetryService.getMetricTrends(undefined, metric.metricName, this.sparklineHours, 50)
                .pipe(takeUntil(this.destroy$))
                .subscribe({
                    next: (response) => {
                        this.metricTrends.set(metric.metricName, response.data);
                        // Build sparkline chart data - reverse for chronological order
                        const sorted = [...response.data].sort((a, b) =>
                            new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
                        this.metricSparklines.set(metric.metricName, {
                            labels: sorted.map(() => ''),
                            datasets: [{
                                data: sorted.map(t => t.value),
                                borderColor: '#17a2b8',
                                backgroundColor: 'rgba(23, 162, 184, 0.1)',
                                fill: true
                            }]
                        });
                    },
                    error: (err: Error) => {
                        console.error(`Failed to load trends for ${metric.metricName}:`, err);
                    }
                });
        });

        // Also load system metric sparklines (memory + CPU + disk)
        this.loadSystemSparklines();

        // Track refresh time
        this.lastRefreshTime = new Date();
    }

    /**
     * Refresh all sparklines (called by refresh button or time range change)
     */
    refreshSparklines(): void {
        this.refreshing = true;
        // Clear existing data
        this.metricSparklines.clear();
        this.memorySparkline = null;
        this.cpuSparkline = null;
        this.diskSparkline = null;
        // Reload
        this.loadMetricSparklines();
        setTimeout(() => this.refreshing = false, 1000);
    }

    /**
     * Handle sparkline time range change
     */
    onSparklineHoursChange(hours: number): void {
        this.sparklineHours = hours;
        this.refreshSparklines();
    }

    /**
     * Load memory and CPU trend sparklines
     */
    loadSystemSparklines(): void {
        // Load memory trends
        this.telemetryService.getMemoryTrends(undefined, this.sparklineHours)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    const sorted = [...response.data].sort((a, b) =>
                        new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
                    this.memorySparkline = {
                        labels: sorted.map(() => ''),
                        datasets: [{
                            data: sorted.map(t => t.workingSetMB ?? 0),
                            borderColor: '#28a745',
                            backgroundColor: 'rgba(40, 167, 69, 0.1)',
                            fill: true
                        }]
                    };
                },
                error: (err: Error) => {
                    console.error('Failed to load memory trends:', err);
                }
            });

        // Load CPU trends
        this.telemetryService.getCpuTrends(undefined, this.sparklineHours)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    const sorted = [...response.data].sort((a, b) =>
                        new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
                    this.cpuSparkline = {
                        labels: sorted.map(() => ''),
                        datasets: [{
                            data: sorted.map(t => t.cpuPercent ?? 0),
                            borderColor: '#ffc107',
                            backgroundColor: 'rgba(255, 193, 7, 0.1)',
                            fill: true
                        }]
                    };
                },
                error: (err: Error) => {
                    console.error('Failed to load CPU trends:', err);
                }
            });

        // Load disk trends
        this.telemetryService.getDiskTrends(undefined, this.sparklineHours)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    const sorted = [...response.data].sort((a, b) =>
                        new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
                    this.diskSparkline = {
                        labels: sorted.map(() => ''),
                        datasets: [{
                            data: sorted.map(t => t.freePercent ?? 0),
                            borderColor: '#6f42c1',
                            backgroundColor: 'rgba(111, 66, 193, 0.1)',
                            fill: true
                        }]
                    };
                },
                error: (err: Error) => {
                    console.error('Failed to load disk trends:', err);
                }
            });

        // Load system memory trends
        this.telemetryService.getSystemMemoryTrends(undefined, this.sparklineHours)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    const sorted = [...response.data].sort((a, b) =>
                        new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
                    this.systemMemorySparkline = {
                        labels: sorted.map(() => ''),
                        datasets: [{
                            data: sorted.map(t => t.systemMemoryPercent ?? 0),
                            borderColor: '#17a2b8',
                            backgroundColor: 'rgba(23, 162, 184, 0.1)',
                            fill: true
                        }]
                    };
                },
                error: (err: Error) => {
                    console.error('Failed to load system memory trends:', err);
                }
            });

        // Load system CPU trends
        this.telemetryService.getSystemCpuTrends(undefined, this.sparklineHours)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    const sorted = [...response.data].sort((a, b) =>
                        new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
                    this.systemCpuSparkline = {
                        labels: sorted.map(() => ''),
                        datasets: [{
                            data: sorted.map(t => t.systemCpuPercent ?? 0),
                            borderColor: '#6c757d',
                            backgroundColor: 'rgba(108, 117, 125, 0.1)',
                            fill: true
                        }]
                    };
                },
                error: (err: Error) => {
                    console.error('Failed to load system CPU trends:', err);
                }
            });

        // Load network utilization trends
        this.telemetryService.getNetworkTrends(undefined, this.sparklineHours)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    const sorted = [...response.data].sort((a, b) =>
                        new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
                    this.networkSparkline = {
                        labels: sorted.map(() => ''),
                        datasets: [{
                            data: sorted.map(t => t.utilizationPercent ?? 0),
                            borderColor: '#fd7e14',
                            backgroundColor: 'rgba(253, 126, 20, 0.1)',
                            fill: true
                        }]
                    };
                },
                error: (err: Error) => {
                    console.error('Failed to load network trends:', err);
                }
            });
    }

    /**
     * Load per-app memory sparklines
     */
    loadAppSparklines(): void {
        if (!this.summary?.applications) return;

        this.summary.applications.forEach(app => {
            this.telemetryService.getMemoryTrends(app.name, this.sparklineHours)
                .pipe(takeUntil(this.destroy$))
                .subscribe({
                    next: (response) => {
                        if (response.data.length > 0) {
                            const sorted = [...response.data].sort((a, b) =>
                                new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
                            this.appSparklines.set(app.name, {
                                labels: sorted.map(() => ''),
                                datasets: [{
                                    data: sorted.map(t => t.workingSetMB ?? 0),
                                    borderColor: '#0d6efd',
                                    backgroundColor: 'rgba(13, 110, 253, 0.1)',
                                    fill: true
                                }]
                            });
                        }
                    },
                    error: () => { /* silently skip apps with no data */ }
                });
        });
    }

    /**
     * Calculate health score (0-100) for an app based on its metrics.
     * Uses same weighted formula as fleet health score for consistency.
     * Weights: CPU 30%, Memory 30%, Errors 20%, Online 20%
     */
    calculateHealthScore(appName: string): number {
        const snap = this.summary?.latestSnapshots?.find(s => s.applicationName === appName);
        if (!snap) return 0;

        // Offline = 0 score
        if (!snap.isOnline) return 0;

        let totalWeight = 0;
        let weightedSum = 0;

        // CPU Score (30% weight) - prefer system CPU, fall back to process CPU
        const cpuValue = (snap as any).systemCpuPercent ?? snap.cpuPercent ?? 0;
        const cpuScore = this.percentToScore(cpuValue);
        weightedSum += cpuScore * 30;
        totalWeight += 30;

        // Memory Score (30% weight) - prefer system memory percent
        const memValue = (snap as any).systemMemoryPercent ?? 0;
        if (memValue > 0) {
            const memScore = this.percentToScore(memValue);
            weightedSum += memScore * 30;
            totalWeight += 30;
        }

        // Errors Score (20% weight) - use fleet errors divided by app count
        const appCount = this.summary?.applications?.length || 1;
        const errorCount = Math.floor((this.fleetMetrics?.system?.totalLogErrors ?? 0) / appCount);
        const errorScore = this.errorCountToScore(errorCount);
        weightedSum += errorScore * 20;
        totalWeight += 20;

        // Online Score (20% weight) - 100 since we already checked isOnline
        weightedSum += 100 * 20;
        totalWeight += 20;

        return totalWeight > 0 ? Math.round(weightedSum / totalWeight) : 0;
    }

    /**
     * Get health score color class (aligned with fleet thresholds)
     */
    getHealthScoreClass(score: number): string {
        if (score >= 70) return 'health-good';
        if (score >= 50) return 'health-warning';
        return 'health-critical';
    }

    /**
     * Initialize health scores for all apps
     */
    initHealthScores(): void {
        if (!this.summary?.applications) return;
        this.summary.applications.forEach(app => {
            const score = this.calculateHealthScore(app.name);
            this.appHealthScores.set(app.name, score);
        });
    }

    getOnlineCount(): number {
        if (!this.summary?.latestSnapshots) return 0;
        return this.summary.latestSnapshots.filter(s => s.isOnline).length;
    }

    /**
     * Get human-readable time since last sparkline refresh
     */
    getTimeSinceRefresh(): string {
        if (!this.lastRefreshTime) return '';
        const seconds = Math.floor((new Date().getTime() - this.lastRefreshTime.getTime()) / 1000);
        if (seconds < 60) return `${seconds}s ago`;
        const minutes = Math.floor(seconds / 60);
        return `${minutes}m ago`;
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

    /**
     * Calculate composite health score (0-100) from fleet metrics or summary.
     * Higher score = healthier system.
     * Weights: CPU 30%, Memory 30%, Errors 20%, Online 20%
     */
    calculateFleetHealthScore(): number {
        let totalWeight = 0;
        let weightedSum = 0;

        // Try to get data from fleetMetrics first, fall back to summary
        if (this.fleetMetrics?.system) {
            const fm = this.fleetMetrics.system;

            // CPU Score (30% weight) - lower CPU = healthier
            const cpuValue = fm.avgSystemCpuPercent ?? fm.avgCpuPercent ?? 0;
            const cpuScore = this.percentToScore(cpuValue);
            weightedSum += cpuScore * 30;
            totalWeight += 30;

            // Memory Score (30% weight) - lower memory = healthier
            const memValue = fm.avgSystemMemoryPercent ?? 0;
            if (memValue > 0) {
                const memScore = this.percentToScore(memValue);
                weightedSum += memScore * 30;
                totalWeight += 30;
            }

            // Errors Score (20% weight)
            const errorCount = fm.totalLogErrors ?? 0;
            let errorScore = 100;
            if (errorCount > 50) errorScore = 0;
            else if (errorCount > 20) errorScore = 25;
            else if (errorCount > 5) errorScore = 50;
            else if (errorCount > 0) errorScore = 75;
            weightedSum += errorScore * 20;
            totalWeight += 20;

            // Online Score (20% weight)
            if (fm.applicationCount > 0) {
                const onlineScore = (fm.onlineCount / fm.applicationCount) * 100;
                weightedSum += onlineScore * 20;
                totalWeight += 20;
            }
        } else if (this.summary) {
            // Fallback to summary data
            // Errors Score (40% weight when we only have summary)
            const errorCount = this.summary.last24Hours?.errorCount ?? 0;
            let errorScore = 100;
            if (errorCount > 50) errorScore = 0;
            else if (errorCount > 20) errorScore = 25;
            else if (errorCount > 5) errorScore = 50;
            else if (errorCount > 0) errorScore = 75;
            weightedSum += errorScore * 40;
            totalWeight += 40;

            // Online Score (60% weight when we only have summary)
            const totalApps = this.getTotalApps();
            const onlineApps = this.getOnlineCount();
            if (totalApps > 0) {
                const onlineScore = (onlineApps / totalApps) * 100;
                weightedSum += onlineScore * 60;
                totalWeight += 60;
            }
        }

        return totalWeight > 0 ? Math.round(weightedSum / totalWeight) : 0;
    }

    /**
     * Convert a percentage (0-100) to a health score (100-0).
     * Lower percentage = healthier = higher score.
     */
    private percentToScore(percent: number): number {
        if (percent < 50) return 100;
        if (percent < 70) return 80;
        if (percent < 80) return 60;
        if (percent < 90) return 40;
        return 20;
    }

    /**
     * Get health score status label
     */
    getHealthScoreStatus(): string {
        const score = this.calculateFleetHealthScore();
        if (score >= 90) return 'Excellent';
        if (score >= 70) return 'Healthy';
        if (score >= 50) return 'Fair';
        if (score >= 25) return 'Degraded';
        return 'Critical';
    }

    /**
     * Get fleet health score color class
     */
    getFleetHealthScoreClass(): string {
        const score = this.calculateFleetHealthScore();
        if (score >= 90) return 'bg-success';
        if (score >= 70) return 'bg-success';
        if (score >= 50) return 'bg-warning';
        if (score >= 25) return 'bg-orange';
        return 'bg-danger';
    }

    /**
     * Get health score breakdown for tooltip
     */
    getHealthScoreBreakdown(): string {
        const lines: string[] = [];

        if (this.fleetMetrics?.system) {
            const fm = this.fleetMetrics.system;
            const cpuValue = fm.avgSystemCpuPercent ?? fm.avgCpuPercent ?? 0;
            const memValue = fm.avgSystemMemoryPercent ?? 0;
            const errorCount = fm.totalLogErrors ?? 0;
            const onlinePercent = fm.applicationCount > 0
                ? Math.round((fm.onlineCount / fm.applicationCount) * 100)
                : 100;

            lines.push(`CPU: ${cpuValue.toFixed(1)}% → ${this.percentToScore(cpuValue)}/100`);
            if (memValue > 0) {
                lines.push(`Memory: ${memValue.toFixed(1)}% → ${this.percentToScore(memValue)}/100`);
            }
            lines.push(`Errors: ${errorCount} → ${this.errorCountToScore(errorCount)}/100`);
            lines.push(`Online: ${onlinePercent}% → ${onlinePercent}/100`);
        } else if (this.summary) {
            const errorCount = this.summary.last24Hours?.errorCount ?? 0;
            const totalApps = this.getTotalApps();
            const onlineApps = this.getOnlineCount();
            const onlinePercent = totalApps > 0 ? Math.round((onlineApps / totalApps) * 100) : 100;

            lines.push(`Errors: ${errorCount} → ${this.errorCountToScore(errorCount)}/100`);
            lines.push(`Online: ${onlinePercent}% → ${onlinePercent}/100`);
        }

        return lines.join('\n');
    }

    /**
     * Convert error count to score (helper for breakdown)
     */
    private errorCountToScore(errorCount: number): number {
        if (errorCount === 0) return 100;
        if (errorCount <= 5) return 75;
        if (errorCount <= 20) return 50;
        if (errorCount <= 50) return 25;
        return 0;
    }

    /**
     * Get trend indicator for a metric based on sparkline data
     * Returns: 'up' | 'down' | 'stable'
     */
    getMetricTrend(sparklineData: ChartData<'line'> | null): 'up' | 'down' | 'stable' {
        if (!sparklineData?.datasets?.[0]?.data?.length) return 'stable';

        const data = sparklineData.datasets[0].data as number[];
        if (data.length < 2) return 'stable';

        // Compare last value to average of first half
        const midpoint = Math.floor(data.length / 2);
        const firstHalfAvg = data.slice(0, midpoint).reduce((a, b) => a + b, 0) / midpoint;
        const lastValue = data[data.length - 1];

        const changePercent = ((lastValue - firstHalfAvg) / (firstHalfAvg || 1)) * 100;

        if (changePercent > 5) return 'up';
        if (changePercent < -5) return 'down';
        return 'stable';
    }

    /**
     * Get trend arrow icon class
     */
    getTrendArrowClass(trend: 'up' | 'down' | 'stable', invertColors: boolean = false): string {
        if (trend === 'up') {
            return invertColors ? 'fa-arrow-up text-danger' : 'fa-arrow-up text-success';
        }
        if (trend === 'down') {
            return invertColors ? 'fa-arrow-down text-success' : 'fa-arrow-down text-danger';
        }
        return 'fa-minus text-muted';
    }

    /**
     * Get time since last error as human-readable string
     */
    getTimeSinceLastError(): string {
        if (!this.summary?.last24Hours) return 'Unknown';

        const errorCount = this.summary.last24Hours.errorCount ?? 0;
        if (errorCount > 0) {
            return `${errorCount} error${errorCount > 1 ? 's' : ''} (24h)`;
        }

        // No errors - check if we have lastErrorTime
        const lastError = (this.summary.last24Hours as any).lastErrorTime;
        if (lastError) {
            const errorDate = new Date(lastError);
            const now = new Date();
            const hoursDiff = Math.floor((now.getTime() - errorDate.getTime()) / (1000 * 60 * 60));

            if (hoursDiff < 1) return 'Error-free < 1h';
            if (hoursDiff < 24) return `Error-free ${hoursDiff}h`;
            const daysDiff = Math.floor(hoursDiff / 24);
            return `Error-free ${daysDiff}d`;
        }

        // No errors and no timestamp - just show clean
        return 'No errors (24h)';
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

        //
        // Only show loading spinner and clear data on initial load (when no healthStatus exists)
        // During refresh, keep existing data visible to avoid flicker
        //
        const isInitialLoad = !this.healthStatus && !this.selectedSnapshot;
        if (isInitialLoad) {
            this.realtimeLoading = true;
            this.healthStatus = null;
            this.authenticatedUsers = null;
            this.selectedSnapshot = null;
        }
        this.loading = false;
        this.error = null;

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
        //
        // Only show loading spinner on initial load, not during refresh
        //
        if (!this.initialLoadComplete) {
            this.loading = true;
        }
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

        // Load trend data separately for charts
        this.loadTrendData();
    }

    loadTrendData(): void {
        this.trendLoading = true;

        forkJoin({
            memory: this.telemetryService.getMemoryTrends(
                this.selectedAppName || undefined,
                this.selectedHours
            ),
            cpu: this.telemetryService.getCpuTrends(
                this.selectedAppName || undefined,
                this.selectedHours
            ),
            network: this.telemetryService.getNetworkTrends(
                this.selectedAppName || undefined,
                this.selectedHours
            ),
            systemMemory: this.telemetryService.getSystemMemoryTrends(
                this.selectedAppName || undefined,
                this.selectedHours
            ),
            systemCpu: this.telemetryService.getSystemCpuTrends(
                this.selectedAppName || undefined,
                this.selectedHours
            )
        })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.memoryTrends = result.memory.data;
                    this.cpuTrends = result.cpu.data;
                    this.networkTrends = result.network.data;
                    this.systemMemoryTrends = result.systemMemory.data;
                    this.systemCpuTrends = result.systemCpu.data;
                    this.buildMemoryChart();
                    this.buildCpuChart();
                    this.buildNetworkChart();
                    this.trendLoading = false;
                },
                error: (err: Error) => {
                    console.error('Trend data error:', err);
                    this.trendLoading = false;
                }
            });
    }

    private buildMemoryChart(): void {
        if (!this.memoryTrends.length) {
            this.memoryChartData = { labels: [], datasets: [] };
            return;
        }

        // Group process memory by application
        const appGroups = new Map<string, { timestamp: Date; value: number }[]>();
        for (const point of this.memoryTrends) {
            const app = point.applicationName;
            if (!appGroups.has(app)) {
                appGroups.set(app, []);
            }
            appGroups.get(app)!.push({
                timestamp: new Date(point.timestamp),
                value: point.workingSetMB || 0
            });
        }

        // Group system memory by application
        const systemGroups = new Map<string, { timestamp: Date; value: number }[]>();
        for (const point of this.systemMemoryTrends) {
            const app = point.applicationName;
            if (!systemGroups.has(app)) {
                systemGroups.set(app, []);
            }
            systemGroups.get(app)!.push({
                timestamp: new Date(point.timestamp),
                value: point.systemMemoryPercent || 0
            });
        }

        // Generate unique labels (time points) from both data sources
        const allTimestamps = [
            ...this.memoryTrends.map(p => new Date(p.timestamp).getTime()),
            ...this.systemMemoryTrends.map(p => new Date(p.timestamp).getTime())
        ]
            .filter((v, i, a) => a.indexOf(v) === i)
            .sort((a, b) => a - b);

        const labels = allTimestamps.map(t => {
            const d = new Date(t);
            return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        });

        const colors = ['#4e79a7', '#f28e2c', '#e15759', '#76b7b2', '#59a14f', '#edc949'];
        const datasets: ChartData<'line'>['datasets'] = [];
        let colorIndex = 0;

        // Add process memory datasets (solid lines)
        appGroups.forEach((points, appName) => {
            const data = allTimestamps.map(ts => {
                const point = points.find(p => p.timestamp.getTime() === ts);
                return point ? point.value : null;
            });

            datasets.push({
                label: `${appName} (Process MB)`,
                data: data,
                borderColor: colors[colorIndex % colors.length],
                backgroundColor: colors[colorIndex % colors.length] + '33',
                fill: false,
                spanGaps: true
            });
            colorIndex++;
        });

        // Add system memory datasets (dashed lines)
        colorIndex = 0;
        systemGroups.forEach((points, appName) => {
            const data = allTimestamps.map(ts => {
                const point = points.find(p => p.timestamp.getTime() === ts);
                return point ? point.value : null;
            });

            datasets.push({
                label: `${appName} (Server %)`,
                data: data,
                borderColor: colors[colorIndex % colors.length],
                backgroundColor: colors[colorIndex % colors.length] + '33',
                borderDash: [5, 5],
                fill: false,
                spanGaps: true
            });
            colorIndex++;
        });

        this.memoryChartData = { labels, datasets };
    }

    private buildCpuChart(): void {
        if (!this.cpuTrends.length) {
            this.cpuChartData = { labels: [], datasets: [] };
            return;
        }

        // Group process CPU by application
        const appGroups = new Map<string, { timestamp: Date; value: number }[]>();
        for (const point of this.cpuTrends) {
            const app = point.applicationName;
            if (!appGroups.has(app)) {
                appGroups.set(app, []);
            }
            appGroups.get(app)!.push({
                timestamp: new Date(point.timestamp),
                value: point.cpuPercent || 0
            });
        }

        // Group system CPU by application
        const systemGroups = new Map<string, { timestamp: Date; value: number }[]>();
        for (const point of this.systemCpuTrends) {
            const app = point.applicationName;
            if (!systemGroups.has(app)) {
                systemGroups.set(app, []);
            }
            systemGroups.get(app)!.push({
                timestamp: new Date(point.timestamp),
                value: point.systemCpuPercent || 0
            });
        }

        // Generate unique labels (time points) from both data sources
        const allTimestamps = [
            ...this.cpuTrends.map(p => new Date(p.timestamp).getTime()),
            ...this.systemCpuTrends.map(p => new Date(p.timestamp).getTime())
        ]
            .filter((v, i, a) => a.indexOf(v) === i)
            .sort((a, b) => a - b);

        const labels = allTimestamps.map(t => {
            const d = new Date(t);
            return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        });

        const colors = ['#e15759', '#4e79a7', '#f28e2c', '#76b7b2', '#59a14f', '#edc949'];
        const datasets: ChartData<'line'>['datasets'] = [];
        let colorIndex = 0;

        // Add process CPU datasets (solid lines)
        appGroups.forEach((points, appName) => {
            const data = allTimestamps.map(ts => {
                const point = points.find(p => p.timestamp.getTime() === ts);
                return point ? point.value : null;
            });

            datasets.push({
                label: `${appName} (Process %)`,
                data: data,
                borderColor: colors[colorIndex % colors.length],
                backgroundColor: colors[colorIndex % colors.length] + '33',
                fill: false,
                spanGaps: true
            });
            colorIndex++;
        });

        // Add system CPU datasets (dashed lines)
        colorIndex = 0;
        systemGroups.forEach((points, appName) => {
            const data = allTimestamps.map(ts => {
                const point = points.find(p => p.timestamp.getTime() === ts);
                return point ? point.value : null;
            });

            datasets.push({
                label: `${appName} (Server %)`,
                data: data,
                borderColor: colors[colorIndex % colors.length],
                backgroundColor: colors[colorIndex % colors.length] + '33',
                borderDash: [5, 5],
                fill: false,
                spanGaps: true
            });
            colorIndex++;
        });

        this.cpuChartData = { labels, datasets };
    }

    private buildNetworkChart(): void {
        if (!this.networkTrends.length) {
            this.networkChartData = { labels: [], datasets: [] };
            return;
        }

        // Group by application + interface
        const appGroups = new Map<string, { timestamp: Date; value: number }[]>();
        for (const point of this.networkTrends) {
            const key = `${point.applicationName} - ${point.interfaceName}`;
            if (!appGroups.has(key)) {
                appGroups.set(key, []);
            }
            appGroups.get(key)!.push({
                timestamp: new Date(point.timestamp),
                value: point.utilizationPercent || 0
            });
        }

        // Generate unique labels (time points)
        const allTimestamps = this.networkTrends
            .map(p => new Date(p.timestamp).getTime())
            .filter((v, i, a) => a.indexOf(v) === i)
            .sort((a, b) => a - b);

        const labels = allTimestamps.map(t => {
            const d = new Date(t);
            return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        });

        const colors = ['#ff8c00', '#4e79a7', '#e15759', '#76b7b2', '#59a14f', '#edc949', '#f28e2c'];
        const datasets: ChartData<'line'>['datasets'] = [];
        let colorIndex = 0;

        appGroups.forEach((points, label) => {
            const data = allTimestamps.map(ts => {
                const point = points.find(p => p.timestamp.getTime() === ts);
                return point ? point.value : null;
            });

            datasets.push({
                label: label,
                data: data,
                borderColor: colors[colorIndex % colors.length],
                backgroundColor: colors[colorIndex % colors.length] + '33',
                fill: false,
                spanGaps: true
            });
            colorIndex++;
        });

        this.networkChartData = { labels, datasets };
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

    formatPercent(percent: number | undefined | null): string {
        if (percent === undefined || percent === null) return '-';
        return `${percent.toFixed(1)}%`;
    }

    formatBytes(bytes: number | undefined | null): string {
        if (bytes === undefined || bytes === null) return '-';
        if (bytes < 1024) return `${bytes.toFixed(0)} B`;
        if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
        if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
        return `${(bytes / (1024 * 1024 * 1024)).toFixed(2)} GB`;
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

    // Snapshot detail modal methods
    openSnapshotDetail(snapshotId: number): void {
        this.snapshotDetailLoading = true;
        this.telemetryService.getSnapshotDetail(snapshotId)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (detail) => {
                    this.selectedSnapshotDetail = detail;
                    this.snapshotDetailLoading = false;
                },
                error: (err: Error) => {
                    console.error('Error loading snapshot detail:', err);
                    this.snapshotDetailLoading = false;
                }
            });
    }

    closeSnapshotDetail(): void {
        this.selectedSnapshotDetail = null;
        this.logErrorFilter = '';
        this.logErrorLevelFilter = 'ALL';
    }

    get filteredLogErrors(): any[] {
        if (!this.selectedSnapshotDetail?.logErrors) return [];

        return this.selectedSnapshotDetail.logErrors.filter(err => {
            // Filter by level
            if (this.logErrorLevelFilter !== 'ALL' && err.level !== this.logErrorLevelFilter) {
                return false;
            }
            // Filter by text search
            if (this.logErrorFilter) {
                const search = this.logErrorFilter.toLowerCase();
                return (err.message?.toLowerCase().includes(search) ||
                    err.exception?.toLowerCase().includes(search) ||
                    err.logFileName?.toLowerCase().includes(search));
            }
            return true;
        });
    }


    // ========================================
    // TrackBy Functions (for *ngFor performance)
    // ========================================

    trackByAppName(index: number, app: { name: string }): string {
        return app.name;
    }

    trackByMetricName(index: number, metric: { metricName: string }): string {
        return metric.metricName;
    }

    trackBySnapshotApp(index: number, snap: { applicationName: string }): string {
        return snap.applicationName;
    }

    trackByDbName(index: number, db: { name: string }): string {
        return db.name;
    }

    trackBySessionUsername(index: number, session: { username: string }): string {
        return session.username;
    }

    trackByDriveName(index: number, drive: { name: string }): string {
        return drive.name;
    }

    trackByNicName(index: number, nic: { name: string }): string {
        return nic.name;
    }

    trackByRunId(index: number, run: { id: number }): number {
        return run.id;
    }

    trackByIndex(index: number): number {
        return index;
    }
}
