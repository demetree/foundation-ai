//
// Login Attempt Custom Listing Component
//
// Premium listing for login attempts with filtering, sorting, pagination, and auto-refresh.
// Inspired by audit-event-custom-listing pattern.
// AI-assisted development - January 2026
//

import { Component, OnInit, OnDestroy } from '@angular/core';
import { BehaviorSubject, Subject, interval, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ChartConfiguration, ChartData } from 'chart.js';

import { LoginAttemptService, LoginAttemptData, LoginAttemptQueryParameters } from '../../../security-data-services/login-attempt.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';


//
// Preferences key for this component
//
const PREFS_KEY = 'loginAttemptFilters';


//
// Filter preferences interface
//
interface LoginAttemptFilterPreferences {
    selectedTimePreset: string;
    statusFilter: 'all' | 'success' | 'failure';
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
    selector: 'app-login-attempt-custom-listing',
    templateUrl: './login-attempt-custom-listing.component.html',
    styleUrls: ['./login-attempt-custom-listing.component.scss']
})
export class LoginAttemptCustomListingComponent implements OnInit, OnDestroy {

    //
    // Lifecycle
    //
    private destroy$ = new Subject<void>();

    //
    // Data state
    //
    loginAttempts$ = new BehaviorSubject<LoginAttemptData[]>([]);
    isLoading$ = new BehaviorSubject<boolean>(true);

    //
    // Time presets
    //
    timePresets: TimePreset[] = [
        { label: 'Last 1 Hour', value: '1h', getStartDate: () => { const d = new Date(); d.setHours(d.getHours() - 1); return d; } },
        { label: 'Last 6 Hours', value: '6h', getStartDate: () => { const d = new Date(); d.setHours(d.getHours() - 6); return d; } },
        { label: 'Last 24 Hours', value: '24h', getStartDate: () => { const d = new Date(); d.setDate(d.getDate() - 1); return d; } },
        { label: 'Last 7 Days', value: '7d', getStartDate: () => { const d = new Date(); d.setDate(d.getDate() - 7); return d; } },
        { label: 'Last 30 Days', value: '30d', getStartDate: () => { const d = new Date(); d.setDate(d.getDate() - 30); return d; } },
        { label: 'Custom Range', value: 'custom', getStartDate: () => new Date() }
    ];
    selectedTimePreset: string = '24h';

    //
    // Custom date range (string format for native date input: yyyy-mm-dd)
    //
    customStartDate: string = '';
    customEndDate: string = '';

    //
    // Filters
    //
    statusFilter: 'all' | 'success' | 'failure' = 'all';
    userFilter: string = '';
    searchText: string = '';

    //
    // Sorting
    //
    sortColumn: string = 'timeStamp';
    sortDirection: 'asc' | 'desc' = 'desc';

    //
    // Pagination
    //
    currentPage: number = 1;
    pageSize: number = 50;
    pagedAttempts: LoginAttemptData[] = [];

    //
    // Expansion state
    //
    expandedIds: Set<number> = new Set();

    //
    // Stats
    //
    stats = {
        totalAttempts: 0,
        successCount: 0,
        failureCount: 0,
        successRate: 0,
        uniqueUsers: 0,
        topFailedUsers: [] as { userName: string; count: number }[]
    };

