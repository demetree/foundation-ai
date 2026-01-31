/*
Foundation Telemetry Collection System database schema.
This module stores historical system health metrics collected from monitored Foundation applications.
It enables time-series analysis, capacity planning, and incident correlation by capturing
periodic snapshots of application metrics, database health, disk usage, user sessions,
and correlated error events from audit logs and log files.
*/
CREATE DATABASE "Telemetry"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    TEMPLATE=template0;

\connect "Telemetry"   -- Run the create database first, then connect to it, and run the rest if running in a query tool rather than through psql scripting.



CREATE SCHEMA "Telemetry"

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "Telemetry"."TelemetryLogError"
-- DROP TABLE "Telemetry"."TelemetryErrorEvent"
-- DROP TABLE "Telemetry"."TelemetryApplicationMetric"
-- DROP TABLE "Telemetry"."TelemetrySessionSnapshot"
-- DROP TABLE "Telemetry"."TelemetryDiskHealth"
-- DROP TABLE "Telemetry"."TelemetryDatabaseHealth"
-- DROP TABLE "Telemetry"."TelemetrySnapshot"
-- DROP TABLE "Telemetry"."TelemetryCollectionRun"
-- DROP TABLE "Telemetry"."TelemetryApplication"

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
CREATE TABLE "Telemetry"."TelemetryApplication"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"url" VARCHAR(500) NULL,
	"isSelf" BOOLEAN NOT NULL DEFAULT false,
	"firstSeen" TIMESTAMP NOT NULL,
	"lastSeen" TIMESTAMP NULL
);
-- Index on the TelemetryApplication table's name field.
CREATE INDEX "I_TelemetryApplication_name" ON "Telemetry"."TelemetryApplication" ("name")
;


-- Metadata about each telemetry collection cycle.
CREATE TABLE "Telemetry"."TelemetryCollectionRun"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"startTime" TIMESTAMP NOT NULL,
	"endTime" TIMESTAMP NULL,
	"applicationsPolled" INT NULL,
	"applicationsSucceeded" INT NULL,
	"errorMessage" TEXT NULL
);
-- Index on the TelemetryCollectionRun table's startTime field.
CREATE INDEX "I_TelemetryCollectionRun_startTime" ON "Telemetry"."TelemetryCollectionRun" ("startTime")
;


-- Core system health metrics captured per application per collection cycle.
CREATE TABLE "Telemetry"."TelemetrySnapshot"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"telemetryApplicationId" INT NOT NULL,		-- Link to the TelemetryApplication table.
	"telemetryCollectionRunId" INT NOT NULL,		-- Link to the TelemetryCollectionRun table.
	"collectedAt" TIMESTAMP NOT NULL,
	"isOnline" BOOLEAN NOT NULL DEFAULT true,
	"uptimeSeconds" BIGINT NULL,
	"memoryWorkingSetMB" DOUBLE PRECISION NULL,
	"memoryGcHeapMB" DOUBLE PRECISION NULL,
	"memoryPercent" DOUBLE PRECISION NULL,
	"systemMemoryPercent" DOUBLE PRECISION NULL,
	"cpuPercent" DOUBLE PRECISION NULL,
	"systemCpuPercent" DOUBLE PRECISION NULL,
	"threadPoolWorkerThreads" INT NULL,
	"threadPoolCompletionPortThreads" INT NULL,
	"threadPoolPendingWorkItems" INT NULL,
	"machineName" VARCHAR(100) NULL,
	"dotNetVersion" VARCHAR(50) NULL,
	"statusJson" TEXT NULL,
	CONSTRAINT "telemetryApplicationId" FOREIGN KEY ("telemetryApplicationId") REFERENCES "Telemetry"."TelemetryApplication"("id"),		-- Foreign key to the TelemetryApplication table.
	CONSTRAINT "telemetryCollectionRunId" FOREIGN KEY ("telemetryCollectionRunId") REFERENCES "Telemetry"."TelemetryCollectionRun"("id")		-- Foreign key to the TelemetryCollectionRun table.
);
-- Index on the TelemetrySnapshot table's telemetryApplicationId field.
CREATE INDEX "I_TelemetrySnapshot_telemetryApplicationId" ON "Telemetry"."TelemetrySnapshot" ("telemetryApplicationId")
;

