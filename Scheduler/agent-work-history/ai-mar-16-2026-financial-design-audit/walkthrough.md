# Financial Integrity Fixes — Walkthrough

## What Was Done

Applied 18 fixes (P0–P2) to the custom financial controllers, following patterns established by the `GiftsController` and the auto-generated Foundation `DataControllers/`.

## Changes Made

### [InvoicesController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/InvoicesController.cs)

render_diffs(file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/InvoicesController.cs)

**Key changes:**
- `CreateFromEventAsync` — wrapped in `invoicePutSyncRoot` lock + `IDbContextTransaction`, writes `InvoiceChangeHistory`, logs serialized after-state in audit
- Added idempotency check (duplicate invoice per event returns error)
- Added non-negative amount validation
- `GenerateNextInvoiceNumber` — rewrote as synchronous method called inside lock; uses mask-aware prefix to filter by year, extracts trailing sequence correctly
- `GeneratePdfAsync` — logs audit warning when `DocumentType "Invoice"` is missing

---

### [ReceiptsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/ReceiptsController.cs)

render_diffs(file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/ReceiptsController.cs)

**Key changes:**
- `CreateFromPaymentAsync` — wrapped in `receiptPutSyncRoot` lock + `IDbContextTransaction`, writes `ReceiptChangeHistory`, logs serialized after-state
- `CreateFromInvoicePaymentAsync` — same treatment, plus validates `amount > 0` and `amount <= remaining balance`
- `GenerateNextReceiptNumber` — same fix as invoices (synchronous, inside lock, mask-aware prefix)
- `GeneratePdfAsync` — logs audit warning when `DocumentType "Receipt"` is missing

---

### [financial-custom-dashboard.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-custom-dashboard/financial-custom-dashboard.component.ts)

render_diffs(file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-custom-dashboard/financial-custom-dashboard.component.ts)

**Key change:**
- Reduced `pageSize` from `10000` to `50` — the dashboard only shows 10 most recent transactions; summary cards and charts already use server-side aggregation endpoints

## Verification

- ✅ `dotnet build Scheduler.Server.csproj` — **0 errors**, 142 warnings (pre-existing nullable reference type warnings, not related to this change)
