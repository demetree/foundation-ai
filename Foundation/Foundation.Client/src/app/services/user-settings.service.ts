//
// User Settings Service
//
// Angular service for managing user-specific settings via the UserSettings API.
// Settings are stored as JSON in the SecurityUser.settings field.
//

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';

export interface SettingValue {
    key: string;
    value: string | null;
}

export interface SetSettingResponse {
    key: string;
    value: string | null;
    success: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class UserSettingsService {

    private readonly baseUrl = '/api/UserSettings';

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
     * Gets all settings for a user as a key-value object.
     */
    getAllSettings(): Observable<Record<string, any>> {
        return this.http.get<Record<string, any>>(this.baseUrl, {
            headers: this.authHeaders
        }).pipe(
            catchError(error => {
                console.error('Error fetching user settings:', error);
                return of({});
            })
        );
    }


    /**
     * Gets a specific setting value by key.
     */
    getSetting(key: string): Observable<string | null> {
        return this.http.get<SettingValue>(`${this.baseUrl}/${encodeURIComponent(key)}`, {
            headers: this.authHeaders
        }).pipe(
            map(response => response.value),
            catchError(error => {
                console.error(`Error fetching user setting '${key}':`, error);
                return of(null);
            })
        );
    }


    /**
     * Sets a setting value by key.
     */
    setSetting(key: string, value: string | null): Observable<boolean> {
        return this.http.put<SetSettingResponse>(`${this.baseUrl}/${encodeURIComponent(key)}`, { value }, {
            headers: this.authHeaders
        }).pipe(
            map(response => response.success),
            catchError(error => {
                console.error(`Error setting user setting '${key}':`, error);
                return of(false);
            })
        );
    }


    /**
     * Deletes a setting by setting its value to null.
     */
    deleteSetting(key: string): Observable<boolean> {
        return this.setSetting(key, null);
    }
}
