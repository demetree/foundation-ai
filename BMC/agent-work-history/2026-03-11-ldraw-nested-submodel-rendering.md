# LDraw Model Rendering & Nested Submodel Hierarchy Fix

**Date:** 2026-03-11

## Summary

Fixed broken 3D rendering of LDraw models in the MOC viewer and added nested submodel hierarchy support to the data model. A 1600-part Porsche 911 RSR (42096) now renders correctly with all parts at proper positions.

## Changes Made

### Client-Side (Angular)
- **`BMC.Client/src/app/components/moc-viewer/moc-viewer.component.ts`** — Rewrote model loading to use `loader.load(url)` instead of manual `parse()` + monkey-patching. Added a thin `fetchData` wrapper for graceful missing-file handling (returns empty geometry instead of killing the model).

### Server-Side (C#)
- **`BMC.Server/Services/ModelExportService.cs`** — Three fixes:
  1. Excluded submodel bricks from the main model query to eliminate duplication
  2. Added "serve original MPD" shortcut — returns stored source file when available, preserving correct hierarchy
  3. Updated export to filter `SubmodelInstance` by `parentSubmodelId` for correct nested reconstruction

- **`BMC.Server/Services/ModelImportService.cs`** — Set `parentSubmodelId = owningSubmodel?.id` when creating `SubmodelInstance` entities during import, preserving which parent submodel each instance belongs to.

### Data Model
- **`BmcDatabaseGenerator/BmcDatabaseGenerator.cs`** — Added `parentSubmodelId` nullable FK to `SubmodelInstance` table (points to `Submodel`, null = main model child).
- **`BmcDatabase/Database/SubmodelInstance.cs`** — Rescaffolded with `parentSubmodelId` property and `parentSubmodel` navigation property.

## Key Decisions

- **`parentSubmodelId` on `SubmodelInstance`** rather than restructuring the entire model: This is the minimal change needed to support arbitrarily deep nesting (e.g., `main → porsche → chassisbottom → bricks`). The `Submodel` table already had a self-referencing FK; only instance placement context was missing.
- **"Serve original MPD" shortcut**: Returns the imported source file for the viewer when available, bypassing reconstruction entirely. This guarantees correct hierarchy for imported models while the reconstructed path catches up via `parentSubmodelId`.
- **Graceful missing-file handling**: The `fetchData` wrapper catches errors from the LDrawLoader's 12-attempt trial-and-error pipeline and returns empty geometry. This prevents a single missing custom part from killing the entire model load.

## Testing / Verification

- **Simple 3-part model**: Renders correctly (flat face issue resolved by switching to `loader.load()`)
- **1600-part Porsche 911 RSR (42096)**: All parts render at correct positions — wheels on the ground, rear wing properly mounted, no duplication, no floating parts
- **Build verification**: All projects compile with 0 errors after changes
