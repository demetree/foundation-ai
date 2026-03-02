import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// Set List Service
//
// Custom Angular service for the "My Set Lists" controller.
// Calls the custom UserSetListController endpoints.
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface SetListSummary {
    id: number;
    name: string;
    isBuildable: boolean;
    rebrickableListId: number | null;
    itemCount: number;
    totalSets: number;
    totalParts: number;
    versionNumber: number;
    pendingSyncCount: number;
}

export interface SetListItem {
    id: number;
    legoSetId: number;
    setName: string;
    setNumber: string;
    imageUrl: string | null;
    year: number;
    partCount: number;
    themeName: string | null;
    rebrickableUrl: string | null;
    quantity: number;
    includeSpares: boolean;
}

export interface SetListDetail {
    id: number;
    name: string;
    isBuildable: boolean;
    rebrickableListId: number | null;
    versionNumber: number;
    pendingSyncCount: number;
    items: SetListItem[];
}

export interface VersionHistoryEntry {
    versionNumber: number;
    changeDescription: string;
    changeDate: string;
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class SetListService {
    private readonly baseUrl = '/api/user-set-lists';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }


    /** GET /api/user-set-lists — all set lists with summary stats */
    getAll(): Observable<SetListSummary[]> {
        return this.http.get<SetListSummary[]>(
            this.baseUrl,
            { headers: this.headers }
        );
    }

    /** GET /api/user-set-lists/{id} — single set list with items */
    getById(id: number): Observable<SetListDetail> {
        return this.http.get<SetListDetail>(
            `${this.baseUrl}/${id}`,
            { headers: this.headers }
        );
    }

    /** POST /api/user-set-lists — create a new set list */
    create(name: string, isBuildable: boolean = false): Observable<{ id: number; name: string }> {
        return this.http.post<{ id: number; name: string }>(
            this.baseUrl,
            { name, isBuildable },
            { headers: this.headers }
        );
    }

    /** PUT /api/user-set-lists/{id} — update set list properties */
    update(id: number, name?: string, isBuildable?: boolean): Observable<any> {
        return this.http.put(
            `${this.baseUrl}/${id}`,
            { name, isBuildable },
            { headers: this.headers }
        );
    }

    /** DELETE /api/user-set-lists/{id} — soft-delete set list */
    delete(id: number): Observable<any> {
        return this.http.delete(
            `${this.baseUrl}/${id}`,
            { headers: this.headers }
        );
    }

    /** POST /api/user-set-lists/{id}/items — add a set to the list */
    addItem(listId: number, legoSetId: number, quantity: number = 1, includeSpares: boolean = true): Observable<any> {
        return this.http.post(
            `${this.baseUrl}/${listId}/items`,
            { legoSetId, quantity, includeSpares },
            { headers: this.headers }
        );
    }

    /** PUT /api/user-set-lists/{id}/items/{itemId} — update item quantity/spares */
    updateItem(listId: number, itemId: number, quantity?: number, includeSpares?: boolean): Observable<any> {
        return this.http.put(
            `${this.baseUrl}/${listId}/items/${itemId}`,
            { quantity, includeSpares },
            { headers: this.headers }
        );
    }

    /** DELETE /api/user-set-lists/{id}/items/{itemId} — remove item from list */
    removeItem(listId: number, itemId: number): Observable<any> {
        return this.http.delete(
            `${this.baseUrl}/${listId}/items/${itemId}`,
            { headers: this.headers }
        );
    }

    /** GET /api/user-set-lists/{id}/history — version change history */
    getHistory(id: number): Observable<VersionHistoryEntry[]> {
        return this.http.get<VersionHistoryEntry[]>(
            `${this.baseUrl}/${id}/history`,
            { headers: this.headers }
        );
    }
}
