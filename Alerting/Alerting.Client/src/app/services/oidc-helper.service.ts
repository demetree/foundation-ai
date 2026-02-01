import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Location } from '@angular/common';
import { from, merge, Observable, of, throwError } from 'rxjs';
import { map, mergeMap, filter } from 'rxjs/operators';
import { AuthConfig, OAuthEvent, OAuthService, OAuthStorage } from 'angular-oauth2-oidc';

import { LocalStoreManager } from './local-store-manager.service';
import { ConfigurationService } from './configuration.service';
import { Utilities } from './utilities';
import { DBkeys } from './db-keys';
import { LoginResponse } from '../models/login-response.model';

@Injectable()
export class OidcHelperService {
  private readonly clientId = 'alerting_spa';
  private readonly scope = 'openid email phone profile offline_access roles';

  private readonly tokenEndpoint = '/connect/token';
  private readonly twitterRequestTokenEndpoint = '/oauth/twitter/request_token';
  private readonly twitterAccessTokenEndpoint = '/oauth/twitter/access_token';

  private readonly googleOidcConfig: AuthConfig = {
    issuer: 'https://accounts.google.com',
    redirectUri: this.configurations.baseUrl + '/google-login',
    clientId: this.configurations.googleClientId ?? undefined,
    strictDiscoveryDocumentValidation: false,
    skipIssuerCheck: true,
    scope: 'openid profile email',
    sessionChecksEnabled: false
  };

  private readonly facebookOidcConfig: AuthConfig = {
    loginUrl: 'https://www.facebook.com/v5.0/dialog/oauth',
    redirectUri: this.configurations.baseUrl + '/facebook-login',
    clientId: this.configurations.facebookClientId ?? undefined,
    strictDiscoveryDocumentValidation: false,
    skipIssuerCheck: true,
    scope: 'email',
    sessionChecksEnabled: false
  };

  private readonly microsoftOidcConfig: AuthConfig = {
    issuer: 'https://login.microsoftonline.com/common/v2.0',
    redirectUri: this.configurations.baseUrl + '/microsoft-login',
    clientId: this.configurations.microsoftClientId ?? undefined,
    responseType: 'code',
    strictDiscoveryDocumentValidation: false,
    skipIssuerCheck: true,
    scope: 'openid profile email',
    sessionChecksEnabled: false
  };

  constructor(
    private http: HttpClient,
    private location: Location,
    private oauthService: OAuthService,
    private configurations: ConfigurationService,
    private localStorage: LocalStoreManager) {

  }

