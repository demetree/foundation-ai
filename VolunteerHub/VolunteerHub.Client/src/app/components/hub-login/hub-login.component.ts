import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { HubAuthService } from '../../services/hub-auth.service';

@Component({
    selector: 'app-hub-login',
    templateUrl: './hub-login.component.html',
    styleUrls: ['./hub-login.component.scss']
})
export class HubLoginComponent {

    step: 'identifier' | 'code' | 'pending' = 'identifier';
    identifier = '';
    code = '';
    isLoading = false;
    errorMessage = '';
    successMessage = '';

    constructor(
        private auth: HubAuthService,
        private router: Router
    ) {
        // If already logged in, redirect to dashboard
        if (this.auth.hasValidSession()) {
            this.router.navigate(['/dashboard']);
        }
    }

    async onRequestCode(): Promise<void> {
        if (!this.identifier.trim()) return;

        this.isLoading = true;
        this.errorMessage = '';

        try {
            const result = await this.auth.requestCode(this.identifier.trim());

            // Check if the server detected a pending registration
            if ((result as any).status === 'pending') {
                this.successMessage = result.message;
                this.step = 'pending';
            } else {
                this.successMessage = result.message;
                this.step = 'code';
            }
        } catch (err: any) {
            this.errorMessage = err?.error?.message || 'Something went wrong. Please try again.';
        } finally {
            this.isLoading = false;
        }
    }

    async onVerifyCode(): Promise<void> {
        if (!this.code.trim()) return;

        this.isLoading = true;
        this.errorMessage = '';

        try {
            await this.auth.verifyCode(this.identifier.trim(), this.code.trim());
            this.router.navigate(['/dashboard']);
        } catch (err: any) {
            this.errorMessage = err?.error?.message || 'Invalid or expired code. Please try again.';
        } finally {
            this.isLoading = false;
        }
    }

    onBack(): void {
        this.step = 'identifier';
        this.code = '';
        this.errorMessage = '';
        this.successMessage = '';
    }
}