-- Index on the TelemetrySnapshot table's telemetryCollectionRunId field.
CREATE INDEX "I_TelemetrySnapshot_telemetryCollectionRunId" ON "Telemetry"."TelemetrySnapshot" ("telemetryCollectionRunId")
;

-- Index on the TelemetrySnapshot table's collectedAt field.
CREATE INDEX "I_TelemetrySnapshot_collectedAt" ON "Telemetry"."TelemetrySnapshot" ("collectedAt")
;

-- Index on the TelemetrySnapshot table's telemetryApplicationId,collectedAt fields.
CREATE INDEX "I_TelemetrySnapshot_telemetryApplicationId_collectedAt" ON "Telemetry"."TelemetrySnapshot" ("telemetryApplicationId", "collectedAt")
;


-- Database connectivity and health status per snapshot.
CREATE TABLE "Telemetry"."TelemetryDatabaseHealth"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"telemetrySnapshotId" INT NOT NULL,		-- Link to the TelemetrySnapshot table.
	"databaseName" VARCHAR(100) NOT NULL,
	"isConnected" BOOLEAN NOT NULL DEFAULT true,
	"status" VARCHAR(50) NULL,
	"server" VARCHAR(250) NULL,
	"provider" VARCHAR(100) NULL,
	"errorMessage" TEXT NULL,
	CONSTRAINT "telemetrySnapshotId" FOREIGN KEY ("telemetrySnapshotId") REFERENCES "Telemetry"."TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetryDatabaseHealth table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetryDatabaseHealth_telemetrySnapshotId" ON "Telemetry"."TelemetryDatabaseHealth" ("telemetrySnapshotId")
;


-- Disk space and health metrics per snapshot.
CREATE TABLE "Telemetry"."TelemetryDiskHealth"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"telemetrySnapshotId" INT NOT NULL,		-- Link to the TelemetrySnapshot table.
	"driveName" VARCHAR(10) NOT NULL,
	"driveLabel" VARCHAR(100) NULL,
	"totalGB" DOUBLE PRECISION NULL,
	"freeGB" DOUBLE PRECISION NULL,
	"freePercent" DOUBLE PRECISION NULL,
	"usedPercent" DOUBLE PRECISION NULL,
	"status" VARCHAR(50) NULL,
	"isApplicationDrive" BOOLEAN NOT NULL DEFAULT false,
	CONSTRAINT "telemetrySnapshotId" FOREIGN KEY ("telemetrySnapshotId") REFERENCES "Telemetry"."TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetryDiskHealth table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetryDiskHealth_telemetrySnapshotId" ON "Telemetry"."TelemetryDiskHealth" ("telemetrySnapshotId")
;


-- Active user session counts per collection cycle.
CREATE TABLE "Telemetry"."TelemetrySessionSnapshot"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"telemetrySnapshotId" INT NOT NULL,		-- Link to the TelemetrySnapshot table.
	"activeSessionCount" INT NOT NULL DEFAULT 0,
	"expiredSessionCount" INT NULL,
	"oldestSessionStart" TIMESTAMP NULL,
	"newestSessionStart" TIMESTAMP NULL,
	CONSTRAINT "telemetrySnapshotId" FOREIGN KEY ("telemetrySnapshotId") REFERENCES "Telemetry"."TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetrySessionSnapshot table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetrySessionSnapshot_telemetrySnapshotId" ON "Telemetry"."TelemetrySessionSnapshot" ("telemetrySnapshotId")
;


