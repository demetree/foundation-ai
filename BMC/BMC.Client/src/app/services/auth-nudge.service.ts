import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';


export interface AuthNudgeConfig {
    featureName: string;
    featureIcon: string;
    message?: string;
}


@Injectable({
    providedIn: 'root'
})
export class AuthNudgeService {

    private nudgeTrigger = new Subject<AuthNudgeConfig>();

    /** Observable that the modal component subscribes to */
    nudge$ = this.nudgeTrigger.asObservable();


    constructor(private router: Router) { }


    /**
     * Show an auth nudge modal for a feature that requires sign-in.
     */
    nudge(config: AuthNudgeConfig): void {
        this.nudgeTrigger.next(config);
    }


    /**
     * Navigate to login, optionally with a return URL so the user
     * comes back to where they were after signing in.
     */
    goToLogin(returnUrl?: string): void {
        if (returnUrl) {
            this.router.navigate(['/login'], { queryParams: { returnUrl } });
        } else {
            this.router.navigate(['/login']);
        }
    }
}
