import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HubAuthService } from './hub-auth.service';
import { environment } from '../../environments/environment';

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

    getMyProfile(): Observable<any> {
        return this.http.get(
            `${environment.apiBaseUrl}/api/volunteerhub/me`,
            { headers: this.headers }
        );
    }


    // ─────── Assignments ───────

    getMyAssignments(from?: Date, to?: Date): Observable<any[]> {
        let params: any = {};
        if (from) params.from = from.toISOString();
        if (to) params.to = to.toISOString();

        return this.http.get<any[]>(
            `${environment.apiBaseUrl}/api/volunteerhub/me/assignments`,
            { headers: this.headers, params }
        );
    }


    // ─────── Session validation ───────

    validateSession(): Observable<any> {
        return this.http.get(
            `${environment.apiBaseUrl}/api/volunteerhub/auth/session`,
            { headers: this.headers }
        );
    }
}
