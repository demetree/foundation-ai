# Manual Editor — Export, Reorder, Re-render & 3D Camera

## Feature 1: PDF/HTML Export
- [x] Create `ManualExportService.cs` — DB → builder pipeline
- [x] Add export endpoint to `ManualGeneratorController.cs`
- [x] Add `exportManual()` to `manual-editor.service.ts`
- [x] Add export buttons + methods to component TS/HTML
- [x] Build & test export (.NET 0 errors, Angular build passed)

## Feature 2: Drag-and-Drop Step Reordering
- [x] Add reorder endpoint to controller
- [x] Add `reorderSteps()` to service
- [x] Add CDK DragDrop to component
- [x] Build & test reorder

## Feature 3: Re-render Step on Camera Change
- [x] Add re-render endpoint to controller
- [x] Add `reRenderStep()` to service
- [x] Add re-render button to component
- [x] Build & test re-render

## Feature 4: Three.js Orbit Camera
- [x] Create `step-camera-editor` component
- [x] Integrate into properties panel
- [x] Wire camera change → coordinate fields
- [x] Build & test
