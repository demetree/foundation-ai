# Manual Editor Integration, Import Enrichment & LPub3D Support

**Date:** 2026-03-14

## Summary

Fixed the broken Manual Editor ↔ MOC Viewer integration, enhanced the file import pipeline to auto-populate manual records with enriched data (smart camera, part links, step images), and added LPub3D meta-command parsing for importing manual layout data authored in LPub3D.

## Changes Made

### Session 1 — Manual Editor Integration Fix
- **`manual-editor.component.ts`** — Added `@Input() projectId` with `OnChanges` for embedded mode
- **`moc-viewer.component.html`** — Replaced ~75 lines of orphaned sidebar HTML with clean info/tips panel
- **`moc-viewer.component.ts`** — Removed unused `ManualEditorService` import and constructor injection
- **`moc-viewer.component.scss`** — Added sidebar help text styles

### Session 1 — Import Manual Auto-Population
- **`ModelImportService.cs`** — Full auto-population: pages grouped 3 per page, `BuildManualStep` records per main-model step with default isometric camera, `fadeStepEnabled=true`, submodel internal steps excluded

### Session 1 — Bug Fixes
- **`ModelImportService.cs`** — Manual names now project-specific to avoid unique constraint violation
- **`ProjectsController.cs`** — Same fix for "Add New Project" flow
- **`upload-model-modal.component.ts`** — `hasSuccessfulUpload` tracking for proper refresh on modal dismiss

### Session 2 — Step Data Enrichment
- **`ModelImportService.cs`** — Smart camera targets (cumulative centroid of all parts placed up to each step) and smart camera distance (bounding extent with padding)
- **`ModelImportService.cs`** — `BuildStepPart` junction records linking each `BuildManualStep` to its `PlacedBrick` entities
- **`ModelImportService.cs`** — Submodel filtering: `HashSet<string>` of submodel filenames excludes sub-assembly references from camera centroid calculations

### Session 2 — Step Image Rendering
- **`ModelImportService.cs`** — Added Step 8c: renders each build step at 256×256 with SSAA2x using `RenderService.RenderStep()`, stores PNG as base64 data URI in `BuildManualStep.renderImagePath`
- **`manual-editor.service.ts`** — Added `renderImagePath`, `pliImagePath`, `fadeStepEnabled` to `BuildManualStepDto`
- **`manual-editor.component.html`** — Step cards show `<img>` when image exists, cube icon placeholder otherwise
- **`manual-editor.component.scss`** — `.step-render-image` styling with `object-fit: contain`

### Session 2 — LPub3D Meta-Command Support (Tier 1)
- **`LDrawModel.cs`** — Extended `LDrawModel` with `PageWidthMm`, `PageHeightMm`, `Landscape`, `HasCoverPage`; extended `LDrawStep` with `RotStepX/Y/Z`, `RotStepType`, `FadePrevStep`, `IsPageBreak`
- **`ModelParser.cs`** — Added parsing in `ParseSingleModel()` for 6 meta-commands: `ROTSTEP` (with pending state carry-forward), `!LPUB PAGE SIZE`, `!LPUB PAGE ORIENTATION`, `!LPUB INSERT COVER_PAGE`, `!LPUB INSERT PAGE`, `!LPUB FADE_PREV_STEP`
- **`ModelImportService.cs`** — Maps parsed LPub3D metadata: page size overrides A4 defaults, ROTSTEP rotation → spherical camera offset, `FadePrevStep` → `fadeStepEnabled`, `IsPageBreak` → intelligent page grouping replacing fixed 3-per-page

### Session 3 — LPub3D Meta-Command Support (Tier 2 — DB Schema + Parsing + Mapping)

#### DB Schema (4 new columns)
- **`BmcDatabaseGenerator.cs`** — Added `backgroundColorHex` (nvarchar(9)) to `BuildManualPage`; added `isCallout` (bit), `calloutModelName` (nvarchar(250)), `showPartsListImage` (bit) to `BuildManualStep`
- **`BuildManualPage.cs`** — Added `backgroundColorHex` property
- **`BuildManualStep.cs`** — Added `isCallout`, `calloutModelName`, `showPartsListImage` properties
- **`BuildManualPageExtension.cs`** — Added `backgroundColorHex` to all 8 DTO mapping locations (DTO class, ToDTO, ToOutputDTO, FromDTO, ApplyDTO, Clone, CreateAnonymous, CreateAnonymousWithFirstLevelSubObjects)
- **`BuildManualStepExtension.cs`** — Added 3 new fields to all 8 DTO mapping locations; user also added `calloutModelName` to `CreateMinimalAnonymous`
- **`build-manual-page.service.ts`** — Added `backgroundColorHex` to QueryParameters, SubmitData, Data class, ConvertToSubmitData
- **`build-manual-step.service.ts`** — Added `isCallout`, `calloutModelName`, `showPartsListImage` to QueryParameters, SubmitData, Data class, ConvertToSubmitData
- **`manual-editor.service.ts`** — Added `backgroundTheme`, `layoutPreset`, `backgroundColorHex` to `BuildManualPageDto`; added `isCallout`, `calloutModelName`, `showPartsListImage` to `BuildManualStepDto`

