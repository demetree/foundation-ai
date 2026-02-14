//
// geocoding.service.ts
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Angular service for resolving address components into geographic coordinates
// via the server-side geocoding API endpoint.
//
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AlertService, MessageSeverity } from './alert.service';
import { AuthService } from './auth.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';


export interface GeocodeRequest {
    addressLine1?: string;
    city?: string;
    stateProvinceId?: number | bigint | null;
    postalCode?: string;
    countryId?: number | bigint | null;
}

export interface GeocodeResult {
    latitude: number;
    longitude: number;
    displayName: string;
    confidence: number;
}


@Injectable({ providedIn: 'root' })
export class GeocodingService extends SecureEndpointBase {

    private readonly apiUrl = '/api/Geocoding/Resolve';

    constructor(
        http: HttpClient,
        alertService: AlertService,
        authService: AuthService
    ) {
        super(http, alertService, authService);
    }


    /**
     * Resolves address components into geographic coordinates.
     *
     * Sends the address fields (including FK IDs for state/province and country)
     * to the server, which resolves names from the database and queries Nominatim.
     */
    resolveAddress(request: GeocodeRequest): Observable<GeocodeResult> {
        const headers = new HttpHeaders({
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + this.authService.accessToken
        });

        return this.http.post<GeocodeResult>(this.apiUrl, request, { headers }).pipe(
            catchError((error: any) => {
                return this.handleError(error, () => this.resolveAddress(request));
            })
        );
    }
}
