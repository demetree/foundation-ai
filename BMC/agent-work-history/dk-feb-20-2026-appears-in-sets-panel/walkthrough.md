# Appears In Sets Panel — Walkthrough

## What Changed

Added a new **"Appears In Sets"** panel to the catalog-part-detail view, showing every LEGO set that contains the current part.

### Files Modified

| File | Changes |
|------|---------|
| [catalog-part-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.ts) | `loadSetParts()`, `filteredSetParts`, `sortedSetParts` getters, `sortSetParts()` toggle, `getSortIcon()` |
| [catalog-part-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.html) | Full-width panel with sortable table, search input, colour swatches, spare badges |
| [catalog-part-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.scss) | Sets panel card, sticky header, scrollable body (420px max), grid layout, responsive |

### Features

- **Lazy loading** — uses existing `BrickPartData.LegoSetParts` getter (no server changes)
- **Sortable columns** — Set name, Colour, Qty, Spare — click headers to toggle ↑/↓
- **Text search** — filters on set name and colour name
- **Colour swatches** — 14px circles using `brickColour.hexRgb`
- **Spare badge** — amber chip for spare parts
- **Scrollable** — 420px max-height with sticky header for large datasets
- **Full-width** — spans both grid columns via `grid-column: 1 / -1`

## Build Verification

```
Application bundle generation complete. [31.747 seconds]
```

✅ Production build passes — zero new errors or warnings.
