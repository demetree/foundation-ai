import { Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import * as d3 from 'd3';
import { sankey as d3Sankey, sankeyLinkHorizontal } from 'd3-sankey';

import {
    PartsUniverseApiService,
    PartsUniversePayload,
    RankedPart,
    SankeyData, SankeyNode, SankeyLink,
    CategoryBubble, BubblePart,
    HeatmapData, HeatmapCell, HeatmapColourLabel,
    ChordData
} from '../../services/parts-universe.service';
import { LDrawThumbnailService } from '../../services/ldraw-thumbnail.service';

@Component({
    selector: 'app-parts-universe',
    templateUrl: './parts-universe.component.html',
    styleUrl: './parts-universe.component.scss'
})
export class PartsUniverseComponent implements OnInit, OnDestroy, AfterViewInit {
    @ViewChild('leaderboardContainer', { static: false }) leaderboardRef!: ElementRef;
    @ViewChild('sankeyContainer', { static: false }) sankeyRef!: ElementRef;
    @ViewChild('bubbleContainer', { static: false }) bubbleRef!: ElementRef;
    @ViewChild('heatmapContainer', { static: false }) heatmapRef!: ElementRef;
    @ViewChild('chordContainer', { static: false }) chordRef!: ElementRef;

    loading = true;
    rankedParts: RankedPart[] = [];
    leaderboardMode: 'most' | 'least' = 'most';
    totalParts = 0;
    totalInstances = 0;
    totalSets = 0;
    totalCategories = 0;
    thumbnails = new Map<string, string>();

    // ── Filter state ─────────────────────────────────────
    searchText = '';
    selectedCategories = new Set<string>();
    selectedThemes = new Set<string>();
    minSets = 0;
    filtersOpen = false;
    categoryDropdownOpen = false;
    themeDropdownOpen = false;
    private _filteredParts: RankedPart[] = [];

    private destroy$ = new Subject<void>();
    private payload: PartsUniversePayload | null = null;

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private partsUniverseApi: PartsUniverseApiService,
        private thumbnailService: LDrawThumbnailService
    ) { }

    ngOnInit(): void {
        // Restore state from query params
        const qp = this.route.snapshot.queryParams;
        if (qp['mode'] === 'least') {
            this.leaderboardMode = 'least';
        }
        if (qp['q']) {
            this.searchText = qp['q'];
        }
        if (qp['cats']) {
            qp['cats'].split(',').forEach((c: string) => this.selectedCategories.add(c));
        }
        if (qp['themes']) {
            qp['themes'].split(',').forEach((t: string) => this.selectedThemes.add(t));
        }
        if (qp['minSets']) {
            this.minSets = parseInt(qp['minSets'], 10) || 0;
        }

        // Subscribe to thumbnail render results
        this.thumbnailService.thumbnail$.pipe(
            takeUntil(this.destroy$)
        ).subscribe(result => {
            this.thumbnails.set(result.cacheKey, result.dataUrl);
        });

        this.loadData();
    }

    ngAfterViewInit(): void { }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    getPartThumbnailKey(rp: RankedPart): string {
        const hex = rp.colours?.length > 0 ? rp.colours[0].hex : undefined;
        return LDrawThumbnailService.cacheKey(
            rp.geometryFilePath || rp.ldrawPartId,
            hex ? (hex.startsWith('#') ? hex.slice(1) : hex) : undefined
        );
    }

    get displayedParts(): RankedPart[] {
        if (this.leaderboardMode === 'least') {
            return [...this._filteredParts].reverse();
        }
        return this._filteredParts;
    }

    get availableCategories(): string[] {
        const cats = new Set(this.rankedParts.map(rp => rp.categoryName));
        return [...cats].sort();
    }

    get availableThemes(): string[] {
        const themes = new Set<string>();
        this.rankedParts.forEach(rp => rp.themes?.forEach(t => themes.add(t.name)));
        return [...themes].sort();
    }

    get hasActiveFilters(): boolean {
        return this.searchText.length > 0
            || this.selectedCategories.size > 0
            || this.selectedThemes.size > 0
            || this.minSets > 0;
    }

    setLeaderboardMode(mode: 'most' | 'least'): void {
        this.leaderboardMode = mode;
        this.updateQueryParams();
    }

    toggleLeaderboardMode(): void {
        this.leaderboardMode = this.leaderboardMode === 'most' ? 'least' : 'most';
        this.updateQueryParams();
    }

    // ── Filter actions ───────────────────────────────────

    onSearchChange(text: string): void {
        this.searchText = text;
        this.applyFilters();
    }

    toggleCategory(cat: string): void {
        if (this.selectedCategories.has(cat)) {
            this.selectedCategories.delete(cat);
        } else {
            this.selectedCategories.add(cat);
        }
        this.applyFilters();
    }

    toggleTheme(theme: string): void {
        if (this.selectedThemes.has(theme)) {
            this.selectedThemes.delete(theme);
        } else {
            this.selectedThemes.add(theme);
        }
        this.applyFilters();
    }

    removeCategory(cat: string): void {
        this.selectedCategories.delete(cat);
        this.applyFilters();
    }

    removeTheme(theme: string): void {
        this.selectedThemes.delete(theme);
        this.applyFilters();
    }

    onMinSetsChange(value: number): void {
        this.minSets = value || 0;
        this.applyFilters();
    }

    clearAllFilters(): void {
        this.searchText = '';
        this.selectedCategories.clear();
        this.selectedThemes.clear();
        this.minSets = 0;
        this.applyFilters();
    }

    private applyFilters(): void {
        this.rebuildFilteredParts();
        this.updateQueryParams();
        setTimeout(() => this.renderAllPanels(), 50);
    }

    private rebuildFilteredParts(): void {
        let parts = this.rankedParts;

        // Text search
        if (this.searchText) {
            const q = this.searchText.toLowerCase();
            parts = parts.filter(rp =>
                (rp.name?.toLowerCase().includes(q)) ||
                (rp.ldrawPartId?.toLowerCase().includes(q)) ||
                (rp.ldrawTitle?.toLowerCase().includes(q))
            );
        }

        // Category filter
        if (this.selectedCategories.size > 0) {
            parts = parts.filter(rp => this.selectedCategories.has(rp.categoryName));
        }

        // Theme filter
        if (this.selectedThemes.size > 0) {
            parts = parts.filter(rp =>
                rp.themes?.some(t => this.selectedThemes.has(t.name))
            );
        }

        // Min sets filter
        if (this.minSets > 0) {
            parts = parts.filter(rp => rp.setCount >= this.minSets);
        }

        this._filteredParts = parts;
    }

    private updateQueryParams(): void {
        const qp: any = { mode: this.leaderboardMode };
        if (this.searchText) qp.q = this.searchText;
        if (this.selectedCategories.size > 0) qp.cats = [...this.selectedCategories].join(',');
        if (this.selectedThemes.size > 0) qp.themes = [...this.selectedThemes].join(',');
        if (this.minSets > 0) qp.minSets = this.minSets;
        this.router.navigate([], {
            relativeTo: this.route,
            queryParams: qp,
            replaceUrl: true
        });
    }

    navigateBack(): void {
        this.router.navigate(['/lego']);
    }

    openPartDetail(rp: RankedPart): void {
        if (rp.brickPartId) {
            this.router.navigate(['/parts', rp.brickPartId]);
        }
    }

    // ================================================================
    //  DATA LOADING — single API call to server-precomputed data
    // ================================================================

    private loadData(): void {
        this.loading = true;

        this.partsUniverseApi.getPayload()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (payload) => {
                    this.payload = payload;
                    this.rankedParts = payload.rankedParts;
                    this.totalParts = payload.stats.totalUniqueParts;
                    this.totalInstances = payload.stats.totalInstances;
                    this.totalSets = payload.stats.totalSets;
                    this.totalCategories = payload.stats.totalCategories;
                    this.loading = false;

                    // Build initial filtered set
                    this.rebuildFilteredParts();

                    // Request thumbnails for top 50
                    const top50 = this.rankedParts.slice(0, 50);
                    const thumbnailRequests = top50
                        .filter(r => r.geometryFilePath)
                        .map(r => ({
                            geometryFilePath: r.geometryFilePath,
                            colourHex: r.colours?.length > 0
                                ? (r.colours[0].hex.startsWith('#') ? r.colours[0].hex.slice(1) : r.colours[0].hex)
                                : undefined
                        }));
                    if (thumbnailRequests.length > 0) {
                        this.thumbnailService.renderBatch(thumbnailRequests);
                    }

                    // Render all visualizations after DOM updates
                    setTimeout(() => this.renderAllPanels(), 100);
                },
                error: (err) => {
                    console.error('[PartsUniverse] Failed to load data:', err);
                    this.loading = false;
                }
            });
    }

    // ================================================================
    //  RENDER ALL PANELS — rebuilds viz data from filtered parts
    // ================================================================

    private renderAllPanels(): void {
        if (!this.payload) return;

        const parts = this._filteredParts;

        // If no filters active, use server-precomputed data for best fidelity
        // (heatmap always rebuilt client-side so partLabels use ldrawTitle)
        if (!this.hasActiveFilters) {
            this.renderSankey(this.payload.sankey);
            this.renderBubbleChart(this.payload.bubbles);
            this.renderHeatmap(this.buildHeatmapData(this.rankedParts));
            this.renderChordDiagram(this.payload.chord);
            return;
        }

        // Otherwise, rebuild from filtered parts
        this.renderSankey(this.buildSankeyData(parts));
        this.renderBubbleChart(this.buildBubbleData(parts));
        this.renderHeatmap(this.buildHeatmapData(parts));
        this.renderChordDiagram(this.buildChordData(parts));
    }

    // ── Viz Data Builders ────────────────────────────────

    private buildSankeyData(parts: RankedPart[]): SankeyData {
        const top = parts.slice(0, 30);
        if (top.length === 0) return { nodes: [], links: [] };

        const nodeMap = new Map<string, number>();
        const nodes: SankeyNode[] = [];
        const linkMap = new Map<string, number>();

        const getIdx = (name: string, group: string) => {
            const key = `${group}:${name}`;
            if (!nodeMap.has(key)) {
                nodeMap.set(key, nodes.length);
                nodes.push({ name, group });
            }
            return nodeMap.get(key)!;
        };

        for (const rp of top) {
            const catIdx = getIdx(rp.categoryName, 'category');
            const partIdx = getIdx(rp.name, 'part');
            // category → part
            const cpKey = `${catIdx}-${partIdx}`;
            linkMap.set(cpKey, (linkMap.get(cpKey) || 0) + rp.totalQty);
            // part → themes
            for (const t of (rp.themes || []).slice(0, 3)) {
                const themeIdx = getIdx(t.name, 'theme');
                const ptKey = `${partIdx}-${themeIdx}`;
                linkMap.set(ptKey, (linkMap.get(ptKey) || 0) + t.qty);
            }
        }

        const links: SankeyLink[] = [];
        linkMap.forEach((value, key) => {
            const [s, t] = key.split('-').map(Number);
            links.push({ source: s, target: t, value });
        });

        return { nodes, links };
    }

    private buildBubbleData(parts: RankedPart[]): CategoryBubble[] {
        const catMap = new Map<string, BubblePart[]>();
        for (const rp of parts) {
            if (!catMap.has(rp.categoryName)) catMap.set(rp.categoryName, []);
            catMap.get(rp.categoryName)!.push({
                name: rp.name,
                totalQty: rp.totalQty,
                setCount: rp.setCount,
                dominantColourHex: rp.colours?.[0]?.hex ?? ''
            });
        }
        return [...catMap.entries()].map(([categoryName, bparts]) => ({
            categoryName,
            parts: bparts
        }));
    }

    private buildHeatmapData(parts: RankedPart[]): HeatmapData {
        const top = parts.slice(0, 25);
        if (top.length === 0) return { partLabels: [], colourLabels: [], cells: [] };

        // Collect all colours across top parts
        const colourSet = new Map<string, { hex: string; name: string }>();
        for (const rp of top) {
            for (const c of (rp.colours || [])) {
                const key = c.hex.replace('#', '').toLowerCase();
                if (!colourSet.has(key)) colourSet.set(key, { hex: c.hex, name: c.name });
            }
        }

        // Take top 30 colours by frequency
        const colourFreq = new Map<string, number>();
        for (const rp of top) {
            for (const c of (rp.colours || [])) {
                const key = c.hex.replace('#', '').toLowerCase();
                colourFreq.set(key, (colourFreq.get(key) || 0) + c.qty);
            }
        }
        const sortedColours = [...colourFreq.entries()]
            .sort((a, b) => b[1] - a[1])
            .slice(0, 30)
            .map(([key]) => key);

        const colourLabels: HeatmapColourLabel[] = sortedColours
            .map(key => colourSet.get(key)!)
            .filter(Boolean);

        const colourIdxMap = new Map(sortedColours.map((key, i) => [key, i]));
        const partLabels = top.map(rp => rp.ldrawTitle || rp.name);
        const cells: HeatmapCell[] = [];

        top.forEach((rp, partIdx) => {
            for (const c of (rp.colours || [])) {
                const key = c.hex.replace('#', '').toLowerCase();
                const colourIdx = colourIdxMap.get(key);
                if (colourIdx !== undefined) {
                    cells.push({ partIdx, colourIdx, hex: c.hex, qty: c.qty });
                }
            }
        });

        return { partLabels, colourLabels, cells };
    }

    private buildChordData(parts: RankedPart[]): ChordData {
        // Build category ↔ theme matrix
        const categories = [...new Set(parts.map(rp => rp.categoryName))].sort();
        const themes = new Set<string>();
        parts.forEach(rp => rp.themes?.forEach(t => themes.add(t.name)));
        const themeList = [...themes].sort().slice(0, 15);

        const names = [...categories, ...themeList];
        const n = names.length;
        if (n === 0) return { names: [], matrix: [], categoryCount: 0 };

        const matrix = Array.from({ length: n }, () => new Array(n).fill(0));
        const nameIdx = new Map(names.map((name, i) => [name, i]));

        for (const rp of parts) {
            const catIdx = nameIdx.get(rp.categoryName);
            if (catIdx === undefined) continue;
            for (const t of (rp.themes || [])) {
                const themeIdx = nameIdx.get(t.name);
                if (themeIdx === undefined) continue;
                matrix[catIdx][themeIdx] += t.qty;
                matrix[themeIdx][catIdx] += t.qty;
            }
        }

        return { names, matrix, categoryCount: categories.length };
    }

    // ================================================================
    //  PANEL 2: SANKEY DIAGRAM
    // ================================================================

    private renderSankey(data: SankeyData): void {
        if (!this.sankeyRef?.nativeElement) return;
        if (!data.nodes.length || !data.links.length) return;

        const container = this.sankeyRef.nativeElement as HTMLElement;
        container.innerHTML = '';

        const margin = { top: 20, right: 180, bottom: 20, left: 180 };
        const width = Math.max(container.clientWidth || 900, 700);
        const height = 600;

        const svg = d3.select(container)
            .append('svg')
            .attr('viewBox', `0 0 ${width} ${height}`)
            .attr('width', '100%')
            .attr('height', '100%');

        // Map group → layer for alignment
        const groupLayer: Record<string, number> = { category: 0, part: 1, theme: 2 };

        // Build Sankey-compatible nodes + links
        const nodes = data.nodes.map((n, i) => ({
            ...n, index: i, layer: groupLayer[n.group] ?? 1
        }));
        const links = data.links.map(l => ({ ...l }));

        // Create Sankey layout
        const sankeyLayout = d3Sankey<any, any>()
            .nodeId((d: any) => d.index)
            .nodeWidth(18)
            .nodePadding(14)
            .nodeAlign((node: any) => node.layer)
            .extent([[margin.left, margin.top], [width - margin.right, height - margin.bottom]]);

        const graph = sankeyLayout({ nodes, links });

        // Color scales
        const catColor = d3.scaleOrdinal(d3.schemeTableau10);
        const getNodeColor = (d: any) => {
            if (d.group === 'category') return catColor(d.name);
            if (d.group === 'part') return '#6c7ae0';
            return '#4ecdc4';
        };

        // Draw links
        svg.append('g')
            .attr('fill', 'none')
            .selectAll('path')
            .data(graph.links)
            .enter()
            .append('path')
            .attr('d', sankeyLinkHorizontal())
            .attr('stroke', (d: any) => getNodeColor(d.source))
            .attr('stroke-opacity', 0.35)
            .attr('stroke-width', (d: any) => Math.max(1, d.width))
            .on('mouseenter', function () {
                d3.select(this).attr('stroke-opacity', 0.7);
            })
            .on('mouseleave', function () {
                d3.select(this).attr('stroke-opacity', 0.35);
            })
            .append('title')
            .text((d: any) => `${d.source.name} → ${d.target.name}\n${d.value.toLocaleString()} instances`);

        // Draw nodes
        svg.append('g')
            .selectAll('rect')
            .data(graph.nodes)
            .enter()
            .append('rect')
            .attr('x', (d: any) => d.x0)
            .attr('y', (d: any) => d.y0)
            .attr('height', (d: any) => Math.max(1, d.y1 - d.y0))
            .attr('width', (d: any) => d.x1 - d.x0)
            .attr('fill', (d: any) => getNodeColor(d))
            .attr('rx', 3)
            .attr('opacity', 0.9)
            .append('title')
            .text((d: any) => `${d.name}\n${(d.value ?? 0).toLocaleString()} instances`);

        // Draw labels
        svg.append('g')
            .selectAll('text')
            .data(graph.nodes)
            .enter()
            .append('text')
            .attr('x', (d: any) => d.x0 < width / 2 ? d.x0 - 6 : d.x1 + 6)
            .attr('y', (d: any) => (d.y1 + d.y0) / 2)
            .attr('dy', '0.35em')
            .attr('text-anchor', (d: any) => d.x0 < width / 2 ? 'end' : 'start')
            .attr('fill', 'var(--bmc-text-primary)')
            .attr('font-size', '0.72rem')
            .attr('font-weight', (d: any) => d.group === 'part' ? '600' : '400')
            .text((d: any) => {
                const label = d.name;
                return label.length > 22 ? label.slice(0, 20) + '…' : label;
            });
    }

    // ================================================================
    //  PANEL 3: ZOOMABLE BUBBLE CHART
    // ================================================================

    private renderBubbleChart(bubbles: CategoryBubble[]): void {
        if (!this.bubbleRef?.nativeElement) return;
        if (!bubbles.length) return;

        const container = this.bubbleRef.nativeElement as HTMLElement;
        container.innerHTML = '';

        const size = Math.min(container.clientWidth || 600, 700);

        // Build D3 hierarchy from server-provided data
        const hierarchyData = {
            name: 'Parts',
            children: bubbles.map(cat => ({
                name: cat.categoryName,
                children: cat.parts.map(p => ({
                    name: p.name,
                    value: p.totalQty,
                    dominantColor: p.dominantColourHex ? ('#' + p.dominantColourHex.replace('#', '')) : '#888',
                    setCount: p.setCount
                }))
            }))
        };

        const root = d3.hierarchy(hierarchyData)
            .sum((d: any) => d.value || 0)
            .sort((a, b) => (b.value ?? 0) - (a.value ?? 0));

        const pack = d3.pack<any>()
            .size([size, size])
            .padding(3);

        const packed = pack(root) as any;

        const colorScale = d3.scaleOrdinal(d3.schemeTableau10);

        const svg = d3.select(container)
            .append('svg')
            .attr('viewBox', `0 0 ${size} ${size}`)
            .attr('width', '100%')
            .attr('height', '100%')
            .style('cursor', 'pointer');

        // Current focus
        let focus: any = packed;
        let view: [number, number, number] = [packed.x, packed.y, packed.r * 2];

        const zoom = (_event: any, d: any) => {
            focus = d;
            const transition = svg.transition().duration(500);
            const target: [number, number, number] = [focus.x, focus.y, focus.r * 2];

            node.select('circle')
                .transition(transition as any)
                .attr('r', (d: any) => d.r * (size / target[2]));

            node.transition(transition as any)
                .attr('transform', (dd: any) => {
                    const k = size / target[2];
                    return `translate(${(dd.x - target[0]) * k + size / 2},${(dd.y - target[1]) * k + size / 2})`;
                });

            labels.transition(transition as any)
                .style('fill-opacity', (dd: any) => dd.parent === focus ? 1 : 0)
                .style('display', (dd: any) => dd.parent === focus ? 'inline' : 'none');

            view = target;
        };

        svg.on('click', (event) => zoom(event, packed));

        const node = svg.selectAll('g.bubble-node')
            .data(packed.descendants().slice(1))
            .enter()
            .append('g')
            .attr('class', 'bubble-node')
            .attr('transform', (d: any) => {
                const k = size / view[2];
                return `translate(${(d.x - view[0]) * k + size / 2},${(d.y - view[1]) * k + size / 2})`;
            });

        node.append('circle')
            .attr('r', (d: any) => d.r * (size / view[2]))
            .attr('fill', (d: any) => {
                if (d.children) {
                    return colorScale(d.data.name);
                }
                const hex = d.data.dominantColor;
                return hex && hex !== '#888' ? hex : colorScale(d.parent?.data?.name ?? '');
            })
            .attr('fill-opacity', (d: any) => d.children ? 0.2 : 0.75)
            .attr('stroke', (d: any) => d.children ? colorScale(d.data.name) : 'rgba(255,255,255,0.3)')
            .attr('stroke-width', (d: any) => d.children ? 2 : 0.5)
            .on('click', (event: any, d: any) => {
                if (focus !== d && d.children) {
                    zoom(event, d);
                    event.stopPropagation();
                }
            })
            .append('title')
            .text((d: any) => {
                if (d.children) return `${d.data.name}\n${d.children.length} parts`;
                return `${d.data.name}\n${(d.value ?? 0).toLocaleString()} instances across ${d.data.setCount ?? 0} sets`;
            });

        // Labels
        const labels = svg.selectAll('text.bubble-label')
            .data(packed.descendants().filter((d: any) => !d.children && d.r > 12))
            .enter()
            .append('text')
            .attr('class', 'bubble-label')
            .attr('text-anchor', 'middle')
            .attr('dy', '0.3em')
            .attr('fill', 'var(--bmc-text-primary)')
            .attr('font-size', '0.55rem')
            .attr('pointer-events', 'none')
            .style('fill-opacity', (d: any) => d.parent === focus ? 1 : 0)
            .style('display', (d: any) => d.parent === focus ? 'inline' : 'none')
            .text((d: any) => {
                const name = d.data.name ?? '';
                return name.length > 14 ? name.slice(0, 12) + '…' : name;
            });

        labels.attr('x', (d: any) => {
            const k = size / view[2];
            return (d.x - view[0]) * k + size / 2;
        })
            .attr('y', (d: any) => {
                const k = size / view[2];
                return (d.y - view[1]) * k + size / 2;
            });
    }

    // ================================================================
    //  PANEL 4: COLOR HEATMAP
    // ================================================================

    private renderHeatmap(data: HeatmapData): void {
        if (!this.heatmapRef?.nativeElement) return;
        if (!data.partLabels.length || !data.colourLabels.length) return;

        const container = this.heatmapRef.nativeElement as HTMLElement;
        container.innerHTML = '';

        const cellSize = 20;
        const labelWidth = 140;
        const headerHeight = 50;
        const margin = { top: headerHeight, right: 20, bottom: 20, left: labelWidth };
        const width = margin.left + data.colourLabels.length * cellSize + margin.right;
        const height = margin.top + data.partLabels.length * cellSize + margin.bottom;

        const svg = d3.select(container)
            .append('svg')
            .attr('viewBox', `0 0 ${width} ${height}`)
            .attr('width', '100%')
            .attr('height', '100%');

        // Max qty for opacity scaling
        const maxQty = d3.max(data.cells, d => d.qty) ?? 1;

        // Draw color header swatches
        svg.selectAll('rect.color-header')
            .data(data.colourLabels)
            .enter()
            .append('rect')
            .attr('class', 'color-header')
            .attr('x', (_d, i) => margin.left + i * cellSize)
            .attr('y', margin.top - 18)
            .attr('width', cellSize - 2)
            .attr('height', 12)
            .attr('fill', d => d.hex.startsWith('#') ? d.hex : `#${d.hex}`)
            .attr('rx', 2)
            .attr('stroke', 'rgba(255,255,255,0.3)')
            .attr('stroke-width', 0.5)
            .append('title')
            .text(d => d.name);

        // Draw row labels (part names)
        svg.selectAll('text.part-label')
            .data(data.partLabels)
            .enter()
            .append('text')
            .attr('class', 'part-label')
            .attr('x', margin.left - 6)
            .attr('y', (_d, i) => margin.top + i * cellSize + cellSize / 2)
            .attr('dy', '0.35em')
            .attr('text-anchor', 'end')
            .attr('fill', 'var(--bmc-text-primary)')
            .attr('font-size', '0.5rem')
            .text(d => {
                const t = d.trim();
                return t.length > 20 ? t.slice(0, 18) + '…' : t;
            });

        // Draw heatmap cells
        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'heatmap-tooltip')
            .style('position', 'absolute')
            .style('pointer-events', 'none')
            .style('opacity', 0)
            .style('background', 'var(--bmc-bg-elevated)')
            .style('border', '1px solid var(--bmc-border)')
            .style('border-radius', '6px')
            .style('padding', '6px 10px')
            .style('font-size', '0.72rem')
            .style('color', 'var(--bmc-text-primary)')
            .style('z-index', '100');

        svg.selectAll('rect.heatmap-cell')
            .data(data.cells)
            .enter()
            .append('rect')
            .attr('class', 'heatmap-cell')
            .attr('x', d => margin.left + d.colourIdx * cellSize)
            .attr('y', d => margin.top + d.partIdx * cellSize)
            .attr('width', cellSize - 2)
            .attr('height', cellSize - 2)
            .attr('rx', 3)
            .attr('fill', d => d.hex.startsWith('#') ? d.hex : `#${d.hex}`)
            .attr('fill-opacity', d => 0.3 + 0.7 * (d.qty / maxQty))
            .attr('stroke', 'rgba(255,255,255,0.15)')
            .attr('stroke-width', 0.5)
            .on('mouseenter', function (_event: MouseEvent, d) {
                d3.select(this).attr('stroke', '#fff').attr('stroke-width', 2);
                const partName = data.partLabels[d.partIdx] ?? '';
                const colourName = data.colourLabels[d.colourIdx]?.name ?? '';
                tooltip
                    .style('opacity', 1)
                    .html(`<strong>${partName}</strong><br>${colourName}: ${d.qty.toLocaleString()} pcs`);
            })
            .on('mousemove', function (event: MouseEvent) {
                tooltip
                    .style('left', (event.offsetX + 12) + 'px')
                    .style('top', (event.offsetY - 10) + 'px');
            })
            .on('mouseleave', function () {
                d3.select(this).attr('stroke', 'rgba(255,255,255,0.15)').attr('stroke-width', 0.5);
                tooltip.style('opacity', 0);
            });

        // Rank numbers on left
        svg.selectAll('text.rank-num')
            .data(data.partLabels)
            .enter()
            .append('text')
            .attr('class', 'rank-num')
            .attr('x', 12)
            .attr('y', (_d, i) => margin.top + i * cellSize + cellSize / 2)
            .attr('dy', '0.35em')
            .attr('fill', 'var(--bmc-text-muted)')
            .attr('font-size', '0.48rem')
            .text((_d, i) => `#${i + 1}`);
    }

    // ================================================================
    //  PANEL 5: CHORD DIAGRAM
    // ================================================================

    private renderChordDiagram(data: ChordData): void {
        if (!this.chordRef?.nativeElement) return;
        if (!data.names.length || !data.matrix.length) return;

        const container = this.chordRef.nativeElement as HTMLElement;
        container.innerHTML = '';

        const names = data.names;
        const matrix = data.matrix;
        const catCount = data.categoryCount;

        const size = Math.min(container.clientWidth || 600, 600);
        const outerRadius = size / 2 - 80;
        const innerRadius = outerRadius - 20;

        const svg = d3.select(container)
            .append('svg')
            .attr('viewBox', `0 0 ${size} ${size}`)
            .attr('width', '100%')
            .attr('height', '100%')
            .append('g')
            .attr('transform', `translate(${size / 2},${size / 2})`);

        const chord = d3.chord()
            .padAngle(0.04)
            .sortSubgroups(d3.descending);

        const chords = chord(matrix);

        const arc = d3.arc<any>()
            .innerRadius(innerRadius)
            .outerRadius(outerRadius);

        const ribbon = d3.ribbon<any, any>()
            .radius(innerRadius);

        const color = d3.scaleOrdinal<string>()
            .domain(names)
            .range([...d3.schemeTableau10.slice(0, catCount),
            ...d3.schemePastel2.slice(0, names.length - catCount)]);

        // Draw ribbons
        svg.append('g')
            .selectAll('path')
            .data(chords)
            .enter()
            .append('path')
            .attr('d', ribbon)
            .attr('fill', (d: any) => color(names[d.source.index]))
            .attr('fill-opacity', 0.5)
            .attr('stroke', 'rgba(255,255,255,0.1)')
            .on('mouseenter', function () {
                d3.select(this).attr('fill-opacity', 0.85);
            })
            .on('mouseleave', function () {
                d3.select(this).attr('fill-opacity', 0.5);
            })
            .append('title')
            .text((d: any) => `${names[d.source.index]} ↔ ${names[d.target.index]}\n${d.source.value.toLocaleString()} parts`);

        // Draw arcs
        svg.append('g')
            .selectAll('path')
            .data(chords.groups)
            .enter()
            .append('path')
            .attr('d', arc)
            .attr('fill', (d: any) => color(names[d.index]))
            .attr('stroke', 'rgba(0,0,0,0.2)');

        // Draw labels
        svg.append('g')
            .selectAll('text')
            .data(chords.groups)
            .enter()
            .append('text')
            .each((d: any) => { d.angle = (d.startAngle + d.endAngle) / 2; })
            .attr('dy', '0.35em')
            .attr('transform', (d: any) =>
                `rotate(${(d.angle * 180 / Math.PI - 90)})` +
                `translate(${outerRadius + 10})` +
                (d.angle > Math.PI ? 'rotate(180)' : '')
            )
            .attr('text-anchor', (d: any) => d.angle > Math.PI ? 'end' : 'start')
            .attr('fill', 'var(--bmc-text-primary)')
            .attr('font-size', '0.65rem')
            .text((d: any) => {
                const name = names[d.index];
                return name.length > 18 ? name.slice(0, 16) + '…' : name;
            });
    }
}
