# Server-Side Render Tab for Catalog Part Detail

Add a **tabbed interface** to catalog-part-detail: "3D Viewer" (existing Three.js) and "Server Render" (server-side image generation). All part-renderer features surfaced in a cleaner, flat UI—no hidden "Advanced" drawer.

## Unified Colour Picker

`BrickColourData` already has both `ldrawColourCode` and `hexRgb`, so the existing `partColours[]` / `allColours[]` arrays serve **both tabs**. When the user selects a colour:
- **3D Viewer tab**: uses `hexRgb` → `applyColourToScene()`
- **Server Render tab**: uses `ldrawColourCode` → sent as `colourCode` query param to the render API

One picker, one selection state (`selectedColour`), two consumers.

## UI Design

### Tab Bar (above viewer area)

```
┌────────────────────────┬───────────────────────┐
│  🎮  3D Viewer         │  📷  Server Render    │
└────────────────────────┴───────────────────────┘
```

### 3D Viewer Tab Changes

- **Pose Part button** in viewer header: disables auto-rotate, shows elevation/azimuth HUD
- **"Render This View" button**: copies angles → switches to Server Render tab

### Server Render Tab — Four Visible Sections

```
┌─────────────────────────┬──────────────────────────────┐
│  CONTROLS               │  PREVIEW                     │
│                         │                              │
│  ── Colour ──────────── │  ┌────────────────────────┐  │
│  [Part] [All]           │  │                        │  │
│  🔴🟢🔵⬛⬜🟡…          │  │   (rendered image)     │  │
│  Red · LDraw 4          │  │                        │  │
│                         │  └────────────────────────┘  │
│  ── View ────────────── │                              │
│  [Std][Front][Top]…     │  ⏱ 342ms · 512×512          │
│  ☑ Flip 180°            │  [⬇ Download] [📦 Batch]    │
│                         │                              │
│  ── Output ──────────── │  ── Build Steps ──────────── │
│  Format: [PNG]…[GIF]    │  ◀  Step 3/12  ▶            │
│  Size:   [512²]…        │  ══════●══════════           │
│  AA:     [Off][2×][4×]  │                              │
│                         │                              │
│  ── Appearance ──────── │                              │
│  ☑ Edge lines           │                              │
│  ☑ Smooth shading       │                              │
│  Engine: [Rast][Ray]    │                              │
│  BG: 🌙🌅🌊🌲…         │                              │
│  ☑ Exploded view  1.0×  │                              │
│                         │                              │
│  [ 📷  Render Part ]    │                              │
└─────────────────────────┴──────────────────────────────┘
```

> [!IMPORTANT]
> All controls are visible in labelled sections. Nothing is hidden behind collapsible "Advanced" drawers.

## Proposed Changes

### Catalog Part Detail Component

#### [MODIFY] [catalog-part-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.ts)

**New state fields:**
- `activeViewerTab: '3d' | 'render' = '3d'`
- `poseMode = false`
- Render config: `renderWidth/Height`, `renderElevation/Azimuth`, `flipView`, `renderEdges`, `smoothShading`, `antiAliasMode`, `rendererType`, `outputFormat`, `webpQuality`, `backgroundHex`, `gradientTopHex/BottomHex`, `explodedView`, `explosionFactor`
- Render state: `rendering`, `renderError`, `renderTimeMs`, `renderedImageUrl`, `renderedBlobUrl`, `renderedFormat`
- Build steps: `stepCount`, `currentStep`, `stepMode`, `loadingSteps`
- Batch: `batchExporting`
- Preset data: `sizePresets`, `sizeCategories`, `sizeCategory`, `anglePresets`, `bgPresets`

**New methods:**
- `renderPart()` — calls `/api/part-renderer/render` (routes to turntable/step variants as needed)
- `renderTurntable()` — animated GIF endpoint
- `renderStep()`, `prevStep()`, `nextStep()`, `jumpToStep()`
- `downloadRender()`, `batchExport()`
- `loadStepCount()` — fetch step count for `this.part.name`
- `selectRenderSize()`, `selectRenderAngle()`, `applyBgPreset()`
- `getCameraAngles()` — derive elevation/azimuth from OrbitControls
- `applyPoseToRender()` — copy camera angles to render config, switch to render tab
- `togglePoseMode()` — disable auto-rotate, show angle HUD

**Colour integration** — uses existing `selectedColour: BrickColourData` and reads `selectedColour.ldrawColourCode` for the render API `colourCode` param. No new colour state or API calls needed.

#### [MODIFY] [catalog-part-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.html)

- Add tab bar above `viewer-card`
- Wrap 3D viewer in `*ngIf="activeViewerTab === '3d'"`
- Add Pose Part button to 3D viewer header
- Add Server Render panel in `*ngIf="activeViewerTab === 'render'"` with controls + preview columns

#### [MODIFY] [catalog-part-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.scss)

- Tab bar, server render layout, config sections, preview panel styles
- Pose mode HUD styling
- Responsive: stack columns vertically on narrow viewports

## Features NOT Carried Over

- File upload / drag-and-drop (stays in standalone part-renderer only)
- Part search (redundant — part is already loaded from route)

## Verification Plan

### Manual Verification
1. Navigate to a part → verify 3D Viewer tab works as before
2. Switch to Server Render → select colour, render, verify image appears
3. Test all output formats (PNG, WebP, SVG, GIF turntable)
4. Test build steps navigation
5. Test Pose Part → angles carry over to render tab
6. Test download + batch export
7. Test responsive layout
