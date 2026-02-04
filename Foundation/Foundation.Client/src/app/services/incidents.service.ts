//
// Incidents Service
//
// Angular service for retrieving incidents from Foundation's Alerting integration.
//
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface IncidentSummary {
    incidentKey: string;
    incidentId: number;
    status: string;
    severity: string;
    title: string;
    createdAt: Date;
}

export interface IncidentFilter {
    since?: Date;
    until?: Date;
    status?: string;
    severity?: string;
    limit?: number;
}

export interface IncidentsResponse {
    incidents: IncidentSummary[];
    isConfigured: boolean;
    message?: string;
}

@Injectable({
    providedIn: 'root'
})
export class IncidentsService {

    constructor(
        private authService: AuthService,
        private http: HttpClient
    ) { }

    private get authHeaders(): HttpHeaders {
        return new HttpHeaders({
            Authorization: 'Bearer ' + this.authService.accessToken
        });
    }

    /**
     * Get incidents raised by this Foundation system.
     */
    getIncidents(filter?: IncidentFilter): Observable<IncidentsResponse> {
        let params = new HttpParams();

        if (filter?.since) {
            params = params.set('since', filter.since.toISOString());
        }
        if (filter?.until) {
            params = params.set('until', filter.until.toISOString());
        }
        if (filter?.status) {
            params = params.set('status', filter.status);
        }
        if (filter?.severity) {
            params = params.set('severity', filter.severity);
        }
        if (filter?.limit) {
            params = params.set('limit', filter.limit.toString());
        }

        return this.http.get<IncidentsResponse>('/api/incidents', {
            headers: this.authHeaders,
            params
        });
    }

    /**
     * Test the Alerting integration by raising a test incident.
     */
    testIntegration(): Observable<TestIntegrationResponse> {
        return this.http.post<TestIntegrationResponse>('/api/incidents/test', {}, {
            headers: this.authHeaders
        });
    }

    /**
     * Resolve an incident by its key.
     */
    resolveIncident(incidentKey: string, resolution?: string): Observable<ResolveIncidentResponse> {
        return this.http.post<ResolveIncidentResponse>(`/api/incidents/${encodeURIComponent(incidentKey)}/resolve`, {
            resolution
        }, {
            headers: this.authHeaders
        });
    }
}

export interface TestIntegrationResponse {
    success: boolean;
    incidentKey?: string;
    message?: string;
}

export interface ResolveIncidentResponse {
    success: boolean;
    incidentKey?: string;
    message?: string;
}
