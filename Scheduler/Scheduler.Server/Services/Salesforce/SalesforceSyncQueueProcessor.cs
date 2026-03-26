//
// SalesforceSyncQueueProcessor.cs
//
// Background service that processes the SalesforceSyncQueue table.
// Polls every 10 seconds for Pending items and pushes them to Salesforce.
//
// Modeled on RebrickableSyncQueueProcessor.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Foundation.Scheduler.Database;
using Scheduler.Salesforce.Sync;


namespace Foundation.Scheduler.Services.Salesforce
{
    /// <summary>
    /// Background service that processes the SalesforceSyncQueue table.
    ///
    /// Polls every 10 seconds for Pending items and pushes them to the
    /// Salesforce API via the SalesforceSyncService push methods.
    ///
    /// Design:
    ///   - Only processes items for tenants with push-capable integration modes (RealTime, PushOnly)
    ///   - Uses exponential backoff on failures (10s, 30s, 90s, 270s, 810s)
    ///   - Marks items as Abandoned after maxAttempts failures
    ///   - Uses IServiceScopeFactory for scoped DbContext + service resolution
    /// </summary>
    public class SalesforceSyncQueueProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SalesforceSyncQueueProcessor> _logger;

        private const int PollIntervalSeconds = 10;
        private const int InitialDelaySeconds = 25;
        private static readonly int[] BackoffSeconds = { 10, 30, 90, 270, 810 };


        public SalesforceSyncQueueProcessor(
            IServiceScopeFactory scopeFactory,
            ILogger<SalesforceSyncQueueProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Salesforce sync queue processor started.");

            // Initial delay to let the app fully start
            await Task.Delay(TimeSpan.FromSeconds(InitialDelaySeconds), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingItemsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Salesforce sync queue processor loop");
                }

                await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Salesforce sync queue processor stopped.");
        }


        private async Task ProcessPendingItemsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var now = DateTime.UtcNow;

            //
            // Find pending items that are ready for processing.
            // Skip items that were attempted recently (exponential backoff).
            //
            var pendingItems = await db.SalesforceSyncQueues
                .Where(q =>
                    q.active == true
                    && q.deleted == false
                    && (q.status == "Pending" || q.status == "Failed"))
                .OrderBy(q => q.createdDate)
                .Take(20)
                .ToListAsync(ct);

            if (pendingItems.Count == 0) return;

            // Filter out items still in backoff
            var readyItems = pendingItems.Where(q =>
            {
                if (q.status == "Pending" && q.attemptCount == 0) return true;
                if (!q.lastAttemptDate.HasValue) return true;

                int backoffIndex = Math.Min(q.attemptCount, BackoffSeconds.Length - 1);
                var nextRetryAt = q.lastAttemptDate.Value.AddSeconds(BackoffSeconds[backoffIndex]);
                return now >= nextRetryAt;
            }).ToList();

            if (readyItems.Count == 0) return;

            _logger.LogInformation("Salesforce sync queue: processing {Count} item(s)", readyItems.Count);

