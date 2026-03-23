using Foundation.CodeGeneration;
using System.Collections.Generic;

namespace Foundation.DeepSpace.Database
{
    /// <summary>
    /// Database schema generator for the DeepSpace storage management module.
    /// 
    /// DeepSpace is a Foundation storage abstraction layer providing "infinite local disk"
    /// to any Foundation application. This generator defines the metadata database that
    /// DeepSpace uses to track storage objects, providers, tiers, lifecycle rules,
    /// replication, and tenant quotas.
    /// 
    /// Primary target: SQLite (no SQL Server or PostgreSQL dependency).
    /// </summary>
    public class DeepspaceDatabaseGenerator : DatabaseGenerator
    {
        // ── Permission levels ──
        private const int DEEPSPACE_READER_PERMISSION_LEVEL = 1;
        private const int DEEPSPACE_WRITER_PERMISSION_LEVEL = 1;                    // Operational tables (access logs, objects)
        private const int DEEPSPACE_CONFIG_WRITER_PERMISSION_LEVEL = 50;            // Configuration tables (providers, rules)
        private const int DEEPSPACE_ADMIN_WRITER_PERMISSION_LEVEL = 100;            // Admin tables (quotas, replication)
        private const int DEEPSPACE_SUPER_ADMIN_WRITER_PERMISSION_LEVEL = 255;      // Master data (read-only code tables)


