import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// Collection Service
//
// Custom Angular service for the "My Collection" composite API.
// Calls the custom CollectionController endpoints (not the auto-generated CRUD ones).
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface CollectionSummary {
    id: number;
    name: string;
    description: string;
    isDefault: boolean;
    uniquePartCount: number;
    totalBrickCount: number;
    wishlistItemCount: number;
    importedSetCount: number;
}

export interface CollectionPart {
    id: number;
    brickPartId: number;
    partName: string;
    ldrawPartId: string;
    ldrawTitle: string;
    categoryName: string;
    geometryFilePath: string;
    brickColourId: number;
    colourName: string;
    colourHex: string;
    quantityOwned: number;
    quantityUsed: number;
}

export interface AddPartRequest {
    brickPartId: number;
    brickColourId: number;
    quantity: number;
}

export interface ImportSetResult {
    partsAdded: number;
    partsUpdated: number;
    totalQuantityAdded: number;
}

export interface SetSearchResult {
    id: number;
    name: string;
    setNumber: string;
    imageUrl: string | null;
    year: number;
    partCount: number;
    themeName: string | null;
}

export interface ImportedSetRecord {
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
    importedDate: Date;
}

export interface WishlistItem {
    id: number;
    brickPartId: number;
    partName: string;
    ldrawPartId: string;
    brickColourId: number | null;
    colourName: string;
    colourHex: string | null;
    quantityDesired: number | null;
    notes: string;
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class CollectionService {
    private readonly baseUrl = '/api/collection';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }


    /** GET /api/collection/mine — all collections with stats */
    getMyCollections(): Observable<CollectionSummary[]> {
        return this.http.get<CollectionSummary[]>(
            `${this.baseUrl}/mine`,
            { headers: this.headers }
        );
    }

    /** GET /api/collection/{id}/parts — paginated parts grid */
    getCollectionParts(
        collectionId: number,
        search?: string,
        pageSize?: number,
        pageNumber?: number
    ): Observable<CollectionPart[]> {
        let params: any = {};
        if (search) params.search = search;
        if (pageSize) params.pageSize = pageSize;
        if (pageNumber) params.pageNumber = pageNumber;

        return this.http.get<CollectionPart[]>(
            `${this.baseUrl}/${collectionId}/parts`,
            { headers: this.headers, params }
        );
    }

    /** POST /api/collection/{id}/add-part — upsert part */
    addPart(collectionId: number, request: AddPartRequest): Observable<any> {
        return this.http.post(
            `${this.baseUrl}/${collectionId}/add-part`,
            request,
            { headers: this.headers }
        );
    }

    /** DELETE /api/collection/{id}/remove-part/{partId} — soft-delete */
    removePart(collectionId: number, partId: number): Observable<any> {
        return this.http.delete(
            `${this.baseUrl}/${collectionId}/remove-part/${partId}`,
            { headers: this.headers }
        );
    }

    /** POST /api/collection/{id}/import-set/{setNumber} — bulk import */
    importSet(collectionId: number, setNumber: string, quantity: number = 1): Observable<ImportSetResult> {
        return this.http.post<ImportSetResult>(
            `${this.baseUrl}/${collectionId}/import-set/${encodeURIComponent(setNumber)}?quantity=${quantity}`,
            null,
            { headers: this.headers }
        );
    }

    /** GET /api/collection/{id}/imported-sets — import history */
    getImportedSets(collectionId: number): Observable<ImportedSetRecord[]> {
        return this.http.get<ImportedSetRecord[]>(
            `${this.baseUrl}/${collectionId}/imported-sets`,
            { headers: this.headers }
        );
    }

    /** GET /api/collection/{id}/wishlist — wishlist items */
    getWishlist(collectionId: number): Observable<WishlistItem[]> {
        return this.http.get<WishlistItem[]>(
            `${this.baseUrl}/${collectionId}/wishlist`,
            { headers: this.headers }
        );
    }

    /** GET /api/collection/search-sets?q= — typeahead for import modal */
    searchSets(query: string): Observable<SetSearchResult[]> {
        return this.http.get<SetSearchResult[]>(
            `${this.baseUrl}/search-sets`,
            { headers: this.headers, params: { q: query } }
        );
    }
}
