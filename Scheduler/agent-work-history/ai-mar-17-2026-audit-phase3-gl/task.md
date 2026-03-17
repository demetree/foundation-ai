# Phase 3 — Double-Entry General Ledger

## Database
- [x] Create SQL migration script (`Phase3_GeneralLedger.sql`)
- [x] Add GL tables to `SchedulerDatabaseGenerator.cs` for rescaffolding

## Backend Entities
- [x] Create `GeneralLedgerEntry.cs` POCO
- [x] Create `GeneralLedgerLine.cs` POCO
- [x] Register DbSets in `SchedulerContextCustom.cs`

## Backend Service
- [x] Add `PostToGeneralLedgerAsync()`
- [x] Add `PostReversalToGLAsync()`
- [x] Add `GetCashAccountIdAsync()` helper
- [x] Integrate into RecordExpenseAsync (DR expense, CR cash)
- [x] Integrate into RecordDirectRevenueAsync (DR cash, CR revenue)
- [x] Integrate into VoidTransactionAsync (reversal entry)
- [x] Add `GetTrialBalanceFromGLAsync()`
- [x] Add `GET /api/FinancialTransactions/GLTrialBalance` endpoint

## Verification
- [x] `dotnet build` (Scheduler.Server) — 0 errors
- [x] `dotnet build` (SchedulerDatabaseGenerator) — 0 errors
