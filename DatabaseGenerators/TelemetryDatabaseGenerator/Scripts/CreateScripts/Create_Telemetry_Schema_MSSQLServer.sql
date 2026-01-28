/*
Foundation Telemetry Collection System database schema.
This module stores historical system health metrics collected from monitored Foundation applications.
It enables time-series analysis, capacity planning, and incident correlation by capturing
periodic snapshots of application metrics, database health, disk usage, user sessions,
and correlated error events from audit logs and log files.
*/
CREATE DATABASE [Telemetry]
GO

ALTER DATABASE [Telemetry] SET RECOVERY SIMPLE
GO

USE [Telemetry]
GO

CREATE SCHEMA [Telemetry]
GO

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE [Telemetry].[TelemetryLogError]
-- DROP TABLE [Telemetry].[TelemetryErrorEvent]
-- DROP TABLE [Telemetry].[TelemetryApplicationMetric]
-- DROP TABLE [Telemetry].[TelemetrySessionSnapshot]
-- DROP TABLE [Telemetry].[TelemetryDiskHealth]
-- DROP TABLE [Telemetry].[TelemetryDatabaseHealth]
-- DROP TABLE [Telemetry].[TelemetrySnapshot]
-- DROP TABLE [Telemetry].[TelemetryCollectionRun]
-- DROP TABLE [Telemetry].[TelemetryApplication]

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON [Telemetry].[TelemetryLogError] DISABLE
-- ALTER INDEX ALL ON [Telemetry].[TelemetryErrorEvent] DISABLE
-- ALTER INDEX ALL ON [Telemetry].[TelemetryApplicationMetric] DISABLE
-- ALTER INDEX ALL ON [Telemetry].[TelemetrySessionSnapshot] DISABLE
-- ALTER INDEX ALL ON [Telemetry].[TelemetryDiskHealth] DISABLE
-- ALTER INDEX ALL ON [Telemetry].[TelemetryDatabaseHealth] DISABLE
-- ALTER INDEX ALL ON [Telemetry].[TelemetrySnapshot] DISABLE
-- ALTER INDEX ALL ON [Telemetry].[TelemetryCollectionRun] DISABLE
-- ALTER INDEX ALL ON [Telemetry].[TelemetryApplication] DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON [Telemetry].[TelemetryLogError] REBUILD
-- ALTER INDEX ALL ON [Telemetry].[TelemetryErrorEvent] REBUILD
-- ALTER INDEX ALL ON [Telemetry].[TelemetryApplicationMetric] REBUILD
-- ALTER INDEX ALL ON [Telemetry].[TelemetrySessionSnapshot] REBUILD
-- ALTER INDEX ALL ON [Telemetry].[TelemetryDiskHealth] REBUILD
-- ALTER INDEX ALL ON [Telemetry].[TelemetryDatabaseHealth] REBUILD
-- ALTER INDEX ALL ON [Telemetry].[TelemetrySnapshot] REBUILD
-- ALTER INDEX ALL ON [Telemetry].[TelemetryCollectionRun] REBUILD
-- ALTER INDEX ALL ON [Telemetry].[TelemetryApplication] REBUILD

-- Registry of monitored Foundation applications.
CREATE TABLE [Telemetry].[TelemetryApplication]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[url] NVARCHAR(500) NULL,
	[isSelf] BIT NOT NULL DEFAULT 0,
	[firstSeen] DATETIME2(7) NOT NULL,
	[lastSeen] DATETIME2(7) NULL
)
GO

-- Index on the TelemetryApplication table's name field.
CREATE INDEX [I_TelemetryApplication_name] ON [Telemetry].[TelemetryApplication] ([name])
GO


-- Metadata about each telemetry collection cycle.
CREATE TABLE [Telemetry].[TelemetryCollectionRun]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[startTime] DATETIME2(7) NOT NULL,
	[endTime] DATETIME2(7) NULL,
	[applicationsPolled] INT NULL,
	[applicationsSucceeded] INT NULL,
	[errorMessage] NVARCHAR(MAX) NULL
)
GO

-- Index on the TelemetryCollectionRun table's startTime field.
CREATE INDEX [I_TelemetryCollectionRun_startTime] ON [Telemetry].[TelemetryCollectionRun] ([startTime])
GO


-- Core system health metrics captured per application per collection cycle.
CREATE TABLE [Telemetry].[TelemetrySnapshot]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[telemetryApplicationId] INT NOT NULL,		-- Link to the TelemetryApplication table.
	[telemetryCollectionRunId] INT NOT NULL,		-- Link to the TelemetryCollectionRun table.
	[collectedAt] DATETIME2(7) NOT NULL,
	[isOnline] BIT NOT NULL DEFAULT 1,
	[uptimeSeconds] BIGINT NULL,
	[memoryWorkingSetMB] FLOAT NULL,
	[memoryGcHeapMB] FLOAT NULL,
	[cpuPercent] FLOAT NULL,
	[threadPoolWorkerThreads] INT NULL,
	[threadPoolCompletionPortThreads] INT NULL,
	[threadPoolPendingWorkItems] INT NULL,
	[machineName] NVARCHAR(100) NULL,
	[dotNetVersion] NVARCHAR(50) NULL,
	[statusJson] NVARCHAR(MAX) NULL
	CONSTRAINT [FK_TelemetrySnapshot_TelemetryApplication_telemetryApplicationId] FOREIGN KEY ([telemetryApplicationId]) REFERENCES [Telemetry].[TelemetryApplication] ([id]),		-- Foreign key to the TelemetryApplication table.
	CONSTRAINT [FK_TelemetrySnapshot_TelemetryCollectionRun_telemetryCollectionRunId] FOREIGN KEY ([telemetryCollectionRunId]) REFERENCES [Telemetry].[TelemetryCollectionRun] ([id])		-- Foreign key to the TelemetryCollectionRun table.
)
GO

