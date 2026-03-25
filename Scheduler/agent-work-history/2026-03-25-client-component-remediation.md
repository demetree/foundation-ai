# Client Component Remediation — Small Business Components

**Date:** 2026-03-25

## Summary

Conducted a quality and UI consistency review of 4 recently added small-business components in `Scheduler.Client`, identified 10 findings, then implemented all fixes to bring the code to production quality.

## Scope

- `overview-dispatcher-tab` — Dispatch summary KPIs on overview page
- `quick-add-job-modal` — Create Client→Target→Event in a single form
- `daily-dispatch` — Drag-and-drop daily job dispatch board
- `staff-quick-add-modal` — Quick-add a Resource (staff member)

## Findings & Fixes

1. **F1 (Critical):** `overview-dispatcher-tab` had `Math.random()` mock data in KPIs — replaced with real `EventResourceAssignmentService` lookup
2. **F2 (High):** `quick-add-job-modal` had 3-level nested callback pyramid — flattened to RxJS `switchMap` chain with `finalize()`
3. **F3 (High):** No permission checks in any component — added `AuthService` permission gates on all write operations across all 4 components
4. **F4 (Medium):** 7 hardcoded `Number(1)` FK IDs — replaced with dynamic defaults loaded from type services via `forkJoin`
5. **F5 (Medium):** Mixed modal patterns (NgbActiveModal vs TemplateRef) — standardized to `@ViewChild TemplateRef` pattern
6. **F6 (Medium):** 3 empty SCSS files + local Bootstrap variable redefinitions — populated SCSS, used `var(--bs-*-rgb)` CSS custom properties
7. **F7 (Low):** 5 inline `style=""` attributes in `daily-dispatch` — extracted to named SCSS classes
8. **F8 (Medium):** Minimal error handling — added structured error extraction matching established `client-custom-add-edit` pattern
9. **F9 (Medium):** Browser timezone conversion issue — built ISO date strings directly (`${date}T${time}:00`)
10. **F10 (Medium):** Unbounded assignment fetch — post-filtered to today's event IDs only

## Key Decisions

- Dynamic type defaults use auto-select-first pattern (no dropdowns) to preserve the "quick" workflow spirit
- Permission checks follow established pattern: `authService.isSchedulerReaderWriter` for write gates
- Error extraction uses same 3-tier pattern as `client-custom-add-edit`: `instanceof Error` → `status + error body` → fallback string

## Files Modified (12)

- `overview-dispatcher-tab.component.ts`, `.html`, `.scss`
- `quick-add-job-modal.component.ts`, `.html`, `.scss`
- `daily-dispatch.component.ts`, `.html`, `.scss`
- `staff-quick-add-modal.component.ts`, `.html`, `.scss`

## Testing / Verification

- `ng build --configuration=production` — 0 errors, output generated at `dist/scheduler.client`
- Only pre-existing warnings (CommonJS modules, CSS selector rules)
