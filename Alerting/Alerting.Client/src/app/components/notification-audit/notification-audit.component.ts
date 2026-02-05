import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { NavigationService } from '../../utility-services/navigation.service';
import { AlertService } from '../../services/alert.service';
import { ConfigurationService } from '../../services/configuration.service';
import { AuthService } from '../../services/auth.service';

/**
 * Metrics summary for the notification audit dashboard.
 */
interface NotificationAuditMetrics {
    sentToday: number;
    failedToday: number;
    pendingNow: number;
    successRate7Day: number;
    avgLatencyMs: number;
    channelBreakdown: ChannelBreakdown[];
}

interface ChannelBreakdown {
    channelTypeId: number;
    channelName: string;
    count: number;
    percentage: number;
}

/**
 * Summary row for delivery attempt list.
 */
interface DeliverySummary {
    id: number;
    objectGuid: string;
    attemptedAt: string;
    channelTypeId: number;
    channelName: string;
    status: string;
    errorMessage: string;
    recipientAddress: string;
    recipientDisplay: string;
    incidentId: number;
    incidentKey: string;
    incidentTitle: string;
    userObjectGuid: string;
}

/**
 * Full delivery detail with content.
 */
interface DeliveryDetail extends DeliverySummary {
    subject: string;
    bodyContent: string;
    response: string;
    attemptNumber: number;
    totalAttempts: number;
    notificationCreatedAt: string;
    incidentCreatedAt: string;
}

/**
 * Paginated delivery list result.
 */
interface DeliveryListResult {
    items: DeliverySummary[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}

/**
 * Notification Audit Console Component
 * 
 * Premium admin interface for browsing and inspecting all notification deliveries
 * with full content visibility for forensic auditing.
 */
@Component({
    selector: 'app-notification-audit',
    standalone: false,
    templateUrl: './notification-audit.component.html',
    styleUrls: ['./notification-audit.component.scss']
})
export class NotificationAuditComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // Loading states
    isLoading = true;
    isLoadingDetail = false;
    errorMessage: string | null = null;

    // Metrics
    metrics: NotificationAuditMetrics | null = null;

    // List data
    deliveries: DeliverySummary[] = [];
    totalCount = 0;
    pageNumber = 1;
    pageSize = 25;

    // Filters
    channelFilter: number | null = null;
    statusFilter: string | null = null;
    searchQuery = '';
    dateFrom = '';
    dateTo = '';

    // Channel options for filter dropdown
    channelOptions = [
        { id: 1, name: 'Email' },
        { id: 2, name: 'SMS' },
        { id: 3, name: 'Voice' },
        { id: 4, name: 'Push' },
        { id: 5, name: 'Teams' }
    ];

    // Status options for filter dropdown
    statusOptions = ['Pending', 'Sent', 'Failed'];

    // Selected detail
    selectedDelivery: DeliveryDetail | null = null;
    renderedHtml: SafeHtml | null = null;

    constructor(
        private http: HttpClient,
        private sanitizer: DomSanitizer,
        private config: ConfigurationService,
        private authService: AuthService,
        private navigationService: NavigationService,
        private alertService: AlertService
    ) { }

