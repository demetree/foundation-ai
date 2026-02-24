# File Upload Rendering

Add a file upload tab to the Part Renderer so users can render custom `.dat`, `.ldr`, and `.mpd` files.

## Proposed Changes

### Server API — PartRendererController

#### [MODIFY] [PartRendererController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartRendererController.cs)

Add a `POST /api/part-renderer/render-upload` endpoint:
- Accepts `IFormFile file` + the same rendering query params (colourCode, width, height, format, edges, smooth, AA, background, etc.)
- Validates extension (`.dat`, `.ldr`, `.mpd`) and size (max 5MB)
- Saves uploaded file to a temp directory, calls `RenderService`, returns the rendered image, then cleans up the temp file
- No caching (user uploads are unique/transient)
- Returns correct content type per format

---

### Angular UI — Part Renderer Component

#### [MODIFY] [part-renderer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/part-renderer/part-renderer.component.ts)

- Add `activeTab: 'search' | 'upload'` state
- Add `uploadedFile: File | null`, `uploadedFileName: string`
- Add `onFileSelected(event)`, `onFileDrop(event)`, `renderUploadedFile()` methods
- `renderUploadedFile()` POSTs a `FormData` to the new endpoint with the file + rendering params

#### [MODIFY] [part-renderer.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/part-renderer/part-renderer.component.html)

- Add tab bar below header: **Search Parts** | **Upload File**
- **Search tab**: Current config panel (search, colour, size, angle, advanced settings)
- **Upload tab**: Drag-and-drop zone, file info card, colour picker (all colours mode only), size/angle presets, advanced settings, render button
- Both tabs share the same preview panel on the right

#### [MODIFY] [part-renderer.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/part-renderer/part-renderer.component.scss)

- Tab bar styles (pill buttons, matching the mode-btn pattern)
- Drag-and-drop zone styles (dashed border, hover highlight, accepted file indicator)

## Verification Plan

### Automated Tests
- `dotnet build` the server project to verify the new endpoint compiles
- Verify Angular builds with `ng build` or production build check
