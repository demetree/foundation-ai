# Financial Integrity Fixes — Implementation Plan

Fix all critical, important, and production-readiness issues identified in the [financial design audit](file:///C:/Users/demet/.gemini/antigravity/brain/978c06af-c466-4e0c-a8e4-e9bea572aa47/financial_design_audit.md).

## Proposed Changes

### InvoicesController — Custom Business Logic

#### [MODIFY] [InvoicesController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/InvoicesController.cs)

**Fix 1 — Wrap `CreateFromEventAsync` in a DB transaction** (P0)
- Add `using Microsoft.EntityFrameworkCore.Storage;` and `using System.Text.Json;`
- Wrap lines 153–182 (both `SaveChangesAsync` calls) in `using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))` + `transaction.CommitAsync()`
- On failure, the transaction auto-rolls back via `using` disposal

**Fix 2 — Write `InvoiceChangeHistory` record** (P1)
- Inside the transaction, after the second `SaveChangesAsync`, create and save an `InvoiceChangeHistory` record with the invoice's initial state (following the `GiftChangeHistory` pattern from `GiftsController`)

**Fix 3 — Include after-state in audit event** (P1)
- Change the `CreateAuditEventAsync` call at line 184 to include the serialized invoice state (6-argument overload with before=null, after=JSON)

**Fix 4 — Fix `GenerateNextInvoiceNumberAsync` race condition** (P0)
- Wrap the number generation + save inside the existing `invoicePutSyncRoot` lock (defined in the auto-generated partial class)
- Replace the "load all, extract digits" approach with a database `MAX()` query that filters by `yearPrefix` and extracts the trailing sequence: `_context.Invoices.Where(i => i.tenantGuid == tenantGuid && i.invoiceNumber.StartsWith(yearPrefix)).Select(i => i.invoiceNumber).ToListAsync()` — then extract only from matching records
- Fix the digit-parsing to use `yearPrefix`-aware extraction instead of blind "last 4 digits"

**Fix 5 — Add idempotency check** (P2)  
- Before creating the invoice, check if an invoice already exists for this `scheduledEventId` and warn/return it

**Fix 6 — Add non-negative amount validation** (P2)
- Validate that `subtotal`, `taxAmount`, `totalAmount` are all ≥ 0

---

### ReceiptsController — Custom Business Logic

#### [MODIFY] [ReceiptsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/ReceiptsController.cs)

**Fix 7 — Wrap `CreateFromPaymentAsync` in a DB transaction** (P0)
- Same pattern as Fix 1

**Fix 8 — Write `ReceiptChangeHistory` record in `CreateFromPaymentAsync`** (P1)
- Same pattern as Fix 2

**Fix 9 — Include after-state in audit event** (P1)
- Same pattern as Fix 3

**Fix 10 — Wrap `CreateFromInvoicePaymentAsync` in a DB transaction** (P0)
- Same pattern as Fix 1

**Fix 11 — Write `ReceiptChangeHistory` record in `CreateFromInvoicePaymentAsync`** (P1)
- Same pattern as Fix 2

**Fix 12 — Include after-state in audit event** (P1)
- Same pattern as Fix 3

**Fix 13 — Fix `GenerateNextReceiptNumberAsync` race condition** (P0)
- Same approach as Fix 4 — use `receiptPutSyncRoot` lock and proper mask-aware parsing

**Fix 14 — Add amount validation on `CreateFromInvoicePaymentAsync`** (P2)
- Validate `amount > 0` and `amount <= (invoice.totalAmount - invoice.amountPaid)`

---

### FinancialTransactionsController — Custom Endpoints

#### [MODIFY] [FinancialTransactionsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs)

**Fix 15 — Include audit event detail on read endpoints** (P1)
- The read-only endpoints (`Summary`, `CategoryBreakdown`, etc.) already log audit events but with minimal detail — enhance the log messages to include the query parameters used (year, officeId)

> [!NOTE]
> This controller only has read-only custom endpoints; all writes go through the auto-generated `DataControllers/` which already have full integrity patterns. No transaction/concurrency fixes needed here.

---

### PDF Storage Warning

#### [MODIFY] [InvoicesController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/InvoicesController.cs)

**Fix 16 — Log warning when DocumentType is missing** (P1)
- In the PDF generation method (`GeneratePdfAsync`), when `documentType` is null, log an audit warning instead of silently skipping

#### [MODIFY] [ReceiptsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/ReceiptsController.cs)

**Fix 17 — Log warning when DocumentType is missing** (P1)
- Same as Fix 16

---

### Client Dashboard

#### [MODIFY] [financial-custom-dashboard.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-custom-dashboard/financial-custom-dashboard.component.ts)

**Fix 18 — Replace `pageSize: 10000` load-all pattern** (P2)
- Change the transaction list load to only request `pageSize: 10` with `pageNumber: 1` and sort by `transactionDate` descending
- Remove the `allTransactions` cache and `getFilteredTransactions()` method since the summary data already comes from server-side endpoints
- The year/office filter already drives `loadCategoryBreakdown()` and `loadMonthlyBreakdown()` which use server-side aggregation — the client-side filtering is redundant

---

## Verification Plan

### Build Verification
```powershell
dotnet build g:\source\repos\Scheduler\Scheduler\Scheduler.Server\Scheduler.Server.csproj
```
The project must compile without errors after all changes.

### Manual Verification

Since there are no existing automated tests in the project, verification will be manual:

1. **Invoice creation from event**: Navigate to a scheduled event with charges → Create Invoice → Verify invoice and line items are created together (check DB directly) → Verify `InvoiceChangeHistory` record exists → Verify audit event contains serialized invoice state

2. **Invoice number uniqueness**: Create two invoices rapidly (or verify by reading the code that the lock is correctly placed) → Verify sequential numbers are generated without gaps or duplicates

3. **Receipt creation**: Create a receipt from a payment → Verify `ReceiptChangeHistory` record exists → Verify audit event contains serialized receipt state

4. **Dashboard load**: Open financial dashboard → Verify no network request loads 10,000 records anymore → Verify summary cards and charts still display correctly

> [!IMPORTANT]
> If the user has access to the running application, the manual tests above can be performed. Otherwise, we rely on build verification + code review of the patterns matching the established `GiftsController` gold standard.
