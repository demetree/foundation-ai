# PBR UI Controls — Walkthrough

## What Changed

Three new controls are conditionally shown when **Ray Tracer** renderer is selected:

| Control | Default | Range |
|---------|---------|-------|
| PBR Shading toggle | On | — |
| Exposure slider | 1.0 | 0.5–3.0 |
| Depth of Field slider | Off (0) | 0–5.0 |

## Files Modified

### Server (5 files)

| File | Change |
|------|--------|
| [RenderService.cs](file:///G:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs) | Added `enablePbr`, `exposure`, `aperture` to all render methods (8 overloads). Configures `RayTraceRenderer.EnablePbr`, `.Exposure`, and `Camera.Aperture` |
| [PartRendererController.cs](file:///G:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartRendererController.cs) | Added query params to `Render` + `RenderUpload`. Included in cache key. Clamped values |

### Client (6 files)

| File | Change |
|------|--------|
| [part-renderer.component.ts](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/part-renderer/part-renderer.component.ts) | PBR fields + URL params for render and upload |
| [part-renderer.component.html](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/part-renderer/part-renderer.component.html) | Conditional "Ray Trace Options" group after renderer buttons |
| [catalog-part-detail.component.ts](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.ts) | PBR fields + URL params |
| [catalog-part-detail.component.html](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.html) | Conditional PBR subsection after renderer toggle |
| [manual-generator.component.ts](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-generator/manual-generator.component.ts) + [.html](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/manual-generator/manual-generator.component.html) | PBR options in `options` object + conditional controls after renderer dropdown |
| [manual-generator-signalr.service.ts](file:///G:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/manual-generator-signalr.service.ts) | `ManualOptionsDto` extended with 3 optional fields |

## Build Verification

- **Server**: `dotnet build BMC.Server` → **Build succeeded, 0 errors**
- **Client**: `ng build` → **Bundle generated, 0 errors** (1 pre-existing CSS warning)
