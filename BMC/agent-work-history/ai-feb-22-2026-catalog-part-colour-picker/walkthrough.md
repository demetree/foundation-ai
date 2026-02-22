# Walkthrough — Catalog Part Colour Picker

## Feature Overview

Added an interactive **Colour Preview** panel to the `catalog-part-detail` component. Users can click a colour swatch to preview the part rendered in that colour in both the Three.js 3D viewer and the SVG isometric fallback. A mode toggle switches between "Part Colours" (colours this part is known to exist in) and "All Colours" (the full `BrickColour` table).

---

## Changes Made

### `catalog-part-detail.component.ts`

#### New Imports
- `BrickColourService` and `BrickColourData` from `brick-colour.service`

#### New State Variables
- `selectedColour: BrickColourData | null` — the currently selected colour
- `colourMode: 'part' | 'all'` — which swatch set is shown (defaults to `'part'`)
- `partColours: BrickColourData[]` — merged, deduplicated colour list for "Part Colours" mode
- `allColours: BrickColourData[]` — full colour table, loaded lazily on first switch to "All Colours" mode
- `isLoadingAllColours`, `allColoursSearch` — loading and filter state
- `partColoursSourceLoaded`, `setPartsSourceLoaded` — coordination flags for the merge

#### New Methods

**`buildPartColourList()`**
Merges colours from two sources into a deduplicated `partColours` list:
1. `BrickPartColour` records (direct part-colour mappings — often sparse)
2. `LegoSetPart` records (colours the part appears in across all sets — much richer)

Uses a `Map<id, BrickColourData>` for deduplication. Called from the `finally` block of both `loadColours()` and `loadSetParts()` — only runs the merge once both flags are `true`.

**`setColourMode(mode)`**
Switches between `'part'` and `'all'` modes. Triggers `loadAllColours()` on first switch to `'all'`.

**`loadAllColours()`**
Fetches all active colours via `BrickColourService.GetBrickColourList({ active: true, pageSize: 500 })`. Cached after first load.

**`selectColour(colour)`**
Sets `selectedColour`. Clicking the same swatch again clears the selection and calls `resetSceneColours()`. Calls `applyColourToScene()` if the scene is ready.

**`applyColourToScene(hexRgb)`**
Traverses the THREE.js scene and updates all `MeshStandardMaterial` colours. Line/edge materials are left unchanged to preserve part outlines.

**`resetSceneColours()`**
Removes mesh/group children from the scene root and reloads the model to restore original colours.

**`getSwatchHex(colour)`**
Safe hex formatter — handles `null`, `undefined`, and both `'RRGGBB'` / `'#RRGGBB'` formats.

**`filteredAllColours` getter**
Filters `allColours` by `allColoursSearch`.

**`visibleColourCount` getter**
Returns `partColours.length` or `filteredAllColours.length` depending on mode.

#### Modified Methods

**`getPartColour()`**
Now returns the selected colour's hex (with lighter top face and darker side face derived via `THREE.Color.lerp`) when a colour is selected, so the SVG isometric fallback also reflects the selection.

**`buildFallbackGeometry()`**
Uses the selected colour for the fallback box material when one is active.

**`initScene()`**
- Removed `this.scene.background` (no background colour set — canvas is transparent)
- Removed `this.scene.fog` (fog requires a background colour)
- Added `this.renderer.setClearColor(0x000000, 0)` to make the WebGL clear colour fully transparent

---

### `catalog-part-detail.component.html`

Replaced the old plain-text "Available Colours" card with a **Colour Preview** card containing:
- **Mode toggle** (Part Colours / All Colours) in the card header
- **Search box** (All Colours mode only)
- **Loading spinner** while the full colour list is fetching
- **Swatch grid** — 24px circular buttons coloured with `hexRgb`, with `.selected`, `.transparent`, and `.metallic` modifier classes
- **Selected colour label** — shows the colour name with a mini swatch and an ✕ clear button
- **Hint text** — "Click a swatch to preview the part in that colour" when nothing is selected

The Colour Preview card was moved to the **top of the right-hand info column** (above Part Information, Dimensions, and Connectors).

---

### `catalog-part-detail.component.scss`

New styles added:
- `.colour-picker-header` — flex header with mode toggle on the right
- `.colour-mode-toggle` / `.colour-mode-btn` — pill-style toggle with active state
- `.colour-picker-body` — flex column body
- `.colour-search` — search input with focus ring
- `.colour-loading` — loading state
- `.colour-swatch-grid` — scrollable flex-wrap grid (max-height 180px)
- `.colour-swatch-dot` — 24px circular button with hover scale, focus ring, `.selected` ring, `.transparent` checkerboard, `.metallic` shimmer overlay
- `.colour-selected-label` / `.colour-selected-swatch` / `.colour-selected-name` / `.colour-clear-btn` — selected colour display row
- `.colour-hint` — italic hint text

Transparency changes:
- `.viewer-container` — `background: transparent`
- `.fallback-viewer` — `background: transparent`
- `.viewer-loading` — `background: var(--bmc-bg-card)` with `opacity: 0.9`

---

## Design Decisions

### Double-Defensive Part Colour List
The `BrickPartColour` table often has incomplete master data. By also mining `LegoSetPart` records (which are populated from the Rebrickable import), the "Part Colours" list is much richer and more useful in practice.

### Lazy Loading of All Colours
The full `BrickColour` table is only fetched when the user first switches to "All Colours" mode, and is cached for subsequent switches. This avoids an unnecessary API call on page load.

### Colour Reset via Model Reload
When the colour selection is cleared, the simplest correct approach is to remove the model from the scene and reload it. This restores the original LDraw colour definitions without needing to track the original material colours.
