//
// Incidents Controller
//
// Provides API endpoint to retrieve incidents from the Alerting system.
//
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Foundation.Security;
using Foundation.Web.Services.Alerting;

namespace Foundation.Server.Controllers
{
    [ApiController]
    [Route("api/incidents")]
    [Authorize]
    public class IncidentsController : SecureWebAPIController
    {
        private readonly IAlertingIntegrationService _alertingService;
        private readonly ILogger<IncidentsController> _logger;

        public IncidentsController(IAlertingIntegrationService alertingService,
                                   ILogger<IncidentsController> logger) : base("Foundation", "Incidents")
        {
            _alertingService = alertingService;
            _logger = logger;
        }

        /// <summary>
        /// Get incidents raised by this Foundation system.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IncidentsResponse), 200)]
        public async Task<IActionResult> GetIncidents([FromQuery] DateTime? since = null,
                                                      [FromQuery] DateTime? until = null,
                                                      [FromQuery] string status = null,
                                                      [FromQuery] string severity = null,
                                                      [FromQuery] int? limit = 50,
                                                      CancellationToken cancellationToken = default)
        {
            // Check if Alerting is configured
            if (!_alertingService.IsConfigured)
            {
                _logger.LogDebug("Alerting integration not configured, returning empty list");
                return Ok(new IncidentsResponse
                {
                    Incidents = new List<IncidentSummary>(),
                    Message = "Alerting integration is not configured. Configure Alerting:BaseUrl in appsettings.json.",
                    IsConfigured = false
                });
            }

            try
            {
                IncidentFilter filter = new IncidentFilter
                {
                    Since = since,
                    Until = until,
                    Status = status,
                    Severity = severity,
                    Limit = limit
                };

                List<IncidentSummary> incidents = await _alertingService.GetMyIncidentsAsync(filter, cancellationToken).ConfigureAwait(false);

                return Ok(new IncidentsResponse
                {
                    Incidents = incidents,
                    IsConfigured = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve incidents from Alerting");
                return Ok(new IncidentsResponse
                {
                    Incidents = new List<IncidentSummary>(),
                    Message = $"Failed to retrieve incidents: {ex.Message}",
                    IsConfigured = true
                });
            }
        }

        /// <summary>
        /// Response wrapper for incidents list.
        /// </summary>
        public class IncidentsResponse
        {
            public List<IncidentSummary> Incidents { get; set; }
            public bool IsConfigured { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        /// Test the Alerting integration by raising a low-severity test incident.
        /// </summary>
        [HttpPost("test")]
        [ProducesResponseType(typeof(TestIntegrationResponse), 200)]
        public async Task<IActionResult> TestIntegration(CancellationToken cancellationToken = default)
        {
            // Check if Alerting is configured
            if (!_alertingService.IsConfigured)
            {
                return Ok(new TestIntegrationResponse
                {
                    Success = false,
                    Message = "Alerting integration is not configured. Configure Alerting:BaseUrl in appsettings.json."
                });
            }

            try
            {
                var testRequest = new RaiseIncidentRequest
                {
                    Severity = "Info",
                    Title = "[TEST] Foundation Integration Test",
                    Description = $"This is a test incident to verify Alerting connectivity. Generated at {DateTime.UtcNow:u} from Foundation Admin.",
                    DeduplicationKey = $"foundation-test-{DateTime.UtcNow:yyyyMMdd}",
                    Source = "Foundation Admin"
                };

                IncidentResponse result = await _alertingService.RaiseIncidentAsync(testRequest, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Test incident raised successfully: {IncidentKey}", result?.IncidentKey);

                return Ok(new TestIntegrationResponse
                {
                    Success = true,
                    IncidentKey = result?.IncidentKey,
                    Message = $"Test incident raised successfully: {result?.IncidentKey}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test Alerting integration");
                return Ok(new TestIntegrationResponse
                {
                    Success = false,
                    Message = $"Failed to raise test incident: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Response wrapper for test integration result.
        /// </summary>
        public class TestIntegrationResponse
        {
            public bool Success { get; set; }
            public string IncidentKey { get; set; }
            public string Message { get; set; }
        }
    }
}
