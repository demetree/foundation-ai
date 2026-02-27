# Server-Side Render Tab — Walkthrough

## Summary

Added a **tabbed interface** to the catalog part detail page with two tabs:
- **3D Viewer** (default) — existing Three.js interactive viewer, now with **Pose Part** mode
- **Server Render** — full server-side image generation with all controls visible (no hidden "Advanced" drawer)

## Changes Made

### [catalog-part-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.ts)

- Added `DomSanitizer`, `Subject`, `takeUntil` imports
- **New state**: tab selection, pose mode, render config (size, angle, format, AA, renderer, background, exploded view), render output state, preset data arrays
- **Render methods**: `renderPart()` (routes to standard/exploded/turntable endpoints), `downloadRender()`, `batchExport()`
- **Pose mode**: `togglePoseMode()`, `getCameraAngles()` (extracts elevation/azimuth from OrbitControls), `applyPoseToRender()` (copies angles → switches tab)
- **Unified colour**: reads `selectedColour.ldrawColourCode` for the server API — no separate colour state needed
- Cleanup in `ngOnDestroy` for blob URL revocation and `destroy$` Subject

### [catalog-part-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.html)

- **Tab bar** above viewer area with "3D Viewer" and "Server Render" buttons
- 3D viewer tab: existing content + **Pose Part** button and **angle HUD** overlay showing live elevation/azimuth
- Server Render tab: two-column layout with:
  - **Config column** (4 sections): Colour, View Angle, Output (format/size/AA), Appearance (edges/shading/renderer/background/exploded)
  - **Preview column**: empty state → loading spinner → rendered image with download/batch export toolbar

### [catalog-part-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.scss)

- 650+ lines of new styles covering tab bar, pose mode, server render panel layout, config sections, preset buttons, background swatches, sliders, preview panel, download toolbar
- Responsive: server render columns stack vertically on viewports ≤ 1024px
- Fixed pre-existing empty ruleset lint warning on `.colour-picker-card`

## Build Verification

```
✅ Application bundle generation complete. [14.722 seconds]
   main.js   17.89 MB | styles.css 486.01 kB | scripts.js 283.57 kB
```

No TypeScript compilation errors.

## What to Test

1. Navigate to a part with geometry → verify 3D Viewer tab appears as default
2. Click **Server Render** tab → see the four config sections with all controls visible
3. Select a colour from the right-side Colour Preview panel → confirm it shows in the Server Render colour section
4. Choose settings and click **Render Part** → verify rendered image appears in preview
5. Test **Pose Part** mode: click button in 3D Viewer → rotate model → click "Render This View" → verify angles carry to render tab
6. Test **Download** and **Export All Sizes** buttons
7. Test GIF turntable (select GIF format → Render Part)
8. Resize to narrow viewport → verify config/preview stack vertically
