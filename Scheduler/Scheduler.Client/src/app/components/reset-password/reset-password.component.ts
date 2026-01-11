import { HttpClient } from '@angular/common/http';
import { Component, Inject, Input, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { AuthService } from '../../services/auth.service';
import { Utilities } from '../../services/utilities';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent implements OnInit {
  isLoading = false;
  showRedirectingMessage = false;
  @Input() isModal = false;
  public isUserLoggedIn = false;

  public token: string | null = null;

  userPassword = {
    newPassword: '',
    confirmPassword: ''
  };
  resetToken: any;

  constructor(private alertService: AlertService,
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
  ) { }

  ngOnInit(): void {


    if (this.authService.isLoggedIn) {
      this.authService.logout();
    }

    this.token = this.route.snapshot.paramMap.get('token');
    console.log(this.token);
    this.checkTokenValidity()


  }

  closeModal() {
    this.userPassword.newPassword = '';
    this.userPassword.confirmPassword = '';
  }

  resetPassword() {
    if (this.userPassword.newPassword !== this.userPassword.confirmPassword) {
      this.alertService.showMessage(
        'Passwords do not match',
        'Please make sure both fields match.',
        MessageSeverity.error
      );
      return;
    }

    this.isLoading = true;
    this.showRedirectingMessage = true;

    const headers = this.authService.GetAuthenticationHeaders();
    const apiEndpoint = `${this.baseUrl}api/User/SetPassword`;

    const requestBody = {
      token: this.token,
      password: this.userPassword.newPassword
    };

    this.http.post<any>(apiEndpoint, requestBody, { headers }).subscribe({
      next: (response) => {
        this.alertService.showMessage(
          'Password Reset',
          'Your password has been reset successfully. Redirecting...',
          MessageSeverity.success
        );

        setTimeout(() => {
          this.isLoading = false;
          this.router.navigate(['/']);
        }, 3000);
      },
      error: (error) => {
        this.isLoading = false;
        this.showRedirectingMessage = false;

        let errorMessage = 'Failed to reset your password.';

        if (Utilities.checkNoNetwork(error)) {
          errorMessage = 'No network connection.';
        } else if (error.status === 400 && error.error) {
          errorMessage = typeof error.error === 'string'
            ? error.error
            : error.error.message || errorMessage;
        } else if (error.status === 404 && error.error) {
          errorMessage = typeof error.error === 'string'
            ? error.error
            : error.error.message || 'Resource not found.';
        }

        this.alertService.showMessage(
          'Error',
          errorMessage,
          MessageSeverity.error
        );
      }
    }); 
  }


  isPasswordValid(password: string): boolean {
    const minLength = 8;
    const upper = /[A-Z]/;
    const lower = /[a-z]/;
    const digit = /\d/;
    const special = /[^A-Za-z0-9]/;

    return password?.length >= minLength &&
      upper.test(password) &&
      lower.test(password) &&
      digit.test(password) &&
      special.test(password);
  }

  isConfirmPasswordValid(): boolean {
    return this.userPassword.confirmPassword === this.userPassword.newPassword;
  }


  checkTokenValidity() {
    const apiEndpoint = `${this.baseUrl}api/User/IsTokenInvalid`;

    const requestBody = {
      token: this.token
    };

    this.http.post<{ isInvalidToken: boolean }>(apiEndpoint, requestBody).subscribe({
      next: (response) => {
        if (response.isInvalidToken) {
          // Invalid token - redirect to homepage
          this.alertService.showMessage(
            'Invalid or Expired Token',
            'The password reset link is either expired or has already been used.',
            MessageSeverity.error
          );
          this.router.navigate(['/']);
        }
      },
      error: (error) => {
        // Could also be 404 or 500, treat as invalid
        this.alertService.showMessage(
          'Invalid Token',
          'The password reset token is not valid or not found.',
          MessageSeverity.error
        );
        this.router.navigate(['/']);
      }
    });
  }



  onSubmitClick() {
    console.log('Submit button clicked');
  }

  showErrorAlert(title: string, message: string) {
    this.alertService.showMessage(title, message, MessageSeverity.warn);
  }
}

