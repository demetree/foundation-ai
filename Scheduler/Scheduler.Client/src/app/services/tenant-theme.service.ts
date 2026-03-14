//
// Tenant Theme Service
//
// AI-DEVELOPED: Service for fetching the tenant's default theme.
//
// Attempts to read the 'scheduler-theme' key from the TenantSettings API.
// If the user lacks admin permissions or the setting isn't configured,
// returns null gracefully so the ThemeService can fall back to the default.
//
// Note: The TenantSettings API requires admin-level access (permission level 3)
// and a tenantId parameter. For non-admin users, this will silently return null,
// and they'll get the built-in default theme unless they set a personal preference.
//
// The tenant default theme is managed through Foundation's admin UI.
//

import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';

import { AuthService } from './auth.service';


/**
 * Response shape from the TenantSettings API.
 */
interface TenantSettingValue {
    key: string;
    value: string | null;
}


/**
 * TenantThemeService — reads the tenant-level default theme setting.
 *
 * Calls the Foundation TenantSettings API to get the 'scheduler-theme' value
 * for the current user's tenant. Returns null gracefully if the user lacks
 * admin access or the setting isn't configured.
 */
@Injectable({ providedIn: 'root' })
export class TenantThemeService {

    private static readonly SETTING_KEY = 'scheduler-theme';


    constructor(
        private http: HttpClient,
        private authService: AuthService,
        @Inject('BASE_URL') private baseUrl: string
    ) { }


    /**
     * Gets the tenant's default theme ID.
     *
     * Returns null if no tenant theme is set, the user lacks admin access,
     * or the API call fails for any reason.
     */
    public getTenantDefaultTheme(): Observable<string | null> {

        //
        // We need to look up the tenant ID from the SecurityTenant table.
        // First, try to get the current user's tenant info.
        //
        let authHeaders = new HttpHeaders({
            Authorization: 'Bearer ' + this.authService.accessToken
        });

        //
        // Try to get the tenant ID by looking up the current user's tenant.
        // The /api/SecurityTenant endpoint can be used to find the current tenant.
        // If this fails (non-admin user), we gracefully return null.
        //
        return this.http.get<any[]>(
            this.baseUrl + 'api/SecurityTenant',
            { headers: authHeaders }
        ).pipe(
            switchMap(tenants => {

                //
                // Find the first (or matching) tenant
                //
                if (tenants == null || tenants.length === 0) {
                    return of(null);
                }

                //
                // Use the first tenant's ID to query its settings
                //
                let tenantId = tenants[0]?.id;

                if (tenantId == null) {
                    return of(null);
                }

                let params = new HttpParams().set('tenantId', tenantId.toString());

                return this.http.get<TenantSettingValue>(
                    this.baseUrl + 'api/TenantSettings/' + encodeURIComponent(TenantThemeService.SETTING_KEY),
                    {
                        headers: authHeaders,
                        params: params
                    }
                ).pipe(
                    map(response => response.value),
                    catchError(() => of(null))
                );
            }),
            catchError(() => {

                //
                // Silently return null — user likely lacks admin permissions
                // or the API is not available. The ThemeService will fall back
                // to the default theme.
                //
                return of(null);
            })
        );
    }
}
