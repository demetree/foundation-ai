# Anonymous Access for BMC

## Phase 1 — Server-Side Anonymous Endpoints
- [x] `PublicBrowseController` — 16 read-only endpoints, `IMemoryCache`, AuditEngine integration

## Phase 2 — Client-Side Routing & Layout
- [x] `PublicAccessGuard`, 12 routes switched, `isOnPublicBrowsePage` detection, app shell layout

## Phase 3 — Landing/Welcome Page Rework
- [x] Hero CTA: "Browse Now" + "Sign In"; feature cards with routes + explore/lock indicators
- [x] Set/decade cards: direct navigation to public pages; CTA footer updated

## Phase 4 — Auth Nudge System
- [x] `AuthNudgeService` + `AuthNudgeModalComponent` — glassmorphism modal, registered globally

## Phase 5 — Client Service Layer (Dual API Paths)
- [x] `PartsCatalogApiService` — 4 methods conditionally use `/api/public/browse/...` for anon
- [x] `SetExplorerApiService` — uses `/api/public/browse/sets` for anon
- [x] Component auth calls (profile, ownership cache) guarded behind `isLoggedIn`
