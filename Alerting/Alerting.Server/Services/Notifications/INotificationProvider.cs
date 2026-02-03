//
// Notification Provider Interface
//
// Defines the contract for notification delivery channels (Email, SMS, Voice, Push).
//
using System.Threading;
using System.Threading.Tasks;
using Foundation.Alerting.Database;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Defines a notification delivery channel provider.
    /// Each implementation handles a specific channel (Email, SMS, Voice, Push).
    /// </summary>
    public interface INotificationProvider
    {
        /// <summary>
        /// The channel type this provider handles.
        /// </summary>
        int ChannelTypeId { get; }

        /// <summary>
        /// Send a notification through this channel.
        /// </summary>
        /// <param name="request">The notification request containing incident and recipient details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result indicating success or failure with details.</returns>
        Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
    }
}