            foreach (var item in readyItems)
            {
                if (ct.IsCancellationRequested) break;

                await ProcessSingleItemAsync(item, ct);
            }
        }


        private async Task ProcessSingleItemAsync(SalesforceSyncQueue item, CancellationToken ct)
        {
            // Create a fresh scope for each item to isolate failures
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var syncService = scope.ServiceProvider.GetRequiredService<SalesforceSyncService>();

            // Re-load the item with tracking
            var queueItem = await db.SalesforceSyncQueues.FindAsync(new object[] { item.id }, ct);
            if (queueItem == null) return;

            //
            // Check if the tenant's integration mode supports pushing
            //
            var tenantLink = await db.SalesforceTenantLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(tl =>
                    tl.tenantGuid == queueItem.tenantGuid
                    && tl.active == true
                    && tl.deleted == false, ct);

            if (tenantLink == null ||
                tenantLink.syncDirectionFlags == SyncDirection.None ||
                tenantLink.syncDirectionFlags == SyncDirection.ImportOnly)
            {
                // Tenant has no push-capable mode — mark as completed (no-op)
                queueItem.status = "Completed";
                queueItem.completedDate = DateTime.UtcNow;
                queueItem.errorMessage = "Skipped — integration mode does not support push";
                await db.SaveChangesAsync(ct);
                return;
            }

            //
            // Mark as InProgress
            //
            queueItem.status = "InProgress";
            queueItem.lastAttemptDate = DateTime.UtcNow;
            queueItem.attemptCount++;
            await db.SaveChangesAsync(ct);

            try
            {
                await DispatchToSyncServiceAsync(syncService, queueItem, ct);

                // Success
                queueItem.status = "Completed";
                queueItem.completedDate = DateTime.UtcNow;
                queueItem.errorMessage = null;
                queueItem.responseBody = null;
                await db.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Salesforce sync queue item {Id} completed: {EntityType}.{Operation}",
                    queueItem.id, queueItem.entityType, queueItem.operationType);
            }
            catch (Exception ex)
            {
                queueItem.errorMessage = ex.Message;
                queueItem.responseBody = ex.InnerException?.Message;

                if (queueItem.attemptCount >= queueItem.maxAttempts)
                {
                    queueItem.status = "Abandoned";
                    _logger.LogWarning(
                        "Salesforce sync queue item {Id} abandoned after {Attempts} attempts: {Error}",
                        queueItem.id, queueItem.attemptCount, ex.Message);
                }
                else
                {
                    queueItem.status = "Failed";
                    _logger.LogWarning(
                        "Salesforce sync queue item {Id} failed (attempt {Attempt}/{Max}): {Error}",
                        queueItem.id, queueItem.attemptCount, queueItem.maxAttempts, ex.Message);
                }

                await db.SaveChangesAsync(ct);
            }
        }


        /// <summary>
        /// Routes a queue item to the appropriate SalesforceSyncService push method
        /// based on entityType and operationType.
        /// </summary>
        private async Task DispatchToSyncServiceAsync(
            SalesforceSyncService syncService,
            SalesforceSyncQueue item,
            CancellationToken ct)
        {
            switch (item.entityType)
            {
                case "Client":
                    await DispatchClientAsync(syncService, item, ct);
                    break;

                case "Contact":
                    await DispatchContactAsync(syncService, item, ct);
                    break;

                case "ScheduledEvent":
                    await DispatchScheduledEventAsync(syncService, item, ct);
                    break;

                default:
                    throw new NotSupportedException($"Unknown entity type: {item.entityType}");
            }
        }


        private async Task DispatchClientAsync(SalesforceSyncService syncService, SalesforceSyncQueue item, CancellationToken ct)
        {
            switch (item.operationType)
            {
                case "Create":
                    await syncService.PushClientCreatedAsync(item.tenantGuid, item.entityId, SalesforceSyncService.TRIGGER_QUEUE_PROCESSOR, ct);
                    break;

                case "Update":
                    await syncService.PushClientUpdatedAsync(item.tenantGuid, item.entityId, SalesforceSyncService.TRIGGER_QUEUE_PROCESSOR, ct);
                    break;

                case "Delete":
                {
                    var payload = DeserializePayload(item.payload);
                    string salesforceId = payload.TryGetValue("salesforceId", out var sfId) ? sfId.GetString() : null;
                    await syncService.PushClientDeletedAsync(item.tenantGuid, salesforceId, SalesforceSyncService.TRIGGER_QUEUE_PROCESSOR, ct);
                    break;
                }

                default:
                    throw new NotSupportedException($"Unknown operation for Client: {item.operationType}");
            }
        }


        private async Task DispatchContactAsync(SalesforceSyncService syncService, SalesforceSyncQueue item, CancellationToken ct)
        {
            switch (item.operationType)
            {
                case "Create":
                    await syncService.PushContactCreatedAsync(item.tenantGuid, item.entityId, SalesforceSyncService.TRIGGER_QUEUE_PROCESSOR, ct);
                    break;

                case "Update":
                    await syncService.PushContactUpdatedAsync(item.tenantGuid, item.entityId, SalesforceSyncService.TRIGGER_QUEUE_PROCESSOR, ct);
                    break;

                case "Delete":
                {
                    var payload = DeserializePayload(item.payload);
                    string salesforceId = payload.TryGetValue("salesforceId", out var sfId) ? sfId.GetString() : null;
                    await syncService.PushContactDeletedAsync(item.tenantGuid, salesforceId, SalesforceSyncService.TRIGGER_QUEUE_PROCESSOR, ct);
                    break;
                }

                default:
                    throw new NotSupportedException($"Unknown operation for Contact: {item.operationType}");
            }
        }


        private async Task DispatchScheduledEventAsync(SalesforceSyncService syncService, SalesforceSyncQueue item, CancellationToken ct)
        {
            switch (item.operationType)
            {
                case "Create":
                    await syncService.PushEventCreatedAsync(item.tenantGuid, item.entityId, SalesforceSyncService.TRIGGER_QUEUE_PROCESSOR, ct);
                    break;

                case "Update":
                    await syncService.PushEventUpdatedAsync(item.tenantGuid, item.entityId, SalesforceSyncService.TRIGGER_QUEUE_PROCESSOR, ct);
                    break;

                case "Delete":
                {
                    var payload = DeserializePayload(item.payload);
                    string salesforceId = payload.TryGetValue("salesforceId", out var sfId) ? sfId.GetString() : null;
                    await syncService.PushEventDeletedAsync(item.tenantGuid, salesforceId, SalesforceSyncService.TRIGGER_QUEUE_PROCESSOR, ct);
                    break;
                }

                default:
                    throw new NotSupportedException($"Unknown operation for ScheduledEvent: {item.operationType}");
            }
        }


        private Dictionary<string, JsonElement> DeserializePayload(string payload)
        {
            if (string.IsNullOrEmpty(payload) == false)
            {
                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload);
            }

            return new Dictionary<string, JsonElement>();
        }
    }
}
