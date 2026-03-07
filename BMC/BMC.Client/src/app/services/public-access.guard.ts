import { CanActivateFn } from '@angular/router';


///
/// Guard for publicly accessible browse routes.
///
/// Always allows access — no authentication check.
/// Routes using this guard are accessible to both anonymous
/// visitors and logged-in users.
///
/// Naming is intentional: routes protected by PublicAccessGuard
/// clearly communicate that they are public-facing in the
/// route configuration.
///
export const PublicAccessGuard: CanActivateFn = () => {
    return true;
}
