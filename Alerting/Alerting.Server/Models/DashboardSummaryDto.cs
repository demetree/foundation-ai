//
// Dashboard Summary DTOs
//
// Data transfer objects for the Alerting Command Center dashboard.
//
using System;
using System.Collections.Generic;

namespace Alerting.Server.Models
{
    /// <summary>
    /// Operational status indicator for the dashboard
    /// </summary>
    public enum OperationalStatus
    {
        Healthy = 0,    // 0 active incidents
        Degraded = 1,   // 1-4 active incidents
        Critical = 2    // 5+ active incidents
    }

    /// <summary>
    /// Main dashboard summary response DTO
    /// </summary>
    public class DashboardSummaryDto
    {
        public OperationalStatus Status { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public IncidentMetricsDto IncidentMetrics { get; set; } = new();
        public List<OnCallScheduleSummaryDto> OnCallSummary { get; set; } = new();
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
        public ConfigurationCountsDto ConfigCounts { get; set; } = new();
        public PerformanceMetricsDto Performance { get; set; } = new();
        public ConfigurationHealthDto ConfigurationHealth { get; set; } = new();
    }

    /// <summary>
    /// Incident counts and metrics
    /// </summary>
    public class IncidentMetricsDto
    {
        // Status counts
        public int ActiveCount { get; set; }
        public int TriggeredCount { get; set; }
        public int AcknowledgedCount { get; set; }
        public int ResolvedTodayCount { get; set; }

        // Severity breakdown (active only)
        public int CriticalCount { get; set; }
        public int HighCount { get; set; }
        public int MediumCount { get; set; }
        public int LowCount { get; set; }
        public int InfoCount { get; set; }
    }

    /// <summary>
    /// On-call schedule summary with current responders
    /// </summary>
    public class OnCallScheduleSummaryDto
    {
        public int ScheduleId { get; set; }
        public Guid ScheduleObjectGuid { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public string? Timezone { get; set; }
        public List<OnCallUserDto> OnCallUsers { get; set; } = new();
    }

    /// <summary>
    /// On-call user details
    /// </summary>
    public class OnCallUserDto
    {
        public Guid UserObjectGuid { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? LayerName { get; set; }
    }

    /// <summary>
    /// Recent activity item (timeline event)
    /// </summary>
    public class RecentActivityDto
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int IncidentId { get; set; }
        public string IncidentTitle { get; set; } = string.Empty;
        public string? ActorName { get; set; }
        public string SeverityName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Configuration entity counts
    /// </summary>
    public class ConfigurationCountsDto
    {
        public int ServicesCount { get; set; }
        public int IntegrationsCount { get; set; }
        public int SchedulesCount { get; set; }
        public int PoliciesCount { get; set; }
    }

    /// <summary>
    /// Performance metrics (MTTA, MTTR)
    /// </summary>
    public class PerformanceMetricsDto
    {
        /// <summary>
        /// Mean Time to Acknowledge (minutes) - last 7 days
        /// </summary>
        public decimal? MttaMinutes { get; set; }

        /// <summary>
        /// Mean Time to Resolve (minutes) - last 7 days
        /// </summary>
        public decimal? MttrMinutes { get; set; }

        /// <summary>
        /// MTTA trend: "improving", "worsening", "stable"
        /// </summary>
        public string MttaTrend { get; set; } = "stable";

        /// <summary>
        /// MTTR trend: "improving", "worsening", "stable"
        /// </summary>
        public string MttrTrend { get; set; } = "stable";

        /// <summary>
        /// Incidents resolved in last 7 days (for trend calculation)
        /// </summary>
        public int IncidentsResolvedLast7Days { get; set; }
    }

    /// <summary>
    /// Configuration health summary for alerting chain validation
    /// </summary>
    public class ConfigurationHealthDto
    {
        /// <summary>
        /// Overall health status: Healthy, Warning, Error
        /// </summary>
        public string OverallStatus { get; set; } = "Healthy";

        /// <summary>
        /// Integrations with complete configuration chain
        /// </summary>
        public int FullyConfiguredCount { get; set; }

        /// <summary>
        /// Integrations with partial configuration (some gaps)
        /// </summary>
        public int PartiallyConfiguredCount { get; set; }

        /// <summary>
        /// Integrations with no service linked
        /// </summary>
        public int UnconfiguredCount { get; set; }

        /// <summary>
        /// Detailed list of configuration issues
        /// </summary>
        public List<ConfigurationIssueDto> Issues { get; set; } = new();
    }

    /// <summary>
    /// Individual configuration issue
    /// </summary>
    public class ConfigurationIssueDto
    {
        /// <summary>
        /// Type of entity: Integration, Service, EscalationPolicy, Schedule
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Entity database ID
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Entity display name
        /// </summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Issue severity: Warning, Error
        /// </summary>
        public string Severity { get; set; } = "Warning";

        /// <summary>
        /// Human-readable issue description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Frontend route to fix the issue
        /// </summary>
        public string QuickFixRoute { get; set; } = string.Empty;
    }
}

