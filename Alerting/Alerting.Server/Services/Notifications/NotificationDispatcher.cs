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

            NotificationLogger.Debug($"NotificationDispatcher initialized with {_providers.Count()} providers: {string.Join(", ", _providers.Select(p => p.GetType().Name))}");
        }

        public async Task<IncidentNotification> DispatchAsync(
            Incident incident,
            Guid userObjectGuid,
            int? escalationRuleId,
            CancellationToken cancellationToken = default)
        {
            NotificationLogger.Debug($"=== BEGIN DispatchAsync ===");
            NotificationLogger.Debug($"Incident ID: {incident.id}, Key: {incident.incidentKey}, Title: {incident.title}");
            NotificationLogger.Debug($"User ObjectGuid: {userObjectGuid}, EscalationRuleId: {escalationRuleId?.ToString() ?? "null"}");

            _logger.LogInformation("Dispatching notification for incident {IncidentId} to user {UserGuid}",
                incident.id, userObjectGuid);

            // 1. Create the IncidentNotification record
            NotificationLogger.Debug("Step 1: Creating IncidentNotification record");
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
            NotificationLogger.Debug($"IncidentNotification created with ID: {notification.id}, ObjectGuid: {notification.objectGuid}");

            // 2. Load user preferences
            NotificationLogger.Debug("Step 2: Loading user notification preferences");
            var userPrefs = await GetUserPreferencesAsync(incident.tenantGuid, userObjectGuid, cancellationToken);

            if (userPrefs != null)
            {
                NotificationLogger.Debug($"User preferences found - ID: {userPrefs.id}, TimeZone: {userPrefs.timeZoneId ?? "default"}");
                NotificationLogger.Debug($"  DND: {userPrefs.isDoNotDisturb}, DND Permanent: {userPrefs.isDoNotDisturbPermanent}, DND Until: {userPrefs.doNotDisturbUntil?.ToString() ?? "n/a"}");
                NotificationLogger.Debug($"  Quiet Hours: {userPrefs.quietHoursStart ?? "none"} - {userPrefs.quietHoursEnd ?? "none"}");
            }
            else
            {
                NotificationLogger.Debug("No user preferences found - using system defaults");
            }

            // 3. Check if user is in DND or quiet hours
            NotificationLogger.Debug("Step 3: Checking DND and quiet hours");
            if (IsBlockedByPreferences(userPrefs))
            {
                NotificationLogger.Info($"User {userObjectGuid} is blocked by DND or quiet hours - skipping notification dispatch");
                _logger.LogInformation("User {UserGuid} is in DND or quiet hours - skipping notification",
                    userObjectGuid);
                return notification;
            }
            NotificationLogger.Debug("User is not blocked by DND or quiet hours - proceeding");

            // 4. Get user contact information
            NotificationLogger.Debug("Step 4: Retrieving user contact information");
            var userInfo = await _userService.GetUserAsync(incident.tenantGuid, userObjectGuid);
            if (userInfo == null)
            {
                NotificationLogger.Warning($"Could not find user {userObjectGuid} in tenant {incident.tenantGuid} - cannot dispatch notification");
                _logger.LogWarning("Could not find user {UserGuid} for notification", userObjectGuid);
                return notification;
            }
            NotificationLogger.Debug($"User info retrieved: {userInfo.firstName} {userInfo.lastName}, Email: {userInfo.emailAddress ?? "none"}, Phone: {userInfo.cellPhoneNumber ?? userInfo.phoneNumber ?? "none"}");

            // 5. Build notification request
            NotificationLogger.Debug("Step 5: Building notification request");
            var request = await BuildNotificationRequestAsync(incident, userInfo, cancellationToken);
            NotificationLogger.Debug($"Notification request built - Severity: {request.Incident.SeverityName}, Service: {request.Incident.ServiceName}");

            // 6. Determine which channels to use (based on preferences)
            NotificationLogger.Debug("Step 6: Determining enabled channels");
            var enabledChannels = await GetEnabledChannelsAsync(userPrefs, cancellationToken);
            NotificationLogger.Debug($"Enabled channels (in priority order): [{string.Join(", ", enabledChannels)}]");

            // 7. Dispatch to each enabled channel
            NotificationLogger.Debug("Step 7: Dispatching to enabled channels");
            foreach (var channelTypeId in enabledChannels)
            {
                var provider = _providers.FirstOrDefault(p => p.ChannelTypeId == channelTypeId);
                if (provider == null)
                {
                    NotificationLogger.Warning($"No provider registered for channel type {channelTypeId} - skipping");
                    _logger.LogWarning("No provider registered for channel type {ChannelTypeId}", channelTypeId);
                    continue;
                }

                NotificationLogger.Debug($"Dispatching to channel {channelTypeId} via {provider.GetType().Name}");
                await DispatchToChannelAsync(notification, provider, request, cancellationToken);
            }

            // Update lastNotifiedAt
            notification.lastNotifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            NotificationLogger.Debug($"=== END DispatchAsync === Notification {notification.id} completed at {notification.lastNotifiedAt}");
            return notification;
        }

        private async Task DispatchToChannelAsync(
            IncidentNotification notification,
            INotificationProvider provider,
            NotificationRequest request,
            CancellationToken cancellationToken)
        {
            NotificationLogger.Debug($"--- BEGIN DispatchToChannelAsync: Channel {provider.ChannelTypeId} ({provider.GetType().Name}) ---");

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
            NotificationLogger.Debug($"Delivery attempt record created - ID: {attempt.id}, Status: {attempt.status}");

            try
            {
                NotificationLogger.Debug($"Calling provider.SendAsync for user {request.UserObjectGuid}");
                var startTime = DateTime.UtcNow;

                // Send the notification
                var result = await provider.SendAsync(request, cancellationToken);

                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                NotificationLogger.Debug($"Provider returned in {elapsed:F0}ms - Success: {result.Success}");

                // Update attempt with result
                attempt.status = result.Success ? "Sent" : "Failed";
                attempt.errorMessage = result.ErrorMessage;
                attempt.response = result.ProviderResponse ?? result.ExternalMessageId;
                
                // Content archival for forensic auditing
                attempt.recipientAddress = result.RecipientAddress;
                attempt.subject = result.Subject;
                attempt.bodyContent = result.BodyContent;

                if (result.Success)
                {
                    NotificationLogger.Info($"Notification delivered successfully via {provider.GetType().Name} for incident {request.Incident.IncidentKey}");
                }
                else
                {
                    NotificationLogger.Error($"Notification delivery FAILED via {provider.GetType().Name}: {result.ErrorMessage}");
                }

                _logger.LogInformation(
                    "Notification delivery attempt {AttemptId} for channel {ChannelId}: {Status}",
                    attempt.id, provider.ChannelTypeId, attempt.status);
            }
            catch (Exception ex)
            {
                attempt.status = "Failed";
                attempt.errorMessage = ex.Message;

                NotificationLogger.Exception($"Exception during notification delivery via {provider.GetType().Name}", ex);
                _logger.LogError(ex, "Exception during notification delivery for channel {ChannelId}",
                    provider.ChannelTypeId);
            }

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            NotificationLogger.Debug($"Delivery attempt {attempt.id} final status: {attempt.status}");
            NotificationLogger.Debug($"--- END DispatchToChannelAsync ---");
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
            if (prefs == null)
            {
                NotificationLogger.Debug("IsBlockedByPreferences: No preferences set, not blocked");
                return false;
            }

            // Check DND
            if (prefs.isDoNotDisturb == true)
            {
                if (prefs.isDoNotDisturbPermanent == true)
                {
                    NotificationLogger.Debug("IsBlockedByPreferences: BLOCKED by permanent DND");
                    return true;
                }

                if (prefs.doNotDisturbUntil.HasValue && DateTime.UtcNow < prefs.doNotDisturbUntil.Value)
                {
                    NotificationLogger.Debug($"IsBlockedByPreferences: BLOCKED by temporary DND until {prefs.doNotDisturbUntil.Value}");
                    return true;
                }

                NotificationLogger.Debug("IsBlockedByPreferences: DND is set but expired");
            }

            // Check quiet hours
            if (!string.IsNullOrEmpty(prefs.quietHoursStart) && !string.IsNullOrEmpty(prefs.quietHoursEnd))
            {
                if (IsInQuietHours(prefs.quietHoursStart, prefs.quietHoursEnd, prefs.timeZoneId))
                {
                    NotificationLogger.Debug($"IsBlockedByPreferences: BLOCKED by quiet hours ({prefs.quietHoursStart} - {prefs.quietHoursEnd})");
                    return true;
                }

                NotificationLogger.Debug("IsBlockedByPreferences: Outside quiet hours");
            }

            NotificationLogger.Debug("IsBlockedByPreferences: Not blocked");
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

                NotificationLogger.Debug($"IsInQuietHours: TZ={timeZoneId ?? "UTC"}, LocalTime={localNow:HH:mm}, QuietHours={startTime}-{endTime}");

                // Handle overnight quiet hours (e.g., 22:00 - 07:00)
                if (start > end)
                {
                    var inQuiet = currentTime >= start || currentTime < end;
                    NotificationLogger.Debug($"IsInQuietHours (overnight): {inQuiet}");
                    return inQuiet;
                }
                else
                {
                    var inQuiet = currentTime >= start && currentTime < end;
                    NotificationLogger.Debug($"IsInQuietHours (same-day): {inQuiet}");
                    return inQuiet;
                }
            }
            catch (Exception ex)
            {
                NotificationLogger.Warning($"Error checking quiet hours for timezone {timeZoneId}: {ex.Message}");
                _logger.LogWarning(ex, "Error checking quiet hours for timezone {TimeZone}", timeZoneId);
                return false;
            }
        }

        private async Task<List<int>> GetEnabledChannelsAsync(
            UserNotificationPreference prefs,
            CancellationToken cancellationToken)
        {
            NotificationLogger.Debug("GetEnabledChannelsAsync: Loading default channel types");

            // Start with default channels in priority order
            var defaultChannels = await _context.NotificationChannelTypes
                .Where(c => c.active == true && c.deleted == false)
                .OrderBy(c => c.defaultPriority)
                .Select(c => c.id)
                .ToListAsync(cancellationToken);

            NotificationLogger.Debug($"GetEnabledChannelsAsync: Default channels: [{string.Join(", ", defaultChannels)}]");

            if (prefs == null)
            {
                NotificationLogger.Debug("GetEnabledChannelsAsync: No user prefs, returning defaults");
                return defaultChannels;
            }

            // Get user's channel preferences
            var channelPrefs = await _context.UserNotificationChannelPreferences
                .Where(p =>
                    p.userNotificationPreferenceId == prefs.id &&
                    p.active == true &&
                    p.deleted == false)
                .ToListAsync(cancellationToken);

            NotificationLogger.Debug($"GetEnabledChannelsAsync: Found {channelPrefs.Count} user channel preferences");

            if (!channelPrefs.Any())
            {
                NotificationLogger.Debug("GetEnabledChannelsAsync: No channel overrides, returning defaults");
                return defaultChannels;
            }

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
                        NotificationLogger.Debug($"  Channel {channelId}: ENABLED (priority {userPref.priorityOverride ?? 999})");
                    }
                    else
                    {
                        NotificationLogger.Debug($"  Channel {channelId}: DISABLED by user");
                    }
                }
                else
                {
                    // No user preference - use default (enabled)
                    enabledChannels.Add((channelId, 999));
                    NotificationLogger.Debug($"  Channel {channelId}: enabled (default, priority 999)");
                }
            }

            var result = enabledChannels
                .OrderBy(c => c.priority)
                .Select(c => c.channelId)
                .ToList();

            NotificationLogger.Debug($"GetEnabledChannelsAsync: Final enabled channels (sorted): [{string.Join(", ", result)}]");
            return result;
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

