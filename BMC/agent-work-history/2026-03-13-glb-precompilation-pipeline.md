# GLB Pre-Compilation Pipeline for 3D Viewer

**Date:** 2026-03-13

## Summary

Implemented a server-side GLB (binary glTF) pre-compilation pipeline that dramatically improves 3D model loading in the MOC viewer. Large LEGO sets like the Titanic (6,829 parts, 334 submodels) went from ~30+ seconds with the text-based LDrawLoader pipeline to ~11 seconds for first-time compilation, and sub-millisecond for cached subsequent loads.

## Changes Made

### New Files
- `BMC.LDraw.Render/GlbExporter.cs` — Converts `LDrawMesh` to GLB binary via SharpGLTF. Groups triangles by colour/finish for minimal draw calls, creates one glTF node per build step (with `extras.stepIndex` metadata), includes optional LINES-mode edge meshes, PBR materials tuned per LDraw finish type, and Y-axis flipped for glTF coordinate system.
- `BMC/BMC.Server/Services/GlbCacheService.cs` — Two-tier cache: RAM (ConcurrentDictionary with LRU eviction at 50 entries) → Database (`CompiledGlb` table) → Build-on-miss. Per-project `SemaphoreSlim` prevents duplicate concurrent builds. Version-based invalidation via `Project.versionNumber`.

### Modified Files
- `BMC.LDraw.Render/BMC.LDraw.Render.csproj` — Added `SharpGLTF.Toolkit` NuGet package
- `BMC.LDraw.Render/RenderService.cs` — Added `ExportToGlb()` methods (file-based + content-based with step boundaries)
- `BmcDatabaseGenerator/BmcDatabaseGenerator.cs` — Added `CompiledGlb` table for persistent GLB cache (user rescaffolded)
- `BMC/BMC.Server/Controllers/MocExportController.cs` — Added `GET /api/moc/project/{projectId}/viewer-glb?edgeLines=true` endpoint, wired to `GlbCacheService`
- `BMC/BMC.Server/Program.cs` — Registered `GlbCacheService` as scoped service
- `BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.ts` — GLB-first loading with `GLTFLoader`, LDrawLoader fallback, dual-strategy build step discovery (GLB `extras.stepIndex` + LDrawLoader `userData.buildingStep`)

## Key Decisions

- **GLB over optimized LDraw** — SharpGLTF produces a single binary file with indexed geometry, eliminating thousands of HTTP requests and all client-side text parsing
- **Separate mesh per step** — Each build step is a glTF node, enabling visibility toggling for step navigation without re-processing
- **Edge lines configurable** — Optional via `?edgeLines=true` query param, included as LINES-mode mesh primitives
- **Y-axis flipped during export** — LDraw Y-down → glTF Y-up conversion happens server-side, so no `rotation.x = Math.PI` needed for GLB path
- **Two-tier cache** — RAM for speed, database for persistence across restarts, with version-based invalidation
- **Graceful fallback** — If GLB endpoint fails (404, error), the viewer seamlessly falls back to the existing LDrawLoader pipeline
- **BmcDatabaseGenerator pattern** — Used code-based schema generator for `CompiledGlb` table instead of EF Core migrations

## Testing / Verification

- BMC.LDraw.Render builds with 0 errors
- BMC.Server builds with 0 errors
- Tested with LEGO 10294 Titanic (6,829 parts, 334 submodels) — loaded in 11 seconds via GLB, rendered correctly with proper colours, geometry, and edge lines
- Build step navigation functional via GLB node visibility toggling
