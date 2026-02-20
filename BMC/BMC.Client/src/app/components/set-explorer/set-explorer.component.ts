import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { LegoSetService, LegoSetData, LegoSetQueryParameters } from '../../bmc-data-services/lego-set.service';
import { LegoThemeService, LegoThemeData } from '../../bmc-data-services/lego-theme.service';

@Component({
    selector: 'app-set-explorer',
    templateUrl: './set-explorer.component.html',
    styleUrl: './set-explorer.component.scss'
})
export class SetExplorerComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    //
    // Data
    //
    sets: LegoSetData[] = [];
    themes: LegoThemeData[] = [];
    loading = true;

    //
    // Filters
    //
    searchTerm = '';
    selectedThemeId: number | bigint | null = null;
    yearMin: number | null = null;
    yearMax: number | null = null;
    sortBy: 'name' | 'year' | 'partCount' = 'name';
    sortDirection: 'asc' | 'desc' = 'asc';

    //
    // Year range for UI
    //
    availableYearMin = 1950;
    availableYearMax = 2026;

    //
    // Pagination
    //
    pageSize = 36;
    currentPage = 1;
    totalCount = 0;
    totalPages = 1;

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private setService: LegoSetService,
        private themeService: LegoThemeService
    ) { }

    ngOnInit(): void {
        //
        // Debounced search
        //
        this.searchSubject.pipe(
            debounceTime(300),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.currentPage = 1;
            this.fetchSets();
        });

        //
        // Load theme list for filter dropdown
        //
        this.themeService.GetLegoThemeList({ active: true, deleted: false }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (themes) => {
                this.themes = themes.sort((a, b) => (a.name || '').localeCompare(b.name || ''));
            },
            error: () => {
                this.themes = [];
            }
        });

        //
        // Read query params for pre-filtering (from Universe dashboard links)
        //
        this.route.queryParams.pipe(
            takeUntil(this.destroy$)
        ).subscribe(params => {
            if (params['theme']) {
                this.selectedThemeId = Number(params['theme']);
            }
            if (params['year']) {
                const y = Number(params['year']);
                this.yearMin = y;
                this.yearMax = y;
            }
            this.fetchSets();
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    fetchSets(): void {
        this.loading = true;

        const config: any = {
            active: true,
            deleted: false,
            includeRelations: true,
            pageSize: this.pageSize,
            pageNumber: this.currentPage
        };

        if (this.searchTerm) {
            config.anyStringContains = this.searchTerm;
        }

        if (this.selectedThemeId !== null) {
            config.legoThemeId = this.selectedThemeId;
        }

        if (this.yearMin !== null && this.yearMax !== null && this.yearMin === this.yearMax) {
            config.year = this.yearMin;
        }

        //
        // Fetch sets and count in parallel
        //
        forkJoin({
            sets: this.setService.GetLegoSetList(config),
            count: this.setService.GetLegoSetsRowCount(config)
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                let sorted = result.sets;

                //
                // Client-side year range filtering (API only supports exact year)
                //
                if (this.yearMin !== null && this.yearMax !== null && this.yearMin !== this.yearMax) {
                    sorted = sorted.filter(s => {
                        const y = Number(s.year);
                        return y >= this.yearMin! && y <= this.yearMax!;
                    });
                }

                //
                // Client-side sorting
                //
                sorted = this.applySorting(sorted);

                this.sets = sorted;
                this.totalCount = Number(result.count);
                this.totalPages = Math.max(1, Math.ceil(this.totalCount / this.pageSize));
                this.loading = false;
            },
            error: () => {
                this.sets = [];
                this.totalCount = 0;
                this.totalPages = 1;
                this.loading = false;
            }
        });
    }

    private applySorting(sets: LegoSetData[]): LegoSetData[] {
        const dir = this.sortDirection === 'asc' ? 1 : -1;

        return [...sets].sort((a, b) => {
            switch (this.sortBy) {
                case 'year':
                    return (Number(a.year) - Number(b.year)) * dir;
                case 'partCount':
                    return (Number(a.partCount) - Number(b.partCount)) * dir;
                case 'name':
                default:
                    return (a.name || '').localeCompare(b.name || '') * dir;
            }
        });
    }

    // ----------------------------------------------------------------
    //  Event Handlers
    // ----------------------------------------------------------------

    onSearch(event: Event): void {
        const term = (event.target as HTMLInputElement).value;
        this.searchSubject.next(term);
    }

    selectTheme(themeId: number | bigint | null): void {
        this.selectedThemeId = this.selectedThemeId === themeId ? null : themeId;
        this.currentPage = 1;
        this.fetchSets();
    }

    applyYearFilter(): void {
        this.currentPage = 1;
        this.fetchSets();
    }

    clearYearFilter(): void {
        this.yearMin = null;
        this.yearMax = null;
        this.currentPage = 1;
        this.fetchSets();
    }

    setSort(field: 'name' | 'year' | 'partCount'): void {
        if (this.sortBy === field) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortBy = field;
            this.sortDirection = 'asc';
        }
        this.sets = this.applySorting(this.sets);
    }

    clearFilters(): void {
        this.searchTerm = '';
        this.selectedThemeId = null;
        this.yearMin = null;
        this.yearMax = null;
        this.sortBy = 'name';
        this.sortDirection = 'asc';
        this.currentPage = 1;
        this.fetchSets();
    }

    hasActiveFilters(): boolean {
        return !!this.searchTerm || this.selectedThemeId !== null ||
            this.yearMin !== null || this.yearMax !== null;
    }

    // ----------------------------------------------------------------
    //  Navigation
    // ----------------------------------------------------------------

    navigateToSet(set: LegoSetData): void {
        this.router.navigate(['/lego/sets', set.id]);
    }

    navigateBack(): void {
        this.router.navigate(['/lego']);
    }

    // ----------------------------------------------------------------
    //  Pagination
    // ----------------------------------------------------------------

    nextPage(): void {
        if (this.currentPage < this.totalPages) {
            this.currentPage++;
            this.fetchSets();
        }
    }

    prevPage(): void {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.fetchSets();
        }
    }

    goToPage(page: number): void {
        this.currentPage = page;
        this.fetchSets();
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

    // ----------------------------------------------------------------
    //  Helpers
    // ----------------------------------------------------------------

    getThemeName(themeId: bigint | number | null): string {
        if (themeId === null) {
            return 'Unknown';
        }
        const theme = this.themes.find(t => Number(t.id) === Number(themeId));
        return theme ? theme.name : 'Unknown';
    }

    getSortIcon(field: string): string {
        if (this.sortBy !== field) {
            return 'fas fa-sort';
        }
        return this.sortDirection === 'asc' ? 'fas fa-sort-up' : 'fas fa-sort-down';
    }
}
