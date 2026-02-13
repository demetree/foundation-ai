// 
// ResourceScheduleContextService
// 
// AI-Developed — This file was significantly developed with AI assistance.
//
// Custom service for resource scheduling context operations.
// Provides methods for audit logging of scheduling warning dismissals
// and (future) fetching schedule context data for calendar display.
//
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, catchError } from 'rxjs';
import { AlertService } from './alert.service';
import { AuthService } from './auth.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';

@Injectable({
    providedIn: 'root'
})
export class ResourceScheduleContextService extends SecureEndpointBase {

    constructor(
        http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        @Inject('BASE_URL') private baseUrl: string
    ) {
        super(http, alertService, authService);
    }

    /**
     * Posts scheduling warning dismissal data to the server for audit logging.
     * 
     * Called when a user dismisses availability conflict or shift boundary
     * warnings while saving an event.
     * 
     * Endpoint: POST api/Resources/LogSchedulingWarningDismissal
     */
    public LogSchedulingWarningDismissal(payload: {
        eventId: number;
        eventName: string;
        warnings: string[];
        resourceIds: number[];
    }): Observable<any> {
        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<any>(
            this.baseUrl + 'api/Resources/LogSchedulingWarningDismissal',
            payload,
            { headers: authenticationHeaders }
        ).pipe(
            catchError(error => {
                return this.handleError(error, () => this.LogSchedulingWarningDismissal(payload));
            })
        );
    }
}
