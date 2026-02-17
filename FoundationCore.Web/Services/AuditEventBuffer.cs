//
// Audit Event Buffer Service
//
// Provides a durable local write-ahead buffer for audit events using
// Foundation.IndexedDB. Events are written to a local SQLite store and
// later flushed to the Auditor SQL Server database by AuditBufferFlushWorker.
//
// This replaces the volatile in-memory List<EventDetails> used by the
// MemoryQueueWithOneMinuteFlush mode, eliminating the data-loss window
// on IIS/app restarts.
//
// Located in Foundation.Web to avoid circular dependency with Foundation.IndexedDB.
//
// AI-assisted development - February 2026
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Foundation.Auditor;
using Foundation.IndexedDB;
using Foundation.IndexedDB.Dexter;
using Microsoft.Extensions.Logging;

namespace Foundation.Web.Services
{
    /// <summary>
    /// Interface for the audit event write-ahead buffer.
    /// </summary>
    public interface IAuditEventBuffer
    {
        /// <summary>
        /// Persists an audit event to the local SQLite buffer.
        /// </summary>
        Task BufferEventAsync(AuditEngine.EventDetails e);

        /// <summary>
        /// Reads and removes up to <paramref name="batchSize"/> oldest buffered records.
        /// Returns the records that were drained.
        /// </summary>
        Task<List<LocalAuditEventRecord>> DrainBatchAsync(int batchSize);

        /// <summary>
        /// Returns the number of events currently buffered locally.
        /// </summary>
        Task<int> GetPendingCountAsync();
    }


    /// <summary>
    /// Dexter database definition for the audit event local buffer.
    /// </summary>
    public class AuditBufferDb : DexterDatabase
    {
        public DexterTable<LocalAuditEventRecord, int> Events { get; }

        public AuditBufferDb(IDBDatabase db) : base(db)
        {
            //
            // Schema:
            //   ++Id              -> auto-increment primary key
            //   BufferedAtUtc     -> indexed for ordered drain
            //
            Version(1).DefineStores(new Dictionary<string, string>
            {
                ["auditEvents"] = "++Id, BufferedAtUtc"
            }).Wait();

            Events = Table<LocalAuditEventRecord, int>("auditEvents");
        }
    }


    /// <summary>
    /// Durable audit event buffer using Foundation.IndexedDB (SQLite).
    /// Thread-safe via SQLite WAL mode.
    /// </summary>
    public class AuditEventBuffer : IAuditEventBuffer, IDisposable
    {
        private readonly ILogger<AuditEventBuffer> _logger;
        private readonly IDBFactory _factory;
        private AuditBufferDb _db;
        private bool _initialized;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private const int MaxInitRetries = 3;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

        //
        // Operation-level lock to serialize all DbContext access.
        // EF Core DbContext is not thread-safe, and this buffer is a singleton,
        // so concurrent callers must be serialized.
        //
        private readonly SemaphoreSlim _opLock = new SemaphoreSlim(1, 1);

        public AuditEventBuffer(ILogger<AuditEventBuffer> logger)
        {
            _logger = logger;

            string basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "";
            _factory = new IDBFactory(basePath);

            _logger.LogInformation("AuditEventBuffer initialized with base path: {BasePath}", basePath);
        }


        private async Task<AuditBufferDb> GetDatabaseAsync()
        {
            if (_initialized && _db != null)
                return _db;

            await _initLock.WaitAsync().ConfigureAwait(false);
            try
            {
                // Double-check after acquiring lock
                if (_initialized && _db != null)
                    return _db;

                for (int attempt = 1; attempt <= MaxInitRetries; attempt++)
                {
                    try
                    {
                        var request = await _factory.OpenAsync("AuditBuffer", version: 1).ConfigureAwait(false);
                        _db = new AuditBufferDb(request.Result);
                        _initialized = true;

                        _logger.LogInformation("AuditEventBuffer database opened successfully.");
                        return _db;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "AuditEventBuffer database open attempt {Attempt}/{Max} failed.",
                            attempt, MaxInitRetries);

                        if (attempt < MaxInitRetries)
                        {
                            await Task.Delay(RetryDelay).ConfigureAwait(false);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                throw new InvalidOperationException("Failed to open AuditEventBuffer database after retries.");
            }
            finally
            {
                _initLock.Release();
            }
        }


        /// <inheritdoc/>
        public async Task BufferEventAsync(AuditEngine.EventDetails e)
        {
            await _opLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);

                var record = new LocalAuditEventRecord
                {
                    StartTime = e.startTime,
                    StopTime = e.stopTime,
                    CompletedSuccessfully = e.completedSuccessfully,
                    AccessType = (int)e.accessType,
                    AuditType = (int)e.auditType,
                    User = e.user ?? "Unknown",
                    Session = e.session ?? "Unknown",
                    Source = e.source ?? "Unknown",
                    UserAgent = e.userAgent ?? "Unknown",
                    Module = e.module ?? "Unknown",
                    ModuleEntity = e.moduleEntity ?? "Unknown",
                    Resource = e.resource ?? "Unknown",
                    HostSystem = e.hostSystem ?? "Unknown",
                    PrimaryKey = e.primaryKey,
                    ThreadId = e.threadId,
                    Message = e.message,
                    EntityBeforeState = e.entityBeforeState,
                    EntityAfterState = e.entityAfterState,
                    ErrorMessagesJson = e.errorMessages != null ? JsonSerializer.Serialize(e.errorMessages) : null,
                    BufferedAtUtc = DateTime.UtcNow
                };

                await db.Events.AddAsync(record).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to buffer audit event locally.");
            }
            finally
            {
                _opLock.Release();
            }
        }


        /// <inheritdoc/>
        public async Task<List<LocalAuditEventRecord>> DrainBatchAsync(int batchSize)
        {
            var drained = new List<LocalAuditEventRecord>();

            await _opLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);

                // Get all records, sort by BufferedAtUtc, take up to batchSize
                var all = await db.Events.ToListAsync().ConfigureAwait(false);
                var records = all.OrderBy(r => r.BufferedAtUtc).Take(batchSize).ToList();

                foreach (var record in records)
                {
                    drained.Add(record);
                    await db.Events.DeleteAsync(record.Id).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to drain audit event batch from local buffer.");
            }
            finally
            {
                _opLock.Release();
            }

            return drained;
        }


        /// <inheritdoc/>
        public async Task<int> GetPendingCountAsync()
        {
            await _opLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);
                return (int)await db.Events.CountAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get pending count from audit event buffer.");
                return -1;
            }
            finally
            {
                _opLock.Release();
            }
        }


        public void Dispose()
        {
            _opLock?.Dispose();
            _db?.Dispose();
        }
    }
}
