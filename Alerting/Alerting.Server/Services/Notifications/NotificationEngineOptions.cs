//
// Notification Engine Options
//
// Configuration options for the notification engine workers and logging.
// AI-assisted development - February 2026
//
namespace Alerting.Server.Services.Notifications
{
    /// <summary>
    /// Configuration options for the notification engine.
    /// Bound from "NotificationEngine" section in appsettings.json.
    /// </summary>
    public class NotificationEngineOptions
    {
        /// <summary>
        /// Configuration section name.
        /// </summary>
        public const string SectionName = "NotificationEngine";

        /// <summary>
        /// Interval in seconds for the escalation worker to check for pending escalations.
        /// Default: 30 seconds.
        /// </summary>
        public int EscalationWorkerIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Interval in seconds for the retry worker to check for failed deliveries.
        /// Default: 60 seconds.
        /// </summary>
        public int RetryWorkerIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Maximum number of retry attempts for failed notifications.
        /// Default: 3.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Backoff intervals in minutes between retry attempts.
        /// Default: [1, 5, 15] (1 min, 5 min, 15 min).
        /// </summary>
        public int[] RetryBackoffMinutes { get; set; } = { 1, 5, 15 };

        /// <summary>
        /// Log level for the notification engine logger.
        /// Valid values: "Debug", "Information", "Warning", "Error".
        /// Default: "Debug" for verbose logging.
        /// </summary>
        public string LogLevel { get; set; } = "Debug";

        /// <summary>
        /// Whether to disable throttling on the notification logger.
        /// Default: true to ensure all events are captured.
        /// </summary>
        public bool DisableLogThrottling { get; set; } = true;
    }
}
