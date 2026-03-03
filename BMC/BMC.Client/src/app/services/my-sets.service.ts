import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// My Sets Service
//
// Custom Angular service for the "My Sets" controller.
// Calls the custom MySetsController endpoints.
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface OwnedSet {
    id: number;
    legoSetId: number;
    setName: string;
    setNumber: string;
    imageUrl: string | null;
    year: number;
    partCount: number;
    themeName: string | null;
    rebrickableUrl: string | null;
    status: string;
    acquiredDate: string | null;
    personalRating: number | null;
    notes: string | null;
    quantity: number;
    isPublic: boolean;
}

export interface OwnedSetsPage {
    items: OwnedSet[];
    totalCount: number;
}

export interface CollectionStats {
    totalSets: number;
    uniqueSets: number;
    totalParts: number;
    totalThemes: number;
    averageRating: number;
}

export interface ThemeOption {
    id: number;
    name: string;
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class MySetsService {
    private readonly baseUrl = '/api/my-sets';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }


    /** GET /api/my-sets — all owned sets with search, filters, sorting, pagination */
    getAll(params: {
        search?: string;
        status?: string;
        themeId?: number;
        sortBy?: string;
        pageSize?: number;
        pageNumber?: number;
    } = {}): Observable<OwnedSetsPage> {
        let httpParams = new HttpParams();
        if (params.search) httpParams = httpParams.set('search', params.search);
        if (params.status) httpParams = httpParams.set('status', params.status);
        if (params.themeId) httpParams = httpParams.set('themeId', params.themeId.toString());
        if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
        if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
        if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());

        return this.http.get<OwnedSetsPage>(
            this.baseUrl,
            { headers: this.headers, params: httpParams }
        );
    }

    /** GET /api/my-sets/stats — collection statistics */
    getStats(): Observable<CollectionStats> {
        return this.http.get<CollectionStats>(
            `${this.baseUrl}/stats`,
            { headers: this.headers }
        );
    }

    /** GET /api/my-sets/themes — distinct themes for filter dropdown */
    getThemes(): Observable<ThemeOption[]> {
        return this.http.get<ThemeOption[]>(
            `${this.baseUrl}/themes`,
            { headers: this.headers }
        );
    }

    /** GET /api/my-sets/{id} — single owned set detail */
    getById(id: number): Observable<OwnedSet> {
        return this.http.get<OwnedSet>(
            `${this.baseUrl}/${id}`,
            { headers: this.headers }
        );
    }

    /** POST /api/my-sets — add a set to collection */
    addSet(legoSetId: number, quantity: number = 1, status: string = 'Owned'): Observable<any> {
        return this.http.post(
            this.baseUrl,
            { legoSetId, quantity, status },
            { headers: this.headers }
        );
    }

    /** PUT /api/my-sets/{id} — update ownership details */
    updateSet(id: number, updates: {
        quantity?: number;
        status?: string;
        notes?: string;
        personalRating?: number;
        isPublic?: boolean;
    }): Observable<any> {
        return this.http.put(
            `${this.baseUrl}/${id}`,
            updates,
            { headers: this.headers }
        );
    }

    /** DELETE /api/my-sets/{id} — remove from collection */
    removeSet(id: number): Observable<any> {
        return this.http.delete(
            `${this.baseUrl}/${id}`,
            { headers: this.headers }
        );
    }
}
