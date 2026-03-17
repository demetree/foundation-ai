-- ============================================================================
-- Phase 3: Double-Entry General Ledger Tables
-- Run against the Scheduler database
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GeneralLedgerEntries]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[GeneralLedgerEntries] (
        [id]                     INT              IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [tenantGuid]             UNIQUEIDENTIFIER NOT NULL,
        [journalEntryNumber]     INT              NOT NULL,
        [transactionDate]        DATETIME         NOT NULL,
        [description]            NVARCHAR(500)    NULL,
        [referenceNumber]        NVARCHAR(100)    NULL,
        [financialTransactionId] INT              NULL,
        [fiscalPeriodId]         INT              NULL,
        [financialOfficeId]      INT              NULL,
        [postedBy]               INT              NOT NULL,
        [postedDate]             DATETIME         NOT NULL,
        [reversalOfId]           INT              NULL,
        [objectGuid]             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [active]                 BIT              NOT NULL DEFAULT 1,
        [deleted]                BIT              NOT NULL DEFAULT 0
    );

    PRINT 'Created table GeneralLedgerEntries';
END
ELSE
    PRINT 'Table GeneralLedgerEntries already exists — skipped';
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GeneralLedgerLines]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[GeneralLedgerLines] (
        [id]                     INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [generalLedgerEntryId]   INT            NOT NULL,
        [financialCategoryId]    INT            NOT NULL,
        [debitAmount]            DECIMAL(18,2)  NOT NULL DEFAULT 0,
        [creditAmount]           DECIMAL(18,2)  NOT NULL DEFAULT 0,
        [description]            NVARCHAR(500)  NULL,

        CONSTRAINT [FK_GeneralLedgerLines_Entry]    FOREIGN KEY ([generalLedgerEntryId])  REFERENCES [dbo].[GeneralLedgerEntries]([id]),
        CONSTRAINT [FK_GeneralLedgerLines_Category] FOREIGN KEY ([financialCategoryId])   REFERENCES [dbo].[FinancialCategories]([id])
    );

    PRINT 'Created table GeneralLedgerLines';
END
ELSE
    PRINT 'Table GeneralLedgerLines already exists — skipped';
GO


-- Index for common queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GLE_TenantDate')
    CREATE INDEX [IX_GLE_TenantDate] ON [dbo].[GeneralLedgerEntries] ([tenantGuid], [transactionDate]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GLE_Transaction')
    CREATE INDEX [IX_GLE_Transaction] ON [dbo].[GeneralLedgerEntries] ([financialTransactionId]) WHERE [financialTransactionId] IS NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GLL_Entry')
    CREATE INDEX [IX_GLL_Entry] ON [dbo].[GeneralLedgerLines] ([generalLedgerEntryId]);

PRINT 'General Ledger indexes created';
GO
