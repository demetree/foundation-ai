// ============================================================================
//
// NetworkingServiceInfo.cs — Standard DTO for networking service status.
//
// Returned by INetworkingServiceStatus.GetStatusAsync().
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;

namespace Foundation.Networking.Common
{
    /// <summary>
    ///
    /// Status information returned by a networking service.
    ///
    /// </summary>
    public class NetworkingServiceInfo
    {
        /// <summary>
        /// Service display name.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Service version string.
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Whether the service is healthy / operational.
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// How long the service has been running.
        /// </summary>
        public TimeSpan? Uptime { get; set; }

        /// <summary>
        /// Human-readable status message (e.g., "Running", "Degraded", "Stopped").
        /// </summary>
        public string StatusMessage { get; set; } = "Unknown";

        /// <summary>
        /// Arbitrary key-value metrics specific to the service.
        /// Examples:
        ///   DeepSpace: { "TotalPuts": "42", "ProviderCount": "2" }
        ///   Coturn:    { "ActiveAllocations": "15" }
        /// </summary>
        public Dictionary<string, string> Metrics { get; set; } = new Dictionary<string, string>();
    }
}
