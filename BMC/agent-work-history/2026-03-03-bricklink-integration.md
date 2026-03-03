# BrickLink Store API Integration

**Date:** 2026-03-03

## Summary

Built a complete BrickLink Store API integration into the BMC project following the established patterns from BMC.BrickSet and BMC.Rebrickable. BrickLink is the world's largest LEGO secondary marketplace and provides the only source for real secondary market pricing at the individual part level. The integration uses OAuth 1.0 authentication.

## Changes Made

- Created `BMC.BrickLink/BMC.BrickLink.csproj` — new .NET 10 class library
- Created `BMC.BrickLink/Api/OAuthHelper.cs` — OAuth 1.0 HMAC-SHA1 signature generation
- Created `BMC.BrickLink/Api/BrickLinkApiClient.cs` — HTTP client with 11 catalog/price guide endpoints
- Created `BMC.BrickLink/Api/BrickLinkApiException.cs` — exception class
- Created 8 response model files in `BMC.BrickLink/Api/Models/Responses/` (BrickLinkApiResponse, Item, PriceGuide, Color, Category, Subset, KnownColor, ItemMapping)
- Created `BMC/BMC.Server/Controllers/BrickLinkSyncController.cs` — 8 REST endpoints (connect, disconnect, status, token-health, price-guide, item, subsets, supersets)
- Modified `BMC/BMC.Server/BMC.Server.csproj` — added BMC.BrickLink project reference
- Modified `BMC/BMC.Server/Program.cs` — registered BrickLinkSyncController in controller whitelist
- Modified `BMC/BMC.Server/appsettings.json` — added BrickLink consumer key/secret config
- Created `BMC/BMC.Client/src/app/services/bricklink-sync.service.ts` — Angular service with typed DTOs
- Added `BMC.BrickLink` project to `Scheduler.sln`

## Key Decisions

- Used OAuth 1.0 HMAC-SHA1 signing (BrickLink's requirement) rather than simple API key auth
- Consumer key/secret stored in appsettings.json (app-level), token key/secret to be stored encrypted per-user in DB (matching BrickSetUserLink pattern)
- Controller endpoints return placeholder responses for data lookups since BrickLinkUserLink DB table doesn't exist yet — connect endpoint fully validates tokens
- Conservative 500ms throttle between requests with exponential backoff on 429 responses
- Catalog/price guide endpoints prioritized over inventory/order management (read-only enrichment first)

## Testing / Verification

- `dotnet build BMC.BrickLink/BMC.BrickLink.csproj` — ✅ 0 errors (20s)
- `dotnet build BMC/BMC.Server/BMC.Server.csproj` — ✅ 0 errors (41s)
- Live API testing pending BrickLink seller account activation (requires positive feedback to register for API access)

## Next Steps

- Register for BrickLink API access once seller account is activated
- Create BrickLinkUserLink database table via migration
- Wire up encrypted credential storage in controller
- Build BrickLinkSyncService (sync service bridging API client and database)
- Build BrickEconomy integration (next priority)
