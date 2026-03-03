import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { Subject, takeUntil, filter } from 'rxjs';
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

    //
    // Route prefixes where the Rebrickable bubble should appear.
    // Includes premium custom routes and scaffolded data routes for synced tables.
    //
    private static readonly REBRICKABLE_ROUTE_PREFIXES: string[] = [
        // Premium custom UIs
        '/my-collection',
        '/my-set-lists',
        '/my-sets',
        '/my-part-lists',
        '/my-lost-parts',
        '/integrations',
        // Scaffolded data routes for Rebrickable-synced tables
        '/userlostparts',
        '/userlostpart',
        '/userpartlists',
        '/userpartlist',
        '/userpartlistitems',
        '/userpartlistitem',
        '/usersetlists',
        '/usersetlist',
        '/usersetlistitems',
        '/usersetlistitem',
        '/usersetownerships',
        '/usersetownership',
        // Rebrickable admin routes
        '/rebrickablesyncqueues',
        '/rebrickablesyncqueue',
        '/rebrickabletransactions',
        '/rebrickabletransaction',
        '/rebrickableuserlinks',
        '/rebrickableuserlink',
    ];

    // State
    isConnected = false;
    isActive = false;
    hasWarning = false;
    warningMessage = '';
    lastActivityText = '';
    showPanel = false;
    showTooltip = false;
    showReauth = false;
    statusLoaded = false;
    isOnRebrickableRoute = false;

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
        private router: Router,
        private authService: AuthService,
        private signalr: RebrickableSignalrService,
        private syncService: RebrickableSyncService
    ) { }


    //
    // Visible is true only when we have loaded status AND the user is on a Rebrickable route.
    //
    get visible(): boolean {
        return this.statusLoaded && this.isOnRebrickableRoute;
    }


    ngOnInit(): void {
        // Only show for logged-in users
        if (!this.authService.isLoggedIn) return;

        // Evaluate the current route immediately
        this.isOnRebrickableRoute = this.checkRebrickableRoute(this.router.url);

        // Subscribe to route changes to toggle visibility
        this.router.events.pipe(
            filter((event): event is NavigationEnd => event instanceof NavigationEnd),
            takeUntil(this.destroy$)
        ).subscribe(event => {
            this.isOnRebrickableRoute = this.checkRebrickableRoute(event.urlAfterRedirects);
        });

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
                this.statusLoaded = false;
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
                this.statusLoaded = true;
            },
            error: () => {
                this.statusLoaded = false;
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


    //
    // Returns true if the given URL starts with any of the Rebrickable route prefixes.
    //
    private checkRebrickableRoute(url: string): boolean {
        const path = url.split('?')[0].split('#')[0].toLowerCase();
        return RebrickableStatusBubbleComponent.REBRICKABLE_ROUTE_PREFIXES.some(
            prefix => path === prefix || path.startsWith(prefix + '/')
        );
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        clearTimeout(this.activityTimeout);
        this.signalr.disconnect();
    }
}
