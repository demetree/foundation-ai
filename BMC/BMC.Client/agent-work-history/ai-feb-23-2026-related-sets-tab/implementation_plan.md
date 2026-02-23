# Theme Explorer — Nested Hierarchy & Expandable Tree

The theme list currently shows only root-level themes in a flat list. Sub-theme counts are shown but the children can't be explored inline. Search only matches root theme names, missing sub-themes entirely.

## Proposed Changes

### Theme Explorer Component

#### [MODIFY] [theme-explorer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-explorer/theme-explorer.component.ts)

- Add `childMap` as a class-level field (currently local to `buildThemeCards`)
- Add `expandedThemes: Set<number>` to track which root themes are expanded
- Add `toggleExpand(themeId)` method
- Update `applyFilter()` to also search sub-theme names — when a sub-theme matches, auto-expand its parent

#### [MODIFY] [theme-explorer.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-explorer/theme-explorer.component.html)

- Add expand/collapse chevron to theme cards that have children
- Add nested `.sub-theme-list` revealed when expanded, showing child themes indented
- Each sub-theme card is clickable → navigates to theme detail

#### [MODIFY] [theme-explorer.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/theme-explorer/theme-explorer.component.scss)

- `.theme-card-expandable` — chevron rotation on expand
- `.sub-theme-list` — indented container with subtle left border
- `.sub-theme-card` — smaller card variant for children
- Smooth expand/collapse with `max-height` transition

## Verification Plan

### Automated Tests
- Production build passes with zero TS errors
