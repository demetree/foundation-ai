//
// Notification Flight Control Service Interface
//
// Defines the contract for aggregating notification engine metrics.
// AI-assisted development - February 2026
//
using System.Threading;
using System.Threading.Tasks;
using Alerting.Server.Models;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Service for aggregating real-time notification engine metrics.
    /// </summary>
    public interface INotificationFlightControlService
    {
        /// <summary>
        /// Gets the complete flight control summary with all metrics.
        /// </summary>
        /// <param name="timeRange">Time range filter for metrics.</param>
        /// <param name="channelFilter">Optional channel filter (null = all channels).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Complete flight control summary.</returns>
        Task<NotificationFlightControlDto> GetSummaryAsync(FlightControlTimeRange timeRange,
                                                           string channelFilter,
                                                           CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets full details for a delivery attempt (for drill-down).
        /// </summary>
        /// <param name="deliveryAttemptId">The delivery attempt ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Delivery attempt details or null if not found.</returns>
        Task<DeliveryAttemptDetailDto> GetDeliveryAttemptDetailAsync(long deliveryAttemptId,
                                                                      CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets full details for a webhook attempt (for drill-down).
        /// </summary>
        /// <param name="webhookAttemptId">The webhook attempt ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Webhook attempt details or null if not found.</returns>
        Task<WebhookAttemptDetailDto> GetWebhookAttemptDetailAsync(long webhookAttemptId,
                                                                    CancellationToken cancellationToken = default);
    }
}
