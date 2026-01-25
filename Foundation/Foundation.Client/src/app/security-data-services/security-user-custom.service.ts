/**
 * Security User Custom Service
 * 
 * Contains custom security user operations that are NOT auto-generated
 * and should survive code regeneration.
 * 
 * This service is separate from the auto-generated security-user.service.ts
 * to prevent custom methods from being clobbered during code regeneration.
 */
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, catchError, map, tap } from 'rxjs';

import { AuthService } from '../services/auth.service';
import { AlertService } from '../services/alert.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';
import { SecurityUserService, SecurityUserData } from './security-user.service';
import { AdminCreateUserRequest } from '../models/create-user-model';

@Injectable({
    providedIn: 'root'
})
export class SecurityUserCustomService extends SecureEndpointBase {

    private static _instance: SecurityUserCustomService;

    constructor(
        http: HttpClient,
        alertService: AlertService,
        authService: AuthService,
        private securityUserService: SecurityUserService,
        @Inject('BASE_URL') private baseUrl: string
    ) {
        super(http, alertService, authService);
        SecurityUserCustomService._instance = this;
    }

    public static get Instance(): SecurityUserCustomService {
        return SecurityUserCustomService._instance;
    }

    /**
     * Creates a new user with password via the Admin endpoint.
     * This is the preferred method for creating users as it handles
     * password securely in a single atomic transaction.
     * 
     * @param request The admin create user request containing account details and password
     * @returns Observable of the created SecurityUserData
     */
    public AdminCreateUser(request: AdminCreateUserRequest): Observable<SecurityUserData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityUserData>(this.baseUrl + 'api/Admin/CreateUser', request, { headers: authenticationHeaders }).pipe(
            tap(() => this.securityUserService.ClearAllCaches()),
            map(raw => this.securityUserService.ReviveSecurityUser(raw)),
            catchError(error => {
                return this.handleError(error, () => this.AdminCreateUser(request));
            }));
    }
}
