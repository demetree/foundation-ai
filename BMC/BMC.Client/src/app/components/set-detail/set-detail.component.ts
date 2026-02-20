import { Component, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import * as d3 from 'd3';

import { LegoSetService, LegoSetData } from '../../bmc-data-services/lego-set.service';
import { LegoSetPartData } from '../../bmc-data-services/lego-set-part.service';
import { LegoSetMinifigData } from '../../bmc-data-services/lego-set-minifig.service';
import { LegoSetSubsetData } from '../../bmc-data-services/lego-set-subset.service';

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
    loading = true;
    partsLoading = true;
    minifigsLoading = true;
    subsetsLoading = true;
    activeTab: 'parts' | 'minifigs' | 'subsets' = 'parts';

    private destroy$ = new Subject<void>();

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private legoSetService: LegoSetService,
    ) { }

    ngOnInit(): void {
        this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
            const id = +params['id'];
            if (id) this.loadSet(id);
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
                this.loadRelatedData();
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
            this.minifigsLoading = false;
        } catch {
            this.minifigsLoading = false;
        }

        // Load subsets
        this.subsetsLoading = true;
        try {
            this.subsets = await this.set.LegoSetSubsetChildLegoSets;
            this.subsetsLoading = false;
        } catch {
            this.subsetsLoading = false;
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

    openExternal(url: string | null): void {
        if (url) window.open(url, '_blank');
    }

    setTab(tab: 'parts' | 'minifigs' | 'subsets'): void {
        this.activeTab = tab;
    }
}
