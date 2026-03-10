import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HubAuthService } from './hub-auth.service';
import { environment } from '../../environments/environment';
import { VolunteerProfile, VolunteerAssignment, Opportunity, BrandingInfo, ProfileUpdateRequest } from '../models/hub-models';

@Injectable({ providedIn: 'root' })
export class HubApiService {

    constructor(
        private http: HttpClient,
        private auth: HubAuthService
    ) { }


    private get headers(): HttpHeaders {
        return new HttpHeaders({
            'X-Volunteer-Session': this.auth.sessionToken || ''
        });
    }


    // ─────── Profile ───────

    getMyProfile(): Observable<VolunteerProfile> {
        return this.http.get<VolunteerProfile>(
            `${environment.apiBaseUrl}/api/volunteerhub/me`,
            { headers: this.headers }
        );
    }

    updateMyProfile(data: ProfileUpdateRequest): Observable<any> {
        return this.http.put(
            `${environment.apiBaseUrl}/api/volunteerhub/me/profile`,
            data,
            { headers: this.headers }
        );
    }


    // ─────── Assignments ───────

    getMyAssignments(from?: Date, to?: Date): Observable<VolunteerAssignment[]> {
        let params: any = {};
        if (from) params.from = from.toISOString();
        if (to) params.to = to.toISOString();

        return this.http.get<VolunteerAssignment[]>(
            `${environment.apiBaseUrl}/api/volunteerhub/me/assignments`,
            { headers: this.headers, params }
        );
    }

    reportHours(assignmentId: number, hours: number, notes?: string): Observable<any> {
        return this.http.post(
            `${environment.apiBaseUrl}/api/volunteerhub/me/assignments/${assignmentId}/report-hours`,
            { hours, notes },
            { headers: this.headers }
        );
    }

    respondToAssignment(assignmentId: number, accepted: boolean): Observable<any> {
        return this.http.post(
            `${environment.apiBaseUrl}/api/volunteerhub/me/assignments/${assignmentId}/respond`,
            { accepted },
            { headers: this.headers }
        );
    }


    // ─────── Session validation ───────

    validateSession(): Observable<any> {
        return this.http.get(
            `${environment.apiBaseUrl}/api/volunteerhub/auth/session`,
            { headers: this.headers }
        );
    }

    logout(): Observable<any> {
        return this.http.post(
            `${environment.apiBaseUrl}/api/volunteerhub/auth/logout`,
            {},
            { headers: this.headers }
        );
    }


    // ─────── Public Registration ───────

    register(data: {
        firstName: string; lastName: string; email: string;
        phone?: string; availabilityPreferences?: string;
        interestsAndSkillsNotes?: string; emergencyContactNotes?: string;
    }): Observable<any> {
        // No auth header — public endpoint
        return this.http.post(
            `${environment.apiBaseUrl}/api/volunteerhub/public/register`,
            data
        );
    }


    // ─────── Branding ───────

    getBranding(): Observable<BrandingInfo> {
        // No auth header — public endpoint
        return this.http.get<BrandingInfo>(
            `${environment.apiBaseUrl}/api/volunteerhub/public/branding`
        );
    }


    // ─────── Opportunities ───────

    getOpportunities(search?: string, fromDate?: Date, toDate?: Date): Observable<Opportunity[]> {
        let params: any = {};

        if (search) {
            params.search = search;
        }

        if (fromDate) {
            params.fromDate = fromDate.toISOString();
        }

        if (toDate) {
            params.toDate = toDate.toISOString();
        }

        return this.http.get<Opportunity[]>(
            `${environment.apiBaseUrl}/api/volunteerhub/opportunities`,
            { headers: this.headers, params }
        );
    }

    signUpForOpportunity(eventId: number): Observable<any> {
        return this.http.post(
            `${environment.apiBaseUrl}/api/volunteerhub/opportunities/${eventId}/sign-up`,
            {},
            { headers: this.headers }
        );
    }
}
