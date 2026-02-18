import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';

//
// AI Chat SignalR Service
//
// Wraps the AiChatHub for streaming chat responses token-by-token.
// Connects to /AiChatSignal with bearer token authentication.
//
// Usage:
//   this.signalr.connect();
//   this.signalr.onToken$.subscribe(token => ...);
//   this.signalr.onComplete$.subscribe(() => ...);
//   this.signalr.onError$.subscribe(err => ...);
//   this.signalr.sendMessage('What parts have gears?');
//

@Injectable({
    providedIn: 'root'
})
export class AiChatSignalrService implements OnDestroy {

    private connection: signalR.HubConnection | null = null;

    /** Emits each token fragment as the AI generates it. */
    readonly onToken$ = new Subject<string>();

    /** Emits when the AI has finished generating the full response. */
    readonly onComplete$ = new Subject<void>();

    /** Emits error messages from the hub. */
    readonly onError$ = new Subject<string>();

    /** Emits the current connection state. */
    readonly onConnectionChange$ = new Subject<boolean>();

    constructor(private authService: AuthService) { }


    /** Connect to the AiChatHub with bearer token auth. */
    async connect(): Promise<void> {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return; // already connected
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/AiChatSignal', {
                accessTokenFactory: () => this.authService.accessToken ?? ''
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        // Wire up server → client events
        this.connection.on('ReceiveToken', (token: string) => {
            this.onToken$.next(token);
        });

        this.connection.on('ChatComplete', () => {
            this.onComplete$.next();
        });

        this.connection.on('ChatError', (message: string) => {
            this.onError$.next(message);
        });

        this.connection.onreconnecting(() => {
            this.onConnectionChange$.next(false);
        });

        this.connection.onreconnected(() => {
            this.onConnectionChange$.next(true);
        });

        this.connection.onclose(() => {
            this.onConnectionChange$.next(false);
        });

        try {
            await this.connection.start();
            this.onConnectionChange$.next(true);
        } catch (err) {
            console.error('SignalR connection failed:', err);
            this.onError$.next('Failed to connect to the AI chat service.');
        }
    }


    /** Send a chat question to the hub. Tokens stream back via onToken$. */
    async sendMessage(question: string): Promise<void> {
        if (this.connection?.state !== signalR.HubConnectionState.Connected) {
            await this.connect();
        }

        try {
            await this.connection!.invoke('SendMessage', question);
        } catch (err) {
            console.error('Failed to send message:', err);
            this.onError$.next('Failed to send the message. Please try again.');
        }
    }


    /** Disconnect from the hub. */
    async disconnect(): Promise<void> {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
            this.onConnectionChange$.next(false);
        }
    }

    ngOnDestroy(): void {
        this.disconnect();
    }
}
