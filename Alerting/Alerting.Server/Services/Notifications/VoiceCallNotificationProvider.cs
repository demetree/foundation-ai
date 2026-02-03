//
// Voice Call Notification Provider
//
// Sends voice call notifications via Twilio with text-to-speech.
//
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Collections.Generic;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Sends notifications via automated voice calls using Twilio.
    /// Uses text-to-speech to announce incident details.
    /// </summary>
    public class VoiceCallNotificationProvider : INotificationProvider
    {
        private readonly IConfiguration _config;
        private readonly ILogger<VoiceCallNotificationProvider> _logger;
        private bool _initialized;

        // NotificationChannelType ID for VoiceCall (from database seed data)
        public int ChannelTypeId => 3; // VoiceCall

        public VoiceCallNotificationProvider(
            IConfiguration config,
            ILogger<VoiceCallNotificationProvider> logger)
        {
            _config = config;
            _logger = logger;
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;

            var accountSid = _config["Twilio:AccountSid"];
            var authToken = _config["Twilio:AuthToken"];

            if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
            {
                _logger.LogWarning("Twilio credentials not configured - Voice calls disabled");
                return;
            }

            TwilioClient.Init(accountSid, authToken);
            _initialized = true;
        }

        public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.UserPhoneNumber))
            {
                _logger.LogWarning("Cannot send voice call to user {UserGuid}: No phone number configured",
                    request.UserObjectGuid);
                return NotificationResult.Failed("No phone number configured for user");
            }

            // Use VoiceFromNumber if configured, otherwise fall back to FromNumber
            var fromNumber = _config["Twilio:VoiceFromNumber"] ?? _config["Twilio:FromNumber"];
            if (string.IsNullOrEmpty(fromNumber))
            {
                _logger.LogWarning("Twilio FromNumber not configured - cannot send voice call");
                return NotificationResult.Failed("Voice provider not configured");
            }

            try
            {
                EnsureInitialized();
                if (!_initialized)
                {
                    return NotificationResult.Failed("Twilio not initialized - credentials missing");
                }

                var twiml = BuildTwiml(request);

                var call = await CallResource.CreateAsync(
                    to: new PhoneNumber(request.UserPhoneNumber),
                    from: new PhoneNumber(fromNumber),
                    twiml: new Twilio.Types.Twiml(twiml)
                );

                var success = call.Status != CallResource.StatusEnum.Failed &&
                              call.Status != CallResource.StatusEnum.Canceled &&
                              call.Status != CallResource.StatusEnum.NoAnswer &&
                              call.Status != CallResource.StatusEnum.Busy;

                if (success || call.Status == CallResource.StatusEnum.Queued || call.Status == CallResource.StatusEnum.Ringing)
                {
                    _logger.LogInformation(
                        "Voice call initiated to {Phone} for incident {IncidentKey} (SID: {Sid}, Status: {Status})",
                        MaskPhoneNumber(request.UserPhoneNumber), request.Incident.IncidentKey, call.Sid, call.Status);
                    return NotificationResult.Succeeded(call.Sid, call.Status?.ToString());
                }
                else
                {
                    _logger.LogError(
                        "Failed to initiate voice call to {Phone} for incident {IncidentKey}: Status {Status}",
                        MaskPhoneNumber(request.UserPhoneNumber), request.Incident.IncidentKey, call.Status);
                    return NotificationResult.Failed($"Call failed with status: {call.Status}", call.Status?.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception sending voice call to {Phone} for incident {IncidentKey}",
                    MaskPhoneNumber(request.UserPhoneNumber), request.Incident.IncidentKey);
                return NotificationResult.Failed(ex.Message);
            }
        }

        private string BuildTwiml(NotificationRequest request)
        {
            var severity = request.Incident.SeverityName;
            var title = request.Incident.Title;
            var service = request.Incident.ServiceName ?? "Unknown Service";
            var incidentKey = request.Incident.IncidentKey;

            // Build a clear, spoken message
            // Using Polly neural voice for natural speech
            var message = $"Alert! This is an automated incident notification. " +
                         $"Severity: {severity}. " +
                         $"Service: {service}. " +
                         $"Incident: {title}. " +
                         $"Incident key: {SpellOutKey(incidentKey)}. " +
                         $"Please acknowledge this incident in your alerting dashboard. " +
                         $"Repeating: Severity {severity}, {title}.";

            // TwiML with voice configuration
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response>
    <Say voice=""Polly.Joanna"">{EscapeXml(message)}</Say>
    <Pause length=""1""/>
    <Say voice=""Polly.Joanna"">End of message. Goodbye.</Say>
</Response>";
        }

        private string SpellOutKey(string key)
        {
            // Spell out incident key character by character for clarity
            // e.g., "INC-001" becomes "I N C dash 0 0 1"
            if (string.IsNullOrEmpty(key)) return "unknown";

            var result = new List<string>();
            foreach (var c in key.ToUpper())
            {
                if (char.IsLetter(c))
                    result.Add(c.ToString());
                else if (char.IsDigit(c))
                    result.Add(c.ToString());
                else if (c == '-')
                    result.Add("dash");
                else if (c == '_')
                    result.Add("underscore");
                else
                    result.Add(c.ToString());
            }
            return string.Join(" ", result);
        }

        private string EscapeXml(string text)
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private string MaskPhoneNumber(string phone)
        {
            // Mask phone number for logging (show last 4 digits only)
            if (string.IsNullOrEmpty(phone) || phone.Length < 4)
                return "****";
            return new string('*', phone.Length - 4) + phone.Substring(phone.Length - 4);
        }
    }
}
