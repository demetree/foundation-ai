import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from './auth.service';


///
/// Guard for the public landing page.
/// If the user is already logged in with a valid session, redirect to the dashboard.
/// If anonymous, allow the public landing page to render.
///
export const PublicOrRedirectGuard: CanActivateFn = () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.isLoggedIn && !authService.isSessionExpired) {
        router.navigate(['/welcome']);
        return false;
    }

    return true;
}
