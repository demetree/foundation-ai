import { Component, OnInit, OnDestroy } from '@angular/core';
import { Location } from '@angular/common';
import { BehaviorSubject, Subject, interval, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NgbDateStruct, NgbCalendar } from '@ng-bootstrap/ng-bootstrap';

import { AuditEventService, AuditEventData, AuditEventQueryParameters } from '../../../auditor-data-services/audit-event.service';
import { AuditEventErrorMessageService, AuditEventErrorMessageData } from '../../../auditor-data-services/audit-event-error-message.service';
import { AuditTypeService, AuditTypeData } from '../../../auditor-data-services/audit-type.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { UserPreferencesService } from '../../../services/user-preferences.service';

//
// Preferences key for this component
//
const PREFS_KEY = 'auditEventsFilters';

//
// Filter preferences interface
//
interface AuditEventFilterPreferences {
    selectedTimePreset: string;
    statusFilter: 'all' | 'success' | 'failure';
    typeFilter: number | null;
    userFilter: string;
    searchText: string;
    autoRefreshEnabled: boolean;
    autoRefreshInterval: number;
}

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
    // Error messages cache (by event id)
    //
    errorMessagesCache: Map<number | bigint, AuditEventErrorMessageData[]> = new Map();
    loadingErrorMessages: Set<number | bigint> = new Set();

    //
    // Make Math available in template
    //
    Math = Math;

    //
    // Failure count for header badge
    //
    failureCount: number = 0;

    //
    // Sorting
    //
    sortColumn: string = 'startTime';
    sortDirection: 'asc' | 'desc' = 'desc';

    //
    // Auto-refresh
    //
    autoRefreshEnabled: boolean = false;
    autoRefreshInterval: number = 30; // seconds
    autoRefreshOptions: number[] = [10, 30, 60, 120, 300];
    private autoRefreshSubscription: Subscription | null = null;
    nextRefreshIn: number = 0;
    private countdownSubscription: Subscription | null = null;

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
        private location: Location,
        private auditEventService: AuditEventService,
        private auditEventErrorMessageService: AuditEventErrorMessageService,
        private auditTypeService: AuditTypeService,
        private alertService: AlertService,
        private authService: AuthService,
        private userPreferencesService: UserPreferencesService,
        private calendar: NgbCalendar
    ) { }

    ngOnInit(): void {
        this.loadAuditTypes();
        this.loadSavedFilters();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.stopAutoRefresh();
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
    // Load saved filter preferences
    //
    private async loadSavedFilters(): Promise<void> {
        const defaults: AuditEventFilterPreferences = {
            selectedTimePreset: 'last24hours',
            statusFilter: 'all',
            typeFilter: null,
            userFilter: '',
            searchText: '',
            autoRefreshEnabled: false,
            autoRefreshInterval: 30
        };

        const prefs = await this.userPreferencesService.getPreference<AuditEventFilterPreferences>(PREFS_KEY, defaults);

        this.selectedTimePreset = prefs.selectedTimePreset;
        this.statusFilter = prefs.statusFilter;
        this.typeFilter = prefs.typeFilter;
        this.userFilter = prefs.userFilter;
        this.searchText = prefs.searchText;
        this.autoRefreshEnabled = prefs.autoRefreshEnabled;
        this.autoRefreshInterval = prefs.autoRefreshInterval;

        // Apply the loaded filters
        this.applyFilters();

        // Start auto-refresh if it was enabled
        if (this.autoRefreshEnabled) {
            this.startAutoRefresh();
        }
    }

    //
    // Save current filter preferences
    //
    private async saveFilterPreferences(): Promise<void> {
        const prefs: AuditEventFilterPreferences = {
            selectedTimePreset: this.selectedTimePreset,
            statusFilter: this.statusFilter,
            typeFilter: this.typeFilter,
            userFilter: this.userFilter,
            searchText: this.searchText,
            autoRefreshEnabled: this.autoRefreshEnabled,
            autoRefreshInterval: this.autoRefreshInterval
        };

        await this.userPreferencesService.setPreference(PREFS_KEY, prefs);
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
                // Calculate failure count
                this.failureCount = events.filter(e => !e.completedSuccessfully).length;

                // Apply initial sort
                const sortedEvents = this.sortEvents(events);
                this.auditEvents$.next(sortedEvents);
                this.totalCount$.next(events.length);
                this.isLoading$.next(false);
            },
            error: (err) => {
                this.alertService.showMessage('Error loading audit events', err.message || 'Unknown error', MessageSeverity.error);
                this.auditEvents$.next([]);
                this.failureCount = 0;
                this.isLoading$.next(false);
            }
        });

        // Save preferences after applying filters
        this.saveFilterPreferences();
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
            // Auto-load error messages for failed events
            if (!event.completedSuccessfully && !this.errorMessagesCache.has(event.id)) {
                this.loadErrorMessages(event);
            }
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

    //
    // Sorting
    //
    sortBy(column: string): void {
        if (this.sortColumn === column) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = column;
            this.sortDirection = 'desc';
        }

        const sortedEvents = this.sortEvents(this.auditEvents$.value);
        this.auditEvents$.next(sortedEvents);
    }

    private sortEvents(events: AuditEventData[]): AuditEventData[] {
        return [...events].sort((a, b) => {
            let aVal: any;
            let bVal: any;

            switch (this.sortColumn) {
                case 'startTime':
                    aVal = new Date(a.startTime).getTime();
                    bVal = new Date(b.startTime).getTime();
                    break;
                case 'user':
                    aVal = a.auditUser?.name?.toLowerCase() || '';
                    bVal = b.auditUser?.name?.toLowerCase() || '';
                    break;
                case 'type':
                    aVal = a.auditType?.name?.toLowerCase() || '';
                    bVal = b.auditType?.name?.toLowerCase() || '';
                    break;
                case 'status':
                    aVal = a.completedSuccessfully ? 1 : 0;
                    bVal = b.completedSuccessfully ? 1 : 0;
                    break;
                case 'module':
                    aVal = a.auditModule?.name?.toLowerCase() || '';
                    bVal = b.auditModule?.name?.toLowerCase() || '';
                    break;
                default:
                    aVal = (a as any)[this.sortColumn];
                    bVal = (b as any)[this.sortColumn];
            }

            if (aVal < bVal) return this.sortDirection === 'asc' ? -1 : 1;
            if (aVal > bVal) return this.sortDirection === 'asc' ? 1 : -1;
            return 0;
        });
    }

    getSortIcon(column: string): string {
        if (this.sortColumn !== column) return 'fa-sort';
        return this.sortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    }

    //
    // Error message loading for failed events
    //
    loadErrorMessages(event: AuditEventData): void {
        if (this.loadingErrorMessages.has(event.id)) return;

        this.loadingErrorMessages.add(event.id);

        this.auditEventErrorMessageService.GetAuditEventErrorMessageList({
            auditEventId: event.id
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (messages) => {
                this.errorMessagesCache.set(event.id, messages);
                this.loadingErrorMessages.delete(event.id);
            },
            error: (err) => {
                console.error('Failed to load error messages', err);
                this.errorMessagesCache.set(event.id, []);
                this.loadingErrorMessages.delete(event.id);
            }
        });
    }

    getErrorMessages(event: AuditEventData): AuditEventErrorMessageData[] {
        return this.errorMessagesCache.get(event.id) || [];
    }

    isLoadingErrorMessages(event: AuditEventData): boolean {
        return this.loadingErrorMessages.has(event.id);
    }

    //
    // Auto-refresh functionality
    //
    toggleAutoRefresh(): void {
        this.autoRefreshEnabled = !this.autoRefreshEnabled;

        if (this.autoRefreshEnabled) {
            this.startAutoRefresh();
        } else {
            this.stopAutoRefresh();
        }

        this.saveFilterPreferences();
    }

    setAutoRefreshInterval(seconds: number): void {
        this.autoRefreshInterval = seconds;

        if (this.autoRefreshEnabled) {
            this.stopAutoRefresh();
            this.startAutoRefresh();
        }

        this.saveFilterPreferences();
    }

    private startAutoRefresh(): void {
        this.stopAutoRefresh();

        this.nextRefreshIn = this.autoRefreshInterval;

        // Countdown timer (every second)
        this.countdownSubscription = interval(1000).subscribe(() => {
            this.nextRefreshIn--;
            if (this.nextRefreshIn <= 0) {
                this.nextRefreshIn = this.autoRefreshInterval;
            }
        });

        // Actual refresh
        this.autoRefreshSubscription = interval(this.autoRefreshInterval * 1000).subscribe(() => {
            this.applyFilters();
            this.nextRefreshIn = this.autoRefreshInterval;
        });
    }

    private stopAutoRefresh(): void {
        if (this.autoRefreshSubscription) {
            this.autoRefreshSubscription.unsubscribe();
            this.autoRefreshSubscription = null;
        }
        if (this.countdownSubscription) {
            this.countdownSubscription.unsubscribe();
            this.countdownSubscription = null;
        }
        this.nextRefreshIn = 0;
    }

    formatAutoRefreshInterval(seconds: number): string {
        if (seconds < 60) return `${seconds}s`;
        return `${seconds / 60}m`;
    }

    //
    // Export to CSV
    //
    exportToCsv(): void {
        const events = this.auditEvents$.value;
        if (events.length === 0) {
            this.alertService.showMessage('Export', 'No events to export', MessageSeverity.info);
            return;
        }

        const headers = [
            'ID', 'Time', 'Status', 'User', 'Type', 'Access', 'Module',
            'Message', 'Duration (ms)', 'Session', 'Source', 'Resource', 'Primary Key'
        ];

        const rows = events.map(e => [
            e.id,
            e.startTime ? new Date(e.startTime).toISOString() : '',
            e.completedSuccessfully ? 'Success' : 'Failure',
            e.auditUser?.name || '',
            e.auditType?.name || '',
            e.auditAccessType?.name || '',
            e.auditModule?.name || '',
            (e.message || '').replace(/"/g, '""'),
            e.startTime && e.stopTime ? new Date(e.stopTime).getTime() - new Date(e.startTime).getTime() : '',
            e.auditSession?.name || '',
            e.auditSource?.name || '',
            e.auditResource?.name || '',
            e.primaryKey || ''
        ]);

        const csvContent = [
            headers.join(','),
            ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
        ].join('\n');

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.setAttribute('href', url);
        link.setAttribute('download', `audit-events-${new Date().toISOString().split('T')[0]}.csv`);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);

        this.alertService.showMessage('Export', `Exported ${events.length} events to CSV`, MessageSeverity.success);
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
