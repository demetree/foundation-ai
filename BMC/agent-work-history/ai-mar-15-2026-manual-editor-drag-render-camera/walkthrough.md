# Manual Editor — Features 2–4 Walkthrough

## Summary

Implemented three new features for the Manual Editor (Feature 1, PDF/HTML Export, was completed in the prior session):

| Feature | Status | Key Files |
|---|---|---|
| **Drag-and-Drop Reorder** | ✅ Built | Controller, service, component TS/HTML/SCSS |
| **Re-render on Camera Change** | ✅ Built | Controller endpoint, service, component handler |
| **Three.js Orbit Camera** | ✅ Built | New `step-camera-editor` component |

---

## Feature 2: Drag-and-Drop Step Reordering

**Server**: `PUT /api/manual-generator/page/{pageId}/reorder-steps` — accepts `{ stepIds: int[], targetPageId?: int }`, updates `stepNumber` and optionally moves steps between pages.

**Client**: 
- `DragDropModule` imported in [app.module.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/app.module.ts)
- `cdkDropList` on `.steps-grid`, `cdkDrag` on each `.step-card`
- `onStepDrop()` uses `moveItemInArray()` + calls `reorderSteps()` service method
- CDK placeholder/drag-preview CSS with smooth 250ms animations

---

## Feature 3: Re-render on Camera Change

**Server**: `POST /api/manual-generator/step/{stepId}/re-render` — loads step → manual → project → model file, converts camera position/target to elevation/azimuth, calls `RenderService.RenderStep()`, saves new base64 PNG to `renderImagePath`.

**Client**:
- `reRenderStep()` service method
- "Re-render Step" button in properties panel (below Camera Zoom)
- Component handler saves step first (persists camera edits), then triggers re-render and updates the local image

---

## Feature 4: Three.js Orbit Camera Editor

**New component**: [step-camera-editor](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/step-camera-editor)

- Three.js scene with wireframe bounding box, grid, and axes  
- `OrbitControls` for interactive orbit/pan/zoom  
- Emits `CameraState` on interaction end → updates step coordinate fields  
- Bidirectional sync: parent input changes update camera position  
- Dark theme styling with grab cursor and interaction hints

**Integration**: Embedded in properties panel above camera coordinate fields. Camera changes flow back to the numeric inputs; user can then click "Re-render Step" to generate a new image.

---

## Build Verification

| Build | Result |
|---|---|
| .NET (`dotnet build`) | ✅ 0 errors |
| Angular (`ng build --configuration=development`) | ✅ Bundle generated |
