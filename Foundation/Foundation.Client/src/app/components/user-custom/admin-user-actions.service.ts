//
// Admin User Actions Service
//
// AI-Generated: Custom service for admin-only user management actions
// such as password reset, account locking, and unlocking.
//

import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Utilities } from '../../services/utilities';


//
// Response types
//
export interface AdminActionResponse {
    message: string;
}


@Injectable({
    providedIn: 'root'
})
export class AdminUserActionsService {

    //
    // API base URL
    //
    private baseUrl: string = '';


    constructor(private http: HttpClient) {
        this.baseUrl = Utilities.baseUrl();
    }


    /// <summary>
    ///
    /// Sends a password reset email to the specified user (admin-initiated).
    ///
    /// </summary>
    public sendPasswordReset(userId: bigint | number): Observable<AdminActionResponse> {
        return this.http.post<AdminActionResponse>(
            `${this.baseUrl}api/Admin/User/${userId}/SendPasswordReset`,
            {}
        );
    }


    /// <summary>
    ///
    /// Sets a temporary password for the specified user (admin-initiated).
    ///
    /// </summary>
    public setTemporaryPassword(userId: bigint | number, password: string): Observable<AdminActionResponse> {
        return this.http.post<AdminActionResponse>(
            `${this.baseUrl}api/Admin/User/${userId}/SetPassword`,
            { password: password }
        );
    }


    /// <summary>
    ///
    /// Locks the specified user account (sets active = false).
    ///
    /// </summary>
    public lockAccount(userId: bigint | number): Observable<AdminActionResponse> {
        return this.http.post<AdminActionResponse>(
            `${this.baseUrl}api/Admin/User/${userId}/Lock`,
            {}
        );
    }


    /// <summary>
    ///
    /// Unlocks the specified user account (sets active = true, resets failed login count).
    ///
    /// </summary>
    public unlockAccount(userId: bigint | number): Observable<AdminActionResponse> {
        return this.http.post<AdminActionResponse>(
            `${this.baseUrl}api/Admin/User/${userId}/Unlock`,
            {}
        );
    }
}
