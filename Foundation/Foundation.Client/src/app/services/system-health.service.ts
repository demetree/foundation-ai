import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, timer } from 'rxjs';
import { switchMap, takeUntil, shareReplay } from 'rxjs/operators';
import { AuthService } from './auth.service';

//
// System Health Service
//
// Angular service for retrieving system health metrics from the backend.
// Supports auto-refresh capability.
//

export interface SystemHealthStatus {
    timestamp: Date;
    application: ApplicationMetrics;
    database: DatabaseStatus;
    disk: DiskMetrics;
    threadPool: ThreadPoolMetrics;
}

export interface ApplicationMetrics {
    status: string;
    uptime: {
        totalSeconds: number;
        days: number;
        hours: number;
        minutes: number;
        display: string;
    };
    memory: {
        workingSetMB: number;
        privateMemoryMB: number;
        gcHeapMB: number;
        gen0Collections: number;
        gen1Collections: number;
        gen2Collections: number;
    };
    process: {
        id: number;
        name: string;
        startTime: Date;
        threadCount: number;
        handleCount: number;
    };
    environment: {
        machineName: string;
        osVersion: string;
        processorCount: number;
        is64Bit: boolean;
        dotNetVersion: string;
    };
}

export interface DatabaseStatus {
    databases: DatabaseInfo[];
}

export interface DatabaseInfo {
    name: string;
    status: string;
    isConnected: boolean;
    provider: string;
    server: string;
    database?: string;
    errorMessage?: string;
}

export interface TableStatisticsInfo {
    databaseName: string;
    provider: string;
    tables: TableInfo[];
    totalTables: number;
    totalRows: number;
    totalSizeMB: number;
    sizeAvailable: boolean;
    errorMessage?: string;
}

export interface TableInfo {
    tableName: string;
    rowCount: number;
    sizeMB: number;
}

export interface DiskMetrics {
    drives: DriveInfo[];
    applicationPath: string;
}

export interface DriveInfo {
    name: string;
    label: string;
    totalGB: number;
    freeGB: number;
    usedGB: number;
    freePercent: number;
    usedPercent: number;
    status: 'Healthy' | 'Warning' | 'Critical';
    isApplicationDrive: boolean;
}

export interface ThreadPoolMetrics {
    workerThreads: {
        available: number;
        max: number;
        min: number;
        inUse: number;
    };
    completionPortThreads: {
        available: number;
        max: number;
        min: number;
        inUse: number;
    };
}


//
// Authenticated Users Types
//

export interface AuthenticatedUsersInfo {
    sessions: AuthenticatedUserSession[];
    totalCount: number;
    asOf: Date;
    errorMessage?: string;
}

export interface AuthenticatedUserSession {
    sessionId?: number;
    username: string;
    displayName?: string;
    clientApplication?: string;
    sessionStart: Date;
    expiresAt: Date;
    isExpired: boolean;
    email?: string;
    ipAddress?: string;
    userAgent?: string;
    loginMethod?: string;
}


//
// Application Metrics Types
//

export type MetricState = 'Healthy' | 'Warning' | 'Critical' | 'Unknown';
export type MetricDataType = 'Number' | 'Percentage' | 'Text' | 'Boolean' | 'Duration';

export interface ApplicationMetricsResponse {
    applications: ApplicationMetricsGroup[];
    errorMessage?: string;
}

export interface ApplicationMetricsGroup {
    applicationName: string;
    metrics: ApplicationMetricItem[];
}

export interface ApplicationMetricItem {
    name: string;
    value: string;
    state: MetricState;
    dataType: MetricDataType;
    category?: string;
    description?: string;
    icon?: string;
}

