# Part Rendering for Parts Catalog

## Context

The Parts Catalog currently shows a **FontAwesome icon** placeholder for each part (cube, plate, gear etc.) based on the part type name. Parts have `widthLdu`, `heightLdu`, `depthLdu` dimension data from the LDraw import but no pre-rendered images.

## Approach: Inline Isometric SVG Brick Renderer

Generate a **proportional isometric 3D brick** using each part's actual LDU dimensions. This creates a unique visual for every part based on its real proportions — wide parts look wide, tall parts look tall, etc.

```
     ___________
    /          /|
   /          / |      ← Top face (lightest)
  /_________ /  |
  |          |   |     ← Front face (mid tone)  
  |          |  /      ← Right face (darkest)
  |__________|/
```

### Features
- **Proportional rendering** — each brick's visual matches its W×H×D ratio
- **Part-type colouring** — bricks are amber, plates are blue, tiles are green, technic is purple, etc.
- **Isometric 3D** — simple three-face projection with lighting (top=light, front=mid, side=dark)
- **Stud dots** on top face for brick/plate types, smooth for tiles
- **Fallback** — parts with zero LDU dimensions get a stylized icon instead

## Proposed Changes

### Parts Catalog Component

#### [MODIFY] [parts-catalog.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.ts)
- Add `getPartColour(part)` method returning a colour palette (top/front/side/stud hex values) based on part type
- Add `getIsometricPath(part)` computing SVG polygon points from LDU dimensions
- Add `shouldShowStuds(part)` — true for brick/plate types

#### [MODIFY] [parts-catalog.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.html)
- Replace `<div class="card-icon"><i ...>` with inline SVG isometric brick rendering
- Add stud dots on top face for applicable types
- Keep icon fallback for parts with no dimensions

#### [MODIFY] [parts-catalog.component.scss](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.scss)
- Style the new SVG container with proper sizing and hover effects
- Add subtle shadow beneath the rendered brick

## Verification Plan

### Build Verification
- Run `npx ng build` to confirm zero errors

### Visual Verification
- User reviews the `/parts` page to see rendered bricks with proper proportions and colours
