# BMC Angular Client — Fresh Project Setup

Initialize a polished Angular 17 SPA for the BMC application, matching the Foundation/Scheduler patterns while giving BMC its own premium visual identity.

## User Review Required

> [!IMPORTANT]
> This plan creates the full Angular project scaffold manually (no `ng new`) since the `src/app/` directory already contains 157 generated files from the code generator. Using `ng new` would overwrite them.

> [!NOTE]
> The BMC client will reuse Foundation services (auth, alerts, OIDC, configuration) from the Scheduler.Client pattern — these are proven, tested utilities. The UI theme will be distinct: a **warm brick-orange/amber** palette rather than Scheduler's blue/slate to reinforce BMC's identity.

## Proposed Changes

### Project Root Files

#### [NEW] [package.json](file:///d:/source/repos/scheduler/BMC/BMC.Client/package.json)
Angular 17.2 dependencies matching Scheduler, plus:
- Bootstrap 5, NgBootstrap, FontAwesome
- OIDC auth libraries
- Toastr/Toasta notifications
- Three.js (future 3D viewer readiness)

#### [NEW] [angular.json](file:///d:/source/repos/scheduler/BMC/BMC.Client/angular.json)
Build config targeting `bmc.client`, SCSS styling, port **12200** for dev server, proxy to BMC.Server on **12101**.

#### [NEW] [tsconfig.json](file:///d:/source/repos/scheduler/BMC/BMC.Client/tsconfig.json)
#### [NEW] [tsconfig.app.json](file:///d:/source/repos/scheduler/BMC/BMC.Client/tsconfig.app.json)

---

### Source Files (`src/`)

#### [NEW] [main.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/main.ts)
Bootstrap with `BASE_URL` provider.

#### [NEW] [index.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/index.html)
Premium loading screen with brick-orange glassmorphic theme, BMC branding.

#### [NEW] [styles.scss](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/styles.scss)
Global styles: BMC color palette (warm amber/orange accents), dark sidebar, custom scrollbars, component themes.

#### [NEW] [proxy.conf.js](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/proxy.conf.js)
Proxy `/api`, `/swagger`, `/connect`, `/.well-known` → `https://localhost:12101`.

---

### Core App Files (`src/app/`)

#### [NEW] [app.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.module.ts)
Root NgModule importing BrowserModule, RouterModule, HttpClient, FormsModule, NgBootstrap, plus generated BMC data service/component modules.

#### [NEW] [app-routing.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app-routing.module.ts)
Routes: `/login`, `/dashboard` (default), `/parts` (catalog), `/projects`, `/project/:id` (builder), `/system-health`, `/not-found`.

#### [NEW] [app.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.component.ts)
Root component with auth state, toast notifications, mobile detection.

#### [NEW] [app.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.component.html)
Layout: header + sidebar + router-outlet, with mobile-responsive variant.

#### [NEW] [app.component.scss](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.component.scss)

---

### Foundation Services (`src/app/services/`)

These are adapted from Scheduler.Client's proven implementations:

- [NEW] `auth.service.ts` — OIDC authentication
- [NEW] `alert.service.ts` — Toast/dialog notifications
- [NEW] `configuration.service.ts` — App config
- [NEW] `current-user.service.ts` — User state
- [NEW] `oidc-helper.service.ts` — Token management
- [NEW] `secure-endpoint-base.service.ts` — Authenticated HTTP base
- [NEW] `local-store-manager.service.ts` — Local storage
- [NEW] `app-title.service.ts` — Page titles
- [NEW] `auth-guard.ts` — Route protection
- [NEW] `jwt-helper.ts` — JWT decoding
- [NEW] `utilities.ts` — Common utilities

---

### UI Components (`src/app/components/`)

#### [NEW] Header Component
Nav bar with BMC branding, user menu, logout.

#### [NEW] Sidebar Component  
Collapsible nav: Dashboard, Parts Catalog, Projects, System Health.

#### [NEW] Login Component
OIDC login form matching Foundation pattern.

#### [NEW] Dashboard Component
Landing page with project quick-access, parts stats, recent activity.

#### [NEW] Not Found Component

---

### Environment Files

#### [NEW] `src/environments/environment.ts`
#### [NEW] `src/environments/environment.prod.ts`

## Verification Plan

### Automated Tests
1. `npm install` completes without errors
2. `npm run build` compiles successfully
3. `ng serve --port 12200` starts dev server

### Manual Verification
- Login page renders with BMC branding
- Authenticated routes redirect to login
- Sidebar navigation works between routes
- Generated data services connect to BMC.Server API
