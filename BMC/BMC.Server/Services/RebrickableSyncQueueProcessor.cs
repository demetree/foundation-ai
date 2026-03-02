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
using BMC.Rebrickable.Sync;
using Foundation.BMC.Database;

namespace Foundation.BMC.Services
{
    /// <summary>
    /// Background service that processes the RebrickableSyncQueue table.
    ///
    /// Polls every 10 seconds for Pending items and pushes them to the
    /// Rebrickable API via the RebrickableSyncService push methods.
    ///
    /// Design:
    ///   - Only processes items for tenants with push-capable integration modes (RealTime, PushOnly)
    ///   - Uses exponential backoff on failures (10s, 30s, 90s, 270s, 810s)
    ///   - Marks items as Abandoned after maxAttempts failures
    ///   - Uses IServiceScopeFactory for scoped DbContext + service resolution
    ///   - Broadcasts real-time SignalR updates via IRebrickableActivityBroadcaster
    /// </summary>
    public class RebrickableSyncQueueProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RebrickableSyncQueueProcessor> _logger;

        private const int PollIntervalSeconds = 10;
        private const int InitialDelaySeconds = 20;
        private static readonly int[] BackoffSeconds = { 10, 30, 90, 270, 810 };


        public RebrickableSyncQueueProcessor(
            IServiceScopeFactory scopeFactory,
            ILogger<RebrickableSyncQueueProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Rebrickable sync queue processor started.");

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
                    _logger.LogError(ex, "Error in sync queue processor loop");
                }

