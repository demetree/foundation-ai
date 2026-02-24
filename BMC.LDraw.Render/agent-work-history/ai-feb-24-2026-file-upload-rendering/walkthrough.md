# File Upload Rendering — Walkthrough

## What Changed

### Server — `PartRendererController.cs`
Added `POST /api/part-renderer/render-upload`:
- Accepts `IFormFile` + all render params as query strings
- Validates extension (`.dat`, `.ldr`, `.mpd`) and size (≤ 5 MB)
- Saves to temp → renders → returns blob → cleans up temp file
- Supports all formats (PNG, WebP, SVG, GIF turntable)
- No caching (uploads are transient)

### Angular — Part Renderer Component

**TypeScript** — New state & methods:
- `activeTab: 'search' | 'upload'` — tab mode selector
- `uploadedFile`, `uploadedFileName`, `isDragOver` — upload state
- `onFileSelected()`, `onFileDrop()`, `onDragOver()`, `onDragLeave()` — file input handlers
- `renderUploadedFile()` — POSTs `FormData` to the upload endpoint

**HTML** — Tabbed config panel:
- Tab bar: **Search Parts** | **Upload File**
- Upload tab: drag-and-drop zone, file info card with remove button, all-colours picker
- Shared sections (Size, Angle, Advanced, Render Button) visible in both tabs

**SCSS** — New styles:
- `.tab-bar` / `.tab-btn` — pill button tab switcher
- `.upload-drop-zone` — dashed border, drag-over highlight, file-selected state
- `.upload-file-info` — file name, size, remove button

## Files Modified

| File | Change |
|------|--------|
| [PartRendererController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartRendererController.cs) | Added `RenderUpload` POST endpoint |
| [part-renderer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/part-renderer/part-renderer.component.ts) | Upload state & methods |
| [part-renderer.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/part-renderer/part-renderer.component.html) | Tab bar + upload panel |
| [part-renderer.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/part-renderer/part-renderer.component.scss) | Tab & upload zone styles |

## Verification
- ✅ Server build passes (`dotnet build` — Build succeeded)
