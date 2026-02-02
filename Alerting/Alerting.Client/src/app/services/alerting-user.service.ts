import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { SecureEndpointBase } from './secure-endpoint-base.service';
import { AlertService } from './alert.service';
import { AuthService } from './auth.service';

/**
 * User DTO matching backend UsersController.UserDto
 */
export interface AlertingUser {
    objectGuid: string;
    accountName: string;
    firstName: string;
    middleName: string;
    lastName: string;
    displayName: string;
    emailAddress: string;
    cellPhoneNumber: string;
    phoneNumber: string;
    teamGuid: string | null;
}

/**
 * Team DTO matching backend UsersController.TeamDto
 */
export interface AlertingTeam {
    objectGuid: string;
    name: string;
    description: string;
}

/**
 * Service for fetching users and teams applicable for the Alerting module.
 * These are users with Alerting module access in the current tenant.
 * 
 * Extends SecureEndpointBase for proper authentication handling.
 */
@Injectable({
    providedIn: 'root'
})
export class AlertingUserService extends SecureEndpointBase {

    constructor(
        http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        @Inject('BASE_URL') private baseUrl: string
    ) {
        super(http, alertService, authService);
    }

    /**
     * Get all users with Alerting module access in the current tenant.
     */
    getUsers(): Observable<AlertingUser[]> {
        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AlertingUser[]>(`${this.baseUrl}api/users`, {
            headers: authHeaders
        }).pipe(
            catchError(error => this.handleError(error, () => this.getUsers()))
        );
    }

    /**
     * Get a specific user by their object GUID.
     */
    getUser(userGuid: string): Observable<AlertingUser> {
        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AlertingUser>(`${this.baseUrl}api/users/${userGuid}`, {
            headers: authHeaders
        }).pipe(
            catchError(error => this.handleError(error, () => this.getUser(userGuid)))
        );
    }

    /**
     * Get all teams in the current tenant.
     */
    getTeams(): Observable<AlertingTeam[]> {
        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AlertingTeam[]>(`${this.baseUrl}api/teams`, {
            headers: authHeaders
        }).pipe(
            catchError(error => this.handleError(error, () => this.getTeams()))
        );
    }

    /**
     * Get a specific team by its object GUID.
     */
    getTeam(teamGuid: string): Observable<AlertingTeam> {
        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AlertingTeam>(`${this.baseUrl}api/teams/${teamGuid}`, {
            headers: authHeaders
        }).pipe(
            catchError(error => this.handleError(error, () => this.getTeam(teamGuid)))
        );
    }

    /**
     * Get all users that belong to a specific team.
     */
    getTeamUsers(teamGuid: string): Observable<AlertingUser[]> {
        const authHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AlertingUser[]>(`${this.baseUrl}api/teams/${teamGuid}/users`, {
            headers: authHeaders
        }).pipe(
            catchError(error => this.handleError(error, () => this.getTeamUsers(teamGuid)))
        );
    }
}
