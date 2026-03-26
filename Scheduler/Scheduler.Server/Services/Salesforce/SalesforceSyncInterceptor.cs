//
// SalesforceSyncInterceptor.cs
//
// EF Core SaveChanges interceptor that automatically detects when Client, Contact,
// or ScheduledEvent entities are created, updated, or deleted, and enqueues
// sync operations to the SalesforceSyncQueue table.
//
// This approach survives code-generation rescaffolding because it operates at the
// DbContext level rather than modifying auto-generated controller code.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Foundation.Scheduler.Database;


namespace Foundation.Scheduler.Services.Salesforce
{
    /// <summary>
    ///
    /// Intercepts SaveChanges calls and enqueues Salesforce sync operations
    /// for Client, Contact, and ScheduledEvent entity changes.
    ///
    /// Registered as a singleton and attached to the SchedulerContext via AddInterceptors.
    /// Uses ConcurrentDictionary for thread safety.
    ///
    /// </summary>
    public class SalesforceSyncInterceptor : SaveChangesInterceptor
    {
        private readonly ILogger<SalesforceSyncInterceptor> _logger;

        //
        // Entity types we care about for Salesforce sync
        //
        private static readonly HashSet<string> SyncableEntityTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(Client),
            nameof(Contact),
            nameof(ScheduledEvent)
        };

        //
        // Thread-safe dictionary to store pending items keyed by DbContext hash code.
        // Items are always removed in FlushCapturedChanges to prevent memory leaks.
        // ConcurrentDictionary is required because this interceptor is a singleton
        // shared across all DbContext instances.
        //
        private readonly ConcurrentDictionary<int, List<PendingSyncItem>> _pendingItemsPerContext = new ConcurrentDictionary<int, List<PendingSyncItem>>();


        public SalesforceSyncInterceptor(ILogger<SalesforceSyncInterceptor> logger)
        {
            _logger = logger;
        }


        /// <summary>
        ///
        /// Captures entity changes BEFORE SaveChanges persists them, because
        /// after persistence the entity states reset to Unchanged.
        ///
        /// </summary>
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            CaptureChanges(eventData.Context);
            return base.SavingChanges(eventData, result);
        }


        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            CaptureChanges(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }


        /// <summary>
        ///
        /// After SaveChanges completes successfully, flush captured changes
        /// to the SalesforceSyncQueue table.
        ///
        /// </summary>
        public override int SavedChanges(
            SaveChangesCompletedEventData eventData,
            int result)
        {
            FlushCapturedChanges(eventData.Context);
            return base.SavedChanges(eventData, result);
        }


        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            FlushCapturedChanges(eventData.Context);
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }


        /// <summary>
        ///
        /// Captures entity changes BEFORE SaveChanges persists them.
        /// Stores a reference to the actual entity object so we can re-read
        /// the auto-generated id after SaveChanges completes.
        ///
        /// </summary>
        private void CaptureChanges(DbContext context)
        {
            if (context == null) return;

            List<PendingSyncItem> pendingItems = new List<PendingSyncItem>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                string entityTypeName = entry.Entity.GetType().Name;

                if (SyncableEntityTypes.Contains(entityTypeName) == false) continue;

                string operationType = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Modified => "Update",
                    EntityState.Deleted => "Delete",
                    _ => null
                };

                if (operationType == null) continue;

                //
                // Extract tenantGuid and externalId now (these don't change after SaveChanges).
                // Store a reference to the entity object so we can read the id after SaveChanges
                // populates the auto-incremented primary key.
                //
                Guid tenantGuid = Guid.Empty;
                string externalId = null;

                var tenantProperty = entry.Entity.GetType().GetProperty("tenantGuid");
                if (tenantProperty != null)
                {
                    tenantGuid = (Guid)(tenantProperty.GetValue(entry.Entity) ?? Guid.Empty);
                }

                var externalIdProperty = entry.Entity.GetType().GetProperty("externalId");
                if (externalIdProperty != null)
                {
                    externalId = externalIdProperty.GetValue(entry.Entity) as string;
                }

                pendingItems.Add(new PendingSyncItem
                {
                    EntityType = entityTypeName,
                    OperationType = operationType,
                    EntityReference = entry.Entity,
                    TenantGuid = tenantGuid,
                    ExternalId = externalId
                });
            }

            if (pendingItems.Count > 0)
            {
                _pendingItemsPerContext[context.GetHashCode()] = pendingItems;
            }
        }


        /// <summary>
        ///
        /// After SaveChanges completes successfully, write the captured changes
        /// to the SalesforceSyncQueue table.
        ///
        /// For newly created entities, the auto-incremented id is now available
        /// on the entity reference we stored earlier.
        ///
        /// </summary>
        private void FlushCapturedChanges(DbContext context)
        {
            if (context == null) return;

            int contextHash = context.GetHashCode();

            if (_pendingItemsPerContext.TryRemove(contextHash, out List<PendingSyncItem> pendingItems) == false)
            {
                return;
            }

            if (context is SchedulerContext schedulerContext == false) return;

            try
            {
                int enqueueCount = 0;

                foreach (PendingSyncItem item in pendingItems)
                {
                    if (item.TenantGuid == Guid.Empty) continue;

                    //
                    // Read the entity id from the entity reference — after SaveChanges,
                    // the auto-incremented id is now populated even for new entities.
                    //
                    int entityId = 0;
                    var idProperty = item.EntityReference.GetType().GetProperty("id");
                    if (idProperty != null)
                    {
                        entityId = (int)(idProperty.GetValue(item.EntityReference) ?? 0);
                    }

                    if (entityId == 0) continue;

                    SalesforceSyncQueue queueItem = new SalesforceSyncQueue
                    {
                        tenantGuid = item.TenantGuid,
                        entityType = item.EntityType,
                        operationType = item.OperationType,
                        entityId = entityId,
                        status = "Pending",
                        attemptCount = 0,
                        maxAttempts = 5,
                        createdDate = DateTime.UtcNow,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    //
                    // For delete operations, include the externalId in the payload
                    // so the queue processor can delete the correct Salesforce record
                    //
                    if (item.OperationType == "Delete" && string.IsNullOrEmpty(item.ExternalId) == false)
                    {
                        queueItem.payload = System.Text.Json.JsonSerializer.Serialize(new { salesforceId = item.ExternalId });
                    }

                    schedulerContext.SalesforceSyncQueues.Add(queueItem);
                    enqueueCount++;
                }

                if (enqueueCount > 0)
                {
                    //
                    // Save the queue items. Since SalesforceSyncQueue is NOT in SyncableEntityTypes,
                    // our CaptureChanges method will skip these entities and avoid infinite recursion.
                    //
                    schedulerContext.SaveChanges();

                    _logger.LogInformation("Enqueued {Count} Salesforce sync item(s)", enqueueCount);
                }
            }
            catch (Exception ex)
            {
                //
                // Log but don't throw — Salesforce sync failures should never
                // prevent the primary write operation from succeeding.
                //
                _logger.LogError(ex, "Failed to enqueue Salesforce sync items");
            }
        }


        /// <summary>
        ///
        /// Internal DTO to hold captured entity changes between SavingChanges and SavedChanges.
        ///
        /// </summary>
        private class PendingSyncItem
        {
            public string EntityType { get; set; }
            public string OperationType { get; set; }
            public object EntityReference { get; set; }
            public Guid TenantGuid { get; set; }
            public string ExternalId { get; set; }
        }
    }
}
