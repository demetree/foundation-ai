# BMC Angular Client — Walkthrough

## What Was Built

A complete Angular 17.2 SPA for BMC, scaffolded from scratch with 157 pre-existing generated files.

### Design System (`styles.scss`)
Warm amber/orange palette distinct from Scheduler's blue/slate — CSS custom properties, dark theme, custom scrollbars, animations.

### UI Components
| Component | Highlights |
|---|---|
| **Header** | Gradient brand with cube icon, auth-aware nav |
| **Sidebar** | Collapsible, 5 routes, amber glow, slide-in |
| **Login** | Glassmorphic card, brick-stack logo, icon inputs |
| **Dashboard** | Live API stat cards, quick actions, floating brick welcome banner |
| **Not Found** | Scattered-brick 404 |

### Loading Screen (`index.html`)
Animated brick-stacking motif with spinning ring and pulsing amber glow.

### Custom Favicon
Handcrafted SVG with 3-row brick stack in amber/orange gradients on dark background.

---

## SPA Proxy Fix

Angular client wasn't launching from Visual Studio. Two issues fixed:

1. **Missing `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`** in `launchSettings.json` — required to activate SPA proxy
2. **Port mismatches** — `.csproj` had `12901` (→ `12200`), `launchSettings.json` had `52899/52900` (→ `12101/12100`)

---

## Auth Infrastructure

Full Foundation-compatible authentication system for BMC:

### Models & Helpers
- `LoginResponse` + `IdToken` interfaces matching Foundation OIDC token shape
- `User` class with roles, permissions, tenant
- `DBkeys` with `bmc_`-prefixed localStorage keys (avoids collisions)
- `JwtHelper` — base64 JWT decode & expiry
- `Utilities` — HTTP response helpers, JSON parse, string utils

### Core Services
- **`AuthService`** — OIDC password grant, JWT ID token decode, role extraction, token refresh timer (1 min before expiry), `GetAuthenticationHeaders()` for API calls
- **`OidcHelperService`** — `bmc_spa` client ID, password + refresh grant flows via `/connect/token`
- **`LocalStoreManager`** — Full cross-tab session storage sync (Foundation pattern)
- **`ConfigurationService`** — Added `homeUrl`, `defaultHomeUrl`, `import()`, `clearLocalChanges()`

### Routing & Guards
- **`AuthGuard`** — Functional `CanActivateFn`, redirects to `/login`, preserves intended URL
- Dashboard route protected by `AuthGuard`
- Login component uses `redirectLoginUser()` for proper post-login redirect

## Build Verification

| Step | Result |
|---|---|
| `npm install` | ✅ 1104 packages |
| `ng build` | ✅ 0 errors |
| Visual Studio F5 | ✅ SPA proxy launches Angular |
| Auth infrastructure build | ✅ 0 errors, 2.99 MB bundle |

## Files Created/Modified

```
BMC.Client/src/
├── favicon.svg                          [NEW]
├── app/
│   ├── models/
│   │   ├── login-response.model.ts      [NEW]
│   │   └── user.model.ts               [NEW]
│   ├── services/
│   │   ├── auth.service.ts              [REWRITTEN]
│   │   ├── auth-guard.ts               [NEW]
│   │   ├── configuration.service.ts     [UPDATED]
│   │   ├── db-keys.ts                  [NEW]
│   │   ├── jwt-helper.ts              [NEW]
│   │   ├── local-store-manager.service.ts [REWRITTEN]
│   │   ├── oidc-helper.service.ts      [REWRITTEN]
│   │   └── utilities.ts               [NEW]
│   ├── app.module.ts                    [UPDATED]
│   ├── app-routing.module.ts            [UPDATED]
│   ├── app.component.ts                 [UPDATED]
│   └── components/login/login.component.ts [UPDATED]

BMC.Server/
├── Properties/launchSettings.json       [FIXED]
└── BMC.Server.csproj                    [FIXED]
```
