# BMC.LDraw.Render — Rendering Expansion Plan

Expand the CPU-based software renderer from a basic flat-shaded rasterizer into a full-featured LDraw rendering engine, adding edge rendering, smooth shading, transparency, multiple output formats, and full model/instruction rendering.

## User Review Required

> [!IMPORTANT]
> **No new 3rd-party packages** are introduced — all features build on the existing `SixLabors.ImageSharp` (already referenced) and pure C# math. The only exception is Phase 3.3 (SVG) which is pure string/XML output.

> [!WARNING]
> **Phase 4 (model rendering)** requires the `ModelParser` to handle `.ldr`/`.mpd` files and the `GeometryResolver` to handle multi-model resolution. Both exist but have only been used for parts so far — model rendering may surface edge cases in the parser.

> [!IMPORTANT]
> This is a **large body of work** spanning many sessions. Each phase is designed to be independently shippable. I recommend we tackle Phase 1 first and revisit priorities after.

---

## Phase 1: Core Rendering Quality

The biggest visual bang for the buck. Edges, smooth shading, and better lighting transform flat cartoon renders into recognisable LEGO parts.

---

### 1.1 — Edge / Outline Rendering

The `GeometryResolver` already populates `LDrawMesh.EdgeLines` with `MeshLine` segments, but `SoftwareRenderer` currently ignores them entirely. Adding edge rendering gives parts the classic LEGO instruction-manual look.

#### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)

- Add a `RenderEdges` boolean property (default: `true`)
- After triangle rasterization, draw each `MeshLine` using Bresenham's line algorithm
- Lines are drawn with a Z-bias so they sit just in front of the surface (prevent z-fighting)
- Edge colour comes from the `MeshLine.R/G/B/A` values (already resolved by `GeometryResolver`)

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Add `bool renderEdges = true` parameter to `RenderToPixels` / `RenderToPng` / `RenderToFile`
- Pass through to `SoftwareRenderer`

---

### 1.2 — Smooth (Gouraud) Shading

Currently every triangle is flat-shaded with a single normal. Smooth shading averages normals at shared vertices so curved surfaces (cylinders, spheres) look smoother without increasing triangle count.

#### [NEW] [NormalSmoother.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/NormalSmoother.cs)

- Post-process pass on `LDrawMesh`: groups triangles by shared vertex positions (within epsilon), computes averaged normals at each vertex
- Produces a new mesh structure where each triangle vertex has its own normal (instead of the current single flat normal per triangle)
- Respects a crease angle threshold (~60°) — edges sharper than this keep their flat normals, preventing sphere normals from bleeding into flat surfaces

#### [MODIFY] [LDrawMesh.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Models/LDrawMesh.cs)

- Add per-vertex normal fields to `MeshTriangle`: `NX1,NY1,NZ1`, `NX2,NY2,NZ2`, `NX3,NY3,NZ3`
- When not smooth-shaded, these default to the existing flat normal (`NX,NY,NZ`)

#### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)

- In `RasterizeTriangle`, interpolate per-vertex normals using barycentric coordinates
- Compute lighting per-pixel instead of per-triangle (Gouraud interpolation)
- Add `SmoothShading` boolean property (default: `true`)

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Call `NormalSmoother` before rendering when smooth shading is enabled

---

### 1.3 — Enhanced Lighting

Replace the hardcoded single directional light with a configurable lighting system supporting multiple lights and specular highlights.

#### [NEW] [LightingModel.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/LightingModel.cs)

- `Light` class: direction/position, colour, intensity, type (directional/point)
- `LightingModel` class: collection of lights, ambient colour/intensity, specular power
- Static factory `LightingModel.Default()` returns current behaviour (single directional + ambient) for backward compatibility

#### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)

- Replace hardcoded `_lightX/Y/Z` and `_ambient` with a `LightingModel` property
- Compute diffuse + specular per light source using Blinn-Phong model
- Sum contributions from all lights, clamped to [0,1]

---

## Phase 2: Visual Fidelity

Polish the render output with transparency support, anti-aliasing, and backgrounds.

---

### 2.1 — Alpha Blending / Transparency

LDraw transparent colours already have proper alpha values resolved by `GeometryResolver`, but the rasterizer ignores alpha — it just overwrites pixels based on Z-depth.

#### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)

- **Two-pass rendering**: first render all opaque triangles (alpha == 255) with Z-buffer; then render transparent triangles sorted back-to-front using painter's algorithm
- Alpha blending uses standard `src_alpha * src + (1 - src_alpha) * dst` formula
- Transparent triangles still write to Z-buffer for correct occlusion between transparent faces

#### [MODIFY] [LDrawMesh.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Models/LDrawMesh.cs)

- Add helper method `SplitByTransparency(out List<MeshTriangle> opaque, out List<MeshTriangle> transparent)` for the two-pass system

---

### 2.2 — Anti-Aliasing

#### [NEW] [PostProcess.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/PostProcess.cs)

- **SSAA (Super-Sample Anti-Aliasing)**: render at 2× resolution, downsample with box filter — simplest and most effective for a CPU renderer
- Static method `Downsample(byte[] pixels, int srcW, int srcH, int dstW, int dstH) → byte[]`
- Optionally, a lightweight **FXAA** implementation operating on the final buffer

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Add `AntiAliasing` enum parameter: `None`, `SSAA2x`, `SSAA4x`
- When enabled, render at multiplied resolution, then downsample

