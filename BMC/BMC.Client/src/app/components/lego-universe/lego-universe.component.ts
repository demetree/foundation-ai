import { Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { LegoSetService, LegoSetData } from '../../bmc-data-services/lego-set.service';
import { LegoMinifigService } from '../../bmc-data-services/lego-minifig.service';
import { LegoThemeService, LegoThemeData } from '../../bmc-data-services/lego-theme.service';
import * as d3 from 'd3';

interface ThemeNode {
    name: string;
    id: bigint | number;
    children?: ThemeNode[];
    value?: number;
}

interface YearBucket {
    year: number;
    count: number;
}

@Component({
    selector: 'app-lego-universe',
    templateUrl: './lego-universe.component.html',
    styleUrl: './lego-universe.component.scss'
})
export class LegoUniverseComponent implements OnInit, OnDestroy, AfterViewInit {

    @ViewChild('sunburstContainer', { static: false }) sunburstContainer!: ElementRef;
    @ViewChild('timelineContainer', { static: false }) timelineContainer!: ElementRef;

    private destroy$ = new Subject<void>();

    //
    // Stats — animated counters
    //
    totalSets = 0;
    totalMinifigs = 0;
    totalThemes = 0;
    displaySets = 0;
    displayMinifigs = 0;
    displayThemes = 0;

    //
    // Data
    //
    themes: LegoThemeData[] = [];
    sets: LegoSetData[] = [];
    loading = true;
    sunburstReady = false;
    timelineReady = false;

    //
    // Recent sets for the carousel
    //
    recentSets: LegoSetData[] = [];

    constructor(
        private router: Router,
        private setService: LegoSetService,
        private minifigService: LegoMinifigService,
        private themeService: LegoThemeService
    ) { }

    ngOnInit(): void {
        this.loadData();
    }

    ngAfterViewInit(): void {
        //
        // D3 rendering will be triggered after data arrives
        //
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadData(): void {
        this.loading = true;

        //
        // Fetch stats in parallel
        //
        forkJoin({
            setCount: this.setService.GetLegoSetsRowCount({ active: true, deleted: false }),
            minifigCount: this.minifigService.GetLegoMinifigsRowCount({ active: true, deleted: false }),
            themes: this.themeService.GetLegoThemeList({ active: true, deleted: false }),
            recentSets: this.setService.GetLegoSetList({
                active: true,
                deleted: false,
                includeRelations: true,
                pageSize: 12,
                pageNumber: 1
            }),
            allSets: this.setService.GetLegoSetList({
                active: true,
                deleted: false,
                pageSize: 5000,
                pageNumber: 1
            })
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                this.totalSets = Number(result.setCount);
                this.totalMinifigs = Number(result.minifigCount);
                this.themes = result.themes;
                this.totalThemes = result.themes.length;
                this.recentSets = result.recentSets;
                this.sets = result.allSets;
                this.loading = false;

                //
                // Animate counters
                //
                this.animateCounter('displaySets', this.totalSets, 1500);
                this.animateCounter('displayMinifigs', this.totalMinifigs, 1800);
                this.animateCounter('displayThemes', this.totalThemes, 1200);

                //
                // Render D3 visualizations after a tick (so ViewChild elements exist)
                //
                setTimeout(() => {
                    this.renderSunburst();
                    this.renderTimeline();
                }, 100);
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    //
    // Animated number counter
    //
    private animateCounter(prop: 'displaySets' | 'displayMinifigs' | 'displayThemes', target: number, duration: number): void {
        const start = 0;
        const startTime = performance.now();

        const step = (timestamp: number) => {
            const elapsed = timestamp - startTime;
            const progress = Math.min(elapsed / duration, 1);

            //
            // Ease-out cubic for smooth deceleration
            //
            const eased = 1 - Math.pow(1 - progress, 3);
            this[prop] = Math.round(start + (target - start) * eased);

            if (progress < 1) {
                requestAnimationFrame(step);
            }
        };

        requestAnimationFrame(step);
    }

    // ----------------------------------------------------------------
    //  D3 Sunburst — Theme Hierarchy
    // ----------------------------------------------------------------

    private renderSunburst(): void {
        if (!this.sunburstContainer || this.themes.length === 0) {
            return;
        }

        const container = this.sunburstContainer.nativeElement as HTMLElement;
        const width = container.clientWidth || 500;
        const height = Math.min(width, 500);
        const radius = Math.min(width, height) / 2;

        //
        // Build hierarchy: top-level themes (legoThemeId === 0 or null) → children
        //
        const themeMap = new Map<string, LegoThemeData[]>();
        const topLevel: LegoThemeData[] = [];

        for (const theme of this.themes) {
            const parentId = Number(theme.legoThemeId);
            if (parentId === 0 || !theme.legoThemeId) {
                topLevel.push(theme);
            } else {
                const key = parentId.toString();
                if (!themeMap.has(key)) {
                    themeMap.set(key, []);
                }
                themeMap.get(key)!.push(theme);
            }
        }

        //
        // Count sets per theme for sizing
        //
        const setCountByTheme = new Map<number, number>();
        for (const s of this.sets) {
            const tid = Number(s.legoThemeId);
            setCountByTheme.set(tid, (setCountByTheme.get(tid) || 0) + 1);
        }

        const buildNode = (theme: LegoThemeData): ThemeNode => {
            const children = themeMap.get(Number(theme.id).toString()) || [];
            const node: ThemeNode = {
                name: theme.name,
                id: theme.id
            };

            if (children.length > 0) {
                node.children = children.map(c => buildNode(c));
            } else {
                node.value = setCountByTheme.get(Number(theme.id)) || 1;
            }

            return node;
        };

        const rootData: ThemeNode = {
            name: 'LEGO',
            id: 0,
            children: topLevel.map(t => buildNode(t))
        };

        //
        // D3 hierarchy
        //
        const root = d3.hierarchy(rootData)
            .sum((d: any) => d.value || 0)
            .sort((a, b) => (b.value || 0) - (a.value || 0));

        const partition = d3.partition<ThemeNode>()
            .size([2 * Math.PI, radius]);

        partition(root);

        //
        // Color scale
        //
        const colorScale = d3.scaleOrdinal(d3.schemeTableau10);

        //
        // Create SVG
        //
        d3.select(container).selectAll('svg').remove();

        const svg = d3.select(container)
            .append('svg')
            .attr('viewBox', `0 0 ${width} ${height}`)
            .attr('width', '100%')
            .attr('height', '100%')
            .append('g')
            .attr('transform', `translate(${width / 2},${height / 2})`);

        const arc = d3.arc<d3.HierarchyRectangularNode<ThemeNode>>()
            .startAngle(d => d.x0)
            .endAngle(d => d.x1)
            .padAngle(0.005)
            .padRadius(radius / 2)
            .innerRadius(d => d.y0)
            .outerRadius(d => d.y1 - 1);

        //
        // Draw arcs
        //
        const paths = svg.selectAll('path')
            .data(root.descendants().filter(d => d.depth > 0))
            .enter()
            .append('path')
            .attr('d', arc as any)
            .attr('fill', (d: any) => {
                //
                // Color by top-level ancestor
                //
                let current = d;
                while (current.depth > 1) {
                    current = current.parent;
                }
                return colorScale(current.data.name);
            })
            .attr('fill-opacity', (d: any) => 0.85 - (d.depth - 1) * 0.15)
            .attr('stroke', 'rgba(0,0,0,0.3)')
            .attr('stroke-width', 0.5)
            .style('cursor', 'pointer')
            .on('mouseover', function (_event: any, d: any) {
                d3.select(this)
                    .attr('fill-opacity', 1)
                    .attr('stroke', 'var(--bmc-primary)')
                    .attr('stroke-width', 2);
            })
            .on('mouseout', function (_event: any, d: any) {
                d3.select(this)
                    .attr('fill-opacity', 0.85 - (d.depth - 1) * 0.15)
                    .attr('stroke', 'rgba(0,0,0,0.3)')
                    .attr('stroke-width', 0.5);
            })
            .on('click', (_event: any, d: any) => {
                this.router.navigate(['/lego/themes', Number(d.data.id)]);
            });

        //
        // Animate arcs in
        //
        paths.attr('opacity', 0)
            .transition()
            .duration(800)
            .delay((_d: any, i: number) => i * 5)
            .attr('opacity', 1);

        //
        // Center label
        //
        svg.append('text')
            .attr('text-anchor', 'middle')
            .attr('dy', '-0.2em')
            .attr('fill', 'var(--bmc-text-primary)')
            .attr('font-size', '1.1rem')
            .attr('font-weight', '700')
            .text('Themes');

        svg.append('text')
            .attr('text-anchor', 'middle')
            .attr('dy', '1.2em')
            .attr('fill', 'var(--bmc-text-muted)')
            .attr('font-size', '0.75rem')
            .text('Click to explore');

        //
        // Tooltip
        //
        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'sunburst-tooltip')
            .style('opacity', 0);

        paths.on('mouseover', function (_event: any, d: any) {
            d3.select(this)
                .attr('fill-opacity', 1)
                .attr('stroke', 'var(--bmc-primary)')
                .attr('stroke-width', 2);

            tooltip.html(`<strong>${d.data.name}</strong><br/>${d.value || 0} sets`)
                .style('opacity', 1);
        })
            .on('mousemove', function (event: any) {
                const [x, y] = d3.pointer(event, container);
                tooltip
                    .style('left', (x + 15) + 'px')
                    .style('top', (y - 10) + 'px');
            })
            .on('mouseout', function (_event: any, d: any) {
                d3.select(this)
                    .attr('fill-opacity', 0.85 - (d.depth - 1) * 0.15)
                    .attr('stroke', 'rgba(0,0,0,0.3)')
                    .attr('stroke-width', 0.5);

                tooltip.style('opacity', 0);
            });

        this.sunburstReady = true;
    }

    // ----------------------------------------------------------------
    //  D3 Timeline — Sets by Year
    // ----------------------------------------------------------------

    private renderTimeline(): void {
        if (!this.timelineContainer || this.sets.length === 0) {
            return;
        }

        const container = this.timelineContainer.nativeElement as HTMLElement;
        const margin = { top: 20, right: 30, bottom: 50, left: 50 };
        const width = (container.clientWidth || 800) - margin.left - margin.right;
        const height = 260 - margin.top - margin.bottom;

        //
        // Aggregate sets by year
        //
        const yearMap = new Map<number, number>();
        for (const s of this.sets) {
            const y = Number(s.year);
            if (y > 0) {
                yearMap.set(y, (yearMap.get(y) || 0) + 1);
            }
        }

        const buckets: YearBucket[] = Array.from(yearMap.entries())
            .map(([year, count]) => ({ year, count }))
            .sort((a, b) => a.year - b.year);

        if (buckets.length === 0) {
            return;
        }

        //
        // Scales
        //
        const xScale = d3.scaleBand()
            .domain(buckets.map(b => b.year.toString()))
            .range([0, width])
            .padding(0.15);

        const yScale = d3.scaleLinear()
            .domain([0, d3.max(buckets, b => b.count) || 1])
            .nice()
            .range([height, 0]);

        //
        // Create SVG
        //
        d3.select(container).selectAll('svg').remove();

        const svg = d3.select(container)
            .append('svg')
            .attr('viewBox', `0 0 ${width + margin.left + margin.right} ${height + margin.top + margin.bottom}`)
            .attr('width', '100%')
            .attr('height', '100%')
            .append('g')
            .attr('transform', `translate(${margin.left},${margin.top})`);

        //
        // Gradient for bars
        //
        const defs = svg.append('defs');
        const gradient = defs.append('linearGradient')
            .attr('id', 'barGradient')
            .attr('x1', '0%').attr('y1', '100%')
            .attr('x2', '0%').attr('y2', '0%');
        gradient.append('stop').attr('offset', '0%').attr('stop-color', '#667eea');
        gradient.append('stop').attr('offset', '100%').attr('stop-color', '#764ba2');

        //
        // X axis — show every Nth label to avoid crowding
        //
        const tickInterval = Math.max(1, Math.floor(buckets.length / 15));
        const xAxis = d3.axisBottom(xScale)
            .tickValues(buckets
                .filter((_b, i) => i % tickInterval === 0)
                .map(b => b.year.toString()));

        svg.append('g')
            .attr('transform', `translate(0,${height})`)
            .call(xAxis)
            .selectAll('text')
            .attr('fill', 'var(--bmc-text-muted)')
            .attr('font-size', '0.7rem')
            .attr('transform', 'rotate(-45)')
            .attr('text-anchor', 'end');

        svg.selectAll('.domain, .tick line')
            .attr('stroke', 'var(--bmc-border)');

        //
        // Y axis
        //
        svg.append('g')
            .call(d3.axisLeft(yScale).ticks(5))
            .selectAll('text')
            .attr('fill', 'var(--bmc-text-muted)')
            .attr('font-size', '0.7rem');

        svg.selectAll('.domain, .tick line')
            .attr('stroke', 'var(--bmc-border)');

        //
        // Tooltip
        //
        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'timeline-tooltip')
            .style('opacity', 0);

        //
        // Bars
        //
        svg.selectAll('.bar')
            .data(buckets)
            .enter()
            .append('rect')
            .attr('class', 'bar')
            .attr('x', d => xScale(d.year.toString()) || 0)
            .attr('width', xScale.bandwidth())
            .attr('y', height)
            .attr('height', 0)
            .attr('rx', 2)
            .attr('fill', 'url(#barGradient)')
            .style('cursor', 'pointer')
            .on('mouseover', function (event: any, d: YearBucket) {
                d3.select(this).attr('fill-opacity', 0.8);
                tooltip.html(`<strong>${d.year}</strong><br/>${d.count} sets`)
                    .style('opacity', 1);
            })
            .on('mousemove', function (event: any) {
                const [x, y] = d3.pointer(event, container);
                tooltip
                    .style('left', (x + 15) + 'px')
                    .style('top', (y - 10) + 'px');
            })
            .on('mouseout', function () {
                d3.select(this).attr('fill-opacity', 1);
                tooltip.style('opacity', 0);
            })
            .on('click', (_event: any, d: YearBucket) => {
                this.navigateToSetsByYear(d.year);
            })
            .transition()
            .duration(600)
            .delay((_d: any, i: number) => i * 8)
            .attr('y', (d: YearBucket) => yScale(d.count))
            .attr('height', (d: YearBucket) => height - yScale(d.count));

        //
        // Y-axis label
        //
        svg.append('text')
            .attr('transform', 'rotate(-90)')
            .attr('y', -margin.left + 12)
            .attr('x', -(height / 2))
            .attr('text-anchor', 'middle')
            .attr('fill', 'var(--bmc-text-muted)')
            .attr('font-size', '0.75rem')
            .text('Number of Sets');

        this.timelineReady = true;
    }

    // ----------------------------------------------------------------
    //  Navigation
    // ----------------------------------------------------------------

    navigateToSets(): void {
        this.router.navigate(['/lego/sets']);
    }

    navigateToMinifigs(): void {
        this.router.navigate(['/lego/minifigs']);
    }

    navigateToThemes(): void {
        this.router.navigate(['/lego/themes']);
    }

    navigateToSetsWithTheme(themeId: bigint | number): void {
        this.router.navigate(['/lego/sets'], { queryParams: { theme: Number(themeId) } });
    }

    navigateToSetsByYear(year: number): void {
        this.router.navigate(['/lego/sets'], { queryParams: { year } });
    }
}
