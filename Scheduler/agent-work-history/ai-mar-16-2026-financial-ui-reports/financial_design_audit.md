# Financial Subsystem — Design Audit

**Audit Date**: 2026-03-16  
**Scope**: All financial-related tables, controllers, services, and client components in `Scheduler.Server` and `Scheduler.Client`

---

## Executive Summary

The financial subsystem is **large and functionally comprehensive** — it covers transactions, invoices, receipts, payments, gifts/donations, rate sheets, event charges, deposits, fiscal periods, budgets, and financial categories. It also provides a dashboard with charts, Excel/PDF export, and report generation.

**However, the system has a clear two-tier quality divide:**

| Tier | Controller | Quality |
|------|-----------|---------|
| **Gold Standard** | `GiftsController` (hand-written) | ✅ DB transactions, version control, change history, optimistic concurrency, object GUID validation, full audit trail with before/after state |
| **Needs Work** | `InvoicesController`, `ReceiptsController`, `FinancialTransactionsController` (AI-developed) | ⚠️ Missing transactions, no concurrency protection, race-prone sequence generation, incomplete audit trail |

The auto-generated `DataControllers/` (which handle standard CRUD for all entities including invoices, transactions, etc.) **do** provide proper patterns: version control, change history records, DB transactions, optimistic concurrency locks, and full audit events. The concern is specifically in the **custom business-logic endpoints** that bypass these safeguards.

---

## Architecture Inventory

### Server — Custom Controllers (Business Logic)

| Controller | File | Endpoints | Purpose |
|-----------|------|-----------|---------|
| [InvoicesController](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/InvoicesController.cs) | Custom (partial) | `CreateFromEvent`, `NextInvoiceNumber`, `GeneratePdf` | Invoice creation from charges, PDF gen, number sequencing |
| [FinancialTransactionsController](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs) | Custom (partial) | `Summary`, `CategoryBreakdown`, `MonthlyBreakdown`, `ExportFinancialReport`, `EventFinancialTimeline`, `OutstandingDeposits` | Dashboard aggregation, Excel export, event timeline |
| [ReceiptsController](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/ReceiptsController.cs) | Custom (partial) | `CreateFromPayment`, `CreateFromInvoicePayment`, `NextReceiptNumber`, `GeneratePdf` | Receipt creation, PDF gen |
| [GiftsController](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/GiftsController.cs) | Custom (partial) | `PutGift`, `PostGift` | Gift CRUD with full integrity pattern |
| [RateSheetsController](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/RateSheetsController.cs) | Custom (partial) | `PutRateSheet`, `PostRateSheet`, `Resolve` | Rate CRUD with hierarchy validation, rate resolution |

### Server — Auto-Generated Data Controllers

All of these provide **proper CRUD with full Foundation patterns** (version control, change history, DB transactions, audit events):

`FinancialTransactionsController`, `InvoicesController`, `InvoiceLineItemsController`, `InvoiceStatusesController`, `InvoiceChangeHistoriesController`, `EventChargesController`, `PaymentTransactionsController`, `PaymentMethodsController`, `PaymentProvidersController`, `PaymentTypesController`, `FinancialOfficesController`, `FinancialCategoriesController`

### Server — Services

| Service | File |
|---------|------|
| [InvoicePdfService](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/InvoicePdfService.cs) | PDF invoice generation |
| [ReceiptPdfService](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/ReceiptPdfService.cs) | PDF receipt generation |

### Client — Custom Components

| Component Group | Components |
|----------------|------------|
| **Financial Dashboard** | `financial-custom-dashboard` (summary cards, monthly chart, category breakdown, outstanding deposits, recent transactions) |
| **Financial Transactions** | `financial-transaction-custom-add-edit`, `financial-transaction-custom-listing` |
| **Financial Categories** | `financial-category-custom-add-edit`, `financial-category-custom-listing` |
| **Budget Manager** | `financial-budget-manager` |
| **Invoices** | `invoice-custom-add-edit`, `invoice-custom-detail`, `invoice-custom-listing` |
| **Receipts** | `receipt-custom-add-edit`, `receipt-custom-detail`, `receipt-custom-listing` |
| **Payments** | `payment-custom-add-edit`, `payment-custom-detail`, `payment-custom-listing` |
| **Rate Sheets** | `rate-sheet-custom-add-edit`, `rate-sheet-custom-listing`, `rate-sheet-custom-table` |
| **Contact Financials** | `contact-financials-tab` |

---

## Critical Findings

### 🔴 CRITICAL: Invoice Number Generation Race Condition

