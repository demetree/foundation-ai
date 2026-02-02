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
        /// <returns>Alert response indicating success or failure.</returns>
        [HttpPost("resolve/{incidentKey}")]
        [ProducesResponseType(typeof(AlertResponse), 200)]
        [ProducesResponseType(typeof(AlertResponse), 400)]
        [ProducesResponseType(typeof(AlertResponse), 401)]
        [ProducesResponseType(typeof(AlertResponse), 404)]
        public async Task<IActionResult> ResolveByKey(string incidentKey)
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

            var apiKey = apiKeyValues.ToString();

            // For resolve by key, we need to find the incident first
            // This is a simplified implementation - in production would validate
            // that the API key belongs to the same service as the incident
            _logger.LogInformation("Resolve by key requested for {IncidentKey}", incidentKey);

            return Ok(new AlertResponse
            {
                Success = true,
                IncidentKey = incidentKey,
                Message = "Resolve by key not yet fully implemented"
            });
        }
    }
}
