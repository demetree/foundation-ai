///
/// AI-Developed: BrickbergPreferenceService — manages the user's Brickberg Terminal opt-in preference.
///
/// Reads/writes the 'bmc-brickberg-enabled' setting via Foundation's UserSettings API
/// (GET/PUT /api/UserSettings/{key}), which persists the value server-side in the
/// SecurityUser.settings JSON column.  This gives cross-device sync without schema changes.
///
/// Default is false — Brickberg is hidden by default.  Power users can enable it
/// in Profile Settings.
///
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { AuthService } from './auth.service';


const SETTING_KEY = 'bmc-brickberg-enabled';


@Injectable({
    providedIn: 'root'
})
export class BrickbergPreferenceService {

    private enabledSubject = new BehaviorSubject<boolean>(false);

    /** Observable stream — subscribe for reactive updates */
    public isEnabled$ = this.enabledSubject.asObservable();


    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) {
        // Auto-load preference when the service is first injected
        this.loadPreference();
    }


    /** Synchronous getter for template *ngIf bindings */
    get isEnabled(): boolean {
        return this.enabledSubject.value;
    }


    /**
     * Loads the Brickberg preference from the Foundation UserSettings API.
     * Falls back to false (disabled) if the setting doesn't exist yet.
     */
    loadPreference(): void {

        if (!this.authService.isLoggedIn) {
            this.enabledSubject.next(false);
            return;
        }

        const headers = this.authService.GetAuthenticationHeaders();

        this.http.get<{ key: string; value: string | null }>(`/api/UserSettings/${SETTING_KEY}`, { headers }).pipe(
            catchError(() => {
                // Setting doesn't exist yet or API error — default to disabled
                this.enabledSubject.next(false);
                return [];
            })
        ).subscribe(result => {
            if (result && result.value != null) {
                this.enabledSubject.next(result.value === 'true');
            } else {
                this.enabledSubject.next(false);
            }
        });
    }


    /**
     * Saves the Brickberg preference to the Foundation UserSettings API.
     * Emits the new value immediately for responsive UI.
     */
    setEnabled(value: boolean): void {

        // Emit immediately for responsive UI
        this.enabledSubject.next(value);

        if (!this.authService.isLoggedIn) {
            return;
        }

        const headers = this.authService.GetAuthenticationHeaders();

        this.http.put(`/api/UserSettings/${SETTING_KEY}`, { Value: value.toString() }, { headers }).pipe(
            catchError(err => {
                console.error('Failed to save Brickberg preference:', err);
                return [];
            })
        ).subscribe();
    }
}