    ngOnInit(): void {
        this.loadMetrics();
        this.loadDeliveries();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    /**
     * Load KPI metrics
     */
    loadMetrics(): void {
        const headers = this.authService.GetAuthenticationHeaders();
        this.http.get<NotificationAuditMetrics>(`${this.config.baseUrl}/api/notification-audit/metrics`, { headers })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (metrics) => {
                    this.metrics = metrics;
                },
                error: (error) => {
                    console.error('Failed to load metrics', error);
                }
            });
    }

    /**
     * Load delivery attempts list
     */
    loadDeliveries(): void {
        this.isLoading = true;
        this.errorMessage = null;

        let url = `${this.config.baseUrl}/api/notification-audit/deliveries?pageNumber=${this.pageNumber}&pageSize=${this.pageSize}`;

        if (this.channelFilter) {
            url += `&channelTypeId=${this.channelFilter}`;
        }
        if (this.statusFilter) {
            url += `&status=${this.statusFilter}`;
        }
        if (this.searchQuery.trim()) {
            url += `&search=${encodeURIComponent(this.searchQuery.trim())}`;
        }
        if (this.dateFrom) {
            url += `&dateFrom=${this.dateFrom}`;
        }
        if (this.dateTo) {
            url += `&dateTo=${this.dateTo}`;
        }

        const headers = this.authService.GetAuthenticationHeaders();
        this.http.get<DeliveryListResult>(url, { headers })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.deliveries = result.items;
                    this.totalCount = result.totalCount;
                    this.isLoading = false;
                },
                error: (error) => {
                    this.errorMessage = 'Failed to load delivery attempts';
                    this.alertService.showHttpErrorMessage('Error', error);
                    this.isLoading = false;
                }
            });
    }

    /**
     * Load detail for a specific delivery attempt
     */
    loadDeliveryDetail(id: number): void {
        this.isLoadingDetail = true;

        const headers = this.authService.GetAuthenticationHeaders();
        this.http.get<DeliveryDetail>(`${this.config.baseUrl}/api/notification-audit/deliveries/${id}`, { headers })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (detail) => {
                    this.selectedDelivery = detail;
                    this.isLoadingDetail = false;

                    // For email, render HTML content
                    if (detail.channelTypeId === 1 && detail.bodyContent) {
                        this.renderedHtml = this.sanitizer.bypassSecurityTrustHtml(detail.bodyContent);
                    } else {
                        this.renderedHtml = null;
                    }
                },
                error: (error) => {
                    this.alertService.showHttpErrorMessage('Error loading detail', error);
                    this.isLoadingDetail = false;
                }
            });
    }

    /**
     * Select a delivery row
     */
    selectDelivery(delivery: DeliverySummary): void {
        this.loadDeliveryDetail(delivery.id);
    }

    /**
     * Close detail panel
     */
    closeDetail(): void {
        this.selectedDelivery = null;
        this.renderedHtml = null;
    }

    /**
     * Apply filters
     */
    applyFilters(): void {
        this.pageNumber = 1;
        this.loadDeliveries();
    }

    /**
     * Clear all filters
     */
    clearFilters(): void {
        this.channelFilter = null;
        this.statusFilter = null;
        this.searchQuery = '';
        this.dateFrom = '';
        this.dateTo = '';
        this.pageNumber = 1;
        this.loadDeliveries();
    }

    /**
     * Go to page
     */
    goToPage(page: number): void {
        if (page < 1 || page > this.totalPages) return;
        this.pageNumber = page;
        this.loadDeliveries();
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
        let start = Math.max(1, this.pageNumber - Math.floor(maxPages / 2));
        const end = Math.min(this.totalPages, start + maxPages - 1);
        start = Math.max(1, end - maxPages + 1);

        for (let i = start; i <= end; i++) {
            pages.push(i);
        }
        return pages;
    }

    /**
     * Get status CSS class
     */
    getStatusClass(status: string): string {
        switch (status?.toLowerCase()) {
            case 'sent': return 'status-sent';
            case 'failed': return 'status-failed';
            case 'pending': return 'status-pending';
            default: return '';
        }
    }

    /**
     * Get channel icon
     */
    getChannelIcon(channelTypeId: number): string {
        switch (channelTypeId) {
            case 1: return 'fa-envelope';
            case 2: return 'fa-comment-sms';
            case 3: return 'fa-phone';
            case 4: return 'fa-bell';
            case 5: return 'fa-users';
            default: return 'fa-paper-plane';
        }
    }

    /**
     * Format relative time
     */
    getRelativeTime(dateString: string): string {
        if (!dateString) return '—';
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
     * Format datetime
     */
    formatDateTime(dateString: string): string {
        if (!dateString) return '—';
        return new Date(dateString).toLocaleString();
    }

    /**
     * Check if there are active filters
     */
    hasActiveFilters(): boolean {
        return this.channelFilter !== null ||
            this.statusFilter !== null ||
            this.searchQuery.trim().length > 0 ||
            this.dateFrom.length > 0 ||
            this.dateTo.length > 0;
    }

    /**
     * Open HTML content in new window
     */
    openInNewWindow(): void {
        if (!this.selectedDelivery?.bodyContent) return;

        const newWindow = window.open('', '_blank', 'width=800,height=600');
        if (newWindow) {
            newWindow.document.write(this.selectedDelivery.bodyContent);
            newWindow.document.close();
        }
    }

    /**
     * Refresh data
     */
    refresh(): void {
        this.loadMetrics();
        this.loadDeliveries();
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
