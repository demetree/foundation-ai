import { Component, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, takeUntil } from 'rxjs';
import * as d3 from 'd3';

import { LegoSetService, LegoSetData } from '../../bmc-data-services/lego-set.service';
import { LegoSetPartData } from '../../bmc-data-services/lego-set-part.service';
import { LegoSetMinifigData } from '../../bmc-data-services/lego-set-minifig.service';
import { LegoSetSubsetData } from '../../bmc-data-services/lego-set-subset.service';
import { LDrawThumbnailService } from '../../services/ldraw-thumbnail.service';
import { SetExplorerApiService, SetExplorerItem } from '../../services/set-explorer-api.service';
import { SetComparisonService } from '../../services/set-comparison.service';
import { SetOwnershipCacheService } from '../../services/set-ownership-cache.service';
import { BrickSetSyncService } from '../../services/brickset-sync.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';

@Component({
    selector: 'app-set-detail',
    templateUrl: './set-detail.component.html',
    styleUrl: './set-detail.component.scss'
})
export class SetDetailComponent implements OnInit, OnDestroy {
    @ViewChild('colourDonut', { static: false }) colourDonutRef!: ElementRef;
    @ViewChild('categoryBar', { static: false }) categoryBarRef!: ElementRef;

    set: LegoSetData | null = null;
    parts: LegoSetPartData[] = [];
    minifigs: LegoSetMinifigData[] = [];
    subsets: LegoSetSubsetData[] = [];
    parentSets: LegoSetSubsetData[] = [];
    loading = true;
    partsLoading = true;
    minifigsLoading = true;
    subsetsLoading = true;
    parentSetsLoading = true;
    activeTab: 'parts' | 'minifigs' | 'related' = 'parts';
    thumbnails = new Map<string, string>();
    selectedColourFilter: string | null = null;
    selectedCategoryFilter: string | null = null;
    similarSets: SetExplorerItem[] = [];
    similarSetsLoading = false;
    enriching = false;

    // ── Brickberg Terminal state ──
    brickbergData: any = null;
    brickbergLoading = false;

    get isInComparison(): boolean {
        return this.set ? this.comparisonService.isInComparison(Number(this.set.id)) : false;
    }

    get isOwned(): boolean {
        return this.set ? this.ownershipCache.isOwned(Number(this.set.id)) : false;
    }

    get isWanted(): boolean {
        return this.set ? this.ownershipCache.isWanted(Number(this.set.id)) : false;
    }

    get filteredParts(): LegoSetPartData[] {
        let result = this.parts;
        if (this.selectedColourFilter) {
            result = result.filter(p => (p.brickColour?.name ?? 'Unknown') === this.selectedColourFilter);
        }
        if (this.selectedCategoryFilter) {
            result = result.filter(p => {
                const cat = p.brickPart?.brickCategory?.name ?? p.brickPart?.ldrawCategory ?? 'Other';
                return cat === this.selectedCategoryFilter;
            });
        }
        return result;
    }

