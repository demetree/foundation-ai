# Phase 4: Advanced Features — Implementation Plan

Full model rendering (.ldr/.mpd) already works through existing `GeometryParser.ParseMpd` and `GeometryResolver.ResolveFile`. Phase 4 adds step-by-step instruction rendering and exploded views.

---

## 4.1 — STEP Meta-Command Parsing

The LDraw `0 STEP` meta-command marks building step boundaries. Currently ignored by `ParseSingle`.

### [MODIFY] [LDrawGeometry.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Models/LDrawGeometry.cs)

- Add `List<int> StepBreaks` — indices into `SubfileReferences` marking where each step ends
- A model with 3 steps and 10 subfile refs might have `StepBreaks = [3, 7, 10]`

### [MODIFY] [GeometryParser.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Parsers/GeometryParser.cs)

- In `ParseSingle`, detect `0 STEP` lines and record the current subfile reference count as a step break
- Add a final implicit step for any trailing geometry after the last `0 STEP`

---

## 4.2 — Step-by-Step Instruction Rendering

### [MODIFY] [GeometryResolver.cs](file:///g:/source/repos/Scheduler/BMC.LDraw/Parsers/GeometryResolver.cs)

- Add `ResolveFileUpToStep(filePath, stepIndex, colourCode)` — resolves only subfile refs up to (and including) the given step
- Reuse existing `ResolveRecursive` by limiting the subfile range via step breaks

### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Add `GetStepCount(inputPath)` — parses file and returns step count
- Add `RenderStep(inputPath, stepIndex, width, height, ...)` — renders a single step (cumulative, showing all parts up to that step)
- Add `RenderAllSteps(inputPath, width, height, ...)` — returns `List<byte[]>` of PNG frames for all steps

---

## 4.3 — Exploded View Rendering

### [NEW] [ExplodedViewBuilder.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/ExplodedViewBuilder.cs)

- Takes a resolved `LDrawMesh` and the original `LDrawGeometry` subfile refs
- Computes per-subfile bounding box centroids and model centroid
- Applies radial offset from centroid to spread parts apart (configurable `explosionFactor`)
- Returns a new `LDrawMesh` with transformed triangle/edge positions

### [MODIFY] [RenderService.cs](file:///g:/source/repos/Scheduler/BMC.LDraw.Render/RenderService.cs)

- Add `RenderExplodedView(inputPath, explosionFactor, ...)` convenience method

---

## Verification Plan

### Build
```
dotnet build BMC.LDraw/BMC.LDraw.csproj
dotnet build BMC.LDraw.Render/BMC.LDraw.Render.csproj
dotnet build BMC.LDraw.Render.CLI/BMC.LDraw.Render.CLI.csproj
```
