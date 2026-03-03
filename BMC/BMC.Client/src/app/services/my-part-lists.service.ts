import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// My Part Lists Service
//
// Custom Angular service for the "My Part Lists" controller.
// Calls the custom MyPartListsController endpoints.
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface PartListSummary {
    id: number;
    name: string;
    isBuildable: boolean;
    rebrickableListId: number | null;
    itemCount: number;
    totalParts: number;
    uniqueColours: number;
    versionNumber: number;
}

export interface PartListItem {
    id: number;
    brickPartId: number;
    partName: string;
    partNum: string;
    partImageUrl: string | null;
    partCategory: string | null;
    rebrickablePartUrl: string | null;
    brickColourId: number;
    colourName: string;
    colourHex: string | null;
    isTransparent: boolean;
    quantity: number;
}

export interface PartListDetail {
    id: number;
    name: string;
    isBuildable: boolean;
    rebrickableListId: number | null;
    versionNumber: number;
    items: PartListItem[];
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class MyPartListsService {
    private readonly baseUrl = '/api/my-part-lists';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }


    /** GET /api/my-part-lists — all part lists with summary stats */
    getAll(): Observable<PartListSummary[]> {
        return this.http.get<PartListSummary[]>(
            this.baseUrl,
            { headers: this.headers }
        );
    }

    /** GET /api/my-part-lists/{id} — single list with denormalized items */
    getById(id: number): Observable<PartListDetail> {
        return this.http.get<PartListDetail>(
            `${this.baseUrl}/${id}`,
            { headers: this.headers }
        );
    }

    /** POST /api/my-part-lists — create a new part list */
    create(name: string, isBuildable: boolean = false): Observable<{ id: number; name: string }> {
        return this.http.post<{ id: number; name: string }>(
            this.baseUrl,
            { name, isBuildable },
            { headers: this.headers }
        );
    }

    /** PUT /api/my-part-lists/{id} — update list properties */
    update(id: number, name?: string, isBuildable?: boolean): Observable<any> {
        return this.http.put(
            `${this.baseUrl}/${id}`,
            { name, isBuildable },
            { headers: this.headers }
        );
    }

    /** DELETE /api/my-part-lists/{id} — soft-delete list */
    delete(id: number): Observable<any> {
        return this.http.delete(
            `${this.baseUrl}/${id}`,
            { headers: this.headers }
        );
    }

    /** POST /api/my-part-lists/{id}/items — add a part to the list */
    addItem(listId: number, brickPartId: number, brickColourId: number, quantity: number = 1): Observable<any> {
        return this.http.post(
            `${this.baseUrl}/${listId}/items`,
            { brickPartId, brickColourId, quantity },
            { headers: this.headers }
        );
    }

    /** PUT /api/my-part-lists/{id}/items/{itemId} — update item */
    updateItem(listId: number, itemId: number, quantity?: number, brickColourId?: number): Observable<any> {
        return this.http.put(
            `${this.baseUrl}/${listId}/items/${itemId}`,
            { quantity, brickColourId },
            { headers: this.headers }
        );
    }

    /** DELETE /api/my-part-lists/{id}/items/{itemId} — remove item from list */
    removeItem(listId: number, itemId: number): Observable<any> {
        return this.http.delete(
            `${this.baseUrl}/${listId}/items/${itemId}`,
            { headers: this.headers }
        );
    }
}
