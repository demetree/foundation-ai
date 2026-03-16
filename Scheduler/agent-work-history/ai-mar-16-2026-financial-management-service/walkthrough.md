# FinancialManagementService — Complete Walkthrough

## Summary

Created [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs) — the centralized financial operations layer (~1850 lines). All multi-entity financial operations now flow through this service, solving the "three disconnected money pipelines" problem.

## All 10 Operations

| Method | What It Does Atomically |
|--------|------------------------|
| `CreateInvoiceFromEventAsync` | Invoice + line items + FinancialTransaction (receivable) + EventCharge → "Invoiced" |
| `RecordInvoicePaymentAsync` | Receipt + recalculate `amountPaid` + cascade status + FinancialTransaction + EventCharge → "Paid" |
| `RecordExpenseAsync` | FinancialTransaction (expense) + fiscal period validation |
| `RecordDirectRevenueAsync` | FinancialTransaction (revenue) for ad-hoc income |
| `ReconcileInvoiceBalanceAsync` | Fixes stale `amountPaid` from Receipt source-of-truth |
| `RecordGiftAsync` | Gift + recalculate `Pledge.balanceAmount` + FinancialTransaction (Donation Revenue) |
| `VoidInvoiceAsync` | Status → "Voided" + reversing FinancialTransaction + EventCharge → "Pending" |
| `IssueRefundAsync` | Negative Receipt + recalculate `amountPaid` + reversing FinancialTransaction |
| `ReconcilePledgeBalanceAsync` | Fixes stale `Pledge.balanceAmount` from Gift source-of-truth |
| `CloseFiscalPeriodAsync` | Validates no unpaid invoices + locks period + records audit trail |

## Controller Refactoring

````carousel
### InvoicesController.CreateFromEventAsync
```diff
-  230 lines of inline logic
+   40 lines → FinancialManagementService.CreateInvoiceFromEventAsync()
```
**Gains**: FinancialTransaction ledger entry, EventCharge cascade, fiscal period validation
<!-- slide -->
### ReceiptsController.CreateFromInvoicePaymentAsync
```diff
-  160 lines (created Receipt ONLY — no balance update, no cascade, no ledger)
+   40 lines → FinancialManagementService.RecordInvoicePaymentAsync()
```
**Gains**: Invoice.amountPaid recalculation, status cascade, FinancialTransaction, fiscal period validation
````

> [!IMPORTANT]
> The receipt endpoint was the most critical fix — previously it **only** created a Receipt. Now a single payment atomically updates 5 related entities.

## Files Changed

| File | Change |
|------|--------|
| [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs) | **[NEW]** 10-operation financial service (~1850 lines) |
| [Program.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Program.cs#L125-L126) | Registered service in DI |
| [InvoicesController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/InvoicesController.cs#L41-L117) | Refactored to delegate |
| [ReceiptsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/ReceiptsController.cs#L214-L281) | Refactored to delegate |

## Build Verification

```
Build succeeded.
    0 Error(s)
```
