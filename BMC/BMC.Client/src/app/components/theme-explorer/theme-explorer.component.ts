import { Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { LegoThemeService, LegoThemeData } from '../../bmc-data-services/lego-theme.service';
import { LegoSetService } from '../../bmc-data-services/lego-set.service';
import * as d3 from 'd3';

interface ThemeNode {
    name: string;
    id: bigint | number;
    children?: ThemeNode[];
    value?: number;
}

interface PreviewImage {
    id: number;
    imageUrl: string;
}

interface ThemeCard {
    theme: LegoThemeData;
    setCount: number;
    subThemeCount: number;
    totalSets: number; // including sub-theme sets
    previewImages: PreviewImage[];
}

@Component({
    selector: 'app-theme-explorer',
    templateUrl: './theme-explorer.component.html',
    styleUrl: './theme-explorer.component.scss'
})
export class ThemeExplorerComponent implements OnInit, OnDestroy, AfterViewInit {

    @ViewChild('sunburstContainer', { static: false }) sunburstContainer!: ElementRef;

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    // Data
    themes: LegoThemeData[] = [];
    topLevelCards: ThemeCard[] = [];
    filteredCards: ThemeCard[] = [];
    loading = true;
    sunburstReady = false;

    // Hierarchy
    childMap = new Map<number, LegoThemeData[]>();
    expandedThemes = new Set<number>();

    // Stats
    totalThemes = 0;
    rootThemeCount = 0;
    largestThemeName = '';
    largestThemeSetCount = 0;

    // Search
    searchTerm = '';

    // Set counts per theme (by id)
    private setCountByTheme = new Map<number, number>();

    // Theme thumbnail previews (up to 4 per theme)
    themePreviewImages = new Map<number, PreviewImage[]>();

    constructor(
        private router: Router,
        private themeService: LegoThemeService,
        private setService: LegoSetService
    ) { }

    ngOnInit(): void {
        this.searchSubject.pipe(
            debounceTime(250),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.applyFilter();
        });

        this.loadData();
    }

    ngAfterViewInit(): void { }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    onSearch(event: Event): void {
        const value = (event.target as HTMLInputElement).value;
        this.searchSubject.next(value);
    }

    private loadData(): void {
        this.loading = true;

        // Load themes and sets for counting
        this.themeService.GetLegoThemeList({ active: true, deleted: false })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (themes) => {
                    this.themes = themes;
                    this.totalThemes = themes.length;

                    // Now load sets to count per theme
                    this.setService.GetLegoSetList({ active: true, deleted: false, pageSize: 5000, pageNumber: 1 })
                        .pipe(takeUntil(this.destroy$))
                        .subscribe({
                            next: (sets) => {
                                // Build set count map and collect preview images
                                for (const s of sets) {
                                    const tid = Number(s.legoThemeId);
                                    this.setCountByTheme.set(tid, (this.setCountByTheme.get(tid) || 0) + 1);

                                    // Collect up to 4 images per theme
                                    if (s.imageUrl) {
                                        const imgs = this.themePreviewImages.get(tid) || [];
                                        if (imgs.length < 4) {
                                            imgs.push({ id: Number(s.id), imageUrl: s.imageUrl });
                                            this.themePreviewImages.set(tid, imgs);
                                        }
                                    }
                                }

                                this.buildThemeCards();
                                this.loading = false;

                                setTimeout(() => {
                                    this.renderSunburst(0);
                                }, 300);
                            },
                            error: () => {
                                this.buildThemeCards();
                                this.loading = false;
                            }
                        });
                },
                error: () => {
                    this.loading = false;
                }
            });
    }

    private buildThemeCards(): void {
        // Build parent-children map (class-level for template access)
        this.childMap = new Map<number, LegoThemeData[]>();
        const topLevel: LegoThemeData[] = [];

        for (const theme of this.themes) {
            const parentId = Number(theme.legoThemeId);
            if (parentId === 0 || !theme.legoThemeId) {
                topLevel.push(theme);
            } else {
                if (!this.childMap.has(parentId)) {
                    this.childMap.set(parentId, []);
                }
                this.childMap.get(parentId)!.push(theme);
            }
        }

        this.rootThemeCount = topLevel.length;

        // Build cards with counts
        this.topLevelCards = topLevel.map(theme => {
            const children = this.childMap.get(Number(theme.id)) || [];
            const directSets = this.setCountByTheme.get(Number(theme.id)) || 0;
            let totalSets = directSets;

            // Count sub-theme sets recursively
            const countSubSets = (themeId: number): number => {
                const subThemes = this.childMap.get(themeId) || [];
                let count = 0;
                for (const sub of subThemes) {
                    const sid = Number(sub.id);
                    count += this.setCountByTheme.get(sid) || 0;
                    count += countSubSets(sid);
                }
                return count;
            };

            totalSets += countSubSets(Number(theme.id));

            // Collect preview images (direct + children, up to 4)
            const tid = Number(theme.id);
            const directImgs = this.themePreviewImages.get(tid) || [];
            let allImgs: PreviewImage[] = [...directImgs];
            if (allImgs.length < 4) {
                const childThemes = this.childMap.get(tid) || [];
                for (const child of childThemes) {
                    if (allImgs.length >= 4) break;
                    const childImgs = this.themePreviewImages.get(Number(child.id)) || [];
                    for (const img of childImgs) {
                        if (allImgs.length >= 4) break;
                        allImgs.push(img);
                    }
                }
            }

            return {
                theme,
                setCount: directSets,
                subThemeCount: children.length,
                totalSets,
                previewImages: allImgs.slice(0, 4)
            };
        });

        // Sort by total sets descending
        this.topLevelCards.sort((a, b) => b.totalSets - a.totalSets);
        this.filteredCards = [...this.topLevelCards];

        // Find largest
        if (this.topLevelCards.length > 0) {
            this.largestThemeName = this.topLevelCards[0].theme.name;
            this.largestThemeSetCount = this.topLevelCards[0].totalSets;
        }
    }

    // ── Expand / Collapse ────────────────────────────

    toggleExpand(themeId: number, event: Event): void {
        event.stopPropagation();
        if (this.expandedThemes.has(themeId)) {
            this.expandedThemes.delete(themeId);
        } else {
            this.expandedThemes.add(themeId);
        }
    }

    isExpanded(themeId: number): boolean {
        return this.expandedThemes.has(themeId);
    }

    getChildThemes(themeId: number): LegoThemeData[] {
        return this.childMap.get(themeId) || [];
    }

    getSetCount(themeId: number): number {
        return this.setCountByTheme.get(themeId) || 0;
    }

    getThemePreviewImages(themeId: number): PreviewImage[] {
        return (this.themePreviewImages.get(themeId) || []).slice(0, 3);
    }

    navigateToSet(setId: number, event: Event): void {
        event.stopPropagation();
        this.router.navigate(['/lego/sets', setId]);
    }

    private applyFilter(): void {
        if (!this.searchTerm.trim()) {
            this.filteredCards = [...this.topLevelCards];
            this.expandedThemes.clear();
        } else {
            const term = this.searchTerm.toLowerCase();

            // Check if any sub-theme matches for a given parent
            const hasMatchingChild = (parentId: number): boolean => {
                const children = this.childMap.get(parentId) || [];
                return children.some(c => c.name.toLowerCase().includes(term));
            };

            this.filteredCards = this.topLevelCards.filter(c =>
                c.theme.name.toLowerCase().includes(term) ||
                hasMatchingChild(Number(c.theme.id))
            );

            // Auto-expand parents whose children match
            for (const card of this.filteredCards) {
                const tid = Number(card.theme.id);
                if (hasMatchingChild(tid)) {
                    this.expandedThemes.add(tid);
                }
            }
        }
    }

    navigateToTheme(themeId: bigint | number): void {
        this.router.navigate(['/lego/themes', Number(themeId)]);
    }

    navigateBack(): void {
        this.router.navigate(['/lego']);
    }

    // ----------------------------------------------------------------
    //  D3 Zoomable Sunburst — Click to drill, center to zoom out
    // ----------------------------------------------------------------
    private renderSunburst(retryCount: number = 0): void {
        if (!this.sunburstContainer || this.themes.length === 0) {
            if (retryCount < 3) {
                setTimeout(() => this.renderSunburst(retryCount + 1), 500);
            }
            return;
        }

        const container = this.sunburstContainer.nativeElement as HTMLElement;
        const width = container.clientWidth || 500;

        if (width < 10 && retryCount < 3) {
            setTimeout(() => this.renderSunburst(retryCount + 1), 500);
            return;
        }

        const height = Math.min(width, 500);
        const radius = Math.min(width, height) / 2;

        // Build hierarchy
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

        const buildNode = (theme: LegoThemeData): ThemeNode => {
            const children = themeMap.get(Number(theme.id).toString()) || [];
            const node: ThemeNode = {
                name: theme.name,
                id: theme.id
            };

            if (children.length > 0) {
                node.children = children.map(c => buildNode(c));
            } else {
                node.value = this.setCountByTheme.get(Number(theme.id)) || 1;
            }

            return node;
        };

        const rootData: ThemeNode = {
            name: 'LEGO',
            id: 0,
            children: topLevel.map(t => buildNode(t))
        };

        const root = d3.hierarchy(rootData)
            .sum((d: any) => d.value || 0)
            .sort((a, b) => (b.value || 0) - (a.value || 0));

        const partition = d3.partition<ThemeNode>()
            .size([2 * Math.PI, radius]);

        partition(root);

        const colorScale = d3.scaleOrdinal(d3.schemeTableau10);

        // Store each node's original position for zoom transitions
        const allNodes = root.descendants();
        allNodes.forEach((d: any) => {
            d.current = { x0: d.x0, x1: d.x1, y0: d.y0, y1: d.y1 };
        });

        d3.select(container).selectAll('svg').remove();
        d3.select(container).selectAll('.sunburst-tooltip').remove();

        const svg = d3.select(container)
            .append('svg')
            .attr('viewBox', `0 0 ${width} ${height}`)
            .attr('width', '100%')
            .attr('height', '100%')
            .append('g')
            .attr('transform', `translate(${width / 2},${height / 2})`);

        // Arc generator reads from .current
        const arc = d3.arc<any>()
            .startAngle((d: any) => d.current.x0)
            .endAngle((d: any) => d.current.x1)
            .padAngle(0.005)
            .padRadius(radius / 2)
            .innerRadius((d: any) => d.current.y0)
            .outerRadius((d: any) => d.current.y1 - 1);

        // Helper: is the arc visible after zoom?
        const arcVisible = (d: any) => d.current.y1 > 0 && d.current.y0 < radius && (d.current.x1 - d.current.x0) > 0.001;

        // Color: find the child of the current zoom root that this node descends from
        let zoomDepth = 0; // depth of the current zoom root
        const getZoomedColor = (d: any): string => {
            let current = d;
            while (current.depth > zoomDepth + 1 && current.parent) {
                current = current.parent;
            }
            return colorScale(current.data.name);
        };

        // Draw arcs
        const paths = svg.selectAll('path.arc-slice')
            .data(allNodes.filter(d => d.depth > 0))
            .enter()
            .append('path')
            .attr('class', 'arc-slice')
            .attr('d', arc)
            .attr('fill', (d: any) => getZoomedColor(d))
            .attr('fill-opacity', (d: any) => arcVisible(d) ? 0.85 - (d.depth - 1) * 0.15 : 0)
            .attr('stroke', 'rgba(0,0,0,0.2)')
            .attr('stroke-width', 0.5)
            .attr('stroke-opacity', (d: any) => arcVisible(d) ? 1 : 0)
            .style('cursor', 'pointer')
            .style('pointer-events', (d: any) => arcVisible(d) ? 'auto' : 'none');

        // Center circle (clickable to zoom out)
        const centerCircle = svg.append('circle')
            .attr('r', (root as any).current.y1 > 0 ? (root as any).current.y1 * 0.01 : radius * 0.18)
            .attr('fill', 'var(--bmc-bg-elevated)')
            .attr('stroke', 'var(--bmc-border)')
            .attr('stroke-width', 1)
            .style('cursor', 'pointer')
            .attr('pointer-events', 'all');

        // Center text (shows current zoom level)
        const centerTitle = svg.append('text')
            .attr('text-anchor', 'middle')
            .attr('dy', '-0.2em')
            .attr('fill', 'var(--bmc-text-primary)')
            .attr('font-size', '1rem')
            .attr('font-weight', '700')
            .style('pointer-events', 'none')
            .text('Themes');

        const centerHint = svg.append('text')
            .attr('text-anchor', 'middle')
            .attr('dy', '1.2em')
            .attr('fill', 'var(--bmc-text-muted)')
            .attr('font-size', '0.7rem')
            .style('pointer-events', 'none')
            .text('Click to drill in');

        // Track current zoom root
        let currentRoot: any = root;

        // Zoom function
        const zoomTo = (target: any) => {
            currentRoot = target;
            zoomDepth = target.depth;

            // Update center label
            if (target === root) {
                centerTitle.text('Themes');
                centerHint.text('Click to drill in');
            } else {
                centerTitle.text(target.data.name);
                centerHint.text('↑ Click center to zoom out');
            }

            // Calculate new positions relative to target
            const targetX0 = target.x0;
            const targetX1 = target.x1;
            const targetY0 = target.y0;
            const xScale = (2 * Math.PI) / (targetX1 - targetX0);

            // Transition each node
            allNodes.forEach((d: any) => {
                d.target = {
                    x0: Math.max(0, Math.min(2 * Math.PI, (d.x0 - targetX0) * xScale)),
                    x1: Math.max(0, Math.min(2 * Math.PI, (d.x1 - targetX0) * xScale)),
                    y0: Math.max(0, d.y0 - targetY0),
                    y1: Math.max(0, d.y1 - targetY0)
                };
            });

            const t = svg.transition().duration(600);

            paths.transition(t as any)
                .tween('data', (d: any) => {
                    const i = d3.interpolate(d.current, d.target);
                    return (t: number) => { d.current = i(t); };
                })
                .attrTween('d', (d: any) => () => arc(d))
                .attr('fill', (d: any) => getZoomedColor(d))
                .attr('fill-opacity', (d: any) => {
                    const relativeDepth = d.depth - target.depth;
                    return relativeDepth > 0 && arcVisible({ current: d.target })
                        ? 0.85 - (relativeDepth - 1) * 0.15
                        : 0;
                })
                .attr('stroke-opacity', (d: any) => {
                    const relativeDepth = d.depth - target.depth;
                    return relativeDepth > 0 && arcVisible({ current: d.target }) ? 1 : 0;
                })
                .style('pointer-events', (d: any) => {
                    const relativeDepth = d.depth - target.depth;
                    return relativeDepth > 0 && arcVisible({ current: d.target }) ? 'auto' : 'none';
                });

            // Update center circle size
            centerCircle.transition(t as any)
                .attr('r', target === root ? radius * 0.18 : Math.max(radius * 0.18, target.y1 - target.y0));
        };

        // Click on arc → zoom in
        paths.on('click', (_event: any, d: any) => {
            // Only zoom if this node has children (something to zoom into)
            if (d.children && d.children.length > 0) {
                zoomTo(d);
            } else {
                // Leaf node → navigate to theme detail
                this.navigateToTheme(d.data.id);
            }
        });

        // Click center → zoom out one level
        centerCircle.on('click', () => {
            if (currentRoot !== root) {
                zoomTo(currentRoot.parent || root);
            }
        });

        // Tooltip
        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'sunburst-tooltip')
            .style('opacity', 0);

        paths.on('mouseover', function (_event: any, d: any) {
            if (!arcVisible(d)) return;
            d3.select(this)
                .attr('stroke', 'var(--bmc-primary)')
                .attr('stroke-width', 2);

            const label = d.children ? `${d.data.name} — click to zoom` : `${d.data.name} — ${d.value || 0} sets`;
            tooltip.html(`<strong>${d.data.name}</strong><br/>${d.value || 0} sets${d.children ? '<br/><em>Click to zoom in</em>' : ''}`)
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
                    .attr('stroke', 'rgba(0,0,0,0.3)')
                    .attr('stroke-width', 0.5);

                tooltip.style('opacity', 0);
            });

        // Animate initial appearance
        paths.attr('opacity', 0)
            .transition()
            .duration(800)
            .delay((_d: any, i: number) => i * 3)
            .attr('opacity', 1);

        this.sunburstReady = true;
    }
}
