# PBR UI Controls

## Server Plumbing
- [x] `RenderService.cs` — add `enablePbr`, `exposure`, `aperture` params
- [x] `PartRendererController.cs` — add query params + cache key
- [x] ManualGenerator hub — accept PBR options via SignalR DTO

## Client UI
- [x] `part-renderer` — fields + conditional UI + URL params
- [x] `catalog-part-detail` — fields + conditional UI + URL params
- [x] `manual-generator` — fields + conditional UI + SignalR DTO

## Verification
- [x] Server build — 0 errors
- [x] Client build — 0 errors (1 pre-existing CSS warning)
