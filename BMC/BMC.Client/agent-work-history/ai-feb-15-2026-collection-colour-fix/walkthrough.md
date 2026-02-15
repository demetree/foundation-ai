# Fix My Collection Colour Rendering — Walkthrough

## Problem
3D thumbnails in the My Collection grid rendered as grey/white despite having colour data. Colour dots and labels displayed correctly.

## Root Cause
Database `BrickColour.hexRgb` stores values with `#` prefix (`#A0A5A9`). The client-side thumbnail service also prepends `#`, creating invalid `##A0A5A9` for Three.js. `THREE.Color` silently fails → default grey materials.

## Changes

### [my-collection.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/my-collection/my-collection.component.ts)

- Added `normalizeHex()` helper — strips leading `#` when mapping colour values to the thumbnail service
- Updated `getThumbnailKey()` and `updateDisplayedParts()` to use `normalizeHex()`
- Fixed `getColourStyle()` to handle values that already include `#`

render_diffs(file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/my-collection/my-collection.component.ts)

### [ldraw-thumbnail.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/ldraw-thumbnail.service.ts)

- Added defensive `#`-stripping in `applyColourOverride()` so it works with both `A0A5A9` and `#A0A5A9` formats

render_diffs(file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/ldraw-thumbnail.service.ts)

## Verification
- Angular build passes with exit code 0
- Stale IndexedDB cache entries (keyed with `#`) will miss on new keys (without `#`) and be re-rendered with correct colours automatically
