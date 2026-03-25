# Financial Dashboard Theme & UI Refactoring Plan

## Goal Description
1. Refactor the `financial-custom-dashboard` component to fully support the dynamic theme system (removing hardcoded colors/gradients).
2. Clean up the dashboard UI to prevent it from feeling cluttered.
3. Add missing navigation links to the custom Invoice and Receipt lists.
4. Establish actual routing for the custom transaction Add/Edit component, replacing the embedded modal approach.

## Proposed Changes

### 1. Theme Variables Refactoring (SCSS)
#### [MODIFY] `financial-custom-dashboard.component.scss`
Replace hardcoded colors and linear gradients with dynamic theme variables from `_themes.scss`.
- Replace `$gradient-finance` with `var(--sch-gradient-primary)`.
- Replace hardcoded `.accent-success`, `.accent-danger`, `.income-icon`, `.expense-icon` colors with dynamic equivalents (`var(--sch-success)`, `var(--sch-danger)` along with subtle transparency backgrounds).
- Replace chart bar static gradients with `var(--sch-success)` / `var(--sch-danger)`.
- Update the hover effects and functional colors to respect light/dark modes seamlessly.

### 2. UI Simplification and Navigation Additions (HTML/TS)
#### [MODIFY] `financial-custom-dashboard.component.html`
- **Action Bar Cleanup**: Reduce the number of buttons in the top header. Keep primary actions (New Transaction, Export) and move secondary links to a tidy menu or the lower "Reports & Tools" grid.
- **Invoices & Receipts**: Add beautiful, prominent entry points (e.g. new summary cards or well-placed buttons) linking to `/finances/invoices` and `/finances/receipts`.
- **Remove Modal**: Remove the embedded `<app-financial-transaction-custom-add-edit>` modal from the template.
- **Update Primary Buttons**: Change the "Record Income" and "Record Expense" buttons to route to the new endpoints (e.g., `routerLink="/finances/transactions/new"` with query params for the default type).

#### [MODIFY] `financial-custom-dashboard.component.ts`
- Implement new routing methods `goToInvoices()`, `goToReceipts()`, and update `recordIncome()` / `recordExpense()` to navigate via `this.router.navigate(...)` instead of opening a modal reference.
- Remove `@ViewChild('txAddEdit')`.

### 3. Routing Configuration
#### [MODIFY] `app-routing.module.ts`
- Import `FinancialTransactionCustomAddEditComponent`.
- Add routes for the transaction add/edit component to enable direct navigation:
  - `{ path: 'finances/transactions/new', component: FinancialTransactionCustomAddEditComponent, canActivate: [AuthGuard], title: 'New Transaction' }`
  - `{ path: 'finances/transactions/:id', component: FinancialTransactionCustomAddEditComponent, canActivate: [AuthGuard], title: 'Edit Transaction' }`

#### [MODIFY] `financial-transaction-custom-add-edit.component.ts` (If necessary)
- Ensure the component reads from activated route params `id` or query params `?type=revenue` to properly initialize if navigated to directly, since it was previously reliant on `@Input()` binding or being pre-seeded by the parent dashboard logic.

## Verification Plan

### Manual Verification
1. I will ask you to open the Financial Dashboard and toggle between themes (Light vs Dark modes like Midnight/Ocean) to ensure the header, icons, and charts dynamically adapt and look premium.
2. I will verify that clicking "Record Income" and "Record Expense" correctly routes to the standalone Add/Edit page.
3. I will verify that clicking the new "Invoices" and "Receipts" UI elements successfully routes to their respective custom lists.
