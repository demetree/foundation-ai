//
// TURN Server Service
//
// Angular service for retrieving TURN server status and allocations
// from Foundation's TURN server admin API.
//
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';


export interface TurnServerStatus {
    isRunning: boolean;
    udpEndpoint: string | null;
    tcpEndpoint: string | null;
    tlsEndpoint: string | null;
    allocationCount: number;
}

export interface TurnAllocationSummary {
    fiveTuple: string;
    username: string;
    relayPort: number;
    lifetimeSeconds: number;
    expiresAtUtc: Date;
    permissionCount: number;
    channelCount: number;
    isExpired: boolean;
}

export interface TurnAllocationsResponse {
    allocations: TurnAllocationSummary[];
    totalCount: number;
}

@Injectable({
    providedIn: 'root'
})
export class TurnServerService {

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
     * Get the current TURN server status.
     */
    getStatus(): Observable<TurnServerStatus> {
        return this.http.get<TurnServerStatus>('/api/turn/status', {
            headers: this.authHeaders
        });
    }

    /**
     * Get all active TURN allocations.
     */
    getAllocations(): Observable<TurnAllocationsResponse> {
        return this.http.get<TurnAllocationsResponse>('/api/turn/allocations', {
            headers: this.authHeaders
        });
    }
}