-- Index on the TelemetrySnapshot table's telemetryApplicationId field.
CREATE INDEX [I_TelemetrySnapshot_telemetryApplicationId] ON [Telemetry].[TelemetrySnapshot] ([telemetryApplicationId])
GO

-- Index on the TelemetrySnapshot table's telemetryCollectionRunId field.
CREATE INDEX [I_TelemetrySnapshot_telemetryCollectionRunId] ON [Telemetry].[TelemetrySnapshot] ([telemetryCollectionRunId])
GO

-- Index on the TelemetrySnapshot table's collectedAt field.
CREATE INDEX [I_TelemetrySnapshot_collectedAt] ON [Telemetry].[TelemetrySnapshot] ([collectedAt])
GO

-- Index on the TelemetrySnapshot table's telemetryApplicationId,collectedAt fields.
CREATE INDEX [I_TelemetrySnapshot_telemetryApplicationId_collectedAt] ON [Telemetry].[TelemetrySnapshot] ([telemetryApplicationId], [collectedAt])
GO


-- Database connectivity and health status per snapshot.
CREATE TABLE [Telemetry].[TelemetryDatabaseHealth]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[telemetrySnapshotId] INT NOT NULL,		-- Link to the TelemetrySnapshot table.
	[databaseName] NVARCHAR(100) NOT NULL,
	[isConnected] BIT NOT NULL DEFAULT 1,
	[status] NVARCHAR(50) NULL,
	[server] NVARCHAR(250) NULL,
	[provider] NVARCHAR(100) NULL,
	[errorMessage] NVARCHAR(MAX) NULL
	CONSTRAINT [FK_TelemetryDatabaseHealth_TelemetrySnapshot_telemetrySnapshotId] FOREIGN KEY ([telemetrySnapshotId]) REFERENCES [Telemetry].[TelemetrySnapshot] ([id])		-- Foreign key to the TelemetrySnapshot table.
)
GO

-- Index on the TelemetryDatabaseHealth table's telemetrySnapshotId field.
CREATE INDEX [I_TelemetryDatabaseHealth_telemetrySnapshotId] ON [Telemetry].[TelemetryDatabaseHealth] ([telemetrySnapshotId])
GO


-- Disk space and health metrics per snapshot.
CREATE TABLE [Telemetry].[TelemetryDiskHealth]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[telemetrySnapshotId] INT NOT NULL,		-- Link to the TelemetrySnapshot table.
	[driveName] NVARCHAR(10) NOT NULL,
	[driveLabel] NVARCHAR(100) NULL,
	[totalGB] FLOAT NULL,
	[freeGB] FLOAT NULL,
	[freePercent] FLOAT NULL,
	[status] NVARCHAR(50) NULL,
	[isApplicationDrive] BIT NOT NULL DEFAULT 0
	CONSTRAINT [FK_TelemetryDiskHealth_TelemetrySnapshot_telemetrySnapshotId] FOREIGN KEY ([telemetrySnapshotId]) REFERENCES [Telemetry].[TelemetrySnapshot] ([id])		-- Foreign key to the TelemetrySnapshot table.
)
GO

-- Index on the TelemetryDiskHealth table's telemetrySnapshotId field.
CREATE INDEX [I_TelemetryDiskHealth_telemetrySnapshotId] ON [Telemetry].[TelemetryDiskHealth] ([telemetrySnapshotId])
GO


-- Active user session counts per collection cycle.
CREATE TABLE [Telemetry].[TelemetrySessionSnapshot]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[telemetrySnapshotId] INT NOT NULL,		-- Link to the TelemetrySnapshot table.
	[activeSessionCount] INT NOT NULL DEFAULT 0,
	[expiredSessionCount] INT NULL,
	[oldestSessionStart] DATETIME2(7) NULL,
	[newestSessionStart] DATETIME2(7) NULL
	CONSTRAINT [FK_TelemetrySessionSnapshot_TelemetrySnapshot_telemetrySnapshotId] FOREIGN KEY ([telemetrySnapshotId]) REFERENCES [Telemetry].[TelemetrySnapshot] ([id])		-- Foreign key to the TelemetrySnapshot table.
)
GO

-- Index on the TelemetrySessionSnapshot table's telemetrySnapshotId field.
CREATE INDEX [I_TelemetrySessionSnapshot_telemetrySnapshotId] ON [Telemetry].[TelemetrySessionSnapshot] ([telemetrySnapshotId])
GO


