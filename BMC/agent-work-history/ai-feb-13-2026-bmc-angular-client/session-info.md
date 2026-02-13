# Session Information

- **Conversation ID:** 17622316-b829-41ad-b715-2107280fa4f4
- **Date:** 2026-02-13
- **Time:** 17:37 NST (UTC-3:30)
- **Duration:** ~4 hours

## Summary

Scaffolded the BMC Angular 17 client from scratch with premium warm brick design system, fixed SPA proxy configuration for Visual Studio F5 launch, created custom SVG favicon, and built complete Foundation-compatible auth infrastructure (OIDC password grant, token refresh, auth guard, cross-tab persistence).

## Files Modified

### Angular Client — New
- `src/favicon.svg` — Custom brick-stack SVG favicon
- `src/app/models/login-response.model.ts` — OIDC token interfaces
- `src/app/models/user.model.ts` — User class with roles/permissions
- `src/app/services/db-keys.ts` — bmc-prefixed localStorage keys
- `src/app/services/jwt-helper.ts` — JWT decode and expiry
- `src/app/services/utilities.ts` — HTTP response helpers
- `src/app/services/auth-guard.ts` — Route guard (CanActivateFn)
- All UI components (header, sidebar, login, dashboard, not-found)

### Angular Client — Rewritten/Updated
- `src/app/services/auth.service.ts` — Full OIDC auth with token refresh
- `src/app/services/oidc-helper.service.ts` — Password/refresh grant flows
- `src/app/services/local-store-manager.service.ts` — Cross-tab sync
- `src/app/services/configuration.service.ts` — Added homeUrl, import(), clearLocalChanges()
- `src/app/app.module.ts` — Added DBkeys, JwtHelper providers
- `src/app/app-routing.module.ts` — AuthGuard on dashboard
- `src/app/app.component.ts` — LocalStoreManager init
- `src/app/components/login/login.component.ts` — redirectLoginUser()

### Server Fixes
- `BMC.Server/Properties/launchSettings.json` — Added ASPNETCORE_HOSTINGSTARTUPASSEMBLIES, fixed ports
- `BMC.Server/BMC.Server.csproj` — Fixed SpaProxyServerUrl port

## Related Sessions

- Initial BMC project creation (database, server, code generation) in earlier conversation
