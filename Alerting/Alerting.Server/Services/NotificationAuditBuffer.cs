//
// Notification Audit Buffer Service
//
// Provides a local write-ahead buffer for notification delivery audit records
// using Foundation.IndexedDB. Every delivery attempt is written to a fast local
// SQLite store via the Dexter API, providing a local forensic copy that survives
// SQL Server outages.
//
// AI-assisted development - February 2026
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Alerting.Server.Models;
using Foundation.IndexedDB;
using Foundation.IndexedDB.Dexter;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Interface for the local notification audit buffer.
    /// </summary>
    public interface INotificationAuditBuffer
    {
        /// <summary>
        /// Records a delivery attempt to the local IndexedDB store.
        /// </summary>
        Task RecordAttemptAsync(Foundation.Alerting.Database.NotificationDeliveryAttempt attempt);

        /// <summary>
        /// Gets unflushed records that are ready for batch upload to SQL Server.
        /// Only returns records with a terminal status (Sent or Failed).
        /// </summary>
        Task<List<LocalDeliveryRecord>> GetUnflushedAsync(int batchSize = 100);

        /// <summary>
        /// Marks a batch of local records as flushed to SQL Server.
        /// </summary>
        Task MarkFlushedAsync(IEnumerable<int> ids);

        /// <summary>
        /// Gets recent records from the local store for diagnostics.
        /// </summary>
        Task<List<LocalDeliveryRecord>> GetRecentAsync(int count = 50);
    }


    /// <summary>
    /// Dexie-style database definition for the notification audit local store.
    /// </summary>
    public class NotificationAuditDb : DexterDatabase
    {
        public DexterTable<LocalDeliveryRecord, int> DeliveryAttempts { get; }

        public NotificationAuditDb(IDBDatabase db) : base(db)
        {
            //
            // Schema definition:
            //   ++Id           → auto-incrementing primary key
            //   &CorrelationId → unique index for deduplication
            //   AttemptedAt    → indexed for time-range queries
            //   ChannelTypeId  → indexed for channel filtering
            //   FlushedToServer → indexed for flush worker queries
            //
            Version(1).DefineStores(new Dictionary<string, string>
            {
                ["deliveryAttempts"] = "++Id, &CorrelationId, AttemptedAt, ChannelTypeId, FlushedToServer"
            }).Wait();

            DeliveryAttempts = Table<LocalDeliveryRecord, int>("deliveryAttempts");
        }
    }


    /// <summary>
    /// Local write-ahead buffer for notification delivery audit records.
    /// Uses Foundation.IndexedDB (Dexter API) to persist records in a local SQLite store.
    /// Thread-safe: the underlying IDBFactory and IDBDatabase handle concurrency via SQLite WAL mode.
    /// </summary>
    public class NotificationAuditBuffer : INotificationAuditBuffer, IDisposable
    {
        private readonly ILogger<NotificationAuditBuffer> _logger;
        private readonly IDBFactory _factory;
        private NotificationAuditDb _db;
        private readonly object _initLock = new object();
        private bool _initialized;

        public NotificationAuditBuffer(ILogger<NotificationAuditBuffer> logger)
        {
            _logger = logger;

            //
            // Create the IDBFactory with the base path set to the application's directory.
            // This ensures the SQLite database is created alongside the server executable.
            //
            string basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "";
            _factory = new IDBFactory(basePath);

            _logger.LogInformation("NotificationAuditBuffer initialized with base path: {BasePath}", basePath);
        }


        /// <summary>
        /// Lazily initializes the database connection on first use.
        /// </summary>
        private async Task<NotificationAuditDb> GetDatabaseAsync()
        {
            if (_initialized && _db != null)
                return _db;

            try
            {
                var request = await _factory.OpenAsync("NotificationAudit", version: 1).ConfigureAwait(false);
                _db = new NotificationAuditDb(request.Result);
                _initialized = true;

                _logger.LogInformation("NotificationAuditBuffer database opened successfully.");
                return _db;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open NotificationAuditBuffer database.");
                throw;
            }
        }


        /// <inheritdoc/>
        public async Task RecordAttemptAsync(Foundation.Alerting.Database.NotificationDeliveryAttempt attempt)
        {
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);

                var record = new LocalDeliveryRecord
                {
                    CorrelationId = attempt.objectGuid.ToString(),
                    TenantGuid = attempt.tenantGuid.ToString(),
                    IncidentNotificationId = attempt.incidentNotificationId,
                    ChannelTypeId = attempt.notificationChannelTypeId,
                    AttemptNumber = attempt.attemptNumber,
                    AttemptedAt = attempt.attemptedAt,
                    Status = attempt.status ?? "Unknown",
                    ErrorMessage = attempt.errorMessage,
                    Response = attempt.response,
                    RecipientAddress = attempt.recipientAddress,
                    Subject = attempt.subject,
                    BodyContent = attempt.bodyContent,
                    FlushedToServer = false
                };

                await db.DeliveryAttempts.PutAsync(record).ConfigureAwait(false);

                _logger.LogDebug(
                    "Buffered delivery attempt {CorrelationId} (Channel: {ChannelTypeId}, Status: {Status})",
                    record.CorrelationId, record.ChannelTypeId, record.Status);
            }
            catch (Exception ex)
            {
                //
                // Never let a local buffer failure disrupt the main notification flow.
                // The SQL Server write has already succeeded at this point.
                //
                _logger.LogWarning(ex,
                    "Failed to buffer delivery attempt {ObjectGuid} to local store. SQL Server write was not affected.",
                    attempt.objectGuid);
            }
        }


        /// <inheritdoc/>
        public async Task<List<LocalDeliveryRecord>> GetUnflushedAsync(int batchSize = 100)
        {
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);

                //
                // Query all records where FlushedToServer is false.
                // We use ToListAsync and then filter in-memory because the IndexedDB
                // layer doesn't support compound queries (Status != "Pending" AND FlushedToServer == false).
                //
                var allRecords = await db.DeliveryAttempts
                    .Where<bool>(r => r.FlushedToServer)
                    .Equals(false)
                    .ToArray()
                    .ConfigureAwait(false);

                //
                // Further filter: only return terminal records (not still Pending)
                //
                var ready = new List<LocalDeliveryRecord>();
                foreach (var record in allRecords)
                {
                    if (record.Status != "Pending")
                    {
                        ready.Add(record);
                        if (ready.Count >= batchSize) break;
                    }
                }

                return ready;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to query unflushed records from local audit buffer.");
                return new List<LocalDeliveryRecord>();
            }
        }


        /// <inheritdoc/>
        public async Task MarkFlushedAsync(IEnumerable<int> ids)
        {
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);

                foreach (var id in ids)
                {
                    var record = await db.DeliveryAttempts.GetAsync(id).ConfigureAwait(false);
                    if (record != null)
                    {
                        record.FlushedToServer = true;
                        await db.DeliveryAttempts.PutAsync(record).ConfigureAwait(false);
                    }
                }

                _logger.LogDebug("Marked {Count} records as flushed in local audit buffer.",
                    System.Linq.Enumerable.Count(ids));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to mark records as flushed in local audit buffer.");
            }
        }


        /// <inheritdoc/>
        public async Task<List<LocalDeliveryRecord>> GetRecentAsync(int count = 50)
        {
            try
            {
                var db = await GetDatabaseAsync().ConfigureAwait(false);
                var allRecords = await db.DeliveryAttempts.ToListAsync().ConfigureAwait(false);

                //
                // Return the most recent records (by AttemptedAt descending)
                //
                allRecords.Sort((a, b) => b.AttemptedAt.CompareTo(a.AttemptedAt));
                if (allRecords.Count > count)
                    allRecords = allRecords.GetRange(0, count);

                return allRecords;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to query recent records from local audit buffer.");
                return new List<LocalDeliveryRecord>();
            }
        }


        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}
