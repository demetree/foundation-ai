# P3 Nice-to-Haves — Walkthrough

## Summary

Implemented two P3 items: **rental agreement tracker** (reads `attributes` JSON from ScheduledEvent) and **fiscal period close workflow** (sets `closedDate`/`closedBy` via PUT). Both client-side-only, no schema changes.

## Changes

### P3.1 — Rental Agreement Tracker (3 files)

| Component | Route |
|-----------|-------|
| [rental-agreement-tracker](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler-custom/rental-agreement-tracker/rental-agreement-tracker.component.ts) | `/scheduling/rental-agreements` |

**Features:** Parses `ScheduledEvent.attributes` JSON for `rentalAgreement` data, summary cards (Draft/Signed/Expired), filterable table, click to navigate to event.

---

### P3.2 — Fiscal Period Close (3 files)

| Component | Route |
|-----------|-------|
| [fiscal-period-close](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/fiscal-period-close/fiscal-period-close.component.ts) | `/finances/fiscal-period-close` |

**Features:** Period list with year filter, Close/Reopen buttons with confirmation dialog, sets `closedDate`/`closedBy` via `PutFiscalPeriod`, Open/Closed summary cards.

---

### Module Registration

- [app.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts) — 2 new imports + declarations
- [app-routing.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app-routing.module.ts) — 2 new routes

### Follow-Up

> [!NOTE]
> **Separate task:** Add rental agreement fields to the event add/edit form so the `attributes` JSON gets populated via a proper UI workflow.

## Verification

- **Build:** `npx ng build --configuration production` — **zero TypeScript errors** from new files
- Exit code 1 from pre-existing bundle size budget warning only
