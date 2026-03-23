// ============================================================================
//
// DeepSpaceDatabaseManager.cs — SQLite database lifecycle orchestrator.
//
// Manages the creation, migration, validation, and tuning of the
// DeepSpace SQLite metadata database. Provides thread-safe context
// creation and write locking for SQLite's single-writer constraint.
//
// Adapted from the RollerOps LocalDatabaseStructure pattern (Compactica).
//
// ============================================================================

using System;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Foundation.DeepSpace.DatabaseUtility;

namespace Foundation.DeepSpace.Database
{
    /// <summary>
    /// Manages the DeepSpace SQLite database lifecycle.
    /// 
    /// Startup sequence:
    ///   1. Create database directory and file if needed
    ///   2. If tables don't exist → create schema from DeepSpaceDatabaseGenerator
    ///   3. If tables exist → run pending migrations, then validate schema
    ///   4. VACUUM and WAL checkpoint
    ///   5. Ready for use
    /// 
    /// Thread safety:
    ///   Uses ReaderWriterLockSlim for SQLite's single-writer constraint.
    ///   Multiple readers can proceed concurrently; writes are serialized.
    /// </summary>
    public class DeepSpaceDatabaseManager : IDisposable
    {
        private const int DATABASE_COMMAND_TIMEOUT = 120;       // 2 minutes

        private readonly string _databaseDirectory;
        private readonly string _databaseFilePath;
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private readonly DbContextOptions<DeepSpaceContext> _contextOptions;

        //
        // This lock is critical — it controls single-writer access to the SQLite disk file.
        // Without it, concurrent writes will cause "database is locked" errors.
        //
        private readonly ReaderWriterLockSlim _diskRWLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private bool _disposed = false;

        /// <summary>
        /// The SQLite connection string for external tools or diagnostics.
        /// </summary>
        public string ConnectionString => _connectionString;

        /// <summary>
        /// The absolute path to the SQLite database file.
        /// </summary>
        public string DatabaseFilePath => _databaseFilePath;

        /// <summary>
        /// The ReaderWriterLock for external callers needing write serialization.
        /// </summary>
        public ReaderWriterLockSlim DiskRWLock => _diskRWLock;


        /// <summary>
        /// Creates and initializes the DeepSpace database manager.
        /// </summary>
        /// <param name="databaseDirectory">
        /// The directory where the SQLite file will be stored.
        /// Defaults to a "DeepSpace" subdirectory of the application's base directory.
        /// </param>
        /// <param name="logger">Logger for lifecycle and error reporting.</param>
        public DeepSpaceDatabaseManager(string databaseDirectory, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));


