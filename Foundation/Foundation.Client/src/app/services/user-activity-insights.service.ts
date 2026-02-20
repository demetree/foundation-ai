//
// User Activity Insights Service
//
// Angular service that fetches pre-aggregated analytics from the UserActivityInsights API.
//
// AI-assisted development - February 2026
//
import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { AuthService } from '../services/auth.service';


//
// Response DTOs matching the server-side UserActivityInsightsController
//

export interface InsightsSummary {
    totalEvents: number;
    uniqueUsers: number;
    uniqueSessions: number;
    successRate: number;
    avgEventsPerDay: number;
    startDate: string;
    endDate: string;
}

export interface HourlyActivity {
    hour: number;
    count: number;
}

export interface DailyActivity {
    date: string;
    count: number;
    uniqueUsers: number;
}

export interface TopUser {
    userName: string;
    eventCount: number;
    lastActive: string;
    topModule: string;
    successRate: number;
}

export interface TopModule {
    moduleName: string;
    eventCount: number;
    uniqueUsers: number;
    readCount: number;
    writeCount: number;
}

export interface EventTypeBreakdown {
    auditTypeName: string;
    auditTypeId: number;
    count: number;
}

export interface UserModuleLink {
    userName: string;
    moduleName: string;
    count: number;
}

export interface RecentSession {
    userName: string;
    sessionId: string;
    sessionStart: string;
    sessionEnd: string;
    eventCount: number;
    modules: string[];
}

export interface FailureHotspot {
    userName: string;
    moduleName: string;
    failureCount: number;
    lastFailure: string;
}

export interface ModuleEntityDetail {
    moduleName: string;
    entityName: string;
    eventCount: number;
    readCount: number;
    writeCount: number;
    uniqueUsers: number;
}

export interface UserActivityInsightsResponse {
    summary: InsightsSummary;
    activityByHour: HourlyActivity[];
    activityByDay: DailyActivity[];
    topUsers: TopUser[];
    topModules: TopModule[];
    eventTypeBreakdown: EventTypeBreakdown[];
    userModuleMatrix: UserModuleLink[];
    recentSessions: RecentSession[];
    failureHotspots: FailureHotspot[];
    moduleEntityBreakdown: ModuleEntityDetail[];
}



@Injectable({ providedIn: 'root' })
export class UserActivityInsightsService {

    private readonly apiUrl: string;

    constructor(
        private http: HttpClient,
        private authService: AuthService,
        @Inject('BASE_URL') baseUrl: string
    ) {
        this.apiUrl = baseUrl + 'api/UserActivityInsights';
    }


    getInsights(startTime?: Date, stopTime?: Date, userName?: string): Observable<UserActivityInsightsResponse> {
        let params = new HttpParams();

        if (startTime) {
            params = params.set('startTime', startTime.toISOString());
        }

        if (stopTime) {
            params = params.set('stopTime', stopTime.toISOString());
        }

        if (userName) {
            params = params.set('userName', userName);
        }

        return this.http.get<UserActivityInsightsResponse>(this.apiUrl, {
            headers: this.authService.GetAuthenticationHeaders(),
            params: params
        });
    }
}
