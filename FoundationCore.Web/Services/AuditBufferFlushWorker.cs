//
// Audit Buffer Flush Worker
//
// Background service that periodically drains audit events from the local
// SQLite buffer and writes them to the Auditor SQL Server database.
//
// Uses AuditEngine.CreateAuditEventAsync(EventDetails) in InProcess mode
// to preserve the full lookup-table resolution and PlanB fallback logic.
//
// AI-assisted development - February 2026
//
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Auditor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Foundation.Web.Services
{
    /// <summary>
    /// Periodically flushes buffered audit events from local SQLite to SQL Server.
    /// </summary>
    public class AuditBufferFlushWorker : BackgroundService
    {
        private readonly IAuditEventBuffer _buffer;
        private readonly ILogger<AuditBufferFlushWorker> _logger;

        private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(10);      // Flush audit events every 10 seconds
        private const int BatchSize = 100;

        public AuditBufferFlushWorker(
            IAuditEventBuffer buffer,
            ILogger<AuditBufferFlushWorker> logger)
        {
            _buffer = buffer;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AuditBufferFlushWorker started. Flush interval: {Interval}s, batch size: {BatchSize}",
                FlushInterval.TotalSeconds, BatchSize);

            // Short initial delay to let the app finish starting up
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await FlushBufferAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during audit buffer flush cycle.");
                }

                try
                {
                    await Task.Delay(FlushInterval, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Shutting down — do a final flush
                    break;
                }
            }

            // Final flush on shutdown to drain remaining records
            _logger.LogInformation("AuditBufferFlushWorker shutting down. Performing final flush...");
            try
            {
                await FlushBufferAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during final audit buffer flush.");
            }
        }


        private async Task FlushBufferAsync()
        {
            var records = await _buffer.DrainBatchAsync(BatchSize).ConfigureAwait(false);

            if (records.Count == 0)
                return;

            _logger.LogDebug("Flushing {Count} buffered audit events to SQL Server.", records.Count);

            int successCount = 0;
            int failCount = 0;

            foreach (var record in records)
            {
                try
                {
                    // Convert the local record back to an EventDetails
                    var eventDetails = ConvertToEventDetails(record);

                    // Use the existing CreateAuditEventAsync which handles all the
                    // lookup-table resolution, entity state, error messages, and PlanB fallback
                    await AuditEngine.CreateAuditEventAsync(eventDetails).ConfigureAwait(false);

                    successCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogWarning(ex, "Failed to flush audit event {Id} to SQL Server.", record.Id);
                }
            }

            if (failCount > 0)
            {
                _logger.LogWarning("Audit buffer flush: {Success} succeeded, {Failed} failed.", successCount, failCount);
            }
            else
            {
                _logger.LogDebug("Audit buffer flush complete: {Count} events written to SQL Server.", successCount);
            }
        }


        /// <summary>
        /// Converts a <see cref="LocalAuditEventRecord"/> back to an <see cref="AuditEngine.EventDetails"/>.
        /// </summary>
        private static AuditEngine.EventDetails ConvertToEventDetails(LocalAuditEventRecord record)
        {
            List<string> errorMessages = null;

            if (!string.IsNullOrEmpty(record.ErrorMessagesJson))
            {
                try
                {
                    errorMessages = JsonSerializer.Deserialize<List<string>>(record.ErrorMessagesJson);
                }
                catch
                {
                    // If deserialization fails, just pass null
                }
            }

            return new AuditEngine.EventDetails(
                startTime: record.StartTime,
                stopTime: record.StopTime,
                completedSuccessfully: record.CompletedSuccessfully,
                accessType: (AuditEngine.AuditAccessType)record.AccessType,
                auditType: (AuditEngine.AuditType)record.AuditType,
                user: record.User,
                session: record.Session,
                source: record.Source,
                userAgent: record.UserAgent,
                module: record.Module,
                moduleEntity: record.ModuleEntity,
                resource: record.Resource,
                hostSystem: record.HostSystem,
                primaryKey: record.PrimaryKey,
                threadId: record.ThreadId,
                message: record.Message,
                entityBeforeState: record.EntityBeforeState,
                entityAfterState: record.EntityAfterState,
                errorMessages: errorMessages
            );
        }
    }
}
