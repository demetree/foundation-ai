# MOC 3D Viewer — Tasks

## Server
- [x] Add `GenerateViewerMpdAsync()` to `ModelExportService` (inlines custom parts)
- [x] Add `GET /api/moc/project/{id}/viewer-mpd` to `MocExportController`
- [x] Add `GET /api/moc/project/{id}/summary` to `MocExportController`
- [x] Add `GET /api/moc/project/{id}/thumbnail` to `MocExportController`
- [x] Build `BMC.Server` — 0 errors ✅

## Client
- [x] Create `moc-viewer` component (TS / HTML / SCSS)
- [x] Update `project.service.ts` with viewer + export methods
- [x] Add routes: `/my-projects`, `/my-projects/:id/viewer`
- [x] Register `MocViewerComponent` in `app.module.ts`
- [x] Build client — 0 errors ✅
