//
// DbContext Health Provider
//
// Generic implementation of IDatabaseHealthProvider that works with any DbContext.
// Tests connectivity and extracts connection info without exposing credentials.
//
// AI-assisted implementation
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Services
{
    /// <summary>
    /// 
    /// Generic database health provider that works with any DbContext type.
    /// Creates a fresh context instance for each health check to get accurate connectivity status.
    /// 
    /// </summary>
    /// <typeparam name="TContext">The DbContext type to monitor</typeparam>
    public class DbContextHealthProvider<TContext> : IDatabaseHealthProvider
        where TContext : DbContext, new()
    {
        private readonly string _name;


        /// <summary>
        /// Creates a new health provider for the specified context type
        /// </summary>
        /// <param name="name">Display name for this database (e.g., "Security", "Auditor")</param>
        public DbContextHealthProvider(string name)
        {
            _name = name;
        }


        /// <summary>
        /// The display name for this database connection
        /// </summary>
        public string Name => _name;


        /// <summary>
        /// Gets the current health status by testing the database connection
        /// </summary>
        public async Task<DatabaseHealthInfo> GetHealthAsync()
        {
            var info = new DatabaseHealthInfo
            {
                Name = _name,
                Status = "Unknown",
                IsConnected = false,
                Provider = "Unknown",
                Server = "Unknown",
                Database = "Unknown"
            };

            try
            {
                //
                // Create a fresh context instance to test connectivity
                //
                await using var context = new TContext();

                //
                // Extract connection info from the database connection
                //
                var connection = context.Database.GetDbConnection();

                info.Server = connection.DataSource ?? "Unknown";
                info.Database = connection.Database ?? "Unknown";
                info.Provider = GetProviderName(context);

                //
                // Test actual connectivity
                //
                bool canConnect = await context.Database.CanConnectAsync().ConfigureAwait(false);

                if (canConnect == true)
                {
                    info.Status = "Connected";
                    info.IsConnected = true;
                }
                else
                {
                    info.Status = "Cannot Connect";
                    info.IsConnected = false;
                }
            }
            catch (Exception ex)
            {
                info.Status = "Error";
                info.IsConnected = false;
                info.ErrorMessage = ex.Message;
            }

            return info;
        }


        /// <summary>
        /// Gets detailed table statistics for this database.
        /// This is an expensive operation and should only be called on-demand.
        /// </summary>
        public async Task<TableStatisticsInfo> GetTableStatisticsAsync()
        {
            var result = new TableStatisticsInfo
            {
                DatabaseName = _name,
                Provider = "Unknown",
                SizeAvailable = false
            };

            try
            {
                await using var context = new TContext();

                var connection = context.Database.GetDbConnection();
                result.DatabaseName = connection.Database ?? _name;
                result.Provider = GetProviderName(context);

                string providerName = context.Database.ProviderName ?? "";
                string query = null;
                bool includesSize = true;

                //
                // Select the appropriate query based on the database provider
                //
                if (providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    query = @"
                        SELECT 
                            t.name AS TableName,
                            p.rows AS [RowCount],
                            SUM(a.used_pages) * 8 / 1024.0 AS UsedSizeMB
                        FROM 
                            sys.tables t
                            INNER JOIN sys.partitions p ON t.object_id = p.object_id
                            INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
                        WHERE 
                            t.is_ms_shipped = 0
                            AND p.index_id IN (0,1)
                        GROUP BY 
                            t.object_id, t.name, p.rows
                        ORDER BY 
                            t.name";
                    includesSize = true;
                }
                else if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
                {
                    query = @"
                        SELECT name AS TableName 
                        FROM sqlite_master 
                        WHERE type = 'table' AND name NOT LIKE 'sqlite_%'
                        ORDER BY name";
                    includesSize = false;
                }
                else if (providerName.Contains("MySql", StringComparison.OrdinalIgnoreCase) ||
                         providerName.Contains("Pomelo", StringComparison.OrdinalIgnoreCase))
                {
                    query = @"
                        SELECT 
                            TABLE_NAME AS TableName,
                            TABLE_ROWS AS RowCount,
                            (DATA_LENGTH + INDEX_LENGTH) / 1024 / 1024 AS UsedSizeMB
                        FROM information_schema.TABLES
                        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_TYPE = 'BASE TABLE'
                        ORDER BY TABLE_NAME";
                    includesSize = true;
                }
                else if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                         providerName.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                {
                    query = @"
                        SELECT 
                            relname AS TableName,
                            n_live_tup AS RowCount,
                            pg_total_relation_size(relid) / 1024.0 / 1024.0 AS UsedSizeMB
                        FROM pg_stat_user_tables
                        ORDER BY relname";
                    includesSize = true;
                }
                else
                {
                    result.ErrorMessage = $"Table statistics not supported for provider: {providerName}";
                    return result;
                }

                result.SizeAvailable = includesSize;

                //
                // Execute the query
                //
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }

                if (includesSize == true)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = query;

                    using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var table = new TableInfo
                        {
                            TableName = reader.GetString(0),
                            RowCount = Convert.ToInt64(reader.GetValue(1) ?? 0),
                            SizeMB = Convert.ToDecimal(reader.GetValue(2) ?? 0)
                        };

                        result.Tables.Add(table);
                        result.TotalRows += table.RowCount;
                        result.TotalSizeMB += table.SizeMB;
                    }
                }
                else
                {
                    //
                    // SQLite path - get table names first, then count each
                    //
                    var tableNames = new List<string>();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            tableNames.Add(reader.GetString(0));
                        }
                    }

                    foreach (string tableName in tableNames)
                    {
                        using var countCommand = connection.CreateCommand();
                        countCommand.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
                        var countResult = await countCommand.ExecuteScalarAsync().ConfigureAwait(false);

                        var table = new TableInfo
                        {
                            TableName = tableName,
                            RowCount = Convert.ToInt64(countResult ?? 0),
                            SizeMB = 0
                        };

                        result.Tables.Add(table);
                        result.TotalRows += table.RowCount;
                    }
                }

                result.TotalTables = result.Tables.Count;
            }
            catch (Exception ex)
            {
                //
                // Include exception type and inner exception for diagnostics
                //
                var innerMessage = ex.InnerException?.Message;
                result.ErrorMessage = innerMessage != null 
                    ? $"{ex.GetType().Name}: {ex.Message} -> {innerMessage}"
                    : $"{ex.GetType().Name}: {ex.Message}";
            }

            return result;
        }


        /// <summary>
        /// Extracts a friendly provider name from the context
        /// </summary>
        private string GetProviderName(DbContext context)
        {
            try
            {
                var providerName = context.Database.ProviderName ?? "";

                if (providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    return "SQL Server";
                }
                else if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                         providerName.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                {
                    return "PostgreSQL";
                }
                else if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
                {
                    return "SQLite";
                }
                else if (providerName.Contains("MySql", StringComparison.OrdinalIgnoreCase))
                {
                    return "MySQL";
                }
                else
                {
                    return providerName;
                }
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}

