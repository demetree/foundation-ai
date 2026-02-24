# Rendering Expansion — Walkthrough

## Phase 1: Core Rendering Quality ✅

| Feature | Key Changes |
|---|---|
| **1.1 Edge Rendering** | Bresenham line rasterizer with Z-bias in `SoftwareRenderer` |
| **1.2 Smooth Shading** | `NormalSmoother.cs` (spatial hash, 60° crease angle), Gouraud interpolation |
| **1.3 Enhanced Lighting** | `LightingModel.cs` with multi-light Blinn-Phong, `Default`/`Studio` presets |

---

## Phase 2: Visual Fidelity ✅

### 2.1 — Alpha Blending / Transparency

#### [MODIFY] [LDrawMesh.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Models/LDrawMesh.cs)
- Added `SplitByTransparency()` method partitioning triangles into opaque/transparent lists

#### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)
- Refactored `Render()` to three passes: **opaque** → **transparent** (sorted back-to-front) → **edges**
- Added `_isTransparentPass` flag and `WritePixel()` helper
- `WritePixel()` uses `src_alpha * src + (1-src_alpha) * dst` blending during transparent pass
- Transparent triangles read (but don't write) the Z-buffer for correct occlusion
- Added `SortBackToFront()` using insertion sort on centroid clip-space depth

---

### 2.2 — Anti-Aliasing (SSAA)

#### [NEW] [AntiAliasMode.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/AntiAliasMode.cs)
- Enum: `None`, `SSAA2x`, `SSAA4x`

#### [NEW] [PostProcess.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/PostProcess.cs)
- `Downsample(pixels, srcW, srcH, dstW, dstH)` — box-filter downsampler averaging NxN pixel blocks

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)
- Added `antiAliasMode` parameter — renders at `width*N × height*N`, then downsamples

---

### 2.3 — Gradient / Custom Backgrounds

#### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)
- Added gradient background fields and `SetGradientBackground(topRGB, bottomRGB)` method
- `ClearBuffers()` interpolates per-scanline when gradient is active
- `SetBackground()` disables gradient mode

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)
- Added `backgroundHex`, `gradientTopHex`, `gradientBottomHex` params to all render methods
- `ParseHex()` helper for hex → RGB conversion

---

## Verification

| Project | Result |
|---|---|
| `BMC.LDraw` | ✅ Build succeeded, 0 errors |
| `BMC.LDraw.Render` | ✅ Build succeeded, 0 errors |
| `BMC.LDraw.Render.CLI` | ✅ Build succeeded, 0 errors |

All new parameters default to backward-compatible values — existing callers are unaffected.
