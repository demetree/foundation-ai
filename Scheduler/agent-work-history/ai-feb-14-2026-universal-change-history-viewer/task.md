# Universal Change History Viewer

## Phase 1 — Create Reusable Component
- [x] Create `change-history-viewer` component (TS + HTML + SCSS) in `shared/`
- [x] Accepts `VersionInformation<T>[]` data + entity name, handles loading/empty states
- [x] Renders timeline with version badge, resolved user name, relative timestamp
- [x] Expandable detail: shows changed fields with old → new diff (comparing adjacent versions)

## Phase 2 — Integrate into Missing Detail Components
- [x] Resource detail — add History tab
- [x] Office detail — add History tab
- [x] Client detail — add History tab
- [x] Crew detail — add History tab
- [x] Calendar detail — add History tab

## Phase 3 — Replace Existing Custom Implementations
- [x] Shift detail — replace inline change history with `<app-change-history-viewer>`
- [x] ShiftPattern detail — replace inline timeline with `<app-change-history-viewer>`
- [x] SchedulingTarget detail — replace auto-generated table with `<app-change-history-viewer>`
- [x] Contact detail — wire "View Full History" button to use component

## Verification
- [x] `ng build` passes
- [x] Walkthrough updated
