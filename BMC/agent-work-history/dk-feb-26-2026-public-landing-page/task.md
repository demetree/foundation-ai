# Public Landing Page — Task Tracker

## SEO Quick Wins
- [x] Open Graph + Twitter Card meta tags on `index.html`
- [x] `robots.txt` for crawler guidance
- [x] `manifest.webmanifest` for PWA
- [x] `og-preview.png` for link previews
- [x] Build verification

## Phase 1 — Foundation
- [x] **Server:** `PublicShowcaseController.cs` — 5 anonymous endpoints (stats, featured-sets, recent-sets, decades, random-discovery) with `MemoryCache`
- [x] **Client Service:** `public-showcase.service.ts` — Angular HTTP service using `@Inject('BASE_URL')`
- [x] **Guard:** `public-or-redirect.guard.ts` — redirects authenticated users to dashboard
- [x] **Component:** `PublicLandingComponent` — hero section, animated stats, featured sets, random discovery, decades strip, features showcase, CTA footer with "Coming Soon" sign-up badge
- [x] **Routing:** Root `/` serves `PublicLandingComponent` with `PublicOrRedirectGuard`
- [x] **Module:** Component declared in `app.module.ts`
- [x] **Layout:** `app.component.ts` extended to hide sidebar/header on `/` (full-bleed landing)
- [x] **Build verification:** Angular build passes

## Phase 2 — Interactive Sandbox (Future)
- [ ] `PublicPartSandbox` component with colour picker + render mode toggle
- [ ] Pre-rendered ray trace gallery
- [ ] Anonymous render endpoint on `PublicShowcaseController`

## Phase 3 — Discovery & Visualizations (Future)
- [ ] D3 sunburst + timeline charts
- [ ] Theme hierarchy browser
- [ ] Anonymous search endpoint

## Phase 4 — Conversion & Polish (Future)
- [ ] Sign-up flow (currently stub/"Coming Soon")
- [ ] Dynamic Open Graph meta tags per route
- [ ] Micro-animations + performance optimization
