///
/// AI-Generated: Brickberg Dashboard — top-level financial terminal for LEGO investments.
///
/// The "Bloomberg Terminal" for LEGO — provides:
///   - Portfolio summary (total collection value from owned sets × cached valuations)
///   - Quick set lookup with instant valuation
///   - Market movers (top gainers/losers from cached BrickEconomy data)
///   - Integration health status
///   - Cache statistics
///
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, forkJoin, of } from 'rxjs';
import { catchError, takeUntil } from 'rxjs/operators';
import { trigger, transition, style, animate } from '@angular/animations';


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
        ])
    ]
})
export class BrickbergDashboardComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

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

    // Quick Lookup
    lookupSetNumber = '';
    lookupResult: any = null;
    lookupLoading = false;
    lookupError: string | null = null;

    constructor(
        private http: HttpClient,
        private router: Router
    ) { }


    ngOnInit(): void {
        document.title = 'Brickberg Terminal';
        this.loadAllPanels();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    private loadAllPanels(): void {
        // Fire all data requests in parallel
        this.http.get('/api/brickberg/portfolio').pipe(
            takeUntil(this.destroy$),
            catchError(() => of(null))
        ).subscribe(data => {
            this.portfolio = data;
            this.portfolioLoading = false;
        });

        this.http.get('/api/brickberg/market-movers').pipe(
            takeUntil(this.destroy$),
            catchError(() => of(null))
        ).subscribe(data => {
            this.marketMovers = data;
            this.moversLoading = false;
        });

        this.http.get('/api/brickberg/status').pipe(
            takeUntil(this.destroy$),
            catchError(() => of(null))
        ).subscribe(data => {
            this.integrationStatus = data;
            this.statusLoading = false;
        });

        this.http.get('/api/brickberg/cache-stats').pipe(
            takeUntil(this.destroy$),
            catchError(() => of(null))
        ).subscribe(data => {
            this.cacheStats = data;
            this.cacheLoading = false;
        });
    }


    // ── Quick Lookup ──
    performLookup(): void {
        const num = this.lookupSetNumber.trim();
        if (!num) return;

        this.lookupLoading = true;
        this.lookupError = null;
        this.lookupResult = null;

        this.http.get(`/api/brickberg/set/${num}`).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (data) => {
                this.lookupResult = data;
                this.lookupLoading = false;
            },
            error: () => {
                this.lookupError = `No data found for "${num}"`;
                this.lookupLoading = false;
            }
        });
    }

    onLookupKeydown(event: KeyboardEvent): void {
        if (event.key === 'Enter') {
            this.performLookup();
        }
    }

    clearLookup(): void {
        this.lookupResult = null;
        this.lookupError = null;
        this.lookupSetNumber = '';
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
