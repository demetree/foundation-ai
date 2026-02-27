import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {

    userName = '';
    password = '';
    rememberMe = false;
    isLoading = false;
    loginStatusMessage = '';
    isModal = false;
    modalClosedCallback: (() => void) | undefined;

    constructor(
        private authService: AuthService,
        private alertService: AlertService,
        private router: Router,
        private route: ActivatedRoute
    ) { }


    ngOnInit(): void {

        //
        // If a returnUrl query parameter was provided (e.g. from the public landing page),
        // set it as the post-login redirect destination.
        //
        let returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');

        if (returnUrl) {
            this.authService.loginRedirectUrl = returnUrl;
        }
    }


    login() {
        if (!this.userName || !this.password) {
            this.alertService.showMessage('Login Error', 'Please enter your username and password.', MessageSeverity.warn);
            return;
        }

        this.isLoading = true;
        this.loginStatusMessage = 'Signing in...';

        this.authService.loginWithPassword(this.userName, this.password, this.rememberMe)
            .subscribe({
                next: () => {
                    this.isLoading = false;
                    this.loginStatusMessage = '';
                    this.alertService.showMessage('Login', 'Welcome back!', MessageSeverity.success);

                    if (this.isModal && this.modalClosedCallback) {
                        this.modalClosedCallback();
                    } else {
                        this.authService.redirectLoginUser();
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
