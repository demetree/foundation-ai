import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { BrickPartService, BrickPartData } from '../../bmc-data-services/brick-part.service';
import { BrickCategoryService, BrickCategoryData } from '../../bmc-data-services/brick-category.service';
import { PartTypeService, PartTypeData } from '../../bmc-data-services/part-type.service';

@Component({
    selector: 'app-parts-catalog',
    templateUrl: './parts-catalog.component.html',
    styleUrl: './parts-catalog.component.scss'
})
export class PartsCatalogComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    allParts: BrickPartData[] = [];
    filteredParts: BrickPartData[] = [];
    displayedParts: BrickPartData[] = [];
    categories: BrickCategoryData[] = [];
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
        private router: Router
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

        this.loadData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadData(): void {
        this.loading = true;

        // Load categories
        this.categoryService.GetBrickCategoryList({ active: true, deleted: false }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (cats) => this.categories = cats.sort((a, b) => (a.name || '').localeCompare(b.name || '')),
            error: () => this.categories = []
        });

        // Load part types
        this.partTypeService.GetPartTypeList({ active: true, deleted: false }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (types) => this.partTypes = types,
            error: () => this.partTypes = []
        });

        // Load parts
        this.partService.GetBrickPartList({ active: true, deleted: false, includeRelations: true }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (parts) => {
                this.allParts = parts;
                this.totalCount = parts.length;
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
        this.router.navigate(['/brickparts', part.id]);
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
        return this.allParts.filter(p => p.brickCategoryId == catId).length;
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
