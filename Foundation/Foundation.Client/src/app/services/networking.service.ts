//
// Networking Service
//
// Angular service for the unified networking dashboard API.
// Retrieves overview status and per-service detail data from Foundation's
// NetworkingController endpoints.
//
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';


// ── Overview Models ──────────────────────────────────────────────────────


export interface ServiceStatusSummary {
    name: string;
    icon: string;
    status: 'healthy' | 'warning' | 'offline';
    metricLabel: string;
    metricValue: string;
    secondaryLabel: string | null;
}

export interface NetworkingOverview {
    services: ServiceStatusSummary[];
    totalServices: number;
    healthyCount: number;
    warningCount: number;
    offlineCount: number;
    overallStatus: string;
}


@Injectable({
    providedIn: 'root'
})
export class NetworkingService {

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
     * Get the aggregated status of all networking services.
     */
    getOverview(): Observable<NetworkingOverview> {
        return this.http.get<NetworkingOverview>('/api/networking/overview', {
            headers: this.authHeaders
        });
    }


    /**
     * Get detail data for a specific service.
     */
    getServiceDetail(service: string): Observable<any> {
        return this.http.get<any>(`/api/networking/${service}`, {
            headers: this.authHeaders
        });
    }
}
