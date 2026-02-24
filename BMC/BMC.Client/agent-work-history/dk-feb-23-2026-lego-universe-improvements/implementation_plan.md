# LEGO Universe Audit Improvements — Implementation Plan

Based on the audit feedback, this plan covers all approved improvements. Items that require schema changes or external data are flagged for discussion.

## Schema Investigation Results

Before scoping, I investigated `BmcDatabaseGenerator.cs` for data availability:

| Audit Item | Schema Status | Impact |
|---|---|---|
| **Set Description** | ❌ No `description` field on `LegoSet`; LDraw/Rebrickable don't provide narrative descriptions | **Dropped** — no data source |
| **Minifig Parts Inventory** | ❌ No `LegoMinifigPart` table; only `partCount` integer | **Needs new table + data import** — significant effort |
| **Colour Year Range / Retired Status** | ❌ No year or retired fields on `BrickColour` | **Dropped** — no data source |
| **Theme Hero Image** | ✅ **Already exists** — `heroImageUrl` with gradient overlay in theme-detail | No action needed |

---

## Proposed Changes

### 1. Instructions Link on Set Detail ⭐

> [!TIP]
> Lowest effort, highest delight. One new button next to Rebrickable/BrickLink.

#### [MODIFY] [set-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.html)

Add an "Instructions" button in the `.external-links` div (around line 62), using the URL pattern `https://www.lego.com/en-us/service/buildinginstructions/{setNumber}`. The `setNumber` includes the suffix (e.g. `42131-1`), so we strip the `-1` suffix for LEGO's search: strip everything from the last `-` onwards.

#### [MODIFY] [set-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.ts)

Add a `getInstructionsUrl()` method that computes the URL from `set.setNumber`.

---

### 2. Universal Search → Direct Item Navigation

#### [MODIFY] [lego-universe.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.html)

Change each `search-result-item` `(mousedown)` handler to navigate directly to the item's detail page instead of the list page. Sets → `/lego/sets/{id}`, Minifigs → `/lego/minifigs/{id}`, Themes → `/lego/themes/{id}`.

#### [MODIFY] [lego-universe.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.ts)

Add `navigateToSearchItem(type, item)` method that routes directly to the detail page for the clicked result. Keep the existing "See all results →" links as-is (they navigate to filtered list pages).

---

### 3. Minifig Rarity Badge ("Appears in X Sets")

Need to verify: does the minifig detail or gallery already have a set-count? The `minifig-detail` loads sets the minifig appears in. The `minifig-gallery` shows minifigs with `partCount` but not a "sets appeared in" count.

#### [MODIFY] [minifig-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/minifig-detail/minifig-detail.component.html)

Add a rarity badge in the minifig info section: "Appears in X set(s)". If X == 1, show "Exclusive" badge. If X <= 3, show "Rare" badge.

#### [MODIFY] [minifig-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/minifig-detail/minifig-detail.component.ts)

Add `getRarityBadge()` and `setCount` property computed after sets load.

#### [MODIFY] [minifig-gallery.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/minifig-gallery/minifig-gallery.component.html)

Show set count on gallery cards if available from the DTO.

> [!IMPORTANT]
> This requires checking whether the `MinifigGalleryItemDTO` already includes a `setCount` field from the server. If not, we may need to add it server-side or compute it client-side from the `LegoSetMinifig` data.

---

### 4. Theme Summary Banner

#### [MODIFY] [theme-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.html)

The year range stat pill already exists (line 40-44). Enhance the hero header to also show total part count across all sets in the theme.

#### [MODIFY] [theme-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.ts)

Add `totalParts` computed property summing `numParts` across all sets in the theme. Display in hero stats row.

---

### 5. Expanded Set Comparison Metrics

#### [MODIFY] [set-comparison.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-comparison/set-comparison.component.ts)

Expand `buildRows()` to include: minifig count, unique parts, unique colours. These will require loading set detail data (parts, minifigs) for each compared set.

> [!IMPORTANT]
> The `SetExplorerItem` interface currently only has `id`, `name`, `setNumber`, `year`, `partCount`, `themeName`, `imageUrl`. We need to check if the comparison service stores full set data or just explorer items. If only explorer items, we may need to load additional data for each compared set.

#### [MODIFY] [set-comparison.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-comparison/set-comparison.component.html)

Add image row at top showing set images side-by-side.

---