@Injectable({
    providedIn: 'root'
})
export class SystemHealthService {
    private readonly baseUrl = '/api/SystemHealth';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    /**
     * Get complete system health status
     */
    getStatus(): Observable<SystemHealthStatus> {
        return this.http.get<SystemHealthStatus>(
            `${this.baseUrl}/status`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get application metrics only
     */
    getApplication(): Observable<ApplicationMetrics> {
        return this.http.get<ApplicationMetrics>(
            `${this.baseUrl}/application`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get database status only
     */
    getDatabase(): Observable<DatabaseStatus> {
        return this.http.get<DatabaseStatus>(
            `${this.baseUrl}/database`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get disk metrics only
     */
    getDisk(): Observable<DiskMetrics> {
        return this.http.get<DiskMetrics>(
            `${this.baseUrl}/disk`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Create an auto-refreshing Observable for system health status
     */
    getStatusWithAutoRefresh(intervalMs: number, stopSignal: Subject<void>): Observable<SystemHealthStatus> {
        return timer(0, intervalMs).pipe(
            takeUntil(stopSignal),
            switchMap(() => this.getStatus())
        );
    }

    /**
     * Get table statistics for a specific database (on-demand, expensive operation).
     * If appName is provided, the request will be proxied to that remote application.
     */
    getTableStatistics(databaseName: string, appName?: string): Observable<TableStatisticsInfo> {
        const params: { [key: string]: string } = { database: databaseName };
        if (appName) {
            params['appName'] = appName;
        }
        return this.http.get<TableStatisticsInfo>(
            `${this.baseUrl}/database/tables`,
            {
                headers: this.authService.GetAuthenticationHeaders(),
                params: params
            }
        );
    }

    /**
     * Get authenticated user sessions from OAuth tokens
     */
    getAuthenticatedUsers(): Observable<AuthenticatedUsersInfo> {
        return this.http.get<AuthenticatedUsersInfo>(
            `${this.baseUrl}/users`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Revoke a specific session by ID (admin only)
     */
    revokeSession(sessionId: number, reason?: string): Observable<{ success: boolean; message: string }> {
        return this.http.post<{ success: boolean; message: string }>(
            `/api/sessions/${sessionId}/revoke`,
            { reason: reason || 'Administrative action' },
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Revoke a session AND lock the user's account (admin only)
     * This is a high-security operation that prevents the user from logging in again.
     */
    revokeSessionAndLockAccount(sessionId: number, reason?: string): Observable<{
        success: boolean;
        message: string;
        revokedCount: number;
        accountLocked: boolean;
        targetUsername: string;
    }> {
        return this.http.post<{
            success: boolean;
            message: string;
            revokedCount: number;
            accountLocked: boolean;
            targetUsername: string;
        }>(
            `/api/sessions/${sessionId}/revoke-and-lock`,
            { reason: reason || 'Security action - account locked' },
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get application-specific business metrics
     */
    getApplicationMetrics(): Observable<ApplicationMetricsResponse> {
        return this.http.get<ApplicationMetricsResponse>(
            `${this.baseUrl}/metrics`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }
}


//
// Monitored Applications Types and Service Extension
//

export interface MonitoredApplicationConfig {
    name: string;
    url: string;
    isSelf: boolean;
}

export interface MonitoredApplicationStatus {
    name: string;
    url: string;
    isSelf: boolean;
    isAvailable: boolean;
    status: string;
    error?: string;
    checkedAt: Date;
    healthData?: SystemHealthStatus;
}

@Injectable({
    providedIn: 'root'
})
export class MonitoredApplicationsService {
    private readonly baseUrl = '/api/MonitoredApplications';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    /**
     * Get list of configured monitored applications
     */
    getApplications(): Observable<MonitoredApplicationConfig[]> {
        return this.http.get<MonitoredApplicationConfig[]>(
            this.baseUrl,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get health status for all configured applications
     */
    getAllStatus(): Observable<MonitoredApplicationStatus[]> {
        return this.http.get<MonitoredApplicationStatus[]>(
            `${this.baseUrl}/status`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get health status for a specific application
     */
    getApplicationStatus(appName: string): Observable<MonitoredApplicationStatus> {
        return this.http.get<MonitoredApplicationStatus>(
            `${this.baseUrl}/${encodeURIComponent(appName)}/status`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }
}
