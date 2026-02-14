# BMC Premium UI Components — Walkthrough

## What Was Built

Three premium UI components for the BMC.Client Angular application:

### 1. Parts Catalog (`/parts`)
- **Isometric SVG brick rendering** — each part gets a proportional 3D brick based on parsed stud dimensions from `ldrawTitle` (e.g., "Brick 2 x 4" → wide brick, "Plate 1 x 1" → tiny flat plate)
- **19 category colour palettes** — bricks (amber), plates (blue), tiles (green), slopes (red), technic (purple), gears (gold), etc.
- **Stud dots** on top face for brick/plate types, scaled to actual stud count
- **Rich card content** — shows `ldrawTitle`, LDraw ID, category badge, author, keywords, mass
- Grid and list views with search, category sidebar, part type filters, pagination

### 2. Colour Library (`/colours`)
- Swatch grid with hex colour rendering  
- Fixed double-hash bug (`##1E2A34` → `#1E2A34`) via `normalizeHex()` helper
- Detail panel with colour metadata

### 3. System Health (`/system-health`)
- Ported from Scheduler.Client, adapted for BMC's single-server architecture
- Health cards, table statistics modal, authenticated users panel

## Key Files Modified

| File | Changes |
|------|---------|
| [parts-catalog.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.ts) | Isometric SVG renderer with dimension parser, colour palettes, stud placement |
| [parts-catalog.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.html) | Inline SVG bricks, improved card content with ldrawTitle/author/keywords |
| [parts-catalog.component.scss](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.scss) | SVG container, hover animations, keyword/mass badge styles |
| [colour-library.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/colour-library/colour-library.component.ts) | normalizeHex/getHexDisplay fix for double-hash |
| [colour-library.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/colour-library/colour-library.component.html) | Template updated for hex display |
| [system-health.component.*](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/system-health/) | New component (TS, HTML, SCSS) |
| [system-health.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/system-health.service.ts) | New service adapted from Scheduler |
| [app-routing.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app-routing.module.ts) | Routes for /parts, /colours, /system-health |
| [app.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.module.ts) | Component declarations |

## Bugs Fixed

1. **Colour swatch rendering** — hex values from DB already include `#` prefix; code was prepending another `#`, producing invalid CSS `##1E2A34`
2. **SVG not rendering** — `hasDimensions()` gate returned false because LDU fields are unpopulated; replaced with always-on rendering using type-based defaults
3. **All SVGs identical** — was reading `partType.name` (always "Part") instead of `brickCategory.name` (the descriptive category); also added title dimension parser

## Verification

- `npx ng build` — **exit code 0**, zero errors across all changes
- Visual verification via screenshots at each iteration
