# Rendering Expansion — Walkthrough

## Phase 1: Core Rendering Quality ✅

| Feature | Key Changes |
|---|---|
| **1.1 Edge Rendering** | Bresenham line rasterizer with Z-bias in `SoftwareRenderer` |
| **1.2 Smooth Shading** | `NormalSmoother.cs` (spatial hash, 60° crease angle), Gouraud interpolation |
| **1.3 Enhanced Lighting** | `LightingModel.cs` with multi-light Blinn-Phong, `Default`/`Studio` presets |

---

## Phase 2: Visual Fidelity ✅

| Feature | Key Changes |
|---|---|
| **2.1 Alpha Blending** | Two-pass opaque/transparent rendering, `WritePixel` compositing |
| **2.2 Anti-Aliasing** | `AntiAliasMode` enum, `PostProcess.Downsample` box filter (2×/4× SSAA) |
| **2.3 Gradient Backgrounds** | `SetGradientBackground` with per-scanline interpolation |

---

## Phase 3: Output & Formats ✅

| Feature | Key Changes |
|---|---|
| **3.1 WebP Output** | [ImageExporter.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/ImageExporter.cs) — replaces `PngExporter`, adds `SaveToWebP`/`ToWebPBytes` |
| **3.2 Turntable GIF** | [TurntableRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/TurntableRenderer.cs) — N-frame rotation loop via `GifEncoder` |
| **3.3 SVG Vector** | [SvgExporter.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SvgExporter.cs) — projected polygons with depth sort and lighting |

---

## Phase 4: Advanced Features ✅

### 4.1 — STEP Meta-Command Parsing

#### [MODIFY] [LDrawGeometry.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Models/LDrawGeometry.cs)
- Added `List<int> StepBreaks` — records subfile ref count at each `0 STEP` boundary

#### [MODIFY] [GeometryParser.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Parsers/GeometryParser.cs)
- Detects `0 STEP` meta-commands in `ParseSingle`, records step breaks
- Adds implicit final step for trailing geometry after last STEP

---

### 4.2 — Step-by-Step Build Instruction Rendering

#### [MODIFY] [GeometryResolver.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Parsers/GeometryResolver.cs)
- `GetStepCount(filePath)` — returns number of build steps
- `ResolveFileUpToStep(filePath, stepIndex, colourCode)` — cumulative step resolution
- `ResolveFileWithPartCounts(...)` — tracks per-subfile triangle/edge counts (for exploded view)
- Helper methods: `ResolveDirectGeometry`, `ResolveSubfilesUpTo`

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)
- `GetStepCount(inputPath)` — step count query
- `RenderStep(inputPath, stepIndex, ...)` — single step PNG (cumulative)
- `RenderAllSteps(inputPath, ...)` — `List<byte[]>` of all step PNGs

---

### 4.3 — Exploded View Rendering

#### [NEW] [ExplodedViewBuilder.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/ExplodedViewBuilder.cs)
- Computes per-part centroids and model centroid
- Applies radial offset (configurable `explosionFactor`)
- Preserves root-level geometry in place

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)
- `RenderExplodedView(inputPath, explosionFactor, ...)` — renders exploded PNG

---

## Verification

| Project | Result |
|---|---|
| `BMC.LDraw` | ✅ Build succeeded, 0 errors |
| `BMC.LDraw.Render` | ✅ Build succeeded, 0 errors |
| `BMC.LDraw.Render.CLI` | ✅ Build succeeded, 0 errors |
