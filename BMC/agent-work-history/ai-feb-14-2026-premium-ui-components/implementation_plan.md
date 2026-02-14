# Premium BMC UI Components

Build three best-of-breed custom UI pages to replace the placeholder sidebar routes, leveraging the BMC dark theme and existing data services.

## Proposed Changes

---

### Parts Catalog (`/parts`)

A premium card-based parts browser with a category filter sidebar, instant search, and dimensional metadata display — designed to feel like a high-end product catalog.

**Design:**
- **Left panel**: Collapsible category filter sidebar with animated category chips and part type filter
- **Main area**: Responsive masonry-style card grid with glassmorphism cards
- **Each card shows**: Part name, LDraw ID, category badge, dimensions (W×H×D in LDU), part type icon, connector count badge
- **Search bar**: Debounced full-text search using `anyStringContains` parameter
- **Stats header**: Total parts count, active filters summary, grid/list view toggle
- **Empty/loading states**: Skeleton cards with shimmer animation
- **Click behaviour**: Navigate to the existing auto-generated detail page (`/brickparts/:id`)

**Data flow:**
- `BrickPartService.GetBrickPartList()` with query params for filtering, paging (`pageSize: 24`)
- `BrickCategoryService.GetBrickCategoryList()` for filter sidebar categories
- `PartTypeService.GetPartTypeList()` for part type filter chips

#### [NEW] [parts-catalog.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.ts)
#### [NEW] [parts-catalog.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.html)
#### [NEW] [parts-catalog.component.scss](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.scss)

---

### Colour Library (`/colours`)

A visual swatch grid that shows every brick colour as its actual rendered colour — the centrepiece of the BMC visual experience.

**Design:**
- **Swatch grid**: Each colour renders as a rounded square showing the actual `hexRgb` colour with edge colour border
- **Swatch card info**: Colour name, LDraw code, transparency/metallic badges, finish type
- **Colour detail panel**: Click a swatch to expand an inline detail panel showing: full hex values, alpha, luminance, edge colour preview, finish type, LEGO colour ID, related parts count
- **Filter bar**: Search by name, group by finish type (dropdown), toggle transparent/metallic filters
- **Header stats**: Total colours count, transparent count, metallic count
- **Micro-animations**: Swatch hover scale + glow effect using the swatch's own colour

**Data flow:**
- `BrickColourService.GetBrickColourList()` with `includeRelations: true` to get `colourFinish` nav property
- `ColourFinishService.GetColourFinishList()` for finish type grouping

#### [NEW] [colour-library.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/colour-library/colour-library.component.ts)
#### [NEW] [colour-library.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/colour-library/colour-library.component.html)
#### [NEW] [colour-library.component.scss](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/colour-library/colour-library.component.scss)

---

### System Health (`/system-health`)

Port the existing Scheduler.Client system health monitoring component to BMC.Client.

#### [NEW] [system-health.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/system-health/system-health.component.ts)
#### [NEW] [system-health.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/system-health/system-health.component.html)
#### [NEW] [system-health.component.scss](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/system-health/system-health.component.scss)
#### [NEW] [system-health.service.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/services/system-health.service.ts)

Copy the full component + service from Scheduler.Client. Adapt API base URL to use BMC.Server's `/api/SystemHealth` endpoint (which exists on all Foundation-based servers).

---

### Routing & Module Registration

#### [MODIFY] [app-routing.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app-routing.module.ts)
- Add routes: `/parts`, `/colours`, `/system-health`

#### [MODIFY] [app.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.module.ts)
- Declare: `PartsCatalogComponent`, `ColourLibraryComponent`, `SystemHealthComponent`
- Import: `SystemHealthService`

#### [MODIFY] [dashboard.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/dashboard/dashboard.component.ts)
- Fix dashboard quick action routes to match the new `/parts`, `/colours` routes (already correct)

## Verification Plan

### Automated Tests
- `ng build --configuration development` to verify compilation

### Manual Verification
- Navigate to `/parts` — verify cards load with category filters, search works, cards link to detail pages
- Navigate to `/colours` — verify colour swatches render with correct hex colours, filters work
- Navigate to `/system-health` — verify server metrics load with auto-refresh
- Verify all sidebar nav items navigate correctly
