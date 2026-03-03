import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// BrickSet Sync Service
//
// Custom Angular service for the BrickSetSyncController endpoints.
// Manages connection, enrichment, quota, and transaction history.
//
// Mirrors the RebrickableSyncService pattern for consistency.
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface BrickSetSyncStatus {
    isConnected: boolean;
    brickSetUsername: string | null;
    syncDirection: string;
    lastSyncDate: string | null;
    lastEnrichmentDate: string | null;
    lastSyncError: string | null;
    totalTransactions: number;
    recentErrorCount: number;
    apiCallsRemainingToday: number | null;
}

export interface BrickSetTransaction {
    id: number;
    transactionDate: string;
    direction: string;
    methodName: string;
    requestSummary: string;
    success: boolean;
    errorMessage: string | null;
    triggeredBy: string;
    recordCount: number | null;
    apiCallsRemaining: number | null;
}

export interface BrickSetTransactionsPage {
    totalCount: number;
    pageSize: number;
    pageNumber: number;
    results: BrickSetTransaction[];
}

export interface BrickSetConnectRequest {
    username: string;
    password: string;
    syncDirection: string;
}

export interface BrickSetUpdateSettingsRequest {
    syncDirection?: string;
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class BrickSetSyncService {
    private readonly baseUrl = '/api/brickset-sync';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }


    /** POST /api/brickset-sync/connect — login + store credentials */
    connect(request: BrickSetConnectRequest): Observable<{ connected: boolean }> {
        return this.http.post<{ connected: boolean }>(
            `${this.baseUrl}/connect`,
            request,
            { headers: this.headers }
        );
    }


    /** POST /api/brickset-sync/disconnect — clear credentials */
    disconnect(): Observable<{ connected: boolean }> {
        return this.http.post<{ connected: boolean }>(
            `${this.baseUrl}/disconnect`,
            null,
            { headers: this.headers }
        );
    }


    /** GET /api/brickset-sync/status — current sync status for UI */
    getStatus(): Observable<BrickSetSyncStatus> {
        return this.http.get<BrickSetSyncStatus>(
            `${this.baseUrl}/status`,
            { headers: this.headers }
        );
    }


    /** GET /api/brickset-sync/hash-health — validate stored userHash */
    checkHashHealth(): Observable<{ valid: boolean; error: string | null }> {
        return this.http.get<{ valid: boolean; error: string | null }>(
            `${this.baseUrl}/hash-health`,
            { headers: this.headers }
        );
    }


    /** POST /api/brickset-sync/enrich/{setNumber} — enrich a single set */
    enrichSet(setNumber: string): Observable<{ enriched: boolean; setNumber: string }> {
        return this.http.post<{ enriched: boolean; setNumber: string }>(
            `${this.baseUrl}/enrich/${encodeURIComponent(setNumber)}`,
            null,
            { headers: this.headers }
        );
    }


    /** POST /api/brickset-sync/enrich-reviews — enrich reviews for a set */
    enrichReviews(legoSetId: number, brickSetId: number): Observable<{ reviewsCached: number }> {
        return this.http.post<{ reviewsCached: number }>(
            `${this.baseUrl}/enrich-reviews`,
            { legoSetId, brickSetId },
            { headers: this.headers }
        );
    }


    /** GET /api/brickset-sync/quota — check API quota remaining */
    getQuota(): Observable<{ apiCallsRemainingToday: number | null }> {
        return this.http.get<{ apiCallsRemainingToday: number | null }>(
            `${this.baseUrl}/quota`,
            { headers: this.headers }
        );
    }


    /** GET /api/brickset-sync/transactions — paginated audit log */
    getTransactions(
        pageSize: number = 50,
        pageNumber: number = 1,
        direction?: string,
        success?: boolean
    ): Observable<BrickSetTransactionsPage> {
        let params: any = { pageSize, pageNumber };
        if (direction) params.direction = direction;
        if (success !== undefined && success !== null) params.success = success;

        return this.http.get<BrickSetTransactionsPage>(
            `${this.baseUrl}/transactions`,
            { headers: this.headers, params }
        );
    }


    /** PUT /api/brickset-sync/settings — update sync direction */
    updateSettings(request: BrickSetUpdateSettingsRequest): Observable<any> {
        return this.http.put(
            `${this.baseUrl}/settings`,
            request,
            { headers: this.headers }
        );
    }
}
