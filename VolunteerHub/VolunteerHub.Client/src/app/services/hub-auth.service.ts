import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

const SESSION_KEY = 'vh_session';
const SESSION_EXPIRY_KEY = 'vh_session_expiry';
const SESSION_USER_KEY = 'vh_user_name';

export interface SessionInfo {
    sessionToken: string;
    expiresAt: string;
    userName: string;
}

@Injectable({ providedIn: 'root' })
export class HubAuthService {

    private _isLoggedIn$ = new BehaviorSubject<boolean>(this.hasValidSession());

    isLoggedIn$ = this._isLoggedIn$.asObservable();

    constructor(
        private http: HttpClient,
        private router: Router
    ) { }


    // ─────── Session management ───────

    get sessionToken(): string | null {
        return localStorage.getItem(SESSION_KEY);
    }

    get userName(): string | null {
        return localStorage.getItem(SESSION_USER_KEY);
    }

    hasValidSession(): boolean {
        const token = localStorage.getItem(SESSION_KEY);
        const expiry = localStorage.getItem(SESSION_EXPIRY_KEY);

        if (!token || !expiry) return false;

        return new Date(expiry) > new Date();
    }


    // ─────── OTP Flow ───────

    /**
     * Step 1: Request an OTP code — server sends it via email/SMS.
     */
    async requestCode(identifier: string): Promise<{ message: string }> {
        return firstValueFrom(
            this.http.post<{ message: string }>(
                `${environment.apiBaseUrl}/api/volunteerhub/auth/request-code`,
                { identifier }
            )
        );
    }

    /**
     * Step 2: Verify OTP code → receive session token.
     */
    async verifyCode(identifier: string, code: string): Promise<SessionInfo> {
        const result = await firstValueFrom(
            this.http.post<SessionInfo>(
                `${environment.apiBaseUrl}/api/volunteerhub/auth/verify-code`,
                { identifier, code }
            )
        );

        // Store session
        localStorage.setItem(SESSION_KEY, result.sessionToken);
        localStorage.setItem(SESSION_EXPIRY_KEY, result.expiresAt);
        localStorage.setItem(SESSION_USER_KEY, result.userName);

        this._isLoggedIn$.next(true);

        return result;
    }


    // ─────── Logout ───────

    logout(): void {
        localStorage.removeItem(SESSION_KEY);
        localStorage.removeItem(SESSION_EXPIRY_KEY);
        localStorage.removeItem(SESSION_USER_KEY);
        this._isLoggedIn$.next(false);
        this.router.navigate(['/login']);
    }
}
