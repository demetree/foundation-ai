# Phase 3 Completion + Phase 4: Collection Integration

Remaining Phase 3 item (Set Timeline on Theme Detail) plus full Phase 4 (Own/Want toggles, badges, dashboard stats, theme completion %).

---

## Proposed Changes

### Phase 3 Remainder — Set Timeline (Theme Detail)

A D3 scatter chart showing each set in the theme plotted by **year (x-axis)** vs **part count (y-axis)**. Each dot is clickable and shows a tooltip with the set name and number. Gives users a visual overview of how the theme evolved over time.

#### [MODIFY] [theme-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.ts)
- Add `import * as d3 from 'd3'`
- Add `ViewChild('timelineChart')` for the SVG container
- Add `buildTimeline()` method called after sets load
- Scatter chart: x = year, y = partCount, radius proportional to partCount, accented dots, tooltip on hover, click navigates to set detail

#### [MODIFY] [theme-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.html)
- Add a new "Set Timeline" section between the Sets table and the Minifigs section
- Contains a section header + a `<div #timelineChart>` SVG container
- Only shown when `sets.length > 1`

#### [MODIFY] [theme-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.scss)
- Add `.timeline-section` glass card styles
- Add D3 tooltip, axis, and dot hover styles

---

### Phase 4 — Collection Integration

#### Step 1: Ownership Cache Service

#### [NEW] [set-ownership-cache.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/set-ownership-cache.service.ts)
- Singleton service that fetches the user's `UserSetOwnershipData` list once and caches it
- Exposes `ownedSetIds$` (Observable\<Set\<number\>\>) and `wantedSetIds$` for quick O(1) lookups
- Methods: `isOwned(id)`, `isWanted(id)`, `toggleOwnership(legoSetId, status)`, `refresh()`
- Uses the existing `UserSetOwnershipService.GetUserSetOwnershipList()` API
- Status values: `"owned"`, `"wanted"`

#### Step 2: Set Detail + Minifig Detail Heroes

#### [MODIFY] [set-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.ts)
- Inject `SetOwnershipCacheService`
- Add `isOwned` / `isWanted` getters
- Add `toggleOwn()` and `toggleWant()` methods that call the cache service

#### [MODIFY] [set-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.html)
- Add "Own" and "Want" toggle buttons in the hero action bar (alongside Compare)
- Show filled heart / check icon when active

#### [MODIFY] [set-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.scss)
- Add `.ext-btn.own` and `.ext-btn.want` active state styles (green for owned, pink for wanted)

#### Step 3: Set Explorer + Minifig Gallery Badges

#### [MODIFY] [set-explorer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.ts)
- Inject `SetOwnershipCacheService`
- Subscribe to ownership sets for badge display

#### [MODIFY] [set-explorer.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.html)
- Add owned/wanted badge overlays on grid cards and table rows

#### [MODIFY] [set-explorer.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.scss)
- Add `.owned-badge` and `.wanted-badge` pill styles

#### Step 4: Lego Universe Dashboard Stats

#### [MODIFY] [lego-universe.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.ts)
- Inject `SetOwnershipCacheService`
- Compute `ownedCount`, `totalSets`, `completionPercentage`

#### [MODIFY] [lego-universe.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.html)
- Add a "Your Collection" stats card showing owned / total / completion %

#### [MODIFY] [lego-universe.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.scss)
- Style the collection stats card

#### Step 5: Theme Completion %

#### [MODIFY] [theme-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.ts)
- Inject `SetOwnershipCacheService`
- Compute `ownedInTheme` and `themeCompletion` percentage

#### [MODIFY] [theme-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.html)
- Add completion % stat pill in the header stats row

---

## Verification Plan

### Automated Tests
- `npx ng build --configuration production` — zero errors after each step

### Manual Verification
- Navigate to a Theme Detail page → visible scatter chart with correct year/part data
- Click a dot → navigates to the set detail page
- Toggle Own/Want on Set Detail → button state persists across navigation
- Set Explorer grid/table shows owned/wanted badges
- Lego Universe dashboard shows collection stats card
- Theme Detail header shows completion % stat pill
