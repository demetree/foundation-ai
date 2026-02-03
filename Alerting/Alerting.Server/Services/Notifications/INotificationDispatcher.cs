//
// Notification Dispatcher Interface
//
// Orchestrates notification delivery across multiple channels based on user preferences.
//
using System.Threading;
using System.Threading.Tasks;
using Foundation.Alerting.Database;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Orchestrates notification delivery across multiple channels.
    /// Handles preference resolution, quiet hours, and channel priority.
    /// </summary>
    public interface INotificationDispatcher
    {
        /// <summary>
        /// Dispatch notifications for an incident to a specific user.
        /// </summary>
        /// <param name="incident">The incident that triggered the notification.</param>
        /// <param name="userObjectGuid">The user to notify.</param>
        /// <param name="escalationRuleId">The escalation rule that triggered this notification.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created IncidentNotification record.</returns>
        Task<IncidentNotification> DispatchAsync(
            Incident incident,
            System.Guid userObjectGuid,
            int? escalationRuleId,
            CancellationToken cancellationToken = default);
    }
}
