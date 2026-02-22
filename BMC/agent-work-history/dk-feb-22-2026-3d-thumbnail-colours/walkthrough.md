# 3D Thumbnail Fixes — Walkthrough

## Summary

Fixed two issues with 3D part thumbnail rendering: parts were **upside-down** (LDraw Y-down vs Three.js Y-up), and all used the **same default colour** instead of their real-world most-common colour.

## Changes Made

### Fix 1: Orientation

#### [ldraw-thumbnail.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/ldraw-thumbnail.service.ts)

```diff
 this.scene.add(group);
+group.rotation.x = Math.PI;  // LDraw Y-down → flip to Y-up
 this.frameModel(group);
```

Also bumped IndexedDB store name `bmc-thumbnails-v2` → `bmc-thumbnails-v3` to invalidate old cached thumbnails.

---

### Fix 2: Per-Part Most Common Colour

#### [PartsCatalogController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartsCatalogController.cs)

- Added `mostCommonColourHex` to `CatalogPartDto`
- Pre-computes via 3-step process:
  1. `GROUP BY (brickPartId, brickColourId)` on `LegoSetParts` → count occurrences
  2. Pick top colour per part (highest count, tie-break by colourId)
  3. Look up `hexRgb` from `BrickColours` table

#### [parts-catalog-api.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/parts-catalog-api.service.ts)

Added `mostCommonColourHex: string | null` to `CatalogPartItem` interface.

#### [parts-catalog.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.ts)

- Maps visible parts to `ThumbnailRequest[]` with `colourHex: p.mostCommonColourHex`
- Added `thumbnailKey(part)` helper using `LDrawThumbnailService.cacheKey()` for colour-aware composite keys

#### [parts-catalog.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.html)

Updated all `thumbnails.get(part.geometryFilePath)` → `thumbnails.get(thumbnailKey(part))`.

## How It Works

The existing `LDrawThumbnailService` already supported colour overrides via `ThumbnailRequest.colourHex` and `applyColourOverride()`. The missing piece was providing the actual colour. Now the server computes the most frequently used colour for each part from `LegoSetPart` → `BrickColour.hexRgb`, and the client passes it through to the rendering pipeline.

## Verification

| Check | Result |
|-------|--------|
| Server build | ✅ 0 errors |
| Client build | ✅ Bundle generated in 26.3s |
