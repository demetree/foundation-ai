//
// Notification Audit Service
//
// Provides data access and aggregation for the Notification Audit Console.
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
    /// Interface for notification audit operations.
    /// </summary>
    public interface INotificationAuditService
    {
        Task<NotificationAuditMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);
        Task<DeliveryAttemptListResult> GetDeliveryListAsync(DeliveryAttemptQueryParams query, CancellationToken cancellationToken = default);
        Task<AuditDeliveryDetailDto> GetDeliveryDetailAsync(int id, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Service for notification audit data access and aggregation.
    /// </summary>
    public class NotificationAuditService : INotificationAuditService
    {
        private readonly AlertingContext _context;
        private readonly ILogger<NotificationAuditService> _logger;

        public NotificationAuditService(
            AlertingContext context,
            ILogger<NotificationAuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<NotificationAuditMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var today = now.AddHours(-24);
            var week = now.AddDays(-7);

            var metrics = new NotificationAuditMetricsDto();

            // Get counts for today
            var todayAttempts = await _context.NotificationDeliveryAttempts
                .Where(a => a.attemptedAt >= today && a.active == true)
                .GroupBy(a => a.status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            metrics.SentToday = todayAttempts.FirstOrDefault(x => x.Status == "Sent")?.Count ?? 0;
            metrics.FailedToday = todayAttempts.FirstOrDefault(x => x.Status == "Failed")?.Count ?? 0;
            metrics.PendingNow = await _context.NotificationDeliveryAttempts
                .CountAsync(a => a.status == "Pending" && a.active == true, cancellationToken)
                .ConfigureAwait(false);

            // 7-day success rate
            var weekAttempts = await _context.NotificationDeliveryAttempts
                .Where(a => a.attemptedAt >= week && a.active == true && a.status != "Pending")
                .GroupBy(a => a.status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var weekSent = weekAttempts.FirstOrDefault(x => x.Status == "Sent")?.Count ?? 0;
            var weekFailed = weekAttempts.FirstOrDefault(x => x.Status == "Failed")?.Count ?? 0;
            var weekTotal = weekSent + weekFailed;
            metrics.SuccessRate7Day = weekTotal > 0 ? Math.Round((double)weekSent / weekTotal * 100, 1) : 100;

            // Channel breakdown (last 7 days)
            var channelData = await _context.NotificationDeliveryAttempts
                .Where(a => a.attemptedAt >= week && a.active == true)
                .GroupBy(a => a.notificationChannelTypeId)
                .Select(g => new { ChannelTypeId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var channelTypes = await _context.NotificationChannelTypes
                .Where(c => c.active == true)
                .ToDictionaryAsync(c => c.id, c => c.name, cancellationToken)
                .ConfigureAwait(false);

            var totalChannelCount = channelData.Sum(c => c.Count);
            metrics.ChannelBreakdown = channelData.Select(c => new ChannelBreakdownDto
            {
                ChannelTypeId = c.ChannelTypeId,
                ChannelName = channelTypes.GetValueOrDefault(c.ChannelTypeId, "Unknown"),
                Count = c.Count,
                Percentage = totalChannelCount > 0 ? Math.Round((double)c.Count / totalChannelCount * 100, 1) : 0
            }).OrderByDescending(c => c.Count).ToList();

            return metrics;
        }

        /// <inheritdoc/>
        public async Task<DeliveryAttemptListResult> GetDeliveryListAsync(
            DeliveryAttemptQueryParams query,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = _context.NotificationDeliveryAttempts
                .Include(a => a.incidentNotification)
                    .ThenInclude(n => n.incident)
                .Where(a => a.active == true);

            // Apply filters
            if (query.ChannelTypeId.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.notificationChannelTypeId == query.ChannelTypeId.Value);
            }

            if (!string.IsNullOrEmpty(query.Status))
            {
                baseQuery = baseQuery.Where(a => a.status == query.Status);
            }

            if (query.DateFrom.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.attemptedAt >= query.DateFrom.Value);
            }

            if (query.DateTo.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.attemptedAt <= query.DateTo.Value);
            }

            if (query.IncidentId.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.incidentNotification.incidentId == query.IncidentId.Value);
            }

            if (query.UserObjectGuid.HasValue)
            {
                baseQuery = baseQuery.Where(a => a.incidentNotification.userObjectGuid == query.UserObjectGuid.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchQuery))
            {
                var search = query.SearchQuery.Trim().ToLower();
                baseQuery = baseQuery.Where(a =>
                    (a.recipientAddress != null && a.recipientAddress.ToLower().Contains(search)) ||
                    (a.subject != null && a.subject.ToLower().Contains(search)) ||
                    (a.incidentNotification.incident.incidentKey != null && 
                     a.incidentNotification.incident.incidentKey.ToLower().Contains(search)));
            }

            // Get total count before pagination
            var totalCount = await baseQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            // Apply sorting
            IOrderedQueryable<NotificationDeliveryAttempt> orderedQuery = query.SortBy?.ToLower() switch
            {
                "status" => query.SortDescending 
                    ? baseQuery.OrderByDescending(a => a.status) 
                    : baseQuery.OrderBy(a => a.status),
                "channel" => query.SortDescending 
                    ? baseQuery.OrderByDescending(a => a.notificationChannelTypeId) 
                    : baseQuery.OrderBy(a => a.notificationChannelTypeId),
                _ => query.SortDescending 
                    ? baseQuery.OrderByDescending(a => a.attemptedAt) 
                    : baseQuery.OrderBy(a => a.attemptedAt)
            };

            // Apply pagination
            var skip = (query.PageNumber - 1) * query.PageSize;
            var attempts = await orderedQuery
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Get channel type names
            var channelTypes = await _context.NotificationChannelTypes
                .Where(c => c.active == true)
                .ToDictionaryAsync(c => c.id, c => c.name, cancellationToken)
                .ConfigureAwait(false);

            // Map to DTOs
            var items = attempts.Select(a => new AuditDeliverySummaryDto
            {
                Id = a.id,
                ObjectGuid = a.objectGuid,
                AttemptedAt = a.attemptedAt,
                ChannelTypeId = a.notificationChannelTypeId,
                ChannelName = channelTypes.GetValueOrDefault(a.notificationChannelTypeId, "Unknown"),
                Status = a.status,
                ErrorMessage = a.errorMessage,
                RecipientAddress = a.recipientAddress,
                RecipientDisplay = MaskRecipient(a.recipientAddress, a.notificationChannelTypeId),
                IncidentId = a.incidentNotification?.incidentId,
                IncidentKey = a.incidentNotification?.incident?.incidentKey,
                IncidentTitle = a.incidentNotification?.incident?.title,
                UserObjectGuid = a.incidentNotification?.userObjectGuid
            }).ToList();

            return new DeliveryAttemptListResult
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        /// <inheritdoc/>
        public async Task<AuditDeliveryDetailDto> GetDeliveryDetailAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var attempt = await _context.NotificationDeliveryAttempts
                .Include(a => a.incidentNotification)
                    .ThenInclude(n => n.incident)
                .FirstOrDefaultAsync(a => a.id == id && a.active == true, cancellationToken)
                .ConfigureAwait(false);

            if (attempt == null)
            {
                return null;
            }

            // Get channel name
            var channelName = await _context.NotificationChannelTypes
                .Where(c => c.id == attempt.notificationChannelTypeId)
                .Select(c => c.name)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            // Get total attempts for this notification
            var totalAttempts = await _context.NotificationDeliveryAttempts
                .CountAsync(a => 
                    a.incidentNotificationId == attempt.incidentNotificationId &&
                    a.notificationChannelTypeId == attempt.notificationChannelTypeId &&
                    a.active == true, 
                    cancellationToken)
                .ConfigureAwait(false);

            return new AuditDeliveryDetailDto
            {
                Id = attempt.id,
                ObjectGuid = attempt.objectGuid,
                AttemptedAt = attempt.attemptedAt,
                ChannelTypeId = attempt.notificationChannelTypeId,
                ChannelName = channelName ?? "Unknown",
                Status = attempt.status,
                ErrorMessage = attempt.errorMessage,
                RecipientAddress = attempt.recipientAddress,
                RecipientDisplay = attempt.recipientAddress, // Full address in detail view
                IncidentId = attempt.incidentNotification?.incidentId,
                IncidentKey = attempt.incidentNotification?.incident?.incidentKey,
                IncidentTitle = attempt.incidentNotification?.incident?.title,
                UserObjectGuid = attempt.incidentNotification?.userObjectGuid,
                Subject = attempt.subject,
                BodyContent = attempt.bodyContent,
                Response = attempt.response,
                AttemptNumber = attempt.attemptNumber,
                TotalAttempts = totalAttempts,
                NotificationCreatedAt = attempt.incidentNotification?.firstNotifiedAt,
                IncidentCreatedAt = attempt.incidentNotification?.incident?.createdAt
            };
        }

        /// <summary>
        /// Mask recipient address for list display (privacy).
        /// </summary>
        private string MaskRecipient(string address, int channelTypeId)
        {
            if (string.IsNullOrEmpty(address)) return "—";

            // Email: show first 3 chars + *** + domain
            if (channelTypeId == 1 && address.Contains('@'))
            {
                var parts = address.Split('@');
                var local = parts[0].Length > 3 ? parts[0].Substring(0, 3) + "***" : parts[0];
                return $"{local}@{parts[1]}";
            }

            // Phone: show last 4 digits
            if (channelTypeId == 2 || channelTypeId == 3) // SMS or Voice
            {
                if (address.Length > 4)
                {
                    return new string('*', address.Length - 4) + address.Substring(address.Length - 4);
                }
            }

            // Push token / webhook: truncate
            if (address.Length > 20)
            {
                return address.Substring(0, 17) + "...";
            }

            return address;
        }
    }
}
