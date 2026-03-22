// ============================================================================
//
// BeaconConfiguration.cs — Configuration for the Beacon DNS/discovery system.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;

namespace Foundation.Networking.Beacon.Configuration
{
    /// <summary>
    ///
    /// Configuration for the Beacon DNS and service discovery system.
    ///
    /// </summary>
    public class BeaconConfiguration
    {
        //
        // ── DNS Resolver ─────────────────────────────────────────────────
        //

        /// <summary>
        /// DNS query timeout in milliseconds.
        /// </summary>
        public int DnsTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Custom DNS servers to query (if empty, uses system defaults).
        /// </summary>
        public List<string> DnsServers { get; set; } = new List<string>();

        //
        // ── DNS Record Management ────────────────────────────────────────
        //

        /// <summary>
        /// Internal DNS zone entries (for serving internal names).
        /// </summary>
        public List<DnsZoneEntry> ZoneEntries { get; set; } = new List<DnsZoneEntry>();

        //
        // ── Service Discovery ────────────────────────────────────────────
        //

        /// <summary>
        /// Stale service timeout in seconds.
        /// </summary>
        public int DiscoveryStaleTimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Whether to run periodic health checks on discovered services.
        /// </summary>
        public bool HealthCheckEnabled { get; set; } = true;

        /// <summary>
        /// Health check interval in seconds.
        /// </summary>
        public int HealthCheckIntervalSeconds { get; set; } = 30;
    }


    /// <summary>
    /// A DNS zone entry for internal name resolution.
    /// </summary>
    public class DnsZoneEntry
    {
        /// <summary>
        /// Hostname (e.g., "scheduler.foundation.local").
        /// </summary>
        public string Hostname { get; set; } = string.Empty;

        /// <summary>
        /// Record type: "A", "AAAA", "CNAME", "TXT", "MX", "SRV".
        /// </summary>
        public string RecordType { get; set; } = "A";

        /// <summary>
        /// Record value (IP address, CNAME target, etc.).
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// TTL in seconds.
        /// </summary>
        public int TtlSeconds { get; set; } = 300;

        /// <summary>
        /// Priority (for MX/SRV records).
        /// </summary>
        public int Priority { get; set; } = 10;
    }
}
