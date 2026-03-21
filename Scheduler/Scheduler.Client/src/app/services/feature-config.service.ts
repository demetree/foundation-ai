// AI-Developed — This file was significantly developed with AI assistance.
//
// FeatureConfigService
//
// Fetches all system-level feature toggle states from the server in a single
// request and caches them for the lifetime of the app session.  Used by the
// sidebar and route guards to determine which modules should be visible.
//

import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map, shareReplay } from 'rxjs/operators';

import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';


interface FeatureConfig {
    volunteerManagementEnabled: boolean;
    fundraisingEnabled: boolean;
    financialManagementEnabled: boolean;
    crewManagementEnabled: boolean;
}


@Injectable({
    providedIn: 'root'
})
export class FeatureConfigService extends SecureEndpointBase {

    private _config$: Observable<FeatureConfig> | null = null;


    constructor(http: HttpClient,
                alertService: AlertService,
                authService: AuthService,
                @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);
    }


    /**
     * Fetches and caches the full feature configuration from the server.
     */
    private get config$(): Observable<FeatureConfig> {

        if (this._config$ == null) {
            this._config$ = this.http.get<FeatureConfig>(
                `${this.baseUrl}api/FeatureConfig`
            ).pipe(
                catchError(error => {
                    console.warn('FeatureConfigService: Unable to fetch feature config, defaulting all features to disabled', error);
                    return of({
                        volunteerManagementEnabled: false,
                        fundraisingEnabled: false,
                        financialManagementEnabled: false,
                        crewManagementEnabled: false
                    } as FeatureConfig);
                }),
                shareReplay({ bufferSize: 1, refCount: false })
            );
        }

        return this._config$;
    }


    /** Whether Volunteer Management is enabled at the system level. */
    public get isVolunteerEnabled$(): Observable<boolean> {
        return this.config$.pipe(map(c => c.volunteerManagementEnabled === true));
    }

    /** Whether Fundraising / Donor Management is enabled at the system level. */
    public get isFundraisingEnabled$(): Observable<boolean> {
        return this.config$.pipe(map(c => c.fundraisingEnabled === true));
    }

    /** Whether Financial Management (rate sheets, invoices, billing) is enabled at the system level. */
    public get isFinancialEnabled$(): Observable<boolean> {
        return this.config$.pipe(map(c => c.financialManagementEnabled === true));
    }

    /** Whether Crew Management (crews, shifts, shift patterns) is enabled at the system level. */
    public get isCrewEnabled$(): Observable<boolean> {
        return this.config$.pipe(map(c => c.crewManagementEnabled === true));
    }
}
