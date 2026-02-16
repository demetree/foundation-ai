using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;


namespace Foundation.IndexedDB.Utility
{
    //
    /// <summary>
    /// 
    /// This class acts as an interceptor for database connections in Entity Framework Core.
    /// 
    /// It configures SQLite-specific PRAGMA settings to enable Write-Ahead Logging (WAL) mode,
    /// improve concurrency(readers do not block writers), and optimize for faster writes.
    /// These settings are applied automatically whenever a new connection is opened.
    ///
    /// </summary>
    public class SqliteWALModeInterceptor : DbConnectionInterceptor
    {
        /// <summary>
        /// 
        /// This method applies the PRAGMA settings to the SQLite connection.
        /// 
        /// Synchronous override for when a connection is opened.
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="eventData"></param>
        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            ApplyPragmas(connection);
        }


        /// <summary>
        /// 
        /// Asynchronous override for when a connection is opened asynchronously.
        /// 
        /// This mirrors the synchronous version but supports async workflows in EF Core.
        /// It allows the method to complete without blocking the calling thread.
        ///
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="eventData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            ApplyPragmas(connection);

            //
            // Placeholder to satisfy async signature; no actual async work here.
            //
            await Task.CompletedTask;
        }


        /// <summary>
        /// 
        /// This centralizes the logic for both sync and async overrides.
        /// 
        /// It checks the connection type and executes each PRAGMA individually for clarity and error isolation.
        /// 
        /// </summary>
        /// <param name="connection"></param>
        private void ApplyPragmas(DbConnection connection)
        {
            if (connection is SqliteConnection sqliteConnection)
            {
                using (var command = sqliteConnection.CreateCommand())
                {
                    ExecutePragma(command, "PRAGMA journal_mode = WAL;", "Turn on write-ahead logging for better concurrency.");
                    ExecutePragma(command, "PRAGMA auto_vacuum = NONE;", "Set auto-vacuum mode to NONE since we primarily add records with rare updates, and we'll vacuum manually at app start if needed.");
                    ExecutePragma(command, "PRAGMA wal_autocheckpoint = 2000;", "Set the auto-checkpoint threshold to 2000 pages (approximately 8 MB with 4 KB pages) to trigger passive checkpoints automatically.");
                    ExecutePragma(command, "PRAGMA journal_size_limit = 104857600;", "Limit the maximum WAL file size to 100 MB for safety; auto-checkpoints help enforce this.");
                    ExecutePragma(command, "PRAGMA synchronous = NORMAL;", "Set synchronous mode to NORMAL to sync after critical operations, balancing safety and performance.");
                    ExecutePragma(command, "PRAGMA temp_store = MEMORY;", "Store temporary tables and indices in memory for improved performance.");
                }
            }
        }


        /// <summary>
        /// 
        /// This executes a pragma command and handles any exceptions.
        /// 
        /// This includes error handling to log failures without crashing the connection setup.
        ///  In a production system, replace Console.WriteLine with your logging framework (e.g., Serilog or ILogger).
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="pragmaText"></param>
        /// <param name="description"></param>
        //
        private void ExecutePragma(DbCommand command, string pragmaText, string description)
        {
            try
            {
                command.CommandText = pragmaText;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Log the error to the diagnostics; do not rethrow to avoid preventing the connection from opening.
                System.Diagnostics.Debug.WriteLine($"Error applying PRAGMA '{pragmaText}' ({description}): {ex.Message}");
            }
        }
    }
}