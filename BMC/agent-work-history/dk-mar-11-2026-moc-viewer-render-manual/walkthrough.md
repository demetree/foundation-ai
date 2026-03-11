# MOC Viewer — Model Rendering & Manual Generation

Two major features added to the MOC viewer: server-side model rendering with full configuration, and build manual generation via SignalR.

## Server Changes

### Model Rendering — MocExportController

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/MocExportController.cs)

New endpoint `GET /api/moc/export/{projectId}/render` generates MPD from PlacedBrick entities via `ModelExportService.GenerateViewerMpdAsync`, then renders using `RenderService`. Supports:
- **PNG/WebP/SVG/GIF** output formats
- Camera config (elevation, azimuth, flip)
- Anti-aliasing (none, 2x, 4x SSAA)
- Custom backgrounds (solid, gradient) 
- PBR ray tracing with exposure/aperture
- 120-second timeout for large models

### Manual Generation — ManualGeneratorController

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/ManualGeneratorController.cs)

Two project-based endpoints that bypass the file upload step:

| Endpoint | Purpose |
|----------|---------|
| `POST /api/manual-generator/analyse-project/{id}` | Step/part breakdown from project data |
| `POST /api/manual-generator/generate-project/{id}` | Stores MPD in `PendingFiles` → returns `generationId` for SignalR |

Both inject `ModelExportService` and `BMCContext` to generate MPD from PlacedBrick entities. The generation endpoint feeds directly into the existing `ManualGeneratorHub` SignalR pipeline.

## Client Changes

### TypeScript — [moc-viewer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.ts)

~420 new lines:
- **Tab system**: `activeViewerTab` (`'3d' | 'render' | 'manual'`)
- **Render config**: size/angle/background presets, format selection, AA, PBR, pose mode (camera → render angle extraction)
- **Render methods**: `renderModel()`, `renderTurntable()`, `downloadRender()`, `revokeRenderBlob()`
- **Manual generation**: `generateManual()` (POST → SignalR connect → stream progress), `analyseProject()`, `cancelGeneration()`, page navigation, download/print
- **SignalR wiring**: `initManualTab()` subscribes to `onStepProgress$`, `onComplete$`, `onError$`, `onConnectionChange$`

### HTML — [moc-viewer.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.html)

Tabbed layout with contextual sidebars:
- **3D tab**: Original canvas, step controls, export sidebar (unchanged)
- **Render tab**: Result preview with timing/download → sidebar has size presets, angle presets, background palette, format/AA/PBR controls, render button
- **Manual tab**: Progress bar with live step preview → page carousel with navigation → download bar (HTML/PDF/Print) → sidebar has generation options + analysis display

### SCSS — [moc-viewer.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.scss)

~600 new lines of theme-aware styling for all new UI elements.

## Verification

| Check | Result |
|-------|--------|
| `dotnet build BMC.Server` | ✅ 0 errors |
| `ng build` | ✅ 0 errors |
