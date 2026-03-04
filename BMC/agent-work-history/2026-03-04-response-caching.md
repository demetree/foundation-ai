# Phase 7: Response Caching for Brickberg Terminal

**Date:** 2026-03-04

## Summary

Implemented database-backed response caching for the Brickberg Terminal. Marketplace API responses from BrickLink, BrickEconomy, and BrickOwl are now cached with configurable per-source TTLs, reducing external API calls and improving load times on repeated lookups.

## Changes Made

### Database Layer
- `BmcDatabaseGenerator/BmcDatabaseGenerator.cs` — Added `MarketDataCache` table definition (source, itemType, itemNumber, condition key + JSON blob + TTL fields)
- `BmcDatabase/Database/MarketDataCache.cs` — New EF entity (re-scaffolded via EF Core Power Tools)
- `BmcDatabase/Database/BMCContext.cs` — Re-scaffolded with `MarketDataCache` DbSet and fluent config
- `BmcDatabase/Database/BMCContextCustom.cs` — Added `OnModelCreatingPartial` stub (config now handled by scaffolding)

### Service Layer
- `BMC.Server/Services/MarketDataCacheOptions.cs` — New config POCO bound to `appsettings.json → MarketDataCache` with per-source TTL defaults (BL: 240min, BE: 240min, BO: 60min)
- `BMC.Server/Services/MarketDataCacheService.cs` — New service with `GetOrFetchAsync<T>` (check-then-fetch with JSON serialization/upsert) and `PurgeExpiredAsync` for cleanup

### Controller & Config
- `BMC.Server/Controllers/BrickbergController.cs` — Injected `MarketDataCacheService`, wrapped all 5 API calls in `GetOrFetchAsync`
- `BMC.Server/Program.cs` — Added `Configure<MarketDataCacheOptions>` and `AddScoped<MarketDataCacheService>`
- `BMC.Server/appsettings.json` — Added `MarketDataCache` configuration section

### Bug Fix
- `BMC.Server/Controllers/BrickOwlSyncController.cs` — Fixed pre-existing `GetOrdersListAsync` → `GetOrderListAsync` method name typo

## Key Decisions

- **Single unified `MarketDataCache` table** rather than per-source tables — simpler queries, maintenance, and extensibility
- **Not multi-tenant** — same set pricing data is shared across all users
- **JSON blob storage** — API responses are complex nested objects; normalizing would add unnecessary schema complexity
- **Cache write failures are swallowed** — cache is a performance optimization, never a correctness requirement
- **Per-source configurable TTLs** via `appsettings.json` — tuneable without redeployment (per user feedback)

## Testing / Verification

- `dotnet build BMC.Server.csproj` — Build succeeded (exit code 0)
- Re-scaffolded via EF Core Power Tools — entity and context generated cleanly
- No new warnings or errors introduced
