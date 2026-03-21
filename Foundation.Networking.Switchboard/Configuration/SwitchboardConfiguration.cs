// ============================================================================
//
// SwitchboardConfiguration.cs — Configuration for the Switchboard load balancer.
//
// Defines backend pools, routing strategy, health check settings,
// and service registry options.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;

namespace Foundation.Networking.Switchboard.Configuration
{
    /// <summary>
    ///
    /// Configuration for the Switchboard load balancer.
    ///
    /// </summary>
    public class SwitchboardConfiguration
    {
        /// <summary>
        /// Routing strategy: "RoundRobin", "LeastConnections", "Weighted", "IpHash".
        /// </summary>
        public string Strategy { get; set; } = "RoundRobin";

        /// <summary>
        /// Whether sticky sessions are enabled (uses IP hash for session affinity).
        /// </summary>
        public bool StickySessionsEnabled { get; set; } = false;

        /// <summary>
        /// Health check interval in seconds.
        /// </summary>
        public int HealthCheckIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Health check timeout in milliseconds.
        /// </summary>
        public int HealthCheckTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Number of consecutive failures before marking a backend as unhealthy.
        /// </summary>
        public int UnhealthyThreshold { get; set; } = 3;

        /// <summary>
        /// Number of consecutive successes before marking a backend as healthy again.
        /// </summary>
        public int HealthyThreshold { get; set; } = 2;

        /// <summary>
        /// Backend server definitions.
        /// </summary>
        public List<SwitchboardBackend> Backends { get; set; } = new List<SwitchboardBackend>();
    }


    /// <summary>
    /// A single backend server configuration.
    /// </summary>
    public class SwitchboardBackend
    {
        /// <summary>
        /// Unique identifier for this backend.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Backend URL (e.g., "http://localhost:10100").
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Weight for weighted routing (higher = more traffic).
        /// </summary>
        public int Weight { get; set; } = 1;

        /// <summary>
        /// Whether this backend is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Health check path (e.g., "/api/health").
        /// </summary>
        public string HealthCheckPath { get; set; } = "/api/health";

        /// <summary>
        /// Maximum concurrent connections to this backend.
        /// </summary>
        public int MaxConnections { get; set; } = 100;
    }
}