### 6. Colour Library → "Parts in this Colour" Drill-Down

#### [MODIFY] [colour-library.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/colour-library/colour-library.component.html)

When a colour is selected (`selectedColour`), show a "View parts in this colour" button that navigates to the parts catalog filtered by colour.

#### [MODIFY] [colour-library.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/colour-library/colour-library.component.ts)

Add `navigateToPartsInColour(colour)` method that routes to `/lego/parts-catalog?colourId={id}`.

#### [MODIFY] [parts-catalog.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/parts-catalog/parts-catalog.component.ts)

Accept `colourId` query param and filter parts by colour on load.

---

### 7. Keyboard Shortcuts

#### [MODIFY] [lego-universe.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.ts)

Add `@HostListener('document:keydown')` for:
- `/` or `S` → focus search input
- `Escape` → close search dropdown

#### [MODIFY] [set-explorer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.ts)

Add keyboard handler for `/` → focus search.

#### [MODIFY] [minifig-gallery.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/minifig-gallery/minifig-gallery.component.ts)

Add keyboard handler for `/` → focus search.

---

### 8. Back-to-Top Button (Universe Dashboard)

#### [MODIFY] [lego-universe.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.html)

Add a floating back-to-top button that appears after scrolling past the hero section.

#### [MODIFY] [lego-universe.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.ts)

Add scroll listener to show/hide the button, and `scrollToTop()` method.

#### [MODIFY] [lego-universe.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.scss)

Add floating button styles matching dark glassmorphism aesthetic.

---

### 9. Breadcrumb + Back Button on Parts Universe

#### [MODIFY] [parts-universe.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/parts-universe/parts-universe.component.html)

Add standard `nav.breadcrumb` matching Set Detail / Minifig Detail pattern: `Universe › Parts Universe`.

#### [MODIFY] [parts-universe.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/parts-universe/parts-universe.component.scss)

Add standard breadcrumb styles (can copy from set-detail).

---

### 10. Standard Back Button on All Universe Pages

Ensure every universe detail/explorer page has a consistent back navigation mechanism. The breadcrumb already serves this purpose on most pages; verify each page has breadcrumbs and that the first crumb always links back to `/lego`.

**Pages to audit/fix:**
- `set-detail` — ✅ has breadcrumb
- `minifig-detail` — needs verification
- `theme-detail` — ✅ has breadcrumb
- `theme-explorer` — needs verification
- `set-explorer` — needs verification
- `minifig-gallery` — needs verification
- `parts-universe` — adding in item 9

---

## Items Requiring Further Discussion

> [!WARNING]
> **Minifig Parts Inventory**: The schema has no `LegoMinifigPart` table. Adding this would require:
> 1. A new table in `BmcDatabaseGenerator.cs`
> 2. A new data import from Rebrickable's `inventory_minifigs` → `inventory_parts` join
> 3. New API endpoints
> 4. New UI in minifig-detail
>
> This is a significant effort (multi-session). I recommend tracking it as a separate feature.

---

## Verification Plan

### Build Verification
- Run `ng build --configuration production` to verify no compilation errors after all changes

### Manual Verification
Since no unit tests exist for any LEGO components (confirmed: zero `.spec.ts` files found), all changes will be verified visually:

1. **Instructions Link**: Navigate to any set detail page → verify "Instructions" button visible → click it → confirm lego.com opens in new tab with correct set number
2. **Search → Detail**: Type a set name in dashboard search → click a result → verify it routes to the set detail page (not the list page)
3. **Minifig Rarity**: Open a minifig detail page → verify "Appears in X sets" badge shows near the minifig info
4. **Theme Summary**: Open a theme detail page → verify total parts count appears in hero stats
5. **Comparison Metrics**: Add 2+ sets to comparison → verify new rows (minifig count, etc.) appear
6. **Colour Drill-Down**: Open colour library → select a colour → verify "View parts" button appears → click it → verify parts catalog opens filtered
7. **Keyboard Shortcuts**: On dashboard, press `/` → verify search input focused. Press `Escape` → verify dropdown closes
8. **Back-to-Top**: Scroll down on Universe dashboard → verify floating button appears → click it → verify scrolls to top
9. **Breadcrumbs**: Navigate to Parts Universe → verify breadcrumb shows `Universe › Parts Universe`
10. **Back Navigation**: Check all universe pages have breadcrumb linking back to `/lego`
