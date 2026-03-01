import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';

//
// Rebrickable SignalR Service
//
// Connects to /RebrickableSyncSignal for real-time sync activity updates.
// Broadcasts: SyncActivity, ConnectionChanged, TokenWarning
//

export interface SyncActivityEvent {
    direction: string;
    httpMethod: string;
    endpoint: string;
    summary: string;
    statusCode: number;
    success: boolean;
    errorMessage: string | null;
    recordCount: number | null;
    timestamp: string;
}

export interface ConnectionChangedEvent {
    isConnected: boolean;
    authMode: string | null;
    username: string | null;
    timestamp: string;
}

export interface TokenWarningEvent {
    message: string;
    timestamp: string;
}


@Injectable({
    providedIn: 'root'
})
export class RebrickableSignalrService implements OnDestroy {

    private connection: signalR.HubConnection | null = null;

    /** Real-time sync activity events (API calls). */
    readonly onSyncActivity$ = new Subject<SyncActivityEvent>();

    /** Connection state changes (connect/disconnect/re-auth). */
    readonly onConnectionChanged$ = new Subject<ConnectionChangedEvent>();

    /** Token warnings (expired, health-check failure). */
    readonly onTokenWarning$ = new Subject<TokenWarningEvent>();

    /** Hub connection state. */
    readonly onHubConnectionChange$ = new Subject<boolean>();

    constructor(private authService: AuthService) { }


    /** Connect to RebrickableHub with bearer token auth. */
    async connect(): Promise<void> {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/RebrickableSyncSignal', {
                accessTokenFactory: () => this.authService.accessToken ?? ''
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        // Wire up server → client events
        this.connection.on('SyncActivity', (event: SyncActivityEvent) => {
            this.onSyncActivity$.next(event);
        });

        this.connection.on('ConnectionChanged', (event: ConnectionChangedEvent) => {
            this.onConnectionChanged$.next(event);
        });

        this.connection.on('TokenWarning', (event: TokenWarningEvent) => {
            this.onTokenWarning$.next(event);
        });

        this.connection.onreconnecting(() => {
            this.onHubConnectionChange$.next(false);
        });

        this.connection.onreconnected(() => {
            this.onHubConnectionChange$.next(true);
        });

        this.connection.onclose(() => {
            this.onHubConnectionChange$.next(false);
        });

        try {
            await this.connection.start();
            this.onHubConnectionChange$.next(true);
        } catch (err) {
            console.error('Rebrickable SignalR connection failed:', err);
        }
    }


    /** Disconnect from the hub. */
    async disconnect(): Promise<void> {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
            this.onHubConnectionChange$.next(false);
        }
    }

    ngOnDestroy(): void {
        this.disconnect();
    }
}
