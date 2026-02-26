# Public Landing Page — "The BMC Storefront"

Replace the login page with a public interactive showcase that gives prospective users a taste of the system. Think Steam's store page: browse publicly, own privately.

## Core Architectural Decision

> [!IMPORTANT]
> **All existing routes keep `AuthGuard`. No codegen exceptions.**
> New public-facing components live on new public routes, backed by a new anonymous controller with its own cached data pipeline. This is additive — no changes to the existing Foundation/enterprise layer.

## Proposed Changes

---

### Server — Public Data API

#### [NEW] PublicShowcaseController.cs

A new controller with `[AllowAnonymous]` providing read-only, heavily cached endpoints:

| Endpoint | Data | Cache Strategy |
|----------|------|----------------|
| `GET /api/public/stats` | Total sets, parts, minifigs, themes, colours | Static `MemoryCache`, 1hr TTL |
| `GET /api/public/featured-sets` | Top 12 "epic" sets by part count | `MemoryCache`, 1hr TTL |
| `GET /api/public/random-discovery` | 6 random sets/minifigs with images | Short TTL (5min) or on-demand |
| `GET /api/public/recent-sets` | Latest 12 sets added | `MemoryCache`, 15min TTL |
| `GET /api/public/decades` | Decade buckets with set/theme counts | Static, 24hr TTL |
| `GET /api/public/themes-hierarchy` | Theme tree for sunburst viz | Static, 24hr TTL |
| `GET /api/public/year-timeline` | Year → set count for timeline chart | Static, 24hr TTL |
| `GET /api/public/search?q=` | Cross-entity search (sets, minifigs, themes) — capped at 5 per type | Per-query, short TTL |
| `GET /api/public/render-part` | Render a single part image (curated subset only) | Render cache (existing `PartImageCache`) |

**Rate limiting**: Tighter limits than authenticated endpoints — e.g. 30 req/min per IP.

**No auth token required.** The proxy config routes these through without OIDC.

---

### Client — New Components

#### [NEW] `public-landing` component
**Route:** `/` (replaces `redirectTo: 'dashboard'` for anonymous users)

This is a brand-new component inspired by `lego-universe` but purpose-built for the public:

**Sections (top to bottom):**

1. **Hero** — Full-viewport animated hero with BMC branding, floating brick particles, rotating taglines. Prominent **"Sign Up Free"** and **"Explore as Guest"** CTAs
2. **Live Stats Ticker** — Animated counters (79K parts, 20K sets, etc.) — pulled from `/api/public/stats`
3. **Featured Sets Showcase** — Horizontally scrollable cards of the most epic sets with images
4. **Interactive Part Renderer Sandbox** — Embedded lightweight version of the part renderer. Pre-loaded with an iconic part (e.g. 2×4 brick). User can pick colours and rotate. **This is the "wow" moment.**
5. **Random Discovery** — "Surprise Me" grid with shuffle button
6. **Theme Sunburst** — Interactive D3 visualization of the theme hierarchy (600+ themes)
7. **Timeline** — Sets-by-year bar chart showing LEGO's history
8. **Feature Cards** — Grid showcasing what members get: Collection Tracking, Build Manual Generator, AI Assistant, Part Renderer, MOC Publishing
9. **Social Proof / Community** — Stats about active users, shared manuals, community builds
10. **CTA Footer** — "Join BMC" sign-up prompt with the login form inline

**Key difference from `lego-universe`:**
- No `AuthGuard`, no auth service dependency
- Uses `PublicShowcaseService` (new) instead of authenticated API services
- Navigation links point to `/login` with a `?returnUrl=` param
- No "My Collection" / "My Universe" sections (those are auth-only)
- Includes sign-up CTAs throughout

#### [NEW] `public-landing.service.ts`
API service that calls `/api/public/*` endpoints. No auth headers. Simple `HttpClient` calls.

#### [NEW] `public-part-sandbox` component
A stripped-down but visually stunning version of the part renderer for the landing page:
- Pre-curated list of ~20 iconic parts (no search required, but search available)
- Colour picker with the most popular 24 colours — **interactive swatches with live preview**
- **Render mode toggle: Software → Ray Trace** — let visitors see the quality jump in real-time. The ray tracer is BMC's crown jewel and should be the "jaw drop" moment
- Pre-rendered gallery of the best ray-traced parts (hero-quality images that load instantly while the live renderer is working)
- Single render resolution (no presets clutter)
- Clean, minimal UI focused on the visual impact
- "Sign up for the full renderer with custom angles, resolutions, and turntable GIFs" CTA

---

### Client — Routing Changes

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app-routing.module.ts)

```diff
-{ path: '', redirectTo: 'dashboard', pathMatch: 'full' },
-{ path: 'login', component: LoginComponent, canActivate: [LoginRedirectGuard], title: 'Login' },
+{ path: '', component: PublicLandingComponent, canActivate: [PublicOrRedirectGuard], title: 'BMC — Brick Machine Construction' },
+{ path: 'login', component: LoginComponent, canActivate: [LoginRedirectGuard], title: 'Login' },
```

#### [NEW] `PublicOrRedirectGuard`
- If user is already logged in → redirect to `/dashboard`
- If anonymous → allow access to `PublicLandingComponent`

---

### Client — Layout Integration

#### [MODIFY] [app.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app.component.html) / [app.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app.component.ts)

The landing page should render **outside** the standard app chrome (no sidebar, no header). Logic:
- If on `/` route and not authenticated → render `<router-outlet>` only (full-bleed landing)
- Otherwise → render normal app shell with header + sidebar

---

## Phased Delivery

### Phase 1 — Foundation (This session or next)
- [x] `PublicShowcaseController` with `/stats`, `/featured-sets`, `/recent-sets`, `/decades`
- [x] `PublicLandingComponent` with Hero + Stats + Featured Sets + CTA
- [x] `PublicOrRedirectGuard`
- [x] Route configuration
- [x] No-auth layout mode

### Phase 2 — Interactive Sandbox
- [ ] `/api/public/render-part` endpoint (rate-limited, curated part subset)
- [ ] `PublicPartSandbox` component (embedded renderer)
- [ ] Part colour picker UI

### Phase 3 — Discovery & Visualizations
- [ ] `/api/public/themes-hierarchy`, `/year-timeline`, `/random-discovery`
- [ ] D3 sunburst + timeline on landing page
- [ ] "Random Discovery" section with shuffle
- [ ] Universal search bar (anonymous, capped results)

### Phase 4 — Conversion & Polish
- [ ] Feature showcase cards
- [ ] Inline sign-up form at bottom
- [ ] Animated scroll-reveal transitions
- [ ] Dynamic OG meta per section anchors
- [ ] Performance testing under load

## Verification Plan

### Automated Tests
- `ng build --configuration production` compiles clean
- All existing authenticated routes still work (AuthGuard intact)
- Public endpoints return data without auth token
- Rate limiting works on public endpoints

### Manual Verification
- Browser test: navigate to `/` while logged out → see landing page
- Browser test: navigate to `/` while logged in → redirect to `/dashboard`
- Mobile: landing page renders beautifully on phone
- Share URL → OG preview shows rich preview card

> [!TIP]
> Phase 1 alone will dramatically improve the first-impression experience. The current login page is purely functional — the landing page will be the "wow" moment that converts visitors into users.
