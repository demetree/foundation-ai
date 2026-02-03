//
// Alerts Controller
//
// REST API for external systems to trigger alerts via API key authentication.
//
using System;
using System.Threading.Tasks;
using Alerting.Server.Models;
using Alerting.Server.Services;
using Foundation.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Controllers
{
    /// <summary>
    /// API for external systems to trigger alerts.
    /// Uses API key authentication via the X-Api-Key header.
    /// </summary>
    [ApiController]
    [Route("api/alerts/v1")]
    [AllowAnonymous] // Uses API key auth, not OIDC
    public class AlertsController : ControllerBase
    {
        private readonly IAlertingService _alertingService;
        private readonly ILogger<AlertsController> _logger;

        private const string API_KEY_HEADER = "X-Api-Key";

        public AlertsController(IAlertingService alertingService, ILogger<AlertsController> logger)
        {
            _alertingService = alertingService;
            _logger = logger;
        }

        /// <summary>
        /// Trigger a new incident or update an existing one.
        /// </summary>
        /// <remarks>
        /// Requires a valid integration API key in the X-Api-Key header.
        /// If an open incident with the same incidentKey exists, it will be updated.
        /// </remarks>
        /// <param name="payload">The alert payload.</param>
        /// <returns>Alert response with incident details.</returns>
        [HttpPost("enqueue")]
        [ProducesResponseType(typeof(AlertResponse), 200)]
        [ProducesResponseType(typeof(AlertResponse), 400)]
        [ProducesResponseType(typeof(AlertResponse), 401)]
        public async Task<IActionResult> EnqueueAlert([FromBody] AlertPayload payload)
        {
            // Get API key from header
            if (!Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyValues))
            {
                _logger.LogWarning("Alert request missing {Header} header", API_KEY_HEADER);
                return Unauthorized(new AlertResponse
                {
                    Success = false,
                    Message = $"Missing {API_KEY_HEADER} header"
                });
            }

            var apiKey = apiKeyValues.ToString();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Unauthorized(new AlertResponse
                {
                    Success = false,
                    Message = "Empty API key"
                });
            }

            // Validate payload
            if (string.IsNullOrWhiteSpace(payload?.Title))
            {
                return BadRequest(new AlertResponse
                {
                    Success = false,
                    Message = "Title is required"
                });
            }

            var result = await _alertingService.TriggerAsync(apiKey, payload).ConfigureAwait(false);

            if (result.Success)
            {
                _logger.LogInformation("Alert enqueued: {IncidentKey} (new: {IsNew})",
                    result.IncidentKey, result.IsNew);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Alert failed: {Message}", result.Message);

                // Return 401 for auth failures, 400 for other errors
                if (result.Message.Contains("Invalid integration"))
                {
                    return Unauthorized(result);
                }

                return BadRequest(result);
            }
        }

        /// <summary>
        /// Programmatically resolve an incident by its key.
        /// </summary>
        /// <remarks>
        /// Requires a valid integration API key in the X-Api-Key header.
        /// The incident must belong to the service associated with the integration.
        /// </remarks>
        /// <param name="incidentKey">The incident key to resolve.</param>
        /// <param name="payload">Optional resolution payload with notes.</param>
        /// <returns>Alert response indicating success or failure.</returns>
        [HttpPost("resolve/{incidentKey}")]
        [ProducesResponseType(typeof(AlertResponse), 200)]
        [ProducesResponseType(typeof(AlertResponse), 400)]
        [ProducesResponseType(typeof(AlertResponse), 401)]
        [ProducesResponseType(typeof(AlertResponse), 404)]
        public async Task<IActionResult> ResolveByKey(string incidentKey, [FromBody] ResolvePayload payload = null)
        {
            // Get API key from header
            if (!Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyValues))
            {
                return Unauthorized(new AlertResponse
                {
                    Success = false,
                    Message = $"Missing {API_KEY_HEADER} header"
                });
            }

            string apiKey = apiKeyValues.ToString();

            AlertResponse result = await _alertingService.ResolveByKeyAsync(apiKey, incidentKey, payload?.Resolution).ConfigureAwait(false);

            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message.Contains("Invalid integration"))
            {
                return Unauthorized(result);
            }

            if (result.Message.Contains("not found"))
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get incidents for this integration's service.
        /// </summary>
        /// <remarks>
        /// Requires a valid integration API key in the X-Api-Key header.
        /// Returns incidents belonging to the service associated with the integration.
        /// </remarks>
        /// <returns>List of incidents.</returns>
        [HttpGet("incidents")]
        [ProducesResponseType(typeof(IncidentQueryResult), 200)]
        [ProducesResponseType(typeof(IncidentQueryResult), 401)]
        public async Task<IActionResult> GetIncidents([FromQuery] DateTime? since = null,
                                                      [FromQuery] DateTime? until = null,
                                                      [FromQuery] string status = null,
                                                      [FromQuery] string severity = null,
                                                      [FromQuery] int limit = 50)
        {
            // Get API key from header
            if (!Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyValues))
            {
                return Unauthorized(new IncidentQueryResult
                {
                    Success = false,
                    Message = $"Missing {API_KEY_HEADER} header"
                });
            }

            var apiKey = apiKeyValues.ToString();

            var result = await _alertingService.GetIncidentsByIntegrationKeyAsync(
                apiKey, since, until, status, severity, limit).ConfigureAwait(false);

            if (!result.Success && result.Message.Contains("Invalid integration"))
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get the status of a specific incident by key.
        /// </summary>
        /// <remarks>
        /// Requires a valid integration API key in the X-Api-Key header.
        /// The incident must belong to the service associated with the integration.
        /// </remarks>
        /// <param name="incidentKey">The incident key to lookup.</param>
        /// <returns>Incident status details.</returns>
        [HttpGet("incidents/{incidentKey}/status")]
        [ProducesResponseType(typeof(IncidentStatusResult), 200)]
        [ProducesResponseType(typeof(IncidentStatusResult), 401)]
        [ProducesResponseType(typeof(IncidentStatusResult), 404)]
        public async Task<IActionResult> GetIncidentStatus(string incidentKey)
        {
            // Get API key from header
            if (!Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyValues))
            {
                return Unauthorized(new IncidentStatusResult
                {
                    Success = false,
                    Message = $"Missing {API_KEY_HEADER} header"
                });
            }

            string apiKey = apiKeyValues.ToString();

            IncidentStatusResult result = await _alertingService.GetIncidentStatusByKeyAsync(apiKey, incidentKey).ConfigureAwait(false);

            if (!result.Success)
            {
                if (result.Message.Contains("Invalid integration"))
                {
                    return Unauthorized(result);
                }
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result);
                }
            }

            return Ok(result);
        }
    }

    /// <summary>
    /// Payload for resolving an incident via API.
    /// </summary>
    public class ResolvePayload
    {
        public string Resolution { get; set; }
    }
}
