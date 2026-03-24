-- ============================================================================
-- Migration: Add storageKey column to Document table
-- Date: 2026-03-24
-- Description: Adds a nullable storageKey column to the Document table.
--              When NULL, binary content is in fileDataData (legacy behavior).
--              When set, binary content is in the configured storage provider.
-- ============================================================================

-- Check if the column already exists before adding it
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'Scheduler' 
      AND TABLE_NAME = 'Document' 
      AND COLUMN_NAME = 'storageKey'
)
BEGIN
    ALTER TABLE [Scheduler].[Document]
        ADD [storageKey] NVARCHAR(500) NULL;

    PRINT 'Added storageKey column to Document table.';
END
ELSE
BEGIN
    PRINT 'storageKey column already exists on Document table. Skipping.';
END
GO
