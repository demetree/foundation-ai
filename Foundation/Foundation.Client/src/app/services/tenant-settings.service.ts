//
// Tenant Settings Service
//
// Angular service for managing tenant-level settings via the TenantSettings API.
// Settings are stored as JSON in the SecurityTenant.settings field.
//

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';

export interface TenantSettingValue {
    key: string;
    value: string | null;
}

export interface TenantSetSettingResponse {
    key: string;
    value: string | null;
    success: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class TenantSettingsService {

    private readonly baseUrl = '/api/TenantSettings';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }


    private get authHeaders(): HttpHeaders {
        return new HttpHeaders({
            Authorization: 'Bearer ' + this.authService.accessToken
        });
    }


    /**
     * Gets all settings for a tenant as a key-value object.
     */
    getAllSettings(tenantId: number): Observable<Record<string, any>> {
        const params = new HttpParams().set('tenantId', tenantId.toString());
        return this.http.get<Record<string, any>>(this.baseUrl, {
            headers: this.authHeaders,
            params
        }).pipe(
            catchError(error => {
                console.error(`Error fetching tenant settings for tenant ${tenantId}:`, error);
                return of({});
            })
        );
    }


    /**
     * Gets a specific setting value by key for a tenant.
     */
    getSetting(tenantId: number, key: string): Observable<string | null> {
        const params = new HttpParams().set('tenantId', tenantId.toString());
        return this.http.get<TenantSettingValue>(`${this.baseUrl}/${encodeURIComponent(key)}`, {
            headers: this.authHeaders,
            params
        }).pipe(
            map(response => response.value),
            catchError(error => {
                console.error(`Error fetching tenant setting '${key}' for tenant ${tenantId}:`, error);
                return of(null);
            })
        );
    }


    /**
     * Sets a setting value by key for a tenant.
     */
    setSetting(tenantId: number, key: string, value: string | null): Observable<boolean> {
        const params = new HttpParams().set('tenantId', tenantId.toString());
        return this.http.put<TenantSetSettingResponse>(
            `${this.baseUrl}/${encodeURIComponent(key)}`,
            { value },
            {
                headers: this.authHeaders,
                params
            }
        ).pipe(
            map(response => response.success),
            catchError(error => {
                console.error(`Error setting tenant setting '${key}' for tenant ${tenantId}:`, error);
                return of(false);
            })
        );
    }


    /**
     * Deletes a setting by setting its value to null.
     */
    deleteSetting(tenantId: number, key: string): Observable<boolean> {
        return this.setSetting(tenantId, key, null);
    }
}
