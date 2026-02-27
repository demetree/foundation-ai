# PBR UI Controls

Expose the new ray tracer features in all three rendering UIs. Controls appear conditionally when **Ray Tracer** is selected.

## Controls

| Control | Type | Default | Range | Purpose |
|---------|------|---------|-------|---------|
| PBR Shading | Toggle | On | — | Revert to Blinn-Phong if off |
| Exposure | Slider | 1.0 | 0.5–3.0 | Brighten/darken HDR output |
| Depth of Field | Slider | 0 (off) | 0–5.0 | Aperture radius for bokeh blur |

---

## Server Changes

#### [MODIFY] [RenderService.cs](file:///G:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Add `enablePbr`, `exposure`, `aperture` params to `RenderToPng`, `RenderToWebP`, `RenderToPixels`
- Configure on `RayTraceRenderer` when creating it

#### [MODIFY] [PartRendererController.cs](file:///G:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartRendererController.cs)

- Add `enablePbr`, `exposure`, `aperture` query params to `Render`, `RenderUpload`, `Turntable`, `Exploded`
- Pass through to `RenderService`, include in cache key

#### [MODIFY] [ManualGeneratorHub](server-side SignalR hub)

- Accept the new PBR options from `ManualOptionsDto` and pass to render calls

---

## Client Changes

#### [MODIFY] [part-renderer.component.ts](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/part-renderer/part-renderer.component.ts) + [.html](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/part-renderer/part-renderer.component.html)

- Add `enablePbr`, `exposure`, `aperture` fields
- Add conditional "Ray Trace Options" UI block after renderer selector
- Append params to all render URLs when `rendererType === 'raytrace'`

#### [MODIFY] [catalog-part-detail.component.ts](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.ts) + [.html](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.html)

- Same 3 fields and conditional UI block after the Renderer buttons (line ~246)
- Append params to render URLs

#### [MODIFY] [manual-generator.component.ts](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-generator/manual-generator.component.ts) + [.html](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-generator/manual-generator.component.html)

- Add `enablePbr`, `exposure`, `aperture` to `options` object
- Add conditional controls after the Renderer dropdown (line ~107)
- Options flow through `ManualOptionsDto` → SignalR

#### [MODIFY] [manual-generator-signalr.service.ts](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/manual-generator-signalr.service.ts)

- Add `enablePbr?`, `exposure?`, `aperture?` to `ManualOptionsDto` interface

---

## Verification

- `dotnet build` — 0 errors (server)
- `ng build` — 0 errors (client)
- Manual: select Ray Tracer → PBR controls appear; select Rasterizer → controls hide
