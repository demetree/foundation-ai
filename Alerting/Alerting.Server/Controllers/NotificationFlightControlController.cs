//
// Notification Flight Control Controller
//
// REST API for the real-time notification engine monitoring dashboard.
// Admin-only access - requires administrator privileges.
// AI-assisted development - February 2026
//
using System.Threading;
using System.Threading.Tasks;
using Alerting.Server.Models;
using Alerting.Server.Services;
using Foundation.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Controllers
{
    /// <summary>
    /// API for the Notification Flight Control dashboard.
    /// Provides real-time visibility into the notification engine.
    /// Restricted to administrators only.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationFlightControlController : SecureWebAPIController
    {
        private readonly INotificationFlightControlService _flightControlService;
        private readonly ILogger<NotificationFlightControlController> _logger;

        public NotificationFlightControlController(
            INotificationFlightControlService flightControlService,
            ILogger<NotificationFlightControlController> logger) : base("Alerting", "NotificationFlightControl")
        {
            _flightControlService = flightControlService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the complete flight control summary.
        /// </summary>
        /// <param name="timeRange">Time range filter: 1 (1h), 6 (6h), 24 (24h), 168 (7d). Default: 24.</param>
        /// <param name="channel">Optional channel filter (Email, SMS, Push, Teams, Voice).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Flight control summary with all metrics.</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(NotificationFlightControlDto), 200)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetSummary(
            [FromQuery] int timeRange = 24,
            [FromQuery] string channel = null,
            CancellationToken cancellationToken = default)
        {
            //
            // Admin-only access check
            //
            var securityUser = await GetSecurityUserAsync(cancellationToken);
            bool isAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

            if (!isAdmin)
            {
                _logger.LogWarning("Non-admin user {User} attempted to access notification flight control",
                    securityUser?.accountName ?? "Unknown");
                return Forbid();
            }

            //
            // Parse time range enum
            //
            var timeRangeEnum = timeRange switch
            {
                1 => FlightControlTimeRange.OneHour,
                6 => FlightControlTimeRange.SixHours,
                168 => FlightControlTimeRange.SevenDays,
                _ => FlightControlTimeRange.TwentyFourHours
            };

            _logger.LogDebug("Flight control summary requested by admin {User}: TimeRange={Range}, Channel={Channel}",
                securityUser?.accountName, timeRangeEnum, channel ?? "All");

            var summary = await _flightControlService
                .GetSummaryAsync(timeRangeEnum, channel, cancellationToken)
                .ConfigureAwait(false);

            return Ok(summary);
        }

        /// <summary>
        /// Gets full details for a delivery attempt (drill-down).
        /// </summary>
        /// <param name="id">Delivery attempt ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Delivery attempt details.</returns>
        [HttpGet("delivery/{id}")]
        [ProducesResponseType(typeof(DeliveryAttemptDetailDto), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetDeliveryAttemptDetail(
            long id,
            CancellationToken cancellationToken = default)
        {
            var securityUser = await GetSecurityUserAsync(cancellationToken);
            bool isAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

            if (!isAdmin)
            {
                return Forbid();
            }

            var detail = await _flightControlService
                .GetDeliveryAttemptDetailAsync(id, cancellationToken)
                .ConfigureAwait(false);

            if (detail == null)
            {
                return NotFound(new { error = "Delivery attempt not found" });
            }

            return Ok(detail);
        }

        /// <summary>
        /// Gets full details for a webhook attempt (drill-down).
        /// </summary>
        /// <param name="id">Webhook attempt ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Webhook attempt details.</returns>
        [HttpGet("webhook/{id}")]
        [ProducesResponseType(typeof(WebhookAttemptDetailDto), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetWebhookAttemptDetail(
            long id,
            CancellationToken cancellationToken = default)
        {
            var securityUser = await GetSecurityUserAsync(cancellationToken);
            bool isAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

            if (!isAdmin)
            {
                return Forbid();
            }

            var detail = await _flightControlService
                .GetWebhookAttemptDetailAsync(id, cancellationToken)
                .ConfigureAwait(false);

            if (detail == null)
            {
                return NotFound(new { error = "Webhook attempt not found" });
            }

            return Ok(detail);
        }
    }
}
