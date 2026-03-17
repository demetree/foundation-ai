# Phase 3 — Double-Entry General Ledger

## Goal

Every financial operation produces balanced debit/credit journal entries. The GL becomes the authoritative ledger for reporting, while `FinancialTransaction` remains the source document.

## User Review Required

> [!IMPORTANT]
> This phase requires **new database tables**. A SQL migration script will be provided that must be run against the Scheduler database before the code will work.

> [!NOTE]
> The existing `AccountType` table already has `isRevenue` but lacks full accounting classification (Asset/Liability/Equity). Rather than modifying the existing scaffolded entity, we'll add a `normalBalance` column to guide debit/credit posting.

---

## Proposed Changes

### New SQL Tables

#### GeneralLedgerEntries
```sql
CREATE TABLE [dbo].[GeneralLedgerEntries] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [tenantGuid] UNIQUEIDENTIFIER NOT NULL,
    [journalEntryNumber] INT NOT NULL,
    [transactionDate] DATETIME NOT NULL,
    [description] NVARCHAR(500),
    [referenceNumber] NVARCHAR(100),
    [financialTransactionId] INT NULL,
    [fiscalPeriodId] INT NULL,
    [financialOfficeId] INT NULL,
    [postedBy] INT NOT NULL,
    [postedDate] DATETIME NOT NULL,
    [reversalOfId] INT NULL,
    [objectGuid] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [active] BIT NOT NULL DEFAULT 1,
    [deleted] BIT NOT NULL DEFAULT 0
);
```

#### GeneralLedgerLines
```sql
CREATE TABLE [dbo].[GeneralLedgerLines] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [generalLedgerEntryId] INT NOT NULL FOREIGN KEY REFERENCES GeneralLedgerEntries(id),
    [financialCategoryId] INT NOT NULL FOREIGN KEY REFERENCES FinancialCategories(id),
    [debitAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [creditAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [description] NVARCHAR(500)
);
```

---

### Backend

#### [NEW] Entity POCOs — `GeneralLedgerEntry.cs`, `GeneralLedgerLine.cs`
- Created manually in `SchedulerDatabase/Database/`
- Standard entity pattern matching existing entities

#### [MODIFY] [SchedulerContextCustom.cs](file:///g:/source/repos/Scheduler/SchedulerDatabase/Database/SchedulerContextCustom.cs)
- Add `DbSet<GeneralLedgerEntry>` and `DbSet<GeneralLedgerLine>` to the partial class

#### [MODIFY] [FinancialManagementService.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Services/FinancialManagementService.cs)
- Add `PostToGeneralLedgerAsync()` — creates balanced GL entries
- Integrate into `RecordExpenseAsync`: Debit expense category, Credit Cash/Bank
- Integrate into `RecordDirectRevenueAsync`: Debit Cash/Bank, Credit revenue category
- Integrate into `VoidTransactionAsync`: Post reversal entry (opposite of original)
- Add `GetTrialBalanceFromGLAsync()` — sum debits/credits per category from GL lines

#### [MODIFY] [FinancialTransactionsController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FinancialTransactionsController.cs)
- Add `GET /api/FinancialTransactions/GLTrialBalance` — GL-based trial balance

### Frontend

#### [MODIFY] Trial balance report (accountant-reports)
- Add toggle: "Source: Transactions | General Ledger"
- When GL selected, fetch from new endpoint

---

## Verification Plan

### Automated Tests
- `dotnet build` passes
- `ng build` passes

### Manual Verification
- Run SQL script against database
- Create expense/revenue transactions, verify GL entries are created
- Trial balance from GL shows balanced debits = credits
