# Enhanced Brickberg Terminal Search

**Date:** 2026-03-09

## Summary

Overhauled the Brickberg Terminal dashboard search from a bare set-number-only input into a rich search experience with typeahead, visual results, recent lookup history, and navigation.

Also fixed the BrickOwl BOID lookup (missing `idType` + fragile `ExtractBoid`) and hero card button overflow.

## Changes Made

### BrickOwl Lookup Fix
- **[MODIFIED] `BMC.Server/Controllers/BrickbergController.cs`** — Added `"set_number"` as `idType` to `CatalogIdLookupAsync`; rewrote `ExtractBoid` with strong typing (`BrickOwlIdLookupResult.Boids[0]`) + JSON fallback; added `using BMC.BrickOwl.Api.Models.Responses`

### Hero Card Overflow Fix
- **[MODIFIED] `BMC.Client/src/app/components/set-detail/set-detail.component.scss`** — Added `flex-wrap: wrap` to `.external-links`, `overflow: hidden` to `.hero-section`

### Enhanced Brickberg Search (full rewrite of 3 files)
- **[MODIFIED] `BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.ts`** — Typeahead search filtering `SetExplorerApiService` cache by name/number, keyboard navigation (↑↓ Enter Esc), recent lookups with `localStorage` persistence (max 5), growth helpers, click-through to set detail
- **[MODIFIED] `BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.html`** — Suggestions dropdown with thumbnails, rich result card with set identity header + 3-column source-labeled market data, recent lookups chip strip, disconnected source states
- **[MODIFIED] `BMC.Client/src/app/components/brickberg-dashboard/brickberg-dashboard.component.scss`** — Glassmorphic suggestions dropdown, result card with source accent colors (BL blue, BE green, BO purple), recent chips, responsive breakpoints

## Key Decisions

- Used `SetExplorerApiService` cached data for client-side typeahead rather than a new server endpoint — zero latency, already loaded
- Set number matches prioritized over name matches in suggestions
- Recent lookups stored in `localStorage` (simpler than Foundation User Settings, no round-trip)
- Market data result card uses a 3-column grid with color-coded source headers for visual clarity

## Testing / Verification

- Angular client build: 0 TypeScript errors ✅
- .NET server build: 0 C# errors ✅
- Manual testing required: typeahead by name/number, result card display, recent lookups persistence, click-through navigation
