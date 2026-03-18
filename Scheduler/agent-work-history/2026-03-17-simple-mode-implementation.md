# Simple Mode Implementation & Theme Fixes

**Date:** 2026-03-17

## Summary

Implemented a hierarchical Simple/Advanced mode toggle for the Scheduler UI. In Simple mode, the UI hides power-user features to provide a clean experience for non-technical users. Also fixed theme compliance issues in the calendar headers and event editor Dependencies tab.

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
- **`event-add-edit-modal.component.scss`** — Converted all hardcoded light colors (`white`, `#1e293b`, `#475569`, `#94a3b8`, `rgba(0,0,0,...)`) to `--sch-*` theme tokens (`--sch-bg-card`, `--sch-text-primary`, `--sch-text-secondary`, `--sch-text-muted`, `--sch-border`, `--sch-bg-deep`). Also fixed `.empty-state` class.

## Key Decisions

- **Hierarchical mode**: Global toggle + per-component overrides for incremental feature unlocking.
- **Per-user, not per-tenant**: Mode is a user preference stored via `UserSettingsService`.
- **Default: Simple**: New users start in Simple mode.
- **No data loss**: Switching modes is purely visual — hidden fields retain values.
- **Inline toggle**: Event editor has its own Simple/Advanced toggle in the header, using component-level override independent of the global mode.

## Testing / Verification

- **Build**: No TypeScript errors from changes. Exit code 1 is from pre-existing NG8102 warnings in unrelated components.
- **Manual testing**: Checklist provided for mode toggle, persistence, data integrity, and UI visibility verification.
