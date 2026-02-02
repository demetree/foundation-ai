//
// Integration Management Service
//
// Client service for the custom IntegrationManagementController.
// Handles integration CRUD with secure API key generation on the backend.
//
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError } from 'rxjs';
import { SecureEndpointBase } from './secure-endpoint-base.service';
import { AuthService } from './auth.service';
import { AlertService as ToastService } from './alert.service';
import { ConfigurationService } from './configuration.service';


// Request/Response DTOs matching the backend

export interface CreateIntegrationRequest {
    name: string;
    description?: string;
    serviceId: number;
    webhookUrl?: string;
    maxRetryAttempts?: number;
    retryBackoffSeconds?: number;
    /** IDs of IncidentEventTypes that should trigger callbacks */
    callbackEventTypeIds?: number[];
}

export interface UpdateIntegrationRequest {
    name?: string;
    description?: string;
    webhookUrl?: string;
    active?: boolean;
    maxRetryAttempts?: number;
    retryBackoffSeconds?: number;
    /** IDs of IncidentEventTypes that should trigger callbacks. Pass null to leave unchanged. */
    callbackEventTypeIds?: number[];
}

export interface CallbackEventTypeDto {
    id: number;
    name: string;
}

export interface IntegrationDto {
    id: number;
    objectGuid: string;
    serviceId: number;
    serviceName: string;
    name: string;
    description?: string;
    webhookUrl?: string;
    active: boolean;
    versionNumber: number;
    // Retry settings
    maxRetryAttempts?: number;
    retryBackoffSeconds?: number;
    // Callback status (read-only)
    lastCallbackSuccessAt?: string;
    consecutiveCallbackFailures?: number;
    // Selected event types for callbacks
    callbackEventTypes?: CallbackEventTypeDto[];
}

export interface IntegrationCreatedResponse extends IntegrationDto {
    /** The plain API key - returned ONLY on creation. Store securely! */
    apiKey: string;
}

export interface RegenerateKeyResponse {
    integrationId: number;
    integrationName: string;
    /** The new plain API key. Store securely! */
    apiKey: string;
    message: string;
}


@Injectable({
    providedIn: 'root'
})
export class IntegrationManagementService extends SecureEndpointBase {

    private readonly apiUrl = '/api/integration-management';

    constructor(
        http: HttpClient,
        alertService: ToastService,
        authService: AuthService,
        private configurationService: ConfigurationService
    ) {
        super(http, alertService, authService);
    }


    /**
     * Create a new integration. The API key is generated server-side
     * and returned ONLY in this response.
     */
    createIntegration(request: CreateIntegrationRequest): Observable<IntegrationCreatedResponse> {
        const url = this.configurationService.baseUrl + this.apiUrl;
        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.post<IntegrationCreatedResponse>(url, request, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.createIntegration(request)))
        );
    }


    /**
     * Get an integration by ID.
     */
    getIntegration(id: number): Observable<IntegrationDto> {
        const url = this.configurationService.baseUrl + this.apiUrl + '/' + id;
        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.get<IntegrationDto>(url, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.getIntegration(id)))
        );
    }


    /**
     * List integrations for the current tenant.
     */
    getIntegrations(serviceId?: number, includeInactive = false): Observable<IntegrationDto[]> {
        let url = this.configurationService.baseUrl + this.apiUrl;

        const params: string[] = [];
        if (serviceId) params.push(`serviceId=${serviceId}`);
        if (includeInactive) params.push('includeInactive=true');
        if (params.length > 0) url += '?' + params.join('&');

        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.get<IntegrationDto[]>(url, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.getIntegrations(serviceId, includeInactive)))
        );
    }


    /**
     * Update an integration (name, description, webhook, active status).
     */
    updateIntegration(id: number, request: UpdateIntegrationRequest): Observable<IntegrationDto> {
        const url = this.configurationService.baseUrl + this.apiUrl + '/' + id;
        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.put<IntegrationDto>(url, request, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.updateIntegration(id, request)))
        );
    }


    /**
     * Regenerate the API key for an integration.
     * The new key is returned ONLY in this response.
     */
    regenerateApiKey(id: number): Observable<RegenerateKeyResponse> {
        const url = this.configurationService.baseUrl + this.apiUrl + '/' + id + '/regenerate-key';
        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.post<RegenerateKeyResponse>(url, {}, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.regenerateApiKey(id)))
        );
    }


    /**
     * Delete an integration (soft delete).
     */
    deleteIntegration(id: number): Observable<void> {
        const url = this.configurationService.baseUrl + this.apiUrl + '/' + id;
        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(url, { headers }).pipe(
            catchError(error => this.handleError(error, () => this.deleteIntegration(id)))
        );
    }
}