  loginWithPassword(userName: string, password: string) {
    const header = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });
    const params = new HttpParams()
      .append('username', userName)
      .append('password', password)
      .append('client_id', this.clientId)
      .append('grant_type', 'password')
      .append('scope', this.scope);

    return this.http.post<LoginResponse>(this.tokenEndpoint, params, { headers: header });
  }

  refreshLogin() {
    const header = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });
    const params = new HttpParams()
      .append('refresh_token', this.refreshToken ?? '')
      .append('client_id', this.clientId)
      .append('grant_type', 'refresh_token');

    return this.http.post<LoginResponse>(this.tokenEndpoint, params, { headers: header });
  }

  loginWithExternalToken(token: string, provider: string, password?: string | null) {
    const header = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });
    let params = new HttpParams()
      .append('assertion', token)
      .append('provider', provider)
      .append('client_id', this.clientId)
      .append('grant_type', 'assertion')
      .append('scope', this.scope);

    if (password) {
      params = params.append('password', password);
    }

    return this.http.post<LoginResponse>(this.tokenEndpoint, params, { headers: header });
  }

  initLoginWithGoogle() {
    this.oauthService.configure(this.googleOidcConfig);

    this.oauthService.loadDiscoveryDocument().then(() => {
      this.oauthService.initImplicitFlow();
    });
  }

  processGoogleLoginTokens() {
    this.oauthService.configure(this.googleOidcConfig);
    return this.processOidcLoginTokens(this.oauthService.loadDiscoveryDocumentAndTryLogin);
  }


  initLoginWithFacebook() {
    this.oauthService.configure(this.facebookOidcConfig);
    this.oauthService.initImplicitFlow();
  }

  processFacebookLoginTokens() {
    this.oauthService.configure(this.facebookOidcConfig);
    return this.processOidcLoginTokens(this.oauthService.tryLoginImplicitFlow);
  }


  initLoginWithMicrosoft() {
    this.oauthService.configure(this.microsoftOidcConfig);

    this.oauthService.loadDiscoveryDocument().then(() => {
      this.oauthService.initCodeFlow();
    });
  }

  processMicrosoftLoginTokens() {
    this.oauthService.configure(this.microsoftOidcConfig);
    return this.processOidcLoginTokens(this.oauthService.loadDiscoveryDocumentAndTryLogin);
  }


  private processOidcLoginTokens(tokenLoader: () => Promise<boolean>): Observable<{ access_token: string | null, id_token: string | null }> {
    let access_token: string | null;
    let id_token: string | null;

    if (!this.location.path(true).includes('=')) {
      access_token = null;
      id_token = null;
      return of({ access_token, id_token });
    }

    return merge([
      from(tokenLoader.bind(this.oauthService)()),
      this.oauthService.events.pipe(filter(event => event.type === 'token_received'))
    ]).pipe(
      mergeMap(r => r),
      filter(r => r === false || (r instanceof OAuthEvent)),
      map(() => {
        access_token = this.oauthService.getAccessToken();
        id_token = this.oauthService.getIdToken();
        this.oauthService.logOut(true);

        return { access_token, id_token };
      }));
  }


  initLoginWithTwitter() {
    this.getTwitterRequestToken()
      .subscribe(response => {
        const tokens = Utilities.getQueryParamsFromString(response);

        if (!tokens['oauth_callback_confirmed'])
          throw new Error('Twitter OAuth Callback Not Confirmed');

        const oauthToken = tokens['oauth_token'];
        const oauthTokenSecret = tokens['oauth_token_secret'];

        if (!oauthToken || !oauthTokenSecret)
          throw new Error('Invalid Twitter OAuthToken parameters');

        this.localStorage.savePermanentData(oauthToken, DBkeys.TWITTER_OAUTH_TOKEN);
        this.localStorage.savePermanentData(oauthTokenSecret, DBkeys.TWITTER_OAUTH_TOKEN_SECRET);

        this.authenticateTwitterRequestToken(oauthToken);

      });
  }

  private getTwitterRequestToken() {
    const requestObject = { oauth_callback: this.configurations.baseUrl + '/twitter-login' };
    return this.http.post(this.twitterRequestTokenEndpoint, requestObject, { responseType: 'text' });
  }

  private authenticateTwitterRequestToken(oauthToken: string) {
    window.location.href = `https://api.twitter.com/oauth/authenticate?oauth_token=${oauthToken}`;
  }

  getTwitterAccessToken(oauthToken: string, oauthVerifier: string) {
    const savedOauthToken = this.localStorage.getDataObject<string>(DBkeys.TWITTER_OAUTH_TOKEN);
    const savedOauthTokenSecret = this.localStorage.getDataObject<string>(DBkeys.TWITTER_OAUTH_TOKEN_SECRET);
    this.localStorage.deleteData(DBkeys.TWITTER_OAUTH_TOKEN);
    this.localStorage.deleteData(DBkeys.TWITTER_OAUTH_TOKEN_SECRET);

    if (oauthToken !== savedOauthToken) {
      return throwError(() => new Error('Invalid or Expired Twitter OAuthToken'));
    }

    const requestObject = {
      oauth_token: oauthToken,
      oauth_token_secret: savedOauthTokenSecret,
      oauth_verifier: oauthVerifier,
    };

    return this.http.post(this.twitterAccessTokenEndpoint, requestObject, { responseType: 'text' });
  }


  get accessToken(): string | null {
    return this.localStorage.getData(DBkeys.ACCESS_TOKEN);
  }

  get accessTokenExpiryDate(): Date | null {
    return this.localStorage.getDataObject<Date>(DBkeys.TOKEN_EXPIRES_IN, true);
  }

  get refreshToken(): string | null {
    return this.localStorage.getData(DBkeys.REFRESH_TOKEN);
  }

  get isSessionExpired(): boolean {
    if (this.accessTokenExpiryDate == null) {
      return true;
    }

    return this.accessTokenExpiryDate.valueOf() <= new Date().valueOf();
  }
}



@Injectable()
export class OidcTempStorage implements OAuthStorage {
  constructor(private localStorage: LocalStoreManager) {
  }

  private getTokens() {
    return this.localStorage.getDataObject<{ [key: string]: string; }>(DBkeys.OIDC_TEMP_STORAGE);
  }

  private saveTokens(tokens: { [key: string]: string; }) {
    this.localStorage.saveSessionData(tokens, DBkeys.OIDC_TEMP_STORAGE);
  }

  getItem(key: string): string | null {
    const tokens = this.getTokens();
    if (tokens == null)
      return null;

    return tokens[key];
  }

  removeItem(key: string): void {
    const tokens = this.getTokens();
    if (tokens == null)
      return;

    delete tokens[key];

    if (Utilities.TestIsObjectEmpty(tokens))
      this.localStorage.deleteData(DBkeys.OIDC_TEMP_STORAGE);
    else
      this.saveTokens(tokens);
  }

  setItem(key: string, data: string): void {
    const tokens = this.getTokens() ?? {};

    tokens[key] = data;
    this.saveTokens(tokens);
  }
}
