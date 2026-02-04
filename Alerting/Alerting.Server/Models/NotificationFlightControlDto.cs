//
// Notification Flight Control DTOs
//
// Data transfer objects for the real-time notification engine monitoring dashboard.
// AI-assisted development - February 2026
//
using System;
using System.Collections.Generic;

namespace Alerting.Server.Models
{
    /// <summary>
    /// Time range options for filtering flight control data.
    /// </summary>
    public enum FlightControlTimeRange
    {
        OneHour = 1,
        SixHours = 6,
        TwentyFourHours = 24,
        SevenDays = 168
    }

    /// <summary>
    /// Main flight control summary response DTO.
    /// Provides real-time visibility into the notification engine.
    /// </summary>
    public class NotificationFlightControlDto
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public FlightControlTimeRange TimeRange { get; set; } = FlightControlTimeRange.TwentyFourHours;
        public string ChannelFilter { get; set; }
        public WorkerStatusDto EscalationWorker { get; set; } = new();
        public WorkerStatusDto RetryWorker { get; set; } = new();
        public NotificationQueueMetricsDto Queue { get; set; } = new();
        public List<ChannelDeliveryMetricsDto> ChannelMetrics { get; set; } = new();
        public List<RecentDeliveryAttemptDto> RecentDeliveries { get; set; } = new();
        public List<RecentWebhookAttemptDto> RecentWebhooks { get; set; } = new();
        public List<PendingNotificationDto> PendingQueue { get; set; } = new();
    }

    /// <summary>
    /// Background worker status indicator.
    /// </summary>
    public class WorkerStatusDto
    {
        public string WorkerName { get; set; } = string.Empty;
        public bool IsRunning { get; set; }
        public DateTime? LastRunAt { get; set; }
        public DateTime? NextRunAt { get; set; }
        public int ItemsProcessedLastRun { get; set; }
        public int TotalItemsProcessed { get; set; }
    }

    /// <summary>
    /// Aggregated notification queue metrics.
    /// </summary>
    public class NotificationQueueMetricsDto
    {
        public int PendingCount { get; set; }
        public int FailedCount { get; set; }
        public int DeliveredCount { get; set; }
        public int TotalAttempts { get; set; }
        public decimal SuccessRate { get; set; }
    }

    /// <summary>
    /// Per-channel delivery metrics.
    /// </summary>
    public class ChannelDeliveryMetricsDto
    {
        public int ChannelTypeId { get; set; }
        public string ChannelName { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingCount { get; set; }
        public decimal SuccessRate { get; set; }
    }

    /// <summary>
    /// Recent delivery attempt for the live feed.
    /// </summary>
    public class RecentDeliveryAttemptDto
    {
        public long Id { get; set; }
        public DateTime AttemptedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public int ChannelTypeId { get; set; }
        public int AttemptNumber { get; set; }
        public string RecipientSummary { get; set; } = string.Empty;
        public string ErrorMessage { get; set; }
        public string Response { get; set; }

        //
        // For drill-down detail
        //
        public int IncidentId { get; set; }
        public string IncidentTitle { get; set; } = string.Empty;
        public Guid? UserObjectGuid { get; set; }
    }

    /// <summary>
    /// Recent webhook callback attempt.
    /// </summary>
    public class RecentWebhookAttemptDto
    {
        public long Id { get; set; }
        public DateTime AttemptedAt { get; set; }
        public bool Success { get; set; }
        public int? HttpStatusCode { get; set; }
        public string TargetUrl { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public string ErrorMessage { get; set; }

        //
        // For drill-down detail
        //
        public int IncidentId { get; set; }
        public string IncidentTitle { get; set; } = string.Empty;
        public string IntegrationName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Full webhook attempt detail for drill-down modal.
    /// </summary>
    public class WebhookAttemptDetailDto : RecentWebhookAttemptDto
    {
        public string PayloadJson { get; set; }
        public string ResponseBody { get; set; }
    }

    /// <summary>
    /// Pending notification in the queue.
    /// </summary>
    public class PendingNotificationDto
    {
        public long NotificationId { get; set; }
        public int IncidentId { get; set; }
        public string IncidentTitle { get; set; } = string.Empty;
        public Guid UserObjectGuid { get; set; }
        public string UserDisplayName { get; set; } = string.Empty;
        public DateTime FirstNotifiedAt { get; set; }
        public int PendingChannels { get; set; }
    }

    /// <summary>
    /// Full delivery attempt detail for drill-down.
    /// </summary>
    public class DeliveryAttemptDetailDto : RecentDeliveryAttemptDto
    {
        public string UserDisplayName { get; set; }
        public string UserEmail { get; set; }
        public string SeverityName { get; set; }
        public string IncidentDescription { get; set; }
    }
}
