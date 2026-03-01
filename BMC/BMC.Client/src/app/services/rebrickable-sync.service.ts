import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// Rebrickable Sync Service
//
// Custom Angular service for the RebrickableSyncController endpoints.
// Manages connection, settings, pull operations, and transaction history.
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface SyncStatus {
    isConnected: boolean;
    rebrickableUsername: string | null;
    integrationMode: string;
    syncEnabled: boolean;
    authMode: string | null;
    lastPullDate: string | null;
    lastPushDate: string | null;
    lastSyncError: string | null;
    pullIntervalMinutes: number | null;
    totalTransactions: number;
    failedTransactions: number;
    recentPushCount: number;
    recentPullCount: number;
}

export interface SyncTransaction {
    id: number;
    transactionDate: string;
    direction: string;
    httpMethod: string;
    endpoint: string;
    requestSummary: string;
    responseStatusCode: number;
    success: boolean;
    errorMessage: string | null;
    triggeredBy: string;
    recordCount: number | null;
}

export interface SyncTransactionsPage {
    totalCount: number;
    pageSize: number;
    pageNumber: number;
    results: SyncTransaction[];
}

export interface SyncImportResult {
    totalCreated: number;
    totalUpdated: number;
    errorCount: number;
    setsCreated: number;
    setsUpdated: number;
    setListsCreated: number;
    partListsCreated: number;
    lostPartsCreated: number;
    errors: string[];
}

export interface ConnectRequest {
    apiToken: string;
    username?: string;
    password?: string;
    userToken?: string;
    authMode: string;
    integrationMode: string;
}

export interface UpdateSettingsRequest {
    integrationMode?: string;
    pullIntervalMinutes?: number;
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class RebrickableSyncService {
    private readonly baseUrl = '/api/rebrickable-sync';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }


    /** POST /api/rebrickable-sync/connect — validate token + store credentials */
    connect(request: ConnectRequest): Observable<{ connected: boolean; authMode: string }> {
        return this.http.post<{ connected: boolean; authMode: string }>(
            `${this.baseUrl}/connect`,
            request,
            { headers: this.headers }
        );
    }


    /** POST /api/rebrickable-sync/reauthenticate — refresh token without losing settings */
    reauthenticate(request: ConnectRequest): Observable<{ reauthenticated: boolean }> {
        return this.http.post<{ reauthenticated: boolean }>(
            `${this.baseUrl}/reauthenticate`,
            request,
            { headers: this.headers }
        );
    }


    /** GET /api/rebrickable-sync/token-health — validate stored token */
    checkTokenHealth(): Observable<{ valid: boolean; error: string | null }> {
        return this.http.get<{ valid: boolean; error: string | null }>(
            `${this.baseUrl}/token-health`,
            { headers: this.headers }
        );
    }


    /** POST /api/rebrickable-sync/disconnect — clear credentials */
    disconnect(): Observable<{ connected: boolean }> {
        return this.http.post<{ connected: boolean }>(
            `${this.baseUrl}/disconnect`,
            null,
            { headers: this.headers }
        );
    }


    /** GET /api/rebrickable-sync/status — current sync status for UI */
    getStatus(): Observable<SyncStatus> {
        return this.http.get<SyncStatus>(
            `${this.baseUrl}/status`,
            { headers: this.headers }
        );
    }


    /** POST /api/rebrickable-sync/pull — trigger full collection pull */
    pullFull(): Observable<SyncImportResult> {
        return this.http.post<SyncImportResult>(
            `${this.baseUrl}/pull`,
            null,
            { headers: this.headers }
        );
    }


    /** POST /api/rebrickable-sync/pull-sets — pull sets only */
    pullSets(): Observable<SyncImportResult> {
        return this.http.post<SyncImportResult>(
            `${this.baseUrl}/pull-sets`,
            null,
            { headers: this.headers }
        );
    }


    /** GET /api/rebrickable-sync/transactions — paginated audit log */
    getTransactions(
        pageSize: number = 50,
        pageNumber: number = 1,
        direction?: string,
        success?: boolean
    ): Observable<SyncTransactionsPage> {
        let params: any = { pageSize, pageNumber };
        if (direction) params.direction = direction;
        if (success !== undefined && success !== null) params.success = success;

        return this.http.get<SyncTransactionsPage>(
            `${this.baseUrl}/transactions`,
            { headers: this.headers, params }
        );
    }


    /** PUT /api/rebrickable-sync/settings — update integration mode + interval */
    updateSettings(request: UpdateSettingsRequest): Observable<any> {
        return this.http.put(
            `${this.baseUrl}/settings`,
            request,
            { headers: this.headers }
        );
    }
}
