using System;
using System.Threading.Tasks;


namespace BMC.Rebrickable.Sync
{
    /// <summary>
    /// Abstraction for broadcasting Rebrickable sync activity to connected clients.
    /// Implemented by the server project using IHubContext to push SignalR events.
    /// </summary>
    public interface IRebrickableActivityBroadcaster
    {
        /// <summary>
        /// Broadcast a sync activity event (API call logged).
        /// </summary>
        Task BroadcastActivityAsync(Guid tenantGuid, string direction, string httpMethod,
            string endpoint, string summary, int statusCode, bool success,
            string errorMessage, int? recordCount);

        /// <summary>
        /// Broadcast a connection state change.
        /// </summary>
        Task BroadcastConnectionChangedAsync(Guid tenantGuid, bool isConnected,
            string authMode, string username);

        /// <summary>
        /// Broadcast a token warning (expired, health-check failure).
        /// </summary>
        Task BroadcastTokenWarningAsync(Guid tenantGuid, string message);

        /// <summary>
        /// Broadcast current API rate limit state.
        /// </summary>
        Task BroadcastRateLimitAsync(Guid tenantGuid, int remaining, int limit, int resetSeconds);
    }


    /// <summary>
    /// No-op implementation used when SignalR broadcasting is not available.
    /// </summary>
    public class NullActivityBroadcaster : IRebrickableActivityBroadcaster
    {
        public Task BroadcastActivityAsync(Guid tenantGuid, string direction, string httpMethod,
            string endpoint, string summary, int statusCode, bool success,
            string errorMessage, int? recordCount) => Task.CompletedTask;

        public Task BroadcastConnectionChangedAsync(Guid tenantGuid, bool isConnected,
            string authMode, string username) => Task.CompletedTask;

        public Task BroadcastTokenWarningAsync(Guid tenantGuid, string message) => Task.CompletedTask;

        public Task BroadcastRateLimitAsync(Guid tenantGuid, int remaining, int limit, int resetSeconds) => Task.CompletedTask;
    }
}
