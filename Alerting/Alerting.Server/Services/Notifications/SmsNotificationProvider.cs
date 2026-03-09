//
// SMS Notification Provider
//
// Sends SMS notifications via Twilio using the Foundation TwilioSmsService.
//
using System;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Services;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Sends notifications via SMS using the Foundation TwilioSmsService.
    /// Handles incident-specific message formatting and delegates delivery
    /// to the shared Foundation service.
    /// </summary>
    public class SmsNotificationProvider : INotificationProvider
    {
        private readonly ILogger<SmsNotificationProvider> _logger;

        // NotificationChannelType ID for SMS (from database seed data)
        public int ChannelTypeId => 2; // SMS

        public SmsNotificationProvider(ILogger<SmsNotificationProvider> logger)
        {
            _logger = logger;
        }

        public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            NotificationLogger.Debug($"SmsNotificationProvider.SendAsync - User: {request.UserObjectGuid}, Phone: {TwilioSmsService.MaskPhoneNumber(request.UserPhoneNumber)}");

            if (string.IsNullOrWhiteSpace(request.UserPhoneNumber))
            {
                NotificationLogger.Warning($"Cannot send SMS to user {request.UserObjectGuid}: No phone number configured");
                _logger.LogWarning("Cannot send SMS notification to user {UserGuid}: No phone number configured",
                    request.UserObjectGuid);
                return NotificationResult.Failed("No phone number configured for user");
            }

            try
            {
                NotificationLogger.Debug($"Building SMS message for incident {request.Incident.IncidentKey}");
                var messageBody = BuildMessageBody(request);
                NotificationLogger.Debug($"SMS body length: {messageBody.Length} characters");

                NotificationLogger.Debug($"Calling TwilioSmsService.SendSmsAsync to {TwilioSmsService.MaskPhoneNumber(request.UserPhoneNumber)}");
                var (success, messageSid, error) = await TwilioSmsService.SendSmsAsync(
                    request.UserPhoneNumber,
                    messageBody
                );

                if (success)
                {
                    NotificationLogger.Info($"SMS sent successfully to {TwilioSmsService.MaskPhoneNumber(request.UserPhoneNumber)} for incident {request.Incident.IncidentKey} (SID: {messageSid})");
                    _logger.LogInformation("SMS notification sent successfully to {Phone} for incident {IncidentKey} (SID: {Sid})",
                        TwilioSmsService.MaskPhoneNumber(request.UserPhoneNumber), request.Incident.IncidentKey, messageSid);
                    return new NotificationResult
                    {
                        Success = true,
                        ExternalMessageId = messageSid,
                        ProviderResponse = "Sent",
                        RecipientAddress = request.UserPhoneNumber,
                        BodyContent = messageBody
                    };
                }
                else
                {
                    NotificationLogger.Error($"SMS failed to {TwilioSmsService.MaskPhoneNumber(request.UserPhoneNumber)}: {error}");
                    _logger.LogError("Failed to send SMS notification to {Phone} for incident {IncidentKey}: {Error}",
                        TwilioSmsService.MaskPhoneNumber(request.UserPhoneNumber), request.Incident.IncidentKey, error);
                    return new NotificationResult
                    {
                        Success = false,
                        ErrorMessage = error ?? "SMS delivery failed",
                        ProviderResponse = "Failed",
                        RecipientAddress = request.UserPhoneNumber,
                        BodyContent = messageBody
                    };
                }
            }
            catch (Exception ex)
            {
                NotificationLogger.Exception($"Exception sending SMS to {TwilioSmsService.MaskPhoneNumber(request.UserPhoneNumber)} for incident {request.Incident.IncidentKey}", ex);
                _logger.LogError(ex, "Exception sending SMS notification to {Phone} for incident {IncidentKey}",
                    TwilioSmsService.MaskPhoneNumber(request.UserPhoneNumber), request.Incident.IncidentKey);
                return NotificationResult.Failed(ex.Message);
            }
        }

        private string BuildMessageBody(NotificationRequest request)
        {
            // SMS has a 160 character limit for a single segment
            // Keep it concise but informative
            var severity = request.Incident.SeverityName.ToUpperInvariant();
            var title = request.Incident.Title;
            var key = request.Incident.IncidentKey;
            var service = request.Incident.ServiceName;

            // Truncate title if needed to fit in SMS
            var maxTitleLength = 80;
            if (title.Length > maxTitleLength)
            {
                title = title.Substring(0, maxTitleLength - 3) + "...";
            }

            return $"[{severity}] {title}\n{key}\nService: {service ?? "Unknown"}";
        }
    }
}
