//
// Notification Audit Controller
//
// API endpoints for the Notification Audit Console.
// AI-assisted development - February 2026
//
using System.Threading;
using System.Threading.Tasks;
using Alerting.Server.Models;
using Alerting.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Controllers
{
    /// <summary>
    /// API for notification audit and content inspection.
    /// Admin-only access for forensic auditing of sent notifications.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/notification-audit")]
    public class NotificationAuditController : ControllerBase
    {
        private readonly INotificationAuditService _auditService;
        private readonly ILogger<NotificationAuditController> _logger;

        public NotificationAuditController(
            INotificationAuditService auditService,
            ILogger<NotificationAuditController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Get audit metrics summary (KPIs).
        /// </summary>
        [HttpGet("metrics")]
        public async Task<ActionResult<NotificationAuditMetricsDto>> GetMetrics(
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting notification audit metrics");
            var metrics = await _auditService.GetMetricsAsync(cancellationToken);
            return Ok(metrics);
        }

        /// <summary>
        /// Get paginated list of delivery attempts with filters.
        /// </summary>
        [HttpGet("deliveries")]
        public async Task<ActionResult<DeliveryAttemptListResult>> GetDeliveries(
            [FromQuery] int? channelTypeId,
            [FromQuery] string status,
            [FromQuery] string dateFrom,
            [FromQuery] string dateTo,
            [FromQuery] string search,
            [FromQuery] int? incidentId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string sortBy = "attemptedAt",
            [FromQuery] bool sortDescending = true,
            CancellationToken cancellationToken = default)
        {
            var query = new DeliveryAttemptQueryParams
            {
                ChannelTypeId = channelTypeId,
                Status = status,
                SearchQuery = search,
                IncidentId = incidentId,
                PageNumber = pageNumber < 1 ? 1 : pageNumber,
                PageSize = pageSize < 1 ? 25 : (pageSize > 100 ? 100 : pageSize),
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            // Parse date filters
            if (!string.IsNullOrEmpty(dateFrom) && System.DateTime.TryParse(dateFrom, out var from))
            {
                query.DateFrom = from;
            }

            if (!string.IsNullOrEmpty(dateTo) && System.DateTime.TryParse(dateTo, out var to))
            {
                query.DateTo = to;
            }

            _logger.LogDebug("Getting delivery attempts: Page={Page}, Size={Size}, Channel={Channel}, Status={Status}",
                query.PageNumber, query.PageSize, query.ChannelTypeId, query.Status);

            var result = await _auditService.GetDeliveryListAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get full detail for a single delivery attempt including content.
        /// </summary>
        [HttpGet("deliveries/{id}")]
        public async Task<ActionResult<AuditDeliveryDetailDto>> GetDeliveryDetail(
            int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting delivery attempt detail: Id={Id}", id);
            
            var detail = await _auditService.GetDeliveryDetailAsync(id, cancellationToken);
            
            if (detail == null)
            {
                return NotFound(new { message = "Delivery attempt not found" });
            }

            return Ok(detail);
        }
    }
}
