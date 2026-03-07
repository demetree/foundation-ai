import { Component, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, takeUntil } from 'rxjs';
import * as d3 from 'd3';

import { LegoMinifigService, LegoMinifigData } from '../../bmc-data-services/lego-minifig.service';
import { LegoSetMinifigData } from '../../bmc-data-services/lego-set-minifig.service';
import { LegoThemeService, LegoThemeData } from '../../bmc-data-services/lego-theme.service';
import { AuthService } from '../../services/auth.service';

interface GraphNode extends d3.SimulationNodeDatum {
    id: string;
    label: string;
    type: 'minifig' | 'set';
    imageUrl?: string | null;
    dataId: number;
}

interface GraphLink extends d3.SimulationLinkDatum<GraphNode> {
    qty: number;
}

@Component({
    selector: 'app-minifig-detail',
    templateUrl: './minifig-detail.component.html',
    styleUrl: './minifig-detail.component.scss'
})
export class MinifigDetailComponent implements OnInit, OnDestroy {
    @ViewChild('forceGraph', { static: false }) forceGraphRef!: ElementRef;

    minifig: LegoMinifigData | null = null;
    appearsIn: LegoSetMinifigData[] = [];
    themes: { id: number; name: string }[] = [];
    loading = true;
    setsLoading = true;

    // Lightweight set list for anonymous mode (populated from public API)
    publicSets: { legoSetId: number; setName: string; setNumber: string; year: number; imageUrl: string | null; quantity: number }[] = [];

    private destroy$ = new Subject<void>();

    constructor(
        private route: ActivatedRoute,
        public router: Router,
        private http: HttpClient,
        private minifigService: LegoMinifigService,
        private themeService: LegoThemeService,
        private authService: AuthService,
    ) { }

