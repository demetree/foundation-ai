# P3 Nice-to-Haves — Rental Agreement Tracking + Year-End Close

## Background

Two P3 items from the PHMC audit. Both are client-side-only — no schema changes needed.

---

## Proposed Changes

### P3.1 — Rental Agreement Tracker

Uses `ScheduledEvent.attributes` (JSON string field) to store structured rental data instead of free-text notes.

#### [NEW] [rental-agreement-tracker.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/scheduler-custom/rental-agreement-tracker/rental-agreement-tracker.component.ts)
#### [NEW] rental-agreement-tracker.component.html + .scss

**Features:**
- Lists events that have rental agreement data in their `attributes` JSON
- Displays: event name, contact, rental dates, agreement status (Draft/Signed/Expired), deposit status
- Filter by status (Draft/Signed/Expired/All)
- Click to navigate to event detail
- Status overview summary cards

---

### P3.2 — Year-End Period Close

Uses existing `FiscalPeriod.closedDate` / `closedBy` fields via `PutFiscalPeriod`.

#### [NEW] [fiscal-period-close.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/fiscal-period-close/fiscal-period-close.component.ts)
#### [NEW] fiscal-period-close.component.html + .scss

**Features:**
- Lists all fiscal periods with status (Open/Closed), date range, fiscal year
- "Close Period" button sets `closedDate` to now and `closedBy` to current user via PUT
- "Reopen Period" button clears `closedDate`/`closedBy`
- Confirmation dialog before close/reopen
- Year filter dropdown
- Closed periods displayed with a distinct visual treatment

---

### Module Registration

#### [MODIFY] app.module.ts — 2 new imports + declarations
#### [MODIFY] app-routing.module.ts — 2 new routes: `/scheduling/rental-agreements`, `/finances/fiscal-period-close`

## Follow-Up Tasks

> [!NOTE]
> **Separate task:** Add rental agreement fields (status, signed date, expiry, contact) to the event add/edit form UI, so the `attributes` JSON gets populated via a proper workflow rather than manual entry.

## Verification

- `npx ng build --configuration production` — zero TS errors
