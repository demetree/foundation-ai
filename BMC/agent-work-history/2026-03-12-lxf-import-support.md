# LXF Import Support — LEGO Digital Designer File Format

**Date:** 2026-03-12

## Summary

Added full `.lxf` (LEGO Digital Designer) import support to BMC, end to end. Users can now upload their legacy LDD project files directly — no pre-conversion required. The implementation spans the LDraw parser library, server import/export services, and all four relevant client-side UI components.

## Changes Made

### BMC.LDraw — New Files
- `Models/LxfResult.cs` — Result DTO for parsed LXF data (LDraw lines, thumbnail, LDD version)
- `Parsers/LxfParser.cs` — Static parser that extracts LXFML XML from the ZIP, converts LDD bricks to LDraw Type 1 lines with ~85-entry materialID→colour code mapping, build step support from `<BuildingInstructions>`, and thumbnail extraction

### BMC.Server — Modified Files
- `Services/ModelImportService.cs` — Added `FORMAT_LXF` constant, `.lxf` handling branch in `ImportFromFileAsync`, LXF thumbnail passthrough via `lxfThumbnailData` parameter, and `GetMimeType` case
- `Controllers/MocImportController.cs` — Added `.lxf` to `SUPPORTED_EXTENSIONS`, formats list endpoint, and error messages
- `Services/ModelExportService.cs` — Added `FORMAT_LXF` constant and included LXF in the viewer MPD source document query

### BMC.Client — Modified Files
- `upload-model-modal.component.ts` — Added `.lxf` to `acceptedExtensions` and `getFormatName()` switch
- `upload-model-modal.component.html` — Added `.lxf` to file input `accept` attribute and format tags display
- `my-projects.component.ts` — Added `.lxf` case to `getFormatIcon()` (uses `fa-drafting-compass`)
- `my-projects.component.html` — Updated empty-state help text to include `.lxf`
- `part-renderer.component.ts` — Added `.lxf` to `acceptedExtensions` for upload rendering
- `manual-generator.component.ts` — Added `lxf` to `selectFile()` extension validation

## Key Decisions

- **Import-only scope** — No "Export to .lxf" feature; users re-export as `.ldr`/`.mpd`/`.io`
- **Static colour mapping** — LDD materialID→LDraw colour code mapping embedded as a ~85-entry dictionary in the parser rather than a database table, since LDD is a legacy format with a fixed colour set
- **Fallback for unmapped colours** — Passes the materialID through directly as a guess; the import pipeline's "unresolved colour" tracking catches any misses
- **Thumbnail reuse** — Extracts `IMAGE100.PNG` from the LXF ZIP if present; falls back to software renderer if not
- **Pattern follows StudioIoParser** — Same architectural approach as `.io` import (ZIP → extract → convert to LDraw lines → feed to existing pipeline)

## Testing / Verification

- **BMC.LDraw** — `dotnet build` — 0 errors ✅
- **BMC.Server** — `dotnet build` — 0 errors ✅
- **BMC.Client** — All changes are string-literal additions to arrays and switch cases (trivial, type-safe)
- **Manual testing** — User will verify end-to-end by uploading an actual `.lxf` file through the MOC import UI
