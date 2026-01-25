using System;

namespace Foundation.Telemetry
{
    /// <summary>
    /// Configuration for the Telemetry collection system.
    /// Loaded from appsettings.json under the "Telemetry" section.
    /// </summary>
    public class TelemetryConfiguration
    {
        /// <summary>
        /// Whether telemetry collection is enabled. Default: true
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// How often to collect telemetry data, in minutes. Default: 1
        /// </summary>
        public int CollectionIntervalMinutes { get; set; } = 1;

        /// <summary>
        /// How many days to retain telemetry data before purging. Default: 90
        /// </summary>
        public int RetentionDays { get; set; } = 90;

        /// <summary>
        /// Whether to collect error events from the Auditor. Default: true
        /// </summary>
        public bool CollectAuditErrors { get; set; } = true;

        /// <summary>
        /// Whether to collect error entries from log files. Default: true
        /// </summary>
        public bool CollectLogErrors { get; set; } = true;

        /// <summary>
        /// UTC time of day to run the data purge job. Default: 03:00:00 (3 AM UTC)
        /// </summary>
        public TimeSpan PurgeRunTimeUtc { get; set; } = TimeSpan.FromHours(3);
    }

    /// <summary>
    /// Configuration for an application to monitor.
    /// These entries seed the TelemetryApplication table on startup.
    /// </summary>
    public class TelemetryApplicationConfiguration
    {
        /// <summary>
        /// Display name for the application
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Base URL of the application (e.g., "https://localhost:5001")
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Whether this is the local Foundation instance (self-monitoring)
        /// </summary>
        public bool IsSelf { get; set; }
    }
}