---

### 2.3 — Gradient / Custom Backgrounds

#### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)

- Add `SetGradientBackground(topR, topG, topB, bottomR, bottomG, bottomB)` method
- During buffer clear, interpolate per-scanline between top and bottom colours
- Keep existing `SetBackground` for solid colours; transparent remains the default

---

## Phase 3: Output & Formats

Expand beyond single-frame PNG to support more formats and animated output.

---

### 3.1 — WebP Output

#### [MODIFY] [PngExporter.cs → ImageExporter.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/PngExporter.cs)

- Rename to `ImageExporter` (keep `PngExporter` class as backward-compatible alias)
- Add `ToWebPBytes(byte[] rgbaPixels, int width, int height, int quality = 90) → byte[]` using ImageSharp's built-in WebP encoder
- Add `SaveToWebP(...)` file variant

---

### 3.2 — Animated Turntable GIF / APNG

#### [NEW] [TurntableRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/TurntableRenderer.cs)

- Takes a `RenderService`, part path, frame count, and azimuth range
- Renders N frames at evenly-spaced azimuth angles around the part
- Assembles frames into animated GIF or APNG using ImageSharp's `GifEncoder` / `PngEncoder`
- Returns byte array or saves to file

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Add `RenderTurntable(string inputPath, int frames, int width, int height, ...)` convenience method

---

### 3.3 — SVG Vector Output

#### [NEW] [SvgExporter.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SvgExporter.cs)

- Project triangles to 2D using the same camera transforms as `SoftwareRenderer`
- Sort by depth (painter's algorithm) and emit as SVG `<polygon>` elements with fill colours
- Render edges as SVG `<line>` elements
- Output clean, scalable vector graphics suitable for print / instructions

---

## Phase 4: Advanced Features

Move from single-part rendering to full model/scene rendering.

---

### 4.1 — Full Model Rendering (.ldr / .mpd)

#### [MODIFY] [GeometryResolver.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Parsers/GeometryResolver.cs)

- Ensure `ResolveFile` correctly handles `.ldr` and `.mpd` multi-model files (it may already work, needs testing)
- Handle MPD submodel name resolution for embedded models

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- `RenderModel(string inputPath, ...)` — resolve full model geometry and render as single scene
- Auto-frame camera to the full assembled model

---

### 4.2 — Step-by-Step Build Instruction Renders

#### [NEW] [InstructionRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/InstructionRenderer.cs)

- Uses `ModelParser` to get step-by-step part placements
- For each step: resolve the cumulative geometry up to that step, render to PNG
- Highlight newly-added parts (render them in full colour, previous steps in muted/ghosted colour)
- Returns `List<byte[]>` (one PNG per step)

---

### 4.3 — Exploded View Rendering

#### [NEW] [ExplodedViewRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/ExplodedViewRenderer.cs)

- Takes a resolved model and applies an explosion factor — translates each part away from the model centre along the vector from centre to part centroid
- Renders the exploded state to show internal structure
- Configurable explosion factor (0 = assembled, 1 = default spread)

---

## Cross-Cutting Updates

### [MODIFY] [PartRendererController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartRendererController.cs)

- Add query parameters for new render options: `edges`, `smooth`, `antiAlias`, `background`, `format`
- Add turntable endpoint: `GET /api/part-renderer/turntable`
- Update cache keys to include new parameters

### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render.CLI/Program.cs) (CLI)

- Add flags for edge rendering, shading mode, lighting presets, output format
- Add `turntable` command

### [MODIFY] [part-renderer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/part-renderer/part-renderer.component.ts)

- Add UI controls for: edges toggle, shading mode, anti-aliasing, background picker, output format, turntable button

---

## Verification Plan

### Build Verification
- `dotnet build BMC.LDraw/BMC.LDraw.csproj` — verify model changes compile
- `dotnet build BMC.LDraw.Render/BMC.LDraw.Render.csproj` — verify renderer changes compile
- `dotnet build BMC.LDraw.Render.CLI/BMC.LDraw.Render.CLI.csproj` — verify CLI compiles
- `dotnet build BMC/BMC.Server/BMC.Server.csproj` — verify server compiles

### CLI Rendering Tests
After each phase, use the CLI to render a known part (e.g. `3001.dat` — the classic 2×4 brick) with the new options and visually inspect the output PNG. Specific test renders per phase:
- **Phase 1**: Render with edges on/off, smooth vs flat shading, different lighting presets
- **Phase 2**: Render transparent parts (e.g. windshields), SSAA on/off, gradient backgrounds
- **Phase 3**: Render to WebP, generate turntable GIF, produce SVG output
- **Phase 4**: Render a full `.ldr` model, generate step-by-step instruction images

### Manual Verification (User)
- After Phase 1, start the BMC.Server and BMC.Client locally. Use the Part Renderer UI to verify new render options appear and work, and visually confirm edge rendering and smooth shading look correct
- After Phase 3, verify turntable animation endpoint works from the Part Renderer UI
