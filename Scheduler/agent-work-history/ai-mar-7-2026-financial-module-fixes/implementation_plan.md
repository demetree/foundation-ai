# Financial Module Fixes — Implementation Plan

Addressing the user-approved findings from the financial module audit.

---

## Proposed Changes

### Bug Fixes

---

#### [MODIFY] [financial-custom-dashboard.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-custom-dashboard/financial-custom-dashboard.component.ts)

**Fix: Monthly chart ignores office filter**

In `rebuildMonthlyBreakdown()` (line 237), change `this.allTransactions` → `this.getFilteredTransactions()` so the chart respects the selected office. Both `processTransactions()` and `rebuildMonthlyBreakdown()` should operate on the filtered set.

```diff
-for (const t of this.allTransactions) {
+for (const t of this.getFilteredTransactions()) {
```

> [!NOTE]
> The summary cards already use `getFilteredTransactions()` via `processTransactions()`, but when `onYearChange()` is called (line 260), it calls `rebuildMonthlyBreakdown()` directly without re-filtering. The fix ensures both year changes and office changes produce consistent filtered results.

---

#### [MODIFY] [financial-budget-manager.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-budget-manager/financial-budget-manager.component.ts)

**Fix: Actuals not filtered by fiscal period**

In `loadBudgetData()` (lines 148-155), add `fiscalPeriodId` to the transaction query params when a fiscal period is selected. The budget query already filters by `fiscalPeriodId`, but the transaction query does not. We need to filter transactions to only include those within the selected period's date range.

Since `FinancialTransaction` has a `fiscalPeriodId` field, we can filter transactions directly:

```diff
 const txParams: any = {
     active: true,
     deleted: false,
     includeRelations: true
 };
 if (this.selectedOfficeId) {
     txParams.financialOfficeId = this.selectedOfficeId;
 }
+if (this.selectedFiscalPeriodId) {
+    txParams.fiscalPeriodId = this.selectedFiscalPeriodId;
+}
```

---

### Enhancements

---

#### [MODIFY] All financial components — Bump `pageSize` to 10000

Add TODO comments and change `pageSize: 5000` → `pageSize: 10000` in:

| File | Line |
|------|------|
| `financial-custom-dashboard.component.ts` | ~126 |
| `financial-transaction-custom-listing.component.ts` | data load call |
| `financial-category-custom-listing.component.ts` | data load call |
| `financial-budget-manager.component.ts` | implicit (already loads all) |

Each change site gets a `// TODO: Replace with server-side pagination when transaction volume grows` comment.

---

#### [MODIFY] [financial-transaction-custom-listing.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-listing/financial-transaction-custom-listing.component.html)

**Add click handler to mobile transaction cards**

Add `(click)="editTransaction(tx)"` and `role="button"` to the mobile card `<div>` elements so transactions are editable on mobile.

---

#### [MODIFY] [financial-transaction-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-add-edit/financial-transaction-custom-add-edit.component.ts) + [.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-add-edit/financial-transaction-custom-add-edit.component.html)

**Implement `hiddenFields` in the transaction add-edit template**

1. Add an `isFieldHidden(fieldName: string): boolean` method to the TS component (mirroring the auto-gen pattern):
```typescript
public isFieldHidden(fieldName: string): boolean {
    if (!this.hiddenFields) return false;
    return this.hiddenFields.includes(fieldName);
}
```

2. In the HTML template, wrap each field's parent `<div>` with `*ngIf="!isFieldHidden('fieldName')"` and clear validators for hidden required fields in `openModal()`.

---

#### [NEW] Custom Category Add-Edit Modal

Create a custom category add-edit modal matching the style/quality of the existing transaction add-edit modal.

##### [NEW] [financial-category-custom-add-edit.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-category-custom-add-edit/financial-category-custom-add-edit.component.ts)

New component inspired by the transaction add-edit pattern, with:
- Reactive form with fields: name, code, description, accountTypeId, financialOfficeId, parentFinancialCategoryId, isTaxApplicable, defaultAmount, color, sequence
- Category dropdown grouped by account type for parent selection
- Collapsible "More Details" section for optional fields (sequence, externalAccountId, color)
- Permission checks matching the auto-gen pattern
- `@Output() financialCategoryChanged` event
- `@Input() preSeededData` for partial field pre-seeding
- `@Input() hiddenFields` with `isFieldHidden()` support

##### [NEW] [financial-category-custom-add-edit.component.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-category-custom-add-edit/financial-category-custom-add-edit.component.html)

Modal template matching transaction add-edit styling (section-cards, section-labels, consistent buttons).

##### [NEW] [financial-category-custom-add-edit.component.scss](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-category-custom-add-edit/financial-category-custom-add-edit.component.scss)

Copy/adapt from transaction add-edit SCSS.

##### [MODIFY] [financial-category-custom-listing.component.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-category-custom-listing/financial-category-custom-listing.component.ts) + [.html](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-category-custom-listing/financial-category-custom-listing.component.html)

Replace `app-financial-category-add-edit` (auto-gen) with `app-financial-category-custom-add-edit` in both the template and the `@ViewChild` reference.

##### [MODIFY] [app.module.ts](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Client/src/app/app.module.ts)

Register `FinancialCategoryCustomAddEditComponent` in the `declarations` array.

---

#### [NEW] Server-Side Financial Aggregation Controller

##### [NEW] [FinancialTransactionsController.cs](file:///d:/source/repos/scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs)

Create a custom partial class extending the auto-generated `FinancialTransactionsController` with a single aggregation endpoint:

**`GET /api/FinancialTransaction/Summary`** — Returns aggregated financial data in one call:

```json
{
  "totalIncome": 125000.00,
  "totalExpenses": 89000.00,
  "netBalance": 36000.00,
  "transactionCount": 542,
  "monthlyBreakdown": [
    { "month": 1, "income": 10000, "expenses": 8000 },
    ...
  ]
}
```

Query parameters: `year` (int, optional), `financialOfficeId` (int, optional).

This replaces the dashboard's need to load all transactions client-side. The dashboard can call this single endpoint and get pre-aggregated data. Uses `_context.FinancialTransactions` with LINQ GroupBy for server-side computation.

> [!IMPORTANT]
> This is an additive endpoint — the existing auto-generated GET/POST/PUT methods remain unchanged. We use the partial class pattern established by `GiftsController.cs`.

---

## Verification Plan

### Build Verification
1. **Client build:** `cd d:\source\repos\scheduler\Scheduler\Scheduler.Client && ng build` — must complete with 0 errors
2. **Server build:** `cd d:\source\repos\scheduler && dotnet build Scheduler/Scheduler.Server/Scheduler.Server.csproj` — must complete with 0 errors

### Manual Verification (User)
- Navigate to `/finances` dashboard and select an office → verify the monthly chart updates to show only that office's data
- Navigate to `/finances/budgets`, select a fiscal period → verify actuals column shows only transactions from that period
- On mobile width, verify transaction cards in `/finances/transactions` are tappable and open the edit modal
- Navigate to `/finances/categories` and click "Add Category" → verify the new custom modal appears with the styled sections
