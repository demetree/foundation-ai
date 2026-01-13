import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable, OnDestroy } from '@angular/core';
import { Observable, Subject, of, throwError } from 'rxjs';
import { catchError, map, shareReplay, takeUntil, tap } from 'rxjs/operators';
import { AlertService } from './alert.service';
import { AuthService } from './auth.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';


//
// Response interfaces for API calls
//
export interface SettingResponse {
    key: string;
    value: string | null;
    success?: boolean;
}

export interface FavouriteItem {
    entity: string;
    id: number;
    description: string | null;
    sequence: number;
}

export interface MostRecentItem {
    entity: string;
    id: number;
    description: string | null;
    sequence: number;
    timestamp?: string;
}


/**
 *
 * Service for managing user-specific settings.
 *
 * Provides methods for getting and setting string/object settings that are persisted
 * per-user in the SecurityUser.settings JSON field on the server.
 *
 * Also provides access to user favourites and most recently accessed items.
 *
 */
@Injectable({
    providedIn: 'root'
})
export class UserSettingsService extends SecureEndpointBase implements OnDestroy {

    //
    // Local cache for settings to reduce API calls
    //
    private settingsCache: Map<string, string | null> = new Map();

    //
    // Cache for favourites and most recents (short-lived)
    //
    private favouritesCache$: Observable<FavouriteItem[]> | null = null;
    private mostRecentsCache$: Observable<MostRecentItem[]> | null = null;

    //
    // Subject used for cleanup of internal subscriptions
    //
    private destroy$ = new Subject<void>();


    constructor(
        http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        @Inject('BASE_URL') private baseUrl: string
    ) {
        super(http, alertService, authService);
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    /**
     * Gets a string setting value for the current user.
     *
     * @param key The setting key/name
     * @param useCache Whether to use cached value if available (default: true)
     * @returns Observable with the setting value, or null if not set
     */
    public getStringSetting(key: string, useCache: boolean = true): Observable<string | null> {

        //
        // Check cache first if enabled
        //
        if (useCache === true && this.settingsCache.has(key)) {
            return of(this.settingsCache.get(key) ?? null);
        }

        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SettingResponse>(
            this.baseUrl + 'api/UserSettings/' + encodeURIComponent(key),
            { headers: authHeaders }
        ).pipe(
            map(response => response.value),
            tap(value => {
                //
                // Cache the result
                //
                this.settingsCache.set(key, value);
            }),
            catchError(error => {
                return this.handleError(error, () => this.getStringSetting(key, useCache));
            }),
            catchError(error => {
                this.alertService.showHttpErrorMessage('Unable to get user setting', error);
                return of(null);
            })
        );
    }


    /**
     * Sets a string setting value for the current user.
     *
     * @param key The setting key/name
     * @param value The value to set (can be null to clear)
     * @returns Observable with success indicator
     */
    public setStringSetting(key: string, value: string | null): Observable<boolean> {

        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SettingResponse>(
            this.baseUrl + 'api/UserSettings/' + encodeURIComponent(key),
            { value: value },
            { headers: authHeaders }
        ).pipe(
            map(response => response.success === true),
            tap(success => {
                if (success === true) {
                    //
                    // Update the cache with the new value
                    //
                    this.settingsCache.set(key, value);
                }
            }),
            catchError(error => {
                return this.handleError(error, () => this.setStringSetting(key, value));
            }),
            catchError(error => {
                this.alertService.showHttpErrorMessage('Unable to save user setting', error);
                return of(false);
            })
        );
    }


    /**
     * Gets all user settings as an object.
     *
     * @returns Observable with all settings as a key-value object
     */
    public getAllSettings(): Observable<Record<string, any>> {

        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Record<string, any>>(
            this.baseUrl + 'api/UserSettings',
            { headers: authHeaders }
        ).pipe(
            tap(settings => {
                //
                // Update cache with all retrieved settings
                //
                if (settings != null) {
                    for (const [key, value] of Object.entries(settings)) {
                        if (typeof value === 'string') {
                            this.settingsCache.set(key, value);
                        }
                    }
                }
            }),
            catchError(error => {
                return this.handleError(error, () => this.getAllSettings());
            }),
            catchError(error => {
                this.alertService.showHttpErrorMessage('Unable to get user settings', error);
                return of({});
            })
        );
    }


    /**
     * Gets the user's favourites list.
     *
     * @param entityType Optional entity type filter (e.g., "Contact", "ScheduledEvent")
     * @param forceRefresh Force refresh from server (bypasses cache)
     * @returns Observable with list of favourited items
     */
    public getFavourites(entityType: string | null = null, forceRefresh: boolean = false): Observable<FavouriteItem[]> {

        //
        // Use cached observable if available and not forcing refresh
        //
        if (forceRefresh === false && this.favouritesCache$ != null && entityType == null) {
            return this.favouritesCache$;
        }

        const authHeaders = this.authService.GetAuthenticationHeaders();

        let url = this.baseUrl + 'api/UserSettings/Favourites';

        if (entityType != null) {
            url += '?entityType=' + encodeURIComponent(entityType);
        }

        const request$ = this.http.get<FavouriteItem[]>(url, { headers: authHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.getFavourites(entityType, forceRefresh));
            }),
            catchError(error => {
                this.alertService.showHttpErrorMessage('Unable to get favourites', error);
                return of([]);
            }),
            shareReplay({ bufferSize: 1, refCount: true })
        );

        //
        // Cache the observable if no entity filter
        //
        if (entityType == null) {
            this.favouritesCache$ = request$;
        }

        return request$;
    }


    /**
     * Gets the user's most recently accessed items.
     *
     * @param entityType Optional entity type filter
     * @param forceRefresh Force refresh from server (bypasses cache)
     * @returns Observable with list of recently accessed items
     */
    public getMostRecents(entityType: string | null = null, forceRefresh: boolean = false): Observable<MostRecentItem[]> {

        //
        // Use cached observable if available and not forcing refresh
        //
        if (forceRefresh === false && this.mostRecentsCache$ != null && entityType == null) {
            return this.mostRecentsCache$;
        }

        const authHeaders = this.authService.GetAuthenticationHeaders();

        let url = this.baseUrl + 'api/UserSettings/MostRecents';

        if (entityType != null) {
            url += '?entityType=' + encodeURIComponent(entityType);
        }

        const request$ = this.http.get<MostRecentItem[]>(url, { headers: authHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.getMostRecents(entityType, forceRefresh));
            }),
            catchError(error => {
                this.alertService.showHttpErrorMessage('Unable to get recent items', error);
                return of([]);
            }),
            shareReplay({ bufferSize: 1, refCount: true })
        );

        //
        // Cache the observable if no entity filter
        //
        if (entityType == null) {
            this.mostRecentsCache$ = request$;
        }

        return request$;
    }


    /**
     * Clears all cached settings and favourites.
     * Call this when the user logs out or when you need to force a refresh.
     */
    public clearAllCaches(): void {
        this.settingsCache.clear();
        this.favouritesCache$ = null;
        this.mostRecentsCache$ = null;
    }


    /**
     * Invalidates the favourites cache so the next call fetches fresh data.
     */
    public invalidateFavouritesCache(): void {
        this.favouritesCache$ = null;
    }


    /**
     * Invalidates the most recents cache so the next call fetches fresh data.
     */
    public invalidateMostRecentsCache(): void {
        this.mostRecentsCache$ = null;
    }
}
