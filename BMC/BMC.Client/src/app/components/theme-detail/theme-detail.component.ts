import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';
import { LegoThemeService, LegoThemeData } from '../../bmc-data-services/lego-theme.service';
import { LegoSetService, LegoSetData } from '../../bmc-data-services/lego-set.service';

interface BreadcrumbItem {
    id: bigint | number;
    name: string;
}

@Component({
    selector: 'app-theme-detail',
    templateUrl: './theme-detail.component.html',
    styleUrl: './theme-detail.component.scss'
})
export class ThemeDetailComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // Data
    theme: LegoThemeData | null = null;
    allThemes: LegoThemeData[] = [];
    subThemes: LegoThemeData[] = [];
    sets: LegoSetData[] = [];
    breadcrumbs: BreadcrumbItem[] = [];
    loading = true;
    setsLoading = true;

    // Stats
    totalSets = 0;
    totalSubThemes = 0;
    yearRange = '';

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private location: Location,
        private themeService: LegoThemeService,
        private setService: LegoSetService
    ) { }

    ngOnInit(): void {
        this.route.params.pipe(
            takeUntil(this.destroy$)
        ).subscribe(params => {
            const id = Number(params['id']);
            this.loadTheme(id);
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadTheme(id: number): void {
        this.loading = true;
        this.setsLoading = true;
        this.sets = [];
        this.subThemes = [];

        forkJoin({
            theme: this.themeService.GetLegoTheme(id, true),
            allThemes: this.themeService.GetLegoThemeList({ active: true, deleted: false })
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                this.theme = result.theme;
                this.allThemes = result.allThemes;

                // Find sub-themes
                this.subThemes = this.allThemes.filter(t =>
                    Number(t.legoThemeId) === Number(this.theme!.id)
                );
                this.totalSubThemes = this.subThemes.length;

                // Build breadcrumb trail
                this.buildBreadcrumbs();

                this.loading = false;

                // Load sets for this theme
                this.loadSets(id);
            },
            error: () => {
                this.loading = false;
                this.setsLoading = false;
            }
        });
    }

    private loadSets(themeId: number): void {
        this.setsLoading = true;

        this.setService.GetLegoSetList({
            legoThemeId: themeId,
            active: true,
            deleted: false,
            includeRelations: true,
            pageSize: 200,
            pageNumber: 1
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (sets) => {
                this.sets = sets;
                this.totalSets = sets.length;

                // Calculate year range
                const years = sets.map(s => Number(s.year)).filter(y => y > 0);
                if (years.length > 0) {
                    const minYear = Math.min(...years);
                    const maxYear = Math.max(...years);
                    this.yearRange = minYear === maxYear ? `${minYear}` : `${minYear}–${maxYear}`;
                }

                this.setsLoading = false;
            },
            error: () => {
                this.setsLoading = false;
            }
        });
    }

    private buildBreadcrumbs(): void {
        this.breadcrumbs = [];

        if (!this.theme) return;

        // Walk up the parent chain
        let current: LegoThemeData | null | undefined = this.theme;
        const trail: BreadcrumbItem[] = [];

        while (current) {
            trail.unshift({ id: current.id, name: current.name });

            // Find parent
            const parentId = Number(current.legoThemeId);
            if (parentId > 0) {
                current = this.allThemes.find(t => Number(t.id) === parentId) || null;
            } else {
                current = null;
            }
        }

        this.breadcrumbs = trail;
    }

    navigateToTheme(id: bigint | number): void {
        this.router.navigate(['/lego/themes', Number(id)]);
    }

    navigateToSet(id: bigint | number): void {
        this.router.navigate(['/lego/sets', Number(id)]);
    }

    navigateBack(): void {
        this.location.back();
    }

    navigateToThemeExplorer(): void {
        this.router.navigate(['/lego/themes']);
    }
}
