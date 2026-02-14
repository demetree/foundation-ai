import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { BrickPartService, BrickPartData } from '../../bmc-data-services/brick-part.service';
import { BrickCategoryService, BrickCategoryData } from '../../bmc-data-services/brick-category.service';
import { PartTypeService, PartTypeData } from '../../bmc-data-services/part-type.service';
import { LDrawThumbnailService } from '../../services/ldraw-thumbnail.service';
import { IndexedDBCacheService } from '../../services/indexeddb-cache.service';

@Component({
    selector: 'app-parts-catalog',
    templateUrl: './parts-catalog.component.html',
    styleUrl: './parts-catalog.component.scss'
})
export class PartsCatalogComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    // 3D thumbnail cache: geometryFilePath → data:URL
    thumbnails = new Map<string, string>();

    allParts: BrickPartData[] = [];
    filteredParts: BrickPartData[] = [];
    displayedParts: BrickPartData[] = [];
    categories: BrickCategoryData[] = [];       // Filtered & sorted for sidebar display
    private allCategories: BrickCategoryData[] = [];  // Unfiltered from server
    categoryCounts = new Map<number | bigint, number>();
    partTypes: PartTypeData[] = [];

    loading = true;
    searchTerm = '';
    selectedCategoryId: number | bigint | null = null;
    selectedPartTypeId: number | bigint | null = null;
    viewMode: 'grid' | 'list' = 'grid';
    sidebarCollapsed = false;

    // Pagination
    pageSize = 48;
    currentPage = 1;
    totalPages = 1;

    // Stats
    totalCount = 0;

    constructor(
        private partService: BrickPartService,
        private categoryService: BrickCategoryService,
        private partTypeService: PartTypeService,
        private router: Router,
        private thumbnailService: LDrawThumbnailService,
        private cacheService: IndexedDBCacheService
    ) { }

    ngOnInit(): void {
        this.searchSubject.pipe(
            debounceTime(250),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.currentPage = 1;
            this.applyFilters();
        });

        //
        // Subscribe to 3D thumbnail render results — updates cards progressively
        //
        this.thumbnailService.thumbnail$.pipe(
            takeUntil(this.destroy$)
        ).subscribe(result => {
            this.thumbnails.set(result.geometryFilePath, result.dataUrl);
        });

        this.loadData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadData(): void {
        this.loading = true;

        const categoryParams = { active: true, deleted: false };
        const partTypeParams = { active: true, deleted: false };
        const partParams = { active: true, deleted: false, includeRelations: true };

        // Load categories (cached 7 days)
        this.cacheService.getOrFetch<BrickCategoryData[]>(
            'brick-categories',
            categoryParams,
            (p) => this.categoryService.GetBrickCategoryList(p),
            10080  // 7 days
        ).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (cats) => this.allCategories = cats,
            error: () => this.allCategories = []
        });

        // Load part types (cached 7 days)
        this.cacheService.getOrFetch<PartTypeData[]>(
            'part-types',
            partTypeParams,
            (p) => this.partTypeService.GetPartTypeList(p),
            10080  // 7 days
        ).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (types) => this.partTypes = types,
            error: () => this.partTypes = []
        });

        // Load parts (cached 24 hours)
        this.cacheService.getOrFetch<BrickPartData[]>(
            'brick-parts',
            partParams,
            (p) => this.partService.GetBrickPartList(p),
            1440  // 24 hours
        ).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (parts) => {
                this.allParts = parts;
                this.totalCount = parts.length;
                this.buildCategorySidebar();
                this.applyFilters();
                this.loading = false;
            },
            error: () => {
                this.allParts = [];
                this.filteredParts = [];
                this.displayedParts = [];
                this.loading = false;
            }
        });
    }

    onSearch(event: Event): void {
        const term = (event.target as HTMLInputElement).value;
        this.searchSubject.next(term);
    }

    applyFilters(): void {
        let result = [...this.allParts];

        // Text search
        if (this.searchTerm) {
            const lower = this.searchTerm.toLowerCase();
            result = result.filter(p =>
                p.name?.toLowerCase().includes(lower) ||
                p.ldrawPartId?.toLowerCase().includes(lower) ||
                p.ldrawTitle?.toLowerCase().includes(lower) ||
                p.keywords?.toLowerCase().includes(lower) ||
                p.ldrawCategory?.toLowerCase().includes(lower)
            );
        }

        // Category filter
        if (this.selectedCategoryId !== null) {
            result = result.filter(p => p.brickCategoryId == this.selectedCategoryId);
        }

        // Part type filter
        if (this.selectedPartTypeId !== null) {
            result = result.filter(p => p.partTypeId == this.selectedPartTypeId);
        }

        this.filteredParts = result;
        this.totalPages = Math.max(1, Math.ceil(result.length / this.pageSize));
        if (this.currentPage > this.totalPages) this.currentPage = 1;
        this.updateDisplayedParts();
    }

    updateDisplayedParts(): void {
        const start = (this.currentPage - 1) * this.pageSize;
        this.displayedParts = this.filteredParts.slice(start, start + this.pageSize);

        //
        // Kick off 3D thumbnail rendering for the new batch of visible parts
        //
        this.thumbnailService.renderBatch(this.displayedParts);
    }

    selectCategory(catId: number | bigint | null): void {
        this.selectedCategoryId = this.selectedCategoryId === catId ? null : catId;
        this.currentPage = 1;
        this.applyFilters();
    }

    selectPartType(typeId: number | bigint | null): void {
        this.selectedPartTypeId = this.selectedPartTypeId === typeId ? null : typeId;
        this.currentPage = 1;
        this.applyFilters();
    }

    navigateToDetail(part: BrickPartData): void {
        this.router.navigate(['/parts', part.id]);
    }

    nextPage(): void {
        if (this.currentPage < this.totalPages) {
            this.currentPage++;
            this.updateDisplayedParts();
        }
    }

    prevPage(): void {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.updateDisplayedParts();
        }
    }

    goToPage(page: number): void {
        this.currentPage = page;
        this.updateDisplayedParts();
    }

    getPartTypeIcon(part: BrickPartData): string {
        const typeName = part.partType?.name?.toLowerCase() || '';
        if (typeName.includes('plate')) return 'fas fa-grip-lines';
        if (typeName.includes('brick')) return 'fas fa-cube';
        if (typeName.includes('tile')) return 'fas fa-square';
        if (typeName.includes('slope')) return 'fas fa-mountain';
        if (typeName.includes('technic')) return 'fas fa-cog';
        if (typeName.includes('axle')) return 'fas fa-arrows-alt-h';
        if (typeName.includes('gear')) return 'fas fa-cog';
        return 'fas fa-puzzle-piece';
    }

    formatDimensions(part: BrickPartData): string {
        if (!part.widthLdu && !part.heightLdu && !part.depthLdu) return '—';
        return `${part.widthLdu || 0} × ${part.heightLdu || 0} × ${part.depthLdu || 0}`;
    }

    // ----------------------------------------------------------------
    //  Isometric SVG Brick Rendering
    // ----------------------------------------------------------------

    /** Get effective LDU dimensions — uses real data, parsed title, or type-based defaults */
    private getPartDimensions(part: BrickPartData): { w: number; h: number; d: number } {
        // 1. Use actual LDU dimensions if available
        if (part.widthLdu > 0 || part.heightLdu > 0 || part.depthLdu > 0) {
            return {
                w: Math.max(part.widthLdu || 20, 10),
                h: Math.max(part.heightLdu || 24, 6),
                d: Math.max(part.depthLdu || 20, 10)
            };
        }

        const categoryName = part.brickCategory?.name?.toLowerCase() || part.ldrawCategory?.toLowerCase() || '';

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

    /** Get colour palette based on brick category */
    getPartColour(part: BrickPartData): { top: string; front: string; side: string; stud: string; outline: string } {
        const typeName = part.brickCategory?.name?.toLowerCase() || part.ldrawCategory?.toLowerCase() || '';

        if (typeName.includes('brick')) return { top: '#ffb74d', front: '#f09030', side: '#c06a20', stud: '#ffd180', outline: '#a05010' };
        if (typeName.includes('plate')) return { top: '#64b5f6', front: '#4090d0', side: '#2a6ca0', stud: '#90caf9', outline: '#1a5080' };
        if (typeName.includes('tile')) return { top: '#81c784', front: '#56a05a', side: '#3a7a3e', stud: '#a5d6a7', outline: '#2a5a2e' };
        if (typeName.includes('slope')) return { top: '#e57373', front: '#c04040', side: '#8a2a2a', stud: '#ef9a9a', outline: '#6a1a1a' };
        if (typeName.includes('technic')) return { top: '#ba68c8', front: '#8a40a0', side: '#6a2a7a', stud: '#ce93d8', outline: '#4a1a5a' };
        if (typeName.includes('axle')) return { top: '#ba68c8', front: '#8a40a0', side: '#6a2a7a', stud: '#ce93d8', outline: '#4a1a5a' };
        if (typeName.includes('gear')) return { top: '#ffd54f', front: '#d0a020', side: '#a07a10', stud: '#ffe082', outline: '#806010' };
        if (typeName.includes('cone')) return { top: '#f48fb1', front: '#d06080', side: '#a04060', stud: '#f8bbd0', outline: '#802040' };
        if (typeName.includes('cylinder')) return { top: '#90caf9', front: '#5090c0', side: '#306a90', stud: '#bbdefb', outline: '#204a70' };
        if (typeName.includes('arch')) return { top: '#ffcc80', front: '#d09040', side: '#a07020', stud: '#ffe0b2', outline: '#805020' };
        if (typeName.includes('door')) return { top: '#a1887f', front: '#7a6058', side: '#5a4038', stud: '#bcaaa4', outline: '#3a2018' };
        if (typeName.includes('window')) return { top: '#80cbc4', front: '#509a94', side: '#307a74', stud: '#b2dfdb', outline: '#205a54' };
        if (typeName.includes('fence')) return { top: '#c5e1a5', front: '#90b070', side: '#608040', stud: '#dcedc8', outline: '#405020' };
        if (typeName.includes('bar')) return { top: '#b0bec5', front: '#808e95', side: '#5a6a72', stud: '#cfd8dc', outline: '#404a50' };
        if (typeName.includes('bracket')) return { top: '#ffab91', front: '#d07050', side: '#a05030', stud: '#ffccbc', outline: '#803020' };
        if (typeName.includes('wedge')) return { top: '#ef9a9a', front: '#c06060', side: '#904040', stud: '#ffcdd2', outline: '#602020' };
        if (typeName.includes('hinge')) return { top: '#b39ddb', front: '#806ab0', side: '#604a90', stud: '#d1c4e9', outline: '#402a70' };
        if (typeName.includes('container')) return { top: '#a5d6a7', front: '#70a074', side: '#4a7a4e', stud: '#c8e6c9', outline: '#2a5a2e' };
        if (typeName.includes('electric')) return { top: '#fff59d', front: '#c0b050', side: '#908030', stud: '#fff9c4', outline: '#605020' };
        return { top: '#b0bec5', front: '#808e95', side: '#5a6a72', stud: '#cfd8dc', outline: '#404a50' };
    }

    /** Compute isometric polygon points for 3 faces of the brick */
    getIsometricPoints(part: BrickPartData): { top: string; front: string; side: string } {
        const dims = this.getPartDimensions(part);
        const rawW = dims.w;
        const rawH = dims.h;
        const rawD = dims.d;

        // Scale to fit within the viewport with isometric projection
        const maxDim = Math.max(rawW, rawH, rawD);
        const scale = 32 / maxDim;

        const w = rawW * scale;  // width along x-axis
        const h = rawH * scale;  // height along y-axis
        const d = rawD * scale;  // depth along z-axis

        // Isometric projection vectors
        // x-axis goes right and slightly down: (cos30, sin30) = (0.866, 0.5)
        // z-axis goes left and slightly down:  (-cos30, sin30) = (-0.866, 0.5)
        // y-axis goes straight up: (0, -1)
        const cx = 55;  // center x of SVG
        const cy = 55;  // center y of SVG

        const ix = 0.866;  // cos(30°)
        const iy = 0.5;    // sin(30°)

        // Compute the 8 corners of the box
        // Bottom-front-left (origin point)
        const bflX = cx - w * ix / 2 + d * ix / 2;
        const bflY = cy + h / 2;

        // Bottom-front-right
        const bfrX = bflX + w * ix;
        const bfrY = bflY + w * iy;

        // Bottom-back-left
        const bblX = bflX - d * ix;
        const bblY = bflY + d * iy;

        // Bottom-back-right
        const bbrX = bblX + w * ix;
        const bbrY = bblY + w * iy;

        // Top versions (shift up by h)
        const tflX = bflX;
        const tflY = bflY - h;

        const tfrX = bfrX;
        const tfrY = bfrY - h;

        const tblX = bblX;
        const tblY = bblY - h;

        const tbrX = bbrX;
        const tbrY = bbrY - h;

        // Top face: tfl -> tfr -> tbr -> tbl
        const top = `${tflX},${tflY} ${tfrX},${tfrY} ${tbrX},${tbrY} ${tblX},${tblY}`;

        // Front face: tfl -> tfr -> bfr -> bfl
        const front = `${tflX},${tflY} ${tfrX},${tfrY} ${bfrX},${bfrY} ${bflX},${bflY}`;

        // Right side face: tfr -> tbr -> bbr -> bfr
        const side = `${tfrX},${tfrY} ${tbrX},${tbrY} ${bbrX},${bbrY} ${bfrX},${bfrY}`;

        return { top, front, side };
    }

    /** Whether studs should be shown (bricks and plates) */
    shouldShowStuds(part: BrickPartData): boolean {
        const typeName = part.brickCategory?.name?.toLowerCase() || part.ldrawCategory?.toLowerCase() || '';
        return typeName.includes('brick') || typeName.includes('plate') || typeName.includes('baseplate');
    }

    /** Get stud positions on the top face as ellipse centers */
    getStudPositions(part: BrickPartData): { cx: number; cy: number }[] {
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

        // Top face origin (tfl)
        const tflX = cx - w * ix / 2 + d * ix / 2;
        const tflY = cy + h / 2 - h;

        // How many studs fit? 1 stud = 20 LDU
        const studsW = Math.max(1, Math.min(Math.round(rawW / 20), 6));
        const studsD = Math.max(1, Math.min(Math.round(rawD / 20), 6));

        const positions: { cx: number; cy: number }[] = [];

        for (let si = 0; si < studsW; si++) {
            for (let sj = 0; sj < studsD; sj++) {
                // Fraction along the width and depth
                const fw = (si + 0.5) / studsW;
                const fd = (sj + 0.5) / studsD;

                // Position in isometric space on the top face
                const sx = tflX + fw * w * ix - fd * d * ix;
                const sy = tflY + fw * w * iy + fd * d * iy;

                positions.push({ cx: sx, cy: sy });
            }
        }

        return positions;
    }

    /** Stud ellipse radius scaled to part size */
    getStudRadius(part: BrickPartData): { rx: number; ry: number } {
        const dims = this.getPartDimensions(part);
        const maxDim = Math.max(dims.w, dims.h, dims.d);
        const scale = 32 / maxDim;

        // Stud is ~6 LDU radius in real space, scale down
        const studR = 5 * scale;
        return { rx: studR * 0.866, ry: studR * 0.5 };
    }

    clearFilters(): void {
        this.searchTerm = '';
        this.selectedCategoryId = null;
        this.selectedPartTypeId = null;
        this.currentPage = 1;
        this.applyFilters();
    }

    hasActiveFilters(): boolean {
        return !!this.searchTerm || this.selectedCategoryId !== null || this.selectedPartTypeId !== null;
    }

    getCategoryCount(catId: number | bigint): number {
        return this.categoryCounts.get(catId) ?? 0;
    }


    /**
     * Build the sidebar category list: compute counts, hide empties, sort by count.
     * Called after both categories and parts have loaded.
     */
    private buildCategorySidebar(): void {
        //
        // Build counts map from loaded parts
        //
        this.categoryCounts.clear();
        for (const part of this.allParts) {
            if (part.brickCategoryId != null) {
                const current = this.categoryCounts.get(part.brickCategoryId) ?? 0;
                this.categoryCounts.set(part.brickCategoryId, current + 1);
            }
        }

        //
        // Filter out empty categories, then sort by count descending
        //
        this.categories = this.allCategories
            .filter(cat => (this.categoryCounts.get(cat.id) ?? 0) > 0)
            .sort((a, b) => {
                const countDiff = (this.categoryCounts.get(b.id) ?? 0) - (this.categoryCounts.get(a.id) ?? 0);
                if (countDiff !== 0) return countDiff;
                return (a.name || '').localeCompare(b.name || '');  // tie-break alphabetically
            });
    }

    getVisiblePages(): number[] {
        const pages: number[] = [];
        const start = Math.max(1, this.currentPage - 2);
        const end = Math.min(this.totalPages, start + 4);
        for (let i = start; i <= end; i++) {
            pages.push(i);
        }
        return pages;
    }

    toggleSidebar(): void {
        this.sidebarCollapsed = !this.sidebarCollapsed;
    }
}
