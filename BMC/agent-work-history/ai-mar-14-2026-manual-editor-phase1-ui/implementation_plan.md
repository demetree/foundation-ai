# Manual Editor — LPub3D Feature Parity Plan

## Current State vs LPub3D Core Features

| Feature | LPub3D | BMC Status | Gap |
|---|---|---|---|
| **Step-by-step page layout** | ✅ | ✅ Pages + step grid | — |
| **Server-rendered step images** | ✅ | ✅ 256×256 PNG base64 | — |
| **Camera per step** | ✅ | ✅ Position, target, zoom | — |
| **ROTSTEP camera angles** | ✅ | ✅ Parsed + mapped | — |
| **Page size presets** | ✅ | ✅ A4/A5/Letter/Square | — |
| **Fade previous steps** | ✅ | ⚠️ DB field exists, not used in render | **Render** |
| **Parts List Image (PLI)** | ✅ | ❌ DB field exists, no generation | **Generate + UI** |
| **Bill of Materials (BOM)** | ✅ | ❌ Flag parsed, no BOM page | **Generate + UI** |
| **Callout submodel insets** | ✅ | ❌ DB fields exist, no UI | **UI** |
| **Page background colour** | ✅ | ❌ DB field exists, no UI | **UI** |
| **Exploded view** | ✅ | ⚠️ Toggle in UI, no effect on render | **Render** |
| **Part count per step** | ✅ | ❌ Data available via BuildStepPart | **UI** |
| **Step drag-and-drop reorder** | ✅ | ❌ | **UI** |
| **PDF export** | ✅ | ❌ | **Export** |
| **Cover page** | ✅ | ❌ Flag parsed, no UI | **UI** |
| **Page orientation** | ✅ | ❌ Parsed, not applied | **UI** |

## Proposed Implementation — Priority Order

### Phase 1: Quick UI Wins (expose existing data)

These features have backend data already — just need UI wiring.

> [!IMPORTANT]
> These are the highest-impact, lowest-effort items.

#### [MODIFY] [manual-editor.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.html)
#### [MODIFY] [manual-editor.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.ts)
#### [MODIFY] [manual-editor.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.scss)

1. **Part count badge** on each step card — query `BuildStepPart` count
2. **Callout badge** — show "Callout: {modelName}" on callout steps
3. **Page background colour** — apply `backgroundColorHex` as CSS background
4. **Fade step toggle** — add checkbox in step properties panel
5. **PLI toggle** — add checkbox in step properties panel
6. **Cover page indicator** — visual badge on the first page if `hasCoverPage`
7. **Page orientation** — apply landscape/portrait to page aspect ratio

---

### Phase 2: PLI Image Generation

Parts List Images show individual parts added in each step with quantities.

#### [MODIFY] [ModelImportService.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/ModelImportService.cs)
- Generate PLI images during import (render each unique part added in a step)
- Store as base64 data URI in `pliImagePath`

#### [MODIFY] [manual-editor.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.html)
- Show PLI image below or beside the step render when `pliImagePath` exists

---

### Phase 3: Fade Step Rendering

#### [MODIFY] [RenderService](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/RenderService.cs)
- When `fadeStepEnabled=true`, render previous-step parts at 30% opacity
- Requires passing previous steps' part lists to the renderer

---

### Phase 4: BOM Page Generation

#### [MODIFY] [ModelImportService.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/ModelImportService.cs)
- If `HasBillOfMaterials`, auto-generate a BOM page listing all unique parts with quantities and colours

#### [MODIFY] [manual-editor.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.html)
- Render BOM page with a parts table layout

---

### Phase 5: PDF Export

#### [NEW] PdfExportService (server-side)
- Render each page to a PDF using a headless renderer or direct PDF generation
- Endpoint: `GET /api/BuildManual/{id}/export/pdf`

#### [MODIFY] [manual-editor.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.html)
- "Export PDF" button in toolbar

---

### Phase 6: Step Drag-and-Drop

#### [MODIFY] [manual-editor.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-editor/manual-editor.component.ts)
- Add CDK DragDrop for reordering steps within and between pages
- Auto-renumber steps on drop

---

## Verification Plan

### Phase 1
- Import test model, verify part counts, callout badges, and background colour render in UI
- Toggle fade/PLI checkboxes, verify they persist on save

### Phases 2-6
- Each phase tested after implementation with the test model
