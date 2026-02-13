import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class OidcHelperService {

    constructor(
        private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string
    ) { }

    loginWithPassword(userName: string, password: string): Observable<any> {
        const url = `${this.baseUrl}connect/token`;

        const body = new URLSearchParams();
        body.set('grant_type', 'password');
        body.set('username', userName);
        body.set('password', password);
        body.set('scope', 'openid email phone profile offline_access roles');

        const headers = new HttpHeaders({
            'Content-Type': 'application/x-www-form-urlencoded'
        });

        return this.http.post(url, body.toString(), { headers });
    }

    refreshToken(refreshToken: string): Observable<any> {
        const url = `${this.baseUrl}connect/token`;

        const body = new URLSearchParams();
        body.set('grant_type', 'refresh_token');
        body.set('refresh_token', refreshToken);

        const headers = new HttpHeaders({
            'Content-Type': 'application/x-www-form-urlencoded'
        });

        return this.http.post(url, body.toString(), { headers });
    }
}
