# Phase 3: Output & Formats — Implementation Plan

Expand beyond single-frame PNG to support WebP output, animated turntable renders, and SVG vector output. No new packages needed — ImageSharp 3.x already supports WebP and GIF encoding.

---

## 3.1 — WebP Output

### [MODIFY] [PngExporter.cs → ImageExporter.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/PngExporter.cs)

- Rename class to `ImageExporter` (keep file as `ImageExporter.cs`)
- Keep existing `SaveToPng` / `ToPngBytes` methods
- Add `SaveToWebP(pixels, width, height, outputPath, quality)` — uses `image.SaveAsWebp()` with configurable quality
- Add `ToWebPBytes(pixels, width, height, quality)` — in-memory WebP encoding

### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Update `PngExporter` references to `ImageExporter`
- Add `RenderToWebP(...)` method returning WebP bytes
- Add `OutputFormat` enum parameter (`Png`, `WebP`) to `RenderToFile`

---

## 3.2 — Animated Turntable GIF

### [NEW] [TurntableRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/TurntableRenderer.cs)

- Takes the parsed/resolved `LDrawMesh`, frame count, and render options
- Renders N frames at evenly-spaced azimuth angles (0° → 360°) using `SoftwareRenderer`
- Assembles into animated GIF via ImageSharp's `GifEncoder` with per-frame delay
- Returns `byte[]` (GIF bytes)

### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Add `RenderTurntableGif(inputPath, frames, width, height, ...)` convenience method
- Handles mesh loading, normal smoothing, and delegates to `TurntableRenderer`

---

## 3.3 — SVG Vector Output

### [NEW] [SvgExporter.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SvgExporter.cs)

- Projects triangles to 2D using the same camera/VP matrix as `SoftwareRenderer`
- Sorts by centroid depth (painter's algorithm) — farthest first
- Emits SVG `<polygon>` elements with flat-shaded fill colours (lighting applied)
- Emits edge lines as SVG `<line>` elements
- Pure string/XML output, no dependencies beyond `System.Text`

### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Add `RenderToSvg(inputPath, width, height, ...)` returning SVG string

---

## Verification Plan

### Build
```
dotnet build BMC.LDraw.Render/BMC.LDraw.Render.csproj
dotnet build BMC.LDraw.Render.CLI/BMC.LDraw.Render.CLI.csproj
```

### Feature
- **3.1**: Render to WebP, compare file size vs PNG
- **3.2**: Generate turntable GIF, verify it animates smoothly
- **3.3**: Generate SVG, open in browser to verify vector output
