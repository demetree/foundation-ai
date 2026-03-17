# Phase 1 — Lock Down All Mutations

## Summary

All financial transaction creates, edits, and voids now route through `FinancialManagementService` with fiscal period validation, change history snapshots, and audit logging.

## What Changed

### Backend Service — [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

**`UpdateTransactionAsync()`** — validates:
- Original fiscal period is open (can't edit transactions in closed periods)
- New fiscal period is open if the date changed (can't move transactions into closed periods)
- Category exists and matches `isRevenue` flag
- Writes `FinancialTransactionChangeHistory` snapshot before applying changes
- Increments `versionNumber` for optimistic concurrency

**`VoidTransactionAsync(reason)`** — replaces soft-delete:
- Requires a reason string (rejects empty)
- Fiscal period must be open
- Sets `deleted = true` and appends `VOIDED: {reason}` to notes
- Change history records action type, reason, and full previous state as JSON

### Backend Controller — [FinancialTransactionsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs)

| Endpoint | Description |
|----------|-------------|
| `PUT /api/FinancialTransactions/{id}/Update` | Service-routed edit |
| `POST /api/FinancialTransactions/{id}/Void` | Void with reason |

### Frontend

| File | Change |
|------|--------|
| [financial-transaction-custom-add-edit.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-add-edit/financial-transaction-custom-add-edit.component.ts) | `putViaService()` replaces direct PUT for edits |
| [financial-transaction-custom-listing.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-listing/financial-transaction-custom-listing.component.ts) | `requestVoid()` / `executeVoid()` / `cancelVoid()` methods |
| [financial-transaction-custom-listing.component.html](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-listing/financial-transaction-custom-listing.component.html) | Void button per row + confirmation overlay with reason textarea |
| [financial-transaction-custom-listing.component.scss](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/financial-custom/financial-transaction-custom-listing/financial-transaction-custom-listing.component.scss) | `.btn-void-icon` / `.void-overlay` / `.void-dialog` styles |

## Verification

| Check | Result |
|-------|--------|
| `dotnet build` | ✅ 0 errors |
| `ng build` | ✅ No new errors |
