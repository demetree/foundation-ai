# BrickEconomy API Integration

**Date:** 2026-03-03

## Summary

Built a complete BrickEconomy API integration into the BMC project following the established patterns from BMC.BrickSet and BMC.BrickLink. BrickEconomy provides AI/ML-powered valuation data for LEGO sets and minifigures, including current market value, growth metrics, and future price forecasts. Uses simple API key auth via x-apikey header.

## Changes Made

- Created `BMC.BrickEconomy/BMC.BrickEconomy.csproj` — new .NET 10 class library
- Created `BMC.BrickEconomy/Api/BrickEconomyApiClient.cs` — HTTP client with 5 endpoints (set valuation, minifig valuation, collection sets, collection minifigs, sales ledger)
- Created `BMC.BrickEconomy/Api/BrickEconomyApiException.cs` — exception class
- Created 5 response model files in `BMC.BrickEconomy/Api/Models/Responses/` (BrickEconomyApiResponse, Set, Minifig, Collection, SalesLedger)
- Created `BMC/BMC.Server/Controllers/BrickEconomySyncController.cs` — 8 REST endpoints (connect, disconnect, status, set/minifig valuation, collection sets/minifigs, sales ledger)
- Modified `BMC/BMC.Server/BMC.Server.csproj` — added BMC.BrickEconomy project reference
- Modified `BMC/BMC.Server/Program.cs` — registered BrickEconomySyncController in controller whitelist
- Created `BMC/BMC.Client/src/app/services/brickeconomy-sync.service.ts` — Angular service with typed DTOs
- Added `BMC.BrickEconomy` project to `Scheduler.sln`
- Fixed lint error in both `bricklink-sync.service.ts` and `brickeconomy-sync.service.ts` — corrected `getAuthorizationHeaders()` to `GetAuthenticationHeaders()`

## Key Decisions

- Used 1000ms throttle between requests (more conservative than BrickLink's 500ms) because BrickEconomy's daily budget is only 100 requests
- API key auth via x-apikey header (much simpler than BrickLink's OAuth 1.0)
- Response models include forecast_value and annual_growth fields specific to BrickEconomy's AI predictions
- Controller endpoints return placeholder responses for data lookups pending BrickEconomyUserLink DB table creation
- Connect endpoint validates API key by fetching a known set (10294-1) as a test

## Testing / Verification

- `dotnet build BMC/BMC.Server/BMC.Server.csproj` — ✅ 0 errors (38s, includes both BrickLink and BrickEconomy)
- Live API testing pending BrickEconomy Premium membership acquisition

## Next Steps

- Acquire BrickEconomy Premium membership for API access
- Create BrickEconomyUserLink database table via migration
- Wire up encrypted API key storage in controller
- Build BrickEconomySyncService (sync service bridging API client and database)
