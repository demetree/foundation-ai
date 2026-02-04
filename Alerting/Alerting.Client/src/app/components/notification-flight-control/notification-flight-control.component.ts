//
// Notification Flight Control Component
//
// Real-time dashboard for monitoring the notification engine.
// Admin-only access, 5-second auto-refresh.
// AI-assisted development - February 2026
//
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { ConfigurationService } from '../../services/configuration.service';
import { NavigationService } from '../../utility-services/navigation.service';
import { AlertService } from '../../services/alert.service';
import { AlertingUserService, AlertingUser } from '../../services/alerting-user.service';


//
// DTO Interfaces
//
interface WorkerStatus {
    workerName: string;
    isRunning: boolean;
    lastRunAt: string | null;
    nextRunAt: string | null;
    itemsProcessedLastRun: number;
    totalItemsProcessed: number;
}

interface NotificationQueueMetrics {
    pendingCount: number;
    failedCount: number;
    deliveredCount: number;
    totalAttempts: number;
    successRate: number;
}

interface ChannelDeliveryMetrics {
    channelTypeId: number;
    channelName: string;
    totalAttempts: number;
    successCount: number;
    failedCount: number;
    pendingCount: number;
    successRate: number;
}

interface RecentDeliveryAttempt {
    id: number;
    attemptedAt: string;
    status: string;
    channelName: string;
    channelTypeId: number;
    attemptNumber: number;
    recipientSummary: string;
    errorMessage: string | null;
    response: string | null;
    incidentId: number;
    incidentTitle: string;
    userObjectGuid: string | null;
}

interface RecentWebhookAttempt {
    id: number;
    attemptedAt: string;
    success: boolean;
    httpStatusCode: number | null;
    targetUrl: string;
    attemptNumber: number;
    errorMessage: string | null;
    incidentId: number;
    incidentTitle: string;
    integrationName: string;
}

interface FlightControlSummary {
    generatedAt: string;
    timeRange: number;
    channelFilter: string | null;
    escalationWorker: WorkerStatus;
    retryWorker: WorkerStatus;
    queue: NotificationQueueMetrics;
    channelMetrics: ChannelDeliveryMetrics[];
    recentDeliveries: RecentDeliveryAttempt[];
    recentWebhooks: RecentWebhookAttempt[];
}

interface DeliveryAttemptDetail extends RecentDeliveryAttempt {
    userDisplayName: string | null;
    userEmail: string | null;
    severityName: string;
    incidentDescription: string | null;
}

interface WebhookAttemptDetail extends RecentWebhookAttempt {
    payloadJson: string | null;
    responseBody: string | null;
}


@Component({
    selector: 'app-notification-flight-control',
    standalone: false,
    templateUrl: './notification-flight-control.component.html',
    styleUrl: './notification-flight-control.component.scss'
})
export class NotificationFlightControlComponent implements OnInit, OnDestroy {

    //
    // Data
    //
    summary: FlightControlSummary | null = null;
    loading = true;
    error: string | null = null;

    //
    // Filters
    //
    selectedTimeRange: number = 24; // Default 24h
    selectedChannel: string | null = null;
    timeRangeOptions = [
        { value: 1, label: 'Last 1 Hour' },
        { value: 6, label: 'Last 6 Hours' },
        { value: 24, label: 'Last 24 Hours' },
        { value: 168, label: 'Last 7 Days' }
    ];

    //
    // Drill-down modals
    //
    selectedDelivery: DeliveryAttemptDetail | null = null;
    selectedWebhook: WebhookAttemptDetail | null = null;
    showDeliveryModal = false;
    showWebhookModal = false;

    //
    // Auto-refresh
    //
    private readonly REFRESH_INTERVAL = 5000; // 5 seconds
    private refreshSubscription?: Subscription;

    //
    // User lookup map for GUID -> display name resolution
    //
    private userMap = new Map<string, AlertingUser>();

    constructor(
        private http: HttpClient,
        private config: ConfigurationService,
        private authService: AuthService,
        private navigationService: NavigationService,
        private alertService: AlertService,
        private router: Router,
        private userService: AlertingUserService
    ) { }

    ngOnInit(): void {
        this.loadUsers();
        this.loadData();
        this.startAutoRefresh();
    }

    ngOnDestroy(): void {
        this.stopAutoRefresh();
    }

    //
    // User Loading
    //
    private loadUsers(): void {
        this.userService.getUsers().subscribe({
            next: (users) => {
                this.userMap.clear();
                for (const user of users) {
                    this.userMap.set(user.objectGuid, user);
                }
            },
            error: (err) => {
                console.error('Failed to load users for display name resolution', err);
            }
        });
    }

    /**
     * Resolves a user GUID to a readable display name.
     * Falls back to the GUID if user not found.
     */
    getUserDisplayName(guid: string | null): string {
        if (!guid) return 'Unknown';
        const user = this.userMap.get(guid);
        if (user) {
            return user.displayName || `${user.firstName} ${user.lastName}`.trim() || user.accountName;
        }
        return `User: ${guid}`;
    }

