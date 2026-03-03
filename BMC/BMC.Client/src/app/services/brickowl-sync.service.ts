import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';


/**
 * Angular service for the Brick Owl integration.
 *
 * Mirrors the BrickOwlSyncController endpoints on the server.
 * Brick Owl is the second-largest LEGO marketplace with cross-platform
 * ID mapping, pricing/availability, collection, and wishlist support.
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 */
@Injectable({
    providedIn: 'root'
})
export class BrickOwlSyncService {

    private readonly baseUrl = '/api/brickowl-sync';


    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }


    // ─── Connection ───

    connect(apiKey: string): Observable<{ connected: boolean }> {
        return this.http.post<{ connected: boolean }>(
            `${this.baseUrl}/connect`,
            { apiKey },
            { headers: this.getAuthHeaders() }
        );
    }

    disconnect(): Observable<{ connected: boolean }> {
        return this.http.post<{ connected: boolean }>(
            `${this.baseUrl}/disconnect`,
            {},
            { headers: this.getAuthHeaders() }
        );
    }


    // ─── Status ───

    getStatus(): Observable<BrickOwlStatus> {
        return this.http.get<BrickOwlStatus>(
            `${this.baseUrl}/status`,
            { headers: this.getAuthHeaders() }
        );
    }


    // ─── Catalog ───

    /**
     * Look up a catalog item by its Brick Owl ID (BOID).
     */
    catalogLookup(boid: string): Observable<any> {
        return this.http.get(
            `${this.baseUrl}/catalog/lookup/${boid}`,
            { headers: this.getAuthHeaders() }
        );
    }

    /**
     * Map an external ID (BrickLink, LEGO set number, design ID) to Brick Owl BOIDs.
     */
    catalogIdLookup(id: string, type: string, idType?: string): Observable<any> {
        let params = new HttpParams()
            .set('id', id)
            .set('type', type);
        if (idType) params = params.set('idType', idType);

        return this.http.get(
            `${this.baseUrl}/catalog/id-lookup`,
            { headers: this.getAuthHeaders(), params }
        );
    }

    /**
     * Get pricing and availability for an item by BOID.
     */
    catalogAvailability(boid: string): Observable<any> {
        return this.http.get(
            `${this.baseUrl}/catalog/availability/${boid}`,
            { headers: this.getAuthHeaders() }
        );
    }


    // ─── Collection & Wishlists ───

    getCollection(): Observable<any> {
        return this.http.get(
            `${this.baseUrl}/collection`,
            { headers: this.getAuthHeaders() }
        );
    }

    getWishlists(): Observable<any> {
        return this.http.get(
            `${this.baseUrl}/wishlists`,
            { headers: this.getAuthHeaders() }
        );
    }

    getWishlistItems(wishlistId: string): Observable<any> {
        return this.http.get(
            `${this.baseUrl}/wishlist/${wishlistId}/items`,
            { headers: this.getAuthHeaders() }
        );
    }


    // ─── Transactions ───

    /**
     * Get paginated transaction history for Brick Owl API calls.
     */
    getTransactions(
        pageSize: number = 50,
        pageNumber: number = 1,
        direction?: string,
        success?: boolean
    ): Observable<BrickOwlTransactionsPage> {
        let params: any = { pageSize, pageNumber };
        if (direction) params.direction = direction;
        if (success !== undefined && success !== null) params.success = success;

        return this.http.get<BrickOwlTransactionsPage>(
            `${this.baseUrl}/transactions`,
            { headers: this.getAuthHeaders(), params }
        );
    }


    // ─── Private ───

    private getAuthHeaders(): HttpHeaders {
        return this.authService.GetAuthenticationHeaders();
    }
}


// ─── DTOs ───

export interface BrickOwlStatus {
    isConnected: boolean;
    lastSyncDate: string | null;
    lastSyncError: string | null;
    message: string | null;
}

export interface BrickOwlTransaction {
    id: number;
    transactionDate: string;
    direction: string;
    methodName: string;
    requestSummary: string;
    success: boolean;
    errorMessage: string | null;
    triggeredBy: string;
    recordCount: number | null;
}

export interface BrickOwlTransactionsPage {
    totalCount: number;
    pageSize: number;
    pageNumber: number;
    results: BrickOwlTransaction[];
}
