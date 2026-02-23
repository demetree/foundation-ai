# Phase 3: Relationship & Discovery Features

Implements three Set Detail / Explorer features using **client-side data only** (no server changes). All leverage the cached `SetExplorerItem[]` dataset (26K sets).

## Proposed Changes

### 1. Similar Sets Recommendation Engine

Adds a "Similar Sets" card grid to the Set Detail page, recommending 8-12 related sets scored by:

| Signal | Weight | Logic |
|--------|--------|-------|
| Same theme | 40pts | Exact themeId match |
| Year proximity | 0-30pts | `30 - min(abs(yearDiff), 30)` |
| Part count proximity | 0-20pts | `20 - min(abs(countRatio), 20)` where ratio = log-scale diff |
| Has image | 10pts | Prefer sets with images for visual appeal |

> [!NOTE]
> Excludes the current set and any subset-related sets (already shown in the Related tab). Scoring runs client-side using the cached explorer data.

#### [MODIFY] [set-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.ts)
- Inject `SetExplorerApiService`
- Add `similarSets: SetExplorerItem[] = []` and `similarSetsLoading = true`
- Add `loadSimilarSets()` method that scores all sets against the current set
- Call from `loadSet()` after set is available

#### [MODIFY] [set-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.html)
- Add "Similar Sets" section below the tabs section
- Card grid with image, name, set number, year, part count, theme badge
- Clickable to navigate to that set

#### [MODIFY] [set-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.scss)
- Add `.similar-sets-section`, `.similar-card`, responsive grid styles

---

### 2. Set Comparison Feature

A comparison tool accessed from any set card or detail page. Users can add up to 4 sets, then view them side-by-side in a dedicated panel.

#### [NEW] [set-comparison.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/set-comparison.service.ts)
- Injectable singleton with `BehaviorSubject<SetExplorerItem[]>`
- `addSet(set)`, `removeSet(id)`, `clearAll()`, `isInComparison(id)` methods
- Max 4 sets enforced
- Persists selection in `sessionStorage`

#### [MODIFY] [set-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.ts)
- Inject `SetComparisonService`
- Add "Compare" button in hero section

#### [MODIFY] [set-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.html)
- Add compare button to hero external links area

#### [MODIFY] [set-explorer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.ts)
- Inject `SetComparisonService`
- Add `toggleCompare(set)` method and comparison bar at bottom

#### [MODIFY] [set-explorer.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.html)
- Add compare checkbox/button overlay on set cards
- Add sticky comparison bar at the bottom when sets are selected
- Comparison bar shows thumbnails of selected sets + "Compare" button

#### [NEW] [set-comparison.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-comparison/set-comparison.component.ts)
- Route: `/lego/compare`
- Side-by-side table comparing: image, name, set number, year, parts, theme, minifigs count
- Highlight differences (e.g., min/max parts)

#### [NEW] [set-comparison.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-comparison/set-comparison.component.html)
#### [NEW] [set-comparison.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-comparison/set-comparison.component.scss)

#### [MODIFY] [lego-universe.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/lego-universe/lego-universe.component.ts)
- Add route for `/lego/compare`

---

### 3. Set Timeline View

A D3 scatter/bubble chart plotting sets on a timeline (X = year, Y = part count, bubble size = part count), filterable by theme. Shown as a new section on the Theme Detail page.

#### [MODIFY] [theme-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.ts)
- Add `@ViewChild('timelineChart')` ref
- Add `renderTimeline()` method using D3 (scatter plot, X=year, Y=partCount)
- Each bubble is clickable → navigate to set detail
- Call `renderTimeline()` after sets are loaded

#### [MODIFY] [theme-detail.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.html)
- Add "Timeline" section between the hero stats and the sets list
- `<div class="timeline-card"><div #timelineChart></div></div>`

#### [MODIFY] [theme-detail.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-detail/theme-detail.component.scss)
- `.timeline-card` glass card styling
- SVG axis/tooltip/bubble styles

## Verification Plan

### Build
- `npx ng build --configuration production` — zero errors

### Manual Browser Testing
- **Similar Sets:** Navigate to a set detail → verify 8-12 similar sets appear with correct theme/year affinity
- **Comparison:** Add 2-4 sets from explorer → click Compare → verify side-by-side table
- **Timeline:** Navigate to a theme → verify D3 scatter chart with year axis and clickable bubbles
