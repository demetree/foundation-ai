# Version Detail Modal for Change History Viewer

Add a "View Full Details" modal to the existing `ChangeHistoryViewerComponent`. Clicking a version entry opens a modal with two tabs: **Changes** (full-width untruncated diffs) and **Snapshot** (all field values at that version).

## Proposed Changes

### Change History Viewer (shared component)

Self-contained enhancement — no changes needed in any parent component or module registration.

#### [MODIFY] [change-history-viewer.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/change-history-viewer/change-history-viewer.component.ts)

- Store the sorted raw `auditHistory` entries alongside `processedEntries` so we can access the full `data` snapshot for any version
- Add `ProcessedHistoryEntry.rawData` and `ProcessedHistoryEntry.previousData` fields to carry the underlying `data` objects
- Add `openDetailModal(entry)` method that opens an `NgbModal` with the detail template
- Add `getSnapshotFields(data)` helper that returns all non-excluded fields with labels and formatted values
- Inject `NgbModal` into constructor

#### [MODIFY] [change-history-viewer.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/change-history-viewer/change-history-viewer.component.html)

- Add a small "View Details" button (icon) on each timeline entry header row — visible on hover or always shown
- Add an `<ng-template #detailModal>` at the bottom with:
  - **Modal header**: version badge, user, timestamp
  - **Two-tab layout** using `ngbNav`:
    - **Changes tab**: full-width diff table (no truncation), reuses same diff data
    - **Snapshot tab**: two-column key-value list of all fields at that version

#### [MODIFY] [change-history-viewer.component.scss](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/shared/change-history-viewer/change-history-viewer.component.scss)

- Add styles for the detail button (hover reveal or subtle icon)
- Add modal-specific styles: snapshot field list, full-width diff table, tab styling

## Verification Plan

### Automated
- `ng build` passes

### Manual
- Open any entity with 2+ versions → History tab → click a version entry → modal opens
- Changes tab shows full untruncated diffs
- Snapshot tab shows all field values for that version
- Initial creation entries show snapshot only (no changes tab data)
