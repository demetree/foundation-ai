# BrickBerg Terminal — Auth Fix, BrickOwl Lookup, and Styling

**Date:** 2026-03-09

## Summary

Fixed three issues preventing the BrickBerg Terminal from functioning correctly: missing authentication on all API calls, broken BrickOwl BOID lookup, and hero card button overflow.

## Changes Made

### Authentication Fix
- **[NEW] `BMC.Client/src/app/services/brickberg-api.service.ts`** — New `BrickbergApiService` extending `SecureEndpointBase` with authenticated methods for all 5 Brickberg API endpoints (set market data, portfolio, market movers, integration status, cache stats)
- **[MODIFIED] `BMC.Client/src/app/components/set-detail/set-detail.component.ts`** — Replaced raw `HttpClient.get()` in `loadBrickbergData()` with `BrickbergApiService.getSetMarketData()`
- **[MODIFIED] `BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.ts`** — Replaced all 5 raw `HttpClient.get()` calls with authenticated `BrickbergApiService` methods

### BrickOwl BOID Lookup Fix
- **[MODIFIED] `BMC.Server/Controllers/BrickbergController.cs`** — Added `"set_number"` as `idType` parameter to `CatalogIdLookupAsync` call; rewrote `ExtractBoid` to use strong typing (`BrickOwlIdLookupResult.Boids[0]`) with JSON fallback, replacing fragile dynamic dispatch

### Hero Card Styling Fix
- **[MODIFIED] `BMC.Client/src/app/components/set-detail/set-detail.component.scss`** — Added `flex-wrap: wrap` to `.external-links` and `overflow: hidden` to `.hero-section`

## Key Decisions

- Created a dedicated `BrickbergApiService` rather than adding auth inline, matching the established pattern used by `SetExplorerApiService`, `PartsCatalogApiService`, etc.
- Kept `HttpClient` in the dashboard component for the `openSet()` navigation lookup (uses the generated data controller endpoint, not a Brickberg endpoint)
- Used strong typing + JSON fallback in `ExtractBoid` rather than pure dynamic dispatch, since the cache service may deserialize to different object shapes

## Testing / Verification

- Angular client build: 0 TypeScript errors ✅
- .NET server build: 0 C# errors ✅
- Manual testing required: navigate to set detail (e.g., 42096-1) to confirm BrickOwl data loads and buttons wrap; test sidebar Brickberg dashboard search
