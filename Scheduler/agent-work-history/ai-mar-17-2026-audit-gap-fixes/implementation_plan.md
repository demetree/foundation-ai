# Audit Gap Fixes — Implementation Plan

## Overview

5 fixes based on auditor assessment. Ordered by priority.

---

## Fix 1: Lock Down Financial CRUD Endpoints (HIGH)

### [MODIFY] [SchedulerDatabaseGenerator.cs](file:///g:/source/repos/Scheduler/SchedulerDatabaseGenerator/SchedulerDatabaseGenerator.cs)

Add `SetTableToBeReadonlyForControllerCreationPurposes()` to all tables managed by `FinancialManagementService`:

| Table | Line | Variable | Action |
|-------|------|----------|--------|
| `FinancialTransaction` | ~3106 | `financialTransactionTable` | **Lock** |
| `FiscalPeriod` | ~3070 | `fiscalPeriodTable` | **Lock** |
| `GeneralLedgerEntry` | ~3192 | `generalLedgerEntryTable` | **Lock** + fix write permission |
| `GeneralLedgerLine` | ~3224 | `generalLedgerLineTable` | **Lock** + fix write permission |
| `Invoice` | ~3444 | `invoiceTable` | **Lock** |
| `InvoiceLineItem` | ~3486 | `invoiceLineItemTable` | **Lock** |
| `Receipt` | ~3512 | `receiptTable` | **Lock** |
| `Gift` | ~4240 | `giftTable` | Already locked ✅ |

> [!NOTE]
> `FinancialCategory` stays writable — it has a UI-managed CRUD flow for category management.
> ChangeHistory tables (`FinancialTransactionChangeHistory`, `InvoiceChangeHistory`, `ReceiptChangeHistory`) already have Put/Post/Delete commented out in the generated code.
> After modifying the generator, you'll need to rescaffold for changes to take effect.

---

## Fix 2: GL Balance Validation (MEDIUM)

### [MODIFY] [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

Add a balance check in `PostToGeneralLedgerAsync` — after creating GL lines, verify `debitAmount == creditAmount`. If not balanced, throw before persisting.

---

## Fix 3: GL Reconciliation Endpoint (MEDIUM)

### [MODIFY] [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

Add `ReconcileGLAsync()` — cross-references `FinancialTransaction` ↔ `GeneralLedgerEntry`:
- Transactions missing GL entries
- GL entries without source transactions
- Amount mismatches

### [MODIFY] [FinancialTransactionsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs)

Add `GET /api/FinancialTransactions/GLReconciliation` endpoint.

---

## Fix 4: Fix userId=0 in Change History (LOW)

### [MODIFY] [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

Add `int userId = 0` parameter to `RecordExpenseAsync` and `RecordDirectRevenueAsync`, pass it through to change history.

### [MODIFY] [FinancialTransactionsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs)

Pass `securityUser.id` when calling the service methods.

---

## Fix 5: Remove pageSize:10000 (LOW)

### [MODIFY] 9 client component files

Remove the hardcoded `pageSize: 10000`. The server already handles unbounded pagination when no pageSize is specified.

---

## Verification

- `dotnet build` Scheduler.Server
- `dotnet build` SchedulerDatabaseGenerator
