// ============================================================================
//
// WatchtowerConfiguration.cs — Configuration for Watchtower diagnostics.
//
// Defines endpoints to monitor, ping/scan timeouts, and latency monitor
// settings.  Bound from the "Watchtower" section of appsettings.json.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;

namespace Foundation.Networking.Watchtower.Configuration
{
    /// <summary>
    ///
    /// Configuration for the Watchtower network diagnostics system.
    ///
    /// Typically bound from appsettings.json section "Watchtower".
    ///
    /// </summary>
    public class WatchtowerConfiguration
    {
        //
        // ── Ping Settings ────────────────────────────────────────────────
        //

        /// <summary>
        /// Timeout for individual ICMP ping requests, in milliseconds.
        /// </summary>
        public int PingTimeoutMs { get; set; } = 3000;

        /// <summary>
        /// Default TTL (Time To Live) for ping requests.
        /// </summary>
        public int PingTtl { get; set; } = 128;

        /// <summary>
        /// Size of the ping payload buffer in bytes.
        /// </summary>
        public int PingBufferSize { get; set; } = 32;

        //
        // ── Port Scanner Settings ────────────────────────────────────────
        //

        /// <summary>
        /// Timeout for individual TCP port scan connection attempts, in milliseconds.
        /// </summary>
        public int PortScanTimeoutMs { get; set; } = 2000;

        /// <summary>
        /// Maximum number of concurrent port scan connections.
        /// </summary>
        public int PortScanMaxConcurrency { get; set; } = 50;

        //
        // ── Traceroute Settings ──────────────────────────────────────────
        //

        /// <summary>
        /// Maximum number of hops for traceroute.
        /// </summary>
        public int TracerouteMaxHops { get; set; } = 30;

        /// <summary>
        /// Timeout per hop in milliseconds.
        /// </summary>
        public int TracerouteTimeoutMs { get; set; } = 3000;

        //
        // ── Latency Monitor Settings ─────────────────────────────────────
        //

        /// <summary>
        /// Latency monitor configuration.
        /// </summary>
        public LatencyMonitorConfiguration LatencyMonitor { get; set; } = new LatencyMonitorConfiguration();
    }


    /// <summary>
    ///
    /// Configuration for the background latency monitor service.
    ///
    /// </summary>
    public class LatencyMonitorConfiguration
    {
        /// <summary>
        /// Whether the latency monitor background service is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// How often to ping all monitored endpoints, in seconds.
        /// </summary>
        public int IntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Maximum number of latency records to keep per endpoint.
        /// </summary>
        public int MaxHistoryPerEndpoint { get; set; } = 1440;

        /// <summary>
        /// Latency threshold in milliseconds — exceeding this triggers a warning.
        /// </summary>
        public int AlertThresholdMs { get; set; } = 500;

        /// <summary>
        /// List of endpoints to monitor.
        /// </summary>
        public List<MonitoredEndpoint> Endpoints { get; set; } = new List<MonitoredEndpoint>();
    }


    /// <summary>
    ///
    /// A single endpoint to monitor with the latency monitor.
    ///
    /// </summary>
    public class MonitoredEndpoint
    {
        /// <summary>
        /// Hostname or IP address to ping.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Expected open TCP ports (verified periodically alongside ping).
        /// </summary>
        public List<int> Ports { get; set; } = new List<int>();

        /// <summary>
        /// Human-readable label for this endpoint (e.g., "Scheduler", "Foundation").
        /// </summary>
        public string Label { get; set; } = string.Empty;
    }
}
