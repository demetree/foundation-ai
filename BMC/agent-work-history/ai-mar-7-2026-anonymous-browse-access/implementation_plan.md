# Anonymous Access for BMC Browse Features

Allow unauthenticated visitors to browse LEGO Universe, Parts Catalog, Part Details (3D viewer only), Set Explorer, Set Detail, Theme Explorer, Theme Detail, Minifig Gallery, Minifig Detail, Colour Library, and Set Comparison — without logging in.

## User Review Required

> [!IMPORTANT]
> **Server-side approach**: The existing custom controllers (`PartsCatalogController`, `CollectionController`, etc.) all extend `SecureWebAPIController`, which bakes Foundation-level auth into every endpoint. Rather than refactoring that base class, I'll create a **single new `PublicBrowseController`** (following the `PublicShowcaseController` pattern) that extends `ControllerBase` + `[AllowAnonymous]` and provides read-only mirrors of the needed data. This avoids touching the Foundation pipeline.

> [!WARNING]
> **Server-side rendering stays locked**: The `PartRendererController` is compute-heavy and will remain behind auth. Anonymous users will see the 3D viewer (client-side Three.js) but not the Server Render tab.

---

## Proposed Changes

### Phase 1 — Server: Public Browse Controller

#### [NEW] [PublicBrowseController.cs](file:///d:/source/repos/scheduler/BMC/BMC.Server/Controllers/PublicBrowseController.cs)

A single `[AllowAnonymous]` controller with cached, read-only endpoints:

| Endpoint | Mirrors from | Purpose |
|----------|-------------|---------|
| `GET /api/public/catalog` | `PartsCatalogController.GetCatalogParts` | Paginated parts list |
| `GET /api/public/catalog/all` | `PartsCatalogController.GetAllCatalogParts` | Full parts dump for IndexedDB |
| `GET /api/public/catalog/categories` | `PartsCatalogController.GetCatalogCategories` | Sidebar category list |
| `GET /api/public/catalog/part-types` | `PartsCatalogController.GetCatalogPartTypes` | Sidebar part type list |
| `GET /api/public/catalog/part-colours` | `PartsCatalogController.GetPartColours` | Part colour mapping |
| `GET /api/public/catalog/{partId}/set-appearances` | `PartsCatalogController.GetSetAppearances` | Set appearances for a part |
| `GET /api/public/catalog/{partId}/colours` | `CollectionController` equivalent | Colour options for a part |
| `GET /api/public/catalog/{partId}/detail` | New query | Part detail data for anonymous |
| `GET /api/public/sets` | `SetExplorerService` cache | Paginated set search |
| `GET /api/public/sets/{id}` | DB query | Set detail (parts, minifigs, images) |
| `GET /api/public/themes` | DB query | Theme listing |
| `GET /api/public/themes/{id}` | DB query | Theme detail |
| `GET /api/public/minifigs` | DB query | Minifig gallery |
| `GET /api/public/minifigs/{id}` | DB query | Minifig detail |
| `GET /api/public/colours` | DB query | Colour library |
| `GET /api/public/parts-universe` | `PartsUniverseService` | Parts universe stats |

All endpoints use `IMemoryCache` with 2-10 minute TTLs. No audit trail for anonymous requests (no `StartAuditEventClock`/`CreateAuditEventAsync`).

---

### Phase 2 — Client: Routing & Layout

#### [NEW] [public-access.guard.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/public-access.guard.ts)

A simple guard that always returns `true` — no auth check. Used on browse routes. Naming is for clarity of intent.

#### [MODIFY] [app-routing.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app-routing.module.ts)

Replace `canActivate: [AuthGuard]` with `canActivate: [PublicAccessGuard]` on these routes:

- `parts`, `parts/:partId` — Parts Catalog + Part Detail
- `colours` — Colour Library
- `lego` — LEGO Universe
- `lego/sets`, `lego/sets/:id` — Set Explorer + Set Detail
- `lego/minifigs`, `lego/minifigs/:id` — Minifig Gallery + Minifig Detail
- `lego/themes`, `lego/themes/:id` — Theme Explorer + Theme Detail
- `lego/parts-universe` — Parts Universe
- `lego/compare` — Set Comparison

