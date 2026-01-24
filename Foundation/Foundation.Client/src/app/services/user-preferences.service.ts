import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of, BehaviorSubject } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { AuthService } from './auth.service';

/**
 * Service for managing user preferences/settings that persist across sessions.
 * Uses the UserFilters API to store settings in the user's profile.
 */
@Injectable({
    providedIn: 'root'
})
export class UserPreferencesService {

    private preferencesCache: Map<string, any> = new Map();
    private loadedFromServer = false;
    private loadPromise: Promise<void> | null = null;

    constructor(
        private http: HttpClient,
        private authService: AuthService,
        @Inject('BASE_URL') private baseUrl: string
    ) { }

    /**
     * Gets a preference value by key.
     * Falls back to defaultValue if not found.
     */
    async getPreference<T>(key: string, defaultValue: T): Promise<T> {
        await this.ensureLoaded();

        if (this.preferencesCache.has(key)) {
            return this.preferencesCache.get(key) as T;
        }
        return defaultValue;
    }

    /**
     * Sets a preference value and persists to server.
     */
    async setPreference<T>(key: string, value: T): Promise<void> {
        await this.ensureLoaded();

        this.preferencesCache.set(key, value);
        await this.saveToServer();
    }

    /**
     * Removes a preference.
     */
    async removePreference(key: string): Promise<void> {
        await this.ensureLoaded();

        this.preferencesCache.delete(key);
        await this.saveToServer();
    }

    /**
     * Ensures preferences are loaded from server.
     */
    private async ensureLoaded(): Promise<void> {
        if (this.loadedFromServer) {
            return;
        }

        if (this.loadPromise) {
            return this.loadPromise;
        }

        this.loadPromise = this.loadFromServer();
        await this.loadPromise;
        this.loadPromise = null;
    }

    /**
     * Loads all user preferences from the server.
     */
    private async loadFromServer(): Promise<void> {
        try {
            const headers = this.authService.GetAuthenticationHeaders();
            const response = await this.http.get<any>(
                this.baseUrl + 'api/UserFilters/GetUserFilters',
                { headers, responseType: 'json' as 'json' }
            ).toPromise();

            if (response) {
                // The response might be nested in a StringContent wrapper
                let data = response;
                if (typeof response === 'string') {
                    data = JSON.parse(response);
                }

                // Populate cache
                if (data && typeof data === 'object') {
                    for (const key of Object.keys(data)) {
                        this.preferencesCache.set(key, data[key]);
                    }
                }
            }

            this.loadedFromServer = true;
        } catch (error) {
            console.warn('Could not load user preferences, using defaults', error);
            this.loadedFromServer = true;
        }
    }

    /**
     * Saves all preferences to the server.
     */
    private async saveToServer(): Promise<void> {
        try {
            const headers = this.authService.GetAuthenticationHeaders();
            const data: Record<string, any> = {};

            this.preferencesCache.forEach((value, key) => {
                data[key] = value;
            });

            await this.http.post(
                this.baseUrl + 'api/UserFilters/SaveUserFilters',
                data,
                { headers }
            ).toPromise();
        } catch (error) {
            console.error('Could not save user preferences', error);
        }
    }

    /**
     * Clears the local cache (forces reload on next access).
     */
    clearCache(): void {
        this.preferencesCache.clear();
        this.loadedFromServer = false;
    }
}
