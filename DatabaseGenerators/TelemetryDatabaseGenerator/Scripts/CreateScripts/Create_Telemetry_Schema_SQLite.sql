/*
Foundation Telemetry Collection System database schema.
This module stores historical system health metrics collected from monitored Foundation applications.
It enables time-series analysis, capacity planning, and incident correlation by capturing
periodic snapshots of application metrics, database health, disk usage, user sessions,
and correlated error events from audit logs and log files.
*/
/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "TelemetryLogError"
-- DROP TABLE "TelemetryErrorEvent"
-- DROP TABLE "TelemetryApplicationMetric"
-- DROP TABLE "TelemetrySessionSnapshot"
-- DROP TABLE "TelemetryDiskHealth"
-- DROP TABLE "TelemetryDatabaseHealth"
-- DROP TABLE "TelemetrySnapshot"
-- DROP TABLE "TelemetryCollectionRun"
-- DROP TABLE "TelemetryApplication"

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON "TelemetryLogError" DISABLE
-- ALTER INDEX ALL ON "TelemetryErrorEvent" DISABLE
-- ALTER INDEX ALL ON "TelemetryApplicationMetric" DISABLE
-- ALTER INDEX ALL ON "TelemetrySessionSnapshot" DISABLE
-- ALTER INDEX ALL ON "TelemetryDiskHealth" DISABLE
-- ALTER INDEX ALL ON "TelemetryDatabaseHealth" DISABLE
-- ALTER INDEX ALL ON "TelemetrySnapshot" DISABLE
-- ALTER INDEX ALL ON "TelemetryCollectionRun" DISABLE
-- ALTER INDEX ALL ON "TelemetryApplication" DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON "TelemetryLogError" REBUILD
-- ALTER INDEX ALL ON "TelemetryErrorEvent" REBUILD
-- ALTER INDEX ALL ON "TelemetryApplicationMetric" REBUILD
-- ALTER INDEX ALL ON "TelemetrySessionSnapshot" REBUILD
-- ALTER INDEX ALL ON "TelemetryDiskHealth" REBUILD
-- ALTER INDEX ALL ON "TelemetryDatabaseHealth" REBUILD
-- ALTER INDEX ALL ON "TelemetrySnapshot" REBUILD
-- ALTER INDEX ALL ON "TelemetryCollectionRun" REBUILD
-- ALTER INDEX ALL ON "TelemetryApplication" REBUILD

-- Registry of monitored Foundation applications.
CREATE TABLE "TelemetryApplication"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"url" VARCHAR(500) NULL COLLATE NOCASE,
	"isSelf" BIT NOT NULL DEFAULT 0,
	"firstSeen" DATETIME NOT NULL,
	"lastSeen" DATETIME NULL
);
-- Index on the TelemetryApplication table's name field.
CREATE INDEX "I_TelemetryApplication_name" ON "TelemetryApplication" ("name")
;


-- Metadata about each telemetry collection cycle.
CREATE TABLE "TelemetryCollectionRun"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"startTime" DATETIME NOT NULL,
	"endTime" DATETIME NULL,
	"applicationsPolled" INTEGER NULL,
	"applicationsSucceeded" INTEGER NULL,
	"errorMessage" TEXT NULL COLLATE NOCASE
);
-- Index on the TelemetryCollectionRun table's startTime field.
CREATE INDEX "I_TelemetryCollectionRun_startTime" ON "TelemetryCollectionRun" ("startTime")
;


