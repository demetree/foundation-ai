# BMC Angular Client — Walkthrough

## What Was Built

A complete Angular 17.2 SPA for BMC, scaffolded from scratch around 157 pre-existing generated files.

### Project Config
- `package.json` — Angular 17.2 + Bootstrap 5 + NgBootstrap + OIDC + Toasta
- `angular.json` — Dev server on **port 12200**, proxy to BMC.Server on **12101**
- `tsconfig.json` / `tsconfig.app.json` / `tsconfig.spec.json`

### Design System (`styles.scss`)
**Warm amber/orange palette** — distinct from Scheduler's blue/slate:
- CSS custom properties for brand colors, surfaces, text, borders, shadows
- Card system, 3 button variants, form/table/modal dark-theme overrides
- Custom scrollbars, fade/slide animations, utility classes

### UI Components
| Component | Highlights |
|---|---|
| **Header** | Gradient brand with cube icon, auth-aware nav, sign-out button |
| **Sidebar** | Collapsible with 5 routes, active-route amber glow, slide-in animation |
| **Login** | Glassmorphic card with brick-stack logo, icon-decorated inputs, spinner |
| **Dashboard** | Live API stat cards (parts, categories, colours, projects), quick actions, floating brick welcome banner |
| **Not Found** | Scattered-brick 404 with gradient text |

### Foundation Services
- `auth.service.ts` — OIDC password-grant login, localStorage token persistence
- `alert.service.ts` — Observable-based toast/dialog notification system
- `configuration.service.ts`, `local-store-manager.service.ts`, `oidc-helper.service.ts`, `app-title.service.ts`

### Loading Screen (`index.html`)
Animated brick-stacking motif with spinning ring and pulsing amber glow.

## SPA Proxy Fix

The Angular client wasn't launching when starting BMC.Server from Visual Studio. Two issues were found:

1. **Missing `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`** in `launchSettings.json` — without this env var set to `Microsoft.AspNetCore.SpaProxy`, the SPA proxy NuGet package is referenced but never activated.
2. **Port mismatches** — `SpaProxyServerUrl` in `.csproj` was `12901` (should be `12200`), and `applicationUrl` in `launchSettings.json` was `52899/52900` (should be `12101/12100`).

## Build Verification

| Step | Result |
|---|---|
| `npm install` | ✅ 1104 packages installed |
| `ng build` | ✅ 0 errors, 896 KB initial bundle |
| `ng serve --port 12200` | ✅ Dev server started |
| Visual Studio F5 launch | ✅ SPA proxy launches Angular client automatically |
| Dashboard renders | ✅ Confirmed with screenshot |

## Files Created/Modified

```
BMC.Client/
├── package.json, angular.json, tsconfig*.json
├── aspnetcore-https.js
└── src/
    ├── main.ts, index.html, styles.scss, proxy.conf.js
    ├── environments/
    └── app/
        ├── app.module.ts, app-routing.module.ts, app.component.*
        ├── components/ (header, sidebar, login, dashboard, not-found)
        ├── services/ (alert, auth, config, localStorage, oidc, appTitle)
        ├── bmc-data-services/   (13 generated)
        └── bmc-data-components/ (144 generated)

BMC.Server/
├── Properties/launchSettings.json  (fixed SPA proxy)
└── BMC.Server.csproj               (fixed SpaProxyServerUrl port)
```
