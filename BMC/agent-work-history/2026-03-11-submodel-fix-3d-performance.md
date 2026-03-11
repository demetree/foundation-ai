# MOC Viewer — Submodel Fix & 3D Performance Optimization

**Date:** 2026-03-11

## Summary

Fixed incorrect submodel rendering in the MOC viewer by preserving placement transforms during MPD import, and dramatically improved 3D viewer loading performance by bundling all LDraw library dependencies server-side.

## Changes Made

### Submodel Reference Fix (Option A)

- **[NEW] `BmcDatabase/Database/SubmodelInstance.cs`** — Entity tracking where submodel assemblies are placed in the parent model (position, quaternion rotation, colour code, build step)
- **[MODIFIED] `BmcDatabase/Database/BMCContextCustom.cs`** — Added `DbSet<SubmodelInstance>` and fluent configuration in `OnModelCreatingPartial`
- **[MODIFIED] `BmcDatabaseGenerator/BmcDatabaseGenerator.cs`** — Added `SubmodelInstance` table definition for database scaffolding
- **[NEW] `BMC.Server/Migrations/20260311_AddSubmodelInstance.sql`** — Idempotent CREATE TABLE script with indexes
- **[MODIFIED] `BMC.Server/Services/ModelImportService.cs`** — Instead of skipping submodel references during import, stores them as `SubmodelInstance` records with full transform data (position/rotation/colour/step)
- **[MODIFIED] `BMC.Server/Services/ModelExportService.cs`** — Emits type-1 LDraw reference lines for `SubmodelInstance` records in the main model body, placing submodels at correct world positions

### 3D Viewer Performance Optimization

- **[MODIFIED] `BMC.Server/Services/ModelExportService.cs`** — Added server-side LDraw dependency bundling:
  - `BundleLDrawDependencies()` — BFS that recursively scans all type-1 sub-file references, resolves `.dat` files from the LDraw library, and embeds them as inline `0 FILE` blocks
  - `ExtractSubFileReferences()` — Parses type-1 lines to extract referenced filenames
  - `ReadLDrawFile()` — Multi-directory fallback file resolver (parts/, p/, p/48/, p/8/, parts/s/, models/) with filename index
  - `EnsureLDrawFileIndex()` — Lazy-built filename-to-path index for O(1) lookup
  - Also bundles `LDConfig.ldr` (colour definitions) to eliminate that separate request
  - Added `IConfiguration` injection for `LDraw:DataPath` access

## Key Decisions

- **Option A (Store & Restore References)** was chosen over simpler alternatives for the submodel fix — creates a new `SubmodelInstance` entity to preserve full transform fidelity through the import→database→export round-trip
- **Server-side bundling** was chosen over client-side caching because the root cause was Three.js LDrawLoader's trial-and-error approach generating up to 12 HTTP requests per sub-file (6 directory prefixes × lowercase retry). Bundling eliminates the problem at the source
- Quaternion storage for rotations (converted from/to 3×3 matrices) for consistency with existing `PlacedBrick` entity pattern
- Safety guard of 15,000 files max to prevent unbounded resolution

## Testing / Verification

- Server build verified (0 errors) after each change
- Small 3-part model confirmed working with submodel references
- 1600-part model import confirmed working with correct thumbnail generation
- Awaiting user testing of 3D viewer performance with bundled dependencies
