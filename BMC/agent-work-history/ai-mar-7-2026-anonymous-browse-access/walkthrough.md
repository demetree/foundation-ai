# Anonymous Access — Phase 1 & 2 Walkthrough

## What Was Built

### Phase 1: Server-Side `PublicBrowseController`

Created [PublicBrowseController.cs](file:///d:/source/repos/scheduler/BMC/BMC.Server/Controllers/PublicBrowseController.cs) — a new controller extending `ControllerBase` (not `SecureWebAPIController`) with `[AllowAnonymous]`. This keeps anonymous endpoints completely outside the Foundation enterprise auth pipeline.

**16 anonymous endpoints** covering all browse features:

| Endpoint | Description |
|---|---|
| `GET /api/public/browse/catalog` | Paginated parts catalog with search/filter |
| `GET /api/public/browse/catalog/all` | Full parts dump for IndexedDB caching |
| `GET /api/public/browse/catalog/categories` | Part categories with counts |
| `GET /api/public/browse/catalog/part-types` | Part types with counts |
| `GET /api/public/browse/catalog/part-colours` | Top colours per part |
| `GET /api/public/browse/catalog/{partId}/set-appearances` | Sets a part appears in |
| `GET /api/public/browse/catalog/{partId}/detail` | Part detail + available colours |
| `GET /api/public/browse/sets` | Set Explorer (from `SetExplorerService` cache) |
| `GET /api/public/browse/sets/{id}` | Set detail with parts & minifigs |
| `GET /api/public/browse/themes` | Theme listing with set counts |
| `GET /api/public/browse/themes/{id}` | Theme detail with child themes & sets |
| `GET /api/public/browse/minifigs` | Minifig Gallery (from `MinifigGalleryService` cache) |
| `GET /api/public/browse/minifigs/{id}` | Minifig detail with sets |
| `GET /api/public/browse/colours` | Full colour library with finish info |
| `GET /api/public/browse/parts-universe` | Parts Universe visualization data |

**Key design decisions:**
- Uses `AuditEngine.Instance.CreateAuditEventAsync()` directly with `user="Anonymous"`, `module="BMC"`, `entity="PublicBrowse"` — maintaining full analytics visibility via Foundation monitoring
- `IMemoryCache` with 3 TTL tiers: 2min (detail pages), 10min (listings), 1hr (reference data like colours)
- Audit failures are caught and logged but never break the user experience

---

### Phase 2: Client-Side Routing & Layout

#### [public-access.guard.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/public-access.guard.ts)
Simple always-allow guard. Named explicitly for route configuration readability.

#### [app-routing.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app-routing.module.ts)
**12 routes** switched from `AuthGuard` → `PublicAccessGuard` with `data: { publicRoute: true }`:
- `parts`, `parts/:partId`, `colours`
- `lego`, `lego/sets`, `lego/sets/:id`, `lego/minifigs`, `lego/minifigs/:id`
- `lego/themes`, `lego/themes/:id`, `lego/parts-universe`, `lego/compare`

**Explicitly locked** routes remain on `AuthGuard`:
- `part-renderer`, `manual-generator`, `brickberg`, `ai`, all `my-*` routes, `integrations`, `dashboard`, `system-health`

#### [app.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.component.ts)
Added `isOnPublicBrowsePage` detection using `ActivatedRoute` snapshot data. Traverses child routes on every `NavigationEnd` to check for `data.publicRoute === true`.

#### [app.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.component.html)
- Header/sidebar visibility: `(isUserLoggedIn || isOnPublicBrowsePage) && !isOnLoginPage`
- Header receives the real `isUserLoggedIn` value → anonymous users see "Sign In", authenticated users see "Sign Out"
- Rebrickable status bubble remains authenticated-only

## Verification

| Check | Result |
|---|---|
| Server build (`dotnet build`) | ✅ 0 errors |
| Client build (`ng build --production`) | ✅ 0 errors |

## Remaining Work (Phases 3–5)
- Landing page rework (direct navigation to browse features)
- Auth nudge modal for locked features
- Client service layer dual API paths (`/api/public/browse/...` vs `/api/...`)
