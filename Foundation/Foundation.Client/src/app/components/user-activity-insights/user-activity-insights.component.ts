//
// User Activity Insights Component
//
// Premium D3.js-powered analytics dashboard for audit event visualization.
// Provides heatmap calendar, radial clock, module bar chart, event type donut,
// user→module Sankey diagram, and session timeline.
//
// AI-assisted development - February 2026
//
import { Component, OnInit, OnDestroy, ViewEncapsulation, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { Location } from '@angular/common';
import { BehaviorSubject, Subject, interval, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AlertService, MessageSeverity } from '../../services/alert.service';
import { AuthService } from '../../services/auth.service';
import {
    UserActivityInsightsService,
    UserActivityInsightsResponse,
    HourlyActivity,
    DailyActivity,
    TopUser,
    TopModule,
    EventTypeBreakdown,
    UserModuleLink,
    RecentSession,
    FailureHotspot
} from '../../services/user-activity-insights.service';

import * as d3 from 'd3';
import { sankey as d3Sankey, sankeyLinkHorizontal, SankeyNode, SankeyLink } from 'd3-sankey';


//
// Time preset options (consistent with existing audit-event-custom-listing pattern)
//
interface TimePreset {
    label: string;
    value: string;
    getStartDate: () => Date;
}


@Component({
    selector: 'app-user-activity-insights',
    templateUrl: './user-activity-insights.component.html',
    styleUrls: ['./user-activity-insights.component.scss'],
    encapsulation: ViewEncapsulation.None     // needed for D3-generated DOM elements
})
export class UserActivityInsightsComponent implements OnInit, OnDestroy, AfterViewInit {

    private destroy$ = new Subject<void>();

    isLoading$ = new BehaviorSubject<boolean>(true);
    data: UserActivityInsightsResponse | null = null;

    //
    // Filter state
    //
    selectedTimePreset: string = '30d';
    autoRefreshEnabled: boolean = false;
    autoRefreshInterval: number = 60;
    private autoRefreshSubscription: Subscription | null = null;

    timePresets: TimePreset[] = [
        { label: 'Last 24 Hours', value: '24h', getStartDate: () => { const d = new Date(); d.setHours(d.getHours() - 24); return d; } },
        { label: 'Last 7 Days', value: '7d', getStartDate: () => { const d = new Date(); d.setDate(d.getDate() - 7); return d; } },
        { label: 'Last 30 Days', value: '30d', getStartDate: () => { const d = new Date(); d.setDate(d.getDate() - 30); return d; } },
        { label: 'Last 90 Days', value: '90d', getStartDate: () => { const d = new Date(); d.setDate(d.getDate() - 90); return d; } },
        { label: 'Last 365 Days', value: '365d', getStartDate: () => { const d = new Date(); d.setDate(d.getDate() - 365); return d; } },
    ];

    //
    // D3 chart containers — referenced via ViewChild
    //
    @ViewChild('heatmapChart', { static: false }) heatmapChartRef!: ElementRef;
    @ViewChild('radialClockChart', { static: false }) radialClockChartRef!: ElementRef;
    @ViewChild('moduleBarChart', { static: false }) moduleBarChartRef!: ElementRef;
    @ViewChild('donutChart', { static: false }) donutChartRef!: ElementRef;
    @ViewChild('sankeyChart', { static: false }) sankeyChartRef!: ElementRef;

    private chartsRendered = false;


    constructor(
        private location: Location,
        private insightsService: UserActivityInsightsService,
        private alertService: AlertService,
        private authService: AuthService
    ) { }


    ngOnInit(): void {
        this.loadData();
    }

    ngAfterViewInit(): void {
        // Charts will render after data loads
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.stopAutoRefresh();
    }


    //
    // Data loading
    //
    loadData(): void {
        this.isLoading$.next(true);

        const preset = this.timePresets.find(p => p.value === this.selectedTimePreset);
        const startDate = preset ? preset.getStartDate() : new Date(Date.now() - 30 * 24 * 60 * 60 * 1000);

        this.insightsService.getInsights(startDate, new Date())
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    this.data = response;
                    this.isLoading$.next(false);

                    // Render D3 charts after data arrives and DOM is ready
                    setTimeout(() => this.renderAllCharts(), 100);
                },
                error: (err) => {
                    this.alertService.showMessage('Error', 'Failed to load activity insights', MessageSeverity.error);
                    this.isLoading$.next(false);
                    console.error('UserActivityInsights load error:', err);
                }
            });
    }


    onTimePresetChange(): void {
        this.loadData();
    }


    //
    // Auto-refresh
    //
    toggleAutoRefresh(): void {
        this.autoRefreshEnabled = !this.autoRefreshEnabled;
        if (this.autoRefreshEnabled) {
            this.startAutoRefresh();
        } else {
            this.stopAutoRefresh();
        }
    }

    private startAutoRefresh(): void {
        this.stopAutoRefresh();
        this.autoRefreshSubscription = interval(this.autoRefreshInterval * 1000)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => this.loadData());
    }

    private stopAutoRefresh(): void {
        if (this.autoRefreshSubscription) {
            this.autoRefreshSubscription.unsubscribe();
            this.autoRefreshSubscription = null;
        }
    }


    //
    // D3 chart rendering orchestrator
    //
    private renderAllCharts(): void {
        if (!this.data) return;

        this.renderRadialClock();
        this.renderHeatmapCalendar();
        this.renderModuleBarChart();
        this.renderDonutChart();
        this.renderSankeyDiagram();

        this.chartsRendered = true;
    }


    //
    // 1. Radial Activity Clock — polar area chart showing hour-of-day patterns
    //
    private renderRadialClock(): void {
        if (!this.radialClockChartRef || !this.data?.activityByHour?.length) return;

        const container = this.radialClockChartRef.nativeElement;
        d3.select(container).selectAll('*').remove();

        const width = container.clientWidth || 320;
        const height = Math.min(width, 320);
        const radius = Math.min(width, height) / 2 - 30;

        const svg = d3.select(container)
            .append('svg')
            .attr('width', width)
            .attr('height', height)
            .append('g')
            .attr('transform', `translate(${width / 2},${height / 2})`);

        // Fill in missing hours with 0
        const hourData: { hour: number; count: number }[] = [];
        for (let h = 0; h < 24; h++) {
            const found = this.data.activityByHour.find(a => a.hour === h);
            hourData.push({ hour: h, count: found ? found.count : 0 });
        }

        const maxCount = d3.max(hourData, d => d.count) || 1;
        const angleScale = d3.scaleLinear().domain([0, 24]).range([0, 2 * Math.PI]);
        const radiusScale = d3.scaleLinear().domain([0, maxCount]).range([radius * 0.2, radius]);

        // Color scale
        const colorScale = d3.scaleSequential(d3.interpolateViridis).domain([0, maxCount]);

        // Draw radial bars
        const arc = d3.arc<{ hour: number; count: number }>()
            .innerRadius(radius * 0.2)
            .outerRadius(d => radiusScale(d.count))
            .startAngle(d => angleScale(d.hour) - Math.PI / 24)
            .endAngle(d => angleScale(d.hour) + Math.PI / 24)
            .padAngle(0.02)
            .cornerRadius(3);

        svg.selectAll('.radial-bar')
            .data(hourData)
            .enter()
            .append('path')
            .attr('class', 'radial-bar')
            .attr('d', arc as any)
            .attr('fill', d => colorScale(d.count))
            .attr('opacity', 0.85)
            .on('mouseover', function (event, d) {
                d3.select(this).attr('opacity', 1).attr('stroke', 'var(--bs-body-color)').attr('stroke-width', 2);
                tooltip.style('opacity', 1)
                    .html(`<strong>${d.hour}:00 – ${d.hour}:59</strong><br/>${d.count.toLocaleString()} events`)
                    .style('left', (event.offsetX + 10) + 'px')
                    .style('top', (event.offsetY - 20) + 'px');
            })
            .on('mouseout', function () {
                d3.select(this).attr('opacity', 0.85).attr('stroke', 'none');
                tooltip.style('opacity', 0);
            })
            .transition()
            .duration(800)
            .attrTween('d', function (d) {
                const interpolateCount = d3.interpolate(0, d.count);
                return function (t) {
                    d.count = interpolateCount(t);
                    return (arc as any)(d);
                };
            });

        // Hour labels
        for (let h = 0; h < 24; h += 3) {
            const angle = angleScale(h) - Math.PI / 2;
            const labelRadius = radius + 15;
            svg.append('text')
                .attr('class', 'radial-label')
                .attr('x', Math.cos(angle) * labelRadius)
                .attr('y', Math.sin(angle) * labelRadius)
                .attr('text-anchor', 'middle')
                .attr('dominant-baseline', 'middle')
                .attr('fill', 'var(--bs-body-color)')
                .attr('font-size', '11px')
                .attr('opacity', 0.7)
                .text(`${h}h`);
        }

        // Tooltip div
        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'insights-tooltip')
            .style('opacity', 0);
    }


    //
    // 2. Heatmap Calendar — daily activity density as colored cells
    //
    private renderHeatmapCalendar(): void {
        if (!this.heatmapChartRef || !this.data?.activityByDay?.length) return;

        const container = this.heatmapChartRef.nativeElement;
        d3.select(container).selectAll('*').remove();

        const cellSize = 14;
        const cellPadding = 2;
        const data = this.data.activityByDay.map(d => ({
            date: new Date(d.date),
            count: d.count,
            uniqueUsers: d.uniqueUsers
        }));

        const maxCount = d3.max(data, d => d.count) || 1;
        const colorScale = d3.scaleSequential(d3.interpolateViridis).domain([0, maxCount]);

        // Calculate weeks
        const startDate = data[0].date;
        const endDate = data[data.length - 1].date;
        const weekCount = d3.timeWeek.count(startDate, endDate) + 2;

        const margin = { top: 20, right: 10, bottom: 10, left: 30 };
        const width = weekCount * (cellSize + cellPadding) + margin.left + margin.right;
        const height = 7 * (cellSize + cellPadding) + margin.top + margin.bottom;

        const svg = d3.select(container)
            .append('svg')
            .attr('width', Math.max(width, container.clientWidth))
            .attr('height', height)
            .append('g')
            .attr('transform', `translate(${margin.left},${margin.top})`);

        // Day labels
        const dayLabels = ['', 'Mon', '', 'Wed', '', 'Fri', ''];
        svg.selectAll('.day-label')
            .data(dayLabels)
            .enter()
            .append('text')
            .attr('class', 'day-label')
            .attr('x', -5)
            .attr('y', (_, i) => i * (cellSize + cellPadding) + cellSize / 2)
            .attr('text-anchor', 'end')
            .attr('dominant-baseline', 'middle')
            .attr('fill', 'var(--bs-body-color)')
            .attr('font-size', '10px')
            .attr('opacity', 0.6)
            .text(d => d);

        // Cells
        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'insights-tooltip')
            .style('opacity', 0);

        svg.selectAll('.heatmap-cell')
            .data(data)
            .enter()
            .append('rect')
            .attr('class', 'heatmap-cell')
            .attr('x', d => {
                const weekNum = d3.timeWeek.count(startDate, d.date);
                return weekNum * (cellSize + cellPadding);
            })
            .attr('y', d => d.date.getDay() * (cellSize + cellPadding))
            .attr('width', cellSize)
            .attr('height', cellSize)
            .attr('rx', 2)
            .attr('fill', d => d.count > 0 ? colorScale(d.count) : 'var(--bs-border-color)')
            .attr('opacity', 0)
            .on('mouseover', function (event, d) {
                d3.select(this).attr('stroke', 'var(--bs-body-color)').attr('stroke-width', 2);
                tooltip.style('opacity', 1)
                    .html(`<strong>${d.date.toLocaleDateString()}</strong><br/>${d.count.toLocaleString()} events<br/>${d.uniqueUsers} users`)
                    .style('left', (event.offsetX + 10) + 'px')
                    .style('top', (event.offsetY - 20) + 'px');
            })
            .on('mouseout', function () {
                d3.select(this).attr('stroke', 'none');
                tooltip.style('opacity', 0);
            })
            .transition()
            .duration(800)
            .delay((_, i) => i * 5)
            .attr('opacity', 1);
    }


    //
    // 3. Module Usage Bar Chart — horizontal bars with read/write breakdown
    //
    private renderModuleBarChart(): void {
        if (!this.moduleBarChartRef || !this.data?.topModules?.length) return;

        const container = this.moduleBarChartRef.nativeElement;
        d3.select(container).selectAll('*').remove();

        const data = this.data.topModules.slice(0, 12);   // top 12 modules

        const margin = { top: 10, right: 20, bottom: 30, left: 120 };
        const width = (container.clientWidth || 400) - margin.left - margin.right;
        const barHeight = 22;
        const height = data.length * (barHeight + 6) + margin.top + margin.bottom;

        const svg = d3.select(container)
            .append('svg')
            .attr('width', width + margin.left + margin.right)
            .attr('height', height)
            .append('g')
            .attr('transform', `translate(${margin.left},${margin.top})`);

        const maxCount = d3.max(data, d => d.eventCount) || 1;
        const xScale = d3.scaleLinear().domain([0, maxCount]).range([0, width]);
        const yScale = d3.scaleBand()
            .domain(data.map(d => d.moduleName))
            .range([0, data.length * (barHeight + 6)])
            .padding(0.15);

        // Read bars (teal)
        svg.selectAll('.bar-read')
            .data(data)
            .enter()
            .append('rect')
            .attr('class', 'bar-read')
            .attr('x', 0)
            .attr('y', d => yScale(d.moduleName) || 0)
            .attr('height', yScale.bandwidth())
            .attr('rx', 4)
            .attr('fill', '#20c997')
            .attr('opacity', 0.8)
            .attr('width', 0)
            .transition()
            .duration(800)
            .attr('width', d => xScale(d.readCount));

        // Write bars (stacked after reads, orange)
        svg.selectAll('.bar-write')
            .data(data)
            .enter()
            .append('rect')
            .attr('class', 'bar-write')
            .attr('x', d => xScale(d.readCount))
            .attr('y', d => yScale(d.moduleName) || 0)
            .attr('height', yScale.bandwidth())
            .attr('rx', 4)
            .attr('fill', '#fd7e14')
            .attr('opacity', 0.8)
            .attr('width', 0)
            .transition()
            .duration(800)
            .delay(400)
            .attr('width', d => xScale(d.writeCount));

        // Other events bar (remaining, muted)
        svg.selectAll('.bar-other')
            .data(data)
            .enter()
            .append('rect')
            .attr('class', 'bar-other')
            .attr('x', d => xScale(d.readCount + d.writeCount))
            .attr('y', d => yScale(d.moduleName) || 0)
            .attr('height', yScale.bandwidth())
            .attr('rx', 4)
            .attr('fill', 'var(--bs-secondary)')
            .attr('opacity', 0.4)
            .attr('width', 0)
            .transition()
            .duration(800)
            .delay(600)
            .attr('width', d => xScale(d.eventCount - d.readCount - d.writeCount));

        // Labels
        svg.selectAll('.bar-label')
            .data(data)
            .enter()
            .append('text')
            .attr('class', 'bar-label')
            .attr('x', -5)
            .attr('y', d => (yScale(d.moduleName) || 0) + yScale.bandwidth() / 2)
            .attr('text-anchor', 'end')
            .attr('dominant-baseline', 'middle')
            .attr('fill', 'var(--bs-body-color)')
            .attr('font-size', '11px')
            .text(d => d.moduleName.length > 18 ? d.moduleName.substring(0, 18) + '…' : d.moduleName);

        // Count labels
        svg.selectAll('.count-label')
            .data(data)
            .enter()
            .append('text')
            .attr('class', 'count-label')
            .attr('x', d => xScale(d.eventCount) + 5)
            .attr('y', d => (yScale(d.moduleName) || 0) + yScale.bandwidth() / 2)
            .attr('dominant-baseline', 'middle')
            .attr('fill', 'var(--bs-body-color)')
            .attr('font-size', '10px')
            .attr('opacity', 0.6)
            .text(d => d.eventCount.toLocaleString());

        // Tooltip
        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'insights-tooltip')
            .style('opacity', 0);

        svg.selectAll('rect')
            .on('mouseover', function (event, d: any) {
                tooltip.style('opacity', 1)
                    .html(`<strong>${d.moduleName}</strong><br/>Total: ${d.eventCount.toLocaleString()}<br/>Reads: ${d.readCount.toLocaleString()}<br/>Writes: ${d.writeCount.toLocaleString()}<br/>Users: ${d.uniqueUsers}`)
                    .style('left', (event.offsetX + 10) + 'px')
                    .style('top', (event.offsetY - 20) + 'px');
            })
            .on('mouseout', function () {
                tooltip.style('opacity', 0);
            });
    }


    //
    // 4. Event Type Donut Chart
    //
    private renderDonutChart(): void {
        if (!this.donutChartRef || !this.data?.eventTypeBreakdown?.length) return;

        const container = this.donutChartRef.nativeElement;
        d3.select(container).selectAll('*').remove();

        const width = container.clientWidth || 300;
        const height = Math.min(width, 300);
        const radius = Math.min(width, height) / 2 - 10;

        const svg = d3.select(container)
            .append('svg')
            .attr('width', width)
            .attr('height', height)
            .append('g')
            .attr('transform', `translate(${width / 2},${height / 2})`);

        const data = this.data.eventTypeBreakdown;
        const total = d3.sum(data, d => d.count);

        const colorPalette = [
            '#6f42c1', '#20c997', '#0dcaf0', '#fd7e14', '#d63384',
            '#198754', '#ffc107', '#0d6efd', '#6610f2', '#dc3545',
            '#adb5bd', '#495057', '#17a2b8', '#28a745', '#e83e8c',
            '#6c757d', '#343a40', '#e9ecef', '#fafbfc'
        ];

        const color = d3.scaleOrdinal<string>().domain(data.map(d => d.auditTypeName)).range(colorPalette);

        const pie = d3.pie<EventTypeBreakdown>().value(d => d.count).sort(null);
        const arc = d3.arc<d3.PieArcDatum<EventTypeBreakdown>>()
            .innerRadius(radius * 0.55)
            .outerRadius(radius);
        const hoverArc = d3.arc<d3.PieArcDatum<EventTypeBreakdown>>()
            .innerRadius(radius * 0.55)
            .outerRadius(radius + 8);

        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'insights-tooltip')
            .style('opacity', 0);

        const arcs = svg.selectAll('.arc')
            .data(pie(data))
            .enter()
            .append('g')
            .attr('class', 'arc');

        arcs.append('path')
            .attr('d', arc)
            .attr('fill', d => color(d.data.auditTypeName))
            .attr('opacity', 0.85)
            .attr('stroke', 'var(--bs-body-bg)')
            .attr('stroke-width', 2)
            .on('mouseover', function (event, d) {
                d3.select(this)
                    .transition().duration(200)
                    .attr('d', hoverArc as any)
                    .attr('opacity', 1);
                const pct = ((d.data.count / total) * 100).toFixed(1);
                tooltip.style('opacity', 1)
                    .html(`<strong>${d.data.auditTypeName}</strong><br/>${d.data.count.toLocaleString()} events (${pct}%)`)
                    .style('left', (event.offsetX + 10) + 'px')
                    .style('top', (event.offsetY - 20) + 'px');
            })
            .on('mouseout', function () {
                d3.select(this)
                    .transition().duration(200)
                    .attr('d', arc as any)
                    .attr('opacity', 0.85);
                tooltip.style('opacity', 0);
            })
            .transition()
            .duration(800)
            .attrTween('d', function (d) {
                const i = d3.interpolate({ startAngle: 0, endAngle: 0 }, d);
                return function (t) {
                    return arc(i(t)) || '';
                };
            });

        // Center text
        svg.append('text')
            .attr('text-anchor', 'middle')
            .attr('dominant-baseline', 'middle')
            .attr('fill', 'var(--bs-body-color)')
            .attr('font-size', '24px')
            .attr('font-weight', '700')
            .text(total.toLocaleString());

        svg.append('text')
            .attr('text-anchor', 'middle')
            .attr('dominant-baseline', 'middle')
            .attr('y', 22)
            .attr('fill', 'var(--bs-body-color)')
            .attr('font-size', '11px')
            .attr('opacity', 0.6)
            .text('events');
    }


    //
    // 5. Sankey Diagram — user → module flow
    //
    private renderSankeyDiagram(): void {
        if (!this.sankeyChartRef || !this.data?.userModuleMatrix?.length) return;

        const container = this.sankeyChartRef.nativeElement;
        d3.select(container).selectAll('*').remove();

        const margin = { top: 10, right: 10, bottom: 10, left: 10 };
        const width = (container.clientWidth || 600) - margin.left - margin.right;
        const height = Math.max(300, this.data.userModuleMatrix.length * 8);

        // Build nodes and links
        const userNames = [...new Set(this.data.userModuleMatrix.map(l => l.userName))];
        const moduleNames = [...new Set(this.data.userModuleMatrix.map(l => l.moduleName))];

        const nodes: { name: string }[] = [
            ...userNames.map(n => ({ name: n })),
            ...moduleNames.map(n => ({ name: n }))
        ];

        const nodeIndex = new Map(nodes.map((n, i) => [n.name, i]));

        // Disambiguate: if a userName matches a moduleName, offset module index
        const links = this.data.userModuleMatrix.map(l => ({
            source: nodeIndex.get(l.userName) ?? 0,
            target: (nodeIndex.get(l.moduleName) ?? 0),
            value: l.count
        })).filter(l => l.source !== l.target);

        if (links.length === 0) return;

        const svg = d3.select(container)
            .append('svg')
            .attr('width', width + margin.left + margin.right)
            .attr('height', height + margin.top + margin.bottom)
            .append('g')
            .attr('transform', `translate(${margin.left},${margin.top})`);

        const sankeyLayout = d3Sankey<{ name: string }, {}>()
            .nodeWidth(15)
            .nodePadding(10)
            .extent([[0, 0], [width, height]]);

        const graph = sankeyLayout({
            nodes: nodes.map(d => ({ ...d })),
            links: links.map(d => ({ ...d }))
        });

        const colorPalette = [
            '#6f42c1', '#20c997', '#0dcaf0', '#fd7e14', '#d63384',
            '#198754', '#ffc107', '#0d6efd', '#6610f2', '#dc3545'
        ];

        const nodeColor = d3.scaleOrdinal<string>().domain(userNames).range(colorPalette);

        // Links
        svg.append('g')
            .selectAll('.sankey-link')
            .data(graph.links)
            .enter()
            .append('path')
            .attr('class', 'sankey-link')
            .attr('d', sankeyLinkHorizontal())
            .attr('fill', 'none')
            .attr('stroke', (d: any) => {
                const sourceName = (d.source as any).name;
                return nodeColor(sourceName);
            })
            .attr('stroke-opacity', 0.35)
            .attr('stroke-width', (d: any) => Math.max(1, d.width))
            .on('mouseover', function (event, d: any) {
                d3.select(this).attr('stroke-opacity', 0.7);
                tooltip.style('opacity', 1)
                    .html(`<strong>${(d.source as any).name}</strong> → <strong>${(d.target as any).name}</strong><br/>${d.value.toLocaleString()} events`)
                    .style('left', (event.offsetX + 10) + 'px')
                    .style('top', (event.offsetY - 20) + 'px');
            })
            .on('mouseout', function () {
                d3.select(this).attr('stroke-opacity', 0.35);
                tooltip.style('opacity', 0);
            });

        // Nodes
        svg.append('g')
            .selectAll('.sankey-node')
            .data(graph.nodes)
            .enter()
            .append('rect')
            .attr('class', 'sankey-node')
            .attr('x', (d: any) => d.x0)
            .attr('y', (d: any) => d.y0)
            .attr('width', (d: any) => d.x1 - d.x0)
            .attr('height', (d: any) => Math.max(1, d.y1 - d.y0))
            .attr('fill', (d: any) => nodeColor(d.name))
            .attr('opacity', 0.9)
            .attr('rx', 3);

        // Node labels
        svg.append('g')
            .selectAll('.sankey-label')
            .data(graph.nodes)
            .enter()
            .append('text')
            .attr('class', 'sankey-label')
            .attr('x', (d: any) => d.x0 < width / 2 ? d.x1 + 6 : d.x0 - 6)
            .attr('y', (d: any) => (d.y0 + d.y1) / 2)
            .attr('text-anchor', (d: any) => d.x0 < width / 2 ? 'start' : 'end')
            .attr('dominant-baseline', 'middle')
            .attr('fill', 'var(--bs-body-color)')
            .attr('font-size', '11px')
            .text((d: any) => d.name);

        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'insights-tooltip')
            .style('opacity', 0);
    }


    //
    // Formatting helpers for the template
    //
    formatSuccessRate(rate: number): string {
        return rate.toFixed(1) + '%';
    }

    getSuccessRateClass(rate: number): string {
        if (rate >= 99) return 'text-success';
        if (rate >= 95) return 'text-warning';
        return 'text-danger';
    }

    formatDate(dateStr: string): string {
        return new Date(dateStr).toLocaleString();
    }

    formatDateShort(dateStr: string): string {
        return new Date(dateStr).toLocaleDateString();
    }

    getTimeAgo(dateStr: string): string {
        const diff = Date.now() - new Date(dateStr).getTime();
        const minutes = Math.floor(diff / 60000);
        if (minutes < 60) return `${minutes}m ago`;
        const hours = Math.floor(minutes / 60);
        if (hours < 24) return `${hours}h ago`;
        const days = Math.floor(hours / 24);
        return `${days}d ago`;
    }


    //
    // Navigation
    //
    goBack(): void {
        this.location.back();
    }

    canGoBack(): boolean {
        return window.history.length > 1;
    }
}