    // Sparkline data (hourly buckets)
    sparklineData: ChartData<'line'> = { labels: [], datasets: [] };
    sparklineOptions: ChartConfiguration<'line'>['options'] = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: { enabled: false } },
        scales: { x: { display: false }, y: { display: false, beginAtZero: true } },
        elements: { point: { radius: 0 }, line: { borderWidth: 2, tension: 0.4 } }
    };

    //
    // Auto-refresh
    //
    autoRefreshEnabled: boolean = false;
    autoRefreshInterval: number = 30;
    autoRefreshOptions: number[] = [15, 30, 60, 120, 300];
    nextRefreshIn: number = 0;
    private autoRefreshSub: Subscription | null = null;
    private countdownSub: Subscription | null = null;

    //
    // Math reference for template
    //
    Math = Math;


    constructor(
        private loginAttemptService: LoginAttemptService,
        private alertService: AlertService,
        private authService: AuthService
    ) { }


    ngOnInit(): void {
        this.loadSavedFilters().then(() => this.applyFilters());
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.stopAutoRefresh();
    }


    //
    // Load saved filter preferences from localStorage
    //
    private async loadSavedFilters(): Promise<void> {
        try {
            const saved = localStorage.getItem(PREFS_KEY);
            if (saved) {
                const prefs = JSON.parse(saved) as LoginAttemptFilterPreferences;
                this.selectedTimePreset = prefs.selectedTimePreset || '24h';
                this.statusFilter = prefs.statusFilter || 'all';
                this.userFilter = prefs.userFilter || '';
                this.searchText = prefs.searchText || '';
                this.autoRefreshEnabled = prefs.autoRefreshEnabled || false;
                this.autoRefreshInterval = prefs.autoRefreshInterval || 30;

                if (this.autoRefreshEnabled) {
                    this.startAutoRefresh();
                }
            }
        } catch (err) {
            console.warn('Failed to load saved filters', err);
        }
    }


    //
    // Save current filter preferences to localStorage
    //
    private saveFilterPreferences(): void {
        const prefs: LoginAttemptFilterPreferences = {
            selectedTimePreset: this.selectedTimePreset,
            statusFilter: this.statusFilter,
            userFilter: this.userFilter,
            searchText: this.searchText,
            autoRefreshEnabled: this.autoRefreshEnabled,
            autoRefreshInterval: this.autoRefreshInterval
        };
        try {
            localStorage.setItem(PREFS_KEY, JSON.stringify(prefs));
        } catch (err) {
            console.warn('Failed to save filter preferences', err);
        }
    }


    //
    // Apply filters and load data
    //
    applyFilters(): void {
        this.isLoading$.next(true);
        this.saveFilterPreferences();

        const params = this.buildQueryParams();

        this.loginAttemptService.ClearAllCaches();
        this.loginAttemptService.GetLoginAttemptList(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (attempts) => {
                let filtered = this.applyClientSideFilters(attempts);
                filtered = this.applySorting(filtered);

                this.loginAttempts$.next(filtered);
                this.computeStats(filtered);
                this.updatePagination();
                this.isLoading$.next(false);
            },
            error: (err) => {
                console.error('Failed to load login attempts', err);
                this.alertService.showMessage('Error', 'Failed to load login attempts', MessageSeverity.error);
                this.isLoading$.next(false);
            }
        });
    }


    //
    // Build query parameters based on current filters
    //
    private buildQueryParams(): Partial<LoginAttemptQueryParameters> {
        const params: Partial<LoginAttemptQueryParameters> = {
            deleted: false,
            includeRelations: false
        };

        // Time filter
        const startDate = this.getFilterStartDate();
        if (startDate) {
            // Note: The service may not support date filtering directly
            // We'll filter client-side if needed
        }

        // Text search
        if (this.searchText?.trim()) {
            params.anyStringContains = this.searchText.trim();
        }

        // User filter
        if (this.userFilter?.trim()) {
            params.userName = this.userFilter.trim();
        }

        return params;
    }


    //
    // Get start date based on preset or custom date
    //
    private getFilterStartDate(): Date | null {
        if (this.selectedTimePreset === 'custom') {
            if (this.customStartDate) {
                return new Date(this.customStartDate);
            }
            return null;
        }

        const preset = this.timePresets.find(p => p.value === this.selectedTimePreset);
        return preset ? preset.getStartDate() : null;
    }


    //
    // Apply client-side filters
    //
    private applyClientSideFilters(attempts: LoginAttemptData[]): LoginAttemptData[] {
        let result = [...attempts];

        // Time filter
        const startDate = this.getFilterStartDate();
        if (startDate) {
            result = result.filter(a => new Date(a.timeStamp) >= startDate);
        }

        // Custom end date
        if (this.selectedTimePreset === 'custom' && this.customEndDate) {
            const end = new Date(this.customEndDate + 'T23:59:59');
            result = result.filter(a => new Date(a.timeStamp) <= end);
        }

        // Status filter
        if (this.statusFilter === 'success') {
            result = result.filter(a => this.isSuccess(a));
        } else if (this.statusFilter === 'failure') {
            result = result.filter(a => !this.isSuccess(a));
        }

        return result;
    }


    //
    // Compute statistics from filtered data
    //
    private computeStats(attempts: LoginAttemptData[]): void {
        const successes = attempts.filter(a => this.isSuccess(a));
        const failures = attempts.filter(a => !this.isSuccess(a));

        // Basic counts
        this.stats.totalAttempts = attempts.length;
        this.stats.successCount = successes.length;
        this.stats.failureCount = failures.length;
        this.stats.successRate = attempts.length > 0
            ? Math.round((successes.length / attempts.length) * 100)
            : 0;

        // Unique users
        const uniqueUserNames = new Set(attempts.map(a => a.userName?.toLowerCase()).filter(u => u));
        this.stats.uniqueUsers = uniqueUserNames.size;

        // Top failed users
        const failedUserCounts = new Map<string, number>();
        failures.forEach(a => {
            const userName = a.userName || 'Unknown';
            failedUserCounts.set(userName, (failedUserCounts.get(userName) || 0) + 1);
        });
        this.stats.topFailedUsers = Array.from(failedUserCounts.entries())
            .map(([userName, count]) => ({ userName, count }))
            .sort((a, b) => b.count - a.count)
            .slice(0, 5);

        // Build sparkline data (hourly buckets for the past 24 hours or current range)
        this.buildSparkline(attempts, successes, failures);
    }


    //
    // Build sparkline chart data from hourly buckets
    //
    private buildSparkline(attempts: LoginAttemptData[], successes: LoginAttemptData[], failures: LoginAttemptData[]): void {
        if (attempts.length === 0) {
            this.sparklineData = { labels: [], datasets: [] };
            return;
        }

        // Determine time range from the data
        const timestamps = attempts.map(a => new Date(a.timeStamp).getTime());
        const minTime = Math.min(...timestamps);
        const maxTime = Math.max(...timestamps);

        // Create hourly buckets
        const hourMs = 60 * 60 * 1000;
        const buckets = new Map<number, { total: number; success: number; failure: number }>();

        // Initialize buckets
        const startBucket = Math.floor(minTime / hourMs) * hourMs;
        const endBucket = Math.ceil(maxTime / hourMs) * hourMs;
        for (let t = startBucket; t <= endBucket; t += hourMs) {
            buckets.set(t, { total: 0, success: 0, failure: 0 });
        }

        // Fill buckets
        attempts.forEach(a => {
            const bucket = Math.floor(new Date(a.timeStamp).getTime() / hourMs) * hourMs;
            const b = buckets.get(bucket);
            if (b) {
                b.total++;
                if (this.isSuccess(a)) b.success++;
                else b.failure++;
            }
        });

        // Convert to chart data
        const sortedBuckets = Array.from(buckets.entries()).sort((a, b) => a[0] - b[0]);
        this.sparklineData = {
            labels: sortedBuckets.map(([t]) => {
                const d = new Date(t);
                return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            }),
            datasets: [
                {
                    data: sortedBuckets.map(([, v]) => v.total),
                    borderColor: '#7b1fa2',
                    backgroundColor: 'rgba(123, 31, 162, 0.1)',
                    fill: true
                }
            ]
        };
    }


    //
    // Apply sorting
    //
    private applySorting(attempts: LoginAttemptData[]): LoginAttemptData[] {
        return attempts.sort((a, b) => {
            let aVal: any = this.getNestedValue(a, this.sortColumn);
            let bVal: any = this.getNestedValue(b, this.sortColumn);

            if (aVal == null) aVal = '';
            if (bVal == null) bVal = '';

            // Handle dates
            if (this.sortColumn === 'timeStamp') {
                aVal = new Date(aVal).getTime();
                bVal = new Date(bVal).getTime();
            }

            if (typeof aVal === 'string') aVal = aVal.toLowerCase();
            if (typeof bVal === 'string') bVal = bVal.toLowerCase();

            let comparison = 0;
            if (aVal < bVal) comparison = -1;
            if (aVal > bVal) comparison = 1;

            return this.sortDirection === 'asc' ? comparison : -comparison;
        });
    }


    private getNestedValue(obj: any, path: string): any {
        return path.split('.').reduce((o, p) => o && o[p], obj);
    }


    //
    // Handle time preset change
    //
    onTimePresetChange(): void {
        if (this.selectedTimePreset !== 'custom') {
            this.customStartDate = '';
            this.customEndDate = '';
        } else {
            // Default to last 7 days for custom
            const today = new Date();
            const weekAgo = new Date();
            weekAgo.setDate(today.getDate() - 7);
            this.customEndDate = today.toISOString().split('T')[0];
            this.customStartDate = weekAgo.toISOString().split('T')[0];
        }
        this.applyFilters();
    }


    //
    // Clear all filters
    //
    clearFilters(): void {
        this.selectedTimePreset = '24h';
        this.statusFilter = 'all';
        this.userFilter = '';
        this.searchText = '';
        this.customStartDate = '';
        this.customEndDate = '';
        this.applyFilters();
    }


    //
    // Toggle row expansion
    //
    toggleExpand(attempt: LoginAttemptData): void {
        const id = Number(attempt.id);
        if (this.expandedIds.has(id)) {
            this.expandedIds.delete(id);
        } else {
            this.expandedIds.add(id);
        }
    }

    isExpanded(attempt: LoginAttemptData): boolean {
        return this.expandedIds.has(Number(attempt.id));
    }


    //
    // Status helpers
    //
    isSuccess(attempt: LoginAttemptData): boolean {
        // Use the success field if it's explicitly set (new records)
        // Handle both boolean and string values due to potential JSON serialization differences
        const success = attempt.success;
        if (success === true || success === 'true' as any) {
            return true;
        }
        if (success === false || success === 'false' as any) {
            return false;
        }

        // Fallback to heuristic for historical data without success field
        const value = (attempt.value || '').toLowerCase();

        // Check for explicit failure indicators first
        const failureIndicators = ['fail', 'error', 'invalid', 'denied', 'locked', 'expired', 'disabled', 'not found', 'unauthorized'];
        for (const indicator of failureIndicators) {
            if (value.includes(indicator)) {
                return false;
            }
        }

        // Check for explicit success indicators
        const successIndicators = ['success', 'ok', 'authenticated', 'granted'];
        for (const indicator of successIndicators) {
            if (value.includes(indicator)) {
                return true;
            }
        }

        // Default: if empty value or no failure indicators, assume success
        return true;
    }


    //
    // Sorting
    //
    sortBy(column: string): void {
        if (this.sortColumn === column) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = column;
            this.sortDirection = column === 'timeStamp' ? 'desc' : 'asc';
        }

        const sorted = this.applySorting(this.loginAttempts$.value);
        this.loginAttempts$.next(sorted);
        this.updatePagination();
    }


    getSortIcon(column: string): string {
        if (this.sortColumn !== column) {
            return 'fa-sort';
        }
        return this.sortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    }


    //
    // Pagination
    //
    get totalPages(): number {
        const total = this.loginAttempts$.value.length;
        return Math.ceil(total / this.pageSize);
    }


    private updatePagination(): void {
        const attempts = this.loginAttempts$.value;
        const start = (this.currentPage - 1) * this.pageSize;
        const end = start + this.pageSize;
        this.pagedAttempts = attempts.slice(start, end);

        // Reset to page 1 if current page is now invalid
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
            this.currentPage = 1;
            this.updatePagination();
        }
    }


    goToPage(page: number): void {
        if (page >= 1 && page <= this.totalPages) {
            this.currentPage = page;
            this.updatePagination();
        }
    }


    //
    // Formatting helpers
    //
    formatRelativeTime(dateString: string | null): string {
        if (!dateString) return 'Never';

        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;

        return date.toLocaleDateString();
    }


    formatFullDateTime(dateString: string | null): string {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleString();
    }


    truncateText(text: string | null, maxLength: number): string {
        if (!text) return '—';
        if (text.length <= maxLength) return text;
        return text.substring(0, maxLength) + '...';
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
        this.nextRefreshIn = this.autoRefreshInterval;

        // Countdown timer
        this.countdownSub = interval(1000).pipe(takeUntil(this.destroy$)).subscribe(() => {
            this.nextRefreshIn--;
            if (this.nextRefreshIn <= 0) {
                this.nextRefreshIn = this.autoRefreshInterval;
            }
        });

        // Actual refresh
        this.autoRefreshSub = interval(this.autoRefreshInterval * 1000).pipe(takeUntil(this.destroy$)).subscribe(() => {
            this.applyFilters();
        });
    }


    private stopAutoRefresh(): void {
        if (this.countdownSub) {
            this.countdownSub.unsubscribe();
            this.countdownSub = null;
        }
        if (this.autoRefreshSub) {
            this.autoRefreshSub.unsubscribe();
            this.autoRefreshSub = null;
        }
    }


    formatAutoRefreshInterval(seconds: number): string {
        if (seconds < 60) return `${seconds} seconds`;
        return `${seconds / 60} minute${seconds > 60 ? 's' : ''}`;
    }


    //
    // Export to CSV
    //
    exportToCsv(): void {
        const attempts = this.loginAttempts$.value;
        if (attempts.length === 0) {
            this.alertService.showMessage('Info', 'No data to export', MessageSeverity.info);
            return;
        }

        const headers = ['ID', 'Timestamp', 'User', 'Status', 'IP Address', 'Resource', 'Session ID', 'User Agent', 'Result'];
        const rows = attempts.map(a => [
            a.id,
            a.timeStamp,
            a.userName || '',
            this.isSuccess(a) ? 'Success' : 'Failed',
            a.ipAddress || '',
            a.resource || '',
            a.sessionId || '',
            a.userAgent || '',
            a.value || ''
        ]);

        const csvContent = [headers, ...rows]
            .map(row => row.map(cell => `"${String(cell).replace(/"/g, '""')}"`).join(','))
            .join('\n');

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', `login-attempts-${new Date().toISOString().split('T')[0]}.csv`);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }


    //
    // TrackBy
    //
    trackByAttemptId(index: number, attempt: LoginAttemptData): number {
        return Number(attempt.id);
    }


    //
    // Permissions
    //
    userIsSecurityLoginAttemptReader(): boolean {
        return this.loginAttemptService.userIsSecurityLoginAttemptReader();
    }
}
