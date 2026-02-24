import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';
import * as d3 from 'd3';
import { LegoThemeService, LegoThemeData } from '../../bmc-data-services/lego-theme.service';
import { LegoSetService, LegoSetData } from '../../bmc-data-services/lego-set.service';
import { MinifigGalleryApiService, MinifigGalleryItem } from '../../services/minifig-gallery-api.service';
import { SetOwnershipCacheService } from '../../services/set-ownership-cache.service';

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

    // Minifigs
    minifigs: MinifigGalleryItem[] = [];
    minifigsLoading = true;

    // Stats
    totalSets = 0;
    totalSubThemes = 0;
    yearRange = '';
    totalParts = 0;
    heroImageUrl: string | null = null;

    // Collection — theme completion
    ownedInTheme = 0;
    themeCompletion = 0;

    // Search within sets table
    setSearchQuery = '';

    // Timeline chart ref
    @ViewChild('timelineChart', { static: false }) timelineChartRef!: ElementRef;

    get filteredSets(): LegoSetData[] {
        if (!this.setSearchQuery) return this.sets;
        const q = this.setSearchQuery.toLowerCase();
        return this.sets.filter(s =>
            (s.name || '').toLowerCase().includes(q) ||
            (s.setNumber || '').toLowerCase().includes(q)
        );
    }

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private location: Location,
        private themeService: LegoThemeService,
        private setService: LegoSetService,
        private minifigGalleryApi: MinifigGalleryApiService,
        private ownershipCache: SetOwnershipCacheService
    ) {
        this.ownershipCache.ensureLoaded();
        this.ownershipCache.ownedIds$.pipe(takeUntil(this.destroy$)).subscribe(ids => {
            this.recalcCompletion(ids);
        });
    }

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
                document.title = `${result.theme.name} — Theme Detail`;

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

                // Load minifigs for this theme
                this.loadMinifigs(id);
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
            pageSize: 5000,
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

                // Calculate total parts across all sets
                this.totalParts = sets.reduce((sum, s) => sum + (Number(s.partCount) || 0), 0);

                this.setsLoading = false;

                // Use first available image as hero banner
                const firstWithImage = sets.find(s => s.imageUrl);
                this.heroImageUrl = firstWithImage?.imageUrl ?? null;

                // Build the timeline chart after a tick so the DOM is ready
                setTimeout(() => this.buildTimeline(), 0);

                // Recalculate theme completion % with current ownership data
                this.recalcCompletion(this.ownershipCache.getOwnedIds());
            },
            error: () => {
                this.setsLoading = false;
            }
        });
    }

    private loadMinifigs(themeId: number): void {
        this.minifigsLoading = true;

        this.minifigGalleryApi.getGalleryMinifigs().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (all) => {
                // Filter minifigs whose themeIds include this theme
                this.minifigs = all.filter(m =>
                    m.themeIds && m.themeIds.includes(themeId)
                );
                this.minifigsLoading = false;
            },
            error: () => {
                this.minifigs = [];
                this.minifigsLoading = false;
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

    navigateToMinifig(id: number): void {
        this.router.navigate(['/lego/minifigs', id]);
    }

    navigateBack(): void {
        this.location.back();
    }

    navigateToThemeExplorer(): void {
        this.router.navigate(['/lego/themes']);
    }

    private recalcCompletion(ownedIds: Set<number>): void {
        if (this.sets.length === 0) return;
        this.ownedInTheme = this.sets.filter(s => ownedIds.has(Number(s.id))).length;
        this.themeCompletion = Math.round((this.ownedInTheme / this.sets.length) * 100);
    }


    // ── D3 Set Timeline Scatter Chart ───────────────────────────
    private buildTimeline(): void {
        if (!this.timelineChartRef || this.sets.length < 2) return;

        const el = this.timelineChartRef.nativeElement as HTMLElement;
        d3.select(el).selectAll('*').remove();

        const data = this.sets
            .map(s => ({
                id: Number(s.id),
                name: s.name,
                setNumber: s.setNumber,
                year: Number(s.year),
                parts: Number(s.partCount) || 0
            }))
            .filter(d => d.year > 0);

        if (data.length < 2) return;

        // Dimensions
        const margin = { top: 20, right: 30, bottom: 44, left: 56 };
        const width = Math.min(el.clientWidth || 800, 1000) - margin.left - margin.right;
        const height = 320 - margin.top - margin.bottom;

        const svgRoot = d3.select(el)
            .append('svg')
            .attr('width', width + margin.left + margin.right)
            .attr('height', height + margin.top + margin.bottom);

        // Glow filter for hovered dots
        const defs = svgRoot.append('defs');
        const glowFilter = defs.append('filter').attr('id', 'dot-glow');
        glowFilter.append('feGaussianBlur').attr('stdDeviation', '3').attr('result', 'blur');
        const feMerge = glowFilter.append('feMerge');
        feMerge.append('feMergeNode').attr('in', 'blur');
        feMerge.append('feMergeNode').attr('in', 'SourceGraphic');

        const svg = svgRoot
            .append('g')
            .attr('transform', `translate(${margin.left},${margin.top})`);

        // Scales
        const xExtent = d3.extent(data, d => d.year) as [number, number];
        const x = d3.scaleLinear()
            .domain([xExtent[0] - 0.5, xExtent[1] + 0.5])
            .range([0, width]);

        const maxParts = d3.max(data, d => d.parts) || 100;
        const y = d3.scaleLinear()
            .domain([0, maxParts * 1.1])
            .range([height, 0]);

        const r = d3.scaleSqrt()
            .domain([0, maxParts])
            .range([4, 18]);

        // Color scale — cool-to-warm gradient based on part count
        const color = d3.scaleSequential()
            .domain([0, maxParts])
            .interpolator(d3.interpolateRgbBasis([
                '#4facfe',   // light blue  (small sets)
                '#00f2fe',   // cyan
                '#43e97b',   // green
                '#f9d423',   // amber
                '#ff6b6b',   // coral-red   (large sets)
            ]));

        // Axes
        const yearTicks = xExtent[1] - xExtent[0];
        svg.append('g')
            .attr('class', 'x-axis')
            .attr('transform', `translate(0,${height})`)
            .call(d3.axisBottom(x)
                .ticks(Math.min(yearTicks, 12))
                .tickFormat(d => String(d))
            );

        svg.append('g')
            .attr('class', 'y-axis')
            .call(d3.axisLeft(y).ticks(6));

        // Axis labels
        svg.append('text')
            .attr('class', 'axis-label')
            .attr('x', width / 2)
            .attr('y', height + 38)
            .attr('text-anchor', 'middle')
            .text('Year');

        svg.append('text')
            .attr('class', 'axis-label')
            .attr('transform', 'rotate(-90)')
            .attr('x', -height / 2)
            .attr('y', -44)
            .attr('text-anchor', 'middle')
            .text('Part Count');

        // Tooltip div
        const tooltip = d3.select(el)
            .append('div')
            .attr('class', 'timeline-tooltip')
            .style('opacity', 0);

        // Sort so smaller dots render on top (visible when overlapping)
        const sorted = [...data].sort((a, b) => b.parts - a.parts);

        // Dots
        const router = this.router;
        svg.selectAll('.dot')
            .data(sorted)
            .enter()
            .append('circle')
            .attr('class', 'dot')
            .attr('cx', d => x(d.year))
            .attr('cy', d => y(d.parts))
            .attr('r', d => r(d.parts))
            .attr('fill', d => color(d.parts) as string)
            .attr('stroke', d => d3.color(color(d.parts) as string)!.darker(0.5).toString())
            .style('cursor', 'pointer')
            .on('mouseover', function (event: MouseEvent, d: any) {
                tooltip
                    .style('opacity', 1)
                    .html(`<strong>${d.name}</strong><br/>${d.setNumber} · ${d.year}<br/>${d.parts.toLocaleString()} pieces`)
                    .style('left', (event.offsetX + 12) + 'px')
                    .style('top', (event.offsetY - 28) + 'px');
                d3.select(this).classed('hovered', true);
            })
            .on('mouseout', function () {
                tooltip.style('opacity', 0);
                d3.select(this).classed('hovered', false);
            })
            .on('click', (_event: MouseEvent, d: any) => {
                router.navigate(['/lego/sets', d.id]);
            });
    }
}
