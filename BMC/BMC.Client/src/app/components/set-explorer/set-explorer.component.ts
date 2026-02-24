import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, Subscription, forkJoin } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { SetExplorerApiService, SetExplorerItem } from '../../services/set-explorer-api.service';
import { AuthService } from '../../services/auth.service';
import { SetOwnershipCacheService } from '../../services/set-ownership-cache.service';


/**
 * An internal type representing a single "row" inside the virtual-scroll viewport.
 * Each row contains `columnsPerRow` set cards rendered horizontally.
 */
interface CardRow {
    items: SetExplorerItem[];
}


@Component({
    selector: 'app-set-explorer',
    templateUrl: './set-explorer.component.html',
    styleUrl: './set-explorer.component.scss'
})
export class SetExplorerComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();
    private loadSub = new Subscription();

    //
    // Full dataset loaded once from the API / IndexedDB cache
    //
    private allSets: SetExplorerItem[] = [];

    //
    // Derived from allSets after applying filters + sort
    //
    filteredSets: SetExplorerItem[] = [];

    //
    // Virtual-scroll rows: each row is a horizontal slice of N cards
    //
    cardRows: CardRow[] = [];
    columnsPerRow = 6;
    rowHeight = 370;  // px — matches the card height in SCSS

    //
    // Extracted locally from the loaded data
    //
    themes: { id: number; name: string }[] = [];
    userPreferredThemes: { id: number, name: string }[] = [];

    loading = true;
    totalCount = 0;

    //
    // Filters
    //
    searchTerm = '';
    selectedThemeId: number | null = null;
    yearMin: number | null = null;
    yearMax: number | null = null;
    partCountMin: number | null = null;
    partCountMax: number | null = null;
    sortBy: 'year' | 'name' | 'partCount' = 'year';
    sortDirection: 'asc' | 'desc' = 'desc';   // newest first by default
    viewMode: 'grid' | 'table' = 'grid';

    //
    // Year range for UI
    //
    availableYearMin = 1950;
    availableYearMax = 2026;

    // Ownership badge sets
    ownedIds = new Set<number>();
    wantedIds = new Set<number>();


    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private http: HttpClient,
        private authService: AuthService,
        private setExplorerApi: SetExplorerApiService,
        public ownershipCache: SetOwnershipCacheService
    ) {
        this.ownershipCache.ensureLoaded();
        this.ownershipCache.ownedIds$.pipe(takeUntil(this.destroy$))
            .subscribe(ids => this.ownedIds = ids);
        this.ownershipCache.wantedIds$.pipe(takeUntil(this.destroy$))
            .subscribe(ids => this.wantedIds = ids);
    }


    ngOnInit(): void {
        // Restore view mode preference
        const savedView = localStorage.getItem('set-explorer-view');
        if (savedView === 'grid' || savedView === 'table') this.viewMode = savedView;

        //
        // Recalculate columns on width change
        //
        this.calculateColumns();

        //
        // Debounced search
        //
        this.searchSubject.pipe(
            debounceTime(300),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.applyPipeline();
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
            if (params['yearMin']) {
                this.yearMin = Number(params['yearMin']);
            }
            if (params['yearMax']) {
                this.yearMax = Number(params['yearMax']);
            }
        });

        //
        // Load full dataset (from IndexedDB cache or server)
        //
        this.loadSub = this.setExplorerApi.getExploreSets().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (sets) => {
                this.allSets = sets;
                this.totalCount = sets.length;

                //
                // Extract unique themes from the data
                //
                const themeMap = new Map<number, string>();
                for (const s of sets) {
                    if (s.themeId != null && s.themeName) {
                        themeMap.set(s.themeId, s.themeName);
                    }
                }
                this.themes = Array.from(themeMap, ([id, name]) => ({ id, name }))
                    .sort((a, b) => a.name.localeCompare(b.name));

                this.applyPipeline();
                this.loading = false;
            },
            error: () => {
                this.allSets = [];
                this.loading = false;
            }
        });

        this.loadUserThemes();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.loadSub.unsubscribe();
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
        if (event.key === 's' || event.key === 'S' || event.key === '/') {
            event.preventDefault();
            document.getElementById('set-search-input')?.focus();
        }
    }

    private loadUserThemes(): void {
        const headers = this.authService.GetAuthenticationHeaders();
        forkJoin({
            profile: this.http.get<any>('/api/profile/mine', { headers }),
            themes: this.http.get<any[]>('/api/LegoThemes', { headers, params: { pageSize: '500' } })
        }).subscribe({
            next: ({ profile, themes }) => {
                const preferredIds = new Set<number>(
                    (profile.preferredThemes || []).map((pt: any) => pt.legoThemeId)
                );
                const themeMap = new Map<number, string>(themes.map(t => [t.id, t.name]));
                this.userPreferredThemes = [...preferredIds]
                    .map(id => ({ id, name: themeMap.get(id) || `Theme ${id}` }))
                    .filter(t => t.name)
                    .sort((a, b) => a.name.localeCompare(b.name));
            },
            error: () => { /* Non-critical */ }
        });
    }


    // ----------------------------------------------------------------
    //  Filter + Sort Pipeline  (operates on full dataset in memory)
    // ----------------------------------------------------------------

    private applyPipeline(): void {
        let result = this.allSets;

        //
        // Text search — name, setNumber, themeName
        //
        if (this.searchTerm) {
            const lower = this.searchTerm.toLowerCase();
            result = result.filter(s =>
                (s.name || '').toLowerCase().includes(lower) ||
                (s.setNumber || '').toLowerCase().includes(lower) ||
                (s.themeName || '').toLowerCase().includes(lower)
            );
        }

        //
        // Theme filter
        //
        if (this.selectedThemeId !== null) {
            result = result.filter(s => s.themeId === this.selectedThemeId);
        }

        //
        // Year range
        //
        if (this.yearMin !== null) {
            result = result.filter(s => s.year >= this.yearMin!);
        }
        if (this.yearMax !== null) {
            result = result.filter(s => s.year <= this.yearMax!);
        }

        //
        // Part count range
        //
        if (this.partCountMin !== null) {
            result = result.filter(s => s.partCount >= this.partCountMin!);
        }
        if (this.partCountMax !== null) {
            result = result.filter(s => s.partCount <= this.partCountMax!);
        }

        //
        // Sorting
        //
        result = this.applySorting(result);

        this.filteredSets = result;
        this.buildCardRows();
    }


    private applySorting(sets: SetExplorerItem[]): SetExplorerItem[] {
        const dir = this.sortDirection === 'asc' ? 1 : -1;

        return [...sets].sort((a, b) => {
            switch (this.sortBy) {
                case 'year':
                    return (a.year - b.year) * dir || (a.partCount - b.partCount) * -1;
                case 'partCount':
                    return (a.partCount - b.partCount) * dir;
                case 'name':
                default:
                    return (a.name || '').localeCompare(b.name || '') * dir;
            }
        });
    }


    /**
     * Chunk filteredSets into rows of `columnsPerRow` items for the virtual-scroll viewport.
     */
    private buildCardRows(): void {
        const rows: CardRow[] = [];
        for (let i = 0; i < this.filteredSets.length; i += this.columnsPerRow) {
            rows.push({ items: this.filteredSets.slice(i, i + this.columnsPerRow) });
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
        let cols: number;
        if (width >= 1600) {
            cols = 6;
        } else if (width >= 1200) {
            cols = 5;
        } else if (width >= 992) {
            cols = 4;
        } else if (width >= 768) {
            cols = 3;
        } else {
            cols = 2;
        }

        if (cols !== this.columnsPerRow) {
            this.columnsPerRow = cols;
            if (this.filteredSets.length > 0) {
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

    selectTheme(themeId: number | null): void {
        this.selectedThemeId = this.selectedThemeId === themeId ? null : themeId;
        this.applyPipeline();
    }

    applyYearFilter(): void {
        this.applyPipeline();
    }

    clearYearFilter(): void {
        this.yearMin = null;
        this.yearMax = null;
        this.applyPipeline();
    }

    applyPartCountFilter(): void {
        this.applyPipeline();
    }

    clearPartCountFilter(): void {
        this.partCountMin = null;
        this.partCountMax = null;
        this.applyPipeline();
    }

    setSort(field: 'name' | 'year' | 'partCount'): void {
        if (this.sortBy === field) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortBy = field;
            this.sortDirection = field === 'year' ? 'desc' : 'asc';
        }
        this.applyPipeline();
    }

    setViewMode(mode: 'grid' | 'table'): void {
        this.viewMode = mode;
        localStorage.setItem('set-explorer-view', mode);
    }

    trackSet(index: number, set: SetExplorerItem): string {
        return set.setNumber;
    }

    clearFilters(): void {
        this.searchTerm = '';
        this.selectedThemeId = null;
        this.yearMin = null;
        this.yearMax = null;
        this.partCountMin = null;
        this.partCountMax = null;
        this.sortBy = 'year';
        this.sortDirection = 'desc';
        this.applyPipeline();
    }

    hasActiveFilters(): boolean {
        return !!this.searchTerm || this.selectedThemeId !== null ||
            this.yearMin !== null || this.yearMax !== null ||
            this.partCountMin !== null || this.partCountMax !== null;
    }


    // ----------------------------------------------------------------
    //  Navigation
    // ----------------------------------------------------------------

    navigateToSet(set: SetExplorerItem): void {
        this.router.navigate(['/lego/sets', set.id]);
    }

    navigateBack(): void {
        this.router.navigate(['/lego']);
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

    /**
     * trackBy for *cdkVirtualFor on card rows — uses the first item's id in the row.
     */
    trackRow(index: number, row: CardRow): number {
        return row.items.length > 0 ? row.items[0].id : index;
    }

    /**
     * trackBy for *ngFor on individual cards within a row.
     */
    trackCard(index: number, item: SetExplorerItem): number {
        return item.id;
    }
}
