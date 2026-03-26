//
// ISalesforceActivityBroadcaster.cs
//
// Interface for broadcasting real-time Salesforce sync activity updates
// to connected clients via SignalR.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration, following the IRebrickableActivityBroadcaster pattern.
//

using System;
using System.Threading.Tasks;


namespace Scheduler.Salesforce.Sync
{
    public interface ISalesforceActivityBroadcaster
    {
        Task BroadcastSyncStartedAsync(Guid tenantGuid, string entityType, string direction);


        Task BroadcastSyncProgressAsync(Guid tenantGuid, string entityType, int processed, int total);


        Task BroadcastSyncCompletedAsync(Guid tenantGuid, string entityType, SalesforceSyncResult result);


        Task BroadcastSyncErrorAsync(Guid tenantGuid, string entityType, string errorMessage);
    }
}