All other routes keep `AuthGuard` (My Sets, My Parts, Brickberg, AI, integrations, profile, admin tables, etc).

#### [MODIFY] [app.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.component.ts)

- Change `isOnLoginPage` to a broader `isFullscreenPage` flag that includes `/login` and `/` (landing page only).
- Add `isOnPublicBrowsePage` flag that checks if the current URL matches a public browse route.
- Show sidebar and header when **either** `isUserLoggedIn` **or** `isOnPublicBrowsePage` is true.

#### [MODIFY] [app.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.component.html)

- Update `*ngIf` conditions to show sidebar/header when `isUserLoggedIn || isOnPublicBrowsePage`.
- Sidebar condition: `(isUserLoggedIn || isOnPublicBrowsePage) && !isFullscreenPage`.

#### [MODIFY] [sidebar.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/sidebar/sidebar.component.ts)

- Inject `AuthService`.
- Group nav items into "public" (Universe, Catalog, Colours, etc.) and "authenticated" (My Sets, Brickberg, AI, etc.).
- Show locked items with a lock icon and click → auth nudge modal.

#### [MODIFY] [header.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/header/header.component.ts)

- When user is not logged in, show "Sign In" button instead of user menu.

---

### Phase 3 — Landing Page Rework

#### [MODIFY] [public-landing.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/public-landing/public-landing.component.ts)

- Feature cards (Parts Catalog, Set Explorer, etc.) now have `route` fields that navigate directly to the feature instead of routing through login.
- `goToSetDetail()` navigates directly to `/lego/sets/{id}` instead of `/login?returnUrl=...`.
- Add a "Sign In" link in the header area for users who want to log in.

#### [MODIFY] [public-landing.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/public-landing/public-landing.component.html)

- Feature cards become clickable links to the features.
- "Get Started" CTA changes to "Start Exploring" and navigates to `/lego`.
- Add a smaller "Sign In" button for returning users.

---

### Phase 4 — Auth Nudge System

#### [NEW] [auth-nudge-modal directory](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/auth-nudge-modal/)

A reusable modal component (`AuthNudgeModalComponent`) with:
- Tasteful glass-effect modal matching BMC theme system
- Configurable message (e.g. "Sign up to save sets to your collection!")
- "Sign Up" primary CTA → `/login` (or future registration page)
- "Continue Browsing" dismiss button
- `AuthNudgeService` for triggering the modal from any component

#### [MODIFY] Multiple components — add nudge triggers

- `set-detail.component.ts` — "Add to My Sets" button shows nudge for anonymous
- `catalog-part-detail.component.ts` — Server Render tab shows nudge for anonymous
- `sidebar.component.ts` — Locked nav items show nudge on click

---

### Phase 5 — Client Service Layer Changes

#### [MODIFY] [parts-catalog-api.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/) (and similar services)

For services that serve public browse pages, add conditional URL selection:
- If `authService.isLoggedIn` → call `/api/parts-catalog/...` (authenticated, with audit trail)
- If not logged in → call `/api/public/catalog/...` (anonymous, no auth header)

This keeps the existing authenticated endpoints untouched while adding a fallback for anonymous browsing.

---

## Verification Plan

### Build Verification
1. **Server build**: `dotnet build BMC/BMC.Server/BMC.Server.csproj` — must compile cleanly
2. **Client build**: `cd BMC/BMC.Client && npx ng build` — must compile cleanly with zero TypeScript errors

### Manual Verification (request user help)

The following flows should be tested with the server running locally:

1. **Anonymous browse flow**: Clear all cookies/tokens → Navigate to `http://localhost:...` → See landing page → Click a feature card (e.g. "Parts Catalog") → Navigate directly to catalog without login → Browse parts, click a part → See 3D viewer but NOT Server Render tab
2. **Set exploration**: From landing page → Navigate to `/lego` → See Universe page → Click "Sets" → Browse Set Explorer → Click a set → See Set Detail with parts/images
3. **Auth nudge**: While browsing anonymously → Click "My Sets" in sidebar → See auth nudge modal → Click "Sign In" → Arrive at login page
4. **Sign-in flow**: From any anonymous page → Click "Sign In" in header → Log in → Redirect back to where you were browsing
5. **Logged-in user unchanged**: Log in → Everything works exactly as before with no regressions
