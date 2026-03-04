# Brickberg Terminal: Live Order Book & Price History Charts

**Date:** 2026-03-04

## Summary

Implemented two Bloomberg-inspired features for the Brickberg Terminal: a Live Order Book showing active-for-sale listings alongside sold data (Phase B), and a D3 price history chart rendering BrickEconomy price events (Phase A). Also completed cache metrics endpoint (Phase 10) earlier in the session.

## Changes Made

### Phase 10: Cache Metrics
- `BMC.Server/Services/MarketDataCacheService.cs` — Added `Interlocked` hit/miss/error counters + `GetStatsAsync()` method
- `BMC.Server/Controllers/BrickbergController.cs` — Added `GET /api/brickberg/cache-stats` endpoint

### Phase B: Live Order Book
- `BMC.Server/Controllers/BrickbergController.cs` — Added `stockGuideNew`/`stockGuideUsed` to DTOs; both set and minifig endpoints now fetch 4 BrickLink price guides in parallel (sold N/U + stock N/U) with cache keys `SN`/`SU`
- `BMC.Client/.../set-detail.component.ts` — Added `priceMatrixTab` toggle property
- `BMC.Client/.../set-detail.component.html` — Replaced static Price Matrix with Sold/For Sale pill-tabbed version
- `BMC.Client/.../set-detail.component.scss` — Added `.price-matrix-tabs` and `.pm-tab` styles

### Phase A: Price History Chart
- `BMC.Client/.../set-detail.component.ts` — Added `priceHistoryRef` ViewChild, `renderPriceHistory()` D3 chart method with gradient area fill, retail price reference line, data point dots
- `BMC.Client/.../set-detail.component.html` — Added `#priceHistoryChart` container in Brickberg Terminal
- `BMC.Client/.../set-detail.component.scss` — Added `.price-history-section` and `.price-history-chart` styles

## Key Decisions

- **Cache key convention:** Stock (for-sale) data uses `SN`/`SU` condition keys vs `N`/`U` for sold data, keeping all 4 entries independently cached and expirable
- **D3 over Chart.js:** Used D3 since the component already imports it for colour donut and category bar charts — no new dependency
- **Retail price reference line:** Dashed gold line on the price history chart gives immediate visual comparison against original retail
- **BrickEconomy data passthrough:** The `valuation` object already contains `price_events`, `retail_price`, and `annual_growth` — no backend changes needed for Phase A

## Testing / Verification

- `dotnet build BMC/BMC.Server/BMC.Server.csproj` → Build succeeded (exit code 0)
- `ng build --configuration production` → Build succeeded (exit code 0)