    ngOnInit(): void {
        this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
            const id = +params['id'];
            if (id) this.loadMinifig(id);
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadMinifig(id: number): void {
        this.loading = true;

        if (this.authService.isLoggedIn) {
            // Authenticated path — use generated data service
            this.minifigService.GetLegoMinifig(id, true).pipe(takeUntil(this.destroy$)).subscribe({
                next: (mf) => {
                    this.minifig = mf;
                    this.loading = false;
                    document.title = `${mf.name} — Minifig Detail`;
                    this.loadSets();
                },
                error: () => { this.loading = false; }
            });
        } else {
            // Anonymous path — use public API
            this.http.get<any>(`/api/public/browse/minifigs/${id}`).pipe(
                takeUntil(this.destroy$)
            ).subscribe({
                next: (result) => {
                    // Map the public DTO to a lightweight minifig object
                    this.minifig = {
                        id: result.minifig.id,
                        name: result.minifig.name,
                        figNumber: result.minifig.figNumber,
                        imageUrl: result.minifig.imageUrl,
                    } as any;
                    this.loading = false;
                    document.title = `${result.minifig.name} — Minifig Detail`;

                    // Store public sets and build the graph
                    this.publicSets = result.sets ?? [];
                    this.setsLoading = false;
                    setTimeout(() => this.renderForceGraph(), 150);
                },
                error: () => { this.loading = false; }
            });
        }
    }

    private async loadSets(): Promise<void> {
        if (!this.minifig) return;
        this.setsLoading = true;
        try {
            this.appearsIn = await this.minifig.LegoSetMinifigs;

            // Extract unique theme IDs from the loaded sets
            const themeIdSet = new Set<number>();
            for (const sm of this.appearsIn) {
                const tid = sm.legoSet?.legoThemeId;
                if (tid != null && Number(tid) > 0) {
                    themeIdSet.add(Number(tid));
                }
            }

            // Fetch theme names from the service
            if (themeIdSet.size > 0) {
                try {
                    const allThemes = await this.themeService.GetLegoThemeList(
                        { active: true, deleted: false }
                    ).toPromise();
                    const themeMap = new Map<number, string>();
                    for (const t of (allThemes ?? [])) {
                        if (themeIdSet.has(Number(t.id))) {
                            themeMap.set(Number(t.id), t.name);
                        }
                    }
                    this.themes = Array.from(themeMap, ([id, name]) => ({ id, name }))
                        .sort((a, b) => a.name.localeCompare(b.name));
                } catch {
                    this.themes = [];
                }
            }

            this.setsLoading = false;
            setTimeout(() => this.renderForceGraph(), 150);
        } catch {
            this.setsLoading = false;
        }
    }

    // ── D3 Force-Directed Graph ─────────────────────
    private renderForceGraph(): void {
        const hasAuthData = this.appearsIn.length > 0;
        const hasPublicData = this.publicSets.length > 0;
        if (!this.forceGraphRef || !this.minifig || (!hasAuthData && !hasPublicData)) return;
        const el = this.forceGraphRef.nativeElement;
        d3.select(el).selectAll('*').remove();

        const width = Math.min(el.clientWidth, 800);
        const height = 420;

        // Build nodes + links from whichever data source is available
        const nodes: GraphNode[] = [{
            id: `mf-${this.minifig.id}`,
            label: this.minifig.name,
            type: 'minifig',
            imageUrl: this.minifig.imageUrl,
            dataId: Number(this.minifig.id),
        }];

        const links: GraphLink[] = [];

        if (hasAuthData) {
            for (const sm of this.appearsIn) {
                if (!sm.legoSet) continue;
                const nodeId = `set-${sm.legoSet.id}`;
                if (!nodes.find(n => n.id === nodeId)) {
                    nodes.push({
                        id: nodeId,
                        label: sm.legoSet.name,
                        type: 'set',
                        imageUrl: sm.legoSet.imageUrl,
                        dataId: Number(sm.legoSet.id),
                    });
                }
                links.push({
                    source: `mf-${this.minifig!.id}`,
                    target: nodeId,
                    qty: Number(sm.quantity) || 1,
                });
            }
        } else {
            for (const s of this.publicSets) {
                const nodeId = `set-${s.legoSetId}`;
                if (!nodes.find(n => n.id === nodeId)) {
                    nodes.push({
                        id: nodeId,
                        label: s.setName ?? `Set ${s.setNumber}`,
                        type: 'set',
                        imageUrl: s.imageUrl,
                        dataId: s.legoSetId,
                    });
                }
                links.push({
                    source: `mf-${this.minifig!.id}`,
                    target: nodeId,
                    qty: Number(s.quantity) || 1,
                });
            }
        }

        const svg = d3.select(el)
            .append('svg')
            .attr('width', width)
            .attr('height', height)
            .attr('viewBox', `0 0 ${width} ${height}`);

        // Defs for glow + per-node clip paths
        const defs = svg.append('defs');
        const filter = defs.append('filter').attr('id', 'glow');
        filter.append('feGaussianBlur').attr('stdDeviation', '3').attr('result', 'coloredBlur');
        const feMerge = filter.append('feMerge');
        feMerge.append('feMergeNode').attr('in', 'coloredBlur');
        feMerge.append('feMergeNode').attr('in', 'SourceGraphic');

        // Clip paths for circular image thumbnails
        const mfRadius = 32;
        const setRadius = 26;
        nodes.forEach(n => {
            defs.append('clipPath')
                .attr('id', `clip-${n.id}`)
                .append('circle')
                .attr('r', n.type === 'minifig' ? mfRadius : setRadius)
                .attr('cx', 0).attr('cy', 0);
        });

        const simulation = d3.forceSimulation<GraphNode>(nodes)
            .force('link', d3.forceLink<GraphNode, GraphLink>(links).id(d => d.id).distance(140))
            .force('charge', d3.forceManyBody().strength(-350))
            .force('center', d3.forceCenter(width / 2, height / 2))
            .force('collision', d3.forceCollide(42));

        // Links
        const link = svg.append('g')
            .selectAll('line')
            .data(links)
            .enter()
            .append('line')
            .attr('stroke', 'var(--bmc-accent)')
            .attr('stroke-width', (d: GraphLink) => Math.max(2, Math.min(d.qty * 2, 5)))
            .attr('stroke-opacity', 0.45)
            .attr('stroke-dasharray', '6 3');

        // Node groups
        const node = svg.append('g')
            .selectAll<SVGGElement, GraphNode>('g')
            .data(nodes)
            .enter()
            .append('g')
            .style('cursor', 'pointer')
            .call(d3.drag<SVGGElement, GraphNode>()
                .on('start', (event, d) => {
                    if (!event.active) simulation.alphaTarget(0.3).restart();
                    d.fx = d.x; d.fy = d.y;
                })
                .on('drag', (event, d) => {
                    d.fx = event.x; d.fy = event.y;
                })
                .on('end', (event, d) => {
                    if (!event.active) simulation.alphaTarget(0);
                    d.fx = null; d.fy = null;
                })
            );

        // Background circle (border ring + fallback fill)
        node.append('circle')
            .attr('r', d => d.type === 'minifig' ? mfRadius + 2 : setRadius + 2)
            .attr('fill', d => d.imageUrl ? 'rgba(0,0,0,0.25)' : (d.type === 'minifig' ? 'var(--bmc-accent)' : 'rgba(255,255,255,0.08)'))
            .attr('stroke', d => d.type === 'minifig' ? 'var(--bmc-accent)' : 'var(--bmc-glass-border)')
            .attr('stroke-width', 2.5)
            .attr('filter', d => d.type === 'minifig' ? 'url(#glow)' : '');

        // Image thumbnails (clipped to circle)
        node.filter(d => !!d.imageUrl)
            .append('image')
            .attr('href', d => d.imageUrl!)
            .attr('x', d => -(d.type === 'minifig' ? mfRadius : setRadius))
            .attr('y', d => -(d.type === 'minifig' ? mfRadius : setRadius))
            .attr('width', d => (d.type === 'minifig' ? mfRadius : setRadius) * 2)
            .attr('height', d => (d.type === 'minifig' ? mfRadius : setRadius) * 2)
            .attr('clip-path', d => `url(#clip-${d.id})`)
            .attr('preserveAspectRatio', 'xMidYMid slice');

        // Fallback icons for nodes without images
        node.filter(d => !d.imageUrl)
            .append('text')
            .attr('text-anchor', 'middle')
            .attr('dy', '0.35em')
            .attr('fill', d => d.type === 'minifig' ? '#fff' : 'var(--bmc-text-secondary)')
            .style('font-family', '"Font Awesome 5 Free"')
            .style('font-weight', '900')
            .style('font-size', d => d.type === 'minifig' ? '1rem' : '0.8rem')
            .text(d => d.type === 'minifig' ? '\uf007' : '\uf1b2');

        // Labels below nodes
        node.append('text')
            .attr('text-anchor', 'middle')
            .attr('dy', d => d.type === 'minifig' ? mfRadius + 14 : setRadius + 12)
            .attr('fill', 'var(--bmc-text-secondary)')
            .style('font-size', '0.65rem')
            .style('pointer-events', 'none')
            .text(d => d.label.length > 22 ? d.label.slice(0, 20) + '…' : d.label);

        // Click handler
        const router = this.router;
        node.on('click', (_event: any, d: GraphNode) => {
            if (d.type === 'set') {
                router.navigate(['/lego/sets', d.dataId]);
            }
        });

        simulation.on('tick', () => {
            link
                .attr('x1', (d: any) => d.source.x)
                .attr('y1', (d: any) => d.source.y)
                .attr('x2', (d: any) => d.target.x)
                .attr('y2', (d: any) => d.target.y);

            node.attr('transform', (d: GraphNode) => `translate(${d.x},${d.y})`);
        });
    }

    // ── Navigation ──────────────────────────────────
    goBack(): void {
        this.router.navigate(['/lego/minifigs']);
    }

    /** Returns the set count from whichever data source is populated */
    get setCount(): number {
        return this.appearsIn.length > 0 ? this.appearsIn.length : this.publicSets.length;
    }

    openSet(sm: LegoSetMinifigData): void {
        if (sm.legoSet) {
            this.router.navigate(['/lego/sets', sm.legoSet.id]);
        }
    }

    openPublicSet(s: { legoSetId: number }): void {
        this.router.navigate(['/lego/sets', s.legoSetId]);
    }
}
