# Calendar Pre-Selection for New Events

**Date:** 2026-03-18

## Summary

When creating a new event from the scheduler calendar view, the event modal now pre-selects the calendars that the user is currently viewing in the sidebar filter. Previously, new events started with no calendars selected, which meant they would appear in all calendar views.

## Changes Made

- **`Scheduler.Client/src/app/components/scheduler/event-add-edit-modal/event-add-edit-modal.component.ts`**
  - Added `@Input() initialCalendarIds: number[] = []` alongside existing initial inputs
  - In `ngOnInit()`, when in create mode and `initialCalendarIds` has values, pre-populates `selectedCalendarIds` from the input

- **`Scheduler.Client/src/app/components/scheduler/scheduler-calendar/scheduler-calendar.component.ts`**
  - In `openFullEventModal()`, passes `initialCalendarIds = Array.from(this.selectedCalendarIds)` when the sidebar calendar filter is active

## Key Decisions

- **Only pre-select when filter is active**: If the user has no sidebar filter (viewing all calendars), the modal behaves as before — no pre-selection. This avoids forcing all calendars on when the user hasn't expressed a preference.
- **Create mode only**: Edit mode is unaffected — it continues to load existing `EventCalendar` associations from the server via `loadExistingCalendarAssignments()`.

## Testing / Verification

- `npx ng build --configuration production` — zero errors from the 2 modified files
- Pre-existing build errors in unrelated components remain unchanged
