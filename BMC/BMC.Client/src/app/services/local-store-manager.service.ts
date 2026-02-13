import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LocalStoreManager {

    private static syncListenerInitialised = false;

    initialiseStorageSyncListener() {
        if (LocalStoreManager.syncListenerInitialised) return;
        LocalStoreManager.syncListenerInitialised = true;

        window.addEventListener('storage', this.sessionStorageTransferHandler, false);
    }

    private sessionStorageTransferHandler = (event: StorageEvent) => {
        if (!event.key) return;
        // Handle cross-tab sync if needed
    };

    savePermanentData(key: string, data: any) {
        localStorage.setItem(key, JSON.stringify(data));
    }

    getPermanentData(key: string): any {
        const data = localStorage.getItem(key);
        return data ? JSON.parse(data) : null;
    }

    deletePermanentData(key: string) {
        localStorage.removeItem(key);
    }

    saveSessionData(key: string, data: any) {
        sessionStorage.setItem(key, JSON.stringify(data));
    }

    getSessionData(key: string): any {
        const data = sessionStorage.getItem(key);
        return data ? JSON.parse(data) : null;
    }

    deleteSessionData(key: string) {
        sessionStorage.removeItem(key);
    }
}
