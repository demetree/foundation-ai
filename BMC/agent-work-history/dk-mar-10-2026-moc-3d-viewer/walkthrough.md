# MOC 3D Viewer — Walkthrough

## What Was Built

An interactive 3D viewer for assembled MOC projects, powered by Three.js and the existing LDrawLoader infrastructure.

### Server — 3 New Endpoints

| Endpoint | Purpose |
|---|---|
| `GET /api/moc/project/{id}/viewer-mpd` | Self-contained MPD text (with inlined custom parts) for the 3D viewer |
| `GET /api/moc/project/{id}/summary` | Project metadata JSON (name, part count, steps, submodels, source format) |
| `GET /api/moc/project/{id}/thumbnail` | Stored thumbnail PNG from the project entity |

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/ModelExportService.cs)

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/MocExportController.cs)

---

### Client — New `moc-viewer` Component

- [moc-viewer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.ts) — Three.js scene, LDrawLoader MPD parsing, build step navigation, export
- [moc-viewer.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.html) — Canvas, loading/error overlays, step slider, sidebar with info + export
- [moc-viewer.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.scss) — Premium dark theme

**Key approach:** Fetches the entire assembled model as a single MPD text from the server, then passes it to `LDrawLoader.parse()` — Three.js handles all part placement. Standard LDraw library parts are resolved via `api/ldraw/file/` with IndexedDB caching. Custom parts from `.io` files are inlined directly in the MPD.

### Service & Routing Updates

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/project.service.ts)

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app-routing.module.ts)

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app.module.ts)

---

## Verification

| Check | Result |
|---|---|
| `dotnet build BMC.Server` | ✅ 0 errors |
| `npx ng build` | ✅ Bundle generation complete (30.9s) |

## Routes Available

- `/my-projects` — Project listing (grid/list view, upload, delete)
- `/my-projects/:id/viewer` — 3D MOC viewer with build step navigation and export
