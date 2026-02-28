import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// Parts Catalog Service
//
// Angular service for the custom PartsCatalogController endpoints.
// Provides server-side filtered, paginated access to renderable parts
// (those with LDraw geometry data).
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface CatalogPart {
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
}

export interface CatalogPageResult {
    totalCount: number;
    pageSize: number;
    pageNumber: number;
    totalPages: number;
    items: CatalogPart[];
}

export interface CatalogCategory {
    id: number;
    name: string;
    description: string | null;
    partCount: number;
}

export interface CatalogPartType {
    id: number;
    name: string;
    partCount: number;
}

export interface CatalogQueryParams {
    search?: string;
    categoryId?: number;
    partTypeId?: number;
    pageSize?: number;
    pageNumber?: number;
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class PartsCatalogService {
    private readonly baseUrl = '/api/parts-catalog';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }

    /** GET /api/parts-catalog — paginated, filtered parts list */
    getCatalogPage(params: CatalogQueryParams = {}): Observable<CatalogPageResult> {
        let httpParams = new HttpParams();

        if (params.search) httpParams = httpParams.set('search', params.search);
        if (params.categoryId) httpParams = httpParams.set('categoryId', params.categoryId.toString());
        if (params.partTypeId) httpParams = httpParams.set('partTypeId', params.partTypeId.toString());
        if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
        if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());

        return this.http.get<CatalogPageResult>(
            this.baseUrl,
            { headers: this.headers, params: httpParams }
        );
    }

    /** GET /api/parts-catalog/categories — categories with renderable part counts */
    getCategories(): Observable<CatalogCategory[]> {
        return this.http.get<CatalogCategory[]>(
            `${this.baseUrl}/categories`,
            { headers: this.headers }
        );
    }

    /** GET /api/parts-catalog/part-types — part types with renderable part counts */
    getPartTypes(): Observable<CatalogPartType[]> {
        return this.http.get<CatalogPartType[]>(
            `${this.baseUrl}/part-types`,
            { headers: this.headers }
        );
    }
}
