# Change History Viewer — Session Walkthrough

## 1. Cache Invalidation Fix

Added `this.auditHistory = null` in each component's reload path so history re-fetches after edits instead of showing stale data.

| Component | Reset Location |
|-----------|---------------|
| Shift | `onShiftChanged()` (already existed) |
| ShiftPattern | `onPatternChanged()` |
| Resource, Office, Client, Crew, Calendar, SchedulingTarget, Contact | `loadData()` |

## 2. Version Detail Modal

Enhanced `ChangeHistoryViewerComponent` with a click-to-open modal for full version details. Self-contained — **no parent component or module changes needed**.

### Files Modified

- [change-history-viewer.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/change-history-viewer/change-history-viewer.component.ts) — Added `rawData`/`previousData` to `ProcessedHistoryEntry`, `NgbModal` injection, `openDetailModal()`, `buildSnapshot()`
- [change-history-viewer.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/change-history-viewer/change-history-viewer.component.html) — Added hover-reveal expand button + `<ng-template #detailModal>` with ngbNav tabs
- [change-history-viewer.component.scss](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/change-history-viewer/change-history-viewer.component.scss) — Added detail button, modal header/tabs, snapshot table, responsive styles

### Modal Features

- **Changes tab**: Full-width diff table with no truncation (long text values display completely)
- **Snapshot tab**: Key-value list of all field values at that version
- Auto-selects the right tab (Snapshot for initial creation, Changes for updates)
- `event.stopPropagation()` prevents double-action with inline expand

### Verification
- `ng build` passes (exit code 0)