**File**: [InvoicesController.cs#L379-L434](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/InvoicesController.cs#L379-L434)

`GenerateNextInvoiceNumberAsync` loads **all invoice numbers** into memory, then parses digits to find the max sequence. This has multiple problems:

1. **Race condition**: Two concurrent requests can generate the **same invoice number** — there is no locking or database-level sequence
2. **Fragile parsing**: Extracts "last 4 digits" from any string, which misparses masks like `INV-2026-0001` (extracts `2026` from the year portion, not `0001`)
3. **Performance**: Loads all invoice numbers to find the max, rather than using `MAX()` on the database
4. **Ignores the mask**: Hardcodes `INV-{year}-` prefix assumption regardless of what the actual `invoiceNumberMask` contains

> [!CAUTION]
> A duplicate invoice number could lead to accounting errors, failed PDF generation references, and regulatory compliance issues.

### 🔴 CRITICAL: Receipt Number Generation — Same Race Condition

**File**: [ReceiptsController.cs#L401-L447](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/ReceiptsController.cs#L401-L447)

`GenerateNextReceiptNumberAsync` has the **identical pattern** as the invoice number generator — load all, parse digits, no locking, no DB sequence.

### 🔴 CRITICAL: Invoice Creation Missing DB Transaction

**File**: [InvoicesController.cs#L47-L188](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/InvoicesController.cs#L47-L188)

`CreateFromEventAsync` calls `SaveChangesAsync` **twice** without a transaction:
1. First save: Creates the invoice record
2. Second save: Creates the line items

If the second save fails, you get an **orphaned invoice with no line items** — the totals are set but the line item detail is missing. Compare to `GiftsController.PostGift` which wraps both saves in `BeginTransactionAsync` + `CommitAsync`.

### 🔴 CRITICAL: Receipt Creation Missing DB Transaction

**Files**: [ReceiptsController.cs#L46-L139](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/ReceiptsController.cs#L46-L139) and [ReceiptsController.cs#L150-L218](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/ReceiptsController.cs#L150-L218)

Both `CreateFromPaymentAsync` and `CreateFromInvoicePaymentAsync` create a receipt record without any transaction scope. If any subsequent operation fails, there's no rollback.

---

### 🟡 IMPORTANT: Custom Write Endpoints Missing Change History

The `GiftsController` and `RateSheetsController` both write change history records (e.g., `GiftChangeHistory`, `RateSheetChangeHistory`) inside transactions on every create/update. The AI-developed controllers do **not** write change history from their custom endpoints:

| Endpoint | Writes Change History? |
|----------|----------------------|
| `InvoicesController.CreateFromEventAsync` | ❌ No `InvoiceChangeHistory` |
| `ReceiptsController.CreateFromPaymentAsync` | ❌ No `ReceiptChangeHistory` |
| `ReceiptsController.CreateFromInvoicePaymentAsync` | ❌ No `ReceiptChangeHistory` |

> [!NOTE]
> Standard CRUD via the auto-generated `DataControllers/` **does** write change history. The gap is only in the custom business-logic endpoints that bypass the standard CRUD path.

### 🟡 IMPORTANT: Audit Events Missing Before/After State

The `GiftsController` records before/after JSON state in audit events:
```csharp
CreateAuditEvent(AuditType.UpdateEntity, "...", true, id.ToString(),
    JsonSerializer.Serialize(cloneOfExisting),  // BEFORE state
    JsonSerializer.Serialize(gift),              // AFTER state
    null);
```

The AI-developed controllers only log summary messages:
```csharp
CreateAuditEventAsync(AuditType.CreateEntity,
    $"Invoice {invoiceNumber} created from event {eventId}...");  // No state snapshot
```

This means that for invoice/receipt creation via the custom endpoints, there is **no audit trail of the exact data that was saved** — only that "an invoice was created."

### 🟡 IMPORTANT: PDF Document Storage Silently Skips on Missing DocumentType

**Files**: [InvoicesController.cs#L336-L366](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/InvoicesController.cs#L336-L366), [ReceiptsController.cs#L358-L388](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/ReceiptsController.cs#L358-L388)

Both PDF generation endpoints look for a `DocumentType` named `"Invoice"` or `"Receipt"`. If it doesn't exist, they silently skip storing the PDF in the `Documents` table. The PDF is still returned to the user as a download, but **no record is persisted**. This means:
- No document audit trail
- Regenerating the same invoice produces a new document with no link to the previous version
- No way to retrieve the PDF later without regenerating it

### 🟡 IMPORTANT: Client Dashboard Loads All Transactions

**File**: [financial-custom-dashboard.component.ts#L162-L179](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-custom-dashboard/financial-custom-dashboard.component.ts#L162-L179)

The dashboard loads **all transactions** with `pageSize: 10000`:
```typescript
this.transactionService.GetFinancialTransactionList({
    pageSize: 10000  // TODO comment acknowledges this
})
```

The code already has server-side aggregation endpoints (`Summary`, `CategoryBreakdown`, `MonthlyBreakdown`) which are used for the charts. But the full transaction list is still loaded for the "recent transactions" section — this should use server-side pagination with a small page size.

---

### 🟢 What's Working Well

| Pattern | Status | Notes |
|---------|--------|-------|
| **Multi-tenancy enforcement** | ✅ All controllers | Every query filters by `tenantGuid` — no cross-tenant data leakage |
| **Security checks** | ✅ All endpoints | `DoesUserHaveReadPrivilegeSecurityCheckAsync` / `DoesUserHaveWritePrivilegeSecurityCheckAsync` on every endpoint |
| **Rate limiting** | ✅ All endpoints | Per-user rate limits on all endpoints |
| **UTC date enforcement** | ✅ Server-wide | `UtcDateTimeInterceptor` on all EF contexts, explicit UTC conversion in `GiftsController` |
| **Audit events** | ✅ All endpoints | Every endpoint logs audit events (though detail level varies) |
| **Standard CRUD integrity** | ✅ Auto-generated | Version control, change history, DB transactions, optimistic concurrency — all handled by Foundation code generation |
| **Gift integrity** | ✅ Exemplary | Full DB transactions, version control, change history, object GUID validation, before/after audit state, donor journey recalculation |
| **Rate Sheet integrity** | ✅ Exemplary | DB transactions, hierarchy validation, duplicate prevention, change history, before/after audit state |
| **Server-side aggregation** | ✅ Dashboard | Summary, category breakdown, and monthly breakdown all computed on the database, not client-side |
| **Excel export** | ✅ Professional | Well-formatted multi-sheet workbook with headers, styled rows, currency formatting |
| **PDF generation** | ✅ Professional | Clean layout with A4 dimensions, proper font sizing, color coding |
| **Client-side permissions** | ✅ Checked | Transaction add-edit checks `userIsSchedulerFinancialTransactionWriter()` before opening |
| **Form validation** | ✅ Reactive Forms | Required validators on critical fields (amount, date, category, currency) |
| **Quick-entry UX** | ✅ Thoughtful | "Save & Add Another" + localStorage memory for repeat entries |

---

## Recommendations (Priority Order)

### P0 — Must Fix Before Trusting Financial Data

1. **Replace invoice/receipt number generation with database sequences or at minimum use locking**
   - Wrap in `invoicePutSyncRoot` lock (the auto-generated code already defines this static lock), or better, use a DB-level atomic sequence
   - The digit-parsing logic should correctly parse based on the configured mask, not just grab "last 4 digits"

2. **Wrap `CreateFromEventAsync` in a DB transaction** — both the invoice creation and line item insertion must succeed or fail together

3. **Wrap `CreateFromPaymentAsync` and `CreateFromInvoicePaymentAsync` in DB transactions**

4. **Write change history records** from all custom write endpoints (`InvoiceChangeHistory`, `ReceiptChangeHistory`)

### P1 — Should Fix for Audit Trust

5. **Include before/after state** in audit events for all custom write endpoints — follow the `GiftsController` pattern

6. **Fail visibly** when `DocumentType` is missing for PDF storage — log a warning and/or return a note in the response, rather than silently skipping

7. **Add write permission check** on `CreateFromInvoicePaymentAsync` — it currently only checks the `amount` parameter via `[FromQuery]` without validation of the amount's relationship to the invoice balance

### P2 — Should Fix for Production Readiness

8. **Replace `pageSize: 10000`** on the dashboard with a dedicated "recent transactions" endpoint that returns only the latest N records, sorted server-side

9. **Add negative amount validation** on `CreateFromEventAsync` — currently no check that `extendedAmount`, `taxAmount`, or `totalAmount` are non-negative

10. **Add idempotency protection** on `CreateFromEventAsync` — calling it twice for the same event creates duplicate invoices with no warning

### P3 — Nice to Have

11. **Add unit tests** — there are currently zero tests anywhere in the Scheduler project
12. **Extract common PDF utility helpers** — `FormatMoney`, `FormatDate`, `DrawLabelValue`, `BuildAddress` are duplicated between `InvoicePdfService` and `ReceiptPdfService`
13. **Use DI for PDF services** — both are instantiated with `new InvoicePdfService()` inside controllers rather than injected

---

## Summary of Pattern Compliance

| Pattern | `GiftsController` | `RateSheetsController` | `InvoicesController` | `ReceiptsController` | `FinancialTransactionsController` |
|---------|:-:|:-:|:-:|:-:|:-:|
| DB Transactions | ✅ | ✅ | ❌ | ❌ | N/A (read-only custom) |
| Version Control | ✅ | ✅ | ❌ | ❌ | N/A |
| Change History | ✅ | ✅ | ❌ | ❌ | N/A |
| Object GUID Validation | ✅ | ✅ | ❌ | ❌ | N/A |
| Before/After Audit State | ✅ | ✅ | ❌ | ❌ | ❌ |
| Optimistic Concurrency Lock | ✅ | ✅ | ❌ | ❌ | N/A |
| UTC Date Enforcement | ✅ | ✅ | ⚠️ (partial) | ⚠️ (partial) | ✅ |
| Security Checks | ✅ | ✅ | ✅ | ✅ | ✅ |
| Tenant Isolation | ✅ | ✅ | ✅ | ✅ | ✅ |
| Rate Limiting | ✅ | ✅ | ✅ | ✅ | ✅ |

> [!IMPORTANT]
> The ❌ marks above apply **only to the custom business-logic endpoints**. Standard CRUD operations (create, update, delete via the auto-generated `DataControllers/`) **do** implement all of these patterns for every financial entity.
