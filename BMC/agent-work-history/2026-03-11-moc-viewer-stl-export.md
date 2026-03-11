# MOC Viewer — STL Export

**Date:** 2026-03-11

## Summary

Added STL file export to the MOC 3D viewer, allowing users to download their assembled LEGO MOC projects as STL files for 3D printing or CAD import. Leveraged the existing `RenderService.ExportToStl()` overload that accepts in-memory `string[]` lines, wiring it to the `MocExportController` which already generates MPD content from PlacedBrick entities via `ModelExportService`.

## Changes Made

- **BMC/BMC.Server/Controllers/MocExportController.cs** — Added `GET /api/moc/export/{projectId}/stl?format=binary|ascii` endpoint with lazy-init `RenderService`, `IConfiguration` injection (for `LDraw:DataPath`), 120-second timeout for large models, and full audit logging. Added `.stl` to the `GetExportFormats()` response.
- **BMC/BMC.Client/src/app/services/project.service.ts** — Added `getStlExportUrl()` method to build the STL export URL with format query parameter.
- **BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.ts** — Added `exportingStl` flag, `stlFormat` toggle state, and `exportStl()` method using authenticated blob download with spinner management.
- **BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.html** — Added "3D Print Export" sidebar section with binary/ASCII toggle pill buttons and export button with spinner state.
- **BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.scss** — Added styles for STL format toggle, active state, and disabled/loading state on the export button.

## Key Decisions

- **Server-side geometry resolution** rather than client-side Three.js STL export — ensures all LDraw sub-parts are fully resolved from the parts library, producing watertight meshes suitable for 3D printing.
- **Reused `GenerateViewerMpdAsync()`** to generate the MPD content, then split to `string[]` and passed to `RenderService.ExportToStl()` — avoids duplicating geometry generation logic.
- **Binary format as default** — significantly more compact than ASCII; ASCII option provided for debugging or CAD tools that prefer it.
- **120-second timeout** — full MOC assemblies with hundreds of parts can take significant time for geometry resolution.

## Testing / Verification

- Server build (`dotnet build BMC.Server`): **0 errors**, 116 warnings (all pre-existing).
- Manual testing recommended: navigate to a MOC project in the viewer, click "Export STL", and verify the downloaded `.stl` file opens correctly in a 3D viewer or slicer.
