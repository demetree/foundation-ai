# Server-Side Render Tab — Task Checklist

## TypeScript (catalog-part-detail.component.ts)
- [x] Add new state fields (tab, pose mode, render config, render state, presets)
- [x] Add render methods (renderPart, renderTurntable, downloadRender, batchExport)
- [x] Add pose mode methods (togglePoseMode, getCameraAngles, applyPoseToRender)
- [x] Add preset helper methods (selectRenderSize, selectRenderAngle, applyBgPreset)
- [x] Add effectiveAntiAlias and sizeDisplayLabel getters

## HTML Template (catalog-part-detail.component.html)
- [x] Add tab bar above viewer card
- [x] Wrap existing 3D viewer content in tab conditional
- [x] Add Pose Part button to 3D viewer header
- [x] Add Server Render tab panel with controls + preview layout
  - [x] Colour section (reuses existing colour picker)
  - [x] View section (angle presets + flip toggle)
  - [x] Output section (format, size presets, anti-aliasing)
  - [x] Appearance section (edges, shading, renderer, backgrounds, exploded view)
  - [x] Render button
  - [x] Preview panel (image, loading, error, toolbar with download/export)

## SCSS Styling (catalog-part-detail.component.scss)
- [x] Tab bar styles
- [x] Server render two-column layout
- [x] Config panel section styles
- [x] Preview panel styles
- [x] Pose mode HUD styles
- [x] Responsive breakpoints

## Verification
- [x] Build compiles without errors
- [ ] Visual check in browser
