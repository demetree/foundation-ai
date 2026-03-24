using Foundation.Cache;
using Foundation.Messaging.Database;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Foundation Push Delivery Service — orchestrates external notification delivery
    /// to registered push providers (email, SMS, etc.).
    /// 
    /// Responsibilities:
    ///   - Accepts notification/message events from NotificationService and ConversationService
    ///   - Reads recipient notification profile (via MessagingProfileService)
    ///   - Dispatches to the appropriate IPushDeliveryProvider
    ///   - Enforces throttling rules (cooldown per user/conversation)
    ///   - Logs all delivery attempts to PushDeliveryLog
    /// 
    /// Throttle rules (configurable):
    ///   - Only push when user has been offline > 5 minutes
    ///   - Max 1 push per conversation per 10 minutes per provider
    /// 
    /// This is a Foundation-level service that can be used by any module.
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 3B, March 2026.
    /// 
    /// </summary>
    public class PushDeliveryService
    {
        private readonly IEnumerable<IPushDeliveryProvider> _providers;
        private readonly MessagingProfileService _profileService;
        private readonly PresenceService _presenceService;
        private readonly IMessagingUserResolver _userResolver;
        private readonly MemoryCacheManager _cache;

        //
        // Throttle defaults (in minutes)
        //
        private const int DEFAULT_OFFLINE_THRESHOLD_MINUTES = 5;
        private const int DEFAULT_CONVERSATION_COOLDOWN_MINUTES = 10;
        private const float THROTTLE_CACHE_TTL_MINUTES = 10f;


        public PushDeliveryService(
            IEnumerable<IPushDeliveryProvider> providers,
            MessagingProfileService profileService,
            PresenceService presenceService,
            IMessagingUserResolver userResolver)
        {
            _providers = providers;
            _profileService = profileService;
            _presenceService = presenceService;
            _userResolver = userResolver;
            _cache = new MemoryCacheManager();
        }


        #region Public Delivery Methods


        /// <summary>
        /// Delivers a notification to a specific user via all their enabled push channels.
        /// Called after in-app notification distribution by NotificationService.
        /// </summary>
        /// <param name="recipientUser">The recipient SecurityUser</param>
        /// <param name="notificationId">The source notification ID</param>
        /// <param name="message">The notification message text</param>
        /// <param name="senderDisplayName">Display name of the sender</param>
        /// <param name="isMention">Whether this notification is a mention-type</param>
        public async Task DeliverNotificationAsync(
            SecurityUser recipientUser,
            int notificationId,
            string message,
            string senderDisplayName,
            bool isMention = false,
            CancellationToken cancellationToken = default)
        {
            if (recipientUser == null) return;

            foreach (IPushDeliveryProvider provider in _providers)
            {
                if (provider.IsEnabled == false) continue;

                //
                // Check if user has this delivery channel enabled and configured
                //
                if (_profileService.IsDeliveryEnabled(recipientUser, provider.ProviderId, isMention) == false)
                {
                    continue;
                }

                //
                // Get the destination for this provider
                //
                string destination = _profileService.GetDestination(recipientUser, provider.ProviderId);
                if (string.IsNullOrWhiteSpace(destination)) continue;

                //
                // Validate destination format
                //
                if (provider.ValidateDestination(destination) == false) continue;

                //
                // Build the delivery request
                //
                PushDeliveryRequest request = new PushDeliveryRequest
                {
                    Destination = destination,
                    Subject = null,
                    Body = message,
                    SenderDisplayName = senderDisplayName,
                    TenantGuid = recipientUser.securityTenant?.objectGuid ?? Guid.Empty,
                    RecipientUserId = recipientUser.id,
                    Metadata = new Dictionary<string, string>
                    {
                        ["sourceType"] = "notification",
                        ["notificationId"] = notificationId.ToString()
                    }
                };

                //
                // Attempt delivery and log
                //
                PushDeliveryResult result = await provider.DeliverAsync(request, cancellationToken);

                await LogDeliveryAttemptAsync(
                    recipientUser.securityTenant?.objectGuid ?? Guid.Empty,
                    recipientUser.id,
                    provider.ProviderId,
                    destination,
                    "notification",
                    notificationId,
                    null,
                    result);
            }
        }


        /// <summary>
        /// Delivers a message alert to a specific user via all their enabled push channels.
        /// Called when a user is offline and receives a new message.
        /// Enforces throttling: max 1 push per conversation per cooldown window.
        /// </summary>
        public async Task DeliverMessageAlertAsync(
            SecurityUser recipientUser,
            int conversationId,
            int messageId,
            string conversationName,
            string senderDisplayName,
            string messagePreview,
            bool isMention = false,
            CancellationToken cancellationToken = default)
        {
            if (recipientUser == null) return;

            Guid tenantGuid = recipientUser.securityTenant?.objectGuid ?? Guid.Empty;

            //
            // Check if user is offline long enough to warrant a push
            //
            if (await IsUserOfflineLongEnoughAsync(recipientUser, tenantGuid) == false)
            {
                return;
            }

            foreach (IPushDeliveryProvider provider in _providers)
            {
                if (provider.IsEnabled == false) continue;

                //
                // Check per-conversation notification preference
                //
                string convPref = _profileService.GetConversationNotificationPreference(recipientUser, conversationId);

                if (convPref == "none") continue;
                if (convPref == "mentions" && isMention == false) continue;

                //
                // Check global delivery preference
                //
                if (_profileService.IsDeliveryEnabled(recipientUser, provider.ProviderId, isMention) == false)
                {
                    continue;
                }

                string destination = _profileService.GetDestination(recipientUser, provider.ProviderId);
                if (string.IsNullOrWhiteSpace(destination)) continue;
                if (provider.ValidateDestination(destination) == false) continue;

                //
                // Throttle check: only 1 push per conversation per cooldown window
                //
                if (IsThrottled(recipientUser.id, conversationId, provider.ProviderId))
                {
                    continue;
                }

                //
                // Build the delivery request
                //
                string body = string.IsNullOrWhiteSpace(messagePreview)
                    ? $"New message from {senderDisplayName} in {conversationName}"
                    : $"{senderDisplayName}: {messagePreview}";

                PushDeliveryRequest request = new PushDeliveryRequest
                {
                    Destination = destination,
                    Subject = $"New message in {conversationName}",
                    Body = body,
                    HtmlBody = BuildHtmlMessageAlert(senderDisplayName, conversationName, messagePreview),
                    SenderDisplayName = senderDisplayName,
                    ConversationName = conversationName,
                    ConversationId = conversationId,
                    MessageId = messageId,
                    TenantGuid = tenantGuid,
                    RecipientUserId = recipientUser.id
                };

                //
                // Attempt delivery
                //
                PushDeliveryResult result = await provider.DeliverAsync(request, cancellationToken);

                //
                // Log the attempt
                //
                await LogDeliveryAttemptAsync(
                    tenantGuid,
                    recipientUser.id,
                    provider.ProviderId,
                    destination,
                    "message",
                    null,
                    messageId,
                    result);

                //
                // Set throttle if delivery succeeded
                //
                if (result.Success)
                {
                    SetThrottle(recipientUser.id, conversationId, provider.ProviderId);
                }
            }
        }

        #endregion


        #region Throttling


        /// <summary>
        /// Checks if a push for this user/conversation/provider has been sent recently.
        /// </summary>
        private bool IsThrottled(int userId, int conversationId, string providerId)
        {
            string key = $"push_throttle:{userId}:{conversationId}:{providerId}";
            return _cache.Get<bool?>(key) == true;
        }


        /// <summary>
        /// Records that a push was sent, preventing further pushes for the cooldown window.
        /// </summary>
        private void SetThrottle(int userId, int conversationId, string providerId)
        {
            string key = $"push_throttle:{userId}:{conversationId}:{providerId}";
            _cache.Set(key, true, THROTTLE_CACHE_TTL_MINUTES);
        }


        /// <summary>
        /// Checks whether the user has been offline long enough to warrant an external push.
        /// </summary>
        private async Task<bool> IsUserOfflineLongEnoughAsync(SecurityUser securityUser, Guid tenantGuid)
        {
            try
            {
                MessagingUser msgUser = await _userResolver.GetUserAsync(securityUser);
                if (msgUser == null) return true;

                PresenceService.PresenceSummary presence = await _presenceService.GetUserPresenceAsync(securityUser, msgUser.id);

                if (presence == null) return true;

                //
                // If the user is currently online, don't push
                //
                if (presence.status != PresenceService.STATUS_OFFLINE)
                {
                    return false;
                }

                //
                // If they went offline recently (within threshold), don't push yet
                //
                TimeSpan offlineDuration = DateTime.UtcNow - presence.lastSeenDateTime;

                return offlineDuration.TotalMinutes >= DEFAULT_OFFLINE_THRESHOLD_MINUTES;
            }
            catch
            {
                //
                // If we can't determine presence, default to allowing push
                //
                return true;
            }
        }

        #endregion


        #region Logging


        /// <summary>
        /// Logs a push delivery attempt to the PushDeliveryLog table.
        /// </summary>
        private async Task LogDeliveryAttemptAsync(
            Guid tenantGuid,
            int userId,
            string providerId,
            string destination,
            string sourceType,
            int? sourceNotificationId,
            int? sourceConversationMessageId,
            PushDeliveryResult result)
        {
            try
            {
                using (MessagingContext db = new MessagingContext())
                {
                    PushDeliveryLog log = new PushDeliveryLog
                    {
                        tenantGuid = tenantGuid,
                        userId = userId,
                        providerId = providerId,
                        destination = MaskDestination(destination, providerId),
                        sourceType = sourceType,
                        sourceNotificationId = sourceNotificationId,
                        sourceConversationMessageId = sourceConversationMessageId,
                        success = result.Success,
                        externalId = result.ExternalId,
                        errorMessage = result.ErrorMessage,
                        attemptNumber = 1,
                        dateTimeCreated = DateTime.UtcNow,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    db.PushDeliveryLogs.Add(log);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                //
                // Logging failures are non-fatal — don't break the delivery flow
                //
                System.Diagnostics.Debug.WriteLine($"Failed to log push delivery: {ex.Message}");
            }
        }

        #endregion


        #region Helpers


        /// <summary>
        /// Masks a destination for storage (privacy).
        /// Email: j***@example.com   Phone: ***4567
        /// </summary>
        private static string MaskDestination(string destination, string providerId)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                return destination;
            }

            if (providerId == "email")
            {
                int atIndex = destination.IndexOf('@');
                if (atIndex > 1)
                {
                    return destination[0] + "***" + destination.Substring(atIndex);
                }
            }
            else if (providerId == "sms")
            {
                if (destination.Length > 4)
                {
                    return "***" + destination.Substring(destination.Length - 4);
                }
            }

            return "***";
        }


        /// <summary>
        /// Builds a simple HTML email body for message alerts.
        /// </summary>
        private static string BuildHtmlMessageAlert(string senderDisplayName, string conversationName, string messagePreview)
        {
            string escapedSender = System.Net.WebUtility.HtmlEncode(senderDisplayName ?? "Someone");
            string escapedConversation = System.Net.WebUtility.HtmlEncode(conversationName ?? "a conversation");
            string escapedMessage = System.Net.WebUtility.HtmlEncode(messagePreview ?? "");

            return $@"<!DOCTYPE html>
<html>
<head>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; color: #333; }}
    .container {{ max-width: 500px; margin: 0 auto; padding: 20px; }}
    .header {{ color: #666; font-size: 14px; margin-bottom: 12px; }}
    .message {{ background: #f5f5f5; border-radius: 8px; padding: 16px; margin-bottom: 16px; }}
    .sender {{ font-weight: 600; margin-bottom: 4px; }}
    .preview {{ color: #555; }}
    .footer {{ color: #999; font-size: 12px; }}
  </style>
</head>
<body>
  <div class=""container"">
    <div class=""header"">New message in <strong>{escapedConversation}</strong></div>
    <div class=""message"">
      <div class=""sender"">{escapedSender}</div>
      <div class=""preview"">{escapedMessage}</div>
    </div>
    <div class=""footer"">You're receiving this because you have email notifications enabled. Manage your settings in Catalyst.</div>
  </div>
</body>
</html>";
        }

        #endregion
    }
}
