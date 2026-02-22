# 3D Thumbnail Fixes: Orientation & Per-Part Colour

## Problem

1. **Upside-down**: LDraw files use Y-down convention; our camera assumes Y-up → all parts render inverted
2. **Same colour**: All parts render in default LDraw material colour; we want each part's most common real-world colour

## Proposed Changes

### Fix 1: Orientation

#### [MODIFY] [ldraw-thumbnail.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/ldraw-thumbnail.service.ts)

In `renderSinglePart()` (line ~400), rotate the loaded group by π radians on X-axis **before** framing:

```diff
 this.scene.add(group);
+group.rotation.x = Math.PI;  // LDraw Y-down → flip to Y-up
 this.frameModel(group);
```

> [!NOTE]
> This invalidates all previously-cached thumbnails in IndexedDB. We'll bump the DB name from `bmc-thumbnails-v2` → `bmc-thumbnails-v3` so old entries are ignored and re-rendered with correct orientation + colours.

---

### Fix 2: Most Common Colour

#### [MODIFY] [PartsCatalogController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartsCatalogController.cs)

In `GetAllCatalogParts()`:
- Add `mostCommonColourHex` to `CatalogPartDto`
- Pre-compute via: `LegoSetParts GROUP BY (brickPartId, brickColourId) → COUNT → join BrickColour.hexRgb → take top-1 per part`

#### [MODIFY] [parts-catalog-api.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/parts-catalog-api.service.ts)

Add `mostCommonColourHex: string | null` to `CatalogPartItem` interface.

#### [MODIFY] [parts-catalog.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.ts)

When calling `thumbnailService.renderBatch()`, map parts to include `colourHex`:

```typescript
const batch = visibleBatch.map(p => ({
    geometryFilePath: p.geometryFilePath,
    colourHex: p.mostCommonColourHex || undefined
}));
this.thumbnailService.renderBatch(batch);
```

Also update the `thumbnails` Map key to include colour (the service's `cacheKey` already does this).

## Verification

- `dotnet build` server — 0 errors
- `ng build --configuration production` client — bundle generated
- Visual check: parts should appear right-side-up, in varied colours per category
