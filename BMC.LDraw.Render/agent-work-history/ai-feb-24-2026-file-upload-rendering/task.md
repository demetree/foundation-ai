# BMC.LDraw.Render — Rendering Expansion

## Phase 1: Core Rendering Quality
- [x] 1.1 Edge/outline rendering (render existing `MeshLine` data)
- [x] 1.2 Smooth shading (compute vertex normals, Gouraud interpolation)
- [x] 1.3 Enhanced lighting (specular highlights, multiple/configurable lights)

## Phase 2: Visual Fidelity
- [x] 2.1 Alpha blending / transparency support (depth-sorted)
- [x] 2.2 Anti-aliasing (SSAA supersampling)
- [x] 2.3 Gradient / custom backgrounds

## Phase 3: Output & Formats
- [x] 3.1 WebP output format
- [x] 3.2 Animated turntable GIF/APNG
- [x] 3.3 SVG vector output

## Phase 4: Advanced Features
- [x] 4.1 STEP meta-command parsing
- [x] 4.2 Step-by-step build instruction renders
- [x] 4.3 Exploded view rendering

## Phase 5: Cross-Cutting Integration
- [x] Update `RenderService` facade with new options
- [x] Update `PartRendererController` endpoints for new features
- [x] Update CLI with new commands/options
- [x] Update Angular `part-renderer` component UI

## Phase 6: File Upload Rendering
- [x] Add `POST /api/part-renderer/render-upload` server endpoint
- [x] Add upload tab to Angular part-renderer component (TS + HTML + SCSS)
