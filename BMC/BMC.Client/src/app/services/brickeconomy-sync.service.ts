import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';


/**
 * Angular service for the BrickEconomy integration.
 *
 * Mirrors the BrickEconomySyncController endpoints on the server.
 * BrickEconomy provides AI/ML-powered valuation data for LEGO sets and minifigures.
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 */
@Injectable({
    providedIn: 'root'
})
export class BrickEconomySyncService {

    private readonly baseUrl = '/api/brickeconomy-sync';


    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }


    // ─── Connection ───

    /**
     * Connect to BrickEconomy by providing a Premium API key.
     * The server validates the key against the BrickEconomy API before storing it.
     */
    connect(apiKey: string): Observable<{ connected: boolean }> {
        return this.http.post<{ connected: boolean }>(
            `${this.baseUrl}/connect`,
            { apiKey },
            { headers: this.getAuthHeaders() }
        );
    }

    /**
     * Disconnect from BrickEconomy — clears stored API key.
     */
    disconnect(): Observable<{ connected: boolean }> {
        return this.http.post<{ connected: boolean }>(
            `${this.baseUrl}/disconnect`,
            {},
            { headers: this.getAuthHeaders() }
        );
    }


    // ─── Status ───

    /**
     * Get the current BrickEconomy connection status.
     */
    getStatus(): Observable<BrickEconomyStatus> {
        return this.http.get<BrickEconomyStatus>(
            `${this.baseUrl}/status`,
            { headers: this.getAuthHeaders() }
        );
    }


    // ─── Set & Minifig Valuation ───

    /**
     * Get AI-powered valuation for a specific LEGO set.
     * Returns current value, forecast value, growth %, and price history.
     *
     * @param setNumber Set number (e.g. "75192-1")
     * @param currency Optional ISO 4217 currency code (default: USD)
     */
    getSetValuation(setNumber: string, currency?: string): Observable<any> {
        let params = new HttpParams();
        if (currency) params = params.set('currency', currency);

        return this.http.get(
            `${this.baseUrl}/set/${setNumber}`,
            { headers: this.getAuthHeaders(), params }
        );
    }

    /**
     * Get valuation for a specific LEGO minifigure.
     *
     * @param minifigNumber Minifig ID (e.g. "sw0001a")
     * @param currency Optional ISO 4217 currency code
     */
    getMinifigValuation(minifigNumber: string, currency?: string): Observable<any> {
        let params = new HttpParams();
        if (currency) params = params.set('currency', currency);

        return this.http.get(
            `${this.baseUrl}/minifig/${minifigNumber}`,
            { headers: this.getAuthHeaders(), params }
        );
    }


    // ─── Collection ───

    /**
     * Get the user's BrickEconomy set collection with current valuations.
     */
    getCollectionSets(currency?: string): Observable<any> {
        let params = new HttpParams();
        if (currency) params = params.set('currency', currency);

        return this.http.get(
            `${this.baseUrl}/collection/sets`,
            { headers: this.getAuthHeaders(), params }
        );
    }

    /**
     * Get the user's BrickEconomy minifig collection with current valuations.
     */
    getCollectionMinifigs(currency?: string): Observable<any> {
        let params = new HttpParams();
        if (currency) params = params.set('currency', currency);

        return this.http.get(
            `${this.baseUrl}/collection/minifigs`,
            { headers: this.getAuthHeaders(), params }
        );
    }


    // ─── Sales Ledger ───

    /**
     * Get the user's sales ledger — transaction history.
     */
    getSalesLedger(): Observable<any> {
        return this.http.get(
            `${this.baseUrl}/salesledger`,
            { headers: this.getAuthHeaders() }
        );
    }


    // ─── Private ───

    private getAuthHeaders(): HttpHeaders {
        return this.authService.GetAuthenticationHeaders();
    }
}


// ─── DTOs ───

export interface BrickEconomyStatus {
    isConnected: boolean;
    lastSyncDate: string | null;
    lastSyncError: string | null;
    dailyQuotaUsed: number;
    dailyQuotaLimit: number;
    message: string | null;
}
