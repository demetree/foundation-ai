//
// Dashboard Controller
//
// REST API for the Alerting Command Center dashboard.
//
using System.Threading.Tasks;
using Alerting.Server.Models;
using Alerting.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Controllers
{
    /// <summary>
    /// API for the Alerting Command Center dashboard.
    /// Provides aggregated metrics, on-call status, and activity feed.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the complete dashboard summary for the Command Center.
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - Incident metrics (counts by status and severity)
        /// - On-call schedules with current responders
        /// - Recent activity feed
        /// - Configuration counts
        /// - MTTA/MTTR performance metrics with trends
        /// </remarks>
        /// <returns>Dashboard summary with all aggregated data.</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardSummaryDto), 200)]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
        {
            _logger.LogDebug("Dashboard summary requested");

            var summary = await _dashboardService.GetSummaryAsync().ConfigureAwait(false);

            _logger.LogDebug("Dashboard summary returned: Status={Status}, ActiveIncidents={ActiveCount}",
                summary.Status, summary.IncidentMetrics.ActiveCount);

            return Ok(summary);
        }
    }
}
