# Brickberg Terminal — Top-Level Financial Dashboard

**Date:** 2026-03-04

## Summary

Elevated the Brickberg Terminal from an embedded panel in set-detail to a **first-class BMC operating mode** with its own route (`/brickberg`), welcome page presence, and sidebar navigation. This is the "Bloomberg Terminal for LEGO" — a financial intelligence dashboard aggregating portfolio value, market movers, and integration health.

## Changes Made

### Backend — New Endpoints (`BrickbergController.cs`)

- **`GET /api/brickberg/portfolio`** — Cross-references owned sets (`UserSetOwnership`) with cached BrickEconomy valuations to produce total collection value, per-set breakdown, top gainers/losers. Uses only cached data.
- **`GET /api/brickberg/market-movers`** — Scans all cached BrickEconomy SET entries to find top 10 gainers, top 10 losers, and recently retired sets.
- **`GET /api/brickberg/status`** — Integration health check for BrickLink, BrickEconomy, and Brick Owl using each sync service's `GetSyncStatusAsync`.
- Added `System.Linq`, `System.Collections.Generic`, and `Microsoft.EntityFrameworkCore` usings.

### Frontend — New Dashboard Component

- **`brickberg-dashboard.component.ts`** — Fires 4 parallel HTTP requests on init (portfolio, market-movers, status, cache-stats). Includes quick set lookup with inline results.
- **`brickberg-dashboard.component.html`** — 5-panel Bloomberg-inspired layout: terminal header with status dots, quick lookup bar, portfolio summary, market movers (gainers/losers/retired), integration health, cache statistics.
- **`brickberg-dashboard.component.scss`** — Glassmorphism panels, monospace terminal fonts (JetBrains Mono), green/teal accent palette, shimmer loading skeletons, responsive grid.

### Navigation & Routing

- **`app-routing.module.ts`** — Added `{ path: 'brickberg', component: BrickbergDashboardComponent }` route.
- **`app.module.ts`** — Registered `BrickbergDashboardComponent` in declarations.
- **`sidebar.component.ts`** — Added Brickberg as first entry in TOOLS nav group.

### Welcome Page

- **`welcome.component.ts`** — Added 3rd pathway card ("Brickberg Terminal" / investor persona) with green/teal gradient + added Brickberg to featureLinks strip.
- **`welcome.component.html`** — Split pathway rendering: first 2 cards in 2-column grid, Brickberg as full-width horizontal card below (per user's design preference).
- **`welcome.component.scss`** — Added `.brickberg-gradient` theme and `.brickberg-featured` full-width horizontal card styles.

## Key Decisions

- **Full-width card below existing two** — User chose Brickberg as a prominent full-width card under the Explorer/Designer pathway cards, rather than a 3-column equal grid.
- **Cache-only endpoints** — All three new backend endpoints use only cached data from `MarketDataCache` table, making zero external API calls. This ensures fast response times and no quota consumption.
- **`dynamic` casting for anonymous type sorting** — Used `dynamic` cast in the portfolio endpoint to sort anonymous objects by growth percentage.

## Testing / Verification

- `dotnet build BMC.Server` — **0 errors**, exit code 0
- `ng build --configuration production` — **0 errors**, exit code 0
- Fixed SCSS import path (`../../styles/glass` → `../../assets/styles/glass`)
