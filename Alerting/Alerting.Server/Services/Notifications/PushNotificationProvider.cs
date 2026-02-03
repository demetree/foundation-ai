//
// Push Notification Provider
//
// Sends web push notifications via Firebase Cloud Messaging (FCM).
//
using System;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Sends push notifications via Firebase Cloud Messaging.
    /// Supports web push (browsers) and mobile push (iOS/Android).
    /// </summary>
    public class PushNotificationProvider : INotificationProvider
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PushNotificationProvider> _logger;
        private bool _initialized;
        private static readonly object _initLock = new object();

        // NotificationChannelType ID for WebPush (from database seed data)
        public int ChannelTypeId => 5; // WebPush

        public PushNotificationProvider(
            IConfiguration config,
            ILogger<PushNotificationProvider> logger)
        {
            _config = config;
            _logger = logger;
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;

            lock (_initLock)
            {
                if (_initialized) return;

                var credentialPath = _config["Firebase:CredentialPath"];
                if (string.IsNullOrEmpty(credentialPath))
                {
                    _logger.LogWarning("Firebase credential path not configured - push notifications disabled");
                    return;
                }

                try
                {
                    if (FirebaseApp.DefaultInstance == null)
                    {
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(credentialPath)
                        });
                    }
                    _initialized = true;
                    _logger.LogInformation("Firebase initialized successfully for push notifications");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Firebase");
                }
            }
        }

        public async Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.PushToken))
            {
                _logger.LogDebug("Cannot send push notification to user {UserGuid}: No push token registered",
                    request.UserObjectGuid);
                return NotificationResult.Failed("No push token registered for user");
            }

            try
            {
                EnsureInitialized();
                if (!_initialized)
                {
                    return NotificationResult.Failed("Firebase not initialized - credentials missing");
                }

                var message = BuildMessage(request);
                var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);

                _logger.LogInformation("Push notification sent successfully to user {UserGuid} for incident {IncidentKey} (MessageId: {MessageId})",
                    request.UserObjectGuid, request.Incident.IncidentKey, messageId);

                return NotificationResult.Succeeded(messageId, "Sent");
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex, "Firebase messaging error for incident {IncidentKey}: {Code}",
                    request.Incident.IncidentKey, ex.MessagingErrorCode);

                // Handle specific error codes
                if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
                {
                    // Token is no longer valid - should be removed from storage
                    return NotificationResult.Failed("Push token expired or unregistered", "TokenInvalid");
                }

                return NotificationResult.Failed(ex.Message, ex.MessagingErrorCode?.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception sending push notification for incident {IncidentKey}",
                    request.Incident.IncidentKey);
                return NotificationResult.Failed(ex.Message);
            }
        }

        private Message BuildMessage(NotificationRequest request)
        {
            var severityEmoji = GetSeverityEmoji(request.Incident.SeverityId);
            
            return new Message
            {
                Token = request.PushToken,
                Notification = new Notification
                {
                    Title = $"{severityEmoji} [{request.Incident.SeverityName}] Alert",
                    Body = TruncateForPush(request.Incident.Title, 100)
                },
                Data = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["incidentId"] = request.Incident.Id.ToString(),
                    ["incidentKey"] = request.Incident.IncidentKey,
                    ["severityId"] = request.Incident.SeverityId.ToString(),
                    ["serviceName"] = request.Incident.ServiceName ?? "",
                    ["url"] = $"/incident/{request.Incident.Id}"
                },
                Webpush = new WebpushConfig
                {
                    Notification = new WebpushNotification
                    {
                        Icon = "/assets/icons/alert-icon-192.png",
                        Badge = "/assets/icons/badge-icon-72.png",
                        RequireInteraction = request.Incident.SeverityId <= 2, // Critical/High require interaction
                        Tag = $"incident-{request.Incident.Id}", // Replace previous notification for same incident
                        Renotify = true
                    },
                    FcmOptions = new WebpushFcmOptions
                    {
                        Link = $"/incident/{request.Incident.Id}"
                    }
                }
            };
        }

        private string GetSeverityEmoji(int severityId)
        {
            return severityId switch
            {
                1 => "🔴", // Critical
                2 => "🟠", // High
                3 => "🟡", // Medium
                4 => "🟢", // Low
                _ => "⚪"
            };
        }

        private string TruncateForPush(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text ?? "";
            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}
