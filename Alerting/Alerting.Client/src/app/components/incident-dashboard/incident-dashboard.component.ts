import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { IncidentService, IncidentData, IncidentQueryParameters } from '../../alerting-data-services/incident.service';
import { IncidentStatusTypeService, IncidentStatusTypeData } from '../../alerting-data-services/incident-status-type.service';
import { SeverityTypeService, SeverityTypeData } from '../../alerting-data-services/severity-type.service';
import { ServiceService, ServiceData } from '../../alerting-data-services/service.service';
import { AlertService } from '../../services/alert.service';
import { NavigationService } from '../../utility-services/navigation.service';

/**
 * Status tab definition
 */
interface StatusTab {
    id: string;
    name: string;
    statusTypeId: number | null;
    count: number;
    icon: string;
    colorClass: string;
}

/**
 * Incident Dashboard Component
 * 
 * Command-center style dashboard for managing incidents with:
 * - Status tabs (Triggered, Acknowledged, Resolved, All)
 * - Severity filtering
 * - Service filtering
 * - Search functionality
 * - Quick actions (Acknowledge, Resolve)
 */
@Component({
    selector: 'app-incident-dashboard',
    templateUrl: './incident-dashboard.component.html',
    styleUrls: ['./incident-dashboard.component.scss']
})
export class IncidentDashboardComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // Loading state
    isLoading = true;
    errorMessage: string | null = null;

    // Data
    incidents: IncidentData[] = [];
    statusTypes: IncidentStatusTypeData[] = [];
    severityTypes: SeverityTypeData[] = [];
    services: ServiceData[] = [];

    // Status tabs
    statusTabs: StatusTab[] = [
        { id: 'all', name: 'All Incidents', statusTypeId: null, count: 0, icon: 'fa-list', colorClass: 'all' },
        { id: 'triggered', name: 'Triggered', statusTypeId: 1, count: 0, icon: 'fa-bolt', colorClass: 'triggered' },
        { id: 'acknowledged', name: 'Acknowledged', statusTypeId: 2, count: 0, icon: 'fa-check', colorClass: 'acknowledged' },
        { id: 'resolved', name: 'Resolved', statusTypeId: 3, count: 0, icon: 'fa-check-double', colorClass: 'resolved' }
    ];
    activeTab: StatusTab;

    // Filters
    selectedSeverityId: number | null = null;
    selectedServiceId: number | null = null;
    searchQuery: string = '';

    // Pagination
    pageSize: number = 20;
    currentPage: number = 1;
    totalCount: number = 0;

    constructor(
        private router: Router,
        private incidentService: IncidentService,
        private incidentStatusTypeService: IncidentStatusTypeService,
        private severityTypeService: SeverityTypeService,
        private serviceService: ServiceService,
        private alertService: AlertService,
        private navigationService: NavigationService
    ) {
        this.activeTab = this.statusTabs[1]; // Default to Triggered
    }

    ngOnInit(): void {
        this.loadInitialData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    /**
     * Load all initial data (lookups + incidents)
     */
    private loadInitialData(): void {
        this.isLoading = true;
        this.errorMessage = null;

        forkJoin({
            statusTypes: this.incidentStatusTypeService.GetIncidentStatusTypeList({ active: true }),
            severityTypes: this.severityTypeService.GetSeverityTypeList({ active: true }),
            services: this.serviceService.GetServiceList({ active: true })
        }).pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.statusTypes = result.statusTypes;
                    this.severityTypes = result.severityTypes;
                    this.services = result.services;

                    // Update status tabs with actual IDs from database
                    this.updateStatusTabIds();

                    // Load incidents
                    this.loadIncidents();
                    this.loadTabCounts();
                },
                error: (error) => {
                    this.errorMessage = 'Failed to load dashboard data';
                    this.alertService.showHttpErrorMessage('Error', error);
                    this.isLoading = false;
                }
            });
    }

    /**
     * Update status tab IDs based on actual database values
     */
    private updateStatusTabIds(): void {
        for (const statusType of this.statusTypes) {
            const tab = this.statusTabs.find(t =>
                t.id.toLowerCase() === statusType.name.toLowerCase()
            );
            if (tab) {
                tab.statusTypeId = statusType.id as number;
            }
        }
    }

    /**
     * Load incidents based on current filters
     */
    loadIncidents(): void {
        this.isLoading = true;

        const params = new IncidentQueryParameters();
        params.active = true;
        params.pageSize = this.pageSize;
        params.pageNumber = this.currentPage;
        params.includeRelations = true;

        // Apply status filter
        if (this.activeTab.statusTypeId !== null) {
            params.incidentStatusTypeId = this.activeTab.statusTypeId;
        }

        // Apply severity filter
        if (this.selectedSeverityId !== null) {
            params.severityTypeId = this.selectedSeverityId;
        }

        // Apply service filter
        if (this.selectedServiceId !== null) {
            params.serviceId = this.selectedServiceId;
        }

        // Apply search
        if (this.searchQuery.trim()) {
            params.anyStringContains = this.searchQuery.trim();
        }

        this.incidentService.GetIncidentList(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (incidents) => {
                    this.incidents = incidents;
                    this.isLoading = false;
                },
                error: (error) => {
                    this.errorMessage = 'Failed to load incidents';
                    this.alertService.showHttpErrorMessage('Error', error);
                    this.isLoading = false;
                }
            });

        // Get total count for pagination
        this.incidentService.GetIncidentsRowCount(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (count: number | bigint) => {
                    this.totalCount = count as number;
                },
                error: () => { /* ignore count errors */ }
            });
    }

    /**
     * Load counts for each status tab
     */
    private loadTabCounts(): void {
        for (const tab of this.statusTabs) {
            const params = new IncidentQueryParameters();
            params.active = true;
            if (tab.statusTypeId !== null) {
                params.incidentStatusTypeId = tab.statusTypeId;
            }

            this.incidentService.GetIncidentsRowCount(params)
                .pipe(takeUntil(this.destroy$))
                .subscribe({
                    next: (count: number | bigint) => {
                        tab.count = count as number;
                    },
                    error: () => { /* ignore */ }
                });
        }
    }

    /**
     * Switch to a different status tab
     */
    selectTab(tab: StatusTab): void {
        this.activeTab = tab;
        this.currentPage = 1;
        this.loadIncidents();
    }

    /**
     * Apply severity filter
     */
    onSeverityFilterChange(): void {
        this.currentPage = 1;
        this.loadIncidents();
    }

    /**
     * Apply service filter
     */
    onServiceFilterChange(): void {
        this.currentPage = 1;
        this.loadIncidents();
    }

    /**
     * Apply search filter
     */
    onSearch(): void {
        this.currentPage = 1;
        this.loadIncidents();
    }

    /**
     * Clear all filters
     */
    clearFilters(): void {
        this.selectedSeverityId = null;
        this.selectedServiceId = null;
        this.searchQuery = '';
        this.currentPage = 1;
        this.loadIncidents();
    }

    /**
     * Navigate to incident detail
     */
    viewIncident(incident: IncidentData): void {
        this.router.navigate(['/incidents', incident.id]);
    }

    /**
     * Quick acknowledge action
     */
    async acknowledgeIncident(incident: IncidentData, event: Event): Promise<void> {
        event.stopPropagation();

        try {
            const submitData = incident.ConvertToSubmitData();
            submitData.incidentStatusTypeId = this.getStatusTypeIdByName('Acknowledged') as number;
            submitData.acknowledgedAt = new Date().toISOString();

            await this.incidentService.PutIncident(incident.id as number, submitData).toPromise();

            this.alertService.showSuccessMessage('Incident acknowledged', null);
            this.loadIncidents();
            this.loadTabCounts();
        } catch (error: any) {
            this.alertService.showHttpErrorMessage('Failed to acknowledge incident', error);
        }
    }

    /**
     * Quick resolve action
     */
    async resolveIncident(incident: IncidentData, event: Event): Promise<void> {
        event.stopPropagation();

        try {
            const submitData = incident.ConvertToSubmitData();
            submitData.incidentStatusTypeId = this.getStatusTypeIdByName('Resolved') as number;
            submitData.resolvedAt = new Date().toISOString();

            await this.incidentService.PutIncident(incident.id as number, submitData).toPromise();

            this.alertService.showSuccessMessage('Incident resolved', null);
            this.loadIncidents();
            this.loadTabCounts();
        } catch (error: any) {
            this.alertService.showHttpErrorMessage('Failed to resolve incident', error);
        }
    }

    /**
     * Get status type ID by name
     */
    private getStatusTypeIdByName(name: string): number | null {
        const statusType = this.statusTypes.find(s => s.name === name);
        return statusType ? statusType.id as number : null;
    }

    /**
     * Get severity color class
     */
    getSeverityClass(incident: IncidentData): string {
        const severityName = this.getSeverityName(incident);
        switch (severityName.toLowerCase()) {
            case 'critical': return 'severity-critical';
            case 'high': return 'severity-high';
            case 'medium': return 'severity-medium';
            case 'low': return 'severity-low';
            default: return 'severity-low';
        }
    }

    /**
     * Get severity name for incident
     */
    getSeverityName(incident: IncidentData): string {
        const severity = this.severityTypes.find(s => s.id === incident.severityTypeId);
        return severity?.name || 'Unknown';
    }

    /**
     * Get status name for incident
     */
    getStatusName(incident: IncidentData): string {
        const status = this.statusTypes.find(s => s.id === incident.incidentStatusTypeId);
        return status?.name || 'Unknown';
    }

    /**
     * Get status class for incident
     */
    getStatusClass(incident: IncidentData): string {
        const statusName = this.getStatusName(incident);
        switch (statusName.toLowerCase()) {
            case 'triggered': return 'status-triggered';
            case 'acknowledged': return 'status-acknowledged';
            case 'resolved': return 'status-resolved';
            default: return 'status-triggered';
        }
    }

    /**
     * Get service name for incident
     */
    getServiceName(incident: IncidentData): string {
        const service = this.services.find(s => s.id === incident.serviceId);
        return service?.name || 'Unknown Service';
    }

    /**
     * Format relative time
     */
    getRelativeTime(dateString: string): string {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMins / 60);
        const diffDays = Math.floor(diffHours / 24);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;
        return date.toLocaleDateString();
    }

    /**
     * Check if incident is triggered (for pulsing animation)
     */
    isTriggered(incident: IncidentData): boolean {
        return this.getStatusName(incident).toLowerCase() === 'triggered';
    }

    /**
     * Check if there are any active filters
     */
    hasActiveFilters(): boolean {
        return this.selectedSeverityId !== null ||
            this.selectedServiceId !== null ||
            this.searchQuery.trim().length > 0;
    }

    /**
     * Pagination - go to page
     */
    goToPage(page: number): void {
        if (page < 1 || page > this.totalPages) return;
        this.currentPage = page;
        this.loadIncidents();
    }

    /**
     * Get total pages
     */
    get totalPages(): number {
        return Math.ceil(this.totalCount / this.pageSize);
    }

    /**
     * Get page numbers for pagination
     */
    get pageNumbers(): number[] {
        const pages: number[] = [];
        const maxPages = 5;
        let start = Math.max(1, this.currentPage - Math.floor(maxPages / 2));
        const end = Math.min(this.totalPages, start + maxPages - 1);
        start = Math.max(1, end - maxPages + 1);

        for (let i = start; i <= end; i++) {
            pages.push(i);
        }
        return pages;
    }

    /**
     * Refresh data
     */
    refresh(): void {
        this.loadIncidents();
        this.loadTabCounts();
    }

    /**
     * Navigate back
     */
    goBack(): void {
        this.navigationService.goBack();
    }

    canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }
}
