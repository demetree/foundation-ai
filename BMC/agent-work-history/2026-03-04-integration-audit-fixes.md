# BMC Integration Audit Fixes

**Date:** 2026-03-04

## Summary

Performed a comprehensive audit of the BMC integration suite (BrickLink, BrickEconomy, BrickOwl, BrickSet, Rebrickable, Brickberg) and implemented all fixes across Phases 1–6. Phase 7 (response caching) was deferred as it requires database schema design.

## Changes Made

### Base Class
- `FoundationCore/Security/BusinessLogic/SecureWebAPIController.cs` — Added two `ResolveTenantAsync` overloads (custom role + read privilege) to eliminate tenant-resolution boilerplate across all controllers

### Controllers (all rewritten with consistent patterns)
- `BMC.Server/Controllers/BrickEconomySyncController.cs` — ResolveTenantAsync, quota in finally blocks, Math.Clamp pageSize, generic error messages, new `key-health` endpoint
- `BMC.Server/Controllers/BrickOwlSyncController.cs` — ResolveTenantAsync, Math.Clamp, generic errors, new `key-health` + `settings` endpoints, updated syncDirection docs
- `BMC.Server/Controllers/BrickLinkSyncController.cs` — ResolveTenantAsync, Math.Clamp, generic errors, fixed missing StartAuditEventClock
- `BMC.Server/Controllers/BrickSetSyncController.cs` — ResolveTenantAsync, Math.Clamp, fixed missing StartAuditEventClock in HashHealth/GetQuota
- `BMC.Server/Controllers/RebrickableSyncController.cs` — ResolveTenantAsync, Math.Clamp, added `methodName` to TransactionDto
- `BMC.Server/Controllers/BrickbergController.cs` — ResolveTenantAsync, replaced `Task.Run` with local async functions + local variables (race condition fix), IncrementQuotaAsync in finally, new minifig endpoint

### Services
- `BMC.BrickEconomy/Sync/BrickEconomySyncService.cs` — Added `ValidateStoredKeyAsync` for key-health endpoint
- `BMC.BrickOwl/Sync/BrickOwlSyncService.cs` — Added `ValidateStoredKeyAsync` for key-health endpoint

### Queue Processor
- `BMC.Server/Services/RebrickableSyncQueueProcessor.cs` — Implemented `DispatchPartListAsync` write-back of rebrickableListId, full `DispatchPartListItemAsync` (Create/Update/Delete), full `DispatchLostPartAsync` (Create/Delete)

## Key Decisions

- **ResolveTenantAsync in base class**: Placed in `SecureWebAPIController` (FoundationCore) rather than a BMC-specific helper so it's reusable by any controller in the platform
- **Local async functions over Task.Run**: Brickberg controller replaced `Task.Run` lambdas with local async functions that capture results in local variables, then assign to response after `Task.WhenAll` — prevents race conditions on shared response object
- **MAX_PAGE_SIZE = 200**: Standardized across all transaction endpoints using `Math.Clamp` instead of just `Math.Max`
- **Generic error messages**: All `Problem($"...{ex.Message}")` replaced with generic client-facing messages to prevent internal info leakage
- **Phase 7 deferred**: Response caching requires new database tables (BrickLinkPriceCache, BrickEconomyValuationCache, BrickOwlAvailabilityCache) — saved for follow-up session

## Testing / Verification

- `dotnet build BMC.Server.csproj` — Build succeeded (exit code 0)
- No new warnings or errors introduced; all warnings are pre-existing (NU1608, SYSLIB0014, CS0168)