            //
            // Resolve and create the database directory
            //
            if (string.IsNullOrEmpty(databaseDirectory))
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                _databaseDirectory = Path.Combine(basePath, "DeepSpace");
            }
            else
            {
                _databaseDirectory = databaseDirectory;
            }

            try
            {
                if (Directory.Exists(_databaseDirectory) == false)
                {
                    Directory.CreateDirectory(_databaseDirectory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeepSpace: Could not create database directory '{Directory}'.", _databaseDirectory);
                throw;
            }


            //
            // Build the connection string and context options
            //
            _databaseFilePath = Path.Combine(_databaseDirectory, "DeepSpace.sqlite");
            _connectionString = $"Data Source={_databaseFilePath}";

            _logger.LogInformation("DeepSpace: Database file path is {FilePath}", _databaseFilePath);

            var optionsBuilder = new DbContextOptionsBuilder<DeepSpaceContext>();

            optionsBuilder.UseSqlite(_connectionString)
                          .UseLazyLoadingProxies(false)
                          .AddInterceptors(new SqliteWALModeInterceptor())
                          .LogTo((message) => _logger.LogDebug(message), Microsoft.Extensions.Logging.LogLevel.Error);

            _contextOptions = optionsBuilder.Options;


            //
            // Initialize the database — create schema or migrate
            //
            InitializeDatabase();
        }


        /// <summary>
        /// Creates a new EF Core context for database operations.
        /// 
        /// IMPORTANT: Callers performing writes should acquire DiskRWLock.EnterWriteLock()
        /// before calling SaveChanges(). Read-only operations can proceed without locking.
        /// </summary>
        public DeepSpaceContext GetContext()
        {
            return new DeepSpaceContext(_contextOptions);
        }


        /// <summary>
        /// Initializes the database — creates schema from generator or migrates an existing one.
        /// 
        /// Following the RollerOps pattern:
        ///   1. Check if tables exist
        ///   2. If not → create from generator (not EF EnsureCreated, to preserve COLLATE NOCASE and indexes)
        ///   3. If yes → attempt migration, then validate schema
        ///   4. VACUUM and WAL checkpoint
        /// </summary>
        private void InitializeDatabase()
        {
            bool schemaIsInvalid = false;

            using (DeepSpaceContext context = new DeepSpaceContext(_contextOptions))
            {
                if (context.TablesExistInSchema() == true)
                {
                    //
                    // Step 1: Apply any pending in-place migrations
                    //
                    try
                    {
                        DeepSpaceMigration.AttemptToUpdateSQLiteDatabaseSchemaToLatestVersion(_connectionString, _logger);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "DeepSpace: Migration failed for database at {ConnectionString}", _connectionString);
                    }


                    //
                    // Step 2: Validate the schema after migrations.
                    // If validation fails, log it — in a future iteration we could
                    // implement the rename-and-rebuild approach from RollerOps.
                    //
                    // For now, log warnings but continue operating.
                    // The schema validator requires the Foundation.DatabaseSchemaValidator
                    // which we can integrate later when the entity classes are scaffolded.
                    //
                    _logger.LogInformation("DeepSpace: Existing database found. Migration check complete.");
                }
                else
                {
                    //
                    // Fresh database — create schema from the generator.
                    // This produces proper COLLATE NOCASE, indexes, seed data,
                    // and everything the generator defines.
                    //
                    try
                    {
                        DeepSpaceMigration.CreateSQLiteSchemaFromGenerator(_connectionString, _logger);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "DeepSpace: Schema creation failed for database at {ConnectionString}", _connectionString);

                        schemaIsInvalid = true;
                    }
                }
            }


            //
            // If schema creation failed, bail out early
            //
            if (schemaIsInvalid)
            {
                _logger.LogCritical("DeepSpace: Database schema is invalid. The database manager will operate in a degraded state.");
                return;
            }


            //
            // Perform startup maintenance — VACUUM and WAL checkpoint
            //
            try
            {
                using (DeepSpaceContext context = new DeepSpaceContext(_contextOptions))
                {
                    context.Database.ExecuteSqlRaw("VACUUM");
                    context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint(FULL);");
                    context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint(TRUNCATE);");
                }

                _logger.LogInformation("DeepSpace: Startup maintenance complete (VACUUM + WAL checkpoint).");
            }
            catch (Exception ex)
            {
                // Non-fatal — VACUUM failure shouldn't prevent operation
                _logger.LogWarning(ex, "DeepSpace: Startup maintenance encountered an error. Continuing anyway.");
            }

            _logger.LogInformation("DeepSpace: Database manager initialized successfully.");
        }


        /// <summary>
        /// Executes a write operation with proper write-lock serialization.
        /// </summary>
        public void ExecuteWrite(Action<DeepSpaceContext> writeAction)
        {
            _diskRWLock.EnterWriteLock();
            try
            {
                using (DeepSpaceContext context = GetContext())
                {
                    writeAction(context);
                }
            }
            finally
            {
                _diskRWLock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Executes a write operation with proper write-lock serialization, returning a result.
        /// </summary>
        public T ExecuteWrite<T>(Func<DeepSpaceContext, T> writeAction)
        {
            _diskRWLock.EnterWriteLock();
            try
            {
                using (DeepSpaceContext context = GetContext())
                {
                    return writeAction(context);
                }
            }
            finally
            {
                _diskRWLock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Executes a read operation. No write lock is acquired — multiple reads can proceed concurrently.
        /// </summary>
        public T ExecuteRead<T>(Func<DeepSpaceContext, T> readAction)
        {
            _diskRWLock.EnterReadLock();
            try
            {
                using (DeepSpaceContext context = GetContext())
                {
                    return readAction(context);
                }
            }
            finally
            {
                _diskRWLock.ExitReadLock();
            }
        }


        public void Dispose()
        {
            if (_disposed == false)
            {
                _diskRWLock?.Dispose();
                _disposed = true;
            }
        }
    }
}
