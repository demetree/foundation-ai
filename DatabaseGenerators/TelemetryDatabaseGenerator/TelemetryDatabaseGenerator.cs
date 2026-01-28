//
// Telemetry Database Generator
//
// Defines the database schema for the Foundation Telemetry Collection System.
// This module enables historical time-series analysis of system health metrics
// across all monitored Foundation-based applications.
//
// Key capabilities:
// - Periodic snapshots of CPU, memory, disk, and thread pool metrics
// - Multi-database health tracking per application
// - Active user session tracking for concurrent user analysis
// - Correlated error events from Auditor and log files
// - Data retention with configurable purge policies
//
// AI-Generated: This file was developed with AI assistance (January 2026)
//
using Foundation.CodeGeneration;
using System.Collections.Generic;

namespace Foundation.Telemetry.Database
{
    /// <summary>
    /// 
    /// Database schema generator for the Foundation Telemetry Collection System.
    /// 
    /// Provides historical storage for system health metrics collected from
    /// Foundation-based applications. Enables trend analysis, capacity planning,
    /// and incident correlation.
    /// 
    /// </summary>
    public class TelemetryDatabaseGenerator : DatabaseGenerator
    {
        public TelemetryDatabaseGenerator() : base("Telemetry", "Telemetry")
        {
            database.comment = @"Foundation Telemetry Collection System database schema.
This module stores historical system health metrics collected from monitored Foundation applications.
It enables time-series analysis, capacity planning, and incident correlation by capturing
periodic snapshots of application metrics, database health, disk usage, user sessions,
and correlated error events from audit logs and log files.";

            this.database.SetSchemaName("Telemetry");


            #region Lookup Tables

            //
            // TelemetryApplication - Registry of monitored applications
            //
            // Stores the list of applications being monitored to avoid string duplication
            // in snapshot records. Each application has a unique name and URL.
            //
            Database.Table telemetryApplicationTable = database.AddTable("TelemetryApplication");
            telemetryApplicationTable.comment = "Registry of monitored Foundation applications.";
            telemetryApplicationTable.isWritable = true;
            telemetryApplicationTable.adminAccessNeededToWrite = true;
            telemetryApplicationTable.AddIdField();
            telemetryApplicationTable.AddString100Field("name", false).EnforceUniqueness().CreateIndex();
            telemetryApplicationTable.AddString500Field("url", true);
            telemetryApplicationTable.AddBoolField("isSelf", false, false);
            telemetryApplicationTable.AddDateTimeField("firstSeen", false);
            telemetryApplicationTable.AddDateTimeField("lastSeen", true);

            #endregion


            #region Collection Metadata

            //
            // TelemetryCollectionRun - Metadata about each collection cycle
            //
            // Tracks when collection runs occurred, how many applications were polled,
            // and whether there were any errors. Useful for diagnosing collection issues.
            //
            Database.Table telemetryCollectionRunTable = database.AddTable("TelemetryCollectionRun");
            telemetryCollectionRunTable.comment = "Metadata about each telemetry collection cycle.";
            telemetryCollectionRunTable.isWritable = true;
            telemetryCollectionRunTable.adminAccessNeededToWrite = true;
            telemetryCollectionRunTable.AddIdField();
            telemetryCollectionRunTable.AddDateTimeField("startTime", false).CreateIndex();
            telemetryCollectionRunTable.AddDateTimeField("endTime", true);
            telemetryCollectionRunTable.AddIntField("applicationsPolled", true);
            telemetryCollectionRunTable.AddIntField("applicationsSucceeded", true);
            telemetryCollectionRunTable.AddTextField("errorMessage", true);

            #endregion


            #region Core Snapshot Tables

            //
            // TelemetrySnapshot - Core metrics per application per collection cycle
            //
            // This is the primary table storing system health metrics. One row is created
            // for each application for each collection cycle. Contains memory, uptime,
            // thread pool metrics, and the raw JSON response for extensibility.
            //
            Database.Table telemetrySnapshotTable = database.AddTable("TelemetrySnapshot");
            telemetrySnapshotTable.comment = "Core system health metrics captured per application per collection cycle.";
            telemetrySnapshotTable.isWritable = true;
            telemetrySnapshotTable.adminAccessNeededToWrite = true;
            telemetrySnapshotTable.AddIdField();

            Database.Table.Field snapshotAppField = telemetrySnapshotTable.AddForeignKeyField("telemetryApplicationId", telemetryApplicationTable, false);
            telemetrySnapshotTable.AddForeignKeyField("telemetryCollectionRunId", telemetryCollectionRunTable, false);

            Database.Table.Field collectedAtField = telemetrySnapshotTable.AddDateTimeField("collectedAt", false);
            collectedAtField.CreateIndex();

            telemetrySnapshotTable.AddBoolField("isOnline", false, true);
            telemetrySnapshotTable.AddLongField("uptimeSeconds", true);

            //
            // Memory metrics - stored as doubles for precision
            //
            telemetrySnapshotTable.AddDoubleField("memoryWorkingSetMB", true);
            telemetrySnapshotTable.AddDoubleField("memoryGcHeapMB", true);

            //
            // CPU metrics
            //
            telemetrySnapshotTable.AddDoubleField("cpuPercent", true);

            //
            // Thread pool metrics
            //
            telemetrySnapshotTable.AddIntField("threadPoolWorkerThreads", true);
            telemetrySnapshotTable.AddIntField("threadPoolCompletionPortThreads", true);
            telemetrySnapshotTable.AddIntField("threadPoolPendingWorkItems", true);

            //
            // Environment info
            //
            telemetrySnapshotTable.AddString100Field("machineName", true);
            telemetrySnapshotTable.AddString50Field("dotNetVersion", true);

            //
            // Raw JSON for future extensibility
            //
            telemetrySnapshotTable.AddTextField("statusJson", true);

            //
            // Create composite index for efficient time-range queries per application
            //
            telemetrySnapshotTable.CreateIndexForFields(new Database.Table.Field[] { snapshotAppField, collectedAtField });


            //
            // TelemetryDatabaseHealth - Database connectivity status per snapshot
            //
            // Child table capturing the health of each database context (Security, Auditor, etc.)
            // for each snapshot. Enables tracking of database availability over time.
            //
            Database.Table telemetryDatabaseHealthTable = database.AddTable("TelemetryDatabaseHealth");
            telemetryDatabaseHealthTable.comment = "Database connectivity and health status per snapshot.";
            telemetryDatabaseHealthTable.isWritable = true;
            telemetryDatabaseHealthTable.adminAccessNeededToWrite = true;
            telemetryDatabaseHealthTable.AddIdField();
            telemetryDatabaseHealthTable.AddForeignKeyField("telemetrySnapshotId", telemetrySnapshotTable, false);
            telemetryDatabaseHealthTable.AddString100Field("databaseName", false);
            telemetryDatabaseHealthTable.AddBoolField("isConnected", false, true);
            telemetryDatabaseHealthTable.AddString50Field("status", true);
            telemetryDatabaseHealthTable.AddString250Field("server", true);
            telemetryDatabaseHealthTable.AddString100Field("provider", true);
            telemetryDatabaseHealthTable.AddTextField("errorMessage", true);


            //
            // TelemetryDiskHealth - Disk space metrics per snapshot
            //
            // Child table capturing disk usage for each drive on the host machine.
            // Enables tracking of disk space trends and early warning of capacity issues.
            //
            Database.Table telemetryDiskHealthTable = database.AddTable("TelemetryDiskHealth");
            telemetryDiskHealthTable.comment = "Disk space and health metrics per snapshot.";
            telemetryDiskHealthTable.isWritable = true;
            telemetryDiskHealthTable.adminAccessNeededToWrite = true;
            telemetryDiskHealthTable.AddIdField();
            telemetryDiskHealthTable.AddForeignKeyField("telemetrySnapshotId", telemetrySnapshotTable, false);
            telemetryDiskHealthTable.AddString10Field("driveName", false);
            telemetryDiskHealthTable.AddString100Field("driveLabel", true);
            telemetryDiskHealthTable.AddDoubleField("totalGB", true);
            telemetryDiskHealthTable.AddDoubleField("freeGB", true);
            telemetryDiskHealthTable.AddDoubleField("freePercent", true);
            telemetryDiskHealthTable.AddString50Field("status", true);
            telemetryDiskHealthTable.AddBoolField("isApplicationDrive", false, false);


            //
            // TelemetrySessionSnapshot - Active user session counts per snapshot
            //
            // Captures the number of active user sessions at each collection point.
            // Enables analysis of peak concurrent users and correlation with system load.
            //
            Database.Table telemetrySessionSnapshotTable = database.AddTable("TelemetrySessionSnapshot");
            telemetrySessionSnapshotTable.comment = "Active user session counts per collection cycle.";
            telemetrySessionSnapshotTable.isWritable = true;
            telemetrySessionSnapshotTable.adminAccessNeededToWrite = true;
            telemetrySessionSnapshotTable.AddIdField();
            telemetrySessionSnapshotTable.AddForeignKeyField("telemetrySnapshotId", telemetrySnapshotTable, false);
            telemetrySessionSnapshotTable.AddIntField("activeSessionCount", false, 0);
            telemetrySessionSnapshotTable.AddIntField("expiredSessionCount", true);
            telemetrySessionSnapshotTable.AddDateTimeField("oldestSessionStart", true);
            telemetrySessionSnapshotTable.AddDateTimeField("newestSessionStart", true);


            //
            // TelemetryApplicationMetric - Application-specific business metrics per snapshot
            //
            // Captures custom metrics from IApplicationMetricsProvider implementations.
            // Examples: active jobs, pending appointments, background tasks, queue depths.
            // Enables historical analysis of business-level health indicators.
            //
            Database.Table telemetryApplicationMetricTable = database.AddTable("TelemetryApplicationMetric");
            telemetryApplicationMetricTable.comment = "Application-specific business metrics captured per snapshot.";
            telemetryApplicationMetricTable.isWritable = true;
            telemetryApplicationMetricTable.adminAccessNeededToWrite = true;
            telemetryApplicationMetricTable.AddIdField();
            telemetryApplicationMetricTable.AddForeignKeyField("telemetrySnapshotId", telemetrySnapshotTable, false);
            telemetryApplicationMetricTable.AddString100Field("metricName", false);
            telemetryApplicationMetricTable.AddString500Field("metricValue", true);
            telemetryApplicationMetricTable.AddIntField("state", true); // 0=Normal, 1=Warning, 2=Critical
            telemetryApplicationMetricTable.AddIntField("dataType", true); // 0=Text, 1=Number, 2=Percentage
            telemetryApplicationMetricTable.AddDoubleField("numericValue", true);
            telemetryApplicationMetricTable.AddString100Field("category", true);

            #endregion


            #region Correlated Event Tables

            //
            // TelemetryErrorEvent - Correlated audit error events
            //
            // Stores copies of error-level audit events from the Auditor database.
            // These are linked to the nearest telemetry snapshot to enable forensic
            // analysis of "what did the system look like when this error occurred?"
            //
            Database.Table telemetryErrorEventTable = database.AddTable("TelemetryErrorEvent");
            telemetryErrorEventTable.comment = "Correlated error events copied from Auditor for forensic analysis.";
            telemetryErrorEventTable.isWritable = true;
            telemetryErrorEventTable.adminAccessNeededToWrite = true;
            telemetryErrorEventTable.AddIdField();
            telemetryErrorEventTable.AddForeignKeyField("telemetryApplicationId", telemetryApplicationTable, false);
            telemetryErrorEventTable.AddForeignKeyField("telemetrySnapshotId", telemetrySnapshotTable, true);
            telemetryErrorEventTable.AddLongField("originalAuditEventId", true);

            Database.Table.Field errorOccurredAtField = telemetryErrorEventTable.AddDateTimeField("occurredAt", false);
            errorOccurredAtField.CreateIndex();

            telemetryErrorEventTable.AddString100Field("auditTypeName", true);
            telemetryErrorEventTable.AddString100Field("moduleName", true);
            telemetryErrorEventTable.AddString100Field("entityName", true);
            telemetryErrorEventTable.AddString500Field("userName", true);
            telemetryErrorEventTable.AddTextField("message", true);
            telemetryErrorEventTable.AddTextField("errorMessage", true);


            //
            // TelemetryLogError - Correlated log file error entries
            //
            // Stores error-level entries captured from application log files.
            // Linked to the nearest telemetry snapshot for correlation with system state.
            //
            Database.Table telemetryLogErrorTable = database.AddTable("TelemetryLogError");
            telemetryLogErrorTable.comment = "Error entries captured from application log files.";
            telemetryLogErrorTable.isWritable = true;
            telemetryLogErrorTable.adminAccessNeededToWrite = true;
            telemetryLogErrorTable.AddIdField();
            telemetryLogErrorTable.AddForeignKeyField("telemetryApplicationId", telemetryApplicationTable, false);
            telemetryLogErrorTable.AddForeignKeyField("telemetrySnapshotId", telemetrySnapshotTable, true);

            Database.Table.Field capturedAtField = telemetryLogErrorTable.AddDateTimeField("capturedAt", false);
            capturedAtField.CreateIndex();

            telemetryLogErrorTable.AddString250Field("logFileName", true);
            telemetryLogErrorTable.AddDateTimeField("logTimestamp", true);
            telemetryLogErrorTable.AddString50Field("level", true);
            telemetryLogErrorTable.AddTextField("message", true);
            telemetryLogErrorTable.AddTextField("exception", true);

            #endregion
        }
    }
}
