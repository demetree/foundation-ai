# Financial UI Enhancements — Implementation Plan

Six new features to wire up the `FinancialManagementService` backend to the Angular UI, plus new reports.

---

## 1. Invoice Action Bar (Record Payment, Void, Refund)

Enhance the existing [invoice-custom-detail.component](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/invoice-custom/invoice-custom-detail) to add contextual action buttons.

#### [MODIFY] invoice-custom-detail.component.html
- Add action dropdown: **Record Payment**, **Void Invoice**, **Issue Refund** (shown contextually based on invoice status)
- Record Payment → opens receipt-custom-add-edit modal pre-seeded with invoice ID
- Void → confirmation dialog → calls `POST /api/Invoices/Void/{id}` (new endpoint)
- Refund → confirmation + amount dialog → calls `POST /api/Invoices/Refund/{id}` (new endpoint)

#### [MODIFY] invoice-custom-detail.component.ts
- Add `voidInvoice()`, `issueRefund()`, `recordPayment()` methods
- Add status-based visibility flags (`canVoid`, `canRefund`, `canRecordPayment`)
- Wire to backend via HttpClient

#### [MODIFY] invoice-custom-detail.component.scss
- Style the action dropdown with status-aware coloring

#### [NEW] Server: Void/Refund API Endpoints
- `POST /api/Invoices/{id}/Void` → calls `FinancialManagementService.VoidInvoiceAsync`
- `POST /api/Invoices/{id}/Refund` → calls `FinancialManagementService.IssueRefundAsync`

---

## 2. A/R Aging Report

New standalone report component for accounts receivable aging.

#### [NEW] ar-aging-report/ (component TS, HTML, SCSS)
- Route: `finances/ar-aging`
- Buckets: Current, 1-30, 31-60, 61-90, 90+ days
- Groups by Client, with per-invoice rows
- Summary row with totals per bucket
- Export to Excel button

#### [NEW] Server: A/R Aging API Endpoint
- `GET /api/Invoices/AgingReport` → server-side computation grouping unpaid invoices by aging bucket

#### [MODIFY] financial-custom-dashboard
- Add "A/R Aging" button to Reports & Tools section
- Add navigation method

#### [MODIFY] app-routing.module.ts
- Add route for `finances/ar-aging`

#### [MODIFY] app.module.ts
- Declare `ArAgingReportComponent`

---

## 3. Revenue by Client Report

New report showing financial summary per client.

#### [NEW] revenue-by-client-report/ (component TS, HTML, SCSS)
- Route: `finances/revenue-by-client`
- Grouped table: Client → Total Invoiced, Total Paid, Balance Outstanding
- Fiscal year filter (reuse pattern from dashboard)
- Sortable columns
- Export button

#### [NEW] Server: Revenue by Client API Endpoint
- `GET /api/Invoices/RevenueByClient?year={year}` → server-side rollup

#### [MODIFY] financial-custom-dashboard
- Add "Revenue by Client" button to Reports & Tools section

#### [MODIFY] app-routing.module.ts / app.module.ts
- Add route and declare component

---

## 4. Gift Entry Form

Purpose-built donation entry that wires to `RecordGiftAsync`.

#### [NEW] gift-entry/ (component TS, HTML, SCSS)
- Route: `finances/gift-entry`
- Fields: Constituent picker, Fund, Campaign, Appeal, Amount, Date, Payment Type, Notes
- Optional pledge matching (autocomplete existing pledges)
- On save → calls `POST /api/Gifts/RecordGift` which routes through `FinancialManagementService.RecordGiftAsync`

#### [NEW] Server: RecordGift API Endpoint
- `POST /api/Gifts/RecordGift` → calls `FinancialManagementService.RecordGiftAsync`

#### [MODIFY] financial-custom-dashboard
- Add "Record Gift" button to header

#### [MODIFY] app-routing.module.ts / app.module.ts
- Add route and declare component

---

## 5. Pledge Fulfillment Dashboard

Visual progress toward pledge goals.

#### [NEW] pledge-dashboard/ (component TS, HTML, SCSS)
- Route: `finances/pledges`
- Cards showing: Total Pledged, Total Fulfilled, Outstanding Balance
- Per-pledge rows with progress bars (fulfilled/total)
- Filter by campaign/fund

#### [NEW] Server: Pledge Summary API Endpoint
- `GET /api/Pledges/Summary` → server-side aggregation with gift fulfillment data

#### [MODIFY] financial-custom-dashboard
- Add "Pledges" button to Reports & Tools section

#### [MODIFY] app-routing.module.ts / app.module.ts
- Add route and declare component

---

## 6. Dashboard Enhancements

Add quick-links to all new features from the existing dashboard.

#### [MODIFY] financial-custom-dashboard.component.html
- Add "A/R Aging" and "Revenue by Client" to Reports & Tools row
- Add "Record Gift" to header action buttons

#### [MODIFY] financial-custom-dashboard.component.ts
- Add navigation methods for new routes

---

## Verification Plan

### Build Verification
- `dotnet build` on Scheduler.Server to verify API endpoints compile
- `ng build` on Scheduler.Client to verify Angular components compile

### Manual Verification
- Navigate to each new route and verify rendering
- Test Invoice Void/Refund buttons with status-based visibility
- Verify A/R Aging report loads with correct aging buckets
