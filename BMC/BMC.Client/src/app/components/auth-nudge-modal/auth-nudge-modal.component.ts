import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthNudgeService, AuthNudgeConfig } from '../../services/auth-nudge.service';
import { AuthService } from '../../services/auth.service';


@Component({
    selector: 'app-auth-nudge-modal',
    templateUrl: './auth-nudge-modal.component.html',
    styleUrl: './auth-nudge-modal.component.scss'
})
export class AuthNudgeModalComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    visible = false;
    config: AuthNudgeConfig | null = null;
    private currentUrl = '';


    constructor(
        private nudgeService: AuthNudgeService,
        private authService: AuthService
    ) { }


    ngOnInit(): void {
        this.nudgeService.nudge$
            .pipe(takeUntil(this.destroy$))
            .subscribe(config => {
                // Only show for anonymous users
                if (!this.authService.isLoggedIn) {
                    this.config = config;
                    this.currentUrl = window.location.pathname;
                    this.visible = true;
                }
            });
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    get displayMessage(): string {
        if (this.config?.message) {
            return this.config.message;
        }
        return `Sign in to access ${this.config?.featureName ?? 'this feature'} and unlock the full BMC experience.`;
    }


    signIn(): void {
        this.visible = false;
        this.nudgeService.goToLogin(this.currentUrl);
    }


    dismiss(): void {
        this.visible = false;
        this.config = null;
    }
}
