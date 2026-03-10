# P1 Audit Gaps — Payment Recording, Budget Report & Deposit Management

## Background

Three P1 gaps from the PHMC audit remain. After research, here's what actually needs building:

> [!IMPORTANT]
> The **Budget vs Actual comparison** was listed as a gap, but `FinancialBudgetManagerComponent` already has it — full variance, progress bars, inline editing, revenue/expense splits, and summary totals. What's actually missing is a **printable/exportable report format** of that data.

---

## Proposed Changes

### P1.1 — Payment Recording Workflow

New custom components for recording payments against event charges and invoices. Uses the existing `PaymentTransaction` entity (already fully modeled with paymentMethodId, eventChargeId, amount, processingFee, netAmount, status, payer info).

#### [NEW] [payment-custom-listing.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/payment-custom/payment-custom-listing/payment-custom-listing.component.ts)
#### [NEW] payment-custom-listing.component.html + .scss
- Listing of all payment transactions with status, amount, date
- Filters: date range, status, payment method
- Link to detail view

#### [NEW] [payment-custom-detail.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/payment-custom/payment-custom-detail/payment-custom-detail.component.ts)
#### [NEW] payment-custom-detail.component.html + .scss
- Detail view showing payment info, linked event charge, linked invoice
- Option to generate receipt from payment

#### [NEW] [payment-custom-add-edit.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/payment-custom/payment-custom-add-edit/payment-custom-add-edit.component.ts)
#### [NEW] payment-custom-add-edit.component.html + .scss
- Modal form: payment method, amount, processing fee (auto-compute net), payer info
- Optional link to event charge or invoice
- Status field (Completed, Pending, Failed, Refunded)

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts)
- Add imports/declarations for 3 payment components

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app-routing.module.ts)
- Add routes: `/finances/payments`, `/finances/payments/new`, `/finances/payments/:paymentTransactionId`

---

### P1.2 — Budget vs Actual Report (Printable/Exportable)

The budget manager already computes all the data. This adds a dedicated report view optimized for printing and accountant export.

#### [NEW] [budget-report.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/budget-report/budget-report.component.ts)
#### [NEW] budget-report.component.html + .scss
- Reuses `BudgetDisplayRow` logic from budget manager
- Print-optimized layout with header (org name, period, date)
- Revenue section, expense section, totals row
- Variance column with color coding
- Print button (window.print()) and CSV export button
- No inline editing — read-only report format

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts)
- Add import/declaration

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app-routing.module.ts)
- Add route: `/finances/budget-report`

---

### P1.3 — Deposit Management Workflow

Enhance the existing event charge UI to properly handle deposits — collection, tracking, and refund recording. The data model already has `isDeposit`, `depositRefundedDate` on `EventCharge`.

#### [NEW] [deposit-manager.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/deposit-manager/deposit-manager.component.ts)
#### [NEW] deposit-manager.component.html + .scss
- Dashboard of all outstanding deposits (EventCharges where isDeposit=true and depositRefundedDate is null)
- Grouped by event, showing client name, deposit amount, event date
- Refund button that sets `depositRefundedDate` via PUT and optionally creates a receipt
- Filters: outstanding only, refunded only, all
- Summary totals for outstanding deposit liability

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts)
- Add import/declaration

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app-routing.module.ts)
- Add route: `/finances/deposits`

---

## Verification Plan

### Automated Tests
- `npx ng build --configuration production` — zero errors from new files
- Grep build output for payment/budget-report/deposit component errors

### Manual Verification
- Navigate to `/finances/payments`, `/finances/budget-report`, `/finances/deposits`
- Verify data loads and UI renders correctly
