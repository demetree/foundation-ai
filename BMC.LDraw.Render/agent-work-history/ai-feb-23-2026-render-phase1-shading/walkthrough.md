# Phase 1: Core Rendering Quality — Walkthrough

## Summary

Implemented three rendering enhancements for `BMC.LDraw.Render`:

1. **Edge rendering** — Bresenham line drawing with Z-bias
2. **Smooth (Gouraud) shading** — Per-vertex normal averaging with crease angle support
3. **Enhanced Blinn-Phong lighting** — Multi-light support with specular highlights

---

## Changes Made

### Phase 1.1: Edge Rendering

#### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)
- Added `RenderEdges` property (default `true`)
- Second rasterization pass draws `MeshLine` edges using Bresenham's algorithm
- Z-bias (`-0.0005`) prevents Z-fighting with underlying triangles

#### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)
- `renderEdges` parameter added to `RenderToFile`, `RenderToPng`, `RenderToPixels`

---

### Phase 1.2: Smooth Shading

#### [MODIFY] [LDrawMesh.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Models/LDrawMesh.cs)
- `MeshTriangle` struct extended with per-vertex normals (`NX1`..`NZ3`) and `HasPerVertexNormals` flag

#### [NEW] [NormalSmoother.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/NormalSmoother.cs)
- Static class that post-processes an `LDrawMesh` to compute averaged vertex normals
- Spatial hashing groups vertices by position (ε = 0.1)
- 60° crease angle threshold preserves sharp box edges while smoothing curves
- `Smooth(mesh)` / `Smooth(mesh, creaseAngle)` API

#### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)
- Split into `FillTriangleFlat` and `FillTriangleSmooth` paths
- Smooth path interpolates per-vertex normals via barycentric coordinates
- `SmoothShading` property (default `true`)

#### [MODIFY] [GeometryResolver.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Parsers/GeometryResolver.cs)
- `MakeTriangle` initializes new per-vertex normal fields to zero/false

---

### Phase 1.3: Enhanced Lighting

#### [NEW] [LightingModel.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/LightingModel.cs)
- `Light` class: direction/position, colour, intensity, type (Directional/Point)
- `LightingModel`: collection of lights + ambient + specular power/intensity
- `LightingModel.Default()`: matches original single-light behavior
- `LightingModel.Studio()`: 3-light setup (key/fill/rim) with specular

#### [MODIFY] [SoftwareRenderer.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/SoftwareRenderer.cs)
- Replaced hardcoded `_lightX/Y/Z` and `_ambient` with `Lighting` property
- Added `ComputeLighting` method implementing multi-light Blinn-Phong:
  - Ambient + Lambertian diffuse + specular (half-vector)
  - Point light support with direction-from-surface computation
- Camera eye position captured for specular view-direction calculation

---

## Verification

| Project | Result |
|---|---|
| `BMC.LDraw` | ✅ Build succeeded, 0 errors |
| `BMC.LDraw.Render` | ✅ Build succeeded, 0 errors |
| `BMC.LDraw.Render.CLI` | ✅ Build succeeded, 0 errors |

All new parameters default to backward-compatible values — existing callers are unaffected.
