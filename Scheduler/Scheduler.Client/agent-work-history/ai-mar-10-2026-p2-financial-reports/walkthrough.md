# P2 Financial Reports — Walkthrough

## Summary

Implemented two P2 items: a formal **P&L income statement** and a tabbed **accountant reports** page with trial balance, chart of accounts, and transaction journal. All views support **PDF**, **CSV**, and **print** export.

## Changes

### P2.1 — P&L / Income Statement (3 files)

| Component | Route |
|-----------|-------|
| [pnl-report](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/pnl-report/pnl-report.component.ts) | `/finances/pnl-report` |

**Features:** Revenue/expense sections grouped by `FinancialCategory`, gross totals, net income bar, office/period filters, deep-indigo gradient header.

**Export:** PDF (via `jsPDF`), CSV (Blob download), `window.print()` with `@media print`.

---

### P2.2 — Accountant Reports (3 files)

| Component | Route |
|-----------|-------|
| [accountant-reports](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/accountant-reports/accountant-reports.component.ts) | `/finances/accountant-reports` |

**Three tabs:**
1. **Trial Balance** — Debit/credit columns by category, balance indicator (Balanced / Out of Balance)
2. **Chart of Accounts** — Hierarchical tree via `parentFinancialCategoryId`, with indent levels and folder icons
3. **Transaction Journal** — Sortable + searchable, with Revenue/Expense type badges

**Export:** PDF, CSV, print — each exports the active tab's data.

---

### Module Registration

- [app.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts) — 2 new imports + declarations
- [app-routing.module.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/app-routing.module.ts) — 2 new imports + 2 routes

## Verification

- **Build:** `npx ng build --configuration production` — **zero TypeScript errors** from new files
- Exit code 1 from pre-existing bundle size budget warning only
