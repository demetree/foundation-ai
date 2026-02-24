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
| **2.1 Alpha Blending** | Two-pass opaque/transparent rendering, `WritePixel` compositing, `SortBackToFront` |
| **2.2 Anti-Aliasing** | `AntiAliasMode` enum, `PostProcess.Downsample` box filter (2×/4× SSAA) |
| **2.3 Gradient Backgrounds** | `SetGradientBackground` with per-scanline interpolation in `ClearBuffers` |

---

## Phase 3: Output & Formats ✅

### 3.1 — WebP Output

#### [NEW] [ImageExporter.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/ImageExporter.cs)
- Replaces `PngExporter` — preserves all PNG methods, adds `SaveToWebP`/`ToWebPBytes` with quality control
- Backward-compat `PngExporter` alias class delegates to `ImageExporter`
- Uses ImageSharp's built-in `WebpEncoder` (lossy, configurable quality 1–100)

#### [DELETE] [PngExporter.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/PngExporter.cs)
- Removed — functionality moved to `ImageExporter.cs`

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)
- Updated `PngExporter` → `ImageExporter` references
- Added `RenderToWebP(...)` method with `quality` parameter

---

### 3.2 — Animated Turntable GIF

#### [NEW] [TurntableRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/TurntableRenderer.cs)
- Renders N frames at evenly-spaced azimuths (0°→360°) using `SoftwareRenderer`
- Assembles into looping animated GIF via ImageSharp's `GifEncoder`
- Configurable: `frameCount`, `frameDelayMs`, `elevation`, `renderEdges`, `smoothShading`

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)
- Added `RenderTurntableGif(...)` convenience method handling mesh loading + normal smoothing

---

### 3.3 — SVG Vector Output

#### [NEW] [SvgExporter.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SvgExporter.cs)
- Projects triangles to 2D using camera VP matrix, sorts by depth (painter's algorithm)
- Emits SVG `<polygon>` elements with flat-shaded Blinn-Phong lighting
- Emits edge `<line>` elements with original colours
- Self-contained projection and lighting — no coupling to `SoftwareRenderer`

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)
- Added `RenderToSvg(...)` returning SVG document string

---

## Verification

| Project | Result |
|---|---|
| `BMC.LDraw` | ✅ Build succeeded, 0 errors |
| `BMC.LDraw.Render` | ✅ Build succeeded, 0 errors |
| `BMC.LDraw.Render.CLI` | ✅ Build succeeded, 0 errors |

All new methods default to backward-compatible values.
