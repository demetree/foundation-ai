//
// SMS Notification Provider
//
// Sends SMS notifications via Twilio.
//
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Sends notifications via SMS using Twilio.
    /// </summary>
    public class SmsNotificationProvider : INotificationProvider
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmsNotificationProvider> _logger;
        private bool _initialized;

        // NotificationChannelType ID for SMS (from database seed data)
        public int ChannelTypeId => 2; // SMS

        public SmsNotificationProvider(
            IConfiguration config,
            ILogger<SmsNotificationProvider> logger)
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
                _logger.LogWarning("Twilio credentials not configured - SMS notifications disabled");
                return;
            }

            TwilioClient.Init(accountSid, authToken);
            _initialized = true;
        }

        public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            NotificationLogger.Debug($"SmsNotificationProvider.SendAsync - User: {request.UserObjectGuid}, Phone: {MaskPhoneNumber(request.UserPhoneNumber)}");

            if (string.IsNullOrWhiteSpace(request.UserPhoneNumber))
            {
                NotificationLogger.Warning($"Cannot send SMS to user {request.UserObjectGuid}: No phone number configured");
                _logger.LogWarning("Cannot send SMS notification to user {UserGuid}: No phone number configured",
                    request.UserObjectGuid);
                return NotificationResult.Failed("No phone number configured for user");
            }

            var fromNumber = _config["Twilio:FromNumber"];
            if (string.IsNullOrEmpty(fromNumber))
            {
                NotificationLogger.Warning("Twilio FromNumber not configured - cannot send SMS");
                _logger.LogWarning("Twilio FromNumber not configured - cannot send SMS");
                return NotificationResult.Failed("SMS provider not configured");
            }

            try
            {
                NotificationLogger.Debug("Ensuring Twilio client is initialized");
                EnsureInitialized();
                if (!_initialized)
                {
                    NotificationLogger.Error("Twilio not initialized - credentials missing");
                    return NotificationResult.Failed("Twilio not initialized - credentials missing");
                }

                NotificationLogger.Debug($"Building SMS message for incident {request.Incident.IncidentKey}");
                var messageBody = BuildMessageBody(request);
                NotificationLogger.Debug($"SMS body length: {messageBody.Length} characters");

                NotificationLogger.Debug($"Calling Twilio MessageResource.CreateAsync to {MaskPhoneNumber(request.UserPhoneNumber)}");
                var message = await MessageResource.CreateAsync(
                    to: new PhoneNumber(request.UserPhoneNumber),
                    from: new PhoneNumber(fromNumber),
                    body: messageBody
                );

                var success = message.Status != MessageResource.StatusEnum.Failed &&
                              message.Status != MessageResource.StatusEnum.Undelivered;

                if (success)
                {
                    NotificationLogger.Info($"SMS sent successfully to {MaskPhoneNumber(request.UserPhoneNumber)} for incident {request.Incident.IncidentKey} (SID: {message.Sid})");
                    _logger.LogInformation("SMS notification sent successfully to {Phone} for incident {IncidentKey} (SID: {Sid})",
                        MaskPhoneNumber(request.UserPhoneNumber), request.Incident.IncidentKey, message.Sid);
                    return NotificationResult.Succeeded(message.Sid, message.Status?.ToString());
                }
                else
                {
                    NotificationLogger.Error($"Twilio SMS failed to {MaskPhoneNumber(request.UserPhoneNumber)}: {message.ErrorMessage} (Status: {message.Status})");
                    _logger.LogError("Failed to send SMS notification to {Phone} for incident {IncidentKey}: {Error}",
                        MaskPhoneNumber(request.UserPhoneNumber), request.Incident.IncidentKey, message.ErrorMessage);
                    return NotificationResult.Failed(message.ErrorMessage ?? "SMS delivery failed", message.Status?.ToString());
                }
            }
            catch (Exception ex)
            {
                NotificationLogger.Exception($"Exception sending SMS to {MaskPhoneNumber(request.UserPhoneNumber)} for incident {request.Incident.IncidentKey}", ex);
                _logger.LogError(ex, "Exception sending SMS notification to {Phone} for incident {IncidentKey}",
                    MaskPhoneNumber(request.UserPhoneNumber), request.Incident.IncidentKey);
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

        private string MaskPhoneNumber(string phone)
        {
            // Mask phone number for logging (show last 4 digits only)
            if (string.IsNullOrEmpty(phone) || phone.Length < 4)
                return "****";
            return new string('*', phone.Length - 4) + phone.Substring(phone.Length - 4);
        }
    }
}
