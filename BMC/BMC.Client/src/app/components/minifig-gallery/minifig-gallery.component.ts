import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, Subscription, forkJoin } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { MinifigGalleryApiService, MinifigGalleryItem } from '../../services/minifig-gallery-api.service';
import { AuthService } from '../../services/auth.service';


/**
 * An internal type representing a single "row" inside the virtual-scroll viewport.
 * Each row contains `columnsPerRow` minifig cards rendered horizontally.
 */
interface CardRow {
    items: MinifigGalleryItem[];
}


@Component({
    selector: 'app-minifig-gallery',
    templateUrl: './minifig-gallery.component.html',
    styleUrl: './minifig-gallery.component.scss'
})
export class MinifigGalleryComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();
    private loadSub = new Subscription();

    //
    // Full dataset loaded once from the API / IndexedDB cache
    //
    private allMinifigs: MinifigGalleryItem[] = [];

    //
    // Derived from allMinifigs after applying filters + sort
    //
    filteredMinifigs: MinifigGalleryItem[] = [];

    //
    // Virtual-scroll rows: each row is a horizontal slice of N cards
    //
    cardRows: CardRow[] = [];
    columnsPerRow = 6;
    rowHeight = 370;  // px — matches the card height in SCSS

    loading = true;
    totalCount = 0;

    //
    // Filters
    //
    searchTerm = '';
    selectedThemeId: number | null = null;
    sortBy: 'year' | 'name' | 'partCount' | 'figNumber' = 'year';
    sortDirection: 'asc' | 'desc' = 'desc';   // newest first by default

    //
    // Themes extracted from data + user preferences
    //
    themes: { id: number; name: string }[] = [];
    userPreferredThemes: { id: number, name: string }[] = [];


    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private http: HttpClient,
        private authService: AuthService,
        private minifigGalleryApi: MinifigGalleryApiService
    ) { }


    ngOnInit(): void {
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
        // Load full dataset (from IndexedDB cache or server)
        //
        this.loadSub = this.minifigGalleryApi.getGalleryMinifigs().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (minifigs) => {
                this.allMinifigs = minifigs;
                this.totalCount = minifigs.length;

                //
                // Extract unique themes from minifig → set theme data
                //
                this.extractThemes(minifigs);

                this.applyPipeline();
                this.loading = false;
            },
            error: () => {
                this.allMinifigs = [];
                this.loading = false;
            }
        });

        //
        // Load user theme preferences (auth-only, non-critical personalization)
        //
        if (this.authService.isLoggedIn) {
            this.loadUserThemes();
        }
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.loadSub.unsubscribe();
    }


    private extractThemes(minifigs: MinifigGalleryItem[]): void {
        const allThemeIds = new Set<number>();
        for (const mf of minifigs) {
            if (mf.themeIds) {
                for (const tid of mf.themeIds) {
                    allThemeIds.add(tid);
                }
            }
        }
        this._allThemeIds = allThemeIds;
        this.tryBuildThemesList();
    }

    private _allThemeIds = new Set<number>();
    private _allThemesMap = new Map<number, string>();
    private _preferredThemeIds = new Set<number>();

    private loadUserThemes(): void {
        const headers = this.authService.GetAuthenticationHeaders();
        forkJoin({
            profile: this.http.get<any>('/api/profile/mine', { headers }),
            themes: this.http.get<any[]>('/api/LegoThemes', { headers, params: { pageSize: '500' } })
        }).subscribe({
            next: ({ profile, themes }) => {
                this._allThemesMap = new Map<number, string>(themes.map(t => [t.id, t.name]));
                this._preferredThemeIds = new Set<number>(
                    (profile.preferredThemes || []).map((pt: any) => pt.legoThemeId)
                );
                this.tryBuildThemesList();
            },
            error: () => { /* Non-critical */ }
        });
    }

    /**
     * Called after both data streams arrive.
     * Whichever finishes last triggers the actual build.
     */
    private tryBuildThemesList(): void {
        if (this._allThemeIds.size === 0 || this._allThemesMap.size === 0) {
            return; // One of the two hasn't arrived yet
        }

        // Build the full theme dropdown from IDs present in minifig data
        this.themes = [...this._allThemeIds]
            .map(id => ({ id, name: this._allThemesMap.get(id) || '' }))
            .filter(t => t.name)
            .sort((a, b) => a.name.localeCompare(b.name));

        // Build user preferred themes
        this.userPreferredThemes = [...this._preferredThemeIds]
            .map(id => ({ id, name: this._allThemesMap.get(id) || '' }))
            .filter(t => t.name)
            .sort((a, b) => a.name.localeCompare(b.name));
    }


    // ----------------------------------------------------------------
    //  Filter + Sort Pipeline  (operates on full dataset in memory)
    // ----------------------------------------------------------------

    private applyPipeline(): void {
        let result = this.allMinifigs;

        //
        // Text search — name, figNumber
        //
        if (this.searchTerm) {
            const lower = this.searchTerm.toLowerCase();
            result = result.filter(mf =>
                (mf.name || '').toLowerCase().includes(lower) ||
                (mf.figNumber || '').toLowerCase().includes(lower)
            );
        }

        //
        // Theme filter
        //
        if (this.selectedThemeId !== null) {
            result = result.filter(mf => mf.themeIds?.includes(this.selectedThemeId!));
        }

        //
        // Sorting
        //
        result = this.applySorting(result);

        this.filteredMinifigs = result;
        this.buildCardRows();
    }


    private applySorting(minifigs: MinifigGalleryItem[]): MinifigGalleryItem[] {
        const dir = this.sortDirection === 'asc' ? 1 : -1;

        return [...minifigs].sort((a, b) => {
            switch (this.sortBy) {
                case 'year':
                    return (a.year - b.year) * dir || (a.name || '').localeCompare(b.name || '');
                case 'partCount':
                    return (a.partCount - b.partCount) * dir;
                case 'figNumber':
                    return (a.figNumber || '').localeCompare(b.figNumber || '') * dir;
                case 'name':
                default:
                    return (a.name || '').localeCompare(b.name || '') * dir;
            }
        });
    }


    /**
     * Chunk filteredMinifigs into rows of `columnsPerRow` items for the virtual-scroll viewport.
     */
    private buildCardRows(): void {
        const rows: CardRow[] = [];
        for (let i = 0; i < this.filteredMinifigs.length; i += this.columnsPerRow) {
            rows.push({ items: this.filteredMinifigs.slice(i, i + this.columnsPerRow) });
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
            if (this.filteredMinifigs.length > 0) {
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

    setSort(field: 'name' | 'year' | 'partCount' | 'figNumber'): void {
        if (this.sortBy === field) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortBy = field;
            this.sortDirection = field === 'year' ? 'desc' : 'asc';
        }
        this.applyPipeline();
    }

    selectTheme(themeId: number | null): void {
        this.selectedThemeId = this.selectedThemeId === themeId ? null : themeId;
        this.applyPipeline();
    }

    clearFilters(): void {
        this.searchTerm = '';
        this.selectedThemeId = null;
        this.sortBy = 'year';
        this.sortDirection = 'desc';
        this.applyPipeline();
    }

    hasActiveFilters(): boolean {
        return !!this.searchTerm || this.selectedThemeId !== null;
    }


    // ----------------------------------------------------------------
    //  Navigation
    // ----------------------------------------------------------------

    openMinifig(mf: MinifigGalleryItem): void {
        this.router.navigate(['/lego/minifigs', mf.id]);
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
    trackCard(index: number, item: MinifigGalleryItem): number {
        return item.id;
    }
}
