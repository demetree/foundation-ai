//
// Alerting Webhook Controller Base
//
// Base class for handling webhook callbacks from Alerting.
//
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Foundation.Web.Services.Alerting
{
    /// <summary>
    /// Base controller for receiving webhook callbacks from Alerting.
    /// Inherit from this class and override the event handler methods.
    /// </summary>
    /// <example>
    /// [Route("api/alerting-webhook")]
    /// public class MyAlertingWebhookController : AlertingWebhookControllerBase
    /// {
    ///     protected override Task OnIncidentCreatedAsync(IncidentWebhookPayload payload)
    ///     {
    ///         // Handle new incident
    ///         return Task.CompletedTask;
    ///     }
    /// }
    /// </example>
    [ApiController]
    public abstract class AlertingWebhookControllerBase : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        protected AlertingWebhookControllerBase(ILogger logger)
        {
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Receives webhook callbacks from Alerting.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync().ConfigureAwait(false);

                var payload = JsonSerializer.Deserialize<IncidentWebhookPayload>(body, _jsonOptions);

                if (payload == null)
                {
                    return BadRequest(new { error = "Invalid payload" });
                }

                _logger.LogInformation("Received Alerting webhook: {EventType} for incident {IncidentKey}",
                    payload.EventType, payload.IncidentKey);

                // Route to appropriate handler based on event type
                switch (payload.EventType?.ToLowerInvariant())
                {
                    case "incident.created":
                        await OnIncidentCreatedAsync(payload).ConfigureAwait(false);
                        break;

                    case "incident.acknowledged":
                        await OnIncidentAcknowledgedAsync(payload).ConfigureAwait(false);
                        break;

                    case "incident.resolved":
                        await OnIncidentResolvedAsync(payload).ConfigureAwait(false);
                        break;

                    case "incident.escalated":
                        await OnIncidentEscalatedAsync(payload).ConfigureAwait(false);
                        break;

                    case "incident.reopened":
                        await OnIncidentReopenedAsync(payload).ConfigureAwait(false);
                        break;

                    default:
                        await OnUnknownEventAsync(payload).ConfigureAwait(false);
                        break;
                }

                return Ok(new { received = true });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Alerting webhook payload");
                return BadRequest(new { error = "Invalid JSON payload" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Alerting webhook");
                return StatusCode(500, new { error = "Internal error processing webhook" });
            }
        }

        /// <summary>
        /// Called when a new incident is created.
        /// </summary>
        protected virtual Task OnIncidentCreatedAsync(IncidentWebhookPayload payload)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when an incident is acknowledged.
        /// </summary>
        protected virtual Task OnIncidentAcknowledgedAsync(IncidentWebhookPayload payload)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when an incident is resolved.
        /// </summary>
        protected virtual Task OnIncidentResolvedAsync(IncidentWebhookPayload payload)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when an incident is escalated to the next level.
        /// </summary>
        protected virtual Task OnIncidentEscalatedAsync(IncidentWebhookPayload payload)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when a resolved incident is reopened.
        /// </summary>
        protected virtual Task OnIncidentReopenedAsync(IncidentWebhookPayload payload)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called for unknown event types.
        /// </summary>
        protected virtual Task OnUnknownEventAsync(IncidentWebhookPayload payload)
        {
            _logger.LogWarning("Received unknown Alerting event type: {EventType}", payload.EventType);
            return Task.CompletedTask;
        }
    }
}
