# Event Timeline Chart & User Name Fix

**Date:** 2026-03-18

## Summary

Fixed two issues in the Event Timeline tab (`app-event-timeline-panel`): the scatter chart was not rendering, and user names showed as "User #3" instead of actual names.

## Changes Made

- **`package.json`**
  - Installed `chartjs-adapter-date-fns` v3.0.0 — required by Chart.js 4.x for `type: 'time'` axis scales

- **`Scheduler.Client/src/app/components/scheduler/event-timeline-panel/event-timeline-panel.component.ts`**
  - Added `import 'chartjs-adapter-date-fns'` side-effect import to register the date adapter
  - Switched from `GetXChangeHistoryList()` to `GetXAuditHistory(id, true)` for all 5 entity types (Event, Charge, Document, Assignment, Recurrence)
  - Renamed `processChangeHistoryGroup()` → `processAuditHistoryGroup()` to accept `VersionInformation<T>[]`
  - User names now read from `record.user.firstName`/`lastName`/`userName` (server-resolved) instead of client-side `AuthService.currentUser` fallback
  - Removed imports: `ScheduledEventChangeHistoryService`, `EventChargeChangeHistoryService`, `DocumentChangeHistoryService`, `EventResourceAssignmentChangeHistoryService`, `RecurrenceRuleChangeHistoryService`, `EventCalendarService`, `AuthService`
  - Added import: `ScheduledEventService` (for `GetScheduledEventAuditHistory`)
  - Removed `resolveUserName()` method and `AuthService` constructor dependency
  - Added `maxTicksLimit: 8`, shortened hour display format to `h:mm a`, limited rotation to 45°, reduced font to 10px on x-axis ticks

- **`Scheduler.Client/src/app/components/scheduler/event-timeline-panel/event-timeline-panel.component.scss`**
  - Increased `.chart-container` height from 120px to 180px to give scatter dots room above the rotated time labels

## Key Decisions

- **AuditHistory endpoints over raw ChangeHistory lists**: The `AuditHistory` endpoints return `VersionInformation<T>` with a server-resolved `user` object containing `userName`, `firstName`, `lastName`. This matches the established pattern used by `contact-custom-detail` → `change-history-viewer`. The raw `ChangeHistoryList` endpoints only return a numeric `userId` with no way to resolve names.
- **Chart.js date adapter**: `chartjs-adapter-date-fns` v3 is compatible with both Chart.js 4.x and the project's existing `date-fns` 4.1.0 dependency.

## Testing / Verification

- `npx ng build --configuration production` — zero errors from the modified component
- Pre-existing build errors in unrelated components (SystemHealth, ShiftPatternCustomDetail, VolunteerOverviewTab) remain unchanged
