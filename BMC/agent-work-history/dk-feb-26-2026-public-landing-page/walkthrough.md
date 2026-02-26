# Public Landing Page — Phase 1 Walkthrough

## What Was Built

Phase 1 of the public landing page is complete. Anonymous visitors see a premium, fully-themed showcase at `/` with a live theme picker.

### Theme System Integration (New)

The landing page is now fully wired into the BMC theme system:

- **Every colour, border, shadow, gradient, and glow** uses `var(--bmc-*)` CSS custom properties from `_themes.scss`
- **"Try a Theme" picker** in the hero section shows all 10 themes with their color swatches and icons
- Clicking a theme instantly transforms the entire landing page — backgrounds, cards, buttons, text, gradients all update
- The theme choice persists in `localStorage` via `ThemeService`, so if a visitor tries a theme and then signs in, their choice carries over

### Server — `PublicShowcaseController.cs`
- **5 endpoints** at `/api/public/*`, all `[AllowAnonymous]`
- Extends `ControllerBase` (not `SecureWebAPIController`) — fully outside Foundation auth
- `MemoryCache` with staggered TTLs: 1hr (stats, featured), 15min (recent), 24hr (decades), uncached (random)

### Client — `PublicLandingComponent`
- **Hero**: Animated "BMC" logo bricks, rotating taglines, live animated counters, theme picker
- **Featured Sets**: Top 12 by part count
- **Random Discovery**: 6 random sets with "Shuffle"
- **Decades Strip**: 1940s–2020s
- **Features Showcase**: 6 gradient feature cards
- **CTA Footer**: Sign In + "Coming Soon" sign-up badge

### Integration
- Route `/` → `PublicLandingComponent` with `PublicOrRedirectGuard`
- Layout hides sidebar/header for full-bleed rendering
- `ThemeService` injected + `currentTheme$` subscription for active indicator

## Files Changed

| File | Action |
|------|--------|
| [PublicShowcaseController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PublicShowcaseController.cs) | NEW |
| [public-showcase.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/public-showcase.service.ts) | NEW |
| [public-or-redirect.guard.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/public-or-redirect.guard.ts) | NEW |
| [public-landing.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/public-landing/public-landing.component.ts) | NEW |
| [public-landing.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/public-landing/public-landing.component.html) | NEW |
| [public-landing.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/public-landing/public-landing.component.scss) | NEW |
| [app-routing.module.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app-routing.module.ts) | MODIFIED |
| [app.module.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app.module.ts) | MODIFIED |
| [app.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app.component.ts) | MODIFIED |

## Verification

- **Angular build**: ✅ Passes — all chunks generated, no errors
