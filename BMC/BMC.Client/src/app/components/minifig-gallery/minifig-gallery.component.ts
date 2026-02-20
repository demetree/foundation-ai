import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, Subscription, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';

import { LegoMinifigService, LegoMinifigData } from '../../bmc-data-services/lego-minifig.service';

@Component({
    selector: 'app-minifig-gallery',
    templateUrl: './minifig-gallery.component.html',
    styleUrl: './minifig-gallery.component.scss'
})
export class MinifigGalleryComponent implements OnInit, OnDestroy {
    minifigs: LegoMinifigData[] = [];
    loading = true;
    totalCount = 0;
    page = 1;
    pageSize = 48;

    search = '';
    private search$ = new Subject<string>();
    private destroy$ = new Subject<void>();
    private loadSub = new Subscription();

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private minifigService: LegoMinifigService,
    ) { }

    ngOnInit(): void {
        // Read query params
        this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
            if (params['search']) this.search = params['search'];
            if (params['page']) this.page = +params['page'];
        });

        // Debounced search
        this.search$.pipe(
            debounceTime(350),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(() => {
            this.page = 1;
            this.loadMinifigs();
        });

        this.loadMinifigs();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.loadSub.unsubscribe();
    }

    private loadMinifigs(): void {
        // Cancel any in-flight requests and reset state
        this.loadSub.unsubscribe();
        this.loadSub = new Subscription();
        this.loading = true;
        this.minifigs = [];

        const params: any = {
            active: true,
            deleted: false,
            pageSize: this.pageSize,
            pageNumber: this.page,
        };
        if (this.search.trim()) {
            params.anyStringContains = this.search.trim();
        }

        // Clone params for count (without pagination) to avoid any cross-contamination
        const countParams: any = { ...params };
        delete countParams.pageSize;
        delete countParams.pageNumber;

        this.loadSub.add(
            this.minifigService.GetLegoMinifigList(params).pipe(takeUntil(this.destroy$)).subscribe({
                next: (list) => {
                    this.minifigs = list;
                    this.loading = false;
                },
                error: () => { this.loading = false; }
            })
        );

        this.loadSub.add(
            this.minifigService.GetLegoMinifigsRowCount(countParams).pipe(takeUntil(this.destroy$)).subscribe({
                next: (count) => { this.totalCount = Number(count); }
            })
        );
    }

    onSearchInput(): void {
        this.search$.next(this.search);
    }

    clearSearch(): void {
        this.search = '';
        this.search$.next('');
    }

    get totalPages(): number {
        return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
    }

    goToPage(p: number): void {
        if (p < 1 || p > this.totalPages) return;
        this.page = p;
        this.loadMinifigs();
    }

    openMinifig(mf: LegoMinifigData): void {
        this.router.navigate(['/lego/minifigs', mf.id]);
    }

    get pageNumbers(): number[] {
        const pages: number[] = [];
        const start = Math.max(1, this.page - 2);
        const end = Math.min(this.totalPages, this.page + 2);
        for (let i = start; i <= end; i++) pages.push(i);
        return pages;
    }
}