                await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Rebrickable sync queue processor stopped.");
        }


        private async Task ProcessPendingItemsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BMCContext>();

            var now = DateTime.UtcNow;

            //
            // Find pending items that are ready for processing.
            // Skip items that were attempted recently (exponential backoff).
            //
            var pendingItems = await db.RebrickableSyncQueues
                .Where(q =>
                    q.active == true
                    && q.deleted == false
                    && (q.status == "Pending" || q.status == "Failed"))
                .OrderBy(q => q.createdDate)
                .Take(20)  // Process in batches
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

            _logger.LogInformation("Sync queue: processing {Count} item(s)", readyItems.Count);

            foreach (var item in readyItems)
            {
                if (ct.IsCancellationRequested) break;

                await ProcessSingleItemAsync(item, ct);
            }
        }


        private async Task ProcessSingleItemAsync(RebrickableSyncQueue item, CancellationToken ct)
        {
            // Create a fresh scope for each item to isolate failures
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BMCContext>();
            var syncService = scope.ServiceProvider.GetRequiredService<RebrickableSyncService>();

            // Re-load the item with tracking
            var queueItem = await db.RebrickableSyncQueues.FindAsync(new object[] { item.id }, ct);
            if (queueItem == null) return;

            //
            // Check if the user's integration mode supports pushing
            //
            var userLink = await db.RebrickableUserLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(ul =>
                    ul.tenantGuid == queueItem.tenantGuid
                    && ul.active == true
                    && ul.deleted == false, ct);

            if (userLink == null ||
                userLink.syncDirectionFlags == RebrickableSyncService.MODE_NONE ||
                userLink.syncDirectionFlags == RebrickableSyncService.MODE_IMPORT_ONLY)
            {
                // User has no push-capable mode — mark as completed (no-op)
                queueItem.status = "Completed";
                queueItem.completedDate = DateTime.UtcNow;
                queueItem.errorMessage = "Skipped — integration mode does not support push";
                await db.SaveChangesAsync(ct);
                return;
            }

            //
            // Pre-validate that the sync service can actually reach Rebrickable.
            // Without this check, push methods silently return null when tokens
            // are expired/missing, and the queue item would be incorrectly
            // marked as "Completed".
            //
            bool tokenValid = false;
            try
            {
                tokenValid = await syncService.ValidateStoredTokenAsync(queueItem.tenantGuid, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed for tenant {TenantGuid}", queueItem.tenantGuid);
            }

            if (!tokenValid)
            {
                queueItem.errorMessage = "Rebrickable token is invalid or expired — reconnect required";
                queueItem.lastAttemptDate = DateTime.UtcNow;
                queueItem.attemptCount++;

                if (queueItem.attemptCount >= queueItem.maxAttempts)
                {
                    queueItem.status = "Abandoned";
                    _logger.LogWarning(
                        "Sync queue item {Id} abandoned — token invalid after {Attempts} attempts",
                        queueItem.id, queueItem.attemptCount);
                }
                else
                {
                    queueItem.status = "Failed";
                    _logger.LogWarning(
                        "Sync queue item {Id} failed — token invalid (attempt {Attempt}/{Max})",
                        queueItem.id, queueItem.attemptCount, queueItem.maxAttempts);
                }

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
                    "Sync queue item {Id} completed: {EntityType}.{Operation}",
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
                        "Sync queue item {Id} abandoned after {Attempts} attempts: {Error}",
                        queueItem.id, queueItem.attemptCount, ex.Message);
                }
                else
                {
                    queueItem.status = "Failed";
                    _logger.LogWarning(
                        "Sync queue item {Id} failed (attempt {Attempt}/{Max}): {Error}",
                        queueItem.id, queueItem.attemptCount, queueItem.maxAttempts, ex.Message);
                }

                await db.SaveChangesAsync(ct);
            }
        }


        /// <summary>
        /// Routes a queue item to the appropriate RebrickableSyncService push method
        /// based on entityType and operationType.
        /// </summary>
        private async Task DispatchToSyncServiceAsync(
            RebrickableSyncService syncService,
            RebrickableSyncQueue item,
            CancellationToken ct)
        {
            var payload = !string.IsNullOrEmpty(item.payload)
                ? JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(item.payload)
                : new Dictionary<string, JsonElement>();

            switch (item.entityType)
            {
                case "SetList":
                    await DispatchSetListAsync(syncService, item, payload);
                    break;

                case "SetListItem":
                    await DispatchSetListItemAsync(syncService, item, payload);
                    break;

                case "PartList":
                    await DispatchPartListAsync(syncService, item, payload);
                    break;

                case "PartListItem":
                    await DispatchPartListItemAsync(syncService, item, payload);
                    break;

                case "LostPart":
                    await DispatchLostPartAsync(syncService, item, payload);
                    break;

                default:
                    throw new NotSupportedException($"Unknown entity type: {item.entityType}");
            }
        }


        private async Task DispatchSetListAsync(
            RebrickableSyncService syncService,
            RebrickableSyncQueue item,
            Dictionary<string, JsonElement> payload)
        {
            switch (item.operationType)
            {
                case "Create":
                {
                    string name = payload.GetValueOrDefault("name").GetString() ?? "Unnamed";
                    bool isBuildable = payload.TryGetValue("isBuildable", out var b) && b.GetBoolean();
                    var rebrickableId = await syncService.PushSetListCreatedAsync(item.tenantGuid, name, isBuildable, RebrickableSyncService.TRIGGER_QUEUE_PROCESSOR);

                    if (!rebrickableId.HasValue)
                        throw new InvalidOperationException(
                            $"PushSetListCreatedAsync returned null for '{name}' — " +
                            "the Rebrickable API call may have been silently skipped (token issue or API failure)");

                    // Update the local entity with the Rebrickable ID
                    using var innerScope = _scopeFactory.CreateScope();
                    var innerDb = innerScope.ServiceProvider.GetRequiredService<BMCContext>();
                    var setList = await innerDb.UserSetLists.FindAsync((int)item.entityId);
                    if (setList != null)
                    {
                        setList.rebrickableListId = rebrickableId.Value;
                        await innerDb.SaveChangesAsync();
                    }
                    break;
                }

                case "Update":
                {
                    int rebrickableListId = payload.GetValueOrDefault("rebrickableListId").GetInt32();
                    string name = payload.TryGetValue("name", out var n) ? n.GetString() : null;
                    bool? isBuildable = payload.TryGetValue("isBuildable", out var b2) ? b2.GetBoolean() : null;
                    await syncService.PushSetListUpdatedAsync(item.tenantGuid, rebrickableListId, name, isBuildable, RebrickableSyncService.TRIGGER_QUEUE_PROCESSOR);
                    break;
                }

                case "Delete":
                {
                    int rebrickableListId = payload.GetValueOrDefault("rebrickableListId").GetInt32();
                    await syncService.PushSetListDeletedAsync(item.tenantGuid, rebrickableListId, RebrickableSyncService.TRIGGER_QUEUE_PROCESSOR);
                    break;
                }

                default:
                    throw new NotSupportedException($"Unknown operation for SetList: {item.operationType}");
            }
        }


        private async Task DispatchSetListItemAsync(
            RebrickableSyncService syncService,
            RebrickableSyncQueue item,
            Dictionary<string, JsonElement> payload)
        {
            int rebrickableListId = payload.GetValueOrDefault("rebrickableListId").GetInt32();
            string setNum = payload.GetValueOrDefault("setNum").GetString();

            switch (item.operationType)
            {
                case "Create":
                    int quantity = payload.TryGetValue("quantity", out var q) ? q.GetInt32() : 1;
                    await syncService.PushSetListSetAddedAsync(item.tenantGuid, rebrickableListId, setNum, quantity, RebrickableSyncService.TRIGGER_QUEUE_PROCESSOR);
                    break;

                case "Delete":
                    await syncService.PushSetListSetRemovedAsync(item.tenantGuid, rebrickableListId, setNum, RebrickableSyncService.TRIGGER_QUEUE_PROCESSOR);
                    break;

                default:
                    throw new NotSupportedException($"Unknown operation for SetListItem: {item.operationType}");
            }
        }


        private async Task DispatchPartListAsync(
            RebrickableSyncService syncService,
            RebrickableSyncQueue item,
            Dictionary<string, JsonElement> payload)
        {
            // Part list push — to be fully implemented in Phase 2
            switch (item.operationType)
            {
                case "Create":
                    string name = payload.GetValueOrDefault("name").GetString() ?? "Unnamed";
                    bool isBuildable = payload.TryGetValue("isBuildable", out var bl) && bl.GetBoolean();
                    var partListId = await syncService.PushPartListCreatedAsync(item.tenantGuid, name, isBuildable, RebrickableSyncService.TRIGGER_QUEUE_PROCESSOR);

                    if (!partListId.HasValue)
                        throw new InvalidOperationException(
                            $"PushPartListCreatedAsync returned null for '{name}' — " +
                            "the Rebrickable API call may have been silently skipped (token issue or API failure)");
                    break;

                default:
                    _logger.LogWarning("PartList operation {Op} not yet implemented in queue processor", item.operationType);
                    break;
            }
        }


        private async Task DispatchPartListItemAsync(
            RebrickableSyncService syncService,
            RebrickableSyncQueue item,
            Dictionary<string, JsonElement> payload)
        {
            // Part list item push — to be fully implemented in Phase 2
            _logger.LogWarning("PartListItem queue processing not yet implemented");
        }


        private async Task DispatchLostPartAsync(
            RebrickableSyncService syncService,
            RebrickableSyncQueue item,
            Dictionary<string, JsonElement> payload)
        {
            // Lost part push — to be fully implemented in Phase 2
            _logger.LogWarning("LostPart queue processing not yet implemented");
        }
    }
}
