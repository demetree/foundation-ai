import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { RebrickableSignalrService, SyncActivityEvent, ConnectionChangedEvent, RateLimitEvent } from '../../services/rebrickable-signalr.service';
import { RebrickableSyncService, SyncStatus } from '../../services/rebrickable-sync.service';

@Component({
    selector: 'app-rebrickable-status-bubble',
    templateUrl: './rebrickable-status-bubble.component.html',
    styleUrls: ['./rebrickable-status-bubble.component.scss']
})
export class RebrickableStatusBubbleComponent implements OnInit, OnDestroy {
    private destroy$ = new Subject<void>();

    // State
    isConnected = false;
    isActive = false;
    hasWarning = false;
    warningMessage = '';
    lastActivityText = '';
    showPanel = false;
    showTooltip = false;
    showReauth = false;
    visible = false;

    // Sync mode awareness
    integrationMode = 'None';
    syncEnabled = false;

    // Recent activity feed (last 5 for tooltip)
    recentEvents: SyncActivityEvent[] = [];
    private activityTimeout: any;

    // Rate limit
    rateLimitRemaining = -1;
    rateLimitLimit = -1;
    rateLimitPercent = 100;
    rateLimitResetSeconds = 0;

    constructor(
        private authService: AuthService,
        private signalr: RebrickableSignalrService,
        private syncService: RebrickableSyncService
    ) { }


    ngOnInit(): void {
        // Only show for logged-in users
        if (!this.authService.isLoggedIn) return;

        // Load initial status
        this.loadStatus();

        // Connect SignalR
        this.signalr.connect();

        // Listen for sync activity
        this.signalr.onSyncActivity$.pipe(takeUntil(this.destroy$)).subscribe(event => {
            this.onSyncActivity(event);
        });

        // Listen for connection changes
        this.signalr.onConnectionChanged$.pipe(takeUntil(this.destroy$)).subscribe(event => {
            this.onConnectionChanged(event);
        });

        // Listen for token warnings
        this.signalr.onTokenWarning$.pipe(takeUntil(this.destroy$)).subscribe(event => {
            this.hasWarning = true;
            this.warningMessage = event.message;
        });

        // Listen for rate limit updates
        this.signalr.onRateLimitUpdate$.pipe(takeUntil(this.destroy$)).subscribe(event => {
            this.onRateLimitUpdate(event);
        });

        // Re-fetch status when the SignalR hub itself connects/reconnects
        // (covers race where initial loadStatus ran before the hub was ready)
        this.signalr.onHubConnectionChange$.pipe(takeUntil(this.destroy$)).subscribe(connected => {
            if (connected) {
                this.loadStatus();
            }
        });

        // Listen for login/logout
        this.authService.getLoginStatusEvent().pipe(takeUntil(this.destroy$)).subscribe(isLoggedIn => {
            if (!isLoggedIn) {
                this.signalr.disconnect();
                this.visible = false;
            } else {
                this.loadStatus();
                this.signalr.connect();
            }
        });
    }


    private loadStatus(): void {
        this.syncService.getStatus().pipe(takeUntil(this.destroy$)).subscribe({
            next: (status: SyncStatus) => {
                this.isConnected = status.isConnected;
                this.integrationMode = status.integrationMode || 'None';
                // Server DTO does not include syncEnabled — derive it from integrationMode.
                // Sync is enabled when integration mode is anything other than 'None'.
                this.syncEnabled = this.integrationMode !== 'None';
                this.visible = true; // Show bubble once status is loaded
            },
            error: () => {
                this.visible = false;
            }
        });
    }


    private onSyncActivity(event: SyncActivityEvent): void {
        // Add to recent feed
        this.recentEvents.unshift(event);
        if (this.recentEvents.length > 5) this.recentEvents.pop();

        // Show active pulse
        this.isActive = true;
        this.lastActivityText = event.summary || `${event.httpMethod} ${event.endpoint}`;

        // Clear warning on successful activity
        if (event.success) {
            this.hasWarning = false;
            this.warningMessage = '';
        }

        // Reset active state after 3s
        clearTimeout(this.activityTimeout);
        this.activityTimeout = setTimeout(() => {
            this.isActive = false;
            this.lastActivityText = '';
        }, 3000);
    }


    private onConnectionChanged(event: ConnectionChangedEvent): void {
        this.isConnected = event.isConnected;
        if (!event.isConnected) {
            this.hasWarning = false;
            this.warningMessage = '';
        }

        //
        // When the server reports connected via SignalR, trust it directly.
        // Delay the status re-fetch slightly to allow the server to finalize
        // any pending writes (e.g. cache population for SessionOnly mode)
        // before we poll.  This prevents a race where the HTTP status response
        // arrives before the server-side state is consistent, overwriting
        // the authoritative SignalR value.
        //
        if (event.isConnected) {
            setTimeout(() => this.loadStatus(), 1000);
        } else {
            this.loadStatus();
        }
    }


    togglePanel(): void {
        this.showPanel = !this.showPanel;
    }


    closePanel(): void {
        this.showPanel = false;
    }


    openReauth(): void {
        this.showReauth = true;
    }


    closeReauth(): void {
        this.showReauth = false;
    }


    onReauthSuccess(): void {
        this.hasWarning = false;
        this.warningMessage = '';
        this.isConnected = true;
        this.loadStatus();
    }


    get bubbleClass(): string {
        if (this.hasWarning) return 'warning';
        if (this.isActive) return 'active';
        if (this.isConnected && this.rateLimitPercent >= 0 && this.rateLimitPercent < 20) return 'rate-limited';
        if (this.isConnected && !this.syncEnabled) return 'sync-disabled';
        if (this.isConnected) return 'connected';
        return 'disconnected';
    }


    get rateLimitClass(): string {
        if (this.rateLimitPercent < 20) return 'critical';
        if (this.rateLimitPercent < 50) return 'warning';
        return 'healthy';
    }


    private onRateLimitUpdate(event: RateLimitEvent): void {
        this.rateLimitRemaining = event.remaining;
        this.rateLimitLimit = event.limit;
        this.rateLimitResetSeconds = event.resetSeconds;
        this.rateLimitPercent = event.limit > 0 ? Math.round((event.remaining / event.limit) * 100) : 100;
    }


    get bubbleIcon(): string {
        if (this.hasWarning) return 'fas fa-exclamation-triangle';
        if (this.isActive) return 'fas fa-bolt';
        if (this.isConnected && !this.syncEnabled) return 'fas fa-pause-circle';
        if (this.isConnected) return 'fas fa-link';
        return 'fas fa-unlink';
    }


    get bubbleText(): string {
        if (this.isActive && this.lastActivityText) return this.lastActivityText;
        if (this.hasWarning) return this.warningMessage || 'Token warning';
        if (this.isConnected && !this.syncEnabled) return 'Sync Disabled';
        return 'Rebrickable';
    }


    get syncModeLabel(): string {
        switch (this.integrationMode) {
            case 'RealTime': return 'Real-Time';
            case 'PushOnly': return 'Push Only';
            case 'ImportOnly': return 'Import Only';
            default: return 'None';
        }
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        clearTimeout(this.activityTimeout);
        this.signalr.disconnect();
    }
}