    private destroy$ = new Subject<void>();

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private http: HttpClient,
        private legoSetService: LegoSetService,
        private thumbnailService: LDrawThumbnailService,
        private explorerApi: SetExplorerApiService,
        public comparisonService: SetComparisonService,
        public ownershipCache: SetOwnershipCacheService,
        private bsSyncService: BrickSetSyncService,
        private alertService: AlertService,
    ) {
        this.ownershipCache.ensureLoaded();
    }

    ngOnInit(): void {
        this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
            const id = +params['id'];
            if (id) this.loadSet(id);
        });

        // Subscribe to 3D thumbnail render results
        this.thumbnailService.thumbnail$.pipe(
            takeUntil(this.destroy$)
        ).subscribe(result => {
            this.thumbnails.set(result.cacheKey, result.dataUrl);
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadSet(id: number): void {
        this.loading = true;
        this.legoSetService.GetLegoSet(id, true).pipe(takeUntil(this.destroy$)).subscribe({
            next: (set) => {
                this.set = set;
                this.loading = false;
                document.title = `${set.name} (${set.setNumber}) — Set Detail`;
                this.loadRelatedData();
                this.loadSimilarSets();
                this.loadBrickbergData();
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    private async loadRelatedData(): Promise<void> {
        if (!this.set) return;

        // Load parts
        this.partsLoading = true;
        try {
            this.parts = await this.set.LegoSetParts;
            this.partsLoading = false;
            this.renderPartThumbnails();
            setTimeout(() => {
                this.renderColourDonut();
                this.renderCategoryBar();
            }, 100);
        } catch {
            this.partsLoading = false;
        }

        // Load minifigs
        this.minifigsLoading = true;
        try {
            this.minifigs = await this.set.LegoSetMinifigs;
            console.log(`[set-detail] minifigs loaded: ${this.minifigs.length} records for setId=${this.set.id}`, this.minifigs);
            this.minifigsLoading = false;
        } catch (err) {
            console.error('[set-detail] minifigs load error:', err);
            this.minifigsLoading = false;
        }

        // Load subsets (children of this set)
        this.subsetsLoading = true;
        try {
            this.subsets = await this.set.LegoSetSubsetParentLegoSets;
            this.subsetsLoading = false;
        } catch {
            this.subsetsLoading = false;
        }

        // Load parent sets (sets that contain this set)
        this.parentSetsLoading = true;
        try {
            this.parentSets = await this.set.LegoSetSubsetChildLegoSets;
            this.parentSetsLoading = false;
        } catch {
            this.parentSetsLoading = false;
        }
    }

    // ── D3 Colour Donut ──────────────────────────────────
    private renderColourDonut(): void {
        if (!this.colourDonutRef || this.parts.length === 0) return;
        const el = this.colourDonutRef.nativeElement;
        d3.select(el).selectAll('*').remove();

        // Aggregate by colour
        const colourMap = new Map<string, { name: string; hex: string; qty: number }>();
        for (const p of this.parts) {
            const colourName = p.brickColour?.name ?? 'Unknown';
            // Defensively handle hex — strip any existing # prefix before adding one
            let rawHex = p.brickColour?.hexRgb ?? '';
            if (rawHex.startsWith('#')) rawHex = rawHex.substring(1);
            const hex = rawHex.length >= 3 ? `#${rawHex}` : '#888888';
            const qty = Number(p.quantity) || 1;
            const existing = colourMap.get(colourName);
            if (existing) {
                existing.qty += qty;
            } else {
                colourMap.set(colourName, { name: colourName, hex, qty });
            }
        }

        const data = Array.from(colourMap.values()).sort((a, b) => b.qty - a.qty);
        const width = Math.min(el.clientWidth, 380);
        const height = width;
        const radius = width / 2;


        const svg = d3.select(el)
            .append('svg')
            .attr('width', width)
            .attr('height', height)
            .append('g')
            .attr('transform', `translate(${width / 2},${height / 2})`);

        const pie = d3.pie<{ name: string; hex: string; qty: number }>()
            .value(d => d.qty)
            .sort(null);

        const arc = d3.arc<d3.PieArcDatum<{ name: string; hex: string; qty: number }>>()
            .innerRadius(radius * 0.55)
            .outerRadius(radius * 0.85);

        const hoverArc = d3.arc<d3.PieArcDatum<{ name: string; hex: string; qty: number }>>()
            .innerRadius(radius * 0.52)
            .outerRadius(radius * 0.9);

        // Tooltip
        const tooltip = d3.select(el)
            .append('div')
            .attr('class', 'chart-tooltip')
            .style('opacity', 0);

        // Slices — use actual brick colour hex for each segment
        svg.selectAll('path')
            .data(pie(data))
            .enter()
            .append('path')
            .attr('d', arc as any)
            .attr('fill', d => d.data.hex)
            .attr('stroke', '#ffffff')
            .attr('stroke-width', 2.5)
            .style('cursor', 'pointer')
            .style('opacity', 1)
            .on('click', (_event: any, d: any) => {
                if (this.selectedColourFilter === d.data.name) {
                    this.selectedColourFilter = null;
                } else {
                    this.selectedColourFilter = d.data.name;
                    this.activeTab = 'parts';
                }
                // Update slice opacity to show selection
                svg.selectAll('path')
                    .transition().duration(200)
                    .style('opacity', (s: any) => {
                        if (!this.selectedColourFilter) return 1;
                        return s.data.name === this.selectedColourFilter ? 1 : 0.25;
                    });
            })
            .on('mouseover', function (event: any, d: any) {
                d3.select(this).transition().duration(150).attr('d', hoverArc as any);
                tooltip.transition().duration(150).style('opacity', 1);
                tooltip.html(`
                    <span class="swatch" style="background:${d.data.hex}; border: 1px solid rgba(255,255,255,0.4);"></span>
                    <strong>${d.data.name}</strong><br>
                    ${d.data.qty} pieces
                `).style('left', `${event.offsetX + 12}px`).style('top', `${event.offsetY - 24}px`);
            })
            .on('mouseout', function () {
                d3.select(this).transition().duration(150).attr('d', arc as any);
                tooltip.transition().duration(150).style('opacity', 0);
            });

        // Centre label
        svg.append('text')
            .attr('text-anchor', 'middle')
            .attr('dy', '-0.3em')
            .attr('fill', 'var(--bmc-text-primary)')
            .style('font-size', '1.6rem')
            .style('font-weight', '700')
            .text(data.length.toString());

        svg.append('text')
            .attr('text-anchor', 'middle')
            .attr('dy', '1.2em')
            .attr('fill', 'var(--bmc-text-secondary)')
            .style('font-size', '0.75rem')
            .text('COLOURS');
    }

    // ── D3 Category Bar Chart ────────────────────────────
    private renderCategoryBar(): void {
        if (!this.categoryBarRef || this.parts.length === 0) return;
        const el = this.categoryBarRef.nativeElement;
        d3.select(el).selectAll('*').remove();

        // Aggregate by category
        const catMap = new Map<string, number>();
        for (const p of this.parts) {
            const catName = p.brickPart?.brickCategory?.name ?? p.brickPart?.ldrawCategory ?? 'Other';
            const qty = Number(p.quantity) || 1;
            catMap.set(catName, (catMap.get(catName) ?? 0) + qty);
        }

        const data = Array.from(catMap.entries())
            .map(([name, count]) => ({ name, count }))
            .sort((a, b) => b.count - a.count)
            .slice(0, 15); // top 15

        const margin = { top: 10, right: 20, bottom: 80, left: 50 };
        const width = Math.min(el.clientWidth, 600) - margin.left - margin.right;
        const height = 280 - margin.top - margin.bottom;

        const svg = d3.select(el)
            .append('svg')
            .attr('width', width + margin.left + margin.right)
            .attr('height', height + margin.top + margin.bottom)
            .append('g')
            .attr('transform', `translate(${margin.left},${margin.top})`);

        const x = d3.scaleBand().domain(data.map(d => d.name)).range([0, width]).padding(0.3);
        const y = d3.scaleLinear().domain([0, d3.max(data, d => d.count) ?? 1]).nice().range([height, 0]);

        // Vibrant colour palette for bars
        const barPalette = [
            '#4F8FF7', '#6C5CE7', '#00B894', '#FDCB6E', '#E17055',
            '#00CEC9', '#E84393', '#0984E3', '#55EFC4', '#FAB1A0',
            '#74B9FF', '#A29BFE', '#FD79A8', '#81ECEC', '#DFE6E9'
        ];

        // Bars
        svg.selectAll('rect')
            .data(data)
            .enter()
            .append('rect')
            .attr('x', d => x(d.name)!)
            .attr('width', x.bandwidth())
            .attr('y', height)
            .attr('height', 0)
            .attr('rx', 4)
            .attr('fill', (_, i) => barPalette[i % barPalette.length])
            .attr('opacity', 0.9)
            .style('cursor', 'pointer')
            .on('click', (_event: any, d: any) => {
                if (this.selectedCategoryFilter === d.name) {
                    this.selectedCategoryFilter = null;
                } else {
                    this.selectedCategoryFilter = d.name;
                    this.activeTab = 'parts';
                }
                // Update bar opacity to show selection
                svg.selectAll('rect')
                    .transition().duration(200)
                    .attr('opacity', (s: any) => {
                        if (!this.selectedCategoryFilter) return 0.9;
                        return s.name === this.selectedCategoryFilter ? 0.9 : 0.2;
                    });
            })
            .transition()
            .duration(600)
            .delay((_, i) => i * 40)
            .attr('y', d => y(d.count))
            .attr('height', d => height - y(d.count));

        // X axis
        svg.append('g')
            .attr('transform', `translate(0,${height})`)
            .call(d3.axisBottom(x))
            .selectAll('text')
            .attr('transform', 'rotate(-40)')
            .style('text-anchor', 'end')
            .style('fill', 'var(--bmc-text-secondary)')
            .style('font-size', '0.65rem');

        svg.selectAll('.domain, .tick line').attr('stroke', 'var(--bmc-glass-border)');

        // Y axis
        svg.append('g')
            .call(d3.axisLeft(y).ticks(5))
            .selectAll('text')
            .style('fill', 'var(--bmc-text-secondary)')
            .style('font-size', '0.7rem');
    }

    // ── Navigation helpers ───────────────────────────────
    goBack(): void {
        this.router.navigate(['/lego/sets']);
    }

    openMinifig(minifig: LegoSetMinifigData): void {
        if (minifig.legoMinifig) {
            this.router.navigate(['/lego/minifigs', minifig.legoMinifig.id]);
        }
    }

    openSubset(subset: LegoSetSubsetData): void {
        if (subset.childLegoSet) {
            this.router.navigate(['/lego/sets', subset.childLegoSet.id]);
        }
    }

    openParentSet(subset: LegoSetSubsetData): void {
        if (subset.parentLegoSet) {
            this.router.navigate(['/lego/sets', subset.parentLegoSet.id]);
        }
    }

    openExternal(url: string | null): void {
        if (url) window.open(url, '_blank');
    }

    openInstructions(): void {
        if (!this.set) return;
        // Strip variant suffix (e.g. '42131-1' → '42131') for cleaner LEGO search
        const baseNumber = (this.set.setNumber ?? '').replace(/-\d+$/, '');
        if (baseNumber) {
            window.open(`https://www.lego.com/en-us/service/buildinginstructions/${baseNumber}`, '_blank');
        }
    }

    openSimilarSet(set: SetExplorerItem): void {
        this.router.navigate(['/lego/sets', set.id]);
    }

    toggleCompare(): void {
        if (!this.set) return;
        const item: SetExplorerItem = {
            id: Number(this.set.id),
            name: this.set.name,
            setNumber: this.set.setNumber,
            year: Number(this.set.year),
            partCount: Number(this.set.partCount),
            imageUrl: this.set.imageUrl ?? null,
            themeId: this.set.legoThemeId ? Number(this.set.legoThemeId) : null,
            themeName: this.set.legoTheme?.name ?? null
        };
        this.comparisonService.toggleSet(item);
    }

    goToCompare(): void {
        this.router.navigate(['/lego/compare']);
    }

    async toggleOwn(): Promise<void> {
        if (!this.set) return;
        await this.ownershipCache.toggleOwnership(Number(this.set.id), 'owned');
    }

    async toggleWant(): Promise<void> {
        if (!this.set) return;
        await this.ownershipCache.toggleOwnership(Number(this.set.id), 'wanted');
    }


    triggerEnrich(): void {
        if (!this.set?.setNumber) return;
        this.enriching = true;
        this.bsSyncService.enrichSet(this.set.setNumber).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                this.enriching = false;
                if (result.enriched) {
                    this.alertService.showMessage('Enriched', `${result.setNumber} updated from BrickSet`, MessageSeverity.success);
                    // Reload the set to pick up enriched fields
                    this.loadSet(Number(this.set!.id));
                } else {
                    this.alertService.showMessage('Not Enriched', `No BrickSet data found for ${result.setNumber}`, MessageSeverity.warn);
                }
            },
            error: () => {
                this.enriching = false;
                this.alertService.showMessage('Error', 'Enrichment failed. Check BrickSet connection.', MessageSeverity.error);
            }
        });
    }

    // ── Similar Sets recommendation engine ───────────────
    private loadSimilarSets(): void {
        if (!this.set) return;
        this.similarSetsLoading = true;
        this.similarSets = [];

        const currentId = Number(this.set.id);
        const currentThemeId = Number(this.set.legoThemeId);
        const currentYear = Number(this.set.year);
        const currentParts = Number(this.set.partCount);
        const logParts = Math.log10(Math.max(currentParts, 1));

        this.explorerApi.getExploreSets().pipe(takeUntil(this.destroy$)).subscribe({
            next: (allSets) => {
                const scored = allSets
                    .filter(s => s.id !== currentId)
                    .map(s => {
                        let score = 0;

                        // Theme match — 40 points
                        if (s.themeId != null && s.themeId === currentThemeId) {
                            score += 40;
                        }

                        // Year proximity — up to 30 points
                        const yearDiff = Math.abs(s.year - currentYear);
                        score += Math.max(0, 30 - yearDiff * 3);

                        // Part count proximity (log scale) — up to 20 points
                        const logS = Math.log10(Math.max(s.partCount, 1));
                        const countDiff = Math.abs(logS - logParts);
                        score += Math.max(0, 20 - countDiff * 10);

                        // Has image — 10 points
                        if (s.imageUrl) score += 10;

                        return { set: s, score };
                    })
                    .sort((a, b) => b.score - a.score)
                    .slice(0, 12)
                    .map(x => x.set);

                this.similarSets = scored;
                this.similarSetsLoading = false;
            },
            error: () => {
                this.similarSetsLoading = false;
            }
        });
    }

    printPartsList(): void {
        this.activeTab = 'parts';
        setTimeout(() => window.print(), 200);
    }

    openInCatalog(part: LegoSetPartData): void {
        if (part.brickPartId) {
            const queryParams: any = {};

            //
            // Pass the colour ID if it's a real value (not 0 / unset).
            // brickColourId is bigint|number — use Number() so 0n is caught too.
            //
            if (Number(part.brickColourId) > 0) {
                queryParams.colourId = part.brickColourId;
            }

            //
            // Also pass the hex from the navigation property as a fallback.
            // The detail page will match by hex if colourId doesn't match.
            //
            if (part.brickColour?.hexRgb) {
                const raw = part.brickColour.hexRgb.replace('#', '');
                if (raw.length >= 3) {
                    queryParams.hex = raw;
                }
            }

            this.router.navigate(['/parts', part.brickPartId],
                Object.keys(queryParams).length > 0 ? { queryParams } : {}
            );
        }
    }

    getPartColourHex(p: LegoSetPartData): string {
        let rawHex = p.brickColour?.hexRgb ?? '';
        if (rawHex.startsWith('#')) rawHex = rawHex.substring(1);
        return rawHex.length >= 3 ? `#${rawHex}` : '#888888';
    }

    clearColourFilter(): void {
        this.selectedColourFilter = null;
        if (this.colourDonutRef) {
            d3.select(this.colourDonutRef.nativeElement)
                .selectAll('path')
                .transition().duration(200)
                .style('opacity', 1);
        }
    }

    clearCategoryFilter(): void {
        this.selectedCategoryFilter = null;
        if (this.categoryBarRef) {
            d3.select(this.categoryBarRef.nativeElement)
                .selectAll('rect')
                .transition().duration(200)
                .attr('opacity', 0.9);
        }
    }

    clearAllFilters(): void {
        this.clearColourFilter();
        this.clearCategoryFilter();
    }

    setTab(tab: 'parts' | 'minifigs' | 'related'): void {
        this.activeTab = tab;
    }

    private renderPartThumbnails(): void {
        const requests = this.parts
            .filter(p => p.brickPart?.geometryOriginalFileName)
            .map(p => ({
                geometryOriginalFileName: p.brickPart!.geometryOriginalFileName!,
                colourHex: this.normalizeHex(p.brickColour?.hexRgb)
            }));
        if (requests.length > 0) {
            this.thumbnailService.renderBatch(requests);
        }
    }

    getPartThumbnailKey(p: LegoSetPartData): string {
        return LDrawThumbnailService.cacheKey(
            p.brickPart?.geometryOriginalFileName ?? '',
            this.normalizeHex(p.brickColour?.hexRgb)
        );
    }

    private normalizeHex(hex: string | null | undefined): string | undefined {
        if (!hex) return undefined;
        return hex.startsWith('#') ? hex.substring(1) : hex;
    }


    // ── Brickberg Terminal ────────────────────────────────

    private loadBrickbergData(): void {
        if (!this.set?.setNumber) return;
        this.brickbergLoading = true;
        this.brickbergData = null;

        this.http.get(`/api/brickberg/set/${this.set.setNumber}`).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (data) => {
                this.brickbergData = data;
                this.brickbergLoading = false;
            },
            error: () => {
                this.brickbergLoading = false;
            }
        });
    }

    /** Investment grade: 1–5 stars based on growth % */
    getInvestmentGrade(): number {
        const growth = this.brickbergData?.valuation?.growthPercentage ?? 0;
        if (growth >= 50) return 5;
        if (growth >= 25) return 4;
        if (growth >= 10) return 3;
        if (growth >= 0) return 2;
        return 1;
    }

    /** Price per part using BrickEconomy valuation */
    getPricePerPart(): number | null {
        const value = this.brickbergData?.valuation?.currentValue;
        const parts = Number(this.set?.partCount);
        if (value && parts > 0) return value / parts;
        return null;
    }

    /** Find the lowest price across all sources */
    getBestPrice(): { source: string; price: number; condition: string } | null {
        const candidates: { source: string; price: number; condition: string }[] = [];

        const blNewMin = this.brickbergData?.priceGuideNew?.min_price;
        const blUsedMin = this.brickbergData?.priceGuideUsed?.min_price;

        if (blNewMin > 0) candidates.push({ source: 'BrickLink', price: blNewMin, condition: 'New' });
        if (blUsedMin > 0) candidates.push({ source: 'BrickLink', price: blUsedMin, condition: 'Used' });

        if (candidates.length === 0) return null;
        return candidates.sort((a, b) => a.price - b.price)[0];
    }

    /** Check if any Brickberg source is connected */
    hasAnyBrickbergSource(): boolean {
        if (!this.brickbergData) return false;
        return this.brickbergData.brickLink?.connected ||
            this.brickbergData.brickEconomy?.connected ||
            this.brickbergData.brickOwl?.connected;
    }
}
