# Anonymous Access for BMC

## Phase 1 — Server-Side Anonymous Endpoints
- [x] Create `PublicBrowseController` extending `ControllerBase` with `[AllowAnonymous]`
- [x] Mirror read-only endpoints from `PartsCatalogController` (catalog, categories, part-types, part-colours, set-appearances)
- [x] Expose read-only set explorer data (search/filter/paginate sets)
- [x] Expose read endpoints for set detail, theme explorer, theme detail, minifig gallery, minifig detail, parts universe, colour library
- [x] Use `IMemoryCache` with appropriate TTLs on all public endpoints
- [x] AuditEngine integration with user='Anonymous' for Foundation monitoring
- [x] Server build verification (0 errors)

## Phase 2 — Client-Side Routing & Layout
- [x] Create `PublicAccessGuard` that always returns true (no auth check)
- [x] Update `app-routing.module.ts` to use `PublicAccessGuard` for anonymous-accessible routes
- [x] Update `app.component.ts/html` to show header/sidebar for anonymous users browsing public routes (new `isOnPublicBrowsePage` logic)
- [x] Correctly split header visibility from auth status (Sign In vs Sign Out)
- [x] Client production build verification (0 errors)

## Phase 3 — Landing/Welcome Page Rework
- [ ] Update `PublicLandingComponent` to make feature cards navigate directly to public browse routes
- [ ] Add prominent "Sign In" CTA alongside direct browse navigation

## Phase 4 — Auth Nudge System
- [ ] Create `AuthNudgeModalComponent` with sign-up/sign-in prompt
- [ ] Create `AuthNudgeService` for triggering nudges from components
- [ ] Integrate nudge triggers in locked features (server render tab, Brickberg, manual generator)

## Phase 5 — Client Service Layer (Dual API Paths)
- [ ] Update services to conditionally call `/api/public/browse/...` or existing `/api/...` endpoints based on auth status
