import { Component, OnInit, OnDestroy, HostListener, ChangeDetectorRef, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, Subscription, forkJoin } from 'rxjs';
import { debounceTime, takeUntil, bufferTime, filter } from 'rxjs/operators';
import { PartsCatalogApiService, CatalogPartItem, CatalogCategory, CatalogPartType } from '../../services/parts-catalog-api.service';
import { LDrawThumbnailService, ThumbnailRequest } from '../../services/ldraw-thumbnail.service';
import { AuthService } from '../../services/auth.service';


/**
 * An internal type representing a single "row" inside the virtual-scroll viewport.
 * Each row contains `columnsPerRow` part cards rendered horizontally.
 */
interface CardRow {
    items: CatalogPartItem[];
}


@Component({
    selector: 'app-parts-catalog',
    templateUrl: './parts-catalog.component.html',
    styleUrl: './parts-catalog.component.scss'
})
export class PartsCatalogComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();
    private loadSub = new Subscription();

    // 3D thumbnail cache: cacheKey → data:URL
    thumbnails = new Map<string, string>();

    //
    // Per-part colour map: partId → hex[] sorted by frequency.
    // Loaded from the server to assign realistic colours to each part.
    // Falls back to a small neutral palette if no data is available.
    //
    private partColoursMap: { [partId: number]: string[] } = {};

    private readonly FALLBACK_PALETTE = [
        '05131D',  // Black
        'C91A09',  // Red
        '0055BF',  // Blue
        'F2CD37',  // Yellow
        '237841',  // Green
        'FFFFFF',  // White
    ];

    //
    // Full datasets loaded once from the API / IndexedDB cache
    //
    private allParts: CatalogPartItem[] = [];
    categories: CatalogCategory[] = [];
    partTypes: CatalogPartType[] = [];

    //
    // Derived from allParts after applying filters + sort
    //
    filteredParts: CatalogPartItem[] = [];

    //
    // Virtual-scroll rows: each row is a horizontal slice of N cards
    //
    cardRows: CardRow[] = [];
    columnsPerRow = 6;
    rowHeight = 260;   // px — matches the card height in SCSS

    loading = true;
    searchTerm = '';
    selectedCategoryId: number | null = null;
    selectedPartTypeId: number | null = null;
    viewMode: 'grid' | 'list' = 'grid';

    // Filter dropdown open/closed state
    categoryDropdownOpen = false;
    partTypeDropdownOpen = false;

    totalCount = 0;

    // Sort
    sortBy: 'relevance' | 'name' | 'category' = 'relevance';
    sortDirection: 'asc' | 'desc' = 'desc';

    // User preferred theme IDs — for category boosting
    userPreferredThemeIds: number[] = [];

    // Boosted category IDs (kept separately for UI ★ indicator)
    boostedCategoryIds = new Set<number>();

    constructor(
        private catalogApi: PartsCatalogApiService,
        private router: Router,
        private thumbnailService: LDrawThumbnailService,
        private http: HttpClient,
        private authService: AuthService,
        private cdr: ChangeDetectorRef,
        private elementRef: ElementRef
    ) { }

    ngOnInit(): void {
        this.calculateColumns();

        this.searchSubject.pipe(
            debounceTime(250),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.applyPipeline();
        });

        //
        // Subscribe to 3D thumbnail render results — updates cards progressively.
        // Buffer results over 100ms to batch change-detection cycles; a raw Map.set()
        // does NOT trigger Angular change detection, which causes thumbnails to
        // appear inconsistently without this.
        //
        this.thumbnailService.thumbnail$.pipe(
            takeUntil(this.destroy$),
            bufferTime(100),
            filter(batch => batch.length > 0)
        ).subscribe(batch => {
            for (const result of batch) {
                this.thumbnails.set(result.cacheKey, result.dataUrl);
            }
            this.cdr.markForCheck();
        });

        this.loadData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.loadSub.unsubscribe();

        //
        // Cancel any in-flight thumbnail rendering and abort pending HTTP requests.
        // Without this, sub-file fetches from the previous batch continue running
        // in the background after navigating away, compounding with new requests.
        //
        this.thumbnailService.cancelAll();
    }

    @HostListener('document:keydown', ['$event'])
    onKeydown(event: KeyboardEvent): void {
        const tag = (event.target as HTMLElement)?.tagName;
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') {
            if (event.key === 'Escape') {
                (event.target as HTMLElement).blur();
                this.searchTerm = '';
                this.searchSubject.next('');
            }
            return;
        }
        if (event.key === 's' || event.key === 'S') {
            event.preventDefault();
            document.getElementById('parts-search')?.focus();
        }
    }


    loadData(): void {
        this.loading = true;

        // Load user's preferred themes for category boosting (non-blocking)
        const headers = this.authService.GetAuthenticationHeaders();
        this.http.get<any>('/api/profile/mine', { headers }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (profile) => {
                this.userPreferredThemeIds = (profile.preferredThemes || []).map((pt: any) => pt.legoThemeId);
            },
            error: () => {
                this.userPreferredThemeIds = [];
            }
        });

        // Load all data in parallel (including per-part colours)
        this.loadSub = forkJoin({
            parts: this.catalogApi.getAllParts(),
            categories: this.catalogApi.getCategories(),
            partTypes: this.catalogApi.getPartTypes(),
            partColours: this.catalogApi.getPartColours()
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (data) => {
                this.allParts = data.parts;
                this.categories = data.categories;
                this.partTypes = data.partTypes;
                this.partColoursMap = data.partColours || {};
                this.totalCount = data.parts.length;
                this.applyPipeline();
                this.loading = false;
            },
            error: () => {
                this.allParts = [];
                this.categories = [];
                this.partTypes = [];
                this.partColoursMap = {};
                this.loading = false;
            }
        });
    }


    // ----------------------------------------------------------------
    //  Filter + Sort Pipeline  (operates on full dataset in memory)
    // ----------------------------------------------------------------

    private applyPipeline(): void {
        let result = this.allParts;

        //
        // Category filter
        //
        if (this.selectedCategoryId !== null) {
            result = result.filter(p => p.brickCategoryId === this.selectedCategoryId);
        }

        //
        // Part type filter
        //
        if (this.selectedPartTypeId !== null) {
            result = result.filter(p => p.partTypeId === this.selectedPartTypeId);
        }

        //
        // Text search — name, ldrawPartId, ldrawTitle, keywords
        //
        if (this.searchTerm) {
            const lower = this.searchTerm.toLowerCase();
            result = result.filter(p =>
                (p.name || '').toLowerCase().includes(lower) ||
                (p.ldrawPartId || '').toLowerCase().includes(lower) ||
                (p.ldrawTitle || '').toLowerCase().includes(lower) ||
                (p.keywords || '').toLowerCase().includes(lower)
            );
        }

        //
        // Sorting
        //
        result = this.applySorting(result);

        this.filteredParts = result;
        this.buildCardRows();

        // Kick off 3D thumbnails for visible parts (first ~100 items)
        const visibleBatch = this.filteredParts.slice(0, Math.min(100, this.filteredParts.length));
        const requests: ThumbnailRequest[] = visibleBatch.map(p => ({
            geometryFilePath: p.geometryFilePath,
            colourHex: this.getHexForPart(p)
        }));
        this.thumbnailService.renderBatch(requests);
    }


    private applySorting(parts: CatalogPartItem[]): CatalogPartItem[] {
        const dir = this.sortDirection === 'asc' ? 1 : -1;

        return [...parts].sort((a, b) => {
            switch (this.sortBy) {
                case 'relevance':
                    return (a.setCount - b.setCount) * dir || (a.name || '').localeCompare(b.name || '');
                case 'category':
                    return ((a.categoryName || '').localeCompare(b.categoryName || '')) * dir
                        || (b.setCount - a.setCount);
                case 'name':
                default:
                    return (a.name || '').localeCompare(b.name || '') * dir;
            }
        });
    }


    /**
     * Chunk filteredParts into rows of `columnsPerRow` items for the virtual-scroll viewport.
     */
    private buildCardRows(): void {
        const rows: CardRow[] = [];
        for (let i = 0; i < this.filteredParts.length; i += this.columnsPerRow) {
            rows.push({ items: this.filteredParts.slice(i, i + this.columnsPerRow) });
        }
        this.cardRows = rows;
    }


    // ----------------------------------------------------------------
    //  Responsive column calculation
    // ----------------------------------------------------------------

    @HostListener('window:resize')
    onResize(): void {
        this.calculateColumns();
    }

    private calculateColumns(): void {
        const width = window.innerWidth;
        const available = width - 40;  // just padding, sidebar is gone

        let cols: number;
        if (available >= 1400) {
            cols = 6;
        } else if (available >= 1100) {
            cols = 5;
        } else if (available >= 850) {
            cols = 4;
        } else if (available >= 600) {
            cols = 3;
        } else {
            cols = 2;
        }

        if (cols !== this.columnsPerRow) {
            this.columnsPerRow = cols;
            if (this.filteredParts.length > 0) {
                this.buildCardRows();
            }
        }
    }


    // ----------------------------------------------------------------
    //  Event Handlers
    // ----------------------------------------------------------------

    onSearch(event: Event): void {
        const term = (event.target as HTMLInputElement).value;
        this.searchSubject.next(term);
    }

    selectCategory(catId: number | null): void {
        this.selectedCategoryId = this.selectedCategoryId === catId ? null : catId;
        this.categoryDropdownOpen = false;
        this.applyPipeline();
    }

    selectPartType(typeId: number | null): void {
        this.selectedPartTypeId = this.selectedPartTypeId === typeId ? null : typeId;
        this.partTypeDropdownOpen = false;
        this.applyPipeline();
    }

    setSort(field: 'relevance' | 'name' | 'category'): void {
        if (this.sortBy === field) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortBy = field;
            this.sortDirection = field === 'relevance' ? 'desc' : 'asc';
        }
        this.applyPipeline();
    }

    clearFilters(): void {
        this.searchTerm = '';
        this.selectedCategoryId = null;
        this.selectedPartTypeId = null;
        this.sortBy = 'relevance';
        this.sortDirection = 'desc';
        this.applyPipeline();
    }

    hasActiveFilters(): boolean {
        return !!this.searchTerm || this.selectedCategoryId !== null || this.selectedPartTypeId !== null;
    }


    // ----------------------------------------------------------------
    //  Category sidebar helpers
    // ----------------------------------------------------------------

    /**
     * Returns categories sorted with user-preferred-theme categories boosted to top.
     * Categories are already sorted by partCount desc from the server.
     */
    get sortedCategories(): CatalogCategory[] {
        if (this.userPreferredThemeIds.length === 0) {
            return this.categories;
        }

        // NOTE: We don't have a direct category→theme mapping on the server yet,
        // so for now we boost categories whose name partially matches a known
        // LEGO theme category naming pattern.  A future improvement could add
        // themeIds to the category endpoint for precise matching.
        return this.categories;
    }


    // ----------------------------------------------------------------
    //  Navigation
    // ----------------------------------------------------------------

    navigateToDetail(part: CatalogPartItem): void {
        const hex = this.getHexForPart(part);
        this.router.navigate(['/parts', part.id], {
            queryParams: { hex: hex ? hex.replace('#', '') : null }
        });
    }

    navigateBack(): void {
        this.router.navigate(['/lego']);
    }

    toggleCategoryDropdown(event: Event): void {
        event.stopPropagation();
        this.categoryDropdownOpen = !this.categoryDropdownOpen;
        this.partTypeDropdownOpen = false;
    }

    togglePartTypeDropdown(event: Event): void {
        event.stopPropagation();
        this.partTypeDropdownOpen = !this.partTypeDropdownOpen;
        this.categoryDropdownOpen = false;
    }

    /** Close dropdowns when clicking outside */
    @HostListener('document:click', ['$event'])
    onDocumentClick(event: Event): void {
        if (!this.elementRef.nativeElement.contains(event.target)) {
            this.categoryDropdownOpen = false;
            this.partTypeDropdownOpen = false;
        }
    }

    /** Display name for the currently selected category */
    get selectedCategoryName(): string | null {
        if (this.selectedCategoryId === null) return null;
        const cat = this.categories.find(c => c.id === this.selectedCategoryId);
        return cat ? cat.name : null;
    }

    /** Display name for the currently selected part type */
    get selectedPartTypeName(): string | null {
        if (this.selectedPartTypeId === null) return null;
        const pt = this.partTypes.find(t => t.id === this.selectedPartTypeId);
        return pt ? pt.name : null;
    }


    // ----------------------------------------------------------------
    //  Helpers
    // ----------------------------------------------------------------

    getSortIcon(field: string): string {
        if (this.sortBy !== field) {
            return 'fas fa-sort';
        }
        return this.sortDirection === 'asc' ? 'fas fa-sort-up' : 'fas fa-sort-down';
    }

    getPartTypeIcon(part: CatalogPartItem): string {
        const typeName = part.partTypeName?.toLowerCase() || '';
        if (typeName.includes('plate')) return 'fas fa-grip-lines';
        if (typeName.includes('brick')) return 'fas fa-cube';
        if (typeName.includes('tile')) return 'fas fa-square';
        if (typeName.includes('slope')) return 'fas fa-mountain';
        if (typeName.includes('technic')) return 'fas fa-cog';
        if (typeName.includes('axle')) return 'fas fa-arrows-alt-h';
        if (typeName.includes('gear')) return 'fas fa-cog';
        return 'fas fa-puzzle-piece';
    }

    formatDimensions(part: CatalogPartItem): string {
        if (!part.widthLdu && !part.heightLdu && !part.depthLdu) return '—';
        return `${part.widthLdu || 0} × ${part.heightLdu || 0} × ${part.depthLdu || 0}`;
    }

    getCategoryCount(catId: number): number {
        const cat = this.categories.find(c => c.id === catId);
        return cat ? cat.partCount : 0;
    }

    /**
     * trackBy for *cdkVirtualFor on card rows — uses the first item's id in the row.
     */
    trackRow(index: number, row: CardRow): number {
        return row.items.length > 0 ? row.items[0].id : index;
    }

    /**
     * trackBy for *ngFor on individual cards within a row.
     */
    trackCard(index: number, item: CatalogPartItem): number {
        return item.id;
    }

    /**
     * Get the hex colour for a part — round-robin from its real top colours,
     * falling back to a small neutral palette if no colour data is available.
     */
    getHexForPart(part: CatalogPartItem): string {
        const colours = this.partColoursMap[part.id];
        if (colours && colours.length > 0) {
            return colours[part.id % colours.length];
        }
        return this.FALLBACK_PALETTE[part.id % this.FALLBACK_PALETTE.length];
    }

    /**
     * Compute the thumbnail cache key for a part, matching the service's
     * composite key format: 'path:colourHex' using the part's real colour.
     */
    thumbnailKey(part: CatalogPartItem): string {
        const colour = this.getHexForPart(part);
        return LDrawThumbnailService.cacheKey(part.geometryFilePath, colour);
    }


    // ----------------------------------------------------------------
    //  Isometric SVG Brick Rendering
    // ----------------------------------------------------------------

    /** Get effective LDU dimensions — uses real data, parsed title, or type-based defaults */
    private getPartDimensions(part: CatalogPartItem): { w: number; h: number; d: number } {
        // 1. Use actual LDU dimensions if available
        if (part.widthLdu > 0 || part.heightLdu > 0 || part.depthLdu > 0) {
            return {
                w: Math.max(part.widthLdu || 20, 10),
                h: Math.max(part.heightLdu || 24, 6),
                d: Math.max(part.depthLdu || 20, 10)
            };
        }

        const categoryName = part.categoryName?.toLowerCase() || part.ldrawCategory?.toLowerCase() || '';

        // 2. Try to parse stud dimensions from ldrawTitle (e.g. "Brick 2 x 4", "Plate 1 x 6 x 2")
        const title = part.ldrawTitle || part.name || '';
        const studMatch = title.match(/(\d+)\s*x\s*(\d+)(?:\s*x\s*(\d+))?/i);
        if (studMatch) {
            const s1 = parseInt(studMatch[1], 10);
            const s2 = parseInt(studMatch[2], 10);
            const s3 = studMatch[3] ? parseInt(studMatch[3], 10) : 0;

            // Convert stud counts to LDU (1 stud = 20 LDU)
            const studW = s1 * 20;
            const studD = s2 * 20;

            // Height depends on category
            let heightLdu = 24; // default brick height
            if (categoryName.includes('plate') || categoryName.includes('baseplate')) {
                heightLdu = 8;
            } else if (categoryName.includes('tile')) {
                heightLdu = 4;
            } else if (s3 > 0) {
                // Third dimension given (e.g. "Slope 2 x 3 x 2" → height = s3 * 24)
                heightLdu = s3 * 24;
            }

            return {
                w: Math.max(studW, 10),
                h: Math.max(heightLdu, 4),
                d: Math.max(studD, 10)
            };
        }

        // 3. Fall back to category-based default shapes
        if (categoryName.includes('plate')) return { w: 40, h: 8, d: 20 };
        if (categoryName.includes('tile')) return { w: 40, h: 4, d: 40 };
        if (categoryName.includes('brick')) return { w: 40, h: 24, d: 20 };
        if (categoryName.includes('slope')) return { w: 20, h: 24, d: 20 };
        if (categoryName.includes('technic')) return { w: 40, h: 24, d: 20 };
        if (categoryName.includes('axle')) return { w: 60, h: 8, d: 8 };
        if (categoryName.includes('gear')) return { w: 24, h: 8, d: 24 };
        if (categoryName.includes('cone')) return { w: 20, h: 30, d: 20 };
        if (categoryName.includes('cylinder')) return { w: 20, h: 24, d: 20 };
        if (categoryName.includes('wedge')) return { w: 30, h: 12, d: 20 };
        if (categoryName.includes('arch')) return { w: 30, h: 24, d: 20 };
        if (categoryName.includes('door')) return { w: 30, h: 36, d: 8 };
        if (categoryName.includes('window')) return { w: 30, h: 36, d: 8 };
        if (categoryName.includes('hinge')) return { w: 20, h: 12, d: 20 };
        if (categoryName.includes('bracket')) return { w: 20, h: 24, d: 10 };
        if (categoryName.includes('fence')) return { w: 40, h: 24, d: 4 };
        if (categoryName.includes('bar')) return { w: 50, h: 6, d: 6 };
        return { w: 20, h: 20, d: 20 };
    }

    /** Get colour shading for the SVG fallback brick — derived from the part's real hex */
    getPartColour(part: CatalogPartItem): { top: string; front: string; side: string; stud: string; outline: string } {
        const hex = this.getHexForPart(part);
        // Parse the hex into RGB components and generate shading variations
        const r = parseInt(hex.substring(0, 2), 16);
        const g = parseInt(hex.substring(2, 4), 16);
        const b = parseInt(hex.substring(4, 6), 16);

        const lighten = (v: number, pct: number) => Math.min(255, Math.round(v + (255 - v) * pct));
        const darken = (v: number, pct: number) => Math.max(0, Math.round(v * (1 - pct)));

        const toHex = (rv: number, gv: number, bv: number) =>
            '#' + [rv, gv, bv].map(c => c.toString(16).padStart(2, '0')).join('');

        return {
            top: toHex(lighten(r, 0.20), lighten(g, 0.20), lighten(b, 0.20)),
            front: toHex(r, g, b),
            side: toHex(darken(r, 0.25), darken(g, 0.25), darken(b, 0.25)),
            stud: toHex(lighten(r, 0.35), lighten(g, 0.35), lighten(b, 0.35)),
            outline: toHex(darken(r, 0.50), darken(g, 0.50), darken(b, 0.50)),
        };
    }

    /** Compute isometric polygon points for 3 faces of the brick */
    getIsometricPoints(part: CatalogPartItem): { top: string; front: string; side: string } {
        const dims = this.getPartDimensions(part);
        const rawW = dims.w;
        const rawH = dims.h;
        const rawD = dims.d;

        const maxDim = Math.max(rawW, rawH, rawD);
        const scale = 32 / maxDim;

        const w = rawW * scale;
        const h = rawH * scale;
        const d = rawD * scale;

        const cx = 55;
        const cy = 55;
        const ix = 0.866;
        const iy = 0.5;

        const bflX = cx - w * ix / 2 + d * ix / 2;
        const bflY = cy + h / 2;
        const bfrX = bflX + w * ix;
        const bfrY = bflY + w * iy;
        const bblX = bflX - d * ix;
        const bblY = bflY + d * iy;
        const bbrX = bblX + w * ix;
        const bbrY = bblY + w * iy;

        const tflX = bflX;
        const tflY = bflY - h;
        const tfrX = bfrX;
        const tfrY = bfrY - h;
        const tblX = bblX;
        const tblY = bblY - h;
        const tbrX = bbrX;
        const tbrY = bbrY - h;

        const top = `${tflX},${tflY} ${tfrX},${tfrY} ${tbrX},${tbrY} ${tblX},${tblY}`;
        const front = `${tflX},${tflY} ${tfrX},${tfrY} ${bfrX},${bfrY} ${bflX},${bflY}`;
        const side = `${tfrX},${tfrY} ${tbrX},${tbrY} ${bbrX},${bbrY} ${bfrX},${bfrY}`;

        return { top, front, side };
    }

    /** Whether studs should be shown (bricks and plates) */
    shouldShowStuds(part: CatalogPartItem): boolean {
        const typeName = part.categoryName?.toLowerCase() || part.ldrawCategory?.toLowerCase() || '';
        return typeName.includes('brick') || typeName.includes('plate') || typeName.includes('baseplate');
    }

    /** Get stud positions on the top face as ellipse centers */
    getStudPositions(part: CatalogPartItem): { cx: number; cy: number }[] {
        const dims = this.getPartDimensions(part);
        const rawW = dims.w;
        const rawH = dims.h;
        const rawD = dims.d;

        const maxDim = Math.max(rawW, rawH, rawD);
        const scale = 32 / maxDim;

        const w = rawW * scale;
        const h = rawH * scale;
        const d = rawD * scale;

        const cx = 55;
        const cy = 55;
        const ix = 0.866;
        const iy = 0.5;

        const tflX = cx - w * ix / 2 + d * ix / 2;
        const tflY = cy + h / 2 - h;

        const studsW = Math.max(1, Math.min(Math.round(rawW / 20), 6));
        const studsD = Math.max(1, Math.min(Math.round(rawD / 20), 6));

        const positions: { cx: number; cy: number }[] = [];

        for (let si = 0; si < studsW; si++) {
            for (let sj = 0; sj < studsD; sj++) {
                const fw = (si + 0.5) / studsW;
                const fd = (sj + 0.5) / studsD;
                const sx = tflX + fw * w * ix - fd * d * ix;
                const sy = tflY + fw * w * iy + fd * d * iy;
                positions.push({ cx: sx, cy: sy });
            }
        }

        return positions;
    }

    /** Stud ellipse radius scaled to part size */
    getStudRadius(part: CatalogPartItem): { rx: number; ry: number } {
        const dims = this.getPartDimensions(part);
        const maxDim = Math.max(dims.w, dims.h, dims.d);
        const scale = 32 / maxDim;
        const studR = 5 * scale;
        return { rx: studR * 0.866, ry: studR * 0.5 };
    }
}
