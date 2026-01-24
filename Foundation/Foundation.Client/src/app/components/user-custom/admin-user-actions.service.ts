//
// Admin User Actions Service
//
// AI-Generated: Custom service for admin-only user management actions
// such as password reset, account locking, and unlocking.
//

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError } from 'rxjs';

import { Utilities } from '../../services/utilities';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';
import { SecureEndpointBase } from '../../services/secure-endpoint-base.service';


//
// Response types
//
export interface AdminActionResponse {
    message: string;
}


@Injectable({
    providedIn: 'root'
})
export class AdminUserActionsService extends SecureEndpointBase {

    //
    // API base URL
    //
    private baseUrl: string = '';


    constructor(
        http: HttpClient,
        authService: AuthService,
        alertService: AlertService
    ) {
        super(http, alertService, authService);
        this.baseUrl = Utilities.baseUrl();
    }


    /// <summary>
    ///
    /// Sends a password reset email to the specified user (admin-initiated).
    ///
    /// </summary>
    public sendPasswordReset(userId: bigint | number): Observable<AdminActionResponse> {
        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AdminActionResponse>(
            `${this.baseUrl}/api/Admin/User/${userId}/SendPasswordReset`,
            {},
            { headers: authHeaders }
        ).pipe(
            catchError(error => this.handleError(error, () => this.sendPasswordReset(userId)))
        );
    }


    /// <summary>
    ///
    /// Sets a temporary password for the specified user (admin-initiated).
    ///
    /// </summary>
    public setTemporaryPassword(userId: bigint | number, password: string): Observable<AdminActionResponse> {
        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AdminActionResponse>(
            `${this.baseUrl}/api/Admin/User/${userId}/SetPassword`,
            { password: password },
            { headers: authHeaders }
        ).pipe(
            catchError(error => this.handleError(error, () => this.setTemporaryPassword(userId, password)))
        );
    }


    /// <summary>
    ///
    /// Locks the specified user account (sets active = false).
    ///
    /// </summary>
    public lockAccount(userId: bigint | number): Observable<AdminActionResponse> {
        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AdminActionResponse>(
            `${this.baseUrl}/api/Admin/User/${userId}/Lock`,
            {},
            { headers: authHeaders }
        ).pipe(
            catchError(error => this.handleError(error, () => this.lockAccount(userId)))
        );
    }


    /// <summary>
    ///
    /// Unlocks the specified user account (sets active = true, resets failed login count).
    ///
    /// </summary>
    public unlockAccount(userId: bigint | number): Observable<AdminActionResponse> {
        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AdminActionResponse>(
            `${this.baseUrl}/api/Admin/User/${userId}/Unlock`,
            {},
            { headers: authHeaders }
        ).pipe(
            catchError(error => this.handleError(error, () => this.unlockAccount(userId)))
        );
    }
}
