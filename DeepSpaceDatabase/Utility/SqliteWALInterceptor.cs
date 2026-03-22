// ============================================================================
//
// SqliteWALInterceptor.cs — Enhanced WAL mode interceptor for DeepSpace SQLite.
//
// Applies tuned PRAGMA settings on every connection open to enable
// Write-Ahead Logging (WAL) mode for better concurrency, and optimize
// for the read-heavy / append-heavy workload that DeepSpace generates.
//
// Adapted from the RollerOps pattern (Compactica).
//
// ============================================================================

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Foundation.DeepSpace.DatabaseUtility
{
    /// <summary>
    /// EF Core connection interceptor that applies SQLite PRAGMA settings
    /// for optimal DeepSpace performance.
    /// 
    /// Settings applied on every connection open:
    ///   - journal_mode = WAL (readers don't block writers)
    ///   - auto_vacuum = NONE (manual vacuum at startup for control)
    ///   - wal_autocheckpoint = 2000 (~8MB with 4KB pages)
    ///   - journal_size_limit = 100MB (safety cap on WAL file)
    ///   - synchronous = NORMAL (balances safety and performance)
    ///   - temp_store = MEMORY (temp tables in memory)
    /// </summary>
    public class SqliteWALModeInterceptor : DbConnectionInterceptor
    {
        /// <summary>
        /// Synchronous connection open handler.
        /// </summary>
        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            ApplyPragmas(connection);
        }


        /// <summary>
        /// Asynchronous connection open handler.
        /// </summary>
        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            ApplyPragmas(connection);

            await Task.CompletedTask;
        }


        private void ApplyPragmas(DbConnection connection)
        {
            if (connection is SqliteConnection sqliteConnection)
            {
                using (var command = sqliteConnection.CreateCommand())
                {
                    ExecutePragma(command, "PRAGMA journal_mode = WAL;");
                    ExecutePragma(command, "PRAGMA auto_vacuum = NONE;");
                    ExecutePragma(command, "PRAGMA wal_autocheckpoint = 2000;");
                    ExecutePragma(command, "PRAGMA journal_size_limit = 104857600;");
                    ExecutePragma(command, "PRAGMA synchronous = NORMAL;");
                    ExecutePragma(command, "PRAGMA temp_store = MEMORY;");
                }
            }
        }


        private void ExecutePragma(DbCommand command, string pragmaText)
        {
            try
            {
                command.CommandText = pragmaText;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeepSpace: Error applying PRAGMA '{pragmaText}': {ex.Message}");
            }
        }
    }
}
