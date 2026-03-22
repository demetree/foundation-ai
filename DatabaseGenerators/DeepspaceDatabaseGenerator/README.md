# DeepspaceDatabaseGenerator

Database schema generator for the DeepSpace storage management module.

DeepSpace is a Foundation storage abstraction layer providing "infinite local disk" to any Foundation application. This generator defines the metadata database that DeepSpace uses to track storage objects, providers, tiers, lifecycle rules, replication, and tenant quotas.

## Primary Target

**SQLite** — DeepSpace is designed to be self-contained with no dependency on SQL Server or PostgreSQL. The generator produces scripts for all 4 database types (MSSQL, PostgreSQL, MySQL, SQLite) but SQLite is the primary deployment target.

## Tables

| Table | Purpose |
|-------|---------|
| StorageProviderType | Master data: Local, S3, AzureBlob, MinIO, R2 |
| StorageTier | Master data: Hot, Warm, Cool, Cold |
| StorageProvider | Registered provider instances |
| StorageObject | Object registry — every file tracked |
| StorageObjectVersion | Version history for objects |
| AccessType | Master data: Read, Write, Delete, Copy, Move |
| AccessLog | Object access tracking for lifecycle rules |
| LifecycleRule | Tenant-scoped tier migration rules |
| MigrationJobStatus | Master data: Pending, InProgress, Completed, Failed, Cancelled |
| MigrationJob | Active/completed tier migrations |
| ReplicationTarget | Cross-provider replication config |
| TenantQuota | Per-tenant storage limits and usage |
