# MOC Viewer — Model Rendering & Manual Generation

Expand the MOC viewer with two features adapted from existing components:

1. **Model Rendering** — server-side rendering with configurable size, angle, format, background, and PBR options (adapted from `catalog-part-detail` render tab)
2. **Manual Generation** — build instruction generation via SignalR with step progress, HTML/PDF preview, and download (adapted from `manual-generator`)

Both features reuse existing server infrastructure — no new rendering engines or SignalR hubs are needed. The key difference from the existing components is that the MOC viewer already has the project data on the server, so we bypass file uploads entirely.

## Proposed Changes

### Server — MocExportController

#### [MODIFY] [MocExportController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/MocExportController.cs)

Add a new endpoint for server-side rendering of the full assembled MOC model:

**`POST /api/moc/export/{projectId}/render`** — Generates the viewer MPD, then calls `RenderService.RenderToPng` (or WebP/SVG/GIF variants) with the same render options as `PartRendererController.RenderUpload`. Returns the rendered image blob. Accepts render config as query params (width, height, elevation, azimuth, format, antiAlias, background, etc.).

---

### Server — ManualGeneratorController

#### [MODIFY] [ManualGeneratorController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/ManualGeneratorController.cs)

Add two project-based endpoints that mirror the upload-based ones:

1. **`POST /api/manual-generator/generate-project/{projectId}`** — Generates the MPD from the project's PlacedBrick entities (via `ModelExportService.GenerateViewerMpdAsync`), stores the lines in `PendingFiles[generationId]`, and returns `{ generationId }`. The client then connects via SignalR and invokes `GenerateManual(generationId, options)` exactly as with uploads.

2. **`POST /api/manual-generator/analyse-project/{projectId}`** — Same as `analyse-upload` but generates the MPD from the project instead of requiring a file upload.

Both endpoints require `ModelExportService` and `BMCContext` to be injected — constructor updated accordingly.

---

### Client — MOC Viewer Component

#### [MODIFY] [moc-viewer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.ts)

**New state variables:**
- `activeViewerTab: '3d' | 'render' | 'manual'` — tab switcher
- Render config: `renderWidth/Height`, `renderElevation/Azimuth`, `renderEdges`, `smoothShading`, `antiAliasMode`, `rendererType`, `outputFormat`, `backgroundHex`, `gradientTopHex/BottomHex`, `enablePbr`, `exposure`, `aperture`, `explodedView`, `explosionFactor`
- Render output: `rendering`, `renderError`, `renderTimeMs`, `renderedImageUrl`, `renderedFormat`
- Manual generation: `isAnalysing`, `analysis`, `manualOptions`, `isGenerating`, `generationProgress`, `generationTotal`, `currentPreview`, `generationError`, `generatedHtml`, `pages`, `currentPage`, `resultStats`, `pdfDownloadUrl`, `htmlDownloadUrl`
- Size/angle/background presets (same arrays as catalog-part-detail)

**New methods:**
- `renderModel()` — POST render request to `/api/moc/export/{id}/render` with blob response
- `downloadRender()` — download the rendered image
- `getCameraAngles()` / `applyPoseToRender()` — extract Three.js camera angles for server render
- `analyseProject()` — POST to `/api/manual-generator/analyse-project/{id}`
- `generateManual()` — POST to `/api/manual-generator/generate-project/{id}`, connect SignalR, start generation
- `cancelGeneration()` — disconnect SignalR
- Page navigation (`prevPage`, `nextPage`, `splitPages`)
- Download methods (`downloadHtml`, `downloadPdf`, `printManual`)

**New imports:** `ManualGeneratorSignalrService`, `DomSanitizer`, `Subscription`

#### [MODIFY] [moc-viewer.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.html)

Replace the current single-canvas layout with a **tabbed viewer area**:

- **Tab bar** in the header: 3D Viewer | Server Render | Build Manual
- **3D tab**: existing canvas + step controls (unchanged)
- **Render tab**: render config panel (size presets, angle presets, background presets, format/AA/PBR controls), render button, result preview with download
- **Manual tab**: options panel (page size, image size, camera, renderer), analyse button, generate button, SignalR progress with live step preview, paginated HTML result preview with download/print

The sidebar stays as-is on the 3D tab but could show contextual info on render/manual tabs.

#### [MODIFY] [moc-viewer.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.scss)

Add styles for:
- Tab bar (header tabs)
- Render config panel (preset grids, sliders, controls)
- Render result preview (image container, download bar, timing badge)
- Manual generation UI (options grid, progress bar, step preview, page carousel, download buttons)

## Verification Plan

### Automated Tests
- `dotnet build BMC.Server` — server compiles with new endpoints
- `ng build` (or TypeScript compile check) — client compiles with new state/methods/template

### Manual Verification
- Navigate to a MOC project viewer → verify 3D tab still works
- Switch to Render tab → configure options → click Render → verify rendered image appears
- Switch to Manual tab → click Generate → verify SignalR progress streams → verify HTML manual preview → download
