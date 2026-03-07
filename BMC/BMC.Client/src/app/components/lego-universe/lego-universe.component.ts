import { Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil, map } from 'rxjs/operators';
import { LegoMinifigService } from '../../bmc-data-services/lego-minifig.service';
import { LegoThemeService } from '../../bmc-data-services/lego-theme.service';
import { UserProfilePreferredThemeService } from '../../bmc-data-services/user-profile-preferred-theme.service';
import { SetExplorerApiService, SetExplorerItem } from '../../services/set-explorer-api.service';
import { AuthService } from '../../services/auth.service';
import { MinifigGalleryApiService, MinifigGalleryItem } from '../../services/minifig-gallery-api.service';
import { PartsUniverseApiService } from '../../services/parts-universe.service';
import { IndexedDBCacheService } from '../../services/indexeddb-cache.service';
import { SetOwnershipCacheService } from '../../services/set-ownership-cache.service';
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

/**
 * Serialisable subset of LegoThemeData — only the fields the sunburst needs.
 * These plain objects survive IndexedDB round-trips without BehaviorSubject issues.
 */
interface CachedTheme {
    id: number;
    name: string;
    legoThemeId: number;
}

interface NavCardData {
    title: string;
    description: string;
    icon: string;
    gradient: string;
    route: string;
    previewImageUrl: string | null;
    statLine: string;
    trendBadge: string;
}

interface DecadeBucket {
    label: string;
    startYear: number;
    endYear: number;
    setCount: number;
    themeCount: number;
}

@Component({
    selector: 'app-lego-universe',
    templateUrl: './lego-universe.component.html',
    styleUrl: './lego-universe.component.scss'
})
export class LegoUniverseComponent implements OnInit, OnDestroy, AfterViewInit {

    /** Static flag — counters animate only on first visit per session */
    private static hasAnimated = false;

    @ViewChild('sunburstContainer', { static: false }) sunburstContainer!: ElementRef;
    @ViewChild('timelineContainer', { static: false }) timelineContainer!: ElementRef;

    private destroy$ = new Subject<void>();
    private taglineInterval: ReturnType<typeof setInterval> | null = null;

    //
    // Hero — rotating taglines
    //
    taglines: string[] = [
        'Discover thousands of sets',
        'Explore every minifig ever made',
        'Dive into hundreds of themes',
        'Rank every brick in the universe'
    ];
    activeTaglineIndex = 0;
    taglineVisible = true;

    //
    // Stats — animated counters
    //
    totalSets = 0;
    totalMinifigs = 0;
    totalThemes = 0;
    totalParts = 0;
    displaySets = 0;
    displayMinifigs = 0;
    displayThemes = 0;
    displayParts = 0;

    //
    // Data
    //
    themes: CachedTheme[] = [];
    sets: SetExplorerItem[] = [];
    loading = true;
    sunburstReady = false;
    timelineReady = false;

    //
    // Recent sets for the carousel (derived from cached set-explorer data)
    //
    recentSets: SetExplorerItem[] = [];

    //
    // Spotlight sections
    //
    randomDiscoveryItems: { type: 'set' | 'minifig'; item: SetExplorerItem | MinifigGalleryItem }[] = [];
    epicSets: SetExplorerItem[] = [];
    carouselOffset = 0;
    private readonly CAROUSEL_PAGE_SIZE = 4;

    //
    // Search — cross-domain search across sets, minifigs, and themes
    //
    searchTerm = '';
    searchDropdownVisible = false;
    private searchDebounceTimer: ReturnType<typeof setTimeout> | null = null;
    searchResults: {
        sets: SetExplorerItem[];
        minifigs: MinifigGalleryItem[];
        themes: CachedTheme[];
    } = { sets: [], minifigs: [], themes: [] };
    private allMinifigs: MinifigGalleryItem[] = [];

    //
    // Live nav cards
    //
    navCards: NavCardData[] = [];