#### Parser + Import Mapping (3 new meta-commands)
- **`LDrawModel.cs`** — Added `PageBackgroundColorHex` to `LDrawModel`; added `IsCallout`, `CalloutModelName`, `ShowPartsListImage` to `LDrawStep`
- **`ModelParser.cs`** — Parsing for `PAGE BACKGROUND COLOR`, `CALLOUT BEGIN/END` (with state-machine tracking and model name capture from first sub-file reference), `PLI PER_STEP ON/OFF`; callout state applied at all 3 step-commit points (STEP, ROTSTEP, end-of-file)
- **`ModelImportService.cs`** — Maps `PageBackgroundColorHex` → `BuildManualPage.backgroundColorHex`; maps `IsCallout`/`CalloutModelName`/`ShowPartsListImage` → `BuildManualStep` fields

#### Quick-Win LPub3D Meta-Commands
- **`LDrawModel.cs`** — Added `HasBillOfMaterials` flag
- **`ModelParser.cs`** — `INSERT BOM` sets flag; `END_OF_FILE` breaks parse loop

#### LDCad Meta-Command Stubs (future CAD editor)
- **`LDrawModel.cs`** — Added `LDCadGroup` class (GID, Name, TopLevel, LocalId, Center), `Groups` dictionary, `HasGeneratedFallback` flag
- **`LDrawSubfileReference.cs`** — Added `GroupLocalIds` list for GROUP_NXT membership tagging
- **`ModelParser.cs`** — Full parsing for `GROUP_DEF` (extracts all `[key=value]` params into `LDCadGroup`), `GROUP_NXT` (tags next sub-file reference with group IDs), `GENERATED` (sets flag and skips fallback geometry). Added `ExtractLDCadParam()` helper for `[key=value]` syntax.

## Key Decisions

- **3 steps per page** as default, overridden by `!LPUB INSERT PAGE` markers when present
- **Base64 data URIs** for step images — avoids file system management and new API endpoints; images are small (~10-30KB at 256×256)
- **ROTSTEP parsed in ModelParser** (not just GeometryParser) — so the import service can use author-specified camera angles for both stored camera values and step image rendering
- **Spherical-to-Cartesian conversion** for ROTSTEP angles — elevation (X) and azimuth (Y) converted to camera offset from centroid using sin/cos
- **Server-side rendering first, client-side later** — step previews as `<img>` tags for simplicity and PDF export compatibility; Three.js mini-viewports planned as future enhancement
- **Callout state machine** — `insideCallout` flag tracks CALLOUT BEGIN/END scope; first sub-file reference inside a callout is captured as the `calloutModelName`
- **`showPartsListImage` defaults to `true`** — PLI is shown unless explicitly disabled by `0 !LPUB PLI PER_STEP OFF`

## Testing / Verification

- Build verification via Visual Studio (terminal command runner had issues)
- User tested import of `test-step.ldr` and confirmed camera values are computed per-step
- LPub3D meta parsing is ready for testing with community LPub3D-authored files
- Tier 2 DB schema changes scaffolded by user via EF Core Power Tools

### Session 4 — LDCad Stubs, Quick-Win Metas, DB Fix, Phase 1 UI

#### DB Fix
- **`BmcDatabaseGenerator.cs`** — Changed `renderImagePath` and `pliImagePath` from `AddString250Field` to `AddTextField` (nvarchar(MAX)) to accommodate base64 data URIs

#### Quick-Win LPub3D Metas
- **`LDrawModel.cs`** — Added `HasBillOfMaterials` flag
- **`ModelParser.cs`** — `INSERT BOM` sets flag; `END_OF_FILE` breaks parse loop

#### LDCad Meta-Command Stubs (future CAD editor)
- **`LDrawModel.cs`** — Added `LDCadGroup` class (GID, Name, TopLevel, LocalId, Center), `Groups` dictionary, `HasGeneratedFallback`
- **`LDrawSubfileReference.cs`** — Added `GroupLocalIds` for GROUP_NXT membership tagging
- **`ModelParser.cs`** — GROUP_DEF, GROUP_NXT, GENERATED parsing + `ExtractLDCadParam()` helper

#### Phase 1 UI Wins (expose existing data in manual editor)
- **`manual-editor.component.html`** — Callout badge (blue), PLI-off badge (amber), page background colour, fade/PLI toggles, callout info section
- **`manual-editor.component.scss`** — `.callout`, `.pli-off` badge styles, `.callout-info` panel styles

### Session 5 — Phase 2: PLI Image Generation

#### RenderService.cs
- **`RenderPartThumbnail(fileName, colourCode, size)`** — renders single LDraw part files with dual PNG/RGBA per-instance cache
- **`RenderPliGrid(partEntries, cellSize)`** — composes part thumbnails into a grid with "×N" quantity labels via built-in 5×7 pixel font (DrawMiniText)

#### ModelImportService.cs
- **Step 8d** — groups each step's parts by `(FileName, ColourCode)`, renders PLI grid, stores as base64 data URI in `pliImagePath`

#### Manual Editor UI
- **`manual-editor.component.html`** — PLI image area below step render, gated by `pliImagePath && showPartsListImage`
- **`manual-editor.component.scss`** — `.step-pli-area`, `.pli-label`, `.pli-image` styles
