//
// ContactsController.Sync.cs
//
// Partial class extension for ContactsController providing fire-and-forget
// Salesforce sync enrollment when Contact entities are created, updated, or deleted.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration, following the UserSetListItemsController.Sync.cs pattern.
//

using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Foundation.Scheduler.Database;
using Scheduler.Salesforce.Sync;


namespace Foundation.Scheduler.Controllers.WebAPI
{
    public partial class ContactsController
    {
        /// <summary>
        ///
        /// Enqueues a Salesforce sync operation for a Contact entity.
        /// This is called as a fire-and-forget after a write operation succeeds.
        ///
        /// </summary>
        private void EnqueueSalesforceSyncForContact(int contactId, Guid tenantGuid, string operationType, string salesforceId = null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

                    //
                    // Check if the tenant has Salesforce sync enabled
                    //
                    bool syncEnabled = await db.SalesforceTenantLinks
                        .AsNoTracking()
                        .AnyAsync(l =>
                            l.tenantGuid == tenantGuid
                            && l.active == true
                            && l.deleted == false
                            && l.syncEnabled == true
                            && (l.syncDirectionFlags == SyncDirection.RealTime || l.syncDirectionFlags == SyncDirection.PushOnly));

                    if (syncEnabled == false) return;

                    //
                    // Build the payload
                    //
                    string payload = null;
                    if (operationType == "Delete" && string.IsNullOrEmpty(salesforceId) == false)
                    {
                        payload = JsonSerializer.Serialize(new { salesforceId = salesforceId });
                    }

                    SalesforceSyncQueue queueItem = new SalesforceSyncQueue
                    {
                        tenantGuid = tenantGuid,
                        entityType = "Contact",
                        operationType = operationType,
                        entityId = contactId,
                        payload = payload,
                        status = "Pending",
                        attemptCount = 0,
                        maxAttempts = 5,
                        createdDate = DateTime.UtcNow,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    db.SalesforceSyncQueues.Add(queueItem);
                    await db.SaveChangesAsync();

                    _logger.LogInformation("Enqueued Salesforce sync for Contact {ContactId}: {Operation}", contactId, operationType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enqueuing Salesforce sync for Contact {ContactId}", contactId);
                }
            });
        }
    }
}
