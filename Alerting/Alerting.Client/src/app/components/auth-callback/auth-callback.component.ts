import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Params } from '@angular/router';
import { Subscription } from 'rxjs';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { AppTranslationService } from '../../services/app-translation.service';
import { AuthService, OidcProviders } from '../../services/auth.service';
import { Utilities } from '../../services/utilities';
import { AlertingDataServiceManagerService } from '../../alerting-data-services/alerting-data-service-manager.service';

@Component({
  selector: 'app-auth-callback',
  templateUrl: './auth-callback.component.html',
  styleUrl: './auth-callback.component.scss'
})
export class AuthCallbackComponent implements OnInit, OnDestroy {
  message: string | null = null;
  isLoading = false;
  provider!: OidcProviders;
  externalAuthToken: string | undefined;
  foundEmail: string | null = null;
  userPassword: string | null = null;
  pageSubscriptions = new Subscription();

  gT = (key: string | Array<string>, interpolateParams?: object) =>
    this.translationService.getTranslation(key, interpolateParams);

  constructor(
    private route: ActivatedRoute,
    private alertService: AlertService,
    private translationService: AppTranslationService,
    private authService: AuthService,
    private alertingDataServiceManagerService: AlertingDataServiceManagerService) {
  }

  ngOnInit() {
    if (this.getShouldRedirect()) {
      this.authService.redirectLoginUser();
      return;
    } else {
      this.pageSubscriptions.add(
        this.authService.getLoginStatusEvent().subscribe(() => {
          if (this.getShouldRedirect()) {
            this.authService.redirectLoginUser();
          }
        }));
    }

    this.setProvider(this.route.snapshot.url[0].path);
    this.isLoading = true;

    if (this.provider === 'twitter') {
      this.route.queryParams.subscribe(params => {
        const queryParams = Utilities.GetObjectWithLoweredPropertyNames(params);
        this.processTokens(queryParams);
      });
    }
    else {
      this.pageSubscriptions.add(
        this.authService.processExternalOidcLoginTokens(this.provider).subscribe({
          next: tokens => {
            this.processTokens(tokens);
          },
          error: error => {
            this.isLoading = false;
            this.message = null;
            this.showLoginErrorMessage(error);
          }
        }));
    }
  }

  ngOnDestroy() {
    this.pageSubscriptions.unsubscribe();
  }

  getShouldRedirect() {
    return this.authService.isLoggedIn && !this.authService.isSessionExpired;
  }

  showErrorAlert(caption: string, message: string) {
    if (caption) {
      caption = this.gT(caption);
    }

    if (message) {
      message = this.gT(message);
    }

    this.alertService.showMessage(caption, message, MessageSeverity.error);
  }

  setProvider(url: string) {
    const google = 'google';
    const facebook = 'facebook';
    const twitter = 'twitter';
    const microsoft = 'microsoft';

    if (url.includes(google)) {
      this.provider = google;
    } else if (url.includes(facebook)) {
      this.provider = facebook;
    } else if (url.includes(twitter)) {
      this.provider = twitter;
    } else if (url.includes(microsoft)) {
      this.provider = microsoft;
    }
    else {
      throw new Error('Unknown login provider');
    }
  }

  processTokens(tokensObject: Params) {
    let tokenProcessed = false;

    if (tokensObject) {
      if (tokensObject['id_token']) {
        tokenProcessed = true;
        this.loginWithToken(tokensObject['id_token'], this.provider);
      }
      else if (tokensObject['oauth_token'] && tokensObject['oauth_verifier']) {
        if (this.provider === 'twitter') {
          tokenProcessed = true;
          this.isLoading = true;
          this.message = this.gT('authCallback.ConnectingToTwitter');
          this.authService.getTwitterAccessToken(tokensObject['oauth_token'], tokensObject['oauth_verifier'])
            .subscribe({
              next: accessToken => {
                this.isLoading = true;
                this.message = this.gT('authCallback.Processing');
                this.loginWithToken(accessToken, this.provider);
              },
              error: error => {
                this.isLoading = false;
                this.message = null;
                this.showLoginErrorMessage(error);
              }
            });
        }
      }
    }

    if (!tokenProcessed) {

      this.alertingDataServiceManagerService.ClearAllCaches();

      setTimeout(() => {
        this.alertService.showMessage(this.gT('authCallback.alerts.InvalidLogin'),
          this.gT('authCallback.alerts.NoValidTokensFound'), MessageSeverity.error);
      }, 500);

      this.message = this.gT('authCallback.Error');
      this.authService.redirectLogoutUser();
    }
  }