    //
    // Fun fact
    //
    didYouKnow = '';

    //
    // My Universe — personalized section from user's preferred themes
    //
    userPreferredThemeIds: number[] = [];
    mySets: SetExplorerItem[] = [];
    myMinifigs: MinifigGalleryItem[] = [];

    //
    // Decade navigation
    //
    decades: DecadeBucket[] = [];

    //
    // Collection stats
    //
    ownedCount = 0;
    wantedCount = 0;
    collectionPct = 0;

    //
    // Tap hint — show pulse animation on stat cards for first visit per session
    //
    showTapHint = false;

    constructor(
        public router: Router,
        private http: HttpClient,
        private minifigService: LegoMinifigService,
        private themeService: LegoThemeService,
        private setExplorerApi: SetExplorerApiService,
        private minifigGalleryApi: MinifigGalleryApiService,
        private partsUniverseApi: PartsUniverseApiService,
        private cacheService: IndexedDBCacheService,
        private userPrefThemeService: UserProfilePreferredThemeService,
        private ownershipCache: SetOwnershipCacheService,
        private authService: AuthService
    ) {
        if (this.authService.isLoggedIn) {
            this.ownershipCache.ensureLoaded();
            this.ownershipCache.ownedIds$.pipe(takeUntil(this.destroy$)).subscribe(ids => {
                this.ownedCount = ids.size;
                this.collectionPct = this.totalSets > 0 ? Math.round((ids.size / this.totalSets) * 100) : 0;
            });
            this.ownershipCache.wantedIds$.pipe(takeUntil(this.destroy$)).subscribe(ids => {
                this.wantedCount = ids.size;
            });
        }
    }

    ngOnInit(): void {
        this.loadData();
        this.startTaglineRotation();

        //
        // Show tap hint pulse on first visit per session
        //
        const hintKey = 'bmc-universe-tap-hint-shown';
        if (!sessionStorage.getItem(hintKey)) {
            this.showTapHint = true;
            sessionStorage.setItem(hintKey, '1');
            // Auto-dismiss after the CSS animation plays 3 times (2s × 3 = 6s)
            setTimeout(() => this.showTapHint = false, 6000);
        }
    }

    ngAfterViewInit(): void {
        //
        // D3 rendering will be triggered after data arrives
        //
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();

        if (this.taglineInterval !== null) {
            clearInterval(this.taglineInterval);
        }

        if (this.searchDebounceTimer !== null) {
            clearTimeout(this.searchDebounceTimer);
        }
    }


    //
    // Tagline rotation — cycles the hero subtitle every 4 seconds with a fade transition
    //
    private startTaglineRotation(): void {
        this.taglineInterval = setInterval(() => {
            //
            // Fade out, swap text, fade in
            //
            this.taglineVisible = false;

            setTimeout(() => {
                this.activeTaglineIndex = (this.activeTaglineIndex + 1) % this.taglines.length;
                this.taglineVisible = true;
            }, 400);
        }, 4000);
    }

