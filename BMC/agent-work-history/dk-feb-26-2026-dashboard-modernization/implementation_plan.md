# Dashboard Modernization

The current dashboard shows 4 RowCount stats, 3 quick actions, and a static welcome banner. The sidebar has 11 destinations but the dashboard only surfaces 3. Time to make it a proper command centre.

## Current State

| Section | Content |
|---------|---------|
| Stats | Parts, Categories, Colours, Projects (4 cards) |
| Quick Actions | Browse Parts, My Projects, Colour Library (3 links) |
| Welcome Banner | Static text + floating bricks decoration |

**Missing from dashboard**: Universe, Set Explorer, Part Renderer, Manual Builder, Minifig Gallery, My Collection, Themes, AI Assistant, System Health

## Proposed Changes

### [MODIFY] [dashboard.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/dashboard/dashboard.component.ts)

**Stats → 7 cards** (use `forkJoin` for cleaner parallel loading):

| Stat | API | Route |
|------|-----|-------|
| LEGO Sets | `/api/LegoSets/RowCount` | `/lego/sets` |
| Parts | `/api/BrickParts/RowCount` | `/parts` |
| Colours | `/api/BrickColours/RowCount` | `/colours` |
| Themes | `/api/LegoThemes/RowCount` | `/lego/themes` |
| Minifigures | `/api/Minifigs/RowCount` | `/lego/minifigs` |
| Projects | `/api/Projects/RowCount` | `/projects` |
| Collection | `/api/UserCollectionSets/RowCount` | `/my-collection` |

**Quick Access → Full grid** covering all sidebar + key destinations:

| Card | Icon | Route | Description |
|------|------|-------|-------------|
| Universe | `fa-globe` | `/lego` | The LEGO Universe — sets, themes, minifigs |
| Parts Catalog | `fa-cubes` | `/parts` | Browse 79,000+ parts |
| Part Renderer | `fa-camera` | `/part-renderer` | 3D rendering & ray tracing |
| Manual Builder | `fa-book` | `/manual-generator` | Step-by-step instructions |
| Set Explorer | `fa-search` | `/lego/sets` | Search and compare sets |
| Theme Explorer | `fa-layer-group` | `/lego/themes` | Browse by theme |
| Minifig Gallery | `fa-child-reaching` | `/lego/minifigs` | Complete minifig catalogue |
| My Collection | `fa-box-open` | `/my-collection` | Track your sets |
| Colour Library | `fa-palette` | `/colours` | Browse brick colours |
| Compare Sets | `fa-exchange-alt` | `/lego/compare` | Side-by-side comparisons |
| AI Assistant | `fa-robot` | `/ai` | Intelligent search |

**Remove**: Static "Welcome to BMC" banner (redundant now that the dashboard is content-rich)

**Add**: Personalized greeting with user's display name from `AuthService`

### [MODIFY] [dashboard.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/dashboard/dashboard.component.html)

- Replace header subtitle with "Welcome back, {displayName}"
- Expand stats grid to 7 cards with skeleton loading states
- Replace 3 quick actions with full 11-card grid using gradient icon backgrounds (matching the landing page feature card style)
- Remove the welcome banner section entirely

### [MODIFY] [dashboard.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/dashboard/dashboard.component.scss)

- Add gradient icon classes for the quick-access grid (reuse pattern from landing page)
- Add skeleton loading styles  
- Improve responsive breakpoints for the expanded grid
- Keep all existing theme-token usage (already good)

## Verification Plan

### Automated Tests
- `ng build --configuration development` — verify compilation

### Manual Verification
- Visual review after server restart
