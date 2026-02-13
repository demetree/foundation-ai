import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, Subject, BehaviorSubject, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

export interface LoginResponse {
    access_token: string;
    refresh_token?: string;
    expires_in: number;
    token_type: string;
}

export interface UserInfo {
    userName: string;
    fullName: string;
    email?: string;
    tenantName?: string;
    roles?: string[];
}


@Injectable({ providedIn: 'root' })
export class AuthService {

    private loginStatus = new Subject<boolean>();
    private _currentUser: UserInfo | null = null;
    private accessToken: string | null = null;

    reLoginDelegate: (() => void) | undefined;


    constructor(
        private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string
    ) {
        // Restore token from storage on startup
        this.accessToken = localStorage.getItem('bmc_access_token');
        const storedUser = localStorage.getItem('bmc_current_user');
        if (storedUser) {
            try {
                this._currentUser = JSON.parse(storedUser);
            } catch { }
        }
    }


    get isLoggedIn(): boolean {
        return !!this.accessToken;
    }


    get currentUser(): UserInfo | null {
        return this._currentUser;
    }


    getLoginStatusEvent(): Observable<boolean> {
        return this.loginStatus.asObservable();
    }


    loginWithPassword(userName: string, password: string): Observable<LoginResponse> {
        const url = `${this.baseUrl}connect/token`;

        const body = new URLSearchParams();
        body.set('grant_type', 'password');
        body.set('username', userName);
        body.set('password', password);
        body.set('scope', 'openid email phone profile offline_access roles');

        const headers = new HttpHeaders({
            'Content-Type': 'application/x-www-form-urlencoded'
        });

        return this.http.post<LoginResponse>(url, body.toString(), { headers })
            .pipe(
                map(response => {
                    this.processLoginResponse(response, userName);
                    return response;
                })
            );
    }


    private processLoginResponse(response: LoginResponse, userName: string) {
        this.accessToken = response.access_token;
        localStorage.setItem('bmc_access_token', this.accessToken);

        this._currentUser = {
            userName: userName,
            fullName: userName,
        };

        localStorage.setItem('bmc_current_user', JSON.stringify(this._currentUser));
        this.loginStatus.next(true);
    }


    logout() {
        this.accessToken = null;
        this._currentUser = null;
        localStorage.removeItem('bmc_access_token');
        localStorage.removeItem('bmc_current_user');
        this.loginStatus.next(false);
    }


    redirectLogoutUser() {
        window.location.href = '/login';
    }


    getAccessToken(): string | null {
        return this.accessToken;
    }
}
