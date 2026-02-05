//
// Alert Test Harness Service
//
// Provides methods for testing the backend alerting APIs.
// Supports both API key auth (for AlertsController) and OIDC auth (for IncidentController).
//
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { ConfigurationService } from './configuration.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';
import { AuthService } from './auth.service';
import { AlertService as ToastService } from './alert.service';


// DTOs matching the backend models
export interface AlertPayload {
    incidentKey?: string;
    title: string;
    description?: string;
    severity?: string;
    sourcePayloadJson?: string;
}

export interface AlertResponse {
    success: boolean;
    message: string;
    incidentId: number;
    incidentKey: string;
    isNew: boolean;
}

export interface IncidentDto {
    id: number;
    objectGuid: string;
    incidentKey: string;
    title: string;
    description: string;
    serviceId: number;
    serviceName: string;
    severityId: number;
    severityName: string;
    statusId: number;
    statusName: string;
    createdAt: Date;
    acknowledgedAt?: Date;
    resolvedAt?: Date;
}

export interface TimelineEventDto {
    id: number;
    eventType: string;
    timestamp: Date;
    actorObjectGuid?: string;
    detailsJson?: string;
    notes?: string;
    source?: string;
}

export interface NoteDto {
    id: number;
    content: string;
    createdAt: Date;
    authorObjectGuid?: string;
}

export interface IncidentDetailDto extends IncidentDto {
    timeline: TimelineEventDto[];
    notes: NoteDto[];
}

export interface IncidentStatsDto {
    countsByStatus: { [key: string]: number };
    countsBySeverity: { [key: string]: number };
    totalActive: number;
}

export interface IntegrationInfo {
    id: number;
    name: string;
    apiKey?: string;  // Only shown in test harness
    serviceId: number;
    serviceName?: string;
}


@Injectable({
    providedIn: 'root'
})
export class AlertTestHarnessService extends SecureEndpointBase {

    private readonly alertsApiUrl = '/api/alerts/v1';
    private readonly incidentsApiUrl = '/api/incident-management';

    constructor(
        http: HttpClient,
        alertService: ToastService,
        authService: AuthService,
        private configurationService: ConfigurationService
    ) {
        super(http, alertService, authService);
    }


    //
    // API Key authenticated endpoints (AlertsController)
    //

    /**
     * Trigger an alert via the API key endpoint
     */
    triggerAlert(apiKey: string, payload: AlertPayload): Observable<AlertResponse> {
        const url = this.configurationService.baseUrl + this.alertsApiUrl + '/enqueue';
        const headers = new HttpHeaders().set('X-Api-Key', apiKey);

        return this.http.post<AlertResponse>(url, payload, { headers }).pipe(
            catchError(error => {
                console.error('Trigger alert failed:', error);
                return throwError(() => error);
            })
        );
    }


    //
    // OIDC authenticated endpoints (IncidentController)
    //

    /**
     * Get all incidents (optional filters)
     */
    getIncidents(serviceId?: number, severityId?: number, includeResolved = false): Observable<IncidentDto[]> {
        let url = this.configurationService.baseUrl + this.incidentsApiUrl;

        const params: string[] = [];
        if (serviceId) params.push(`serviceId=${serviceId}`);
        if (severityId) params.push(`severityId=${severityId}`);
        if (includeResolved) params.push('includeResolved=true');
        if (params.length > 0) url += '?' + params.join('&');

        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentDto[]>(url, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.getIncidents(serviceId, severityId, includeResolved)))
        );
    }


    /**
     * Get incident details with timeline
     */
    getIncidentDetail(id: number): Observable<IncidentDetailDto> {
        const url = this.configurationService.baseUrl + this.incidentsApiUrl + '/' + id;
        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentDetailDto>(url, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.getIncidentDetail(id)))
        );
    }


    /**
     * Acknowledge an incident
     */
    acknowledgeIncident(id: number): Observable<IncidentDto> {
        const url = this.configurationService.baseUrl + this.incidentsApiUrl + '/' + id + '/acknowledge';
        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.post<IncidentDto>(url, {}, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.acknowledgeIncident(id)))
        );
    }


    /**
     * Resolve an incident
     */
    resolveIncident(id: number): Observable<IncidentDto> {
        const url = this.configurationService.baseUrl + this.incidentsApiUrl + '/' + id + '/resolve';
        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.post<IncidentDto>(url, {}, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.resolveIncident(id)))
        );
    }


    /**
     * Add a note to an incident
     */
    addNote(id: number, content: string): Observable<NoteDto> {
        const url = this.configurationService.baseUrl + this.incidentsApiUrl + '/' + id + '/notes';
        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.post<NoteDto>(url, { content }, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.addNote(id, content)))
        );
    }


    /**
     * Get incident statistics
     */
    getStats(): Observable<IncidentStatsDto> {
        const url = this.configurationService.baseUrl + this.incidentsApiUrl + '/stats';
        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentStatsDto>(url, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.getStats()))
        );
    }
}
