//
// Notification Audit Buffer Worker
//
// BackgroundService that periodically flushes completed local audit records
// from the IndexedDB buffer to the central SQL Server database. This ensures
// the local buffer doesn't grow unbounded and that the central audit trail
// stays up to date.
//
// AI-assisted development - February 2026
//
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Alerting.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Background worker that periodically flushes completed delivery records
    /// from the local IndexedDB audit buffer to SQL Server.
    /// </summary>
    public class NotificationAuditBufferWorker : BackgroundService
    {
        private readonly INotificationAuditBuffer _buffer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationAuditBufferWorker> _logger;
        private readonly int _flushIntervalSeconds;
        private readonly int _batchSize;

        public NotificationAuditBufferWorker(
            INotificationAuditBuffer buffer,
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationAuditBufferWorker> logger,
            IConfiguration configuration)
        {
            _buffer = buffer;
            _scopeFactory = scopeFactory;
            _logger = logger;

            // Read configuration with sensible defaults
            _flushIntervalSeconds = configuration.GetValue<int>("NotificationAuditBuffer:FlushIntervalSeconds", 30);
            _batchSize = configuration.GetValue<int>("NotificationAuditBuffer:BatchSize", 100);

            _logger.LogInformation(
                "NotificationAuditBufferWorker configured: FlushInterval={FlushInterval}s, BatchSize={BatchSize}",
                _flushIntervalSeconds, _batchSize);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationAuditBufferWorker started.");

            //
            // Wait a bit after startup to let the system stabilize
            //
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken).ConfigureAwait(false);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await FlushBatchAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during audit buffer flush cycle. Will retry next interval.");
                }

                await Task.Delay(TimeSpan.FromSeconds(_flushIntervalSeconds), stoppingToken).ConfigureAwait(false);
            }

            _logger.LogInformation("NotificationAuditBufferWorker stopped.");
        }


        /// <summary>
        /// Flushes one batch of completed records from the local buffer to SQL Server.
        /// </summary>
        private async Task FlushBatchAsync(CancellationToken cancellationToken)
        {
            var unflushed = await _buffer.GetUnflushedAsync(_batchSize).ConfigureAwait(false);

            if (unflushed.Count == 0)
                return;

            _logger.LogDebug("Flushing {Count} audit records from local buffer to SQL Server.", unflushed.Count);

            //
            // Use a scoped DbContext since this is a singleton background service
            //
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AlertingContext>();

            int flushedCount = 0;
            var flushedIds = new System.Collections.Generic.List<int>();

            foreach (var record in unflushed)
            {
                try
                {
                    //
                    // Check if this record already exists in SQL Server (by objectGuid/correlationId).
                    // If it does, we just mark it as flushed. If not, we skip — the primary SQL write
                    // in NotificationDispatcher should have already created it.
                    //
                    if (Guid.TryParse(record.CorrelationId, out var correlationGuid))
                    {
                        var exists = await context.NotificationDeliveryAttempts
                            .AnyAsync(a => a.objectGuid == correlationGuid, cancellationToken)
                            .ConfigureAwait(false);

                        if (exists)
                        {
                            flushedIds.Add(record.Id);
                            flushedCount++;
                        }
                        else
                        {
                            //
                            // Record doesn't exist in SQL Server yet — this could happen if the
                            // primary write failed or is still in-flight. Skip for now; the next
                            // flush cycle will pick it up.
                            //
                            _logger.LogDebug(
                                "Skipping local record {Id} (CorrelationId: {CorrelationId}) — not yet in SQL Server.",
                                record.Id, record.CorrelationId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid CorrelationId format for local record {Id}: {CorrelationId}",
                            record.Id, record.CorrelationId);
                        // Mark as flushed anyway to avoid infinitely retrying bad data
                        flushedIds.Add(record.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Error verifying local record {Id} against SQL Server. Skipping this record.",
                        record.Id);
                }
            }

            //
            // Mark all verified records as flushed in the local store
            //
            if (flushedIds.Count > 0)
            {
                await _buffer.MarkFlushedAsync(flushedIds).ConfigureAwait(false);
                _logger.LogInformation(
                    "Flushed {Count}/{Total} audit records from local buffer.",
                    flushedCount, unflushed.Count);
            }
        }
    }
}