        public DeepspaceDatabaseGenerator() : base("DeepSpace", "DeepSpace")
        {
            database.comment = @"DeepSpace storage management database schema.
DeepSpace is a cornerstone storage abstraction layer in the Foundation stack. It provides any Foundation application
with the illusion of infinite, local disk — regardless of whether data lives on local SSD, S3, Azure Blob Storage,
or a combination of providers. This schema tracks storage objects, providers, tiers, lifecycle rules, replication
targets, tenant quotas, and access logs. Primary target is SQLite for self-contained deployments.";

            this.database.SetSchemaName("DeepSpace");



            // ══════════════════════════════════════════════════════════════════════
            //  Master Data Tables (read-only code tables)
            // ══════════════════════════════════════════════════════════════════════



            //
            // Storage Provider Types — the flavors of backing store DeepSpace supports
            //
            Database.Table storageProviderTypeTable = database.AddTable("StorageProviderType");
            storageProviderTypeTable.comment = "Master list of storage provider types supported by DeepSpace.";
            storageProviderTypeTable.isWritable = false;
            storageProviderTypeTable.AddIdField();
            storageProviderTypeTable.AddNameAndDescriptionFields(true, true);
            storageProviderTypeTable.AddSequenceField();
            storageProviderTypeTable.AddControlFields();

            storageProviderTypeTable.AddData(new Dictionary<string, string> { { "name", "Local" }, { "description", "Local filesystem storage" }, { "sequence", "1" }, { "objectGuid", "d5a00001-0001-0001-0001-000000000001" } });
            storageProviderTypeTable.AddData(new Dictionary<string, string> { { "name", "S3" }, { "description", "Amazon S3 or S3-compatible storage (MinIO, R2)" }, { "sequence", "2" }, { "objectGuid", "d5a00001-0001-0001-0001-000000000002" } });
            storageProviderTypeTable.AddData(new Dictionary<string, string> { { "name", "AzureBlob" }, { "description", "Azure Blob Storage" }, { "sequence", "3" }, { "objectGuid", "d5a00001-0001-0001-0001-000000000003" } });
            storageProviderTypeTable.AddData(new Dictionary<string, string> { { "name", "MinIO" }, { "description", "MinIO S3-compatible object storage" }, { "sequence", "4" }, { "objectGuid", "d5a00001-0001-0001-0001-000000000004" } });
            storageProviderTypeTable.AddData(new Dictionary<string, string> { { "name", "CloudflareR2" }, { "description", "Cloudflare R2 object storage (S3-compatible, zero egress)" }, { "sequence", "5" }, { "objectGuid", "d5a00001-0001-0001-0001-000000000005" } });



            //
            // Storage Tiers — the temperature tiers for lifecycle management
            //
            Database.Table storageTierTable = database.AddTable("StorageTier");
            storageTierTable.comment = "Master list of storage tiers for lifecycle management.";
            storageTierTable.isWritable = false;
            storageTierTable.AddIdField();
            storageTierTable.AddNameAndDescriptionFields(true, true);
            storageTierTable.AddSequenceField();
            storageTierTable.AddControlFields();

            storageTierTable.AddData(new Dictionary<string, string> { { "name", "Hot" }, { "description", "High-performance local storage. Sub-millisecond access, memory-mappable." }, { "sequence", "1" }, { "objectGuid", "d5a00002-0001-0001-0001-000000000001" } });
            storageTierTable.AddData(new Dictionary<string, string> { { "name", "Warm" }, { "description", "Standard cloud storage. ~100ms first byte, cost-effective for frequently accessed data." }, { "sequence", "2" }, { "objectGuid", "d5a00002-0001-0001-0001-000000000002" } });
            storageTierTable.AddData(new Dictionary<string, string> { { "name", "Cool" }, { "description", "Infrequent access cloud storage. ~200ms, lower cost with retrieval fees." }, { "sequence", "3" }, { "objectGuid", "d5a00002-0001-0001-0001-000000000003" } });
            storageTierTable.AddData(new Dictionary<string, string> { { "name", "Cold" }, { "description", "Archive storage. Minutes-to-hours retrieval, lowest cost for long-term retention." }, { "sequence", "4" }, { "objectGuid", "d5a00002-0001-0001-0001-000000000004" } });



            //
            // Access Types — types of storage operations for access logging
            //
            Database.Table accessTypeTable = database.AddTable("AccessType");
            accessTypeTable.comment = "Master list of storage access operation types.";
            accessTypeTable.isWritable = false;
            accessTypeTable.AddIdField();
            accessTypeTable.AddNameAndDescriptionFields(true, true);

            accessTypeTable.AddData(new Dictionary<string, string> { { "name", "Read" }, { "description", "Object read / download" } });
            accessTypeTable.AddData(new Dictionary<string, string> { { "name", "Write" }, { "description", "Object write / upload" } });
            accessTypeTable.AddData(new Dictionary<string, string> { { "name", "Delete" }, { "description", "Object deletion" } });
            accessTypeTable.AddData(new Dictionary<string, string> { { "name", "Copy" }, { "description", "Object copy within or across providers" } });
            accessTypeTable.AddData(new Dictionary<string, string> { { "name", "Move" }, { "description", "Object move (tier migration or provider transfer)" } });
            accessTypeTable.AddData(new Dictionary<string, string> { { "name", "ListKeys" }, { "description", "List objects by prefix" } });
            accessTypeTable.AddData(new Dictionary<string, string> { { "name", "PresignedUrl" }, { "description", "Presigned URL generation for direct client access" } });



            //
            // Migration Job Status — states for tier migration and replication jobs
            //
            Database.Table migrationJobStatusTable = database.AddTable("MigrationJobStatus");
            migrationJobStatusTable.comment = "Master list of migration/replication job statuses.";
            migrationJobStatusTable.isWritable = false;
            migrationJobStatusTable.AddIdField();
            migrationJobStatusTable.AddNameAndDescriptionFields(true, true);

            migrationJobStatusTable.AddData(new Dictionary<string, string> { { "name", "Pending" }, { "description", "Job created but not yet started" } });
            migrationJobStatusTable.AddData(new Dictionary<string, string> { { "name", "InProgress" }, { "description", "Job is actively running" } });
            migrationJobStatusTable.AddData(new Dictionary<string, string> { { "name", "Completed" }, { "description", "Job finished successfully" } });
            migrationJobStatusTable.AddData(new Dictionary<string, string> { { "name", "Failed" }, { "description", "Job encountered an error and stopped" } });
            migrationJobStatusTable.AddData(new Dictionary<string, string> { { "name", "Cancelled" }, { "description", "Job was cancelled by an administrator" } });



            // ══════════════════════════════════════════════════════════════════════
            //  Core Infrastructure Tables
            // ══════════════════════════════════════════════════════════════════════



            //
            // Storage Provider — registered backing store instances
            //
            Database.Table storageProviderTable = database.AddTable("StorageProvider");
            storageProviderTable.comment = "Registered storage provider instances. Each represents a configured backing store (a local path, an S3 bucket, an Azure container, etc.).";
            storageProviderTable.SetMinimumPermissionLevels(DEEPSPACE_READER_PERMISSION_LEVEL, DEEPSPACE_CONFIG_WRITER_PERMISSION_LEVEL);
            storageProviderTable.AddIdField();
            storageProviderTable.AddNameAndDescriptionFields(true, true);
            storageProviderTable.AddForeignKeyField("storageProviderTypeId", storageProviderTypeTable, false).AddScriptComments("The type of provider (Local, S3, Azure, etc.)");
            storageProviderTable.AddTextField("configJson").AddScriptComments("Provider configuration as JSON (connection strings, bucket names, paths, credentials reference). Sensitive values should reference environment variables or key vault.");
            storageProviderTable.AddBoolField("isDefault", false, false).AddScriptComments("Whether this is the default provider for new object storage");
            storageProviderTable.AddBoolField("isEnabled", false, true).AddScriptComments("Whether this provider is currently active and accepting operations");
            storageProviderTable.AddString50Field("healthStatus").AddScriptComments("Current health: Healthy, Degraded, Unavailable, Unknown");
            storageProviderTable.AddDateTimeField("lastHealthCheckUtc").AddScriptComments("Last time a health check was performed against this provider");
            storageProviderTable.AddIntField("totalCapacityBytes").AddScriptComments("Total capacity in bytes (null for cloud providers with unlimited capacity)");
            storageProviderTable.AddIntField("usedCapacityBytes").AddScriptComments("Currently used capacity in bytes");
            storageProviderTable.AddControlFields();

            Database.Table.Index storageProviderIdActiveDeletedIndex = storageProviderTable.CreateIndex("I_StorageProvider_id_active_deleted");
            storageProviderIdActiveDeletedIndex.AddField("id");
            storageProviderIdActiveDeletedIndex.AddField("active");
            storageProviderIdActiveDeletedIndex.AddField("deleted");



            //
            // Storage Object — the core registry of every object stored in DeepSpace
            //
            Database.Table storageObjectTable = database.AddTable("StorageObject");
            storageObjectTable.comment = "Core object registry. Every file stored in DeepSpace has a record here, tracking its location, tier, size, and metadata.";
            storageObjectTable.SetMinimumPermissionLevels(DEEPSPACE_READER_PERMISSION_LEVEL, DEEPSPACE_WRITER_PERMISSION_LEVEL);
            storageObjectTable.AddIdField();
            storageObjectTable.AddMultiTenantSupport();
            Database.Table.Field storageObjectKeyField = storageObjectTable.AddString1000Field("key", false);
            storageObjectKeyField.AddScriptComments("The storage key / path for this object (e.g., 'documents/report.pdf')");
            storageObjectKeyField.CreateIndex();
            storageObjectTable.AddForeignKeyField("storageProviderId", storageProviderTable, false).AddScriptComments("The provider currently holding this object");
            storageObjectTable.AddForeignKeyField("storageTierId", storageTierTable, false).AddScriptComments("The current storage tier (Hot, Warm, Cool, Cold)");
            storageObjectTable.AddIntField("sizeBytes", false).AddScriptComments("Object size in bytes");
            storageObjectTable.AddString250Field("contentType").AddScriptComments("MIME content type (e.g., 'application/pdf', 'image/png')");
            storageObjectTable.AddString100Field("md5Hash").AddScriptComments("MD5 hash of the object content for integrity verification");
            storageObjectTable.AddString100Field("sha256Hash").AddScriptComments("SHA-256 hash of the object content for integrity verification");
            storageObjectTable.AddGuidField("createdByUserGuid").AddScriptComments("The Security user GUID who created/uploaded this object (cross-database reference)");
            storageObjectTable.AddDateTimeField("lastAccessedUtc").AddScriptComments("Last time this object was accessed (read or downloaded). Used by lifecycle rules.").CreateIndex();
            storageObjectTable.AddIntField("accessCount", false, 0).AddScriptComments("Total number of times this object has been accessed. Used by lifecycle rules.");
            storageObjectTable.AddBoolField("isDeleted", false, false).AddScriptComments("Soft delete flag for objects. Allows recovery before permanent deletion.");
            storageObjectTable.AddDateTimeField("deletedUtc").AddScriptComments("When the object was soft-deleted");
            storageObjectTable.AddGuidField("deletedByUserGuid").AddScriptComments("The Security user GUID who deleted this object (cross-database reference)");
            storageObjectTable.AddVersionControl();
            storageObjectTable.AddControlFields();

            // Non Composite index for key lookups.  Note composite index with tenantguid is created by foundation's .CreatIndex on the key field
            storageObjectTable.CreateIndexForFields(new List<string>() { "key" });

            Database.Table.Index storageObjectIdActiveDeletedIndex = storageObjectTable.CreateIndex("I_StorageObject_id_active_deleted");
            storageObjectIdActiveDeletedIndex.AddField("id");
            storageObjectIdActiveDeletedIndex.AddField("active");
            storageObjectIdActiveDeletedIndex.AddField("deleted");



            //
            // Storage Object Version — version history for objects
            //
            Database.Table storageObjectVersionTable = database.AddTable("StorageObjectVersion");
            storageObjectVersionTable.comment = "Version history for storage objects. Each version represents a prior state of an object before it was updated.";
            storageObjectVersionTable.SetMinimumPermissionLevels(DEEPSPACE_READER_PERMISSION_LEVEL, DEEPSPACE_WRITER_PERMISSION_LEVEL);
            storageObjectVersionTable.AddIdField();
            storageObjectVersionTable.AddForeignKeyField("storageObjectId", storageObjectTable, false).AddScriptComments("The object this version belongs to");
            storageObjectVersionTable.AddIntField("versionNumber", false).AddScriptComments("Sequential version number (1, 2, 3, ...)");
            storageObjectVersionTable.AddForeignKeyField("storageProviderId", storageProviderTable, false).AddScriptComments("The provider holding this version's data");
            storageObjectVersionTable.AddString1000Field("providerKey").AddScriptComments("The actual key/path in the provider for this version's data (may differ from current key)");
            storageObjectVersionTable.AddIntField("sizeBytes", false).AddScriptComments("Size of this version in bytes");
            storageObjectVersionTable.AddString100Field("md5Hash").AddScriptComments("MD5 hash of this version");
            storageObjectVersionTable.AddGuidField("createdByUserGuid").AddScriptComments("The Security user GUID who created this version (cross-database reference)");
            storageObjectVersionTable.AddDateTimeField("createdUtc", false).AddScriptComments("When this version was created");
            storageObjectVersionTable.AddControlFields();

            // Composite index for fast version lookups: object + version number
            storageObjectVersionTable.CreateIndexForFields(new List<string>() { "storageObjectId", "versionNumber" });

            Database.Table.Index storageObjectVersionIdActiveDeletedIndex = storageObjectVersionTable.CreateIndex("I_StorageObjectVersion_id_active_deleted");
            storageObjectVersionIdActiveDeletedIndex.AddField("id");
            storageObjectVersionIdActiveDeletedIndex.AddField("active");
            storageObjectVersionIdActiveDeletedIndex.AddField("deleted");



            // ══════════════════════════════════════════════════════════════════════
            //  Lifecycle & Tiering Tables
            // ══════════════════════════════════════════════════════════════════════



            //
            // Lifecycle Rule — tenant-scoped rules for automatic tier migration
            //
            Database.Table lifecycleRuleTable = database.AddTable("LifecycleRule");
            lifecycleRuleTable.comment = "Tenant-scoped rules for automatic tier migration. Objects matching a rule's criteria are moved between storage tiers based on age and access patterns.";
            lifecycleRuleTable.SetMinimumPermissionLevels(DEEPSPACE_READER_PERMISSION_LEVEL, DEEPSPACE_CONFIG_WRITER_PERMISSION_LEVEL);
            lifecycleRuleTable.AddIdField();
            lifecycleRuleTable.AddMultiTenantSupport();
            lifecycleRuleTable.AddNameAndDescriptionFields(true, true, false);
            lifecycleRuleTable.AddString500Field("keyPrefixPattern", false).AddScriptComments("Key prefix to match (e.g., 'cdf/', 'images/'). Supports glob patterns.");
            lifecycleRuleTable.AddIntField("minAgeDays", false).AddScriptComments("Minimum age in days before the rule applies");
            lifecycleRuleTable.AddForeignKeyField("sourceStorageTierId", storageTierTable, false).AddScriptComments("Source tier — objects currently in this tier are candidates");
            lifecycleRuleTable.AddForeignKeyField("targetStorageTierId", storageTierTable, false).AddScriptComments("Target tier — objects will be moved to this tier");
            lifecycleRuleTable.AddIntField("minAccessCount").AddScriptComments("Minimum access count threshold (null = not considered). Objects below this count are candidates for colder tiers.");
            lifecycleRuleTable.AddBoolField("isEnabled", false, true).AddScriptComments("Whether this rule is currently active");
            lifecycleRuleTable.AddIntField("priority", false, 0).AddScriptComments("Rule evaluation priority (lower = higher priority). First matching rule wins.");
            lifecycleRuleTable.AddDateTimeField("lastEvaluatedUtc").AddScriptComments("Last time this rule was evaluated by the lifecycle worker");
            lifecycleRuleTable.AddVersionControl();
            lifecycleRuleTable.AddControlFields();

            Database.Table.Index lifecycleRuleIdActiveDeletedIndex = lifecycleRuleTable.CreateIndex("I_LifecycleRule_id_active_deleted");
            lifecycleRuleIdActiveDeletedIndex.AddField("id");
            lifecycleRuleIdActiveDeletedIndex.AddField("active");
            lifecycleRuleIdActiveDeletedIndex.AddField("deleted");



            //
            // Migration Job — tracks active and completed tier migration operations
            //
            Database.Table migrationJobTable = database.AddTable("MigrationJob");
            migrationJobTable.comment = "Tracks tier migration and replication jobs. Each job represents a single object being moved between providers/tiers.";
            migrationJobTable.SetMinimumPermissionLevels(DEEPSPACE_READER_PERMISSION_LEVEL, DEEPSPACE_ADMIN_WRITER_PERMISSION_LEVEL);
            migrationJobTable.AddIdField();
            migrationJobTable.AddForeignKeyField("lifecycleRuleId", lifecycleRuleTable, true).AddScriptComments("The lifecycle rule that triggered this job (null for manual migrations)");
            migrationJobTable.AddForeignKeyField("storageObjectId", storageObjectTable, false).AddScriptComments("The object being migrated");
            migrationJobTable.AddForeignKeyField("sourceStorageProviderId", storageProviderTable, false).AddScriptComments("The provider the object is being moved from");
            migrationJobTable.AddForeignKeyField("targetStorageProviderId", storageProviderTable, false).AddScriptComments("The provider the object is being moved to");
            migrationJobTable.AddForeignKeyField("migrationJobStatusId", migrationJobStatusTable, false).AddScriptComments("Current status of this migration job");
            migrationJobTable.AddDateTimeField("startedUtc").AddScriptComments("When the job started executing");
            migrationJobTable.AddDateTimeField("completedUtc").AddScriptComments("When the job finished (success or failure)");
            migrationJobTable.AddIntField("bytesTransferred").AddScriptComments("Number of bytes transferred so far");
            migrationJobTable.AddTextField("errorMessage").AddScriptComments("Error details if the job failed");
            migrationJobTable.AddIntField("retryCount", false, 0).AddScriptComments("Number of retry attempts");
            migrationJobTable.AddControlFields();
            migrationJobTable.AddSortSequence("id", true);

            Database.Table.Index migrationJobIdActiveDeletedIndex = migrationJobTable.CreateIndex("I_MigrationJob_id_active_deleted");
            migrationJobIdActiveDeletedIndex.AddField("id");
            migrationJobIdActiveDeletedIndex.AddField("active");
            migrationJobIdActiveDeletedIndex.AddField("deleted");



            // ══════════════════════════════════════════════════════════════════════
            //  Replication & Tenant Management Tables
            // ══════════════════════════════════════════════════════════════════════



            //
            // Replication Target — cross-provider replication configuration
            //
            Database.Table replicationTargetTable = database.AddTable("ReplicationTarget");
            replicationTargetTable.comment = "Cross-provider replication configuration. Defines rules for automatically replicating objects from one provider to another for redundancy.";
            replicationTargetTable.SetMinimumPermissionLevels(DEEPSPACE_READER_PERMISSION_LEVEL, DEEPSPACE_ADMIN_WRITER_PERMISSION_LEVEL);
            replicationTargetTable.AddIdField();
            replicationTargetTable.AddNameAndDescriptionFields(true, true, false);
            replicationTargetTable.AddForeignKeyField("sourceStorageProviderId", storageProviderTable, false).AddScriptComments("Source provider to replicate from");
            replicationTargetTable.AddForeignKeyField("targetStorageProviderId", storageProviderTable, false).AddScriptComments("Target provider to replicate to");
            replicationTargetTable.AddString500Field("keyPrefixPattern").AddScriptComments("Key prefix filter — only objects matching this prefix are replicated (null = replicate all)");
            replicationTargetTable.AddBoolField("isEnabled", false, true).AddScriptComments("Whether this replication target is currently active");
            replicationTargetTable.AddDateTimeField("lastSyncUtc").AddScriptComments("Last time a successful sync completed");
            replicationTargetTable.AddIntField("objectsInSync", false, 0).AddScriptComments("Number of objects currently in sync");
            replicationTargetTable.AddIntField("objectsPendingSync", false, 0).AddScriptComments("Number of objects pending sync");
            replicationTargetTable.AddVersionControl();
            replicationTargetTable.AddControlFields();

            Database.Table.Index replicationTargetIdActiveDeletedIndex = replicationTargetTable.CreateIndex("I_ReplicationTarget_id_active_deleted");
            replicationTargetIdActiveDeletedIndex.AddField("id");
            replicationTargetIdActiveDeletedIndex.AddField("active");
            replicationTargetIdActiveDeletedIndex.AddField("deleted");



            //
            // Tenant Quota — per-tenant storage limits and current usage
            //
            Database.Table tenantQuotaTable = database.AddTable("TenantQuota");
            tenantQuotaTable.comment = "Per-tenant storage quotas and current usage tracking. Enforced by StorageManager before accepting new objects.";
            tenantQuotaTable.SetMinimumPermissionLevels(DEEPSPACE_READER_PERMISSION_LEVEL, DEEPSPACE_ADMIN_WRITER_PERMISSION_LEVEL);
            tenantQuotaTable.AddIdField();
            tenantQuotaTable.AddMultiTenantSupport();
            tenantQuotaTable.AddIntField("maxStorageBytes", false).AddScriptComments("Maximum total storage allowed for this tenant in bytes");
            tenantQuotaTable.AddIntField("currentUsageBytes", false, 0).AddScriptComments("Current storage usage in bytes (updated on each put/delete)");
            tenantQuotaTable.AddIntField("maxObjectCount").AddScriptComments("Maximum number of objects allowed (null = unlimited)");
            tenantQuotaTable.AddIntField("currentObjectCount", false, 0).AddScriptComments("Current number of objects stored");
            tenantQuotaTable.AddIntField("alertThresholdPercent", false, 80).AddScriptComments("Usage percentage at which to trigger capacity alerts (default 80%)");
            tenantQuotaTable.AddBoolField("isEnforced", false, true).AddScriptComments("Whether quota limits are enforced (false = warn only, true = reject on exceed)");
            tenantQuotaTable.AddDateTimeField("lastRecalculatedUtc").AddScriptComments("Last time usage was fully recalculated from actual object sizes");
            tenantQuotaTable.AddVersionControl();
            tenantQuotaTable.AddControlFields();

            // Only one quota record per tenant
            tenantQuotaTable.AddUniqueConstraint(new List<string>() { "tenantGuid" }, false);

            Database.Table.Index tenantQuotaIdActiveDeletedIndex = tenantQuotaTable.CreateIndex("I_TenantQuota_id_active_deleted");
            tenantQuotaIdActiveDeletedIndex.AddField("id");
            tenantQuotaIdActiveDeletedIndex.AddField("active");
            tenantQuotaIdActiveDeletedIndex.AddField("deleted");



            //
            // Access Log — object access tracking for lifecycle rule evaluation
            //
            Database.Table accessLogTable = database.AddTable("AccessLog");
            accessLogTable.comment = "Object access tracking for lifecycle rule evaluation and audit trail. Write-heavy, append-only.";
            accessLogTable.SetMinimumPermissionLevels(DEEPSPACE_READER_PERMISSION_LEVEL, DEEPSPACE_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            accessLogTable.AddIdField();
            accessLogTable.AddForeignKeyField("storageObjectId", storageObjectTable, false).AddScriptComments("The object that was accessed");
            accessLogTable.AddForeignKeyField("accessTypeId", accessTypeTable, false).AddScriptComments("The type of access operation");
            accessLogTable.AddGuidField("accessedByUserGuid").AddScriptComments("The Security user GUID who performed the access (cross-database reference)");
            Database.Table.Field accessedUtcField = accessLogTable.AddDateTimeField("accessedUtc", false);
            accessedUtcField.AddScriptComments("When the access occurred");
            accessedUtcField.CreateIndex();
            accessLogTable.AddString50Field("ipAddress").AddScriptComments("Client IP address for the access request");
            accessLogTable.AddIntField("bytesTransferred").AddScriptComments("Number of bytes read or written during this access");
            accessLogTable.AddControlFields(false);
            accessLogTable.AddSortSequence("accessedUtc", true);

            // Composite index for per-object access history lookups
            accessLogTable.CreateIndexForFields(new List<string>() { "storageObjectId", "accessedUtc" });

            Database.Table.Index accessLogIdActiveDeletedIndex = accessLogTable.CreateIndex("I_AccessLog_id_active_deleted");
            accessLogIdActiveDeletedIndex.AddField("id");
            accessLogIdActiveDeletedIndex.AddField("active");
            accessLogIdActiveDeletedIndex.AddField("deleted");
        }
    }
}
