using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using BMC.Rebrickable.Sync;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// SignalR-backed implementation of IRebrickableActivityBroadcaster.
    /// Pushes real-time events to tenant-scoped groups via RebrickableHub.
    /// </summary>
    public class RebrickableActivityBroadcaster : IRebrickableActivityBroadcaster
    {
        private readonly IHubContext<RebrickableHub> _hub;

        public RebrickableActivityBroadcaster(IHubContext<RebrickableHub> hub)
        {
            _hub = hub;
        }


        public async Task BroadcastActivityAsync(Guid tenantGuid, string direction, string httpMethod,
            string endpoint, string summary, int statusCode, bool success,
            string errorMessage, int? recordCount)
        {
            await _hub.Clients.Group($"rebrickable_{tenantGuid}").SendAsync("SyncActivity", new
            {
                direction,
                httpMethod,
                endpoint,
                summary,
                statusCode,
                success,
                errorMessage,
                recordCount,
                timestamp = DateTime.UtcNow
            });
        }


        public async Task BroadcastConnectionChangedAsync(Guid tenantGuid, bool isConnected,
            string authMode, string username)
        {
            await _hub.Clients.Group($"rebrickable_{tenantGuid}").SendAsync("ConnectionChanged", new
            {
                isConnected,
                authMode,
                username,
                timestamp = DateTime.UtcNow
            });
        }


        public async Task BroadcastTokenWarningAsync(Guid tenantGuid, string message)
        {
            await _hub.Clients.Group($"rebrickable_{tenantGuid}").SendAsync("TokenWarning", new
            {
                message,
                timestamp = DateTime.UtcNow
            });
        }


        public async Task BroadcastRateLimitAsync(Guid tenantGuid, int remaining, int limit, int resetSeconds)
        {
            await _hub.Clients.Group($"rebrickable_{tenantGuid}").SendAsync("RateLimitUpdate", new
            {
                remaining,
                limit,
                resetSeconds,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
