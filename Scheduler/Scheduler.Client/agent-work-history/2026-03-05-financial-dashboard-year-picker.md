# Financial Dashboard — Year Picker & Height Reduction

**Date:** 2026-03-05

## Summary

Improved the financial custom dashboard component with a fiscal year picker for the monthly breakdown chart and reduced overall component height so that action buttons are visible without scrolling.

## Changes Made

- **`financial-custom-dashboard.component.ts`** — Injected `FiscalPeriodService` to load fiscal periods and derive available years from the `FiscalPeriod` table. Added `selectedYear`, `currentYear`, `availableYears`, and `fiscalPeriodList` properties. Cached all transactions in `allTransactions` so year changes recalculate the monthly breakdown via `rebuildMonthlyBreakdown()` without re-fetching. Added `stepYear()` and `canStepYear()` for chevron navigation. Summary cards remain all-time totals.

- **`financial-custom-dashboard.component.html`** — Added a fiscal year picker (select dropdown with ◀/▶ chevron buttons) to the Monthly Breakdown card header. Moved quick action buttons (Record Income, Record Expense, Categories, All Transactions) into the premium header, removing the bottom row that required scrolling. Current-month highlighting only applies when viewing the current calendar year.

- **`financial-custom-dashboard.component.scss`** — Added `.year-select` (90px, centered, bold) and `.year-step-btn` styles. Reduced chart container height (220→180px), bar pair height (170→140px), and recent transactions list max-height (450→300px). Removed `.quick-action` styles.

## Key Decisions

- **Fiscal years from FiscalPeriod table** — Instead of deriving available years from transaction dates, the year picker is driven by the `FiscalPeriod` table so only officially defined fiscal years appear.
- **All-time summary cards** — The four summary cards (Total Income, Total Expenses, Net Balance, Records) remain all-time totals and are not filtered by the selected year. Only the monthly breakdown chart responds to the year picker.
- **Quick actions in header** — Moved to the premium header rather than keeping a dedicated bottom row, which was the root cause of the scrolling issue.

## Testing / Verification

- `npx ng build` — exit code 0, no errors. All warnings are pre-existing and unrelated to these changes.
