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
- [ ] 3.1 WebP output format
- [ ] 3.2 Animated turntable GIF/APNG
- [ ] 3.3 SVG vector output

## Phase 4: Advanced Features
- [ ] 4.1 Full model rendering (.ldr/.mpd multi-part scenes)
- [ ] 4.2 Step-by-step build instruction renders
- [ ] 4.3 Exploded view rendering

## Cross-cutting
- [x] Update `RenderService` facade with new options
- [ ] Update `PartRendererController` endpoints for new features
- [ ] Update CLI with new commands/options
- [ ] Update Angular `part-renderer` component UI
