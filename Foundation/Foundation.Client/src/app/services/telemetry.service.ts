import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// Telemetry Service
//
// Angular service for retrieving historical telemetry data from the backend.
// Provides access to snapshots, trends, errors, and dashboard summary.
//

// Snapshot response types
export interface TelemetrySnapshotDto {
    id: number;
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
}

export interface TelemetrySnapshotsResponse {
    snapshots: TelemetrySnapshotDto[];
    count: number;
}

// Trend response types
export interface MemoryTrendPoint {
    timestamp: Date;
    applicationName: string;
    workingSetMB?: number;
    gcHeapMB?: number;
}

export interface MemoryTrendsResponse {
    data: MemoryTrendPoint[];
    hours: number;
    count: number;
}

export interface CpuTrendPoint {
    timestamp: Date;
    applicationName: string;
    cpuPercent?: number;
}

export interface CpuTrendsResponse {
    data: CpuTrendPoint[];
    hours: number;
    count: number;
}

export interface DiskTrendPoint {
    timestamp: Date;
    applicationName: string;
    driveName: string;
    totalGB: number;
    freeGB: number;
    freePercent: number;
    status?: string;
}

export interface DiskTrendsResponse {
    data: DiskTrendPoint[];
    hours: number;
    count: number;
}

export interface NetworkTrendPoint {
    timestamp: Date;
    applicationName: string;
    interfaceName: string;
    linkSpeedMbps?: number;
    bytesSentPerSecond?: number;
    bytesReceivedPerSecond?: number;
    utilizationPercent?: number;
    status?: string;
}

export interface NetworkTrendsResponse {
    data: NetworkTrendPoint[];
    hours: number;
    count: number;
}

export interface SystemMemoryTrendPoint {
    timestamp: Date;
    applicationName: string;
    systemMemoryPercent?: number;
}

export interface SystemMemoryTrendsResponse {
    data: SystemMemoryTrendPoint[];
    hours: number;
    count: number;
}

export interface SystemCpuTrendPoint {
    timestamp: Date;
    applicationName: string;
    systemCpuPercent?: number;
}

export interface SystemCpuTrendsResponse {
    data: SystemCpuTrendPoint[];
    hours: number;
    count: number;
}

export interface SessionTrendPoint {
    timestamp: Date;
    applicationName: string;
    activeSessionCount: number;
    expiredSessionCount: number;
}

export interface SessionTrendsResponse {
    data: SessionTrendPoint[];
    hours: number;
    count: number;
}

// Snapshot detail types for drill-down modal
export interface DatabaseHealthDto {
    databaseName: string;
    isConnected: boolean;
    status?: string;
    server?: string;
    provider?: string;
    errorMessage?: string;
}

export interface DiskHealthDto {
    driveName: string;
    driveLabel?: string;
    totalGB: number;
    freeGB: number;
    freePercent: number;
    status?: string;
    isApplicationDrive: boolean;
}

export interface SessionSnapshotDto {
    activeSessionCount: number;
    expiredSessionCount: number;
    oldestSessionStart?: Date;
    newestSessionStart?: Date;
}

export interface ApplicationMetricDto {
    metricName: string;
    metricValue?: string;
    state?: number;  // 0=Normal, 1=Warning, 2=Critical
    dataType?: number;  // 0=Text, 1=Number, 2=Percentage
    numericValue?: number;
    category?: string;
}

export interface SnapshotDetailDto {
    id: number;
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
    databases: DatabaseHealthDto[];
    disks: DiskHealthDto[];
    sessions?: SessionSnapshotDto;
    metrics: ApplicationMetricDto[];
    logErrors: LogErrorDto[];
}

export interface LogErrorDto {
    logTimestamp?: Date;
    level?: string;
    message?: string;
    exception?: string;
    logFileName?: string;
    occurrenceCount?: number;
}

// Error event types
export interface TelemetryErrorEventDto {
    id: number;
    applicationName: string;
    occurredAt: Date;
    auditTypeName?: string;
    moduleName?: string;
    entityName?: string;
    userName?: string;
    message?: string;
}

export interface TelemetryErrorsResponse {
    errors: TelemetryErrorEventDto[];
    count: number;
}

// Application types
export interface TelemetryApplicationDto {
    id: number;
    name: string;
    url?: string;
    isSelf: boolean;
    firstSeen?: Date;
    lastSeen?: Date;
}

