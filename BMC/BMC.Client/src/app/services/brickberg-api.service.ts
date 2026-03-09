///
/// AI-Developed: BrickbergApiService — authenticated HTTP service for the Brickberg Terminal.
///
/// Provides typed methods for all Brickberg API endpoints with proper Bearer token
/// auth headers and automatic token-refresh retry on 401.
///
/// This service was created to fix a bug where the brickberg-dashboard and set-detail
/// components used raw HttpClient.get() without auth headers, causing all Brickberg
/// API calls to return 401 from the SecureWebAPIController.
///
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { AlertService } from './alert.service';
import { AuthService } from './auth.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';


@Injectable({
    providedIn: 'root'
})
export class BrickbergApiService extends SecureEndpointBase {

    constructor(
        http: HttpClient,
        alertService: AlertService,
        authService: AuthService,
        @Inject('BASE_URL') private baseUrl: string
    ) {
        super(http, alertService, authService);
    }


    /**
     * GET /api/brickberg/set/{setNumber}
     *
     * Returns unified market data for a LEGO set by aggregating
     * BrickLink, BrickEconomy, and Brick Owl in parallel.
     */
    public getSetMarketData(setNumber: string): Observable<any> {

        const headers = this.getAuthHeaders();

        return this.http.get<any>(`${this.baseUrl}api/brickberg/set/${setNumber}`, {
            headers: headers
        }).pipe(
            catchError(error => this.handleError(error, () => this.getSetMarketData(setNumber)))
        );
    }


    /**
     * GET /api/brickberg/portfolio
     *
     * Returns portfolio summary cross-referencing owned sets with
     * cached BrickEconomy valuations.
     */
    public getPortfolio(): Observable<any> {

        const headers = this.getAuthHeaders();

        return this.http.get<any>(`${this.baseUrl}api/brickberg/portfolio`, {
            headers: headers
        }).pipe(
            catchError(error => this.handleError(error, () => this.getPortfolio()))
        );
    }


    /**
     * GET /api/brickberg/market-movers
     *
     * Returns top gainers, losers, and recently retired sets
     * from cached BrickEconomy valuations.
     */
    public getMarketMovers(): Observable<any> {

        const headers = this.getAuthHeaders();

        return this.http.get<any>(`${this.baseUrl}api/brickberg/market-movers`, {
            headers: headers
        }).pipe(
            catchError(error => this.handleError(error, () => this.getMarketMovers()))
        );
    }


    /**
     * GET /api/brickberg/status
     *
     * Returns connection status for each marketplace integration
     * (BrickLink, BrickEconomy, Brick Owl).
     */
    public getIntegrationStatus(): Observable<any> {

        const headers = this.getAuthHeaders();

        return this.http.get<any>(`${this.baseUrl}api/brickberg/status`, {
            headers: headers
        }).pipe(
            catchError(error => this.handleError(error, () => this.getIntegrationStatus()))
        );
    }


    /**
     * GET /api/brickberg/cache-stats
     *
     * Returns cache hit/miss metrics and entry statistics.
     */
    public getCacheStats(): Observable<any> {

        const headers = this.getAuthHeaders();

        return this.http.get<any>(`${this.baseUrl}api/brickberg/cache-stats`, {
            headers: headers
        }).pipe(
            catchError(error => this.handleError(error, () => this.getCacheStats()))
        );
    }


    /**
     * Builds the Authorization header with the current Bearer token.
     */
    private getAuthHeaders(): HttpHeaders {

        return new HttpHeaders({
            Authorization: 'Bearer ' + this.authService.accessToken
        });
    }
}
