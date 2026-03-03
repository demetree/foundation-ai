import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';


/**
 * Angular service for the BrickLink integration.
 *
 * Mirrors the BrickLinkSyncController endpoints on the server.
 * Follows the established pattern from brickset-sync.service.ts.
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 */
@Injectable({
    providedIn: 'root'
})
export class BrickLinkSyncService {

    private readonly baseUrl = '/api/bricklink-sync';


    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }


    // ─── Connection ───

    /**
     * Connect to BrickLink by providing OAuth token credentials.
     * The server validates the tokens against the BrickLink API before storing them.
     */
    connect(tokenValue: string, tokenSecret: string): Observable<{ connected: boolean }> {
        return this.http.post<{ connected: boolean }>(
            `${this.baseUrl}/connect`,
            { tokenValue, tokenSecret },
            { headers: this.getAuthHeaders() }
        );
    }

    /**
     * Disconnect from BrickLink — clears stored credentials.
     */
    disconnect(): Observable<{ connected: boolean }> {
        return this.http.post<{ connected: boolean }>(
            `${this.baseUrl}/disconnect`,
            {},
            { headers: this.getAuthHeaders() }
        );
    }


    // ─── Status & Health ───

    /**
     * Get the current BrickLink connection status.
     */
    getStatus(): Observable<BrickLinkStatus> {
        return this.http.get<BrickLinkStatus>(
            `${this.baseUrl}/status`,
            { headers: this.getAuthHeaders() }
        );
    }

    /**
     * Validate stored OAuth tokens against the BrickLink API.
     */
    getTokenHealth(): Observable<{ valid: boolean; error: string }> {
        return this.http.get<{ valid: boolean; error: string }>(
            `${this.baseUrl}/token-health`,
            { headers: this.getAuthHeaders() }
        );
    }


    // ─── Catalog & Price Guide ───

    /**
     * Get the BrickLink price guide for a specific item.
     *
     * @param type Item type (SET, PART, MINIFIG, etc.)
     * @param no Item number (e.g. "75192-1" for a set, "3001" for a part)
     * @param options Optional parameters: colorId, guideType (stock/sold), newOrUsed (N/U), currencyCode
     */
    getPriceGuide(type: string, no: string, options?: PriceGuideOptions): Observable<any> {
        let params = new HttpParams();

        if (options?.colorId != null) params = params.set('colorId', options.colorId.toString());
        if (options?.guideType) params = params.set('guideType', options.guideType);
        if (options?.newOrUsed) params = params.set('newOrUsed', options.newOrUsed);
        if (options?.currencyCode) params = params.set('currencyCode', options.currencyCode);

        return this.http.get(
            `${this.baseUrl}/price-guide/${type}/${no}`,
            { headers: this.getAuthHeaders(), params }
        );
    }

    /**
     * Get catalog item details from BrickLink.
     */
    getItem(type: string, no: string): Observable<any> {
        return this.http.get(
            `${this.baseUrl}/item/${type}/${no}`,
            { headers: this.getAuthHeaders() }
        );
    }

    /**
     * Get the subset (part-out) of a set — all parts contained within.
     */
    getSubsets(type: string, no: string, breakMinifigs?: boolean, breakSubsets?: boolean): Observable<any> {
        let params = new HttpParams();

        if (breakMinifigs != null) params = params.set('breakMinifigs', breakMinifigs.toString());
        if (breakSubsets != null) params = params.set('breakSubsets', breakSubsets.toString());

        return this.http.get(
            `${this.baseUrl}/subsets/${type}/${no}`,
            { headers: this.getAuthHeaders(), params }
        );
    }

    /**
     * Find sets that contain the specified part.
     */
    getSupersets(type: string, no: string, colorId?: number): Observable<any> {
        let params = new HttpParams();

        if (colorId != null) params = params.set('colorId', colorId.toString());

        return this.http.get(
            `${this.baseUrl}/supersets/${type}/${no}`,
            { headers: this.getAuthHeaders(), params }
        );
    }


    // ─── Private ───

    private getAuthHeaders(): HttpHeaders {
        return this.authService.getAuthorizationHeaders();
    }
}


// ─── DTOs ───

export interface BrickLinkStatus {
    isConnected: boolean;
    lastSyncDate: string | null;
    lastSyncError: string | null;
    message: string | null;
}

export interface PriceGuideOptions {
    colorId?: number;
    guideType?: 'stock' | 'sold';
    newOrUsed?: 'N' | 'U';
    currencyCode?: string;
}