    loadData(): void {
        this.loading = true;

        //
        // Fetch data from cached sources in parallel.
        // For anonymous users, use the gallery count instead of the authenticated row-count endpoint,
        // and use a direct public API call for themes instead of the generated data service.
        //
        const minifigCountSource = this.authService.isLoggedIn
            ? this.cacheService.getOrFetch<number>(
                'lego-minifig-count',
                {},
                () => this.minifigService.GetLegoMinifigsRowCount({ active: true, deleted: false }).pipe(
                    map(n => Number(n))
                ),
                1440
            )
            : this.minifigGalleryApi.getGalleryMinifigs().pipe(map(list => list.length));

        const themesSource = this.authService.isLoggedIn
            ? this.cacheService.getOrFetch<CachedTheme[]>(
                'lego-themes',
                {},
                () => this.themeService.GetLegoThemeList({ active: true, deleted: false }).pipe(
                    map(list => list.map(t => ({
                        id: Number(t.id),
                        name: t.name,
                        legoThemeId: Number(t.legoThemeId)
                    })))
                ),
                1440
            )
            : this.cacheService.getOrFetch<CachedTheme[]>(
                'lego-themes-public',
                {},
                () => this.http.get<any[]>('/api/public/browse/themes').pipe(
                    map(list => list.map((t: any) => ({
                        id: Number(t.id),
                        name: t.name,
                        legoThemeId: Number(t.legoThemeId ?? t.parentId ?? 0)
                    })))
                ),
                1440
            );

        forkJoin({
            allSets: this.setExplorerApi.getExploreSets(),
            allMinifigs: this.minifigGalleryApi.getGalleryMinifigs(),
            minifigCount: minifigCountSource,
            themes: themesSource,
            partsStats: this.partsUniverseApi.getPayload().pipe(
                map(payload => payload.stats)
            )
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                //
                // Sets — derive count and recent (top 12 by year desc) from the full cached list
                //
                this.sets = result.allSets;
                this.totalSets = result.allSets.length;

                const sorted = [...result.allSets].sort((a, b) => b.year - a.year);
                this.recentSets = sorted.slice(0, 12);

                //
                // Epic sets — top 6 by part count
                //
                this.epicSets = [...result.allSets]
                    .sort((a, b) => (b.partCount ?? 0) - (a.partCount ?? 0))
                    .slice(0, 6);

                //
                // Decade buckets — group sets by decade for browsing
                //
                const decadeMap = new Map<number, { count: number; themes: Set<number> }>();
                for (const s of result.allSets) {
                    if (!s.year || s.year < 1900) continue;
                    const decade = Math.floor(s.year / 10) * 10;
                    let bucket = decadeMap.get(decade);
                    if (!bucket) {
                        bucket = { count: 0, themes: new Set() };
                        decadeMap.set(decade, bucket);
                    }
                    bucket.count++;
                    if (s.themeId) bucket.themes.add(s.themeId);
                }
                this.decades = Array.from(decadeMap.entries())
                    .map(([decade, b]) => ({
                        label: `${decade}s`,
                        startYear: decade,
                        endYear: decade + 9,
                        setCount: b.count,
                        themeCount: b.themes.size
                    }))
                    .sort((a, b) => a.startYear - b.startYear);

                //
                // Minifigs
                //
                this.allMinifigs = result.allMinifigs;
                this.totalMinifigs = result.minifigCount;

                //
                // Build random discovery items once minifigs are loaded
                //
                this.shuffleDiscovery();

                //
                // Themes
                //
                this.themes = result.themes;
                this.totalThemes = result.themes.length;

                //
                // Parts
                //
                this.totalParts = result.partsStats.totalUniqueParts;

                //
                // Update taglines with real numbers now that data has loaded
                //
                this.taglines = [
                    `Discover ${this.totalSets.toLocaleString()} sets`,
                    `Explore ${this.totalMinifigs.toLocaleString()} minifigs`,
                    `Dive into ${this.totalThemes.toLocaleString()} themes`,
                    `Rank ${this.totalParts.toLocaleString()} unique parts`
                ];

                this.loading = false;

                //
                // Animate counters
                //
                if (LegoUniverseComponent.hasAnimated) {
                    // Skip animation — show final values immediately
                    this.displaySets = this.totalSets;
                    this.displayMinifigs = this.totalMinifigs;
                    this.displayThemes = this.totalThemes;
                    this.displayParts = this.totalParts;
                } else {
                    this.animateCounter('displaySets', this.totalSets, 1500);
                    this.animateCounter('displayMinifigs', this.totalMinifigs, 1800);
                    this.animateCounter('displayThemes', this.totalThemes, 1200);
                    this.animateCounter('displayParts', this.totalParts, 1600);
                    LegoUniverseComponent.hasAnimated = true;
                }

                //
                // Build nav cards with live data
                //
                this.buildNavCards();

                //
                // Generate a fun fact
                //
                this.generateFunFact();

                //
                // Load user preferred themes for My Universe section
                //
                if (this.authService.isLoggedIn) {
                    this.loadUserPreferredThemes();
                }

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
    private animateCounter(prop: 'displaySets' | 'displayMinifigs' | 'displayThemes' | 'displayParts', target: number, duration: number): void {
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
        const themeMap = new Map<string, CachedTheme[]>();
        const topLevel: CachedTheme[] = [];

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
            const tid = Number(s.themeId);
            setCountByTheme.set(tid, (setCountByTheme.get(tid) || 0) + 1);
        }

        const buildNode = (theme: CachedTheme): ThemeNode => {
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

    navigateToPartsUniverse(): void {
        this.router.navigate(['/lego/parts-universe']);
    }

    navigateToSetsWithTheme(themeId: bigint | number): void {
        this.router.navigate(['/lego/sets'], { queryParams: { theme: Number(themeId) } });
    }

    navigateToSetsByYear(year: number): void {
        this.router.navigate(['/lego/sets'], { queryParams: { year } });
    }

    /**
     * Navigate to the detail page for a discovery item (set or minifig).
     */
    navigateToDiscoveryItem(entry: { type: 'set' | 'minifig'; item: SetExplorerItem | MinifigGalleryItem }): void {
        if (entry.type === 'set') {
            this.router.navigate(['/lego/sets', entry.item.id]);
        } else {
            this.router.navigate(['/lego/minifigs', entry.item.id]);
        }
    }


    // ----------------------------------------------------------------
    //  Spotlight — Random Discovery
    // ----------------------------------------------------------------

    shuffleDiscovery(): void {
        const items: { type: 'set' | 'minifig'; item: SetExplorerItem | MinifigGalleryItem }[] = [];

        //
        // Pick 3 random sets (from items with images when possible)
        //
        const setsWithImages = this.sets.filter(s => s.imageUrl !== null && s.imageUrl !== '');
        const setPool = setsWithImages.length >= 3 ? setsWithImages : this.sets;
        for (let i = 0; i < 3 && setPool.length > 0; i++) {
            const idx = Math.floor(Math.random() * setPool.length);
            items.push({ type: 'set', item: setPool[idx] });
        }

        //
        // Pick 3 random minifigs (from items with images when possible)
        //
        const figsWithImages = this.allMinifigs.filter(m => m.imageUrl !== null && m.imageUrl !== '');
        const figPool = figsWithImages.length >= 3 ? figsWithImages : this.allMinifigs;
        for (let i = 0; i < 3 && figPool.length > 0; i++) {
            const idx = Math.floor(Math.random() * figPool.length);
            items.push({ type: 'minifig', item: figPool[idx] });
        }

        this.randomDiscoveryItems = items;
    }


    // ----------------------------------------------------------------
    //  Spotlight — Carousel Navigation
    // ----------------------------------------------------------------

    scrollCarousel(direction: 'prev' | 'next'): void {
        if (direction === 'prev') {
            this.carouselOffset = Math.max(0, this.carouselOffset - this.CAROUSEL_PAGE_SIZE);
        } else {
            const maxOffset = Math.max(0, this.recentSets.length - this.CAROUSEL_PAGE_SIZE);
            this.carouselOffset = Math.min(maxOffset, this.carouselOffset + this.CAROUSEL_PAGE_SIZE);
        }
    }

    get visibleRecentSets(): SetExplorerItem[] {
        return this.recentSets.slice(this.carouselOffset, this.carouselOffset + this.CAROUSEL_PAGE_SIZE);
    }

    get canScrollPrev(): boolean {
        return this.carouselOffset > 0;
    }

    get canScrollNext(): boolean {
        return this.carouselOffset + this.CAROUSEL_PAGE_SIZE < this.recentSets.length;
    }


    // ----------------------------------------------------------------
    //  Universal Search
    // ----------------------------------------------------------------

    onSearchInput(): void {
        //
        // Debounce to avoid filtering on every keystroke
        //
        if (this.searchDebounceTimer !== null) {
            clearTimeout(this.searchDebounceTimer);
        }

        if (this.searchTerm.trim().length === 0) {
            this.closeSearch();
            return;
        }

        this.searchDebounceTimer = setTimeout(() => {
            this.performSearch();
        }, 300);
    }

    private performSearch(): void {
        const term = this.searchTerm.trim().toLowerCase();

        if (term.length < 2) {
            this.searchDropdownVisible = false;
            return;
        }

        //
        // Search sets by name or set number
        //
        this.searchResults.sets = this.sets
            .filter(s => s.name.toLowerCase().includes(term) || s.setNumber.toLowerCase().includes(term))
            .slice(0, 5);

        //
        // Search minifigs by name or fig number
        //
        this.searchResults.minifigs = this.allMinifigs
            .filter(m => m.name.toLowerCase().includes(term) || m.figNumber.toLowerCase().includes(term))
            .slice(0, 5);

        //
        // Search themes by name
        //
        this.searchResults.themes = this.themes
            .filter(t => t.name.toLowerCase().includes(term))
            .slice(0, 5);

        this.searchDropdownVisible =
            this.searchResults.sets.length > 0 ||
            this.searchResults.minifigs.length > 0 ||
            this.searchResults.themes.length > 0;
    }

    closeSearch(): void {
        this.searchDropdownVisible = false;
        this.searchResults = { sets: [], minifigs: [], themes: [] };
    }

    /**
     * Navigate to a sub-component with the current search term pre-applied.
     */
    navigateFromSearch(type: 'sets' | 'minifigs' | 'themes'): void {
        const search = this.searchTerm.trim();

        switch (type) {
            case 'sets':
                this.router.navigate(['/lego/sets'], { queryParams: { search } });
                break;
            case 'minifigs':
                this.router.navigate(['/lego/minifigs'], { queryParams: { search } });
                break;
            case 'themes':
                this.router.navigate(['/lego/themes'], { queryParams: { search } });
                break;
        }

        this.closeSearch();
    }

    @HostListener('document:keydown.escape')
    onEscapeKey(): void {
        this.closeSearch();
    }

    @HostListener('document:keydown', ['$event'])
    onKeydown(event: KeyboardEvent): void {
        const tag = (event.target as HTMLElement)?.tagName;
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;
        if (event.key === '/') {
            event.preventDefault();
            document.getElementById('universe-search-input')?.focus();
        }
    }

    // Back-to-top
    showBackToTop = false;

    @HostListener('window:scroll')
    onWindowScroll(): void {
        this.showBackToTop = window.scrollY > 400;
    }

    scrollToTop(): void {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    /**
     * Navigate directly to a specific item's detail page from search results.
     */
    navigateToSearchResult(type: 'set' | 'minifig' | 'theme', id: number | bigint): void {
        switch (type) {
            case 'set':
                this.router.navigate(['/lego/sets', Number(id)]);
                break;
            case 'minifig':
                this.router.navigate(['/lego/minifigs', Number(id)]);
                break;
            case 'theme':
                this.router.navigate(['/lego/themes', Number(id)]);
                break;
        }
        this.closeSearch();
    }


    // ----------------------------------------------------------------
    //  Phase 4 — Live Nav Cards
    // ----------------------------------------------------------------

    private buildNavCards(): void {
        //
        // Determine year range for the trend badge
        //
        const years = this.sets.map(s => s.year).filter(y => y > 0);
        const minYear = years.length > 0 ? Math.min(...years) : 1950;
        const maxYear = years.length > 0 ? Math.max(...years) : 2024;

        this.navCards = [
            {
                title: 'Set Explorer',
                description: 'Browse and search through all LEGO sets with visual filters',
                icon: 'fas fa-box-open',
                gradient: 'sets-gradient',
                route: '/lego/sets',
                previewImageUrl: null,
                statLine: `${this.totalSets.toLocaleString()} sets loaded`,
                trendBadge: `${minYear}–${maxYear}`
            },
            {
                title: 'Minifig Gallery',
                description: 'Discover minifigs and see which sets they appear in',
                icon: 'fas fa-child',
                gradient: 'minifigs-gradient',
                route: '/lego/minifigs',
                previewImageUrl: null,
                statLine: `${this.totalMinifigs.toLocaleString()} figs catalogued`,
                trendBadge: 'All time'
            },
            {
                title: 'Theme Explorer',
                description: 'Explore LEGO themes, sub-themes, and their hierarchies',
                icon: 'fas fa-layer-group',
                gradient: 'themes-gradient',
                route: '/lego/themes',
                previewImageUrl: null,
                statLine: `${this.totalThemes.toLocaleString()} themes mapped`,
                trendBadge: 'Hierarchy'
            },
            {
                title: 'Parts Universe',
                description: 'Rank, visualize, and explore every brick across all sets',
                icon: 'fas fa-atom',
                gradient: 'parts-gradient',
                route: '/lego/parts-universe',
                previewImageUrl: null,
                statLine: `${this.totalParts.toLocaleString()} unique elements`,
                trendBadge: 'Rankings'
            }
        ];
    }


    // ----------------------------------------------------------------
    //  Phase 7 — Fun Fact
    // ----------------------------------------------------------------

    private generateFunFact(): void {
        const facts: string[] = [];

        if (this.totalSets > 0 && this.totalParts > 0) {
            const avgParts = Math.round(this.totalParts / this.totalSets);
            facts.push(`On average, each LEGO set uses about ${avgParts} unique part types.`);
        }

        if (this.totalMinifigs > 0) {
            facts.push(`There are ${this.totalMinifigs.toLocaleString()} minifigs in the database — that's a tiny army!`);
        }

        if (this.epicSets.length > 0) {
            const biggest = this.epicSets[0];
            facts.push(`The most epic set is "${biggest.name}" with ${(biggest.partCount ?? 0).toLocaleString()} parts!`);
        }

        if (this.totalThemes > 0) {
            facts.push(`LEGO has created ${this.totalThemes.toLocaleString()} different themes over the years.`);
        }

        const years = this.sets.map(s => s.year).filter(y => y > 0);
        if (years.length > 0) {
            const span = Math.max(...years) - Math.min(...years);
            facts.push(`The sets in this database span ${span} years of LEGO history.`);
        }

        if (facts.length > 0) {
            this.didYouKnow = facts[Math.floor(Math.random() * facts.length)];
        }
    }


    // ----------------------------------------------------------------
    //  Phase 6 — My Universe (personalized)
    // ----------------------------------------------------------------

    private loadUserPreferredThemes(): void {
        this.userPrefThemeService.GetUserProfilePreferredThemeList().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (prefs) => {
                if (!prefs || prefs.length === 0) {
                    return;
                }

                this.userPreferredThemeIds = prefs.map(p => Number(p.legoThemeId));

                //
                // Filter sets that belong to user's preferred themes
                //
                this.mySets = this.sets
                    .filter(s => this.userPreferredThemeIds.includes(s.themeId))
                    .slice(0, 6);

                //
                // Filter minifigs whose themeIds overlap with preferred themes
                //
                this.myMinifigs = this.allMinifigs
                    .filter(m => m.themeIds?.some(tid => this.userPreferredThemeIds.includes(tid)))
                    .slice(0, 6);
            },
            error: () => {
                // Silently fail — personalization is optional
            }
        });
    }
}
