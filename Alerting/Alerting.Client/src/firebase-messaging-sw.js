// firebase-messaging-sw.js
// Service Worker for Firebase Cloud Messaging
// This file must be in the root of the public directory

importScripts('https://www.gstatic.com/firebasejs/10.7.0/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.7.0/firebase-messaging-compat.js');

// Initialize Firebase - config will be passed via postMessage
let messagingInitialized = false;

self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'FIREBASE_CONFIG') {
        if (!messagingInitialized) {
            firebase.initializeApp(event.data.config);

            const messaging = firebase.messaging();

            // Handle background messages
            messaging.onBackgroundMessage((payload) => {
                console.log('[SW] Received background message:', payload);

                const notificationTitle = payload.notification?.title || 'New Alert';
                const notificationOptions = {
                    body: payload.notification?.body || 'You have a new notification',
                    icon: '/assets/icons/alert-icon-192.png',
                    badge: '/assets/icons/badge-icon-72.png',
                    tag: payload.data?.incidentId ? `incident-${payload.data.incidentId}` : undefined,
                    renotify: true,
                    requireInteraction: payload.data?.severityId <= 2, // Critical/High require interaction
                    data: payload.data
                };

                self.registration.showNotification(notificationTitle, notificationOptions);
            });

            messagingInitialized = true;
            console.log('[SW] Firebase messaging initialized');
        }
    }
});

// Handle notification click
self.addEventListener('notificationclick', (event) => {
    console.log('[SW] Notification click:', event);

    event.notification.close();

    const urlToOpen = event.notification.data?.url || '/';

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientList) => {
            // Check if there's already an open window
            for (const client of clientList) {
                if (client.url.includes(self.location.origin) && 'focus' in client) {
                    client.navigate(urlToOpen);
                    return client.focus();
                }
            }
            // Open a new window if none exists
            if (clients.openWindow) {
                return clients.openWindow(urlToOpen);
            }
        })
    );
});
