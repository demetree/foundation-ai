import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { HubAuthService } from '../services/hub-auth.service';

@Injectable({ providedIn: 'root' })
export class HubAuthGuard implements CanActivate {

    constructor(
        private auth: HubAuthService,
        private router: Router
    ) { }

    canActivate(): boolean {
        if (this.auth.hasValidSession()) {
            return true;
        }

        this.router.navigate(['/login']);
        return false;
    }
}