-- Core system health metrics captured per application per collection cycle.
CREATE TABLE "TelemetrySnapshot"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"telemetryApplicationId" INTEGER NOT NULL,		-- Link to the TelemetryApplication table.
	"telemetryCollectionRunId" INTEGER NOT NULL,		-- Link to the TelemetryCollectionRun table.
	"collectedAt" DATETIME NOT NULL,
	"isOnline" BIT NOT NULL DEFAULT 1,
	"uptimeSeconds" BIGINT NULL,
	"memoryWorkingSetMB" REAL NULL,
	"memoryGcHeapMB" REAL NULL,
	"memoryPercent" REAL NULL,
	"cpuPercent" REAL NULL,
	"threadPoolWorkerThreads" INTEGER NULL,
	"threadPoolCompletionPortThreads" INTEGER NULL,
	"threadPoolPendingWorkItems" INTEGER NULL,
	"machineName" VARCHAR(100) NULL COLLATE NOCASE,
	"dotNetVersion" VARCHAR(50) NULL COLLATE NOCASE,
	"statusJson" TEXT NULL COLLATE NOCASE,
	FOREIGN KEY ("telemetryApplicationId") REFERENCES "TelemetryApplication"("id"),		-- Foreign key to the TelemetryApplication table.
	FOREIGN KEY ("telemetryCollectionRunId") REFERENCES "TelemetryCollectionRun"("id")		-- Foreign key to the TelemetryCollectionRun table.
);
-- Index on the TelemetrySnapshot table's telemetryApplicationId field.
CREATE INDEX "I_TelemetrySnapshot_telemetryApplicationId" ON "TelemetrySnapshot" ("telemetryApplicationId")
;

-- Index on the TelemetrySnapshot table's telemetryCollectionRunId field.
CREATE INDEX "I_TelemetrySnapshot_telemetryCollectionRunId" ON "TelemetrySnapshot" ("telemetryCollectionRunId")
;

-- Index on the TelemetrySnapshot table's collectedAt field.
CREATE INDEX "I_TelemetrySnapshot_collectedAt" ON "TelemetrySnapshot" ("collectedAt")
;

-- Index on the TelemetrySnapshot table's telemetryApplicationId,collectedAt fields.
CREATE INDEX "I_TelemetrySnapshot_telemetryApplicationId_collectedAt" ON "TelemetrySnapshot" ("telemetryApplicationId", "collectedAt")
;


-- Database connectivity and health status per snapshot.
CREATE TABLE "TelemetryDatabaseHealth"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"telemetrySnapshotId" INTEGER NOT NULL,		-- Link to the TelemetrySnapshot table.
	"databaseName" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"isConnected" BIT NOT NULL DEFAULT 1,
	"status" VARCHAR(50) NULL COLLATE NOCASE,
	"server" VARCHAR(250) NULL COLLATE NOCASE,
	"provider" VARCHAR(100) NULL COLLATE NOCASE,
	"errorMessage" TEXT NULL COLLATE NOCASE,
	FOREIGN KEY ("telemetrySnapshotId") REFERENCES "TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetryDatabaseHealth table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetryDatabaseHealth_telemetrySnapshotId" ON "TelemetryDatabaseHealth" ("telemetrySnapshotId")
;


-- Disk space and health metrics per snapshot.
CREATE TABLE "TelemetryDiskHealth"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"telemetrySnapshotId" INTEGER NOT NULL,		-- Link to the TelemetrySnapshot table.
	"driveName" VARCHAR(10) NOT NULL COLLATE NOCASE,
	"driveLabel" VARCHAR(100) NULL COLLATE NOCASE,
	"totalGB" REAL NULL,
	"freeGB" REAL NULL,
	"freePercent" REAL NULL,
	"usedPercent" REAL NULL,
	"status" VARCHAR(50) NULL COLLATE NOCASE,
	"isApplicationDrive" BIT NOT NULL DEFAULT 0,
	FOREIGN KEY ("telemetrySnapshotId") REFERENCES "TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetryDiskHealth table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetryDiskHealth_telemetrySnapshotId" ON "TelemetryDiskHealth" ("telemetrySnapshotId")
;


-- Active user session counts per collection cycle.
CREATE TABLE "TelemetrySessionSnapshot"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"telemetrySnapshotId" INTEGER NOT NULL,		-- Link to the TelemetrySnapshot table.
	"activeSessionCount" INTEGER NOT NULL DEFAULT 0,
	"expiredSessionCount" INTEGER NULL,
	"oldestSessionStart" DATETIME NULL,
	"newestSessionStart" DATETIME NULL,
	FOREIGN KEY ("telemetrySnapshotId") REFERENCES "TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetrySessionSnapshot table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetrySessionSnapshot_telemetrySnapshotId" ON "TelemetrySessionSnapshot" ("telemetrySnapshotId")
