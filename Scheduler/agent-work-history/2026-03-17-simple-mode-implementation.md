# Simple Mode Implementation

**Date:** 2026-03-17

## Summary

Implemented a hierarchical Simple/Advanced mode toggle for the Scheduler UI. In Simple mode, the UI hides power-user features (resource management, crew scheduling, shift patterns, dependencies, financials, etc.) to provide a clean, focused experience for non-technical small-town coordinators. The mode is persisted per-user via the existing UserSettingsService API and defaults to Simple for new users.

## Changes Made

- **[NEW] `Scheduler.Client/src/app/services/scheduler-mode.service.ts`** — Centralized mode service with global `BehaviorSubject<'simple'|'advanced'>` plus per-component override map. Persists via `UserSettingsService`.
- **`sidebar.component.ts` + `.html`** — Hide Volunteers group (2 items) and Setup group (8 items) in simple mode. Added mode toggle widget (pill switch) at the bottom of the sidebar.
- **`event-add-edit-modal.component.ts` + `.html`** — Hide 5 of 7 tabs (Assignments, Advanced, Dependencies, Financials, Rental Agreement) and 10 Detail fields (Status, Priority, Color, Target, Client, Office, Source, Calendars, Notes, Dynamic Attributes). Keep Details + Recurrence tabs in simple mode.
- **`recurrence-builder.component.ts` + `.html`** — Accept `@Input() simpleMode`. Hide Monthly/Yearly frequencies, interval config, and count-based end condition. Keep Daily/Weekly, day picker, Never/On-date end.
- **`overview.component.ts` + `.html`** — Hide Activity (targets) and Resources cards; hide Active Resources and Unavailable stats in header bar.

## Key Decisions

- **Hierarchical mode**: Global toggle + per-component overrides so users can unlock advanced features incrementally for specific areas (e.g., Advanced event editor but Simple everything else).
- **Per-user, not per-tenant**: Mode is a user preference, not a tenant-wide setting, allowing different users within the same organization to have different complexity levels.
- **Default: Simple**: New users start in Simple mode to deliver the "easy to use" first impression.
- **No data loss**: Switching modes is purely visual — hidden fields retain their values, and switching back to Advanced reveals everything intact.
- **Finances visible in Simple**: Per owner direction, the Finances sidebar link stays visible in Simple mode since small-town coordinators may need to track payments.
- **Recurrence in Simple**: Kept the Recurrence tab but simplified it (Daily/Weekly only, no interval, no count-based end).

## Testing / Verification

- **Build**: `npx ng build` — no TypeScript errors from Simple Mode changes. Exit code 1 is from pre-existing NG8102 warnings in unrelated components (ShiftPattern, SystemHealth, Volunteer).
- **Manual testing**: Checklist provided for the owner to verify mode toggle, persistence, data integrity on mode switch, and correct UI visibility.
