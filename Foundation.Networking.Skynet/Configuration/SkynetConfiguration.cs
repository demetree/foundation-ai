// ============================================================================
//
// SkynetConfiguration.cs — Configuration for the Skynet edge proxy/firewall.
//
// Defines backend servers, firewall rules, rate limiting, and GeoIP settings.
// Bound from the "Skynet" section of appsettings.json.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;

namespace Foundation.Networking.Skynet.Configuration
{
    /// <summary>
    ///
    /// Configuration for the Skynet edge proxy and firewall system.
    ///
    /// </summary>
    public class SkynetConfiguration
    {
        //
        // ── Proxy Settings ───────────────────────────────────────────────
        //

        /// <summary>
        /// Whether the reverse proxy is enabled.
        /// </summary>
        public bool ProxyEnabled { get; set; } = true;

        /// <summary>
        /// Connection timeout to backend servers, in milliseconds.
        /// </summary>
        public int BackendTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Maximum concurrent connections per backend.
        /// </summary>
        public int MaxConnectionsPerBackend { get; set; } = 100;

        /// <summary>
        /// Backend server definitions.
        /// </summary>
        public List<BackendServer> Backends { get; set; } = new List<BackendServer>();

        //
        // ── Firewall Settings ────────────────────────────────────────────
        //

        /// <summary>
        /// Whether the firewall is enabled.
        /// </summary>
        public bool FirewallEnabled { get; set; } = true;

        /// <summary>
        /// Firewall rules (evaluated in order).
        /// </summary>
        public List<FirewallRuleConfig> Rules { get; set; } = new List<FirewallRuleConfig>();

        //
        // ── Rate Limiting ────────────────────────────────────────────────
        //

        /// <summary>
        /// Whether rate limiting is enabled.
        /// </summary>
        public bool RateLimitEnabled { get; set; } = true;

        /// <summary>
        /// Default rate limit — requests per window per IP.
        /// </summary>
        public int DefaultRateLimit { get; set; } = 100;

        /// <summary>
        /// Rate limit window size in seconds.
        /// </summary>
        public int RateLimitWindowSeconds { get; set; } = 60;

        //
        // ── Threat Logging ───────────────────────────────────────────────
        //

        /// <summary>
        /// Maximum number of threat log entries to keep in memory.
        /// </summary>
        public int ThreatLogMaxEntries { get; set; } = 10000;
    }


    /// <summary>
    /// Backend server configuration.
    /// </summary>
    public class BackendServer
    {
        /// <summary>
        /// Human-readable label for this backend.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Backend server URL (e.g., "http://localhost:10100").
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
    }


    /// <summary>
    /// Firewall rule configuration from appsettings.
    /// </summary>
    public class FirewallRuleConfig
    {
        /// <summary>
        /// Rule name for logging.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Rule action: "Allow" or "Deny".
        /// </summary>
        public string Action { get; set; } = "Deny";

        /// <summary>
        /// IP addresses or CIDR ranges this rule applies to.
        /// Empty means all IPs.
        /// </summary>
        public List<string> IpRanges { get; set; } = new List<string>();

        /// <summary>
        /// Country codes to match (for GeoIP rules).
        /// Empty means all countries.
        /// </summary>
        public List<string> CountryCodes { get; set; } = new List<string>();

        /// <summary>
        /// URL path patterns to match (supports wildcards).
        /// Empty means all paths.
        /// </summary>
        public List<string> Paths { get; set; } = new List<string>();

        /// <summary>
        /// Whether this rule is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Priority (lower number = higher priority).
        /// </summary>
        public int Priority { get; set; } = 100;
    }
}