  loginWithToken(token: string, provider: string) {
    this.externalAuthToken = token;
    this.isLoading = true;
    this.message = this.gT('authCallback.Processing');
    this.alertService.startLoadingMessage('', this.gT('authCallback.alerts.SigningIn'));

    // Clear all data service caches at login events.
    this.alertingDataServiceManagerService.ClearAllCaches();

    this.authService.loginWithExternalToken(token, provider)
      .subscribe({
        next: user => {
          setTimeout(() => {
            this.alertService.stopLoadingMessage();
            this.isLoading = false;

            this.alertService.showMessage(this.gT('login.alerts.Login'),
              this.gT('login.alerts.Welcome', { username: user.userName }), MessageSeverity.success);
          }, 500);
        },
        error: error => {
          this.alertService.stopLoadingMessage();
          this.isLoading = false;
          this.message = this.gT('authCallback.Error');
          this.foundEmail = Utilities.findHttpResponseMessage('email', error);

          if (this.foundEmail) {
            const errorMessage = Utilities.getHttpResponseMessage(error) as string;
            this.alertService.showStickyMessage(this.gT('authCallback.alerts.UserAlreadyExists'),
              this.mapLoginErrorMessage(errorMessage), MessageSeverity.default, error);
          } else {
            this.showLoginErrorMessage(error);
          }
        }
      });
  }

  linkAccountAndLogin() {
    this.isLoading = true;
    this.alertService.startLoadingMessage('', this.gT('login.alerts.AttemptingLogin'));

    this.authService.loginWithExternalToken(this.externalAuthToken as string, this.provider, this.userPassword)
      .subscribe({
        next: user => {

          // Clear all data service caches at login events.
          this.alertingDataServiceManagerService.ClearAllCaches();

          setTimeout(() => {
            this.alertService.stopLoadingMessage();
            this.isLoading = false;
            this.userPassword = null;

            this.alertService.showMessage(this.gT('login.alerts.Login'),
              this.gT('login.alerts.Welcome', { username: user.userName }), MessageSeverity.success);
          }, 500);
        },
        error: error => {

          this.alertingDataServiceManagerService.ClearAllCaches();

          this.alertService.stopLoadingMessage();
          this.showLoginErrorMessage(error, false);

          setTimeout(() => {
            this.isLoading = false;
          }, 500);
        }
      });
  }

  showLoginErrorMessage(error: HttpErrorResponse, redirect = true) {
    setTimeout(() => {
      if (Utilities.checkNoNetwork(error)) {
        this.alertService.showStickyMessage(this.gT('app.NoNetwork'),
          this.gT('app.ServerCannotBeReached'), MessageSeverity.error, error);
      } else {
        const errorMessage = Utilities.getHttpResponseMessage(error);

        if (errorMessage) {
          this.alertService.showStickyMessage(this.gT('login.alerts.UnableToLogin'),
            this.mapLoginErrorMessage(errorMessage), MessageSeverity.error, error);
        } else {
          this.alertService.showStickyMessage(this.gT('login.alerts.UnableToLogin'),
            this.gT('login.alerts.LoginErrorOccurred', { error: Utilities.stringify(error) }),
            MessageSeverity.error, error);
        }
      }

    }, 500);

    if (redirect) {
      this.authService.redirectLogoutUser();
    }
  }

  mapLoginErrorMessage(error: string) {
    if (error === 'invalid_username_or_password') {
      return this.gT('login.alerts.InvalidUsernameOrPassword');
    }

    return error;
  }

  get providerName() {
    return { provider: this.gT(`authCallback.${this.provider}Provider`) };
  }
}
