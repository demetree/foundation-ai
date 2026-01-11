import { Component, OnInit, OnDestroy, Input, Inject, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { AppTranslationService } from '../../services/app-translation.service';
import { AuthService } from '../../services/auth.service';
import { CacheManagerService } from '../../services/cache-manager.service';
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
  public userLogin = new UserLogin();
  public isLoading = false;
  public isExternalLogin = false;
  public formResetToggle = true;
  public modalClosedCallback: { (): void } | undefined;
  public loginStatusSubscription: Subscription | undefined;
  public userEmail: string = ''; // Forgot password email model
  public showForgotPassword: boolean = false; // Toggle between login and forgot password
  public showPassword: boolean = false;

  @Input() isModal = false;

  // Reference to the native username input element
  @ViewChild('usernameInput', { static: false }) usernameInputElement!: ElementRef<HTMLInputElement>;

  gT = (key: string | Array<string>, interpolateParams?: object) => this.translationService.getTranslation(key, interpolateParams);

  constructor(private alertService: AlertService,
    private translationService: AppTranslationService,
    @Inject('BASE_URL') private baseUrl: string,
    private authService: AuthService,
    private cacheManagerService: CacheManagerService,
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

  ngAfterViewInit(): void {
    // Initial focus when the component is first displayed
    this.focusUsernameField();

    // Optional: re-focus when returning from "Forgot Password" view
    // We can watch the flag with a simple setter or use a subscription if preferred
  }

  ngOnDestroy() {
    this.loginStatusSubscription?.unsubscribe();
  }


  /**
   * Focuses the username input field if the login form is currently visible.
   * Safe to call multiple times — checks existence first.
   */
  private focusUsernameField(): void {
    if (!this.showForgotPassword && this.usernameInputElement?.nativeElement) {
      // Use setTimeout to ensure the DOM is fully rendered (especially in modals)
      setTimeout(() => {
        this.usernameInputElement.nativeElement.focus();
      }, 0);
    }
  }


  focusUsernameAfterCancel(): void {
    this.showForgotPassword = false;
    this.focusUsernameField();
  }

  getShouldRedirect() {
    return !this.isModal && this.authService.isLoggedIn && !this.authService.isSessionExpired;
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
    this.cacheManagerService.ClearAllCaches();

    this.authService.loginWithPassword(this.userLogin.userName, this.userLogin.password, this.userLogin.rememberMe)
      .subscribe({
        next: user => {
          setTimeout(() => {
            this.alertService.stopLoadingMessage();

            this.isLoading = false;

            if (!this.isModal) {

              if (this.authService.isSchedulerReader == false) {
                //
                // Display can't read alert
                //
                this.alertService.resetStickyMessage();
                this.alertService.showMessage(this.gT('app.alerts.Login'), "Your account does not have the privilege to read from Basecamp", MessageSeverity.error);

              } else {
                //
                // Display normal login alert
                //
                let welcomeMessage = "Welcome to " + user.tenantName + " " + user.fullName;

                this.alertService.showMessage(this.gT('login.alerts.Login'), welcomeMessage, MessageSeverity.success);

                // Begin polling for setup data changes
                //this.cacheManagerService.startPolling();
              }
            } else {
              this.alertService.showMessage(this.gT('login.alerts.Login'), this.gT('login.alerts.UserSessionRestored', { username: user.userName }), MessageSeverity.success);
              //setTimeout(() => {
              //  this.alertService.showStickyMessage(this.gT('login.alerts.SessionRestored'), this.gT('login.alerts.RetryLastOperation'), MessageSeverity.default);
              //}, 500);


              // Begin polling for setup data changes
              //this.cacheManagerService.startPolling();

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

  reset() {
    this.formResetToggle = false;

    setTimeout(() => {
      this.formResetToggle = true;
    });
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
      accountName: this.userEmail
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


  showErrorAlert(caption: string, message: string) {
    caption = this.gT(caption);
    message = this.gT(message);
    this.alertService.showMessage(caption, message, MessageSeverity.error);
    this.isLoading = false;
  }

  public togglePasswordVisibility() {
    if (this.showPassword) {
      this.showPassword = false;
    }
    else {
      this.showPassword = true;
    }
  }

  // Handles login form submission
  public onSubmit(form: any): void {
    // Trigger validation visuals
    form.control.markAllAsTouched();

    // Handle invalid form in ONE place
    if (form.invalid) {
      // Both fields empty
      if (!this.userLogin.userName && !this.userLogin.password) {
        this.showErrorAlert('Login Required', 'Please enter your username and password');
      }
      // Username missing
      else if (!this.userLogin.userName) {
        this.showErrorAlert('Username Required','Please enter a valid username');
      }
      // Password missing
      else if (!this.userLogin.password) {
        this.showErrorAlert('Password Required', 'Please enter a valid password');
      }
      return;
    }

    this.login();
  }
}


