import { Component, OnInit, OnDestroy, Input, Inject } from '@angular/core';
import { Subscription } from 'rxjs';

import { AlertService, MessageSeverity, DialogType } from '../../services/alert.service';
import { AppTranslationService } from '../../services/app-translation.service';
import { AuthService } from '../../services/auth.service';
import { AuditorDataServiceManagerService } from '../../auditor-data-services/auditor-data-service-manager.service';
import { SecurityDataServiceManagerService } from '../../security-data-services/security-data-service-manager.service';
import { ConfigurationService } from '../../services/configuration.service';
import { Utilities } from '../../services/utilities';
import { UserLogin } from '../../models/user-login.model';
import { HttpClient } from '@angular/common/http';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})


export class LoginComponent implements OnInit, OnDestroy {
  userLogin = new UserLogin();
  isLoading = false;
  isExternalLogin = false;
  formResetToggle = true;
  modalClosedCallback: { (): void } | undefined;
  loginStatusSubscription: Subscription | undefined;
  userEmail: string = ''; // Forgot password email model
  showForgotPassword: boolean = false; // Toggle between login and forgot password

  @Input()
  isModal = false;

  gT = (key: string | Array<string>, interpolateParams?: object) => this.translationService.getTranslation(key, interpolateParams);

  constructor(private alertService: AlertService,
    private translationService: AppTranslationService,
    @Inject('BASE_URL') private baseUrl: string,
    private authService: AuthService,
    private auditorDataServiceManagerService: AuditorDataServiceManagerService,
    private securityDataServiceManagerService: SecurityDataServiceManagerService,
    private configurations: ConfigurationService,
    private http: HttpClient) {

  }

  ngOnInit() {
    this.userLogin.rememberMe = this.authService.rememberMe;

    if (this.getShouldRedirect()) {
      this.authService.redirectLoginUser();
    } else {
      this.loginStatusSubscription = this.authService.getLoginStatusEvent().subscribe(() => {
        if (this.getShouldRedirect()) {
          this.authService.redirectLoginUser();
        }
      });
    }
  }

  ngOnDestroy() {
    this.loginStatusSubscription?.unsubscribe();
  }

  getShouldRedirect() {
    return !this.isModal && this.authService.isLoggedIn && !this.authService.isSessionExpired;
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

  closeModal() {
    if (this.modalClosedCallback) {
      this.modalClosedCallback();
    }
  }

  changeLanguage(language: string) {
    this.configurations.globalLanguage = language;
    this.configurations.language = language;
  }

  login() {
    this.isLoading = true;
    this.isExternalLogin = false;
    this.alertService.startLoadingMessage('', this.gT('login.alerts.AttemptingLogin'));

    // Wipe all of the data service caches when attempting any login activity.
    this.auditorDataServiceManagerService.ClearAllCaches();
    this.securityDataServiceManagerService.ClearAllCaches();

    this.authService.loginWithPassword(this.userLogin.userName, this.userLogin.password, this.userLogin.rememberMe)
      .subscribe({
        next: user => {
          setTimeout(() => {
            this.alertService.stopLoadingMessage();
            this.isLoading = false;
            this.reset();


            if (!this.isModal) {

              if (this.authService.isAuditorReader == false &&
                  this.authService.isSecurityReader == false) {
                //
                // Display can't read alert
                //
                this.alertService.resetStickyMessage();
                this.alertService.showMessage(this.gT('app.alerts.Login'), "Your account does not have the privilege to read from Auditor or Security", MessageSeverity.error);

              } else {
                //
                // Display normal login alert
                //
                this.alertService.showMessage(this.gT('login.alerts.Login'), this.gT('login.alerts.Welcome', { username: user.userName }), MessageSeverity.success);
              }
            } else {
              this.alertService.showMessage(this.gT('login.alerts.Login'), this.gT('login.alerts.UserSessionRestored', { username: user.userName }), MessageSeverity.success);
              //setTimeout(() => {
              //  this.alertService.showStickyMessage(this.gT('login.alerts.SessionRestored'), this.gT('login.alerts.RetryLastOperation'), MessageSeverity.default);
              //}, 500);

              this.closeModal();
            }
          }, 500);
        },
        error: error => {
          this.alertService.stopLoadingMessage();

          if (Utilities.checkNoNetwork(error)) {
            this.alertService.showStickyMessage(this.gT('app.NoNetwork'), this.gT('app.ServerCannotBeReached'), MessageSeverity.error, error);
          } else {
            const errorMessage = Utilities.getHttpResponseMessage(error);

            if (errorMessage) {
              this.alertService.showStickyMessage(this.gT('login.alerts.UnableToLogin'), this.mapLoginErrorMessage(errorMessage), MessageSeverity.error, error);
            } else {
              this.alertService.showStickyMessage(this.gT('login.alerts.UnableToLogin'),
                this.gT('login.alerts.LoginErrorOccurred', { error: Utilities.stringify(error) }), MessageSeverity.error, error);
            }
          }

          setTimeout(() => {
            this.isLoading = false;
          }, 500);
        }
      });
  }

  loginWithGoogle() {
    this.isLoading = true;
    this.isExternalLogin = true;
    this.alertService.startLoadingMessage('', this.gT('login.alerts.RedirectingToGoogle'));

    this.authService.initLoginWithGoogle(this.userLogin.rememberMe);
  }

  loginWithFacebook() {
    this.isLoading = true;
    this.isExternalLogin = true;
    this.alertService.startLoadingMessage('', this.gT('login.alerts.RedirectingToFacebook'));

    this.authService.initLoginWithFacebook(this.userLogin.rememberMe);
  }

  loginWithTwitter() {
    this.isLoading = true;
    this.isExternalLogin = true;
    this.alertService.startLoadingMessage('', this.gT('login.alerts.RedirectingToTwitter'));

    this.authService.initLoginWithTwitter(this.userLogin.rememberMe);
  }

  loginWithMicrosoft() {
    this.isLoading = true;
    this.isExternalLogin = true;
    this.alertService.startLoadingMessage('', this.gT('login.alerts.RedirectingToMicrosoft'));

    this.authService.initLoginWithMicrosoft(this.userLogin.rememberMe);
  }

  mapLoginErrorMessage(error: string) {
    if (error === 'invalid_username_or_password') {
      return this.gT('login.alerts.InvalidUsernameOrPassword');
    }

    return error;
  }

  recoverPassword() {
    this.isLoading = true;

    const headers = this.authService.GetAuthenticationHeaders();
    const apiEndpoint = `${this.baseUrl}api/User/SendPasswordResetEmail`;

    const requestBody = {
      AccountName: this.userEmail
    };

    this.http.post<any>(apiEndpoint, requestBody, { headers }).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.showForgotPassword = false;
        this.alertService.showMessage(
          'Password Reset Requested',
          'If an account exists for this email, you will receive a password reset link.',
          MessageSeverity.success
        );
      },
      error: (error) => {
        this.alertService.stopLoadingMessage();
        this.isLoading = false;

        let errorMessage = 'Failed to send password reset request.';
        if (Utilities.checkNoNetwork(error)) {
          errorMessage = 'No network connection. Please check your internet.';
        }

        this.alertService.showMessage(
          'Error',
          errorMessage,
          MessageSeverity.error
        );
      }
    });
  }


  onSubmitClick() {
    console.log('Reset Password button clicked');
    this.alertService.showMessage(
      "Email with reset password link will be sent if an account exists",
      '',
      MessageSeverity.success
    );
    // You can add tracking, animations, etc. here
  }

  reset() {
    this.formResetToggle = false;

    setTimeout(() => {
      this.formResetToggle = true;
    });
  }
}
