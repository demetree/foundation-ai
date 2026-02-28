import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { IndexedDBCacheService } from './indexeddb-cache.service';

//
// Parts Universe API Service
//
// Angular service for the PartsUniverseController endpoints.
// All data is precomputed server-side and served from memory,
// making requests near-instant.
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface ColourEntry {
    brickColourId: number;
    name: string;
    hex: string;
    qty: number;
}

export interface ThemeEntry {
    name: string;
    qty: number;
}

export interface RankedPart {
    brickPartId: number;
    name: string;
    ldrawPartId: string;
    ldrawTitle: string;
    geometryOriginalFileName: string;
    categoryName: string;
    partTypeName: string;
    totalQty: number;
    setCount: number;
    colours: ColourEntry[];
    themes: ThemeEntry[];
}

export interface SankeyNode {
    name: string;
    group: string;
}

export interface SankeyLink {
    source: number;
    target: number;
    value: number;
}

export interface SankeyData {
    nodes: SankeyNode[];
    links: SankeyLink[];
}

export interface BubblePart {
    name: string;
    totalQty: number;
    setCount: number;
    dominantColourHex: string;
}

export interface CategoryBubble {
    categoryName: string;
    parts: BubblePart[];
}

export interface HeatmapColourLabel {
    hex: string;
    name: string;
}

export interface HeatmapCell {
    partIdx: number;
    colourIdx: number;
    hex: string;
    qty: number;
    brickPartId: number;
    brickColourId: number;
}

export interface HeatmapData {
    partLabels: string[];
    colourLabels: HeatmapColourLabel[];
    cells: HeatmapCell[];
}

export interface ChordData {
    names: string[];
    matrix: number[][];
    categoryCount: number;
}

export interface SummaryStats {
    totalUniqueParts: number;
    totalInstances: number;
    totalSets: number;
    totalCategories: number;
}

export interface PartsUniversePayload {
    rankedParts: RankedPart[];
    sankey: SankeyData;
    bubbles: CategoryBubble[];
    heatmap: HeatmapData;
    chord: ChordData;
    stats: SummaryStats;
    computedAtUtc: string;
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class PartsUniverseApiService {
    private readonly baseUrl = '/api/parts-universe';

    private static readonly CACHE_STORE = 'parts-universe';
    private static readonly CACHE_TTL = 1440; // 24 hours

    constructor(
        private http: HttpClient,
        private authService: AuthService,
        private cacheService: IndexedDBCacheService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }

    /** GET /api/parts-universe — full precomputed payload (cached in IndexedDB for 24h) */
    getPayload(): Observable<PartsUniversePayload> {
        return this.cacheService.getOrFetch<PartsUniversePayload>(
            PartsUniverseApiService.CACHE_STORE,
            {},
            () => this.http.get<PartsUniversePayload>(this.baseUrl, { headers: this.headers }),
            PartsUniverseApiService.CACHE_TTL
        );
    }

    /** POST /api/parts-universe/refresh — trigger server-side recomputation and invalidate local cache */
    refresh(): Observable<{ message: string }> {
        return this.http.post<{ message: string }>(
            `${this.baseUrl}/refresh`,
            null,
            { headers: this.headers }
        ).pipe(
            tap(() => this.cacheService.invalidate(PartsUniverseApiService.CACHE_STORE))
        );
    }
}
