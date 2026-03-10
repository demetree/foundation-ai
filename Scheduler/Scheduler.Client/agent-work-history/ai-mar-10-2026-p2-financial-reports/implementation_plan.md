# P2 Financial Reports — P&L + Accountant-Ready

## Background

Two P2 items from PHMC audit: a formal P&L income statement suitable for council, and accountant-ready financial reports (trial balance, chart of accounts, journal).

---

## Proposed Changes

### P2.1 — P&L / Income Statement

A formal income statement with standard accounting layout.

#### [NEW] [pnl-report.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/pnl-report/pnl-report.component.ts)
#### [NEW] pnl-report.component.html + .scss

**Features:**
- Period selector (fiscal period dropdown)
- Revenue section: lists `FinancialTransaction` records where `isRevenue=true`, grouped by `FinancialCategory`
- Expense section: same for `isRevenue=false`
- **Gross Revenue** / **Total Expenses** / **Net Income** summary
- Comparison column (prior period) if two periods selected
- Print-optimized layout with `@media print`
- CSV export and `window.print()`
- Header with org name, report period, generation date

---

### P2.2 — Accountant-Ready Reports

A tabbed report page with three accountant views.

#### [NEW] [accountant-reports.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/accountant-reports/accountant-reports.component.ts)
#### [NEW] accountant-reports.component.html + .scss

**Three tabs:**

1. **Trial Balance** — All categories with debit/credit totals. Verifies debits = credits.
2. **Chart of Accounts** — Hierarchical display of `FinancialCategory` tree using `parentFinancialCategoryId`, with account type, code, and external ID.
3. **Transaction Journal** — Filtered, sortable list of all `FinancialTransaction` records with date, category, description, amount, revenue/expense type. CSV export.

**Shared features:**
- Period/office filters
- Print + CSV export on each tab
- Professional formatting for accountant handoff

---

### Module Registration

#### [MODIFY] [app.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts)
- 2 new imports + declarations

#### [MODIFY] [app-routing.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app-routing.module.ts)
- Routes: `/finances/pnl-report`, `/finances/accountant-reports`

---

## Verification Plan

### Automated Tests
- `npx ng build --configuration production` — zero TS errors from new files

### Manual Verification
- Navigate to both routes, verify data loads and print layout works
