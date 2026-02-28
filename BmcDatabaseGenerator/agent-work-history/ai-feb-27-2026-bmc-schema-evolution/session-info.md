# Session Information

- **Conversation ID:** 99f363f0-49c8-47fb-9006-e47c1518fc0a
- **Date:** 2026-02-28
- **Time:** 00:16 NST (UTC-03:30)
- **Duration:** ~4 hours

## Summary

Evolved the BMC database schema (Rebrickable-primary identity + deeper CAD/model schema), fixed all resulting build breakage across server and client, implemented auto-bootstrap import into `DataImportWorker`, and fixed two colour import unique constraint violations.

## Files Modified

### Schema Evolution & Build Fixes (Phase 1)
- `BmcDatabaseGenerator/BmcDatabaseGenerator.cs` — 20 schema items: 8 modified tables, 12 new tables
- `BMC.LDraw/Import/LDrawImportService.cs` — Nullable dict keys, geometry rename, rebrickablePartNum, rebrickableColorId, name deduplication
- `BMC.Rebrickable/Import/RebrickableImportService.cs` — 10 nullable filtering fixes, rebrickableColorId
- `BMC/BMC.Server/Controllers/PartsCatalogController.cs` — geometry rename + nullable ldrawColourCode
- `BMC/BMC.Server/Services/PartsUniverseService.cs` — geometry rename
- `BMC/BMC.Server/Controllers/CollectionController.cs` — DTO + projection rename
- `BMC/BMC.Server/Controllers/ManualGeneratorHub.cs` — nullable ldrawColourCode
- 11 Angular client files — global rename geometryFilePath → geometryOriginalFileName

### Auto-Bootstrap Import (Phase 2)
- `BMC/BMC.Server/appsettings.json` — Enabled=true, AutoBootstrap=true
- `BMC/BMC.Server/Program.cs` — unconditional worker registration
- `BMC/BMC.Server/Services/DataImportWorker.cs` — auto-bootstrap on empty DB, IsDatabaseEmptyAsync

## Related Sessions

- Continues design discussion from earlier in this same conversation
