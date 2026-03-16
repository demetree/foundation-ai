# Financial Module — Navigation, Auth & Filtering Improvements

**Date:** 2026-03-15

## Summary

Improved the financial module's usability by fixing authentication on the export report feature, rewiring quick-action buttons, exposing hidden features through navigation links, adding GL code management access from Administration, promoting the fiscal year picker to a top-level dashboard filter, fixing the export filename, and fixing fiscal period filtering across 3 report components.

## Changes Made

### Dashboard Component (`financial-custom-dashboard`)
- **Auth headers fix**: Added `Authorization: Bearer` headers to all 3 raw `HttpClient.get()` calls — `exportReport()`, `loadCategoryBreakdown()`, and `loadOutstandingDeposits()`. Fixes 401 error on Export Report.
- **Export filename fix**: Wrapped blob in a typed `Blob` with explicit Excel MIME type and improved Content-Disposition regex. Downloads now have a meaningful filename (`Financial_Report_2026.xlsx`).
- **Record Income/Expense buttons**: Changed from navigating to the transaction list to opening the transaction add-edit modal directly, pre-seeded with `isRevenue: true` (income) or `isRevenue: false` (expense), and the currently selected office.
- **Reports & Tools section**: Added a new card grid at the bottom of the dashboard with buttons linking to P&L Report, Budget Report, Accountant Reports, Deposit Manager, and Fiscal Period Close.
- **Promoted fiscal year picker**: Moved year picker from the Monthly Breakdown chart header to the top of the dashboard, inline with the office picker. Year now filters everything — summary cards, monthly chart, and category breakdown all respond to the selected year.
- **Dashboard refresh**: After saving a transaction from the modal, the dashboard reloads all summary data.

### Administration Component
- Added a "Financial Categories" link to the Financial Configuration card, routing to `/finances/categories`.

### Fiscal Period Filtering Bug Fixes (3 components)
- **P&L Report** (`pnl-report.component.ts`): `selectedFiscalPeriodId` was completely ignored — transactions were never filtered by period date range. Fixed.
- **Budget Report** (`budget-report.component.ts`): Budgets were filtered by period, but transaction actuals were not — actuals always showed all-period totals. Fixed.
- **Accountant Reports** (`accountant-reports.component.ts`): `selectedFiscalPeriodId` was completely ignored — trial balance, journal, etc. always showed all-time data. Fixed.
- **Budget Manager**: Already correct (no change needed).

### Files Modified
- `financial-custom-dashboard/financial-custom-dashboard.component.ts`
- `financial-custom-dashboard/financial-custom-dashboard.component.html`
- `administration/administration.component.html`
- `pnl-report/pnl-report.component.ts`
- `budget-report/budget-report.component.ts`
- `accountant-reports/accountant-reports.component.ts`

## Key Decisions

- Auth headers added inline (following project pattern from `invoice-helper.service.ts`) since there's no global HTTP interceptor.
- Reports & Tools section placed at the bottom of the dashboard to avoid overcrowding the header.
- Fiscal period filtering done client-side by comparing transaction dates to the period's `startDate`/`endDate` range, since the auto-generated data services don't support server-side date-range filtering.

## Deferred Work

- **Budget custom component suite**: The Budgets feature currently routes to the auto-generated add-edit modal. A full custom budget add-edit component should be built in a future session to provide inline editing, better UI, and a proper budget creation workflow.

## Testing / Verification

All features verified in-browser on `localhost:10100`:

- ✅ **Export Report**: Downloads Excel file with correct filename, no 401 error
- ✅ **Record Income/Expense**: Opens transaction add-edit modal with correct Revenue/Expense pre-set
- ✅ **Reports & Tools section**: All 5 buttons visible and navigating correctly
- ✅ **Admin Financial Categories link**: Visible in Administration > Configuration > Financial Configuration
- ✅ **Year picker at top**: Filters summary cards, monthly chart, and category breakdown by selected year
- ✅ **P&L period filtering**: Revenue/expense totals update when selecting different fiscal periods
- ✅ **Budget Report period filtering**: Actuals now scoped to selected period
- ✅ **Accountant Reports period filtering**: Trial balance, journal entries scoped to selected period
