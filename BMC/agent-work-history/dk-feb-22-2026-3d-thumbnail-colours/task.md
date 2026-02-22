# 3D Thumbnail Fixes: Orientation & Per-Part Colour

- [x] Fix orientation: `group.rotation.x = Math.PI` in `ldraw-thumbnail.service.ts`
- [x] Bump IndexedDB store name `v2` → `v3`
- [x] Server: add `mostCommonColourHex` to `CatalogPartDto` + compute via LegoSetPart
- [x] Client: add field to `CatalogPartItem` interface
- [x] Client: map to `ThumbnailRequest` with `colourHex` in component
- [x] Client: update HTML cache key lookups to `thumbnailKey(part)`
- [x] Server build — 0 errors
- [x] Client build — bundle generated (26.3s)
