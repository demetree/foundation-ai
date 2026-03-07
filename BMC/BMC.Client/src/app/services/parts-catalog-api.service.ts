import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AlertService } from './alert.service';
import { AuthService } from './auth.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';
import { IndexedDBCacheService } from './indexeddb-cache.service';


/**
 * Lean interface matching the server's CatalogPartDto.
 * Contains the fields needed by the parts-catalog card grid,
 * including setCount for relevance-based sorting.
 */
export interface CatalogPartItem {
    id: number;
    name: string;
    ldrawPartId: string;
    ldrawTitle: string | null;
    ldrawCategory: string | null;
    brickCategoryId: number;
    categoryName: string | null;
    partTypeId: number;
    partTypeName: string | null;
    geometryOriginalFileName: string;
    keywords: string | null;
    author: string | null;
    widthLdu: number | null;
    heightLdu: number | null;
    depthLdu: number | null;
    massGrams: number | null;
    versionNumber: number;
    setCount: number;
}

/**
 * Lean interface for category sidebar entries.
 */
export interface CatalogCategory {
    id: number;
    name: string;
    description: string | null;
    partCount: number;
}

/**
 * Lean interface for part-type sidebar entries.
 */
export interface CatalogPartType {
    id: number;
    name: string;
    partCount: number;
}


/**
 * Service for fetching the full parts-catalog dataset from the
 * custom server endpoint. Results are cached in IndexedDB with a
 * 24-hour TTL so subsequent visits load instantly from the local cache.
 *
 * Usage:
 *   this.partsCatalogApi.getAllParts().subscribe(parts => { ... });
 */
@Injectable({
    providedIn: 'root'
})
export class PartsCatalogApiService extends SecureEndpointBase {

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
     * Returns the full list of renderable parts with setCount.
     * Cached in IndexedDB for 24 hours.
     */
    getAllParts(): Observable<CatalogPartItem[]> {
        return this.cacheService.getOrFetch<CatalogPartItem[]>(
            'parts-catalog',
            {},
            () => this.fetchFromServer(),
            1440   // 24 hours in minutes
        );
    }

    /**
     * Returns categories with renderable-part counts.
     * Cached in IndexedDB for 24 hours.
     */
    getCategories(): Observable<CatalogCategory[]> {
        return this.cacheService.getOrFetch<CatalogCategory[]>(
            'parts-catalog-categories',
            {},
            () => this.fetchCategoriesFromServer(),
            1440
        );
    }

    /**
     * Returns part types with renderable-part counts.
     * Cached in IndexedDB for 24 hours.
     */
    getPartTypes(): Observable<CatalogPartType[]> {
        return this.cacheService.getOrFetch<CatalogPartType[]>(
            'parts-catalog-part-types',
            {},
            () => this.fetchPartTypesFromServer(),
            1440
        );
    }

    /**
     * Returns a map of partId → top-N most common colour hex values.
     * Used for realistic colour assignment in the catalog grid.
     * Cached in IndexedDB for 24 hours.
     */
    getPartColours(): Observable<{ [partId: number]: string[] }> {
        return this.cacheService.getOrFetch<{ [partId: number]: string[] }>(
            'parts-catalog-part-colours',
            {},
            () => this.fetchPartColoursFromServer(),
            1440
        );
    }


    private fetchFromServer(): Observable<CatalogPartItem[]> {
        if (this.authService.isLoggedIn) {
            const url = this.baseUrl + 'api/parts-catalog/all';
            const headers = new HttpHeaders({
                Authorization: 'Bearer ' + this.authService.accessToken
            });
            return this.http.get<CatalogPartItem[]>(url, { headers }).pipe(
                catchError(error => this.handleError(error, () => this.fetchFromServer()))
            );
        } else {
            const url = this.baseUrl + 'api/public/browse/catalog/all';
            return this.http.get<CatalogPartItem[]>(url).pipe(
                catchError(error => this.handleError(error, () => this.fetchFromServer()))
            );
        }
    }

    private fetchCategoriesFromServer(): Observable<CatalogCategory[]> {
        if (this.authService.isLoggedIn) {
            const url = this.baseUrl + 'api/parts-catalog/categories';
            const headers = new HttpHeaders({
                Authorization: 'Bearer ' + this.authService.accessToken
            });
            return this.http.get<CatalogCategory[]>(url, { headers }).pipe(
                catchError(error => this.handleError(error, () => this.fetchCategoriesFromServer()))
            );
        } else {
            const url = this.baseUrl + 'api/public/browse/catalog/categories';
            return this.http.get<CatalogCategory[]>(url).pipe(
                catchError(error => this.handleError(error, () => this.fetchCategoriesFromServer()))
            );
        }
    }

    private fetchPartTypesFromServer(): Observable<CatalogPartType[]> {
        if (this.authService.isLoggedIn) {
            const url = this.baseUrl + 'api/parts-catalog/part-types';
            const headers = new HttpHeaders({
                Authorization: 'Bearer ' + this.authService.accessToken
            });
            return this.http.get<CatalogPartType[]>(url, { headers }).pipe(
                catchError(error => this.handleError(error, () => this.fetchPartTypesFromServer()))
            );
        } else {
            const url = this.baseUrl + 'api/public/browse/catalog/part-types';
            return this.http.get<CatalogPartType[]>(url).pipe(
                catchError(error => this.handleError(error, () => this.fetchPartTypesFromServer()))
            );
        }
    }

    private fetchPartColoursFromServer(): Observable<{ [partId: number]: string[] }> {
        if (this.authService.isLoggedIn) {
            const url = this.baseUrl + 'api/parts-catalog/part-colours?top=10';
            const headers = new HttpHeaders({
                Authorization: 'Bearer ' + this.authService.accessToken
            });
            return this.http.get<{ [partId: number]: string[] }>(url, { headers }).pipe(
                catchError(error => this.handleError(error, () => this.fetchPartColoursFromServer()))
            );
        } else {
            const url = this.baseUrl + 'api/public/browse/catalog/part-colours?top=10';
            return this.http.get<{ [partId: number]: string[] }>(url).pipe(
                catchError(error => this.handleError(error, () => this.fetchPartColoursFromServer()))
            );
        }
    }
}

