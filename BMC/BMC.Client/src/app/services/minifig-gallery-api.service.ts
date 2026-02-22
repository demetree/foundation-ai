import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AlertService } from './alert.service';
import { AuthService } from './auth.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';
import { IndexedDBCacheService } from './indexeddb-cache.service';


/**
 * Lean interface matching the server's MinifigGalleryItemDTO.
 * Contains only the fields needed by the minifig-gallery card grid.
 */
export interface MinifigGalleryItem {
    id: number;
    name: string;
    figNumber: string;
    partCount: number;
    imageUrl: string | null;
    year: number;
}


/**
 * Service for fetching the full minifig-gallery dataset from the custom
 * server endpoint. Results are cached in IndexedDB with a 24-hour TTL
 * so subsequent visits load instantly from the local cache.
 *
 * Usage:
 *   this.minifigGalleryApi.getGalleryMinifigs().subscribe(minifigs => { ... });
 */
@Injectable({
    providedIn: 'root'
})
export class MinifigGalleryApiService extends SecureEndpointBase {

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
     * Returns the full list of LEGO minifigs from the server's precomputed cache.
     * The data is cached in IndexedDB for 24 hours to avoid redundant network requests.
     */
    getGalleryMinifigs(): Observable<MinifigGalleryItem[]> {
        return this.cacheService.getOrFetch<MinifigGalleryItem[]>(
            'minifig-gallery',
            {},
            () => this.fetchFromServer(),
            1440   // 24 hours in minutes
        );
    }


    /**
     * Fetches fresh data from the server, bypassing the IndexedDB cache.
     */
    private fetchFromServer(): Observable<MinifigGalleryItem[]> {
        const url = this.baseUrl + 'api/minifig-gallery';
        const headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.authService.accessToken
        });

        return this.http.get<MinifigGalleryItem[]>(url, { headers }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.fetchFromServer());
            })
        );
    }
}
