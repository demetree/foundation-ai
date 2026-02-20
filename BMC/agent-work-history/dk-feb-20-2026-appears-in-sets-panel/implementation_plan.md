# Appears In Sets — Part Detail Panel

Add an "Appears In Sets" panel to the `catalog-part-detail` component showing every set containing the part, with colour, quantity, and a sortable/filterable table.

## Data Model

`BrickPartData.LegoSetParts` (lazy-loading getter) returns `LegoSetPartData[]` with `includeRelations=true`. Each item has:
- `legoSet` nav property → `name`
- `brickColour` nav property → `name`, `hexRgb`
- `quantity`, `isSpare`

**No server changes needed** — the API and data services already exist.

## Proposed Changes

### catalog-part-detail Component

---

#### [MODIFY] [catalog-part-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.ts)

- Add `setParts: LegoSetPartData[]`, `isLoadingSetParts`, `setPartsSearch`, `setPartsSortField`, `setPartsSortDir` properties
- Add `loadSetParts()` method using `this.part.LegoSetParts`
- Add `filteredSetParts` getter: text search on set name + colour name
- Add `sortedSetParts` getter: sort by field/direction
- Add `sortSetParts(field)` toggle method
- Call `loadSetParts()` from `loadPart()` alongside existing `loadConnectors` / `loadColours`
- Import `LegoSetPartData`

---

#### [MODIFY] [catalog-part-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.html)

Add a new panel after the Colours card, **spanning full width below the existing two-column layout**:
- Header: icon, "Appears In Sets", count badge
- Search input for quick text filter (set name / colour name)
- Sortable table columns: **Set** (name), **Colour** (swatch + name), **Qty**, **Spare?**
- Click column headers to sort ↑/↓
- Empty state message when no set parts
- Loading spinner while `isLoadingSetParts` is true

---

#### [MODIFY] [catalog-part-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.scss)

- `sets-panel` full-width card below info panels
- `sets-search` input styling
- `sets-table` with `sets-header-row` (clickable, sort indicator) and `sets-row` data rows
- `colour-swatch` circle
- `spare-badge` chip
- Scrollable body with virtualized max-height (~400px)

## Verification Plan

### Automated Tests
- `npx ng build --configuration=production` — verify zero compilation errors
