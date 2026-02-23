# Phase 2: Explorer & Detail Upgrades — Implementation Plan

> 5 items that enhance browsing, navigation and detail pages.

---

## 1. Grid/Table View Toggle in Set Explorer

Currently the Set Explorer renders sets in a card grid via CDK virtual scroll (`cardRows` → `*cdkVirtualFor`). Add a table view as an alternative.

### Proposed Changes

#### [MODIFY] [set-explorer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.ts)
- Add `viewMode: 'grid' | 'table' = 'grid'` field
- Persist choice in `localStorage` so it survives reloads

#### [MODIFY] [set-explorer.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.html)
- Add grid/table toggle buttons in `header-actions` (icons: `fa-th-large` / `fa-list`)
- Wrap card viewport in `*ngIf="viewMode === 'grid'"`
- Add a `<table>` section in `*ngIf="viewMode === 'table'"` with columns: Image, Set #, Name, Year, Parts, Theme
- Table uses the same `filteredSets` data source — no CDK virtual scroll needed for table (rows are lightweight)

#### [MODIFY] [set-explorer.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.scss)
- Add `.view-toggle` button styles, `.sets-table` and `.sets-row` styles matching the theme-detail table look

---

## 2. Theme Detail — Minifig Section

Theme Detail currently shows sets but no minifigs. Add a minifig grid section below the sets table.

### Proposed Changes

#### [MODIFY] [theme-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.ts)
- Import `LegoSetMinifigService` and `LegoSetMinifigData`
- Add `minifigs` array and `minifigsLoading` flag
- After sets load, collect unique minifigs across all sets by calling `GetLegoSetMinifigList` filtered by legoTheme's sets
- Alternatively, iterate loaded sets' `LegoSetMinifigs` lazy relations (may be slow for large themes)

> [!IMPORTANT]
> Need to decide: fetch minifigs via server call (efficient) or lazy-load from each set (no server change). I'll use the server approach — `GetLegoSetMinifigList` by set IDs — to avoid N+1 lazy loads.

#### [MODIFY] [theme-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.html)
- Add "Minifigs in this Theme" section after the sets table, with a card grid of minifigs
- Each card shows image, name, fig number, click navigates to `/lego/minifigs/:id`

#### [MODIFY] [theme-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.scss)
- Add `.minifig-section`, `.minifig-grid`, `.minifig-card` styles matching existing set card styles

---

## 3. Theme Detail — CDK Virtual Scroll for Large Themes

Currently `loadSets` caps at `pageSize: 200`. Themes like "Star Wars" have 800+ sets. Instead of pagination (which loses client-side search), load all sets and use CDK virtual scroll for rendering performance.

### Proposed Changes

#### [MODIFY] [theme-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.ts)
- Remove `pageSize: 200` cap — load all sets (`pageSize: 5000` or remove limit)
- Add `searchQuery` field + `filteredSets` getter for client-side search
- Compute `rowHeight` for the virtual scroll item size

#### [MODIFY] [theme-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.html)
- Add a quick-search input above the sets table
- Replace `<table>` with a `<cdk-virtual-scroll-viewport>` using `*cdkVirtualFor` on set rows
- Show "X of Y sets" count reflecting search filter

#### [MODIFY] [theme-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.scss)
- Add `.sets-search` input styles, viewport height, and virtual row styles

---

## 4. Parts Catalog — "Used in X Sets" Count Link

The catalog part detail already has an "Appears In Sets" panel (`setParts` array with `filteredSetParts`). The audit item was to add a count link from the **parts leaderboard/catalog cards** — i.e., show the count on list views and make it clickable.

### Proposed Changes

#### [MODIFY] [parts-leaderboard.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/parts-leaderboard/parts-leaderboard.component.html)
- In each leaderboard row, add a "used in X sets" column/badge if set count data is available
- Click navigates to the part detail page

> [!NOTE]
> If set count is not on the existing DTO, this may need a server-side change to add `setCount` to the parts leaderboard response. Will verify during implementation.

---

## 5. Keyboard Shortcuts

Add keyboard shortcuts to improve power-user navigation across the Lego Universe.

| Key | Action | Scope |
|-----|--------|-------|
| `S` | Focus search/universal search | Universe & Explorer |
| `←` / `→` | Previous/Next in carousels | Universe dashboard |
| `Esc` | Close modals / clear search | Global |

### Proposed Changes

#### [MODIFY] [lego-universe.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.ts)
- Add `@HostListener('document:keydown', ['$event'])` handler
- `S` → focus universal search input
- `←` / `→` → scroll carousel strips
- `Esc` → blur search / close any popover

#### [MODIFY] [set-explorer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.ts)
- Add `@HostListener` for `S` → focus search input, `Esc` → clear search

---

## Verification Plan

### Automated
- `npx ng build --configuration production` — must pass with zero errors after each item

### Manual
- Set Explorer: toggle between grid/table views, verify data consistency
- Theme Detail: navigate to Star Wars theme, verify pagination + minifig section
- Parts Leaderboard: verify set count displays and links to part detail
- Keyboard shortcuts: press S, ←, →, Esc across Universe and Explorer pages
