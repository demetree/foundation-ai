import { Component, OnInit, OnDestroy } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NgbDateStruct, NgbCalendar } from '@ng-bootstrap/ng-bootstrap';

import { AuditEventService, AuditEventData, AuditEventQueryParameters } from '../../../auditor-data-services/audit-event.service';
import { AuditTypeService, AuditTypeData } from '../../../auditor-data-services/audit-type.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';

//
// Time preset options
//
interface TimePreset {
    label: string;
    value: string;
    getStartDate: () => Date;
}

@Component({
    selector: 'app-audit-event-custom-listing',
    templateUrl: './audit-event-custom-listing.component.html',
    styleUrls: ['./audit-event-custom-listing.component.scss']
})
export class AuditEventCustomListingComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    //
    // Data
    //
    auditEvents$ = new BehaviorSubject<AuditEventData[]>([]);
    auditTypes$ = new BehaviorSubject<AuditTypeData[]>([]);
    isLoading$ = new BehaviorSubject<boolean>(true);
    totalCount$ = new BehaviorSubject<number>(0);

    //
    // Filters
    //
    startDate: NgbDateStruct | null = null;
    endDate: NgbDateStruct | null = null;
    selectedTimePreset: string = 'last24hours';
    statusFilter: 'all' | 'success' | 'failure' = 'all';
    typeFilter: number | null = null;
    userFilter: string = '';
    searchText: string = '';

    //
    // Pagination
    //
    pageSize: number = 50;
    currentPage: number = 1;

    //
    // Expansion
    //
    expandedEventIds: Set<number | bigint> = new Set();

    //
    // Make Math available in template
    //
    Math = Math;

    //
    // Time presets
    //
    timePresets: TimePreset[] = [
        { label: 'Last Hour', value: 'lasthour', getStartDate: () => new Date(Date.now() - 60 * 60 * 1000) },
        { label: 'Today', value: 'today', getStartDate: () => { const d = new Date(); d.setHours(0, 0, 0, 0); return d; } },
        { label: 'Last 24 Hours', value: 'last24hours', getStartDate: () => new Date(Date.now() - 24 * 60 * 60 * 1000) },
        { label: 'Last Week', value: 'lastweek', getStartDate: () => new Date(Date.now() - 7 * 24 * 60 * 60 * 1000) },
        { label: 'Last 30 Days', value: 'last30days', getStartDate: () => new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) },
        { label: 'Custom', value: 'custom', getStartDate: () => new Date(Date.now() - 24 * 60 * 60 * 1000) }
    ];

    constructor(
        private auditEventService: AuditEventService,
        private auditTypeService: AuditTypeService,
        private alertService: AlertService,
        private authService: AuthService,
        private calendar: NgbCalendar
    ) { }

    ngOnInit(): void {
        this.loadAuditTypes();
        this.applyFilters();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    //
    // Load audit types for filter dropdown
    //
    private loadAuditTypes(): void {
        this.auditTypeService.GetAuditTypeList({}).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (types) => {
                this.auditTypes$.next(types);
            },
            error: (err) => {
                console.error('Failed to load audit types', err);
            }
        });
    }

    //
    // Apply filters and load data
    //
    applyFilters(): void {
        this.isLoading$.next(true);
        this.currentPage = 1;

        const params = this.buildQueryParams();

        this.auditEventService.ClearAllCaches();
        this.auditEventService.GetAuditEventList(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (events) => {
                this.auditEvents$.next(events);
                this.totalCount$.next(events.length);
                this.isLoading$.next(false);
            },
            error: (err) => {
                this.alertService.showMessage('Error loading audit events', err.message || 'Unknown error', MessageSeverity.error);
                this.auditEvents$.next([]);
                this.isLoading$.next(false);
            }
        });
    }

    //
    // Build query parameters based on current filters
    //
    private buildQueryParams(): Partial<AuditEventQueryParameters> {
        const params: Partial<AuditEventQueryParameters> = {
            includeRelations: true,
            pageSize: 500  // Large page to get all, pagination is client-side for now
        };

        //
        // Date filter
        //
        const startDate = this.getFilterStartDate();
        if (startDate) {
            params.startTime = startDate.toISOString();
        }

        if (this.selectedTimePreset === 'custom' && this.endDate) {
            const end = new Date(this.endDate.year, this.endDate.month - 1, this.endDate.day, 23, 59, 59);
            // Note: Server might need an end date parameter - using startTime for now
        }

        //
        // Status filter
        //
        if (this.statusFilter === 'success') {
            params.completedSuccessfully = true;
        } else if (this.statusFilter === 'failure') {
            params.completedSuccessfully = false;
        }

        //
        // Type filter
        //
        if (this.typeFilter) {
            params.auditTypeId = this.typeFilter;
        }

        //
        // User filter
        //
        if (this.userFilter && this.userFilter.trim()) {
            (params as any).userName = this.userFilter.trim();
        }

        //
        // Text search
        //
        if (this.searchText && this.searchText.trim()) {
            params.anyStringContains = this.searchText.trim();
        }

        return params;
    }

    //
    // Get start date based on preset or custom date
    //
    private getFilterStartDate(): Date | null {
        if (this.selectedTimePreset === 'custom') {
            if (this.startDate) {
                return new Date(this.startDate.year, this.startDate.month - 1, this.startDate.day);
            }
            return null;
        }

        const preset = this.timePresets.find(p => p.value === this.selectedTimePreset);
        return preset ? preset.getStartDate() : null;
    }

    //
    // Handle time preset change
    //
    onTimePresetChange(): void {
        if (this.selectedTimePreset !== 'custom') {
            this.startDate = null;
            this.endDate = null;
        }
        this.applyFilters();
    }

    //
    // Clear all filters
    //
    clearFilters(): void {
        this.selectedTimePreset = 'last24hours';
        this.startDate = null;
        this.endDate = null;
        this.statusFilter = 'all';
        this.typeFilter = null;
        this.userFilter = '';
        this.searchText = '';
        this.applyFilters();
    }

    //
    // Toggle row expansion
    //
    toggleExpand(event: AuditEventData): void {
        if (this.expandedEventIds.has(event.id)) {
            this.expandedEventIds.delete(event.id);
        } else {
            this.expandedEventIds.add(event.id);
        }
    }

    isExpanded(event: AuditEventData): boolean {
        return this.expandedEventIds.has(event.id);
    }

    //
    // Format helpers
    //
    formatRelativeTime(isoString: string): string {
        if (!isoString) return '—';

        const date = new Date(isoString);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffSec = Math.floor(diffMs / 1000);
        const diffMin = Math.floor(diffSec / 60);
        const diffHours = Math.floor(diffMin / 60);
        const diffDays = Math.floor(diffHours / 24);

        if (diffSec < 60) return 'Just now';
        if (diffMin < 60) return `${diffMin}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;

        return date.toLocaleDateString();
    }

    formatFullDateTime(isoString: string): string {
        if (!isoString) return '';
        const date = new Date(isoString);
        return date.toLocaleString();
    }

    formatDuration(startTime: string, stopTime: string): string {
        if (!startTime || !stopTime) return '—';

        const start = new Date(startTime);
        const stop = new Date(stopTime);
        const diffMs = stop.getTime() - start.getTime();

        if (diffMs < 0) return '—';
        if (diffMs < 1000) return `${diffMs}ms`;
        if (diffMs < 60000) return `${(diffMs / 1000).toFixed(1)}s`;
        return `${(diffMs / 60000).toFixed(1)}m`;
    }

    truncateMessage(message: string, maxLength: number = 80): string {
        if (!message) return '—';
        if (message.length <= maxLength) return message;
        return message.substring(0, maxLength) + '...';
    }

    //
    // Pagination
    //
    get pagedEvents(): AuditEventData[] {
        const events = this.auditEvents$.value;
        const start = (this.currentPage - 1) * this.pageSize;
        return events.slice(start, start + this.pageSize);
    }

    get totalPages(): number {
        return Math.ceil(this.auditEvents$.value.length / this.pageSize);
    }

    goToPage(page: number): void {
        if (page >= 1 && page <= this.totalPages) {
            this.currentPage = page;
        }
    }

    //
    // Track by function for ngFor
    //
    trackByEventId(index: number, event: AuditEventData): number | bigint {
        return event.id;
    }
}
