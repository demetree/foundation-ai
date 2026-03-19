//
// scheduler-signalr.service.ts
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Connects to /SchedulerSignal for real-time event change notifications.
// Broadcasts: EventsChanged (action, eventId, timestamp)
//
// Usage in the SchedulerCalendarComponent:
//   this.schedulerSignalr.connect();
//   this.schedulerSignalr.onEventsChanged$.subscribe(event => this.loadEvents());
//

import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';


export interface EventsChangedPayload {
    action: string;         // "created" | "updated" | "deleted"
    eventId: number;
    timestamp: string;
}


@Injectable({
    providedIn: 'root'
})
export class SchedulerSignalrService implements OnDestroy {

    private connection: signalR.HubConnection | null = null;

    /** Real-time event change notifications from other schedulers. */
    readonly onEventsChanged$ = new Subject<EventsChangedPayload>();

    /** Hub connection state changes. */
    readonly onHubConnectionChange$ = new Subject<boolean>();

    constructor(private authService: AuthService) { }


    /** Connect to SchedulerHub with bearer token auth. */
    async connect(): Promise<void> {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/SchedulerSignal', {
                accessTokenFactory: () => this.authService.accessToken ?? ''
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        // Wire up server → client events
        this.connection.on('EventsChanged', (event: EventsChangedPayload) => {
            this.onEventsChanged$.next(event);
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
            console.error('Scheduler SignalR connection failed:', err);
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
