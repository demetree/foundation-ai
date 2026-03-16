# Financial Integrity Fixes

## P0 — Critical Fixes
- [x] Fix 1: Wrap `InvoicesController.CreateFromEventAsync` in DB transaction
- [x] Fix 4: Fix `GenerateNextInvoiceNumberAsync` race condition (lock + proper parsing)
- [x] Fix 5: Add idempotency check on `CreateFromEventAsync`
- [x] Fix 6: Add non-negative amount validation on `CreateFromEventAsync`
- [x] Fix 7: Wrap `ReceiptsController.CreateFromPaymentAsync` in DB transaction
- [x] Fix 10: Wrap `ReceiptsController.CreateFromInvoicePaymentAsync` in DB transaction
- [x] Fix 13: Fix `GenerateNextReceiptNumberAsync` race condition (lock + proper parsing)
- [x] Fix 14: Add amount validation on `CreateFromInvoicePaymentAsync`

## P1 — Important Fixes
- [x] Fix 2: Write `InvoiceChangeHistory` record in `CreateFromEventAsync`
- [x] Fix 3: Include after-state in `CreateFromEventAsync` audit event
- [x] Fix 8: Write `ReceiptChangeHistory` in `CreateFromPaymentAsync`
- [x] Fix 9: Include after-state in `CreateFromPaymentAsync` audit event
- [x] Fix 11: Write `ReceiptChangeHistory` in `CreateFromInvoicePaymentAsync`
- [x] Fix 12: Include after-state in `CreateFromInvoicePaymentAsync` audit event
- [x] Fix 15: FinancialTransactionsController audit detail already adequate (year, officeId, count)
- [x] Fix 16: Log warning when DocumentType missing in InvoicesController PDF gen
- [x] Fix 17: Log warning when DocumentType missing in ReceiptsController PDF gen

## P2 — Production Readiness
- [x] Fix 18: Replace `pageSize: 10000` in dashboard with `pageSize: 50`

## Verification
- [x] Build verification — `dotnet build` succeeded with 0 errors