export interface TelemetryApplicationsResponse {
    applications: TelemetryApplicationDto[];
    count: number;
}

// Collection run types
export interface TelemetryCollectionRunDto {
    id: number;
    startTime: Date;
    endTime?: Date;
    durationMs?: number;
    applicationsPolled: number;
    applicationsSucceeded: number;
    errorMessage?: string;
}

export interface TelemetryCollectionRunsResponse {
    runs: TelemetryCollectionRunDto[];
    count: number;
}

// Dashboard summary types
export interface TelemetrySummaryResponse {
    applications: {
        name: string;
        url?: string;
        isSelf: boolean;
        firstSeen?: Date;
        lastSeen?: Date;
    }[];
    latestSnapshots: {
        applicationName: string;
        collectedAt: Date;
        isOnline: boolean;
        uptimeSeconds?: number;
        memoryWorkingSetMB?: number;
        memoryGcHeapMB?: number;
        cpuPercent?: number;
        machineName?: string;
        systemMemoryPercent?: number;
        systemCpuPercent?: number;
    }[];
    lastCollectionRun?: {
        startTime: Date;
        endTime?: Date;
        applicationsPolled: number;
        applicationsSucceeded: number;
        errorMessage?: string;
    };
    last24Hours: {
        totalSnapshots: number;
        onlineCount: number;
        avgMemoryMB?: number;
        maxMemoryMB?: number;
        errorCount: number;
    };
}

// Fleet aggregation types
export interface FleetMetricsResponse {
    system: {
        applicationCount: number;
        onlineCount: number;
        totalMemoryMB: number;
        avgCpuPercent: number;
        totalLogErrors: number;
        avgSystemMemoryPercent?: number;
        avgSystemCpuPercent?: number;
    };
    metrics: {
        metricName: string;
        total: number;
        average: number;
        count: number;
        worstState: number;
    }[];
    timestamp: Date;
}

// Metric trend types
export interface MetricTrendPoint {
    timestamp: Date;
    applicationName: string;
    metricName: string;
    value: number;
    state?: number;
}

export interface MetricTrendsResponse {
    data: MetricTrendPoint[];
    hours: number;
    count: number;
    availableMetrics: string[];
}