-- Application-specific business metrics captured per snapshot.
CREATE TABLE [Telemetry].[TelemetryApplicationMetric]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[telemetrySnapshotId] INT NOT NULL,		-- Link to the TelemetrySnapshot table.
	[metricName] NVARCHAR(100) NOT NULL,
	[metricValue] NVARCHAR(500) NULL,
	[state] INT NULL,
	[dataType] INT NULL,
	[numericValue] FLOAT NULL,
	[category] NVARCHAR(100) NULL
	CONSTRAINT [FK_TelemetryApplicationMetric_TelemetrySnapshot_telemetrySnapshotId] FOREIGN KEY ([telemetrySnapshotId]) REFERENCES [Telemetry].[TelemetrySnapshot] ([id])		-- Foreign key to the TelemetrySnapshot table.
)
GO

-- Index on the TelemetryApplicationMetric table's telemetrySnapshotId field.
CREATE INDEX [I_TelemetryApplicationMetric_telemetrySnapshotId] ON [Telemetry].[TelemetryApplicationMetric] ([telemetrySnapshotId])
GO


-- Correlated error events copied from Auditor for forensic analysis.
CREATE TABLE [Telemetry].[TelemetryErrorEvent]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[telemetryApplicationId] INT NOT NULL,		-- Link to the TelemetryApplication table.
	[telemetrySnapshotId] INT NULL,		-- Link to the TelemetrySnapshot table.
	[originalAuditEventId] BIGINT NULL,
	[occurredAt] DATETIME2(7) NOT NULL,
	[auditTypeName] NVARCHAR(100) NULL,
	[moduleName] NVARCHAR(100) NULL,
	[entityName] NVARCHAR(100) NULL,
	[userName] NVARCHAR(500) NULL,
	[message] NVARCHAR(MAX) NULL,
	[errorMessage] NVARCHAR(MAX) NULL
	CONSTRAINT [FK_TelemetryErrorEvent_TelemetryApplication_telemetryApplicationId] FOREIGN KEY ([telemetryApplicationId]) REFERENCES [Telemetry].[TelemetryApplication] ([id]),		-- Foreign key to the TelemetryApplication table.
	CONSTRAINT [FK_TelemetryErrorEvent_TelemetrySnapshot_telemetrySnapshotId] FOREIGN KEY ([telemetrySnapshotId]) REFERENCES [Telemetry].[TelemetrySnapshot] ([id])		-- Foreign key to the TelemetrySnapshot table.
)
GO

-- Index on the TelemetryErrorEvent table's telemetryApplicationId field.
CREATE INDEX [I_TelemetryErrorEvent_telemetryApplicationId] ON [Telemetry].[TelemetryErrorEvent] ([telemetryApplicationId])
GO

-- Index on the TelemetryErrorEvent table's telemetrySnapshotId field.
CREATE INDEX [I_TelemetryErrorEvent_telemetrySnapshotId] ON [Telemetry].[TelemetryErrorEvent] ([telemetrySnapshotId])
GO

-- Index on the TelemetryErrorEvent table's occurredAt field.
CREATE INDEX [I_TelemetryErrorEvent_occurredAt] ON [Telemetry].[TelemetryErrorEvent] ([occurredAt])
GO


-- Error entries captured from application log files.
CREATE TABLE [Telemetry].[TelemetryLogError]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[telemetryApplicationId] INT NOT NULL,		-- Link to the TelemetryApplication table.
	[telemetrySnapshotId] INT NULL,		-- Link to the TelemetrySnapshot table.
	[capturedAt] DATETIME2(7) NOT NULL,
	[logFileName] NVARCHAR(250) NULL,
	[logTimestamp] DATETIME2(7) NULL,
	[level] NVARCHAR(50) NULL,
	[message] NVARCHAR(MAX) NULL,
	[exception] NVARCHAR(MAX) NULL
	CONSTRAINT [FK_TelemetryLogError_TelemetryApplication_telemetryApplicationId] FOREIGN KEY ([telemetryApplicationId]) REFERENCES [Telemetry].[TelemetryApplication] ([id]),		-- Foreign key to the TelemetryApplication table.
	CONSTRAINT [FK_TelemetryLogError_TelemetrySnapshot_telemetrySnapshotId] FOREIGN KEY ([telemetrySnapshotId]) REFERENCES [Telemetry].[TelemetrySnapshot] ([id])		-- Foreign key to the TelemetrySnapshot table.
)
GO

-- Index on the TelemetryLogError table's telemetryApplicationId field.
CREATE INDEX [I_TelemetryLogError_telemetryApplicationId] ON [Telemetry].[TelemetryLogError] ([telemetryApplicationId])
GO

-- Index on the TelemetryLogError table's telemetrySnapshotId field.
CREATE INDEX [I_TelemetryLogError_telemetrySnapshotId] ON [Telemetry].[TelemetryLogError] ([telemetrySnapshotId])
GO

-- Index on the TelemetryLogError table's capturedAt field.
CREATE INDEX [I_TelemetryLogError_capturedAt] ON [Telemetry].[TelemetryLogError] ([capturedAt])
GO


