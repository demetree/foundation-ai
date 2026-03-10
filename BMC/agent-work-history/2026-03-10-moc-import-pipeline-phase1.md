# MOC Import Pipeline — Phase 1 Implementation

**Date:** 2026-03-10

## Summary

Implemented the complete MOC import pipeline for BMC, enabling users to upload `.ldr`, `.mpd`, and `.io` (BrickLink Studio) model files and have them converted into BMC's native entity hierarchy.

## Changes Made

### BMC.LDraw (Parser Layer)
- **Created** `Models/StudioIoResult.cs` — Result model for `.io` file extraction (LDraw lines, thumbnail, Studio version, part count, instruction XML)
- **Created** `Parsers/StudioIoParser.cs` — Extracts LDraw content from `.io` ZIP archives with fallback entry name matching, thumbnail extraction, and `.INFO` metadata parsing

### BMC.Server (Service + API Layer)
- **Created** `Services/ModelImportService.cs` (~530 lines) — Core conversion engine:
  - Dual-layer storage: import-fidelity (`ModelDocument` → `ModelSubFile` → `ModelBuildStep` → `ModelStepPart`) + native (`Project` → `PlacedBrick` + `Submodel` + `SubmodelPlacedBrick`)
  - Quaternion decomposition from 3x3 rotation matrices (Shepperd method)
  - Database-backed part/colour resolution with pre-loaded caches (`BrickPart.ldrawPartId`, `BrickColour.ldrawColourCode`)
  - Part filename resolution fallbacks (exact → strip `.dat` → strip path prefix)
  - LDraw special colour code handling (16=current, 24=edge → default)
  - MPD submodel detection and hierarchy creation
  - Unresolved part/colour tracking without blocking import
- **Created** `Controllers/MocImportController.cs` — Two endpoints:
  - `POST /api/moc/import/upload` — File upload with format validation, tenant scoping, audit logging
  - `GET /api/moc/import/formats` — Returns supported format metadata
- **Modified** `Program.cs` — Added `ModelImportService` DI registration and `MocImportController` to controller list

### BmcDatabaseGenerator
- **Modified** `BmcDatabaseGenerator.cs`:
  - Added `userId` (nullable int) to `Project` table — cross-database reference to `SecurityUser.id`
  - Added `ExportFormat` seed data: BrickLink Studio (`.io`) and STL (`.stl`)

## Key Decisions

- **Dual-layer entity storage**: Raw imported file data preserved in `ModelDocument` chain for lossless round-trip re-export, while also creating native `PlacedBrick` entities for the design canvas. This avoids forcing a choice between fidelity and usability.
- **Pre-loaded lookup caches**: Part and colour resolution loads all mappings into memory once per import rather than querying per-part, making large model imports (1000+ parts) efficient.
- **`userId` as nullable int (not FK)**: The `Project` table is in the BMC database while `SecurityUser` is in the Security database. Cross-database FKs aren't supported by SQL Server, so we use a plain int with a comment documenting the relationship.
- **Graceful unresolved handling**: Parts or colours not found in the BMC catalog don't block import — they're logged, tracked in the response, and preserved in the import-fidelity layer for later resolution.

## Testing / Verification

- `dotnet build BMC.LDraw/BMC.LDraw.csproj` — ✅ 0 errors
- `dotnet build BMC/BMC.Server/BMC.Server.csproj` — ✅ 0 errors (before and after rescaffold)
- `dotnet build BmcDatabaseGenerator/BmcDatabaseGenerator.csproj` — ✅ 0 errors
- Verified `Project.userId` (nullable int) appeared correctly in regenerated entity after EF Core Power Tools rescaffold