    //
    // Data Loading
    //
    loadData(): void {
        this.loading = true;
        const headers = this.authService.GetAuthenticationHeaders();

        let url = `${this.config.baseUrl}/api/NotificationFlightControl/summary?timeRange=${this.selectedTimeRange}`;
        if (this.selectedChannel) {
            url += `&channel=${encodeURIComponent(this.selectedChannel)}`;
        }

        this.http.get<FlightControlSummary>(url, { headers }).subscribe({
            next: (data) => {
                this.summary = data;
                this.loading = false;
                this.error = null;
            },
            error: (err) => {
                this.loading = false;
                if (err.status === 403) {
                    this.error = 'Access denied. This dashboard requires administrator privileges.';
                } else {
                    this.error = 'Failed to load flight control data.';
                }
                console.error('Flight control error:', err);
            }
        });
    }

    refresh(): void {
        this.loadData();
    }

    //
    // Auto-Refresh
    //
    private startAutoRefresh(): void {
        this.refreshSubscription = interval(this.REFRESH_INTERVAL).subscribe(() => {
            this.loadData();
        });
    }

    private stopAutoRefresh(): void {
        this.refreshSubscription?.unsubscribe();
    }

    //
    // Filter Changes
    //
    onTimeRangeChange(): void {
        this.loadData();
    }

    onChannelChange(channelName: string | null): void {
        this.selectedChannel = channelName;
        this.loadData();
    }

    //
    // Navigation
    //
    canGoBack(): boolean {
        return this.navigationService?.canGoBack?.() ?? false;
    }

    goBack(): void {
        this.navigationService?.goBack?.();
    }

    viewIncident(incidentId: number): void {
        this.router.navigate(['/incident-view', incidentId]);
    }

    //
    // Drill-down
    //
    openDeliveryDetail(attempt: RecentDeliveryAttempt): void {
        const headers = this.authService.GetAuthenticationHeaders();
        this.http.get<DeliveryAttemptDetail>(
            `${this.config.baseUrl}/api/NotificationFlightControl/delivery/${attempt.id}`,
            { headers }
        ).subscribe({
            next: (data) => {
                this.selectedDelivery = data;
                this.showDeliveryModal = true;
            },
            error: (error) => this.alertService.showHttpErrorMessage('Failed to load delivery details', error)
        });
    }

    openWebhookDetail(attempt: RecentWebhookAttempt): void {
        const headers = this.authService.GetAuthenticationHeaders();
        this.http.get<WebhookAttemptDetail>(
            `${this.config.baseUrl}/api/NotificationFlightControl/webhook/${attempt.id}`,
            { headers }
        ).subscribe({
            next: (data) => {
                this.selectedWebhook = data;
                this.showWebhookModal = true;
            },
            error: (error) => this.alertService.showHttpErrorMessage('Failed to load webhook details', error)
        });
    }

    closeDeliveryModal(): void {
        this.showDeliveryModal = false;
        this.selectedDelivery = null;
    }

    closeWebhookModal(): void {
        this.showWebhookModal = false;
        this.selectedWebhook = null;
    }

    //
    // Formatting Helpers
    //
    formatTimeAgo(timestamp: string): string {
        const date = new Date(timestamp);
        const now = new Date();
        const seconds = Math.floor((now.getTime() - date.getTime()) / 1000);

        if (seconds < 60) return `${seconds}s ago`;
        const minutes = Math.floor(seconds / 60);
        if (minutes < 60) return `${minutes}m ago`;
        const hours = Math.floor(minutes / 60);
        if (hours < 24) return `${hours}h ago`;
        const days = Math.floor(hours / 24);
        return `${days}d ago`;
    }

    formatDateTime(timestamp: string | null): string {
        if (!timestamp) return 'N/A';
        return new Date(timestamp).toLocaleString();
    }

    getStatusClass(status: string): string {
        switch (status?.toLowerCase()) {
            case 'delivered':
            case 'sent':
                return 'status-delivered';
            case 'pending':
                return 'status-pending';
            case 'failed':
            case 'abandoned':
            case 'error':
                return 'status-failed';
            default:
                // Treat unknown statuses as failed for visibility
                return status ? 'status-failed' : '';
        }
    }

    getWorkerStatusClass(worker: WorkerStatus): string {
        if (!worker.isRunning) return 'worker-stopped';
        if (!worker.lastRunAt) return 'worker-idle';
        const lastRun = new Date(worker.lastRunAt);
        const now = new Date();
        const secondsSinceRun = (now.getTime() - lastRun.getTime()) / 1000;
        return secondsSinceRun < 120 ? 'worker-healthy' : 'worker-stale';
    }

    getSuccessRateClass(rate: number): string {
        if (rate >= 95) return 'rate-excellent';
        if (rate >= 80) return 'rate-good';
        if (rate >= 60) return 'rate-warning';
        return 'rate-danger';
    }

    getChannelIcon(channelName: string): string {
        switch (channelName?.toLowerCase()) {
            case 'email': return 'bi bi-envelope';
            case 'sms': return 'bi bi-phone';
            case 'push': return 'bi bi-app-indicator';
            case 'teams': return 'bi bi-microsoft-teams';
            case 'voice': return 'bi bi-telephone';
            default: return 'bi bi-send';
        }
    }
}
