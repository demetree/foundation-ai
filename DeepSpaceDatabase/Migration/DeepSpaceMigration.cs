// ============================================================================
//
// DeepSpaceMigration.cs — Schema creation and migration for DeepSpace SQLite.
//
// Uses the DeepSpaceDatabaseGenerator at runtime to create the schema,
// and provides check-then-alter migration blocks for in-place schema updates.
//
// Adapted from the RollerOps Migration pattern (Compactica).
//
// ============================================================================

using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using Foundation.CodeGeneration;

namespace Foundation.DeepSpace.Database
{
    /// <summary>
    /// Manages SQLite schema creation and in-place migration for the DeepSpace database.
    /// 
    /// Schema creation:
    ///   Uses DeepSpaceDatabaseGenerator.GenerateDatabaseCreationScripts(SQLite) at runtime
    ///   to create the schema from the generator (not EF's EnsureCreated), preserving
    ///   COLLATE NOCASE, indexes, and seed data.
    /// 
    /// Schema migration:
    ///   Uses check-then-alter blocks to apply incremental schema changes to existing databases.
    ///   Each migration block checks for a marker (column/table existence) before applying changes.
    /// </summary>
    public static class DeepSpaceMigration
    {
        /// <summary>
        /// Creates the DeepSpace SQLite schema from the generator output.
        /// Called when a fresh database file is created with no existing tables.
        /// </summary>
        public static void CreateSQLiteSchemaFromGenerator(string connectionString, ILogger logger)
        {
            try
            {
                DeepSpaceDatabaseGenerator generator = new DeepSpaceDatabaseGenerator();

                string schemaCreationScript = generator.GenerateDatabaseCreationScripts(DatabaseGenerator.DatabaseType.SQLite);

                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqliteCommand(schemaCreationScript, connection))
                    {
                        command.ExecuteNonQuery();
                    }


                    //
                    // Seed master/reference data
                    //
                    InsertSeedData(connection, logger);

                    connection.Close();
                }

                logger.LogInformation("DeepSpace: SQLite schema created successfully from generator.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DeepSpace: CreateSQLiteSchemaFromGenerator failed. Connection string is {ConnectionString}", connectionString);

                throw;
            }
        }



        /// <summary>
        /// Creates the DeepSpace SQLite schema from the generator output using an existing connection.
        /// </summary>
        public static void CreateSQLiteSchemaFromGenerator(DbConnection connection, ILogger logger)
        {
            try
            {
                DeepSpaceDatabaseGenerator generator = new DeepSpaceDatabaseGenerator();

                string schemaCreationScript = generator.GenerateDatabaseCreationScripts(DatabaseGenerator.DatabaseType.SQLite);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = schemaCreationScript;

                    command.ExecuteNonQuery();
                }


                //
                // Seed master/reference data
                //
                if (connection is SqliteConnection sqliteConn)
                {
                    InsertSeedData(sqliteConn, logger);
                }

                logger.LogInformation("DeepSpace: SQLite schema created successfully from generator (existing connection).");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DeepSpace: CreateSQLiteSchemaFromGenerator failed. Connection string is {ConnectionString}", connection.ConnectionString);

                throw;
            }
        }


        /// <summary>
        /// Attempts to apply any pending in-place migrations to the DeepSpace SQLite database.
        /// 
        /// Each migration block:
        ///   1. Checks for a marker (column/table existence) via PRAGMA table_info
        ///   2. If the marker is missing, applies the ALTER TABLE / CREATE TABLE statements
        ///   3. If the marker exists, skips (migration already applied)
        /// 
        /// This is version 1 of the schema — no migrations exist yet.
        /// Future schema changes should be added as dated blocks following the RollerOps pattern.
        /// </summary>
        public static void AttemptToUpdateSQLiteDatabaseSchemaToLatestVersion(string connectionString, ILogger logger)
        {
            try
            {
                //
                // ══════════════════════════════════════════════════════════════
                //  Version 1 — Initial schema. No migrations needed.
                //
                //  Future migrations should be added below as dated blocks:
                //
                //  // Schema changes YYYY-MM-DD — Description
                //  using (SqliteConnection connection = new SqliteConnection(connectionString))
                //  {
                //      connection.Open();
                //      string checkColumnSql = "SELECT COUNT(*) FROM pragma_table_info('TableName') WHERE name = 'newColumnName'";
                //      using (var command = new SqliteCommand(checkColumnSql, connection))
                //      {
                //          long columnExists = (long)command.ExecuteScalar();
                //          if (columnExists == 0)
                //          {
                //              string alterTableSql = "ALTER TABLE TableName ADD COLUMN newColumnName TYPE NULL;";
                //              using (var alterCommand = new SqliteCommand(alterTableSql, connection))
                //              {
                //                  alterCommand.ExecuteNonQuery();
                //              }
                //          }
                //      }
                //      connection.Close();
                //  }
                // ══════════════════════════════════════════════════════════════
                //

                //
                // Migration 2026-03-24 — Seed data for existing databases that were created before seed data was added
                //
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    InsertSeedData(connection, logger);
                    connection.Close();
                }


                logger.LogInformation("DeepSpace: Schema migration check completed. All migrations applied.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DeepSpace: AttemptToUpdateSQLiteDatabaseSchemaToLatestVersion failed. Connection string is {ConnectionString}", connectionString);

                throw;
            }
        }


        /// <summary>
        /// Inserts seed/reference data into the DeepSpace database.
        /// Uses INSERT OR IGNORE so this is safe to call multiple times.
        /// </summary>
        private static void InsertSeedData(SqliteConnection connection, ILogger logger)
        {
            try
            {
                string seedSql = @"
                    -- StorageProviderType: master list of supported provider backends
                    INSERT OR IGNORE INTO StorageProviderType (id, name, description, sequence, objectGuid, active, deleted)
                    VALUES
                        (1, 'Local',     'Local filesystem storage',               1, '" + Guid.NewGuid() + @"', 1, 0),
                        (2, 'S3',        'Amazon S3 compatible storage',            2, '" + Guid.NewGuid() + @"', 1, 0),
                        (3, 'AzureBlob', 'Azure Blob Storage',                      3, '" + Guid.NewGuid() + @"', 1, 0),
                        (4, 'DeepSpace', 'DeepSpace managed multi-provider store',  4, '" + Guid.NewGuid() + @"', 1, 0);

                    -- AccessType: types of access operations tracked in access logs
                    INSERT OR IGNORE INTO AccessType (id, name, description)
                    VALUES
                        (1, 'Read',   'Object content was retrieved'),
                        (2, 'Write',  'Object content was created or updated'),
                        (3, 'Delete', 'Object was deleted'),
                        (4, 'List',   'Objects were listed or enumerated'),
                        (5, 'Copy',   'Object was copied within or across providers');

                    -- StorageTier: classification tiers for storage lifecycle management
                    INSERT OR IGNORE INTO StorageTier (id, name, description, sequence, objectGuid, active, deleted)
                    VALUES
                        (1, 'Standard', 'Default storage tier for active objects',       1, '" + Guid.NewGuid() + @"', 1, 0),
                        (2, 'Archive',  'Long-term archive tier for infrequent access',  2, '" + Guid.NewGuid() + @"', 1, 0);
                ";

                using (var command = new SqliteCommand(seedSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                logger.LogInformation("DeepSpace: Seed data inserted successfully.");
            }
            catch (Exception ex)
            {
                // Non-fatal — seed data failure shouldn't prevent operation
                logger.LogWarning(ex, "DeepSpace: Seed data insertion encountered an error. Continuing anyway.");
            }
        }
    }
}
