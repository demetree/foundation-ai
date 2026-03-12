# MOC Viewer UX Tuning — Camera, Grid & Animated Playback

**Date:** 2026-03-12

## Summary

Major UX improvements to the MOC 3D viewer component, focusing on initial camera framing, grid behaviour, and an entirely new animated build step playback system with media-player-style transport controls.

## Changes Made

### Camera Auto-Fit
- **`moc-viewer.component.ts`** — Rewrote `centreAndFrameModel()`: replaced the `2.5×` distance multiplier with a `1.2×` padding factor and added aspect-ratio-aware FOV calculation so models fill the viewport on load regardless of window shape. Also sets `controls.minDistance` and `camera.near` dynamically.

### Grid Toggle & Shadow Plane
- **`moc-viewer.component.ts`** — Stored `GridHelper` and shadow plane as class references. Grid now repositions to the model's bounding box floor (not `y=-0.5`) so it acts as a ground plane instead of slicing through geometry. Added `ShadowMaterial` mesh for subtle grounding even when grid is hidden. Added `toggleGrid()` method and `showGrid` property.
- **`moc-viewer.component.html`** — Added grid toggle button (`fa-border-all`) in header actions bar with active/inactive class.
- **`moc-viewer.component.scss`** — Added `.toggle-off` opacity style for grid button.

### Animated Build Step Playback
- **`moc-viewer.component.ts`** — Complete playback system: `isPlaying`, `playbackSpeed`, `loopPlayback` state. Timer-based auto-advance with `togglePlayback()`, `stopPlayback()`, `firstStep()`, `lastStep()`, `setPlaybackSpeed()`, `toggleLoop()`. Drag-to-scrub with document-level `mousemove`/`mouseup` listeners. Hover tooltip showing step number on progress bar. All manual navigation auto-stops playback. Cleanup in `ngOnDestroy`.
- **`moc-viewer.component.html`** — Replaced basic step slider with two-row playback bar: gradient progress track with floating scrub tooltip, transport buttons (first/prev/play-pause/next/last), loop toggle, auto-rotate toggle, step label, and speed pills (`0.5s`/`1s`/`2s`) with "Speed" label and descriptive tooltips.
- **`moc-viewer.component.scss`** — Full styling for playback bar: `.step-progress-track` with hover/scrubbing states, `.scrub-tooltip` floating pill, `.btn-play` with pulse animation, `.btn-loop` accent highlight, `.speed-pill` toggle group, `.speed-label`, `.step-transport-group`/`.step-transport-divider` layout.

### Other Tweaks
- Defaulted `autoRotate` to `false` to avoid confusion with the play button.

## Key Decisions

- **Camera distance**: Aspect-ratio-aware calculation using both horizontal and vertical FOV ensures models fill the frame regardless of viewport shape.
- **Grid as floor**: Positioning at `bounding box bottom` rather than a fixed Y ensures the grid never slices through any model.
- **Shadow plane**: `ShadowMaterial` provides subtle visual grounding without obscuring the model, a non-intrusive alternative to the grid.
- **Speed pills labeled as durations**: "0.5s / 1s / 2s" with a "Speed" prefix and tooltips ("Fast — hold each step for ½ second") to avoid confusion with speed multipliers.
- **Manual nav stops playback**: Prevents timer and user input from fighting.
- **Auto-rotate defaults off**: Rotation on load was confusing alongside the new play button.

## Testing / Verification

- Angular production build verified clean (31s, zero TS/template errors)
- Only warning is a pre-existing Bootstrap CSS selector issue (unrelated)
- Manual testing recommended: model load framing, grid toggle, build step playback, scrub interaction, speed changes, loop toggle
