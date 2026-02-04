//
// Notification Flight Control Service Implementation
//
// Aggregates real-time metrics from the notification engine for the flight control dashboard.
// AI-assisted development - February 2026
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alerting.Server.Models;
using Foundation.Alerting.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Aggregates real-time notification engine metrics for the flight control dashboard.
    /// </summary>
    public class NotificationFlightControlService : INotificationFlightControlService
    {
        private readonly AlertingContext _context;
        private readonly ILogger<NotificationFlightControlService> _logger;

        //
        // Static worker state tracking (in-memory for this process)
        // In a distributed environment, this would need to be stored in a shared cache
        //
        private static WorkerStatusDto _escalationWorkerStatus = new()
        {
            WorkerName = "EscalationWorker",
            IsRunning = true
        };

        private static WorkerStatusDto _retryWorkerStatus = new()
        {
            WorkerName = "RetryWorker",
            IsRunning = true
        };

        public NotificationFlightControlService(
            AlertingContext context,
            ILogger<NotificationFlightControlService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Updates escalation worker status (called by EscalationWorker).
        /// </summary>
        public static void UpdateEscalationWorkerStatus(DateTime lastRunAt, int itemsProcessed)
        {
            _escalationWorkerStatus.LastRunAt = lastRunAt;
            _escalationWorkerStatus.ItemsProcessedLastRun = itemsProcessed;
            _escalationWorkerStatus.TotalItemsProcessed += itemsProcessed;
            _escalationWorkerStatus.NextRunAt = lastRunAt.AddSeconds(30); // Assuming 30s interval
        }

        /// <summary>
        /// Updates retry worker status (called by NotificationRetryWorker).
        /// </summary>
        public static void UpdateRetryWorkerStatus(DateTime lastRunAt, int itemsProcessed)
        {
            _retryWorkerStatus.LastRunAt = lastRunAt;
            _retryWorkerStatus.ItemsProcessedLastRun = itemsProcessed;
            _retryWorkerStatus.TotalItemsProcessed += itemsProcessed;
            _retryWorkerStatus.NextRunAt = lastRunAt.AddMinutes(1); // Assuming 1 min interval
        }

        /// <inheritdoc/>
        public async Task<NotificationFlightControlDto> GetSummaryAsync(
            FlightControlTimeRange timeRange,
            string channelFilter,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var rangeStart = now.AddHours(-(int)timeRange);

            _logger.LogDebug("Getting flight control summary: TimeRange={TimeRange}, ChannelFilter={ChannelFilter}",
                timeRange, channelFilter ?? "All");

            //
            // Execute queries sequentially - DbContext is NOT thread-safe
            //
            var queueMetrics = await GetQueueMetricsAsync(rangeStart, now, channelFilter, cancellationToken)
                .ConfigureAwait(false);

            var channelMetrics = await GetChannelMetricsAsync(rangeStart, now, cancellationToken)
                .ConfigureAwait(false);

            var recentDeliveries = await GetRecentDeliveriesAsync(rangeStart, channelFilter, cancellationToken)
                .ConfigureAwait(false);

            var recentWebhooks = await GetRecentWebhooksAsync(rangeStart, cancellationToken)
                .ConfigureAwait(false);

            var pendingQueue = await GetPendingQueueAsync(cancellationToken)
                .ConfigureAwait(false);

            return new NotificationFlightControlDto
            {
                GeneratedAt = now,
                TimeRange = timeRange,
                ChannelFilter = channelFilter,
                EscalationWorker = _escalationWorkerStatus,
                RetryWorker = _retryWorkerStatus,
                Queue = queueMetrics,
                ChannelMetrics = channelMetrics,
                RecentDeliveries = recentDeliveries,
                RecentWebhooks = recentWebhooks,
                PendingQueue = pendingQueue
            };
        }

        /// <inheritdoc/>
        public async Task<DeliveryAttemptDetailDto> GetDeliveryAttemptDetailAsync(
            long deliveryAttemptId,
            CancellationToken cancellationToken = default)
        {
            var attempt = await _context.NotificationDeliveryAttempts
                .Include(a => a.incidentNotification)
                    .ThenInclude(n => n.incident)
                        .ThenInclude(i => i.severityType)
                .Include(a => a.notificationChannelType)
                .Where(a => a.id == deliveryAttemptId)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (attempt == null)
            {
                return null;
            }

            return new DeliveryAttemptDetailDto
            {
                Id = attempt.id,
                AttemptedAt = attempt.attemptedAt,
                Status = attempt.status,
                ChannelName = attempt.notificationChannelType?.name ?? "Unknown",
                ChannelTypeId = attempt.notificationChannelTypeId,
                AttemptNumber = attempt.attemptNumber,
                RecipientSummary = $"User: {attempt.incidentNotification?.userObjectGuid}",
                ErrorMessage = attempt.errorMessage,
                Response = attempt.response,
                IncidentId = attempt.incidentNotification?.incidentId ?? 0,
                IncidentTitle = attempt.incidentNotification?.incident?.title ?? "Unknown",
                IncidentDescription = attempt.incidentNotification?.incident?.description,
                SeverityName = attempt.incidentNotification?.incident?.severityType?.name ?? "Unknown",
                UserObjectGuid = attempt.incidentNotification?.userObjectGuid
            };
        }

        /// <inheritdoc/>
        public async Task<WebhookAttemptDetailDto> GetWebhookAttemptDetailAsync(
            long webhookAttemptId,
            CancellationToken cancellationToken = default)
        {
            var attempt = await _context.WebhookDeliveryAttempts
                .Include(a => a.incident)
                .Include(a => a.integration)
                .Where(a => a.id == webhookAttemptId)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (attempt == null)
            {
                return null;
            }

            return new WebhookAttemptDetailDto
            {
                Id = attempt.id,
                AttemptedAt = attempt.attemptedAt,
                Success = attempt.success,
                HttpStatusCode = attempt.httpStatusCode,
                TargetUrl = "", // Not stored in entity - would need to lookup from integration
                AttemptNumber = attempt.attemptNumber,
                ErrorMessage = attempt.errorMessage,
                IncidentId = attempt.incidentId,
                IncidentTitle = attempt.incident?.title ?? "Unknown",
                IntegrationName = attempt.integration?.name ?? "Unknown",
                PayloadJson = attempt.payloadJson,
                ResponseBody = attempt.responseBody
            };
        }

        #region Private Methods

        private async Task<NotificationQueueMetricsDto> GetQueueMetricsAsync(
            DateTime from,
            DateTime to,
            string channelFilter,
            CancellationToken cancellationToken)
        {
            var attemptsQuery = _context.NotificationDeliveryAttempts
                .Where(a => a.attemptedAt >= from && a.attemptedAt <= to);

            if (!string.IsNullOrEmpty(channelFilter))
            {
                attemptsQuery = attemptsQuery
                    .Include(a => a.notificationChannelType)
                    .Where(a => a.notificationChannelType.name == channelFilter);
            }

            var totalAttempts = await attemptsQuery.CountAsync(cancellationToken).ConfigureAwait(false);
            var delivered = await attemptsQuery.CountAsync(a => a.status == "Delivered", cancellationToken).ConfigureAwait(false);
            var failed = await attemptsQuery.CountAsync(a => a.status == "Failed", cancellationToken).ConfigureAwait(false);
            var pending = await attemptsQuery.CountAsync(a => a.status == "Pending", cancellationToken).ConfigureAwait(false);

            decimal successRate = totalAttempts > 0 ? Math.Round((decimal)delivered / totalAttempts * 100, 1) : 0;

            return new NotificationQueueMetricsDto
            {
                PendingCount = pending,
                FailedCount = failed,
                DeliveredCount = delivered,
                TotalAttempts = totalAttempts,
                SuccessRate = successRate
            };
        }

        private async Task<List<ChannelDeliveryMetricsDto>> GetChannelMetricsAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken)
        {
            var channelTypes = await _context.NotificationChannelTypes
                .Where(c => c.active && !c.deleted)
                .OrderBy(c => c.id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var result = new List<ChannelDeliveryMetricsDto>();

            foreach (var channel in channelTypes)
            {
                var attemptsQuery = _context.NotificationDeliveryAttempts
                    .Where(a => a.notificationChannelTypeId == channel.id &&
                                a.attemptedAt >= from && a.attemptedAt <= to);

                var total = await attemptsQuery.CountAsync(cancellationToken).ConfigureAwait(false);
                var success = await attemptsQuery.CountAsync(a => a.status == "Delivered", cancellationToken).ConfigureAwait(false);
                var failed = await attemptsQuery.CountAsync(a => a.status == "Failed", cancellationToken).ConfigureAwait(false);
                var pending = await attemptsQuery.CountAsync(a => a.status == "Pending", cancellationToken).ConfigureAwait(false);

                result.Add(new ChannelDeliveryMetricsDto
                {
                    ChannelTypeId = channel.id,
                    ChannelName = channel.name,
                    TotalAttempts = total,
                    SuccessCount = success,
                    FailedCount = failed,
                    PendingCount = pending,
                    SuccessRate = total > 0 ? Math.Round((decimal)success / total * 100, 1) : 0
                });
            }

            return result;
        }

        private async Task<List<RecentDeliveryAttemptDto>> GetRecentDeliveriesAsync(
            DateTime from,
            string channelFilter,
            CancellationToken cancellationToken)
        {
            var query = _context.NotificationDeliveryAttempts
                .Include(a => a.notificationChannelType)
                .Include(a => a.incidentNotification)
                    .ThenInclude(n => n.incident)
                .Where(a => a.attemptedAt >= from)
                .OrderByDescending(a => a.attemptedAt);

            if (!string.IsNullOrEmpty(channelFilter))
            {
                query = (IOrderedQueryable<NotificationDeliveryAttempt>)query
                    .Where(a => a.notificationChannelType.name == channelFilter);
            }

            var attempts = await query
                .Take(50)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return attempts.Select(a => new RecentDeliveryAttemptDto
            {
                Id = a.id,
                AttemptedAt = a.attemptedAt,
                Status = a.status,
                ChannelName = a.notificationChannelType?.name ?? "Unknown",
                ChannelTypeId = a.notificationChannelTypeId,
                AttemptNumber = a.attemptNumber,
                RecipientSummary = $"User: {a.incidentNotification?.userObjectGuid}",
                ErrorMessage = a.errorMessage,
                Response = a.response,
                IncidentId = a.incidentNotification?.incidentId ?? 0,
                IncidentTitle = a.incidentNotification?.incident?.title ?? "Unknown",
                UserObjectGuid = a.incidentNotification?.userObjectGuid
            }).ToList();
        }

        private async Task<List<RecentWebhookAttemptDto>> GetRecentWebhooksAsync(
            DateTime from,
            CancellationToken cancellationToken)
        {
            var attempts = await _context.WebhookDeliveryAttempts
                .Include(a => a.incident)
                .Include(a => a.integration)
                .Where(a => a.attemptedAt >= from)
                .OrderByDescending(a => a.attemptedAt)
                .Take(50)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return attempts.Select(a => new RecentWebhookAttemptDto
            {
                Id = a.id,
                AttemptedAt = a.attemptedAt,
                Success = a.success,
                HttpStatusCode = a.httpStatusCode,
                TargetUrl = "", // Not stored - would need integration lookup
                AttemptNumber = a.attemptNumber,
                ErrorMessage = a.errorMessage,
                IncidentId = a.incidentId,
                IncidentTitle = a.incident?.title ?? "Unknown",
                IntegrationName = a.integration?.name ?? "Unknown"
            }).ToList();
        }

        private async Task<List<PendingNotificationDto>> GetPendingQueueAsync(
            CancellationToken cancellationToken)
        {
            //
            // Get notifications that have pending delivery attempts
            //
            var pendingNotifications = await _context.IncidentNotifications
                .Include(n => n.incident)
                .Include(n => n.NotificationDeliveryAttempts)
                .Where(n => n.active && !n.deleted &&
                           n.NotificationDeliveryAttempts.Any(a => a.status == "Pending"))
                .OrderByDescending(n => n.firstNotifiedAt)
                .Take(20)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return pendingNotifications.Select(n => new PendingNotificationDto
            {
                NotificationId = n.id,
                IncidentId = n.incidentId,
                IncidentTitle = n.incident?.title ?? "Unknown",
                UserObjectGuid = n.userObjectGuid,
                UserDisplayName = "User", // Would need Security module lookup
                FirstNotifiedAt = n.firstNotifiedAt,
                PendingChannels = n.NotificationDeliveryAttempts.Count(a => a.status == "Pending")
            }).ToList();
        }

        #endregion
    }
}
