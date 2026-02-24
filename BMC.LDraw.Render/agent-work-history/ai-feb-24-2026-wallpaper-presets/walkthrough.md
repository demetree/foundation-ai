# Stream-Based Rendering Methods

Added content-based (`string[]`) rendering methods alongside existing file-based ones, eliminating temp file I/O for uploads.

## Changes

### [GeometryResolver.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Parsers/GeometryResolver.cs)

Three new methods that accept `string[] lines, string fileName` instead of a file path:

| Method | Purpose |
|---|---|
| `ResolveFromContent` | Parse lines → resolve mesh (mirrors `ResolveFile`) |
| `ResolveFromContentWithPartCounts` | Same but tracks part boundaries (mirrors `ResolveFileWithPartCounts`) |
| `GetStepCountFromContent` | Step count from lines (mirrors `GetStepCount`) |

render_diffs(file:///g:/source/repos/Scheduler/BMC.LDraw/Parsers/GeometryResolver.cs)

---

### [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

Six content-based overloads added under the "Content-Based Overloads" section:
- `RenderToPixels(lines, fileName, ...)`
- `RenderToPng(lines, fileName, ...)`
- `RenderToWebP(lines, fileName, ...)`
- `RenderToSvg(lines, fileName, ...)`
- `RenderTurntableGif(lines, fileName, ...)`
- `RenderExplodedView(lines, fileName, ...)`

Each calls `ResolveFromContent` instead of `ResolveFile`, then delegates to the same rendering pipeline.

render_diffs(file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

---

### [PartRendererController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartRendererController.cs)

`RenderUpload` endpoint simplified:
- **Before**: Save `IFormFile` to temp file → render → delete temp file (with `finally` cleanup)
- **After**: Read `IFormFile` into `string[]` via `StreamReader` → pass directly to content-based methods

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartRendererController.cs)

## Verification

- ✅ `dotnet build BMC.Server.csproj` — Build succeeded
