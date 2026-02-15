# Fix My Collection Part Colour Rendering

The 3D thumbnails in the My Collection grid all render as grey/white despite having colour information. The colour dots and labels render correctly, but the Three.js models are not tinted.

## Root Cause

The database `BrickColour.hexRgb` column stores hex values **with** the `#` prefix (e.g. `#A0A5A9`) — both the LDraw and Rebrickable importers do this. The client-side `LDrawThumbnailService` expects values **without** `#` (per its own comment on line 17) and prepends `#` itself:

```typescript
// applyColourOverride, line 441:
const colour = new THREE.Color(`#${colourHex}`);  // → ##A0A5A9 (invalid!)
```

`THREE.Color` silently fails with an invalid hex string, leaving the model with default grey materials.

## Proposed Changes

### Client — My Collection Component

#### [MODIFY] [my-collection.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/my-collection/my-collection.component.ts)

Add a `normalizeHex()` helper that strips a leading `#` if present. Use it in:
- `updateDisplayedParts()` when building `partsWithGeometry` (line 224)
- `getThumbnailKey()` (line 333)
- `getColourStyle()` (line 336) — use the raw value directly since it already has `#`

---

### Client — Thumbnail Service (defensive)

#### [MODIFY] [ldraw-thumbnail.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/ldraw-thumbnail.service.ts)

Add defensive `#`-stripping in `applyColourOverride()` (line 441) so the service handles both formats gracefully regardless of caller.

## Verification Plan

### Manual Verification
- Load the My Collection page in the browser
- Confirm 3D thumbnails now render with the correct colours (Red parts appear red, Black appear dark, etc.)
- Verify the colour dot badge still displays correctly
- Check that IndexedDB cache updates with new colour-keyed entries

> [!NOTE]
> Existing IndexedDB thumbnail cache entries will have been stored with stale keys (including `#`). After this fix, old entries won't match the new keys and will be re-rendered with correct colours. No manual cache clear needed — it's self-healing.