-- Application-specific business metrics captured per snapshot.
CREATE TABLE "Telemetry"."TelemetryApplicationMetric"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"telemetrySnapshotId" INT NOT NULL,		-- Link to the TelemetrySnapshot table.
	"metricName" VARCHAR(100) NOT NULL,
	"metricValue" VARCHAR(500) NULL,
	"state" INT NULL,
	"dataType" INT NULL,
	"numericValue" DOUBLE PRECISION NULL,
	"category" VARCHAR(100) NULL,
	CONSTRAINT "telemetrySnapshotId" FOREIGN KEY ("telemetrySnapshotId") REFERENCES "Telemetry"."TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetryApplicationMetric table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetryApplicationMetric_telemetrySnapshotId" ON "Telemetry"."TelemetryApplicationMetric" ("telemetrySnapshotId")
;


-- Correlated error events copied from Auditor for forensic analysis.
CREATE TABLE "Telemetry"."TelemetryErrorEvent"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"telemetryApplicationId" INT NOT NULL,		-- Link to the TelemetryApplication table.
	"telemetrySnapshotId" INT NULL,		-- Link to the TelemetrySnapshot table.
	"originalAuditEventId" BIGINT NULL,
	"occurredAt" TIMESTAMP NOT NULL,
	"auditTypeName" VARCHAR(100) NULL,
	"moduleName" VARCHAR(100) NULL,
	"entityName" VARCHAR(100) NULL,
	"userName" VARCHAR(500) NULL,
	"message" TEXT NULL,
	"errorMessage" TEXT NULL,
	CONSTRAINT "telemetryApplicationId" FOREIGN KEY ("telemetryApplicationId") REFERENCES "Telemetry"."TelemetryApplication"("id"),		-- Foreign key to the TelemetryApplication table.
	CONSTRAINT "telemetrySnapshotId" FOREIGN KEY ("telemetrySnapshotId") REFERENCES "Telemetry"."TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetryErrorEvent table's telemetryApplicationId field.
CREATE INDEX "I_TelemetryErrorEvent_telemetryApplicationId" ON "Telemetry"."TelemetryErrorEvent" ("telemetryApplicationId")
;

-- Index on the TelemetryErrorEvent table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetryErrorEvent_telemetrySnapshotId" ON "Telemetry"."TelemetryErrorEvent" ("telemetrySnapshotId")
;

-- Index on the TelemetryErrorEvent table's occurredAt field.
CREATE INDEX "I_TelemetryErrorEvent_occurredAt" ON "Telemetry"."TelemetryErrorEvent" ("occurredAt")
;


-- Error entries captured from application log files.
CREATE TABLE "Telemetry"."TelemetryLogError"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"telemetryApplicationId" INT NOT NULL,		-- Link to the TelemetryApplication table.
	"telemetrySnapshotId" INT NULL,		-- Link to the TelemetrySnapshot table.
	"capturedAt" TIMESTAMP NOT NULL,
	"logFileName" VARCHAR(250) NULL,
	"logTimestamp" TIMESTAMP NULL,
	"level" VARCHAR(50) NULL,
	"message" TEXT NULL,
	"exception" TEXT NULL,
	"occurrenceCount" INT NOT NULL DEFAULT 1,		-- For deduplication - how many identical errors
	CONSTRAINT "telemetryApplicationId" FOREIGN KEY ("telemetryApplicationId") REFERENCES "Telemetry"."TelemetryApplication"("id"),		-- Foreign key to the TelemetryApplication table.
	CONSTRAINT "telemetrySnapshotId" FOREIGN KEY ("telemetrySnapshotId") REFERENCES "Telemetry"."TelemetrySnapshot"("id")		-- Foreign key to the TelemetrySnapshot table.
);
-- Index on the TelemetryLogError table's telemetryApplicationId field.
CREATE INDEX "I_TelemetryLogError_telemetryApplicationId" ON "Telemetry"."TelemetryLogError" ("telemetryApplicationId")
;

-- Index on the TelemetryLogError table's telemetrySnapshotId field.
CREATE INDEX "I_TelemetryLogError_telemetrySnapshotId" ON "Telemetry"."TelemetryLogError" ("telemetrySnapshotId")
;

-- Index on the TelemetryLogError table's capturedAt field.
CREATE INDEX "I_TelemetryLogError_capturedAt" ON "Telemetry"."TelemetryLogError" ("capturedAt")
;


