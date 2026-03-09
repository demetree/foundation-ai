///
/// AI-Developed: Brickberg Dashboard — top-level financial terminal for LEGO investments.
///
/// The "Bloomberg Terminal" for LEGO — provides:
///   - Portfolio summary (total collection value from owned sets × cached valuations)
///   - Quick set lookup with instant typeahead search and rich results
///   - Market movers (top gainers/losers from cached BrickEconomy data)
///   - Integration health status
///   - Cache statistics
///   - Recent lookups history with localStorage persistence
///
import { Component, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, of } from 'rxjs';
import { catchError, takeUntil } from 'rxjs/operators';
import { trigger, transition, style, animate } from '@angular/animations';

import { BrickbergApiService } from '../../services/brickberg-api.service';
import { SetExplorerApiService, SetExplorerItem } from '../../services/set-explorer-api.service';


//
// Shape for a recent lookup entry stored in localStorage
//
interface RecentLookup {
    setNumber: string;
    name: string;
    imageUrl: string | null;
    year: number;
    themeName: string | null;
    timestamp: number;
}

//
// localStorage key for persisting recent lookups
//
const RECENT_LOOKUPS_KEY = 'brickberg_recent_lookups';
const MAX_RECENT_LOOKUPS = 5;
const MAX_SUGGESTIONS = 8;


