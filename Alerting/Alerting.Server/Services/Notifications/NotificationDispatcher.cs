//
// Notification Dispatcher Implementation
//
// Orchestrates notification delivery across multiple channels based on user preferences.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Alerting.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Orchestrates notification delivery across multiple channels.
    /// Handles preference resolution, quiet hours, and channel priority.
    /// </summary>
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly AlertingContext _context;
        private readonly IEnumerable<INotificationProvider> _providers;
        private readonly IUserService _userService;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(
            AlertingContext context,
            IEnumerable<INotificationProvider> providers,
            IUserService userService,
            ILogger<NotificationDispatcher> logger)
        {
            _context = context;
            _providers = providers;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IncidentNotification> DispatchAsync(
            Incident incident,
            Guid userObjectGuid,
            int? escalationRuleId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Dispatching notification for incident {IncidentId} to user {UserGuid}",
                incident.id, userObjectGuid);

            // 1. Create the IncidentNotification record
            var notification = new IncidentNotification
            {
                tenantGuid = incident.tenantGuid,
                incidentId = incident.id,
                escalationRuleId = escalationRuleId,
                userObjectGuid = userObjectGuid,
                firstNotifiedAt = DateTime.UtcNow,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.IncidentNotifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // 2. Load user preferences
            var userPrefs = await GetUserPreferencesAsync(incident.tenantGuid, userObjectGuid, cancellationToken);

            // 3. Check if user is in DND or quiet hours
            if (IsBlockedByPreferences(userPrefs))
            {
                _logger.LogInformation("User {UserGuid} is in DND or quiet hours - skipping notification",
                    userObjectGuid);
                return notification;
            }

            // 4. Get user contact information
            var userInfo = await _userService.GetUserAsync(incident.tenantGuid, userObjectGuid);
            if (userInfo == null)
            {
                _logger.LogWarning("Could not find user {UserGuid} for notification", userObjectGuid);
                return notification;
            }

            // 5. Build notification request
            var request = await BuildNotificationRequestAsync(incident, userInfo, cancellationToken);

            // 6. Determine which channels to use (based on preferences)
            var enabledChannels = await GetEnabledChannelsAsync(userPrefs, cancellationToken);

            // 7. Dispatch to each enabled channel
            foreach (var channelTypeId in enabledChannels)
            {
                var provider = _providers.FirstOrDefault(p => p.ChannelTypeId == channelTypeId);
                if (provider == null)
                {
                    _logger.LogWarning("No provider registered for channel type {ChannelTypeId}", channelTypeId);
                    continue;
                }

                await DispatchToChannelAsync(notification, provider, request, cancellationToken);
            }

            // Update lastNotifiedAt
            notification.lastNotifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return notification;
        }

        private async Task DispatchToChannelAsync(
            IncidentNotification notification,
            INotificationProvider provider,
            NotificationRequest request,
            CancellationToken cancellationToken)
        {
            // Create delivery attempt record
            var attempt = new NotificationDeliveryAttempt
            {
                tenantGuid = notification.tenantGuid,
                incidentNotificationId = notification.id,
                notificationChannelTypeId = provider.ChannelTypeId,
                attemptNumber = 1,
                attemptedAt = DateTime.UtcNow,
                status = "Pending",
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.NotificationDeliveryAttempts.Add(attempt);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // Send the notification
                var result = await provider.SendAsync(request, cancellationToken);

                // Update attempt with result
                attempt.status = result.Success ? "Sent" : "Failed";
                attempt.errorMessage = result.ErrorMessage;
                attempt.response = result.ProviderResponse ?? result.ExternalMessageId;

                _logger.LogInformation(
                    "Notification delivery attempt {AttemptId} for channel {ChannelId}: {Status}",
                    attempt.id, provider.ChannelTypeId, attempt.status);
            }
            catch (Exception ex)
            {
                attempt.status = "Failed";
                attempt.errorMessage = ex.Message;
                _logger.LogError(ex, "Exception during notification delivery for channel {ChannelId}",
                    provider.ChannelTypeId);
            }

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<UserNotificationPreference> GetUserPreferencesAsync(
            Guid tenantGuid,
            Guid userObjectGuid,
            CancellationToken cancellationToken)
        {
            return await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p =>
                    p.tenantGuid == tenantGuid &&
                    p.securityUserObjectGuid == userObjectGuid &&
                    p.active == true &&
                    p.deleted == false,
                    cancellationToken);
        }

        private bool IsBlockedByPreferences(UserNotificationPreference prefs)
        {
            if (prefs == null) return false;

            // Check DND
            if (prefs.isDoNotDisturb == true)
            {
                if (prefs.isDoNotDisturbPermanent == true)
                    return true;

                if (prefs.doNotDisturbUntil.HasValue && DateTime.UtcNow < prefs.doNotDisturbUntil.Value)
                    return true;
            }

            // Check quiet hours
            if (!string.IsNullOrEmpty(prefs.quietHoursStart) && !string.IsNullOrEmpty(prefs.quietHoursEnd))
            {
                if (IsInQuietHours(prefs.quietHoursStart, prefs.quietHoursEnd, prefs.timeZoneId))
                    return true;
            }

            return false;
        }

        private bool IsInQuietHours(string startTime, string endTime, string timeZoneId)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId ?? "UTC");
                var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                var currentTime = localNow.TimeOfDay;

                var start = TimeSpan.Parse(startTime);
                var end = TimeSpan.Parse(endTime);

                // Handle overnight quiet hours (e.g., 22:00 - 07:00)
                if (start > end)
                {
                    return currentTime >= start || currentTime < end;
                }
                else
                {
                    return currentTime >= start && currentTime < end;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking quiet hours for timezone {TimeZone}", timeZoneId);
                return false;
            }
        }

        private async Task<List<int>> GetEnabledChannelsAsync(
            UserNotificationPreference prefs,
            CancellationToken cancellationToken)
        {
            // Start with default channels in priority order
            var defaultChannels = await _context.NotificationChannelTypes
                .Where(c => c.active == true && c.deleted == false)
                .OrderBy(c => c.defaultPriority)
                .Select(c => c.id)
                .ToListAsync(cancellationToken);

            if (prefs == null) return defaultChannels;

            // Get user's channel preferences
            var channelPrefs = await _context.UserNotificationChannelPreferences
                .Where(p =>
                    p.userNotificationPreferenceId == prefs.id &&
                    p.active == true &&
                    p.deleted == false)
                .ToListAsync(cancellationToken);

            if (!channelPrefs.Any()) return defaultChannels;

            // Filter to enabled channels and re-sort by user priority overrides
            var enabledChannels = new List<(int channelId, int priority)>();

            foreach (var channelId in defaultChannels)
            {
                var userPref = channelPrefs.FirstOrDefault(p => p.notificationChannelTypeId == channelId);

                // If user has a preference for this channel
                if (userPref != null)
                {
                    if (userPref.isEnabled == true)
                    {
                        enabledChannels.Add((channelId, userPref.priorityOverride ?? 999));
                    }
                    // Skip disabled channels
                }
                else
                {
                    // No user preference - use default (enabled)
                    enabledChannels.Add((channelId, 999));
                }
            }

            return enabledChannels
                .OrderBy(c => c.priority)
                .Select(c => c.channelId)
                .ToList();
        }

        private async Task<NotificationRequest> BuildNotificationRequestAsync(
            Incident incident,
            User userInfo,
            CancellationToken cancellationToken)
        {
            // Load related data
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.id == incident.serviceId, cancellationToken);

            var severity = await _context.SeverityTypes
                .FirstOrDefaultAsync(s => s.id == incident.severityTypeId, cancellationToken);

            var status = await _context.IncidentStatusTypes
                .FirstOrDefaultAsync(s => s.id == incident.incidentStatusTypeId, cancellationToken);

            return new NotificationRequest
            {
                UserObjectGuid = userInfo.objectGuid,
                UserEmail = userInfo.emailAddress,
                UserPhoneNumber = userInfo.cellPhoneNumber ?? userInfo.phoneNumber,
                PushToken = null, // Future: load from user contact table
                UserDisplayName = $"{userInfo.firstName} {userInfo.lastName}".Trim(),
                TenantGuid = incident.tenantGuid,
                Incident = new IncidentInfo
                {
                    Id = incident.id,
                    IncidentKey = incident.incidentKey,
                    Title = incident.title,
                    Description = incident.description,
                    SeverityName = severity?.name ?? "Unknown",
                    SeverityId = incident.severityTypeId,
                    ServiceName = service?.name,
                    CreatedAt = incident.createdAt,
                    StatusName = status?.name ?? "Unknown"
                }
            };
        }
    }
}
