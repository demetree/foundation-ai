//
// Notification Audit DTOs
//
// Data transfer objects for the Notification Audit Console.
// AI-assisted development - February 2026
//
using System;
using System.Collections.Generic;

namespace Alerting.Server.Models
{
    /// <summary>
    /// Metrics summary for the notification audit dashboard.
    /// </summary>
    public class NotificationAuditMetricsDto
    {
        /// <summary>
        /// Total successful deliveries in the last 24 hours.
        /// </summary>
        public int SentToday { get; set; }

        /// <summary>
        /// Total failed deliveries in the last 24 hours.
        /// </summary>
        public int FailedToday { get; set; }

        /// <summary>
        /// Total pending deliveries.
        /// </summary>
        public int PendingNow { get; set; }

        /// <summary>
        /// Rolling 7-day success rate as a percentage (0-100).
        /// </summary>
        public double SuccessRate7Day { get; set; }

        /// <summary>
        /// Average delivery latency in milliseconds (from escalation trigger to send).
        /// </summary>
        public double AvgLatencyMs { get; set; }

        /// <summary>
        /// Channel distribution breakdown.
        /// </summary>
        public List<ChannelBreakdownDto> ChannelBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Channel breakdown for pie/donut chart.
    /// </summary>
    public class ChannelBreakdownDto
    {
        public int ChannelTypeId { get; set; }
        public string ChannelName { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// Summary row for the notification audit list.
    /// </summary>
    public class AuditDeliverySummaryDto
    {
        public int Id { get; set; }
        public Guid ObjectGuid { get; set; }
        public DateTime AttemptedAt { get; set; }
        
        // Channel info
        public int ChannelTypeId { get; set; }
        public string ChannelName { get; set; }
        
        // Status
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        
        // Recipient
        public string RecipientAddress { get; set; }
        public string RecipientDisplay { get; set; } // Masked for privacy in list
        
        // Incident context
        public int? IncidentId { get; set; }
        public string IncidentKey { get; set; }
        public string IncidentTitle { get; set; }
        
        // User context
        public Guid? UserObjectGuid { get; set; }
        public string UserDisplayName { get; set; }
    }

    /// <summary>
    /// Full detail for a single delivery attempt including content.
    /// </summary>
    public class AuditDeliveryDetailDto : AuditDeliverySummaryDto
    {
        // Content fields for auditing
        public string Subject { get; set; }
        public string BodyContent { get; set; }
        
        // Provider response
        public string Response { get; set; }
        public string ExternalMessageId { get; set; }
        
        // Retry info
        public int AttemptNumber { get; set; }
        public int TotalAttempts { get; set; }
        
        // Timestamps
        public DateTime? NotificationCreatedAt { get; set; }
        public DateTime? IncidentCreatedAt { get; set; }
    }

    /// <summary>
    /// Query parameters for filtering delivery attempts.
    /// </summary>
    public class DeliveryAttemptQueryParams
    {
        public int? ChannelTypeId { get; set; }
        public string Status { get; set; } // Pending, Sent, Failed
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string SearchQuery { get; set; } // Search in recipient, incident key, subject
        public int? IncidentId { get; set; }
        public Guid? UserObjectGuid { get; set; }
        
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        
        // Sorting
        public string SortBy { get; set; } = "attemptedAt";
        public bool SortDescending { get; set; } = true;
    }

    /// <summary>
    /// Paginated result wrapper.
    /// </summary>
    public class DeliveryAttemptListResult
    {
        public List<AuditDeliverySummaryDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
