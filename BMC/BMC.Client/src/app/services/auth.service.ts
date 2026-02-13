import { Injectable } from '@angular/core';
import { Router, NavigationExtras } from '@angular/router';
import { Observable, Subject, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { HttpHeaders } from '@angular/common/http';

import { LocalStoreManager } from './local-store-manager.service';
import { OidcHelperService } from './oidc-helper.service';
import { ConfigurationService } from './configuration.service';
import { DBkeys } from './db-keys';
import { JwtHelper } from './jwt-helper';
import { Utilities } from './utilities';
import { IdToken, LoginResponse } from '../models/login-response.model';
import { User } from '../models/user.model';


@Injectable()
export class AuthService {
    public get loginUrl() { return this.configurations.loginUrl; }
    public get homeUrl() { return this.configurations.homeUrl; }

    public loginRedirectUrl: string | null = null;
    public logoutRedirectUrl: string | null = null;
    public reLoginDelegate: { (): void } | undefined;

    private previousIsLoggedInCheck = false;
    private loginStatus = new Subject<boolean>();
    private tokenRefreshTimer: any = null;

    constructor(
        private router: Router,
        private oidcHelperService: OidcHelperService,
        private configurations: ConfigurationService,
        private localStorage: LocalStoreManager) {

        this.initializeLoginStatus();
        this.startTokenRefreshTimer();
    }


    private initializeLoginStatus() {
        this.localStorage.getInitEvent().subscribe(() => {
            this.reevaluateLoginStatus();
        });
    }


    gotoPage(page: string, preserveParams = true) {
        const navigationExtras: NavigationExtras = {
            queryParamsHandling: preserveParams ? 'merge' : '', preserveFragment: preserveParams
        };
        this.router.navigate([page], navigationExtras);
    }

    gotoHomePage() {
        this.router.navigate([this.homeUrl]);
    }


    redirectLoginUser() {
        const redirect = this.loginRedirectUrl && this.loginRedirectUrl !== '/' &&
            this.loginRedirectUrl !== ConfigurationService.defaultHomeUrl ? this.loginRedirectUrl : this.homeUrl;
        this.loginRedirectUrl = null;

        const urlParamsAndFragment = Utilities.splitInTwo(redirect, '#');
        const urlAndParams = Utilities.splitInTwo(urlParamsAndFragment.firstPart, '?');

        const navigationExtras: NavigationExtras = {
            fragment: urlParamsAndFragment.secondPart,
            queryParams: urlAndParams.secondPart ? Utilities.getQueryParamsFromString(urlAndParams.secondPart) : null,
            queryParamsHandling: 'merge'
        };

        this.router.navigate([urlAndParams.firstPart], navigationExtras);
    }


    redirectLogoutUser() {
        const redirect = this.logoutRedirectUrl ? this.logoutRedirectUrl : this.loginUrl;
        this.logoutRedirectUrl = null;
        this.router.navigate([redirect]);
    }


    redirectForLogin() {
        this.loginRedirectUrl = this.router.url;
        this.router.navigate([this.loginUrl]);
    }


    reLogin() {
        if (this.reLoginDelegate) {
            this.reLoginDelegate();
        } else {
            this.redirectForLogin();
        }
    }


    refreshLogin() {
        return this.oidcHelperService.refreshLogin().pipe(
            map(resp => this.processLoginResponse(resp, this.rememberMe)),
            catchError(() => {
                this.reLogin();
                return throwError('Session expired, please log in again.');
            })
        );
    }


    loginWithPassword(userName: string, password: string, rememberMe?: boolean) {
        if (this.isLoggedIn) {
            this.logout();
        }

        return this.oidcHelperService.loginWithPassword(userName, password)
            .pipe(map(resp => this.processLoginResponse(resp, rememberMe)));
    }


    private processLoginResponse(response: LoginResponse, rememberMe?: boolean) {
        const idToken = response.id_token;
        const accessToken = response.access_token;
        const refreshToken = response.refresh_token;

        if (idToken == null) { throw new Error('idToken cannot be null'); }
        if (accessToken == null) { throw new Error('accessToken cannot be null'); }

        rememberMe = rememberMe ?? this.rememberMe;

        const accessTokenExpiry = new Date();
        accessTokenExpiry.setSeconds(accessTokenExpiry.getSeconds() + response.expires_in);

        const jwtHelper = new JwtHelper();
        const decodedIdToken = jwtHelper.decodeToken(idToken) as IdToken;

        //
        // Force the roles from the decoded token into a string array.
        //
        var roles: Array<string>;

        if (decodedIdToken.role && decodedIdToken.role.length > 0 && decodedIdToken.role[0] != undefined) {
            roles = Array.isArray(decodedIdToken.role) ? decodedIdToken.role : [decodedIdToken.role];
        } else {
            roles = new Array<string>();
        }

        var user = new User(
            decodedIdToken.sub,
            decodedIdToken.name,
            decodedIdToken.full_name,
            decodedIdToken.email,
            decodedIdToken.settings,
            parseInt(decodedIdToken.read_permission, 10) || 0,
            parseInt(decodedIdToken.write_permission, 10) || 0,
            decodedIdToken.tenant_name,
            roles);

        this.saveUserDetails(user, roles, accessToken, refreshToken, accessTokenExpiry, rememberMe);
        this.reevaluateLoginStatus(user);
        this.startTokenRefreshTimer();

        return user;
    }


    private startTokenRefreshTimer() {
        if (this.tokenRefreshTimer) {
            clearTimeout(this.tokenRefreshTimer);
        }

        if (this.accessTokenExpiryDate == null) {
            return;
        }

        const millisecondsBeforeTokenRefresh = this.accessTokenExpiryDate.getTime() - Date.now() - 60000;
        console.log('Access token refresh in ' + millisecondsBeforeTokenRefresh + ' milliseconds');

        this.tokenRefreshTimer = setTimeout(() => {
            this.refreshLogin().subscribe(
                () => { console.log('Relogged in prior to refresh token expiry.'); },
                () => this.reLogin()
            );
        }, millisecondsBeforeTokenRefresh);
    }


    private saveUserDetails(user: User, roles: string[], accessToken: string, refreshToken: string, expiresIn: Date, rememberMe: boolean) {
        if (rememberMe) {
            this.localStorage.savePermanentData(accessToken, DBkeys.ACCESS_TOKEN);
            this.localStorage.savePermanentData(refreshToken, DBkeys.REFRESH_TOKEN);
            this.localStorage.savePermanentData(expiresIn, DBkeys.TOKEN_EXPIRES_IN);
            this.localStorage.savePermanentData(roles, DBkeys.USER_ROLES);
            this.localStorage.savePermanentData(user, DBkeys.CURRENT_USER);
        } else {
            this.localStorage.saveSyncedSessionData(accessToken, DBkeys.ACCESS_TOKEN);
            this.localStorage.saveSyncedSessionData(refreshToken, DBkeys.REFRESH_TOKEN);
            this.localStorage.saveSyncedSessionData(expiresIn, DBkeys.TOKEN_EXPIRES_IN);
            this.localStorage.saveSyncedSessionData(roles, DBkeys.USER_ROLES);
            this.localStorage.saveSyncedSessionData(user, DBkeys.CURRENT_USER);
        }

        this.localStorage.savePermanentData(rememberMe, DBkeys.REMEMBER_ME);
    }


    logout(): void {
        this.localStorage.deleteData(DBkeys.ACCESS_TOKEN);
        this.localStorage.deleteData(DBkeys.REFRESH_TOKEN);
        this.localStorage.deleteData(DBkeys.TOKEN_EXPIRES_IN);
        this.localStorage.deleteData(DBkeys.USER_ROLES);
        this.localStorage.deleteData(DBkeys.CURRENT_USER);

        this.reevaluateLoginStatus();
    }


    private reevaluateLoginStatus(currentUser?: User | null) {
        const user = currentUser ?? this.localStorage.getDataObject<User>(DBkeys.CURRENT_USER);
        const isLoggedIn = user != null;

        if (this.previousIsLoggedInCheck !== isLoggedIn) {
            setTimeout(() => { this.loginStatus.next(isLoggedIn); });
        }

        this.previousIsLoggedInCheck = isLoggedIn;
    }


    getLoginStatusEvent(): Observable<boolean> {
        return this.loginStatus.asObservable();
    }


    get currentUser(): User | null {
        const user = this.localStorage.getDataObject<User>(DBkeys.CURRENT_USER);
        this.reevaluateLoginStatus(user);
        return user;
    }

    get userRoles(): string[] {
        return this.localStorage.getDataObject<string[]>(DBkeys.USER_ROLES) ?? [];
    }

    get accessToken(): string | null {
        return this.oidcHelperService.accessToken;
    }

    get accessTokenExpiryDate(): Date | null {
        return this.oidcHelperService.accessTokenExpiryDate;
    }

    get refreshToken(): string | null {
        return this.oidcHelperService.refreshToken;
    }

    get isSessionExpired(): boolean {
        return this.oidcHelperService.isSessionExpired;
    }

    get isLoggedIn(): boolean {
        return this.currentUser != null;
    }

    get rememberMe(): boolean {
        return this.localStorage.getDataObject<boolean>(DBkeys.REMEMBER_ME) === true;
    }

    get userName(): string {
        return this.currentUser?.userName ?? '';
    }

    get fullName(): string {
        return this.currentUser?.fullName ?? '';
    }


    ///
    /// Returns the headers needed to authenticate requests to the server.
    ///
    public GetAuthenticationHeaders(): HttpHeaders {
        return new HttpHeaders({
            Authorization: `Bearer ${this.accessToken}`,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });
    }
}
