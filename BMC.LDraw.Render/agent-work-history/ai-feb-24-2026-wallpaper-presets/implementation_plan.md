# Stream-Based Rendering Methods

Add content-based (`string[]`) overloads alongside file-based methods to eliminate temp file I/O for uploads.

## Proposed Changes

### BMC.LDraw — GeometryResolver

#### [MODIFY] [GeometryResolver.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Parsers/GeometryResolver.cs)

Add two new methods (mirroring `ResolveFile` / `ResolveFileWithPartCounts`):

- **`ResolveFromContent(string[] lines, string fileName, int parentColourCode)`** — calls `GeometryParser.ParseLines(lines, fileName)` instead of `ParseFile`, then caches MPD submodels and calls `Resolve()`
- **`ResolveFromContentWithPartCounts(...)`** — same but tracks per-part triangle/edge counts (for exploded view)
- **`GetStepCountFromContent(string[] lines, string fileName)`** — returns step count from lines

---

### BMC.LDraw.Render — RenderService

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

Add content-based overloads for all render methods. Each replaces the `string inputPath` parameter with `string[] lines, string fileName`:

| Existing (file-based) | New (content-based) |
|---|---|
| `RenderToPixels(inputPath, ...)` | `RenderToPixels(lines, fileName, ...)` |
| `RenderToPng(inputPath, ...)` | `RenderToPng(lines, fileName, ...)` |
| `RenderToWebP(inputPath, ...)` | `RenderToWebP(lines, fileName, ...)` |
| `RenderToSvg(inputPath, ...)` | `RenderToSvg(lines, fileName, ...)` |
| `RenderTurntableGif(inputPath, ...)` | `RenderTurntableGif(lines, fileName, ...)` |
| `RenderExplodedView(inputPath, ...)` | `RenderExplodedView(lines, fileName, ...)` |

Each new overload calls `resolver.ResolveFromContent(lines, fileName, ...)` instead of `resolver.ResolveFile(inputPath, ...)`, then delegates to the same rendering pipeline.

---

### BMC.Server — PartRendererController

#### [MODIFY] [PartRendererController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartRendererController.cs)

Simplify `RenderUpload` endpoint:
- Read `IFormFile` into `string[]` in memory via `StreamReader.ReadToEnd().Split('\\n')`
- Call content-based `RenderService` methods directly
- Remove all temp file save/delete logic

## Verification Plan

### Automated Tests
- `dotnet build` the solution to verify compilation
