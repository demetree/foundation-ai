import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, timer } from 'rxjs';
import { switchMap, takeUntil } from 'rxjs/operators';
import { AuthService } from './auth.service';

//
// System Health Service (BMC)
//
// Angular service for retrieving system health metrics from the BMC backend.
// Ported from Scheduler.Client — uses the same Foundation-based SystemHealth API.
//

export interface SystemHealthStatus {
    timestamp: Date;
    application: ApplicationMetrics;
    database: DatabaseStatus;
    disk: DiskMetrics;
    network?: NetworkMetrics;
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
        percent?: number;
        systemPercent?: number;
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
    cpu?: {
        percent: number;
        systemPercent?: number;
        processorCount: number;
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

export interface NetworkMetrics {
    interfaces: NetworkInterfaceInfo[];
}

export interface NetworkInterfaceInfo {
    name: string;
    description: string;
    linkSpeedMbps: number;
    bytesSentTotal: number;
    bytesReceivedTotal: number;
    bytesSentPerSecond: number;
    bytesReceivedPerSecond: number;
    utilizationPercent: number;
    status: 'Healthy' | 'Warning' | 'Critical';
    isActive: boolean;
}

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


@Injectable({
    providedIn: 'root'
})
export class SystemHealthService {
    private readonly baseUrl = '/api/SystemHealth';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    getStatus(): Observable<SystemHealthStatus> {
        return this.http.get<SystemHealthStatus>(
            `${this.baseUrl}/status`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    getDatabase(): Observable<DatabaseStatus> {
        return this.http.get<DatabaseStatus>(
            `${this.baseUrl}/database`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    getDisk(): Observable<DiskMetrics> {
        return this.http.get<DiskMetrics>(
            `${this.baseUrl}/disk`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    getStatusWithAutoRefresh(intervalMs: number, stopSignal: Subject<void>): Observable<SystemHealthStatus> {
        return timer(0, intervalMs).pipe(
            takeUntil(stopSignal),
            switchMap(() => this.getStatus())
        );
    }

    getTableStatistics(databaseName: string): Observable<TableStatisticsInfo> {
        return this.http.get<TableStatisticsInfo>(
            `${this.baseUrl}/database/tables`,
            {
                headers: this.authService.GetAuthenticationHeaders(),
                params: { database: databaseName }
            }
        );
    }

    getAuthenticatedUsers(): Observable<AuthenticatedUsersInfo> {
        return this.http.get<AuthenticatedUsersInfo>(
            `${this.baseUrl}/users`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }
}
