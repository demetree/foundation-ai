//
// Microsoft Teams Notification Provider
//
// Sends notifications to Microsoft Teams channels via Incoming Webhooks.
// Uses Adaptive Cards for rich formatting with action buttons.
//
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Sends notifications to Microsoft Teams via Incoming Webhooks.
    /// Posts Adaptive Cards with incident details and action buttons.
    /// </summary>
    public class TeamsNotificationProvider : INotificationProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TeamsNotificationProvider> _logger;

        // NotificationChannelType ID for Teams (needs to be added to database seed)
        public int ChannelTypeId => 6; // Teams

        public TeamsNotificationProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<TeamsNotificationProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            NotificationLogger.Debug($"TeamsNotificationProvider.SendAsync - Incident: {request.Incident.IncidentKey}, HasWebhookUrl: {!string.IsNullOrEmpty(request.TeamsWebhookUrl)}");

            // Teams webhooks are typically configured at the service/escalation policy level
            // For now, we'll check for a webhook URL in the incident's service configuration
            // This could be extended to support per-user Teams DMs via Graph API
            
            // For the initial implementation, we'll expect the webhook URL to be passed
            // via a future ServiceConfiguration or EscalationPolicy field
            // For now, return a "not configured" result to indicate the channel exists but isn't set up
            
            if (string.IsNullOrEmpty(request.TeamsWebhookUrl))
            {
                NotificationLogger.Debug($"Teams webhook URL not configured for incident {request.Incident.IncidentKey}");
                _logger.LogDebug("Teams webhook URL not configured for incident {IncidentKey}", 
                    request.Incident.IncidentKey);
                return NotificationResult.Failed("Teams webhook URL not configured");
            }

            try
            {
                NotificationLogger.Debug($"Building Adaptive Card for incident {request.Incident.IncidentKey}");
                var adaptiveCard = BuildAdaptiveCard(request);
                var payload = BuildWebhookPayload(adaptiveCard);
                NotificationLogger.Debug($"Teams payload length: {payload.Length} characters");

                NotificationLogger.Debug($"Posting to Teams webhook URL");
                using var client = _httpClientFactory.CreateClient();
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync(request.TeamsWebhookUrl, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    NotificationLogger.Info($"Teams notification sent successfully for incident {request.Incident.IncidentKey}");
                    _logger.LogInformation("Teams notification sent successfully for incident {IncidentKey}",
                        request.Incident.IncidentKey);
                    return NotificationResult.Succeeded(null, $"HTTP {(int)response.StatusCode}");
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    NotificationLogger.Error($"Teams webhook failed for incident {request.Incident.IncidentKey}: HTTP {(int)response.StatusCode} - {errorBody}");
                    _logger.LogError("Failed to send Teams notification for incident {IncidentKey}: {StatusCode} - {Error}",
                        request.Incident.IncidentKey, response.StatusCode, errorBody);
                    return NotificationResult.Failed($"HTTP {(int)response.StatusCode}: {errorBody}");
                }
            }
            catch (Exception ex)
            {
                NotificationLogger.Exception($"Exception sending Teams notification for incident {request.Incident.IncidentKey}", ex);
                _logger.LogError(ex, "Exception sending Teams notification for incident {IncidentKey}",
                    request.Incident.IncidentKey);
                return NotificationResult.Failed(ex.Message);
            }
        }

        private object BuildAdaptiveCard(NotificationRequest request)
        {
            var severityColor = GetSeverityColor(request.Incident.SeverityId);
            var createdAt = request.Incident.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC");

            return new
            {
                type = "AdaptiveCard",
                version = "1.4",
                body = new object[]
                {
                    // Header with severity color strip
                    new
                    {
                        type = "Container",
                        style = severityColor,
                        items = new object[]
                        {
                            new
                            {
                                type = "TextBlock",
                                text = $"🚨 {request.Incident.SeverityName.ToUpperInvariant()} ALERT",
                                weight = "Bolder",
                                size = "Medium",
                                color = "Light"
                            }
                        }
                    },
                    // Incident title
                    new
                    {
                        type = "TextBlock",
                        text = request.Incident.Title,
                        weight = "Bolder",
                        size = "Large",
                        wrap = true
                    },
                    // Incident details
                    new
                    {
                        type = "FactSet",
                        facts = new object[]
                        {
                            new { title = "Incident Key", value = request.Incident.IncidentKey },
                            new { title = "Service", value = request.Incident.ServiceName ?? "Unknown" },
                            new { title = "Status", value = request.Incident.StatusName },
                            new { title = "Created", value = createdAt }
                        }
                    },
                    // Description (if present)
                    string.IsNullOrEmpty(request.Incident.Description) ? null : new
                    {
                        type = "TextBlock",
                        text = request.Incident.Description.Length > 200 
                            ? request.Incident.Description.Substring(0, 200) + "..." 
                            : request.Incident.Description,
                        wrap = true,
                        isSubtle = true
                    }
                },
                actions = new object[]
                {
                    new
                    {
                        type = "Action.OpenUrl",
                        title = "🔍 View Incident",
                        url = $"https://alerting.local/incident/{request.Incident.Id}"
                    },
                    new
                    {
                        type = "Action.OpenUrl",
                        title = "✓ Acknowledge",
                        url = $"https://alerting.local/incident/{request.Incident.Id}/acknowledge"
                    }
                }
            };
        }

        private string BuildWebhookPayload(object adaptiveCard)
        {
            // Teams Incoming Webhook expects the Adaptive Card wrapped in an attachment
            var payload = new
            {
                type = "message",
                attachments = new object[]
                {
                    new
                    {
                        contentType = "application/vnd.microsoft.card.adaptive",
                        contentUrl = (string)null,
                        content = adaptiveCard
                    }
                }
            };

            return JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }

        private string GetSeverityColor(int severityId)
        {
            // Adaptive Card container styles
            return severityId switch
            {
                1 => "attention",  // Critical - Red
                2 => "warning",    // High - Orange/Yellow
                3 => "accent",     // Medium - Blue
                4 => "good",       // Low - Green
                _ => "default"
            };
        }
    }
}
