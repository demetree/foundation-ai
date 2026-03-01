import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { RebrickableSignalrService, SyncActivityEvent, ConnectionChangedEvent } from '../../services/rebrickable-signalr.service';
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
    visible = false;

    // Recent activity feed (last 5 for tooltip)
    recentEvents: SyncActivityEvent[] = [];
    private activityTimeout: any;

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
    }


    togglePanel(): void {
        this.showPanel = !this.showPanel;
    }


    closePanel(): void {
        this.showPanel = false;
    }


    get bubbleClass(): string {
        if (this.hasWarning) return 'warning';
        if (this.isActive) return 'active';
        if (this.isConnected) return 'connected';
        return 'disconnected';
    }


    get bubbleIcon(): string {
        if (this.hasWarning) return 'fas fa-exclamation-triangle';
        if (this.isActive) return 'fas fa-bolt';
        if (this.isConnected) return 'fas fa-link';
        return 'fas fa-unlink';
    }


    get bubbleText(): string {
        if (this.isActive && this.lastActivityText) return this.lastActivityText;
        if (this.hasWarning) return this.warningMessage || 'Token warning';
        return 'Rebrickable';
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        clearTimeout(this.activityTimeout);
        this.signalr.disconnect();
    }
}