;


-- Application-specific business metrics captured per snapshot.
CREATE TABLE "TelemetryApplicationMetric"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"telemetrySnapshotId" INTEGER NOT NULL,		-- Link to the TelemetrySnapshot table.
	"metricName" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"metricValue" VARCHAR(500) NULL COLLATE NOCASE,
	"state" INTEGER NULL,
	"dataType" INTEGER NULL,
	"numericValue" REAL NULL,
	"category" VARCHAR(100) NULL COLLATE NOCASE,
	FOREIGN KEY ("telemetrySnapshotId") REFERENCES "TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetryApplicationMetric table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetryApplicationMetric_telemetrySnapshotId" ON "TelemetryApplicationMetric" ("telemetrySnapshotId")
;


-- Correlated error events copied from Auditor for forensic analysis.
CREATE TABLE "TelemetryErrorEvent"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"telemetryApplicationId" INTEGER NOT NULL,		-- Link to the TelemetryApplication table.
	"telemetrySnapshotId" INTEGER NULL,		-- Link to the TelemetrySnapshot table.
	"originalAuditEventId" BIGINT NULL,
	"occurredAt" DATETIME NOT NULL,
	"auditTypeName" VARCHAR(100) NULL COLLATE NOCASE,
	"moduleName" VARCHAR(100) NULL COLLATE NOCASE,
	"entityName" VARCHAR(100) NULL COLLATE NOCASE,
	"userName" VARCHAR(500) NULL COLLATE NOCASE,
	"message" TEXT NULL COLLATE NOCASE,
	"errorMessage" TEXT NULL COLLATE NOCASE,
	FOREIGN KEY ("telemetryApplicationId") REFERENCES "TelemetryApplication"("id"),		-- Foreign key to the TelemetryApplication table.
	FOREIGN KEY ("telemetrySnapshotId") REFERENCES "TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetryErrorEvent table's telemetryApplicationId field.
CREATE INDEX "I_TelemetryErrorEvent_telemetryApplicationId" ON "TelemetryErrorEvent" ("telemetryApplicationId")
;

-- Index on the TelemetryErrorEvent table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetryErrorEvent_telemetrySnapshotId" ON "TelemetryErrorEvent" ("telemetrySnapshotId")
;

-- Index on the TelemetryErrorEvent table's occurredAt field.
CREATE INDEX "I_TelemetryErrorEvent_occurredAt" ON "TelemetryErrorEvent" ("occurredAt")
;


-- Error entries captured from application log files.
CREATE TABLE "TelemetryLogError"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"telemetryApplicationId" INTEGER NOT NULL,		-- Link to the TelemetryApplication table.
	"telemetrySnapshotId" INTEGER NULL,		-- Link to the TelemetrySnapshot table.
	"capturedAt" DATETIME NOT NULL,
	"logFileName" VARCHAR(250) NULL COLLATE NOCASE,
	"logTimestamp" DATETIME NULL,
	"level" VARCHAR(50) NULL COLLATE NOCASE,
	"message" TEXT NULL COLLATE NOCASE,
	"exception" TEXT NULL COLLATE NOCASE,
	"occurrenceCount" INTEGER NOT NULL DEFAULT 1,		-- For deduplication - how many identical errors
	FOREIGN KEY ("telemetryApplicationId") REFERENCES "TelemetryApplication"("id"),		-- Foreign key to the TelemetryApplication table.
	FOREIGN KEY ("telemetrySnapshotId") REFERENCES "TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetryLogError table's telemetryApplicationId field.
CREATE INDEX "I_TelemetryLogError_telemetryApplicationId" ON "TelemetryLogError" ("telemetryApplicationId")
;

-- Index on the TelemetryLogError table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetryLogError_telemetrySnapshotId" ON "TelemetryLogError" ("telemetrySnapshotId")
;

-- Index on the TelemetryLogError table's capturedAt field.
CREATE INDEX "I_TelemetryLogError_capturedAt" ON "TelemetryLogError" ("capturedAt")
;


