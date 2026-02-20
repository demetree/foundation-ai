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
    FailureHotspot,
    ModuleEntityDetail
} from '../../services/user-activity-insights.service';

import * as d3 from 'd3';
import { sankey as d3Sankey, sankeyLinkHorizontal, sankeyLeft } from 'd3-sankey';




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
    @ViewChild('sankeyFlowChart', { static: false }) sankeyFlowChartRef!: ElementRef;
    @ViewChild('treemapChart', { static: false }) treemapChartRef!: ElementRef;

    private chartsRendered = false;

    //
    // Drill-down state
    //
    drillModule: string | null = null;

    //
    // Focus filters — client-side user/module filtering
    //
    selectedUser: string = '';
    selectedModule: string = '';

    get availableUsers(): string[] {
        if (!this.data?.userModuleMatrix?.length) return [];
        return [...new Set(this.data.userModuleMatrix.map(l => l.userName))].sort();
    }

    get availableModules(): string[] {
        if (!this.data?.userModuleMatrix?.length) return [];
        let matrix = this.data.userModuleMatrix;
        if (this.selectedUser) {
            matrix = matrix.filter(l => l.userName === this.selectedUser);
        }
        return [...new Set(matrix.map(l => l.moduleName))].sort();
    }

    get hasActiveFilter(): boolean {
        return !!this.selectedUser || !!this.selectedModule;
    }

    /** Returns a filtered copy of the raw data based on selectedUser/selectedModule. */
    get filteredData(): UserActivityInsightsResponse | null {
        if (!this.data) return null;
        if (!this.selectedUser && !this.selectedModule) return this.data;

        // Filter userModuleMatrix
        let matrix = this.data.userModuleMatrix;
        if (this.selectedUser) {
            matrix = matrix.filter(l => l.userName === this.selectedUser);
        }
        if (this.selectedModule) {
            matrix = matrix.filter(l => l.moduleName === this.selectedModule);
        }

        // Rebuild topModules from filtered matrix
        const moduleMap = new Map<string, { eventCount: number; readCount: number; writeCount: number; users: Set<string> }>();
        for (const link of matrix) {
            const existing = moduleMap.get(link.moduleName);
            if (existing) {
                existing.eventCount += link.count;
                existing.users.add(link.userName);
            } else {
                moduleMap.set(link.moduleName, { eventCount: link.count, readCount: 0, writeCount: 0, users: new Set([link.userName]) });
            }
        }
        // Enrich with read/write from original topModules
        for (const [name, stats] of moduleMap) {
            const orig = this.data.topModules.find(m => m.moduleName === name);
            if (orig) {
                stats.readCount = orig.readCount;
                stats.writeCount = orig.writeCount;
            }
        }
        const filteredTopModules: TopModule[] = [...moduleMap.entries()].map(([name, stats]) => ({
            moduleName: name,
            eventCount: stats.eventCount,
            uniqueUsers: stats.users.size,
            readCount: stats.readCount,
            writeCount: stats.writeCount
        })).sort((a, b) => b.eventCount - a.eventCount);

        // Filter topUsers
        let filteredTopUsers = this.data.topUsers;
        if (this.selectedUser) {
            filteredTopUsers = filteredTopUsers.filter(u => u.userName === this.selectedUser);
        }

        // Filter moduleEntityBreakdown
        let filteredEntities = this.data.moduleEntityBreakdown;
        if (this.selectedModule) {
            filteredEntities = filteredEntities.filter(e => e.moduleName === this.selectedModule);
        }
        if (this.selectedUser) {
            // Only show entities for modules the selected user has accessed
            const userModules = new Set(matrix.map(l => l.moduleName));
            filteredEntities = filteredEntities.filter(e => userModules.has(e.moduleName));
        }

        // Filter recentSessions
        let filteredSessions = this.data.recentSessions;
        if (this.selectedUser) {
            filteredSessions = filteredSessions.filter(s => s.userName === this.selectedUser);
        }
        if (this.selectedModule) {
            filteredSessions = filteredSessions.filter(s => s.modules.includes(this.selectedModule));
        }

        // Filter failureHotspots
        let filteredHotspots = this.data.failureHotspots;
        if (this.selectedUser) {
            filteredHotspots = filteredHotspots.filter(h => h.userName === this.selectedUser);
        }
        if (this.selectedModule) {
            filteredHotspots = filteredHotspots.filter(h => h.moduleName === this.selectedModule);
        }

        return {
            ...this.data,
            userModuleMatrix: matrix,
            topModules: filteredTopModules,
            topUsers: filteredTopUsers,
            moduleEntityBreakdown: filteredEntities,
            recentSessions: filteredSessions,
            failureHotspots: filteredHotspots
        };
    }

    onFilterChange(): void {
        // If user changes, check if selected module is still valid
        if (this.selectedModule && !this.availableModules.includes(this.selectedModule)) {
            this.selectedModule = '';
        }
        setTimeout(() => this.renderAllCharts(), 50);
    }

    clearFilters(): void {
        this.selectedUser = '';
        this.selectedModule = '';
        setTimeout(() => this.renderAllCharts(), 50);
    }


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
        const d = this.filteredData;
        if (!d) return;

        this.renderRadialClock();
        this.renderHeatmapCalendar();
        this.renderModuleBarChart();
        this.renderDonutChart();
        this.renderSunburst();
        this.renderSankeyFlow();
        this.renderTreemap();

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

        //
        // Build a lookup map from the server data (keyed by date string YYYY-MM-DD)
        //
        const dataMap = new Map<string, { count: number; uniqueUsers: number }>();
        for (const d of this.data.activityByDay) {
            const dateKey = new Date(d.date).toISOString().slice(0, 10);
            dataMap.set(dateKey, { count: d.count, uniqueUsers: d.uniqueUsers });
        }

        //
        // Use the full query time range (not just dates with data)
        // so the grid always spans the entire requested period.
        //
        const rangeStart = new Date(this.data.summary.startDate);
        const rangeEnd = new Date(this.data.summary.endDate);

        // Align start to the previous Sunday so columns line up properly
        const gridStart = d3.timeSunday.floor(rangeStart);

        const allDays: { date: Date; count: number; uniqueUsers: number }[] = [];
        const cursor = new Date(gridStart);
        while (cursor <= rangeEnd) {
            const key = cursor.toISOString().slice(0, 10);
            const entry = dataMap.get(key);
            allDays.push({
                date: new Date(cursor),
                count: entry ? entry.count : 0,
                uniqueUsers: entry ? entry.uniqueUsers : 0
            });
            cursor.setDate(cursor.getDate() + 1);
        }

        const maxCount = d3.max(allDays, d => d.count) || 1;
        const colorScale = d3.scaleSequential(d3.interpolateViridis).domain([0, maxCount]);

        // Calculate the number of weeks (columns)
        const weekCount = d3.timeWeek.count(gridStart, rangeEnd) + 2;

        const margin = { top: 20, right: 10, bottom: 10, left: 30 };
        const gridWidth = weekCount * (cellSize + cellPadding);
        const svgWidth = Math.max(gridWidth + margin.left + margin.right, container.clientWidth);
        const height = 7 * (cellSize + cellPadding) + margin.top + margin.bottom;

        const svg = d3.select(container)
            .append('svg')
            .attr('width', svgWidth)
            .attr('height', height)
            .append('g')
            .attr('transform', `translate(${margin.left},${margin.top})`);

        // Day-of-week labels
        const dayLabels = ['Sun', 'Mon', '', 'Wed', '', 'Fri', ''];
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

        // Month labels along the top
        const monthsShown = new Set<string>();
        for (const d of allDays) {
            if (d.date.getDate() <= 7) {   // first week of month
                const monthKey = `${d.date.getFullYear()}-${d.date.getMonth()}`;
                if (!monthsShown.has(monthKey)) {
                    monthsShown.add(monthKey);
                    const weekNum = d3.timeWeek.count(gridStart, d.date);
                    svg.append('text')
                        .attr('class', 'month-label')
                        .attr('x', weekNum * (cellSize + cellPadding))
                        .attr('y', -6)
                        .attr('fill', 'var(--bs-body-color)')
                        .attr('font-size', '10px')
                        .attr('opacity', 0.6)
                        .text(d.date.toLocaleString('default', { month: 'short' }));
                }
            }
        }

        // Tooltip
        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'insights-tooltip')
            .style('opacity', 0);

        // Render ALL cells
        svg.selectAll('.heatmap-cell')
            .data(allDays)
            .enter()
            .append('rect')
            .attr('class', 'heatmap-cell')
            .attr('x', d => {
                const weekNum = d3.timeWeek.count(gridStart, d.date);
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
            .duration(600)
            .delay((_, i) => i * 3)
            .attr('opacity', 1);
    }


    //
    // 3. Module Usage Bar Chart — horizontal bars with read/write breakdown
    //
    private renderModuleBarChart(): void {
        if (!this.moduleBarChartRef || !this.filteredData?.topModules?.length) return;

        const container = this.moduleBarChartRef.nativeElement;
        d3.select(container).selectAll('*').remove();

        const data = this.filteredData!.topModules.slice(0, 12);   // top 12 modules

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

        // Click handler — drill into module
        svg.selectAll('.bar-read')
            .style('cursor', 'pointer')
            .on('click', (_event: any, d: any) => this.drillIntoModule(d.moduleName));

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
            })
            .on('click', (_event: any, d: any) => this.drillIntoModule(d.moduleName));

        svg.selectAll('rect').style('cursor', 'pointer');
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
    // 5. Sequences Sunburst — hierarchical User → Module → Entity breakdown
    //
    private renderSunburst(): void {
        if (!this.sankeyChartRef || !this.filteredData?.userModuleMatrix?.length) return;

        const container = this.sankeyChartRef.nativeElement;
        d3.select(container).selectAll('*').remove();

        //
        // Build hierarchical data: root → users → modules → entities
        //
        interface SunburstNode {
            name: string;
            value?: number;
            children?: SunburstNode[];
        }

        const root: SunburstNode = { name: 'All Activity', children: [] };

        // Group userModuleMatrix by user
        const userGroups = new Map<string, Map<string, number>>();
        for (const link of this.filteredData!.userModuleMatrix) {
            if (!userGroups.has(link.userName)) {
                userGroups.set(link.userName, new Map());
            }
            userGroups.get(link.userName)!.set(link.moduleName, link.count);
        }

        // Group moduleEntityBreakdown by module
        const moduleEntityGroups = new Map<string, { entityName: string; eventCount: number }[]>();
        if (this.filteredData!.moduleEntityBreakdown) {
            for (const me of this.filteredData!.moduleEntityBreakdown) {
                if (!moduleEntityGroups.has(me.moduleName)) {
                    moduleEntityGroups.set(me.moduleName, []);
                }
                moduleEntityGroups.get(me.moduleName)!.push(me);
            }
        }

        // Build tree: root → users → modules → entities
        // Limit to top 8 users for readability
        const topUsers = [...userGroups.entries()]
            .sort((a, b) => {
                const sumA = [...a[1].values()].reduce((s, v) => s + v, 0);
                const sumB = [...b[1].values()].reduce((s, v) => s + v, 0);
                return sumB - sumA;
            })
            .slice(0, 8);

        for (const [userName, modules] of topUsers) {
            const userNode: SunburstNode = { name: userName, children: [] };

            for (const [moduleName, count] of modules) {
                const entities = moduleEntityGroups.get(moduleName);
                if (entities && entities.length > 0) {
                    // Add entities as children of the module
                    const moduleNode: SunburstNode = {
                        name: moduleName,
                        children: entities.slice(0, 10).map(e => ({
                            name: e.entityName,
                            value: e.eventCount
                        }))
                    };
                    userNode.children!.push(moduleNode);
                } else {
                    // Leaf module (no entities)
                    userNode.children!.push({ name: moduleName, value: count });
                }
            }

            root.children!.push(userNode);
        }

        //
        // D3 partition / sunburst layout
        //
        const containerWidth = container.clientWidth || 600;
        const size = Math.min(containerWidth, 520);
        const radius = size / 2;

        const partition = d3.partition<SunburstNode>()
            .size([2 * Math.PI, radius]);

        const hierarchy = d3.hierarchy(root)
            .sum(d => d.value || 0)
            .sort((a, b) => (b.value || 0) - (a.value || 0));

        partition(hierarchy);

        // Color palette: one hue per top-level user
        // Collect all unique module names for a diverse color palette
        const moduleNames = [...new Set(
            hierarchy.descendants()
                .filter(d => d.depth === 2)
                .map(d => d.data.name)
        )];
        const colorPalette = [
            '#0dcaf0', '#20c997', '#fd7e14', '#d63384', '#6f42c1',
            '#198754', '#ffc107', '#0d6efd', '#dc3545', '#6610f2',
            '#e83e8c', '#17a2b8', '#28a745', '#ff6b6b'
        ];
        const moduleColorScale = d3.scaleOrdinal<string>()
            .domain(moduleNames)
            .range(colorPalette);

        // User (depth 1) gets a muted slate, modules get distinct hues,
        // entities inherit their parent module color but lighter
        function getNodeColor(d: d3.HierarchyRectangularNode<SunburstNode>): string {
            if (d.depth === 0) return 'transparent';
            if (d.depth === 1) {
                // User ring — use a semi-transparent muted color
                const idx = hierarchy.children
                    ? hierarchy.children.indexOf(d as any) : 0;
                const hue = (idx * 47 + 210) % 360; // spread hues
                return `hsl(${hue}, 40%, 55%)`;
            }
            // Find the module ancestor (depth 2)
            let moduleNode = d;
            while (moduleNode.depth > 2 && moduleNode.parent) {
                moduleNode = moduleNode.parent;
            }
            const baseColor = moduleColorScale(moduleNode.data.name);
            if (d.depth === 2) return baseColor;
            // Entities — brighten
            const c = d3.color(baseColor);
            return c ? (c as d3.RGBColor).brighter(0.6).toString() : baseColor;
        }

        // SVG
        const svg = d3.select(container)
            .append('svg')
            .attr('width', size)
            .attr('height', size)
            .style('display', 'block')
            .style('margin', '0 auto');

        const g = svg.append('g')
            .attr('transform', `translate(${radius},${radius})`);

        const arc = d3.arc<d3.HierarchyRectangularNode<SunburstNode>>()
            .startAngle(d => d.x0)
            .endAngle(d => d.x1)
            .innerRadius(d => d.y0 * 0.65)
            .outerRadius(d => d.y1 * 0.65 - 1)
            .padAngle(0.004)
            .padRadius(radius / 2);

        // 1) Draw arcs FIRST (bottom layer)
        const allPaths = g.selectAll('path.sunburst-arc')
            .data(hierarchy.descendants().filter(d => d.depth > 0))
            .enter()
            .append('path')
            .attr('class', 'sunburst-arc')
            .attr('d', arc as any)
            .attr('fill', d => getNodeColor(d as any))
            .attr('stroke', 'var(--bs-body-bg)')
            .attr('stroke-width', 1)
            .attr('cursor', 'pointer')
            .attr('opacity', 1);

        // 2) Ring-level labels (above arcs)
        const depthLabels = ['', 'Users', 'Modules', 'Entities'];
        for (let depth = 1; depth <= 3; depth++) {
            const r = (hierarchy.descendants().find(d => d.depth === depth) as any);
            if (r) {
                const labelRadius = (r.y0 + r.y1) / 2 * 0.65;
                g.append('text')
                    .attr('text-anchor', 'middle')
                    .attr('y', -labelRadius)
                    .attr('fill', 'var(--bs-secondary-color)')
                    .attr('font-size', '9px')
                    .attr('font-weight', '600')
                    .attr('opacity', 0.5)
                    .attr('text-transform', 'uppercase')
                    .text(depthLabels[depth]);
            }
        }

        // 3) Center text group on TOP (rendered last = topmost layer)
        const centerGroup = g.append('g')
            .attr('class', 'sunburst-center')
            .style('pointer-events', 'none');

        const centerLabel = centerGroup.append('text')
            .attr('text-anchor', 'middle')
            .attr('fill', 'var(--bs-body-color)');

        const centerValue = centerLabel.append('tspan')
            .attr('x', 0)
            .attr('dy', '-0.1em')
            .attr('font-size', '1.6rem')
            .attr('font-weight', '800')
            .attr('letter-spacing', '-0.03em')
            .text((hierarchy.value || 0).toLocaleString());

        const centerCaption = centerLabel.append('tspan')
            .attr('x', 0)
            .attr('dy', '1.5em')
            .attr('font-size', '0.7rem')
            .attr('fill', 'var(--bs-secondary-color)')
            .attr('text-transform', 'uppercase')
            .text('total events');

        // Breadcrumb trail (below the value)
        const breadcrumbText = centerGroup.append('text')
            .attr('text-anchor', 'middle')
            .attr('y', 40)
            .attr('fill', 'var(--bs-secondary-color)')
            .attr('font-size', '0.72rem')
            .attr('opacity', 0);

        // Hover interaction — highlight ancestors (sequence path)
        allPaths
            .on('mouseover', function (_event, d) {
                // Build the ancestor sequence
                const ancestors = d.ancestors().reverse().filter(a => a.depth > 0);
                const ancestorSet = new Set(ancestors);

                // Fade non-ancestor arcs
                allPaths.attr('opacity', node => ancestorSet.has(node) ? 1 : 0.25);

                // Update center text to show the hovered segment
                const percentage = ((d.value || 0) / (hierarchy.value || 1) * 100);
                centerValue.text(percentage < 1 ? '<1%' : percentage.toFixed(1) + '%');
                centerCaption.text((d.value || 0).toLocaleString() + ' events');

                // Build breadcrumb trail text
                const trail = ancestors.map(a => a.data.name).join(' → ');
                breadcrumbText.text(trail).attr('opacity', 1);
            })
            .on('mouseout', function () {
                allPaths.attr('opacity', 1);
                centerValue.text((hierarchy.value || 0).toLocaleString());
                centerCaption.text('total events');
                breadcrumbText.attr('opacity', 0);
            })
            .on('click', (_event, d) => {
                // Click user/module to drill
                if (d.depth === 2 || (d.depth === 1 && d.children)) {
                    const moduleName = d.depth === 2 ? d.data.name
                        : d.children ? d.data.name : null;
                    // If it's a module node (depth 2), drill into it
                    if (d.depth === 2) {
                        this.drillIntoModule(d.data.name);
                    }
                }
            });
    }


    //
    // 5b. Sankey Flow — 3-column: User → Module → Operation Type
    //
    private renderSankeyFlow(): void {
        if (!this.sankeyFlowChartRef || !this.filteredData?.userModuleMatrix?.length) return;

        const container = this.sankeyFlowChartRef.nativeElement;
        d3.select(container).selectAll('*').remove();

        const fd = this.filteredData!;
        const matrix = fd.userModuleMatrix;
        const modules = fd.topModules;

        if (!modules?.length) return;

        // Build node lists
        const userNames = [...new Set(matrix.map(l => l.userName))].sort();
        const moduleNames = [...new Set(matrix.map(l => l.moduleName))].sort();
        const opTypes = ['Read', 'Write', 'Other'];

        // Limit for readability
        const topUserNames = userNames.slice(0, 10);
        const topModuleNames = moduleNames.slice(0, 10);

        // Build nodes: [users..., modules..., opTypes...]
        const nodes: { name: string; column: number }[] = [
            ...topUserNames.map(n => ({ name: n, column: 0 })),
            ...topModuleNames.map(n => ({ name: n, column: 1 })),
            ...opTypes.map(n => ({ name: n, column: 2 }))
        ];

        const nodeIndex = new Map(nodes.map((n, i) => [n.column + ':' + n.name, i]));

        // Links: User → Module
        const links: { source: number; target: number; value: number }[] = [];
        for (const link of matrix) {
            const sIdx = nodeIndex.get('0:' + link.userName);
            const tIdx = nodeIndex.get('1:' + link.moduleName);
            if (sIdx !== undefined && tIdx !== undefined && link.count > 0) {
                links.push({ source: sIdx, target: tIdx, value: link.count });
            }
        }

        // Links: Module → Operation Type
        for (const mod of modules) {
            const mIdx = nodeIndex.get('1:' + mod.moduleName);
            if (mIdx === undefined) continue;

            const readIdx = nodeIndex.get('2:Read')!;
            const writeIdx = nodeIndex.get('2:Write')!;
            const otherIdx = nodeIndex.get('2:Other')!;

            if (mod.readCount > 0) links.push({ source: mIdx, target: readIdx, value: mod.readCount });
            if (mod.writeCount > 0) links.push({ source: mIdx, target: writeIdx, value: mod.writeCount });
            const other = mod.eventCount - mod.readCount - mod.writeCount;
            if (other > 0) links.push({ source: mIdx, target: otherIdx, value: other });
        }

        if (links.length === 0) return;

        // Layout
        const margin = { top: 12, right: 10, bottom: 12, left: 10 };
        const width = (container.clientWidth || 700) - margin.left - margin.right;
        const height = Math.max(300, Math.min(500, topUserNames.length * 28 + 40));

        const svg = d3.select(container)
            .append('svg')
            .attr('width', width + margin.left + margin.right)
            .attr('height', height + margin.top + margin.bottom)
            .append('g')
            .attr('transform', `translate(${margin.left},${margin.top})`);

        const sankeyLayout = (d3Sankey as any)()
            .nodeWidth(16)
            .nodePadding(10)
            .nodeAlign(sankeyLeft as any)
            .extent([[0, 0], [width, height]]);

        const graph = sankeyLayout({
            nodes: nodes.map(d => ({ ...d })),
            links: links.map(d => ({ ...d }))
        });

        // Colors
        const userPalette = ['#6f42c1', '#0d6efd', '#0dcaf0', '#198754',
            '#fd7e14', '#d63384', '#20c997', '#ffc107', '#6610f2', '#dc3545'];
        const modulePalette = ['#0dcaf0', '#20c997', '#fd7e14', '#d63384',
            '#6f42c1', '#198754', '#ffc107', '#0d6efd', '#dc3545', '#6610f2'];
        const opColors: Record<string, string> = {
            'Read': '#20c997', 'Write': '#fd7e14', 'Other': '#6c757d'
        };

        function nodeColor(d: any): string {
            if (d.column === 0) return userPalette[topUserNames.indexOf(d.name) % userPalette.length];
            if (d.column === 1) return modulePalette[topModuleNames.indexOf(d.name) % modulePalette.length];
            return opColors[d.name] || '#6c757d';
        }

        // Tooltip
        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'insights-tooltip')
            .style('opacity', 0);

        // Links
        svg.append('g')
            .selectAll('.sankey-link')
            .data(graph.links)
            .enter()
            .append('path')
            .attr('class', 'sankey-link')
            .attr('d', sankeyLinkHorizontal() as any)
            .attr('fill', 'none')
            .attr('stroke', (d: any) => nodeColor(d.source))
            .attr('stroke-opacity', 0.3)
            .attr('stroke-width', (d: any) => Math.max(1, d.width))
            .on('mouseover', function (event: any, d: any) {
                d3.select(this).attr('stroke-opacity', 0.6);
                tooltip.style('opacity', 1)
                    .html(`<strong>${d.source.name}</strong> → <strong>${d.target.name}</strong><br/>${d.value.toLocaleString()} events`)
                    .style('left', (event.offsetX + 12) + 'px')
                    .style('top', (event.offsetY - 24) + 'px');
            })
            .on('mouseout', function () {
                d3.select(this).attr('stroke-opacity', 0.3);
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
            .attr('height', (d: any) => Math.max(2, d.y1 - d.y0))
            .attr('fill', (d: any) => nodeColor(d))
            .attr('opacity', 0.9)
            .attr('rx', 3);

        // Node labels
        svg.append('g')
            .selectAll('.sankey-label')
            .data(graph.nodes)
            .enter()
            .append('text')
            .attr('class', 'sankey-label')
            .attr('x', (d: any) => d.column === 2 ? d.x1 + 6 : d.column === 0 ? d.x0 - 6 : (d.x0 + d.x1) / 2)
            .attr('y', (d: any) => (d.y0 + d.y1) / 2)
            .attr('text-anchor', (d: any) => d.column === 2 ? 'start' : d.column === 0 ? 'end' : 'middle')
            .attr('dominant-baseline', 'middle')
            .attr('fill', 'var(--bs-body-color)')
            .attr('font-size', '11px')
            .attr('font-weight', (d: any) => d.column === 2 ? '700' : '400')
            .text((d: any) => {
                const label = d.name as string;
                return label.length > 18 ? label.substring(0, 16) + '…' : label;
            });

        // Column headers
        const colHeaders = [
            { x: 0, label: 'Users' },
            { x: width / 2, label: 'Modules' },
            { x: width, label: 'Operations' }
        ];
        for (const col of colHeaders) {
            svg.append('text')
                .attr('x', col.x)
                .attr('y', -2)
                .attr('text-anchor', col.x === 0 ? 'start' : col.x === width ? 'end' : 'middle')
                .attr('fill', 'var(--bs-secondary-color)')
                .attr('font-size', '9px')
                .attr('font-weight', '600')
                .attr('text-transform', 'uppercase')
                .attr('opacity', 0.6)
                .text(col.label);
        }
    }


    //
    // Heatmap summary computed properties
    //
    get busiestDay(): { date: string; count: number } | null {
        if (!this.data?.activityByDay?.length) return null;
        return this.data.activityByDay.reduce((max, d) => d.count > max.count ? d : max, this.data.activityByDay[0]);
    }

    get activeDaysCount(): number {
        return this.data?.activityByDay?.filter(d => d.count > 0).length || 0;
    }

    get totalDaysInRange(): number {
        if (!this.data?.summary) return 0;
        const start = new Date(this.data.summary.startDate);
        const end = new Date(this.data.summary.endDate);
        return Math.round((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24)) + 1;
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


    //
    // Drill-down methods
    //
    drillIntoModule(moduleName: string): void {
        this.drillModule = moduleName;
        setTimeout(() => this.renderAllCharts(), 50);
    }

    clearDrill(): void {
        this.drillModule = null;
        setTimeout(() => this.renderAllCharts(), 50);
    }

    get breadcrumbs(): string[] {
        const crumbs = ['All Modules'];
        if (this.drillModule) crumbs.push(this.drillModule);
        return crumbs;
    }

    /** Entities within the currently drilled module */
    get drillEntities(): ModuleEntityDetail[] {
        if (!this.filteredData || !this.drillModule) return [];
        return this.filteredData.moduleEntityBreakdown
            .filter(e => e.moduleName === this.drillModule)
            .sort((a, b) => b.eventCount - a.eventCount);
    }


    //
    // 6. Treemap — module → entity hierarchy (sized by event count)
    //
    private renderTreemap(): void {
        if (!this.treemapChartRef || !this.filteredData?.moduleEntityBreakdown?.length) return;

        const container = this.treemapChartRef.nativeElement;
        d3.select(container).selectAll('*').remove();

        const width = container.clientWidth || 600;
        const height = 320;

        // Build hierarchy data
        const entityData = this.filteredData!.moduleEntityBreakdown;
        const moduleGroups = new Map<string, { name: string; children: { name: string; value: number }[] }>();

        // If drilled, only show entities within that module
        const filtered = this.drillModule
            ? entityData.filter(e => e.moduleName === this.drillModule)
            : entityData;

        for (const entity of filtered) {
            if (!moduleGroups.has(entity.moduleName)) {
                moduleGroups.set(entity.moduleName, { name: entity.moduleName, children: [] });
            }
            moduleGroups.get(entity.moduleName)!.children.push({
                name: entity.entityName,
                value: entity.eventCount
            });
        }

        const root = d3.hierarchy({ name: 'root', children: Array.from(moduleGroups.values()) })
            .sum((d: any) => d.value || 0)
            .sort((a, b) => (b.value || 0) - (a.value || 0));

        d3.treemap()
            .size([width, height])
            .padding(2)
            .paddingTop(18)
            .round(true)
            (root as unknown as d3.HierarchyNode<unknown>);

        const svg = d3.select(container)
            .append('svg')
            .attr('width', width)
            .attr('height', height);

        // Colors per module
        const moduleColorPalette = [
            '#6f42c1', '#20c997', '#0dcaf0', '#fd7e14', '#d63384',
            '#198754', '#ffc107', '#0d6efd', '#6610f2', '#dc3545'
        ];
        const moduleNames = Array.from(moduleGroups.keys());
        const moduleColor = d3.scaleOrdinal<string>().domain(moduleNames).range(moduleColorPalette);

        const tooltip = d3.select(container)
            .append('div')
            .attr('class', 'insights-tooltip')
            .style('opacity', 0);

        // Module group headers
        const groups = svg.selectAll('.treemap-group')
            .data(root.children || [])
            .enter()
            .append('g')
            .attr('class', 'treemap-group');

        groups.append('rect')
            .attr('x', (d: any) => d.x0)
            .attr('y', (d: any) => d.y0)
            .attr('width', (d: any) => d.x1 - d.x0)
            .attr('height', (d: any) => d.y1 - d.y0)
            .attr('fill', 'none')
            .attr('stroke', (d: any) => moduleColor(d.data.name))
            .attr('stroke-width', 1.5)
            .attr('stroke-opacity', 0.6)
            .attr('rx', 4);

        groups.append('text')
            .attr('x', (d: any) => d.x0 + 4)
            .attr('y', (d: any) => d.y0 + 13)
            .attr('fill', 'var(--bs-body-color)')
            .attr('font-size', '11px')
            .attr('font-weight', '600')
            .attr('opacity', 0.8)
            .text((d: any) => d.data.name)
            .each(function (d: any) {
                const availWidth = d.x1 - d.x0 - 8;
                const textEl = d3.select(this);
                if ((textEl.node() as SVGTextElement).getComputedTextLength() > availWidth) {
                    const text = d.data.name;
                    for (let len = text.length; len > 0; len--) {
                        textEl.text(text.substring(0, len) + '…');
                        if ((textEl.node() as SVGTextElement).getComputedTextLength() <= availWidth) break;
                    }
                }
            });

        // Entity cells (leaves)
        const leaves = svg.selectAll('.treemap-cell')
            .data(root.leaves())
            .enter()
            .append('g')
            .attr('class', 'treemap-cell')
            .style('cursor', this.drillModule ? 'default' : 'pointer');

        leaves.append('rect')
            .attr('x', (d: any) => d.x0)
            .attr('y', (d: any) => d.y0)
            .attr('width', (d: any) => Math.max(0, d.x1 - d.x0))
            .attr('height', (d: any) => Math.max(0, d.y1 - d.y0))
            .attr('fill', (d: any) => {
                const modName = d.parent?.data?.name || '';
                const baseColor = d3.color(moduleColor(modName));
                return baseColor ? baseColor.brighter(0.5).toString() : '#adb5bd';
            })
            .attr('rx', 3)
            .attr('opacity', 0)
            .on('mouseover', function (event: any, d: any) {
                d3.select(this).attr('opacity', 1).attr('stroke', 'var(--bs-body-color)').attr('stroke-width', 1.5);
                const modName = d.parent?.data?.name || '';
                tooltip.style('opacity', 1)
                    .html(`<strong>${modName} / ${d.data.name}</strong><br/>${(d.value || 0).toLocaleString()} events`)
                    .style('left', (event.offsetX + 10) + 'px')
                    .style('top', (event.offsetY - 20) + 'px');
            })
            .on('mouseout', function () {
                d3.select(this).attr('opacity', 0.75).attr('stroke', 'none');
                tooltip.style('opacity', 0);
            })
            .on('click', (_event: any, d: any) => {
                if (!this.drillModule) {
                    const modName = d.parent?.data?.name || '';
                    if (modName) this.drillIntoModule(modName);
                }
            })
            .transition()
            .duration(600)
            .delay((_d: any, i: number) => i * 15)
            .attr('opacity', 0.75);

        // Cell labels (only if cell is big enough)
        leaves.append('text')
            .attr('x', (d: any) => d.x0 + 4)
            .attr('y', (d: any) => d.y0 + 13)
            .attr('fill', 'var(--bs-body-color)')
            .attr('font-size', '10px')
            .attr('opacity', 0.8)
            .text((d: any) => d.data.name)
            .each(function (d: any) {
                const cellWidth = (d.x1 - d.x0) - 8;
                const cellHeight = (d.y1 - d.y0);
                if (cellHeight < 16 || cellWidth < 20) {
                    d3.select(this).remove();
                    return;
                }
                const textEl = d3.select(this);
                if ((textEl.node() as SVGTextElement).getComputedTextLength() > cellWidth) {
                    const text = d.data.name;
                    for (let len = text.length; len > 0; len--) {
                        textEl.text(text.substring(0, len) + '…');
                        if ((textEl.node() as SVGTextElement).getComputedTextLength() <= cellWidth) break;
                    }
                }
            });
    }
}
