# Minifig Gallery — CDK Virtual Scroll + IndexedDB Caching

## Server
- [x] Create `MinifigGalleryService.cs` — BackgroundService, derives year from LegoSetMinifig→LegoSet join
- [x] Create `MinifigGalleryController.cs` — GET /api/minifig-gallery
- [x] Register in `Program.cs`

## Client — API Service
- [x] Create `minifig-gallery-api.service.ts` — fetch + IndexedDB cache (24h)

## Client — Component Rewrite
- [x] Rewrite `minifig-gallery.component.ts` — load all, client-side filter/sort, CDK virtual scroll
- [x] Rewrite `minifig-gallery.component.html` — cdk-virtual-scroll-viewport, sort controls, no pagination
- [x] Update `minifig-gallery.component.scss` — viewport styles, remove pagination

## Verification
- [x] Server build — 0 CS errors
- [x] Client build — 0 TS errors, bundle generated
