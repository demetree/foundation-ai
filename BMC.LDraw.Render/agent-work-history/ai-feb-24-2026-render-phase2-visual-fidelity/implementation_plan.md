# Phase 2: Visual Fidelity — Implementation Plan

Polish the rendered output with transparency support, anti-aliasing, and configurable backgrounds.

> [!IMPORTANT]
> All three features are independently shippable — each can land without the others.

---

## 2.1 — Alpha Blending / Transparency

Currently the rasterizer overwrites pixels based purely on Z-depth, ignoring the alpha channel. LDraw transparent colours (e.g. colour 36 = Trans-Red) already have proper alpha values from `GeometryResolver`, so we just need to handle blending.

**Approach:** Two-pass rendering — opaque triangles first (Z-buffer), then transparent triangles sorted back-to-front with alpha compositing.

### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)

Refactor `Render()` to three passes:

1. **Opaque pass** — Rasterize triangles with `A == 255` using Z-buffer (current behaviour)
2. **Transparent pass** — Sort remaining triangles back-to-front by centroid depth, rasterize with alpha blending (`src_alpha * src + (1 - src_alpha) * dst`)
3. **Edge pass** — Unchanged

Changes to pixel-writing logic:
- `FillTriangleFlat` and `FillTriangleSmooth` gain an `isTransparent` parameter
- When transparent, blend with existing pixel instead of overwriting
- Transparent triangles still read Z-buffer (for occlusion by opaque) but skip Z-writes to allow multiple transparent layers to stack

### [MODIFY] [LDrawMesh.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Models/LDrawMesh.cs)

- Add `SplitByTransparency()` method that partitions `Triangles` into opaque (`A == 255`) and transparent (`A < 255`) lists

---

## 2.2 — Anti-Aliasing (SSAA)

Super-Sample Anti-Aliasing is the simplest AA for a CPU renderer — render at 2× or 4× resolution, then downsample with a box filter.

### [NEW] [PostProcess.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/PostProcess.cs)

- `static byte[] Downsample(byte[] pixels, int srcW, int srcH, int dstW, int dstH)` — box-filter downsampler
- Averages NxN pixel blocks from the source into single destination pixels
- Operates on RGBA byte arrays

### [NEW] [AntiAliasMode.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/AntiAliasMode.cs)

Simple enum: `None`, `SSAA2x`, `SSAA4x`

### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Add `AntiAliasMode antiAliasMode = AntiAliasMode.None` parameter
- When `SSAA2x`: render at `width*2 × height*2`, downsample to `width × height`
- When `SSAA4x`: render at `width*4 × height*4`, downsample

---

## 2.3 — Gradient / Custom Backgrounds

### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)

- Add `SetGradientBackground(byte topR, topG, topB, byte bottomR, bottomG, bottomB)` method
- Store gradient state in private fields
- In the buffer-clear loop of `Render()`, interpolate per-scanline between top and bottom colours
- Existing `SetBackground()` for solid colours is preserved; gradient overrides it
- Default remains transparent (no gradient, `_bgA = 0`)

### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Add optional background parameters: `backgroundColour` (hex) and `gradientTop`/`gradientBottom` (hex pair)
- Parse hex values and call `SetBackground`/`SetGradientBackground` on the renderer

---

## Verification Plan

### Build Verification
```
dotnet build BMC.LDraw/BMC.LDraw.csproj
dotnet build BMC.LDraw.Render/BMC.LDraw.Render.csproj
dotnet build BMC.LDraw.Render.CLI/BMC.LDraw.Render.CLI.csproj
```

### Feature Verification
- **2.1**: Render a transparent part (colour 36/41/47) and verify blending looks correct against background
- **2.2**: Render with SSAA2x and compare jagged edges to None — edges should be visually smoother
- **2.3**: Render with gradient background — verify smooth top-to-bottom colour transition
