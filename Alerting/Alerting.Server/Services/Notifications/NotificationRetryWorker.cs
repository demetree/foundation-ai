//
// Notification Retry Worker
//
// Background service that retries failed notification deliveries with exponential backoff.
//
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Alerting.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Background service that retries failed notification delivery attempts.
    /// Uses exponential backoff: 1 min, 5 min, 15 min.
    /// </summary>
    public class NotificationRetryWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationRetryWorker> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(60);
        private const int MaxRetryAttempts = 3;

        // Backoff intervals in minutes: attempt 1 = 1 min, attempt 2 = 5 min, attempt 3 = 15 min
        private static readonly int[] BackoffMinutes = { 1, 5, 15 };

        public NotificationRetryWorker(
            IServiceProvider serviceProvider,
            ILogger<NotificationRetryWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationRetryWorker starting with {Interval}s interval", _interval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessFailedAttemptsAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in NotificationRetryWorker");
                }

                await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
            }

            _logger.LogInformation("NotificationRetryWorker stopping");
        }

        private async Task ProcessFailedAttemptsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AlertingContext>();
            var providers = scope.ServiceProvider.GetServices<INotificationProvider>().ToList();

            // Find failed attempts that need retry
            var failedAttempts = await context.NotificationDeliveryAttempts
                .Include(a => a.incidentNotification)
                    .ThenInclude(n => n.incident)
                .Where(a => 
                    a.status == "Failed" &&
                    a.attemptNumber < MaxRetryAttempts &&
                    a.active == true &&
                    a.deleted == false)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (failedAttempts.Count == 0)
                return;

            _logger.LogDebug("Found {Count} failed notification attempts to retry", failedAttempts.Count);

            foreach (var attempt in failedAttempts)
            {
                // Check if enough time has passed for backoff
                var backoffMinutes = attempt.attemptNumber < BackoffMinutes.Length 
                    ? BackoffMinutes[attempt.attemptNumber] 
                    : BackoffMinutes[BackoffMinutes.Length - 1];

                var nextRetryTime = attempt.attemptedAt.AddMinutes(backoffMinutes);
                if (DateTime.UtcNow < nextRetryTime)
                {
                    _logger.LogDebug(
                        "Skipping attempt {AttemptId} - backoff not elapsed (next retry at {NextRetry})",
                        attempt.id, nextRetryTime);
                    continue;
                }

                await RetryAttemptAsync(context, providers, attempt, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task RetryAttemptAsync(
            AlertingContext context,
            System.Collections.Generic.List<INotificationProvider> providers,
            NotificationDeliveryAttempt attempt,
            CancellationToken cancellationToken)
        {
            var provider = providers.FirstOrDefault(p => p.ChannelTypeId == attempt.notificationChannelTypeId);
            if (provider == null)
            {
                _logger.LogWarning(
                    "No provider found for channel type {ChannelType} - abandoning attempt {AttemptId}",
                    attempt.notificationChannelTypeId, attempt.id);
                attempt.status = "Abandoned";
                attempt.errorMessage = "No provider registered for this channel type";
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            // Rebuild notification request from incident
            var notification = attempt.incidentNotification;
            if (notification?.incident == null)
            {
                _logger.LogWarning("Could not load incident for attempt {AttemptId}", attempt.id);
                attempt.status = "Abandoned";
                attempt.errorMessage = "Could not retrieve incident data";
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            // Get user service to rebuild request
            var userService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.GetUserAsync(notification.tenantGuid, notification.userObjectGuid);
            
            if (userInfo == null)
            {
                attempt.status = "Abandoned";
                attempt.errorMessage = "Could not retrieve user information";
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            // Build request
            var incident = notification.incident;
            var request = new NotificationRequest
            {
                UserObjectGuid = userInfo.objectGuid,
                UserEmail = userInfo.emailAddress,
                UserPhoneNumber = userInfo.cellPhoneNumber ?? userInfo.phoneNumber,
                UserDisplayName = $"{userInfo.firstName} {userInfo.lastName}".Trim(),
                TenantGuid = incident.tenantGuid,
                Incident = new IncidentInfo
                {
                    Id = incident.id,
                    IncidentKey = incident.incidentKey,
                    Title = incident.title,
                    Description = incident.description,
                    SeverityId = incident.severityTypeId,
                    CreatedAt = incident.createdAt
                }
            };

            // Update attempt number and time before retrying
            attempt.attemptNumber++;
            attempt.attemptedAt = DateTime.UtcNow;
            attempt.status = "Retrying";
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                _logger.LogInformation(
                    "Retrying notification attempt {AttemptId} (attempt #{Number})",
                    attempt.id, attempt.attemptNumber);

                var result = await provider.SendAsync(request, cancellationToken);

                attempt.status = result.Success ? "Sent" : "Failed";
                attempt.errorMessage = result.ErrorMessage;
                attempt.response = result.ProviderResponse ?? result.ExternalMessageId;

                _logger.LogInformation(
                    "Retry result for attempt {AttemptId}: {Status}",
                    attempt.id, attempt.status);
            }
            catch (Exception ex)
            {
                attempt.status = attempt.attemptNumber >= MaxRetryAttempts ? "Abandoned" : "Failed";
                attempt.errorMessage = ex.Message;
                _logger.LogError(ex, "Exception during retry of attempt {AttemptId}", attempt.id);
            }

            // Mark as abandoned if max retries reached
            if (attempt.attemptNumber >= MaxRetryAttempts && attempt.status == "Failed")
            {
                attempt.status = "Abandoned";
                _logger.LogWarning(
                    "Attempt {AttemptId} abandoned after {MaxRetries} retries",
                    attempt.id, MaxRetryAttempts);
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
