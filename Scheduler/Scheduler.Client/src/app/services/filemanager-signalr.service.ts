//
// filemanager-signalr.service.ts
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Connects to /FileManagerSignal for real-time document and folder change
// notifications. Other users' uploads, deletes, moves, and folder changes
// trigger auto-refresh in the file manager UI.
//
// Usage:
//   this.fmSignalr.connect();
//   this.fmSignalr.onDocumentChanged$.subscribe(() => this.loadDocuments());
//

import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';


export interface FileManagerSignalPayload {
    action: string;       // "uploaded" | "deleted" | "restored" | "moved" | etc.
    documentId?: number;
    folderId?: number;
    count?: number;
}


@Injectable({
    providedIn: 'root'
})
export class FileManagerSignalrService implements OnDestroy {

    private connection: signalR.HubConnection | null = null;

    /** Real-time document change notifications. */
    readonly onDocumentChanged$ = new Subject<FileManagerSignalPayload>();

    /** Real-time document deletion notifications. */
    readonly onDocumentDeleted$ = new Subject<FileManagerSignalPayload>();

    /** Real-time folder change notifications. */
    readonly onFolderChanged$ = new Subject<FileManagerSignalPayload>();

    /** Real-time folder deletion notifications. */
    readonly onFolderDeleted$ = new Subject<FileManagerSignalPayload>();

    /** Hub connection state changes. */
    readonly onConnectionChange$ = new Subject<boolean>();

    constructor(private authService: AuthService) { }


    /** Connect to FileManagerHub with bearer token auth. */
    async connect(): Promise<void> {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/FileManagerSignal', {
                accessTokenFactory: () => this.authService.accessToken ?? ''
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        // Wire server → client signals
        this.connection.on('DocumentChanged', (event: FileManagerSignalPayload) => {
            this.onDocumentChanged$.next(event);
        });

        this.connection.on('DocumentDeleted', (event: FileManagerSignalPayload) => {
            this.onDocumentDeleted$.next(event);
        });

        this.connection.on('FolderChanged', (event: FileManagerSignalPayload) => {
            this.onFolderChanged$.next(event);
        });

        this.connection.on('FolderDeleted', (event: FileManagerSignalPayload) => {
            this.onFolderDeleted$.next(event);
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
            console.error('FileManager SignalR connection failed:', err);
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
