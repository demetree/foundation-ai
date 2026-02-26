import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';


///
/// API service for the public landing page.
/// Calls /api/public/* endpoints — no authentication required.
///
@Injectable({ providedIn: 'root' })
export class PublicShowcaseService {

    constructor(
        private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string
    ) { }


    getStats(): Observable<any> {
        return this.http.get(`${this.baseUrl}api/public/stats`);
    }

    getFeaturedSets(): Observable<any[]> {
        return this.http.get<any[]>(`${this.baseUrl}api/public/featured-sets`);
    }

    getRecentSets(): Observable<any[]> {
        return this.http.get<any[]>(`${this.baseUrl}api/public/recent-sets`);
    }

    getDecades(): Observable<any[]> {
        return this.http.get<any[]>(`${this.baseUrl}api/public/decades`);
    }

    getRandomDiscovery(): Observable<any[]> {
        return this.http.get<any[]>(`${this.baseUrl}api/public/random-discovery`);
    }
}