@Component({
    selector: 'app-brickberg-dashboard',
    templateUrl: './brickberg-dashboard.component.html',
    styleUrl: './brickberg-dashboard.component.scss',
    animations: [
        trigger('fadeInUp', [
            transition(':enter', [
                style({ opacity: 0, transform: 'translateY(20px)' }),
                animate('500ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
            ])
        ]),
        trigger('fadeIn', [
            transition(':enter', [
                style({ opacity: 0 }),
                animate('300ms ease-out', style({ opacity: 1 }))
            ])
        ]),
        trigger('slideDown', [
            transition(':enter', [
                style({ opacity: 0, transform: 'translateY(-8px)' }),
                animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
            ])
        ])
    ]
})
export class BrickbergDashboardComponent implements OnInit, OnDestroy {

    @ViewChild('searchInput') searchInputRef!: ElementRef;

    private destroy$ = new Subject<void>();

    // Set explorer data for typeahead
    private allSets: SetExplorerItem[] = [];
    setsLoaded = false;

    // Typeahead state
    searchTerm = '';
    suggestions: SetExplorerItem[] = [];
    showSuggestions = false;
    selectedSuggestionIndex = -1;

    // Lookup state
    selectedSet: SetExplorerItem | null = null;
    lookupResult: any = null;
    lookupLoading = false;
    lookupError: string | null = null;

    // Recent lookups
    recentLookups: RecentLookup[] = [];

    // Panel data
    portfolio: any = null;
    marketMovers: any = null;
    integrationStatus: any = null;
    cacheStats: any = null;

    // Loading states
    portfolioLoading = true;
    moversLoading = true;
    statusLoading = true;
    cacheLoading = true;

    constructor(
        private http: HttpClient,
        private router: Router,
        private brickbergApi: BrickbergApiService,
        private explorerApi: SetExplorerApiService
    ) { }


    ngOnInit(): void {
        document.title = 'Brickberg Terminal';
        this.loadAllPanels();
        this.loadSetExplorerData();
        this.loadRecentLookups();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    // ═══════════════════════════════════════════════════════════════
    //  TYPEAHEAD SEARCH
    // ═══════════════════════════════════════════════════════════════

    //
    // Load the full set list from SetExplorerApiService for client-side filtering
    //
    private loadSetExplorerData(): void {
        this.explorerApi.getExploreSets().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (sets) => {
                this.allSets = sets;
                this.setsLoaded = true;
            },
            error: () => {
                this.setsLoaded = false;
            }
        });
    }


    //
    // Called on every keystroke — filter sets by name or number
    //
    onSearchInput(): void {
        const term = this.searchTerm.trim().toLowerCase();

        if (term.length < 2) {
            this.suggestions = [];
            this.showSuggestions = false;
            this.selectedSuggestionIndex = -1;
            return;
        }

        //
        // Filter sets matching by name or set number (case-insensitive)
        // Prioritize set number matches first, then name matches
        //
        const numberMatches: SetExplorerItem[] = [];
        const nameMatches: SetExplorerItem[] = [];

        for (const set of this.allSets) {
            if (set.setNumber && set.setNumber.toLowerCase().includes(term)) {
                numberMatches.push(set);
            } else if (set.name && set.name.toLowerCase().includes(term)) {
                nameMatches.push(set);
            }

            // Stop early if we have enough
            if (numberMatches.length + nameMatches.length >= MAX_SUGGESTIONS * 2) break;
        }

        this.suggestions = [...numberMatches, ...nameMatches].slice(0, MAX_SUGGESTIONS);
        this.showSuggestions = this.suggestions.length > 0;
        this.selectedSuggestionIndex = -1;
    }


    //
    // Keyboard navigation for the suggestions dropdown
    //
    onSearchKeydown(event: KeyboardEvent): void {
        if (event.key === 'ArrowDown') {
            event.preventDefault();
            if (this.showSuggestions && this.selectedSuggestionIndex < this.suggestions.length - 1) {
                this.selectedSuggestionIndex++;
            }
        } else if (event.key === 'ArrowUp') {
            event.preventDefault();
            if (this.selectedSuggestionIndex > 0) {
                this.selectedSuggestionIndex--;
            }
        } else if (event.key === 'Enter') {
            event.preventDefault();
            if (this.showSuggestions && this.selectedSuggestionIndex >= 0) {
                this.selectSuggestion(this.suggestions[this.selectedSuggestionIndex]);
            } else if (this.searchTerm.trim()) {
                // Fall back to direct set number lookup if no suggestion is selected
                this.performDirectLookup(this.searchTerm.trim());
            }
        } else if (event.key === 'Escape') {
            this.showSuggestions = false;
            this.selectedSuggestionIndex = -1;
        }
    }


    //
    // Select a set from the typeahead suggestions
    //
    selectSuggestion(set: SetExplorerItem): void {
        this.selectedSet = set;
        this.searchTerm = `${set.setNumber} — ${set.name}`;
        this.showSuggestions = false;
        this.selectedSuggestionIndex = -1;

        // Auto-trigger the market data lookup
        this.performLookup(set.setNumber);
    }


    //
    // Direct lookup by set number (when user types a number and presses Enter
    // without selecting from suggestions)
    //
    private performDirectLookup(term: string): void {
        // Try to find the set in our explorer data for identity info
        const match = this.allSets.find(s =>
            s.setNumber.toLowerCase() === term.toLowerCase() ||
            s.setNumber.toLowerCase().startsWith(term.toLowerCase())
        );

        if (match) {
            this.selectSuggestion(match);
        } else {
            // Fall back to API-only lookup without set identity
            this.selectedSet = null;
            this.performLookup(term);
        }
    }


    //
    // Fetch market data for a set number
    //
    private performLookup(setNumber: string): void {
        this.lookupLoading = true;
        this.lookupError = null;
        this.lookupResult = null;

        this.brickbergApi.getSetMarketData(setNumber).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (data) => {
                this.lookupResult = data;
                this.lookupLoading = false;

                // Add to recent lookups
                this.addToRecentLookups(setNumber);
            },
            error: () => {
                this.lookupError = `No market data available for "${setNumber}"`;
                this.lookupLoading = false;
            }
        });
    }


    //
    // Close the suggestions dropdown when clicking outside
    //
    onSearchBlur(): void {

        //
        // Delay to allow click events on suggestions to fire first
        //
        setTimeout(() => {
            this.showSuggestions = false;
        }, 200);
    }


    onSearchFocus(): void {
        if (this.suggestions.length > 0 && this.searchTerm.trim().length >= 2) {
            this.showSuggestions = true;
        }
    }


    clearLookup(): void {
        this.lookupResult = null;
        this.lookupError = null;
        this.lookupLoading = false;
        this.searchTerm = '';
        this.selectedSet = null;
        this.suggestions = [];
        this.showSuggestions = false;
    }


    // ═══════════════════════════════════════════════════════════════
    //  RECENT LOOKUPS
    // ═══════════════════════════════════════════════════════════════

    private loadRecentLookups(): void {
        try {
            const stored = localStorage.getItem(RECENT_LOOKUPS_KEY);
            if (stored) {
                this.recentLookups = JSON.parse(stored);
            }
        } catch {
            this.recentLookups = [];
        }
    }


    private addToRecentLookups(setNumber: string): void {
        const set = this.selectedSet;
        const entry: RecentLookup = {
            setNumber: setNumber,
            name: set?.name ?? setNumber,
            imageUrl: set?.imageUrl ?? null,
            year: set?.year ?? 0,
            themeName: set?.themeName ?? null,
            timestamp: Date.now()
        };

        // Remove duplicate if exists
        this.recentLookups = this.recentLookups.filter(r => r.setNumber !== setNumber);

        // Add to front
        this.recentLookups.unshift(entry);

        // Trim to max
        if (this.recentLookups.length > MAX_RECENT_LOOKUPS) {
            this.recentLookups = this.recentLookups.slice(0, MAX_RECENT_LOOKUPS);
        }

        // Persist
        try {
            localStorage.setItem(RECENT_LOOKUPS_KEY, JSON.stringify(this.recentLookups));
        } catch {
            // Storage full — silently ignore
        }
    }


    selectRecentLookup(recent: RecentLookup): void {
        // Try to find the full set in explorer data
        const match = this.allSets.find(s => s.setNumber === recent.setNumber);
        if (match) {
            this.selectSuggestion(match);
        } else {
            this.selectedSet = null;
            this.searchTerm = recent.setNumber;
            this.performLookup(recent.setNumber);
        }
    }


    clearRecentLookups(): void {
        this.recentLookups = [];
        try {
            localStorage.removeItem(RECENT_LOOKUPS_KEY);
        } catch {
            // Silently ignore
        }
    }


    // ═══════════════════════════════════════════════════════════════
    //  RESULT HELPERS
    // ═══════════════════════════════════════════════════════════════

    //
    // Navigate to the set detail page
    //
    navigateToSet(): void {
        if (this.selectedSet) {
            this.router.navigate(['/lego/sets', this.selectedSet.id]);
        }
    }


    //
    // Growth direction and colour helpers
    //
    getGrowthClass(value: number | null | undefined): string {
        if (value == null) return '';
        return value >= 0 ? 'positive' : 'negative';
    }


    getGrowthArrow(value: number | null | undefined): string {
        if (value == null) return '';
        return value >= 0 ? '▲' : '▼';
    }


    //
    // Check if any source returned data
    //
    hasAnyData(): boolean {
        if (!this.lookupResult) return false;
        return this.lookupResult.brickLink?.loaded ||
            this.lookupResult.brickEconomy?.loaded ||
            this.lookupResult.brickOwl?.loaded;
    }


    // ═══════════════════════════════════════════════════════════════
    //  DASHBOARD PANELS
    // ═══════════════════════════════════════════════════════════════

    //
    // Fire all dashboard data requests in parallel using the authenticated API service
    //
    private loadAllPanels(): void {

        this.brickbergApi.getPortfolio().pipe(
            takeUntil(this.destroy$),
            catchError(() => of(null))
        ).subscribe(data => {
            this.portfolio = data;
            this.portfolioLoading = false;
        });

        this.brickbergApi.getMarketMovers().pipe(
            takeUntil(this.destroy$),
            catchError(() => of(null))
        ).subscribe(data => {
            this.marketMovers = data;
            this.moversLoading = false;
        });

        this.brickbergApi.getIntegrationStatus().pipe(
            takeUntil(this.destroy$),
            catchError(() => of(null))
        ).subscribe(data => {
            this.integrationStatus = data;
            this.statusLoading = false;
        });

        this.brickbergApi.getCacheStats().pipe(
            takeUntil(this.destroy$),
            catchError(() => of(null))
        ).subscribe(data => {
            this.cacheStats = data;
            this.cacheLoading = false;
        });
    }


    // ── Navigation ──
    openSet(setNumber: string): void {
        // Look up setNumber → navigate to set detail (search by number)
        this.http.get<any[]>(`/api/LegoSets?setNumber=${setNumber}&limit=1`).pipe(
            takeUntil(this.destroy$),
            catchError(() => of([]))
        ).subscribe(sets => {
            if (sets?.length > 0) {
                this.router.navigate(['/lego/sets', sets[0].id]);
            }
        });
    }

    goToIntegrations(): void {
        this.router.navigate(['/integrations']);
    }
}
