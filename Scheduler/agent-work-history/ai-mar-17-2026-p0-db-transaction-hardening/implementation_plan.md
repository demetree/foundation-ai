# P0 Fixes — DB Transaction Hardening

## Problem

4 service methods perform multiple `SaveChangesAsync()` calls (transaction + audit + GL posting) without a DB transaction. If any intermediate save fails, data is left in a partial state.

## Audit Results

| Method | Has Transaction? | # of SaveChanges | Fix Needed |
|--------|:---:|:---:|:---:|
| `CreateInvoiceFromEventAsync` | ✅ | 4 | No |
| `RecordInvoicePaymentAsync` | ✅ | 3 | No |
| `RecordGiftAsync` | ✅ | 2 | No |
| `VoidInvoiceAsync` | ✅ | 4 | No |
| `IssueRefundAsync` | ✅ | 3 | No |
| `GenerateFiscalYearAsync` | ✅ | 1 | No |
| `CloseFiscalPeriodAsync` | ✅ | 1 | No |
| **`RecordExpenseAsync`** | ❌ | 3 | **Yes** |
| **`RecordDirectRevenueAsync`** | ❌ | 3 | **Yes** |
| **`UpdateTransactionAsync`** | ❌ | 2 | **Yes** |
| **`VoidTransactionAsync`** | ❌ | 3 | **Yes** |

`ReceiptsController.CreateFromPaymentAsync` ✅ — already has transaction + change history.

## Proposed Changes

### [MODIFY] [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

Wrap the 4 methods in `BeginTransactionAsync`/`CommitAsync`:

1. **`RecordExpenseAsync`** (~line 1092): Wrap transaction + audit + GL posting in a single DB transaction
2. **`RecordDirectRevenueAsync`** (~line 1212): Same pattern
3. **`UpdateTransactionAsync`** (~line 2453): Wrap update + audit write
4. **`VoidTransactionAsync`** (~line 2611): Wrap void + audit + GL reversal

Pattern to follow (matching existing methods like `RecordGiftAsync`):
```csharp
using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
{
    try
    {
        // ... all SaveChangesAsync calls ...
        await transaction.CommitAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        // transaction auto-rolls-back on dispose
        return FinancialOperationResult.Fail(...);
    }
}
```

## Verification Plan

### Automated Tests
- `dotnet build` — verify clean compilation
