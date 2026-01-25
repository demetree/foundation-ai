//
// Application Metrics Provider Interface
//
// Enables applications to register custom business-domain metrics
// that are displayed in the System Health dashboard.
//
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foundation.Services
{
    /// <summary>
    /// Interface for providers that supply application-specific metrics
    /// for the System Health dashboard
    /// </summary>
    public interface IApplicationMetricsProvider
    {
        /// <summary>
        /// The application name these metrics belong to
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Gets the current application metrics
        /// </summary>
        Task<IEnumerable<ApplicationMetric>> GetMetricsAsync();
    }


    /// <summary>
    /// Represents a single application metric
    /// </summary>
    public class ApplicationMetric
    {
        /// <summary>
        /// Display name of the metric (e.g., "Active Jobs")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Current value as a string (e.g., "42", "85%", "Running")
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Health state of this metric
        /// </summary>
        public MetricState State { get; set; } = MetricState.Healthy;

        /// <summary>
        /// Data type for rendering (affects UI visualization)
        /// </summary>
        public MetricDataType DataType { get; set; } = MetricDataType.Number;

        /// <summary>
        /// Optional category for grouping related metrics
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Optional description for tooltip
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional icon name hint for UI
        /// </summary>
        public string Icon { get; set; }
    }


    /// <summary>
    /// Health state of a metric
    /// </summary>
    public enum MetricState
    {
        Healthy = 0,
        Warning = 1,
        Critical = 2,
        Unknown = 3
    }


    /// <summary>
    /// Data type of a metric (affects UI rendering)
    /// </summary>
    public enum MetricDataType
    {
        /// <summary>
        /// Plain numeric value (e.g., "42")
        /// </summary>
        Number = 0,

        /// <summary>
        /// Percentage value (e.g., "85" displayed as "85%")
        /// </summary>
        Percentage = 1,

        /// <summary>
        /// Text/status value (e.g., "Running", "Idle")
        /// </summary>
        Text = 2,

        /// <summary>
        /// Boolean on/off value
        /// </summary>
        Boolean = 3,

        /// <summary>
        /// Duration/time span value (e.g., "2h 15m")
        /// </summary>
        Duration = 4
    }


    /// <summary>
    /// Response model for application metrics endpoint
    /// </summary>
    public class ApplicationMetricsResponse
    {
        /// <summary>
        /// Metrics grouped by application
        /// </summary>
        public List<ApplicationMetricsGroup> Applications { get; set; } = new List<ApplicationMetricsGroup>();

        /// <summary>
        /// Error message if retrieval failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }


    /// <summary>
    /// Metrics for a single application
    /// </summary>
    public class ApplicationMetricsGroup
    {
        /// <summary>
        /// Application name
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// List of metrics for this application
        /// </summary>
        public List<ApplicationMetric> Metrics { get; set; } = new List<ApplicationMetric>();
    }
}
