import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AlertService } from './alert.service';
import { AuthService } from './auth.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';
import { IndexedDBCacheService } from './indexeddb-cache.service';


/**
 * Lean interface matching the server's SetExplorerItemDTO.
 * Contains only the fields needed by the set-explorer card grid.
 */
export interface SetExplorerItem {
    id: number;
    name: string;
    setNumber: string;
    year: number;
    partCount: number;
    imageUrl: string | null;
    themeId: number | null;
    themeName: string | null;
}


/**
 * Service for fetching the full set-explorer dataset from the custom
 * server endpoint. Results are cached in IndexedDB with a 24-hour TTL
 * so subsequent visits load instantly from the local cache.
 *
 * Usage:
 *   this.setExplorerApiService.getExploreSets().subscribe(sets => { ... });
 */
@Injectable({
    providedIn: 'root'
})
export class SetExplorerApiService extends SecureEndpointBase {

    constructor(
        http: HttpClient,
        alertService: AlertService,
        authService: AuthService,
        private cacheService: IndexedDBCacheService,
        @Inject('BASE_URL') private baseUrl: string
    ) {
        super(http, alertService, authService);
    }


    /**
     * Returns the full list of LEGO sets from the server's precomputed cache.
     * The data is cached in IndexedDB for 24 hours to avoid redundant network requests.
     */
    getExploreSets(): Observable<SetExplorerItem[]> {
        return this.cacheService.getOrFetch<SetExplorerItem[]>(
            'set-explorer',
            {},
            () => this.fetchFromServer(),
            1440   // 24 hours in minutes
        );
    }


    /**
     * Fetches fresh data from the server, bypassing the IndexedDB cache.
     */
    private fetchFromServer(): Observable<SetExplorerItem[]> {
        if (this.authService.isLoggedIn) {
            const url = this.baseUrl + 'api/set-explorer';
            const headers = new HttpHeaders({
                Authorization: 'Bearer ' + this.authService.accessToken
            });
            return this.http.get<SetExplorerItem[]>(url, { headers }).pipe(
                catchError(error => this.handleError(error, () => this.fetchFromServer()))
            );
        } else {
            const url = this.baseUrl + 'api/public/browse/sets';
            return this.http.get<SetExplorerItem[]>(url).pipe(
                catchError(error => this.handleError(error, () => this.fetchFromServer()))
            );
        }
    }
}

