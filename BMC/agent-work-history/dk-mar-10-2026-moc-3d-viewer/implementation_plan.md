# MOC 3D Viewer — Implementation Plan

Build an interactive 3D viewer for assembled MOC projects, reusing the existing Three.js + `LDrawLoader` stack.

## Key Insight

`LDrawLoader` already understands MPD format with embedded subfiles. The server already generates MPD from `PlacedBrick` entities. We just need to:
1. Serve the MPD as **text** (not as a download) via a viewer endpoint
2. **Inline custom part geometry** as MPD `FILE` blocks so the model is self-contained
3. Build the client viewer component using patterns from `catalog-part-detail`

---

## Proposed Changes

### Server — Viewer Endpoint

---

#### [MODIFY] [MocExportController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/MocExportController.cs)

Add a new endpoint:
- `GET /api/moc/project/{projectId}/viewer-mpd` → returns `text/plain` MPD content (not a file download)
- Also add `GET /api/moc/project/{projectId}/summary` → returns project metadata (name, part count, step count, thumbnail URL)

---

#### [MODIFY] [ModelExportService.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/ModelExportService.cs)

Add `GenerateViewerMpdAsync()` method:
- Calls existing `GenerateLDrawContentAsync(projectId, tenantGuid, true)` to get the MPD
- Looks up the project's `ModelDocument` for stored custom parts (`sourceFileData` → `StudioIoParser` → `CustomParts`)
- Appends each custom part `.dat` file as an inline `0 FILE CustomParts/{name}` / `0 NOFILE` block
- Returns the complete self-contained MPD as a string

---

### Client — MOC Viewer Component

---

#### [NEW] `moc-viewer/moc-viewer.component.ts`
Path: `BMC.Client/src/app/components/moc-viewer/`

Three.js viewer for assembled projects:
- Fetches MPD text from `/api/moc/project/{id}/viewer-mpd`
- Passes it to `LDrawLoader.parse()` (Three.js handles all part placement from the MPD)
- `OrbitControls` for camera interaction
- Build step slider: `LDrawLoader` exposes build step groups — toggle visibility by step
- Project info panel: name, part count, step count
- Export buttons: `.ldr` / `.mpd` / `.io` download links

Reuses from `catalog-part-detail`:
- Scene setup (PerspectiveCamera, WebGLRenderer, lights, environment)
- OrbitControls configuration
- ResizeObserver for responsive canvas
- Animation loop pattern
- `LDrawFileCacheService` initialisation

---

#### [NEW] `moc-viewer/moc-viewer.component.html`
Viewer layout:
- Full-width 3D canvas with loading overlay
- Sidebar: project info, build step slider, export buttons
- Responsive: sidebar collapses on mobile

#### [NEW] `moc-viewer/moc-viewer.component.scss`
Premium styling matching `catalog-part-detail`'s dark viewer theme.

---

#### [MODIFY] [project.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/project.service.ts)

Add methods:
- `getViewerMpd(projectId: number)` → `GET /api/moc/project/{id}/viewer-mpd` → `Observable<string>`
- `getProjectSummary(projectId: number)` → `GET /api/moc/project/{id}/summary`
- `exportProject(projectId: number, format: string)` → triggers download

---

#### [MODIFY] `app-routing.module.ts`

Add routes:
- `{path: 'my-projects', component: MyProjectsComponent}` — wire up the orphaned listing
- `{path: 'my-projects/:projectId/viewer', component: MocViewerComponent}` — the 3D viewer

#### [MODIFY] `app.module.ts`

Register `MocViewerComponent`.

---

## Verification Plan

### Build Check
- `dotnet build BMC.Server` — 0 errors
- `npx ng build` — 0 errors

### Browser Test
1. Navigate to `/my-projects` — verify grid listing appears
2. Click a project card → navigate to `/my-projects/{id}/viewer`
3. Verify 3D model loads and renders in the viewer
4. Test orbit controls (rotate, zoom, pan)
5. Test build step slider (if steps exist)
6. Test export buttons download files in each format
