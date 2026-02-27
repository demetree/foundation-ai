import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';

//
// Manual Generator SignalR Service
//
// Wraps the ManualGeneratorHub for streaming manual generation progress.
// Connects to /ManualGeneratorSignal with bearer token authentication.
//
// Usage:
//   this.signalr.connect();
//   this.signalr.onStepProgress$.subscribe(p => ...);
//   this.signalr.onComplete$.subscribe(result => ...);
//   this.signalr.onError$.subscribe(err => ...);
//   this.signalr.generateManual(generationId, options);
//

export interface StepProgressEvent {
    step: number;
    total: number;
    previewBase64: string;
}

export interface GenerationCompleteEvent {
    format: string;       // "html" or "pdf"
    html: string | null;
    pdfBase64: string | null;      // Legacy (small PDFs)
    downloadUrl: string | null;    // Preferred for PDF (avoids SignalR size limits)
    totalSteps: number;
    totalParts: number;
    renderTimeMs: number;
}

export interface ManualOptionsDto {
    pageSize?: string;
    imageSize?: number;
    elevation?: number;
    azimuth?: number;
    renderEdges?: boolean;
    smoothShading?: boolean;
    outputFormat?: string;   // "html" or "pdf"
    renderer?: string;       // "rasterizer" or "raytrace"
    enablePbr?: boolean;
    exposure?: number;
    aperture?: number;
}

@Injectable({
    providedIn: 'root'
})
export class ManualGeneratorSignalrService implements OnDestroy {

    private connection: signalR.HubConnection | null = null;

    /** Emits progress for each rendered step. */
    readonly onStepProgress$ = new Subject<StepProgressEvent>();

    /** Emits when the full manual HTML has been generated. */
    readonly onComplete$ = new Subject<GenerationCompleteEvent>();

    /** Emits error messages from the hub. */
    readonly onError$ = new Subject<string>();

    /** Emits the current connection state. */
    readonly onConnectionChange$ = new Subject<boolean>();

    constructor(private authService: AuthService) { }


    /** Connect to the ManualGeneratorHub with bearer token auth. */
    async connect(): Promise<void> {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/ManualGeneratorSignal', {
                accessTokenFactory: () => this.authService.accessToken ?? ''
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        // Manual generation can take several minutes for large models
        this.connection.serverTimeoutInMilliseconds = 10 * 60 * 1000;  // 10 minutes
        this.connection.keepAliveIntervalInMilliseconds = 15 * 1000;   // 15 seconds

        // Wire up server → client events
        this.connection.on('StepProgress', (event: StepProgressEvent) => {
            this.onStepProgress$.next(event);
        });

        this.connection.on('GenerationComplete', (event: GenerationCompleteEvent) => {
            this.onComplete$.next(event);
        });

        this.connection.on('GenerationError', (message: string) => {
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
            console.error('ManualGenerator SignalR connection failed:', err);
            this.onError$.next('Failed to connect to the manual generator service.');
        }
    }


    /** Invoke manual generation on the hub. Progress streams back via onStepProgress$. */
    async generateManual(generationId: string, options: ManualOptionsDto): Promise<void> {
        if (this.connection?.state !== signalR.HubConnectionState.Connected) {
            await this.connect();
        }

        try {
            await this.connection!.invoke('GenerateManual', generationId, options);
        } catch (err) {
            console.error('Failed to start manual generation:', err);
            this.onError$.next('Failed to start manual generation. Please try again.');
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
