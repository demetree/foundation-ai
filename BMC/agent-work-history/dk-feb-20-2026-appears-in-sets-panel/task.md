# Appears In Sets Panel + Color DNA Fix

## Sets Panel
- [x] Initial implementation (TS, HTML, SCSS)
- [x] Move under 3D viewer, fix colour swatch, remove Spare column
- [x] Add Set # column, fix `hexRgb` missing from `BrickColour.CreateMinimalAnonymous`
- [x] Make rows clickable → navigate to `/lego/sets/:id`

## Color DNA Panel
- [x] Audit heatmap labels — `rp.name` is part number, not description
- [x] Fix client `buildHeatmapData` to use `ldrawTitle || name`
- [x] Fix server `BuildHeatmapData` to use `LdrawTitle` with fallback
- [x] Always rebuild heatmap client-side to avoid stale server cache
- [x] Reduce font/cell sizes, trim labels
- [x] Save work history
