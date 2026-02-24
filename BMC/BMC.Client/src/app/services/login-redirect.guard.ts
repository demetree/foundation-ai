import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from './auth.service';


///
/// Prevents authenticated users from seeing the login page.
/// If the user is already logged in and holds a valid (non-expired) token,
/// they are redirected to the home page instead of displaying the login form.
///
export const LoginRedirectGuard: CanActivateFn = () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.isLoggedIn && !authService.isSessionExpired) {
        router.navigate([authService.homeUrl]);
        return false;
    }

    return true;
}