@Injectable({
    providedIn: 'root'
})
export class TelemetryService {
    private readonly baseUrl = '/api/Telemetry';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    /**
     * Get dashboard summary with latest snapshots and aggregated statistics
     */
    getSummary(): Observable<TelemetrySummaryResponse> {
        return this.http.get<TelemetrySummaryResponse>(
            `${this.baseUrl}/summary`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get aggregated fleet metrics across all applications
     */
    getFleetMetrics(): Observable<FleetMetricsResponse> {
        return this.http.get<FleetMetricsResponse>(
            `${this.baseUrl}/fleetMetrics`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get metric trends for sparklines/charts
     */
    getMetricTrends(appName?: string, metricName?: string, hours: number = 24, limit: number = 100): Observable<MetricTrendsResponse> {
        let params = new HttpParams().set('hours', hours.toString()).set('limit', limit.toString());
        if (appName) params = params.set('appName', appName);
        if (metricName) params = params.set('metricName', metricName);

        return this.http.get<MetricTrendsResponse>(
            `${this.baseUrl}/trends/metrics`,
            { headers: this.authService.GetAuthenticationHeaders(), params }
        );
    }

    /**
     * Get historical snapshots with optional filtering
     */
    getSnapshots(appName?: string, startDate?: Date, endDate?: Date, limit: number = 100): Observable<TelemetrySnapshotsResponse> {
        let params = new HttpParams();
        if (appName) params = params.set('appName', appName);
        if (startDate) params = params.set('startDate', startDate.toISOString());
        if (endDate) params = params.set('endDate', endDate.toISOString());
        params = params.set('limit', limit.toString());

        return this.http.get<TelemetrySnapshotsResponse>(
            `${this.baseUrl}/snapshots`,
            { headers: this.authService.GetAuthenticationHeaders(), params }
        );
    }

    /**
     * Get full snapshot detail with child records for drill-down modal
     */
    getSnapshotDetail(id: number): Observable<SnapshotDetailDto> {
        return this.http.get<SnapshotDetailDto>(
            `${this.baseUrl}/snapshots/${id}`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get memory usage trends for charting
     */
    getMemoryTrends(appName?: string, hours: number = 24): Observable<MemoryTrendsResponse> {
        let params = new HttpParams().set('hours', hours.toString());
        if (appName) params = params.set('appName', appName);

        return this.http.get<MemoryTrendsResponse>(
            `${this.baseUrl}/trends/memory`,
            { headers: this.authService.GetAuthenticationHeaders(), params }
        );
    }

    /**
     * Get CPU usage trends for charting
     */
    getCpuTrends(appName?: string, hours: number = 24): Observable<CpuTrendsResponse> {
        let params = new HttpParams().set('hours', hours.toString());
        if (appName) params = params.set('appName', appName);

        return this.http.get<CpuTrendsResponse>(
            `${this.baseUrl}/trends/cpu`,
            { headers: this.authService.GetAuthenticationHeaders(), params }
        );
    }

    /**
     * Get disk usage trends for charting
     */
    getDiskTrends(appName?: string, hours: number = 24): Observable<DiskTrendsResponse> {
        let params = new HttpParams().set('hours', hours.toString());
        if (appName) params = params.set('appName', appName);

        return this.http.get<DiskTrendsResponse>(
            `${this.baseUrl}/trends/disk`,
            { headers: this.authService.GetAuthenticationHeaders(), params }
        );
    }

    /**
     * Get network utilization trends for sparklines
     */
    getNetworkTrends(appName?: string, hours: number = 24): Observable<NetworkTrendsResponse> {
        let params = new HttpParams().set('hours', hours.toString());
        if (appName) params = params.set('appName', appName);

        return this.http.get<NetworkTrendsResponse>(
            `${this.baseUrl}/trends/network`,
            { headers: this.authService.GetAuthenticationHeaders(), params }
        );
    }

    /**
     * Get system-wide memory usage trends for sparklines
     */
    getSystemMemoryTrends(appName?: string, hours: number = 24): Observable<SystemMemoryTrendsResponse> {
        let params = new HttpParams().set('hours', hours.toString());
        if (appName) params = params.set('appName', appName);

        return this.http.get<SystemMemoryTrendsResponse>(
            `${this.baseUrl}/trends/systemMemory`,
            { headers: this.authService.GetAuthenticationHeaders(), params }
        );
    }

    /**
     * Get system-wide CPU usage trends for sparklines
     */
    getSystemCpuTrends(appName?: string, hours: number = 24): Observable<SystemCpuTrendsResponse> {
        let params = new HttpParams().set('hours', hours.toString());
        if (appName) params = params.set('appName', appName);

        return this.http.get<SystemCpuTrendsResponse>(
            `${this.baseUrl}/trends/systemCpu`,
            { headers: this.authService.GetAuthenticationHeaders(), params }
        );
    }

    /**
     * Get session count trends for charting
     */
    getSessionTrends(appName?: string, hours: number = 24): Observable<SessionTrendsResponse> {
        let params = new HttpParams().set('hours', hours.toString());
        if (appName) params = params.set('appName', appName);

        return this.http.get<SessionTrendsResponse>(
            `${this.baseUrl}/trends/sessions`,
            { headers: this.authService.GetAuthenticationHeaders(), params }
        );
    }

    /**
     * Get error events with optional filtering
     */
    getErrors(appName?: string, startDate?: Date, endDate?: Date, limit: number = 100): Observable<TelemetryErrorsResponse> {
        let params = new HttpParams();
        if (appName) params = params.set('appName', appName);
        if (startDate) params = params.set('startDate', startDate.toISOString());
        if (endDate) params = params.set('endDate', endDate.toISOString());
        params = params.set('limit', limit.toString());

        return this.http.get<TelemetryErrorsResponse>(
            `${this.baseUrl}/errors`,
            { headers: this.authService.GetAuthenticationHeaders(), params }
        );
    }

    /**
     * Get list of monitored applications
     */
    getApplications(): Observable<TelemetryApplicationsResponse> {
        return this.http.get<TelemetryApplicationsResponse>(
            `${this.baseUrl}/applications`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get recent collection run history
     */
    getCollectionRuns(limit: number = 50): Observable<TelemetryCollectionRunsResponse> {
        const params = new HttpParams().set('limit', limit.toString());
        return this.http.get<TelemetryCollectionRunsResponse>(
            `${this.baseUrl}/collection-runs`,
            { headers: this.authService.GetAuthenticationHeaders(), params }
        );
    }
}
