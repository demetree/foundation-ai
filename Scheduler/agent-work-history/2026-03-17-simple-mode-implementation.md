# Simple Mode Implementation & Theme/Calendar Fixes

**Date:** 2026-03-17

## Summary

Implemented a hierarchical Simple/Advanced mode toggle for the Scheduler UI. In Simple mode, the UI hides power-user features to provide a clean experience for non-technical users. Also fixed theme compliance issues and a critical calendar refresh bug.

## Changes Made

### Simple Mode Feature
- **[NEW] `services/scheduler-mode.service.ts`** — Centralized mode service with global `BehaviorSubject<'simple'|'advanced'>` plus per-component override map. Persists via `UserSettingsService`.
- **`sidebar.component.ts` + `.html`** — Hide Volunteers (2 items) and Setup (8 items) groups in simple mode. Added mode toggle pill widget at bottom of sidebar.
- **`event-add-edit-modal.component.ts` + `.html`** — Hide 5 of 7 tabs (Assignments, Advanced, Dependencies, Financials, Rental Agreement) and 10 Detail fields. Added inline Simple/Advanced toggle button in modal header for per-component override.
- **`recurrence-builder.component.ts` + `.html`** — Accept `@Input() simpleMode`. Hide Monthly/Yearly frequencies, interval config, and count-based end condition.
- **`overview.component.ts` + `.html`** — Hide Activity (targets) and Resources cards; hide Active Resources and Unavailable stats.

### Calendar Header Theme Fix
- **`scheduler-calendar.component.scss`** — Added `!important` to `.fc-col-header-cell` background, themed `.fc-scrollgrid-section-header`, `.fc-timegrid-axis`, and `.fc-daygrid-body` so weekly/daily view headers respect the dark theme.

### Dependencies Tab Theme Fix
- **`event-add-edit-modal.component.scss`** — Converted all hardcoded light colors to `--sch-*` theme tokens for dark theme compatibility.

### Calendar Refresh After Event Save (Critical Bug Fix)
- **`scheduler-calendar.component.ts`** — Fixed a bug where the calendar would go blank after saving a new event.

**Root cause:** A race condition between `handleDatesSet` and `loadCalendars`:
1. On page load, `handleDatesSet` fires first and calls `loadEvents()` with no calendar filter (`calendarIds: undefined`) — shows all events.
2. `loadCalendars()` completes asynchronously, restoring 3 calendar IDs from localStorage into `selectedCalendarIds`.
3. After saving, `loadEvents()` now picks up those 3 calendar IDs as a filter — API returns 0 events because test events aren't assigned to those calendars.

**Fix applied:**
- Added `isReload` flag (captured before range fallback) to distinguish between explicit loads and post-save reloads.
- Store `lastRangeStart`, `lastRangeEnd`, and `lastUsedCalendarIds` from each successful load.
- Post-save reloads reuse stored parameters, ensuring the view stays consistent.
- Sidebar toggle methods (`toggleCalendar`, `selectAllCalendars`, `clearCalendarSelection`) explicitly update `lastUsedCalendarIds` before reloading.
- Clear `shareReplay` caches on `ScheduledEventService` and `ScheduledEventDependencyService` before each load.
- Use object spread (`{ ...calendarOptions, events }`) to ensure FullCalendar detects the change.
- `openEditModal` now uses `xl` + `static` backdrop (matching `handleDateSelect`).
- Added error handler on `forkJoin` subscribe.

## Key Decisions

- **Hierarchical mode**: Global toggle + per-component overrides for incremental feature unlocking.
- **Per-user, not per-tenant**: Mode is a user preference stored via `UserSettingsService`.
- **Default: Simple**: New users start in Simple mode.
- **No data loss**: Switching modes is purely visual — hidden fields retain values.
- **Calendar reload consistency**: Post-save reloads reuse the exact parameters that produced the current view, avoiding filter mismatches from async calendar selection restoration.
