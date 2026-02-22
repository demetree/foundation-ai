# Parts Universe — Layout Restructure

Convert from a long vertical scroll of stacked panels to a focused exploration layout with tabbed visualizations and a compact leaderboard.

## Proposed Changes

### [MODIFY] [parts-universe.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/parts-universe/parts-universe.component.ts)

**New state properties:**
- `leaderboardLimit: number = 15` — rows to show (15/25/50/All)
- `activeVizTab: string = 'galaxy'` — which viz is showing (`galaxy` | `flow` | `colordna` | `connections`)

**Leaderboard show-N:**
- `setLeaderboardLimit(n: number)` — updates `leaderboardLimit`, persisted to URL query params
- `displayedParts` getter sliced to `leaderboardLimit` instead of hardcoded 50

**Tabbed render optimisation:**
- `renderAllPanels()` → only renders the active tab's visualization (deferred renders for inactive tabs)
- `setVizTab(tab: string)` — sets `activeVizTab`, persists to URL, triggers render of that tab
- Tab state restored from `?viz=galaxy` query param on init

---

### [MODIFY] [parts-universe.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/parts-universe/parts-universe.component.html)

**Leaderboard changes:**
- Add a show-N dropdown in the panel header next to the mode switcher
- Change `slice:0:50` → `slice:0:leaderboardLimit`

**Visualization restructure:**
- Remove the 4 separate `viz-panel` blocks + `viz-row` wrapper
- Replace with a single `viz-tabs-container`:
  - **Tab bar** — 4 icon+label buttons: Galaxy, Part Flow, Color DNA, Connections
  - **Tab content** — 4 `*ngIf`-gated container divs (only the active one renders its DOM)
  - Each tab keeps its existing panel header (icon + title + description) inside the content area

```html
<!-- Simplified structure -->
<div class="viz-tabs-container">
    <div class="viz-tab-bar">
        <button [class.active]="activeVizTab === 'galaxy'" (click)="setVizTab('galaxy')">
            <i class="fas fa-circle-nodes"></i> Galaxy
        </button>
        <!-- ... 3 more tabs ... -->
    </div>
    <div class="viz-tab-content">
        <div *ngIf="activeVizTab === 'galaxy'" class="viz-panel bubble-panel">
            <!-- existing panel header + #bubbleContainer -->
        </div>
        <!-- ... -->
    </div>
</div>
```

---

### [MODIFY] [parts-universe.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/parts-universe/parts-universe.component.scss)

- **New** `.viz-tabs-container`, `.viz-tab-bar`, `.viz-tab-content` styles
- Tab bar: horizontal pill buttons with icons, glassmorphism active state, scrollable on mobile
- Remove `.viz-row` side-by-side layout (no longer needed)
- Add `.show-n-select` inline dropdown styles for leaderboard header
- Adjust `.bubble-body`, `.sankey-body` etc. — remove fixed max-widths so viz fills full tab width

## Verification Plan

- `ng build --configuration production` — 0 errors
- Visual: all 4 tabs render correctly, tab switching triggers correct visualization
- URL persistence: `?viz=flow` restores correct tab on reload
- Leaderboard: show-N dropdown changes visible row count
