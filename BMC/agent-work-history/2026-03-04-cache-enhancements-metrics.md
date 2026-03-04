# Cache Enhancements & Metrics

**Date:** 2026-03-04

## Summary

Implemented Phase 8 cache enhancements (background purge service, quota optimization, BOID caching) and Phase 10 cache metrics (hit/miss counters + API endpoint). Confirmed that Phases 9 (BrickSet UI) and 11 (Brickberg Terminal polish) were already complete from prior sessions.

## Changes Made

### New Files
- `BMC.Server/Services/MarketDataCachePurgeService.cs` — `BackgroundService` that purges expired `MarketDataCache` entries every 6h (configurable via `PurgeIntervalMinutes`)

### Modified Files
- `BMC.Server/Services/MarketDataCacheOptions.cs` — Added `PurgeIntervalMinutes` property (default: 360)
- `BMC.Server/Services/MarketDataCacheService.cs` — Added thread-safe `Interlocked` hit/miss/error counters and `GetStatsAsync()` method returning in-memory metrics + DB-level entry stats by source
- `BMC.Server/Controllers/BrickbergController.cs`:
  - Moved `IncrementQuotaAsync` inside `GetOrFetchAsync` fetch lambdas so quota only fires on cache misses (both set and minifig endpoints)
  - Wrapped `CatalogIdLookupAsync` in its own cache entry (`"BrickOwl", "BOID"`) to avoid redundant ID mapping API calls
  - Added `GET /api/brickberg/cache-stats` endpoint
- `BMC.Server/Program.cs` — Registered `MarketDataCachePurgeService` as hosted service

## Key Decisions

- **Quota inside fetch lambda:** Moving `IncrementQuotaAsync` into the async lambda ensures quota is only consumed when the API is actually called (cache miss), not on cache hits
- **Separate BOID cache entry:** BOID lookups cached under key `("BrickOwl", "BOID", setNumber, null)` to avoid the ID mapping API call on repeat views
- **Static Interlocked counters:** Cache metrics use `static long` fields with `Interlocked` for thread safety; counters reset on app restart (acceptable for monitoring purposes)
- **Purge interval configurable:** `PurgeIntervalMinutes` in `appsettings.json` under `MarketDataCache` section

## Testing / Verification

- `dotnet build BMC/BMC.Server/BMC.Server.csproj` → Build succeeded (exit code 0)
- Only pre-existing NU1608 Pomelo package warning present
