# Phase 3 — Double-Entry General Ledger

## Summary

Every financial operation now produces balanced debit/credit journal entries in a new General Ledger. The GL becomes the authoritative ledger for reporting, while `FinancialTransaction` remains the source document.

## What Changed

### Database Schema

#### [NEW] [SchedulerDatabaseGenerator.cs](file:///g:/source/repos/Scheduler/SchedulerDatabaseGenerator/SchedulerDatabaseGenerator.cs)
Added `GeneralLedgerEntry` and `GeneralLedgerLine` table definitions for rescaffolding.

#### [NEW] [Phase3_GeneralLedger.sql](file:///g:/source/repos/Scheduler/SchedulerDatabase/Migrations/Phase3_GeneralLedger.sql)
Idempotent SQL migration script — run against the Scheduler database before use.

> [!IMPORTANT]
> You must run `Phase3_GeneralLedger.sql` against the database before GL features will work.

### Entity POCOs

| File | Purpose |
|------|---------|
| [GeneralLedgerEntry.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/Database/GeneralLedgerEntry.cs) | Journal entry header — tenant, date, description, links to transaction/fiscal period |
| [GeneralLedgerLine.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/Database/GeneralLedgerLine.cs) | Debit/credit line within an entry, linked to a FinancialCategory (account) |
| [SchedulerContextCustom.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/Database/SchedulerContextCustom.cs) | DbSet registration + `OnModelCreatingPartial` configuration |

### Service Methods — [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)

| Method | Purpose |
|--------|---------|
| `GetCashAccountIdAsync()` | Finds Cash/Bank contra-account by name convention |
| `PostToGeneralLedgerAsync()` | Creates balanced journal entry with auto-incrementing number |
| `PostReversalToGLAsync()` | Posts reversal entry (swaps debits/credits of original) |
| `GetTrialBalanceFromGLAsync()` | Aggregates debits/credits per category from GL |

### GL Posting Integration

| Mutation | Debit | Credit |
|----------|-------|--------|
| `RecordExpenseAsync` | Expense category | Cash/Bank |
| `RecordDirectRevenueAsync` | Cash/Bank | Revenue category |
| `VoidTransactionAsync` | Reversal of original entry (swapped) | — |

### API Endpoint — [FinancialTransactionsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs)

| Endpoint | Description |
|----------|-------------|
| `GET /api/FinancialTransactions/GLTrialBalance` | GL-based trial balance with `?fiscalPeriodId=`, `?fromDate=`, `?toDate=` filters |

## Verification

| Check | Result |
|-------|--------|
| `dotnet build` (Scheduler.Server) | ✅ 0 errors |
| `dotnet build` (SchedulerDatabaseGenerator) | ✅ 0 errors |
