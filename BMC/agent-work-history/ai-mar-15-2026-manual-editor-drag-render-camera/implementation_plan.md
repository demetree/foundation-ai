# Manual Editor — Export, Reorder, Re-render & 3D Camera

Four features in priority order. Each is designed to be independently shippable.

## Proposed Changes

---

### Feature 1: PDF/HTML Export from Manual Editor

Reuse existing `HtmlManualBuilder` and `PdfManualBuilder` in `BMC.LDraw.Render`. The current manual-generator hub re-renders everything from scratch using uploaded files. The manual editor already has all images stored in DB (`renderImagePath`, `pliImagePath`). We just need a new endpoint that reads DB data and feeds it through the existing builders.

#### [NEW] `ManualExportService.cs` — Server service

Path: `BMC.Server/Services/ManualExportService.cs`

- `ExportManualAsync(int manualId, string format)` → `ManualGenerationResult`
- Loads `BuildManual` → `BuildManualPage` → `BuildManualStep` from DB
- Constructs a `ManualBuildPlan` + `ManualBuildStep` objects from DB data
- Converts base64 `renderImagePath` / `pliImagePath` → `byte[]` arrays
- Feeds them through `HtmlManualBuilder` or `PdfManualBuilder`
- Returns the result (HTML string or PDF bytes)

#### [MODIFY] [ManualGeneratorController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/ManualGeneratorController.cs)

- Add `POST /api/manual-generator/manual/{manualId}/export?format=html|pdf`
- Calls `ManualExportService.ExportManualAsync()`
- Stores result in `CompletedManuals` dictionary, returns download URL
- Reuses existing `/api/manual-generator/download/{id}` endpoint

#### [MODIFY] [manual-editor.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/manual-editor.service.ts)

- Add `exportManual(manualId: number, format: string): Observable<{downloadUrl: string}>`

#### [MODIFY] [manual-editor.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.ts)

- Add `exportHtml()` and `exportPdf()` methods
- Call service, then `window.open(downloadUrl)` to trigger download

#### [MODIFY] [manual-editor.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.html)

- Add export buttons to toolbar: "Export HTML" and "Export PDF"

---

### Feature 2: Drag-and-Drop Step Reordering

#### [MODIFY] [manual-editor.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.ts)

- Install Angular CDK (`@angular/cdk`) for `DragDropModule`
- Add `onStepDrop(event: CdkDragDrop)` handler
- Reorder steps within page or move between pages
- Call service to persist new step numbers

#### [MODIFY] [manual-editor.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.html)

- Add `cdkDropList` to `.steps-grid`, `cdkDrag` to `.step-card`

#### [MODIFY] [manual-editor.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/manual-editor.service.ts)

- Add `reorderSteps(pageId: number, stepIds: number[]): Observable<void>`

#### [MODIFY] [ManualGeneratorController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/ManualGeneratorController.cs)

- Add `PUT /api/manual-generator/page/{pageId}/reorder-steps` endpoint
- Accepts `{ stepIds: number[] }`, updates `stepNumber` for each

---

### Feature 3: Re-render Step on Camera Change

#### [MODIFY] [ManualGeneratorController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/ManualGeneratorController.cs)

- Add `POST /api/manual-generator/step/{stepId}/re-render` endpoint
- Accepts camera params (position, target, zoom)
- Loads the project's LDraw source, resolves geometry for the step, renders PNG
- Updates `BuildManualStep.renderImagePath` with new base64 image
- Returns the new image data URI

#### [MODIFY] [manual-editor.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/manual-editor.service.ts)

- Add `reRenderStep(stepId: number, camera: CameraParams): Observable<{renderImagePath: string}>`

#### [MODIFY] [manual-editor.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.ts)

- Add "Re-render" button in properties panel
- On click → call `reRenderStep()` → update step's `renderImagePath`

---

### Feature 4: Three.js Orbit Camera for Step Rotation

> [!IMPORTANT]
> This feature introduces Three.js as a client-side dependency. It replaces the manual camera coordinate inputs with an interactive 3D orbit control.

#### [NEW] `step-camera-editor.component.ts/html/scss`

Path: `BMC.Client/src/app/components/manual-editor/step-camera-editor/`

- Standalone Angular component with a Three.js canvas
- Loads the step's rendered image as a textured plane (or loads the actual 3D model GLB if available)
- `OrbitControls` for drag-to-rotate
- Emits `(cameraChanged)` event with new position/target/zoom
- Shows live preview; user clicks "Apply" to commit

#### [MODIFY] [manual-editor.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.html)

- Replace camera coordinate inputs in properties panel with `<app-step-camera-editor>`
- Wire `(cameraChanged)` → update step + trigger re-render

---

## Verification Plan

### Automated Tests
- `dotnet build` after each server-side change
- `ng build --configuration=development` after each client-side change

### Manual Verification
- **Export**: Import test model → open manual editor → Export HTML → verify download contains cover + steps + PLI + BOM. Same for PDF.
- **Drag-and-drop**: Drag step 3 before step 1 → verify step numbers update → refresh page → verify persisted
- **Re-render**: Change camera position → click re-render → verify image updates in step card
- **3D camera**: Open orbit editor → drag to rotate → click Apply → verify camera coordinates update → re-render shows new angle
