import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { initializeApp, FirebaseApp } from 'firebase/app';
import { getMessaging, getToken, onMessage, Messaging } from 'firebase/messaging';
import { BehaviorSubject, Observable } from 'rxjs';

export interface PushNotificationPayload {
    title: string;
    body: string;
    incidentId?: number;
    incidentKey?: string;
    severityId?: number;
    serviceName?: string;
    url?: string;
}

@Injectable({
    providedIn: 'root'
})
export class PushNotificationService {
    private firebaseApp: FirebaseApp | null = null;
    private messaging: Messaging | null = null;
    private currentToken: string | null = null;
    private serviceWorkerRegistration: ServiceWorkerRegistration | null = null;

    // Observable for foreground messages
    private messageSubject = new BehaviorSubject<PushNotificationPayload | null>(null);
    public messages$ = this.messageSubject.asObservable();

    // Observable for permission status
    private permissionSubject = new BehaviorSubject<NotificationPermission>('default');
    public permissionStatus$ = this.permissionSubject.asObservable();

    constructor(private http: HttpClient) {
        // Check initial permission status
        if ('Notification' in window) {
            this.permissionSubject.next(Notification.permission);
        }
    }

    /**
     * Initialize Firebase with the provided config
     */
    async initialize(firebaseConfig: any): Promise<boolean> {
        try {
            if (!('serviceWorker' in navigator)) {
                console.warn('Service workers not supported');
                return false;
            }

            // Initialize Firebase app
            this.firebaseApp = initializeApp(firebaseConfig);

            // Register service worker
            this.serviceWorkerRegistration = await navigator.serviceWorker.register('/firebase-messaging-sw.js');
            console.log('Service worker registered:', this.serviceWorkerRegistration.scope);

            // Wait for service worker to be ready
            await navigator.serviceWorker.ready;

            // Pass Firebase config to the service worker
            if (this.serviceWorkerRegistration.active) {
                this.serviceWorkerRegistration.active.postMessage({
                    type: 'FIREBASE_CONFIG',
                    config: firebaseConfig
                });
            }

            // Initialize messaging
            this.messaging = getMessaging(this.firebaseApp);

            // Set up foreground message handler
            onMessage(this.messaging, (payload) => {
                console.log('Foreground message received:', payload);
                this.messageSubject.next({
                    title: payload.notification?.title || 'New Alert',
                    body: payload.notification?.body || '',
                    incidentId: payload.data?.['incidentId'] ? parseInt(payload.data['incidentId'], 10) : undefined,
                    incidentKey: payload.data?.['incidentKey'],
                    severityId: payload.data?.['severityId'] ? parseInt(payload.data['severityId'], 10) : undefined,
                    serviceName: payload.data?.['serviceName'],
                    url: payload.data?.['url']
                });
            });

            return true;
        } catch (error) {
            console.error('Failed to initialize push notifications:', error);
            return false;
        }
    }

    /**
     * Request notification permission and get FCM token
     */
    async requestPermission(vapidKey: string): Promise<string | null> {
        try {
            if (!this.messaging || !this.serviceWorkerRegistration) {
                console.error('Push notifications not initialized');
                return null;
            }

            // Request permission
            const permission = await Notification.requestPermission();
            this.permissionSubject.next(permission);

            if (permission !== 'granted') {
                console.log('Notification permission denied');
                return null;
            }

            // Get FCM token
            this.currentToken = await getToken(this.messaging, {
                vapidKey: vapidKey,
                serviceWorkerRegistration: this.serviceWorkerRegistration
            });

            if (this.currentToken) {
                console.log('FCM token obtained');
                // Register token with backend
                await this.registerToken(this.currentToken);
            }

            return this.currentToken;
        } catch (error) {
            console.error('Failed to get notification permission:', error);
            return null;
        }
    }

    /**
     * Register the FCM token with the backend
     */
    private async registerToken(token: string): Promise<void> {
        const deviceFingerprint = this.getDeviceFingerprint();

        try {
            await this.http.post('/api/push-tokens/register', {
                token: token,
                deviceFingerprint: deviceFingerprint,
                platform: 'web',
                userAgent: navigator.userAgent
            }).toPromise();

            console.log('Push token registered with backend');
        } catch (error) {
            console.error('Failed to register push token:', error);
        }
    }

    /**
     * Unregister the current token (e.g., on logout)
     */
    async unregisterToken(): Promise<void> {
        if (!this.currentToken) return;

        try {
            await this.http.post('/api/push-tokens/unregister', {
                token: this.currentToken
            }).toPromise();

            this.currentToken = null;
            console.log('Push token unregistered');
        } catch (error) {
            console.error('Failed to unregister push token:', error);
        }
    }

    /**
     * Check if notifications are supported and enabled
     */
    isSupported(): boolean {
        return 'Notification' in window && 'serviceWorker' in navigator;
    }

    /**
     * Check current permission status
     */
    getPermissionStatus(): NotificationPermission {
        if (!('Notification' in window)) {
            return 'denied';
        }
        return Notification.permission;
    }

    /**
     * Generate a simple device fingerprint for token management
     */
    private getDeviceFingerprint(): string {
        const canvas = document.createElement('canvas');
        const gl = canvas.getContext('webgl');
        const debugInfo = gl?.getExtension('WEBGL_debug_renderer_info');
        const renderer = debugInfo ? gl?.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL) : 'unknown';

        const fingerprint = [
            navigator.userAgent,
            navigator.language,
            screen.width + 'x' + screen.height,
            new Date().getTimezoneOffset(),
            renderer
        ].join('|');

        // Simple hash
        let hash = 0;
        for (let i = 0; i < fingerprint.length; i++) {
            const char = fingerprint.charCodeAt(i);
            hash = ((hash << 5) - hash) + char;
            hash = hash & hash;
        }

        return 'web_' + Math.abs(hash).toString(16);
    }

    /**
     * Get current FCM token
     */
    getCurrentToken(): string | null {
        return this.currentToken;
    }
}
