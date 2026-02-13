import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss'
})
export class LoginComponent {

    userName = '';
    password = '';
    isLoading = false;
    loginStatusMessage = '';
    isModal = false;
    modalClosedCallback: (() => void) | undefined;

    constructor(
        private authService: AuthService,
        private alertService: AlertService,
        private router: Router,
    ) { }


    login() {
        if (!this.userName || !this.password) {
            this.alertService.showMessage('Login Error', 'Please enter your username and password.', MessageSeverity.warn);
            return;
        }

        this.isLoading = true;
        this.loginStatusMessage = 'Signing in...';

        this.authService.loginWithPassword(this.userName, this.password)
            .subscribe({
                next: () => {
                    this.isLoading = false;
                    this.loginStatusMessage = '';
                    this.alertService.showMessage('Login', 'Welcome back!', MessageSeverity.success);

                    if (this.isModal && this.modalClosedCallback) {
                        this.modalClosedCallback();
                    } else {
                        this.router.navigate(['/dashboard']);
                    }
                },
                error: (error) => {
                    this.isLoading = false;
                    this.loginStatusMessage = '';
                    this.alertService.showMessage('Login Error', 'Invalid username or password.', MessageSeverity.error);
                }
            });
    }


    reset() {
        this.userName = '';
        this.password = '';
        this.isLoading = false;
        this.loginStatusMessage = '';
    }
}
