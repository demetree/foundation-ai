# Anonymous Access — Full Implementation Walkthrough

## Phase 1: Server-Side `PublicBrowseController`

[PublicBrowseController.cs](file:///d:/source/repos/scheduler/BMC/BMC.Server/Controllers/PublicBrowseController.cs) — 16 anonymous read-only endpoints under `/api/public/browse/...` using `[AllowAnonymous]`, `IMemoryCache` (3 TTL tiers), and `AuditEngine` with `user="Anonymous"`.

| Endpoint Group | Public Endpoints |
|---|---|
| Parts Catalog | `catalog`, `catalog/all`, `catalog/categories`, `catalog/part-types`, `catalog/part-colours`, `catalog/{id}/set-appearances`, `catalog/{id}/detail` |
| Sets | `sets`, `sets/{id}` |
| Themes | `themes`, `themes/{id}` |
| Minifigs | `minifigs`, `minifigs/{id}` |
| Other | `colours`, `parts-universe` |

---

## Phase 2: Client Routing & Layout

- [public-access.guard.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/public-access.guard.ts) — Always-allow guard for public routes
- [app-routing.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app-routing.module.ts) — 12 routes use `PublicAccessGuard` with `data: { publicRoute: true }`
- [app.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.component.ts) — `isOnPublicBrowsePage` detects public routes for layout
- [app.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.component.html) — Header/sidebar visible for anonymous browse; Sign In/Out button correct for auth state

---

## Phase 3: Landing Page Rework

[public-landing.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/public-landing/public-landing.component.ts) / [HTML](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/public-landing/public-landing.component.html)

- **Hero CTA**: "Browse Now" (primary) + "Sign In" (secondary)
- **Feature cards**: `route` + `requiresAuth` properties; public → "Explore", auth-only → 🔒 "Sign in required"
- **Set/discovery cards**: Navigate directly to `/lego/sets/:id`
- **Decade cards**: Navigate to Set Explorer
- **Footer CTA**: "Start Browsing" + "Sign In for Full Access"

---

## Phase 4: Auth Nudge System

- [auth-nudge.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/auth-nudge.service.ts) — Subject-based trigger; components call `nudgeService.nudge({ featureName, featureIcon })`
- [auth-nudge-modal.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/auth-nudge-modal/auth-nudge-modal.component.ts) — Subscribes to `nudge$`, shows modal only for non-authenticated users, redirects to login with `returnUrl`
- [SCSS](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/auth-nudge-modal/auth-nudge-modal.component.scss) — Glassmorphism design using BMC theme variables
- Registered in `app.module.ts`, placed in `app.component.html` for global availability

---

## Phase 5: Dual API Paths

Services automatically select the right endpoint based on auth status:

- [parts-catalog-api.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/parts-catalog-api.service.ts) — All 4 fetch methods (`catalog/all`, `categories`, `part-types`, `part-colours`) route to `/api/public/browse/...` for anonymous users
- [set-explorer-api.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/set-explorer-api.service.ts) — Routes to `/api/public/browse/sets` for anonymous users
- [parts-catalog.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.ts) — Profile preference calls guarded behind `isLoggedIn`
- [set-explorer.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.ts) — Ownership cache + theme preference calls guarded behind `isLoggedIn`

---

## Phase 5b: Anonymous Browse Redirect Fix

Browser testing revealed clicking "Browse Now" redirected to `/login`. Root cause: `SecureEndpointBase.handleError()` tried token refresh on 401 → failed for anonymous users → called `reLogin()` → login redirect.

**Fixes applied:**

- [secure-endpoint-base.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/secure-endpoint-base.service.ts) — Added early return for anonymous users in 401 handler (skip refresh/redirect)
- [lego-universe.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.ts) — Guarded `ownershipCache.ensureLoaded()` + `loadUserPreferredThemes()` behind `isLoggedIn`; replaced auth-only theme/minifig-count calls with public API alternatives
- [set-detail.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.ts) — Guarded `ownershipCache.ensureLoaded()` behind `isLoggedIn`
- [theme-detail.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.ts) — Guarded `ownershipCache.ensureLoaded()` behind `isLoggedIn`
- [minifig-gallery-api.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/minifig-gallery-api.service.ts) — Dual API path (`/api/public/browse/minifigs` for anonymous)
- [parts-universe.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/parts-universe.service.ts) — Dual API path (`/api/public/browse/parts-universe` for anonymous)

---

## Verification

| Check | Result |
|---|---|
| Server build (`dotnet build`) | ✅ 0 errors |
| Client build (`ng build --production`) | ✅ 0 errors |
| Browser: "Browse Now" → stays on `/lego` | ✅ No redirect to login |
| Browser: sidebar "Parts Catalog" → stays on `/parts` | ✅ No redirect to login |
| Browser: public API calls sent correctly | ✅ `/api/public/browse/{minifigs,themes,sets,parts-universe}` |

### Before fix (redirect to login):
![Before fix](C:/Users/demet/.gemini/antigravity/brain/3e900747-a8ff-406a-bea9-3fda3a7819aa/after_browse_click_1772902292804.png)

### After fix (stays on Universe page):
![After fix](C:/Users/demet/.gemini/antigravity/brain/3e900747-a8ff-406a-bea9-3fda3a7819aa/universe_page_empty_1772902848891.png)
