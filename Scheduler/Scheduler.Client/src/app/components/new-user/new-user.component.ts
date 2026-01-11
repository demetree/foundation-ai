import { HttpClient } from '@angular/common/http';
import { Component, Inject, Input, OnInit } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { AuthService } from '../../services/auth.service';
import { ConfigurationService } from '../../services/configuration.service';

@Component({
  selector: 'app-new-user',
  templateUrl: './new-user.component.html',
  styleUrl: './new-user.component.scss'
})
export class NewUserComponent implements OnInit {
  isLoading = false;
  @Input() isModal = false;
  public newUserToken: string | null = null;

  modalClosedCallback: { (): void } | undefined;
  securityUser: any;
  scrolledToBottom = false;

  constructor(private alertService: AlertService,
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private configurations: ConfigurationService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,


  ) {

  }


  ngOnInit() {

    this.newUserToken = this.route.snapshot.paramMap.get('newUserToken');
    this.checkTokenValidity();

    if (this.authService.isLoggedIn) {
      this.authService.logout(); // Call your logout function here
    }
    if (this.newUserToken) {
      this.http.get<any>(`${this.baseUrl}api/User/GetUserByToken?token=${encodeURIComponent(this.newUserToken)}`)
        .subscribe({
          next: (user) => {
            this.securityUser = user;
            console.log("SecurityUser", this.securityUser);
          },
          error: (err) => {
            this.alertService.showMessage('Error', 'Unable to fetch user from token.', MessageSeverity.error);
          }
        });
    }

  }
  onSubmitForm(form: NgForm): void {
    if (form.valid) {
      this.submitForm();
    } else {
      this.alertService.showMessage('Error', 'Please complete all required fields before submitting.', MessageSeverity.error);
    }
  }


  saveSecurityUser() {
    this.isLoading = true;
    setTimeout(() => {
      console.log('Security User Saved:', this.securityUser);
      alert('Dummy Save Complete! Check console for output.');
      this.isLoading = false;
    }, 1000);
  }

  onImageUpload(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.securityUser.image = file;
      console.log('Image uploaded:', file.name);
    }
  }


  submitForm() {
    //
    // This will activate the user if the token is valid.
    //
    const apiEndpoint = `${this.baseUrl}api/User/ActivateUser`;

    const requestBody = {
      token: this.newUserToken,  // this.token should be available from route or service
      updatedUser: this.securityUser
    };

    this.isLoading = true;

    this.http.put<any>(apiEndpoint, requestBody).subscribe({
      next: (response) => {
        this.alertService.showMessage(
          'Success',
          'User details updated successfully.',
          MessageSeverity.success
        );
        this.isLoading = false;
        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error('SubmitUser error:', error); // Helpful for debugging
        this.alertService.showMessage(
          'Error',
          'Failed to update user details. Please try again later.',
          MessageSeverity.error
        );
        this.isLoading = false;
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
    return this.securityUser.password === this.securityUser.confirmPassword;
  }

  showTerms = false;

  onTermsScroll(event: Event): void {
    const target = event.target as HTMLElement;
    const atBottom = target.scrollHeight - target.scrollTop <= target.clientHeight + 10;
    if (atBottom) {
      this.scrolledToBottom = true;
    }
  }
  //WIll use the check token  from reset controller
  onTermsCheckboxClick(event: Event): void {
    if (!this.scrolledToBottom) {
      event.preventDefault(); // Prevent the checkbox from toggling
      this.alertService.showMessage(
        'Please scroll through and read the Terms and Conditions to check this box.',
        '',
        MessageSeverity.warn
      );
    }
  }

  checkTokenValidity() {
    const apiEndpoint = `${this.baseUrl}api/User/IsTokenInvalid`;

    const requestBody = {
      token: this.newUserToken
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

  resetForm() {
    this.securityUser = {

    };
  }

  closeModal() {
    console.log('Modal closed');
  }

  showErrorAlert(title: string, message: string) {
    alert(`${title}: ${message}`);
  }
}

