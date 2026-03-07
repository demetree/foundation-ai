# Financial Module Fixes — Walkthrough

## Changes Made

### Bug Fixes

#### 1. Dashboard chart now respects office filter
**File:** [financial-custom-dashboard.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-custom-dashboard/financial-custom-dashboard.component.ts)

`rebuildMonthlyBreakdown()` was iterating `this.allTransactions` (the unfiltered set), making the year chart ignore the office dropdown. Changed to use `this.getFilteredTransactions()` so the chart is consistent with the summary cards.

```diff
-        for (const t of this.allTransactions) {
+        for (const t of this.getFilteredTransactions()) {
```

---

#### 2. Budget manager filters actuals by fiscal period
**File:** [financial-budget-manager.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-budget-manager/financial-budget-manager.component.ts)

The transaction query params didn't include `fiscalPeriodId`, so budget-vs-actual comparisons were using unfiltered totals. Added the fiscal period filter when a period is selected.

```diff
         if (this.selectedOfficeId) {
             txParams.financialOfficeId = this.selectedOfficeId;
         }
+        if (this.selectedFiscalPeriodId) {
+            txParams.fiscalPeriodId = this.selectedFiscalPeriodId;
+        }
```

---

### Enhancements

#### 3. pageSize bumped to 10,000
**Files:** [financial-custom-dashboard.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-custom-dashboard/financial-custom-dashboard.component.ts), [financial-transaction-custom-listing.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-listing/financial-transaction-custom-listing.component.ts)

Both files changed from `pageSize: 5000` → `pageSize: 10000` with TODO comments for future server-side pagination.

---

#### 4. Mobile card click handlers
**File:** [financial-transaction-custom-listing.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-listing/financial-transaction-custom-listing.component.html)

Mobile card divs now have `(click)="editTransaction(tx)"`, `role="button"`, and `cursor: pointer` — matching the desktop table row behaviour.

---

#### 5. hiddenFields in transaction add-edit
**Files:** [financial-transaction-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-add-edit/financial-transaction-custom-add-edit.component.ts), [financial-transaction-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-add-edit/financial-transaction-custom-add-edit.component.html)

- Added `isFieldHidden()` helper method
- Added validator clearing for hidden required fields via `clearValidators()`
- Wrapped all 10 primary field columns with `*ngIf="!isFieldHidden('fieldName')"`

---

#### 6. Custom category add-edit modal (new component)

| File | Status |
|------|--------|
| [financial-category-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-category-custom-add-edit/financial-category-custom-add-edit.component.ts) | **NEW** |
| [financial-category-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-category-custom-add-edit/financial-category-custom-add-edit.component.html) | **NEW** |
| [financial-category-custom-add-edit.component.scss](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-category-custom-add-edit/financial-category-custom-add-edit.component.scss) | **NEW** |
| [app.module.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts) | import + declaration |
| [financial-category-custom-listing.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-category-custom-listing/financial-category-custom-listing.component.ts) | import + ViewChild type |
| [financial-category-custom-listing.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-category-custom-listing/financial-category-custom-listing.component.html) | selector swap |

Replaces the auto-generated category add-edit modal with a custom one featuring: reactive form, section-card layout, collapsible "More Details", color picker, permission checks, pre-seeded data, and `hiddenFields` support.

---

#### 7. Server-side aggregation endpoint (new)
**File:** [FinancialTransactionsController.cs](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs) — **NEW**

Partial class extension of the auto-generated controller. Adds `GET /api/FinancialTransactions/Summary` that computes totals, counts, and a 12-month income/expense breakdown entirely on the database. Supports `financialOfficeId` and `year` filters.

---

## Verification

| Build | Result |
|-------|--------|
| **Server** (`dotnet build`) | ✅ 0 errors |
| **Client** (`ng build --configuration=development`) | ✅ 0 errors |
