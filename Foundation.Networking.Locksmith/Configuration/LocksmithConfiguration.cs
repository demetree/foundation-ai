// ============================================================================
//
// LocksmithConfiguration.cs — Configuration for Locksmith certificate management.
//
// Defines endpoints to monitor for certificate expiry, alert thresholds,
// and optional ACME (Let's Encrypt) auto-provisioning settings.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;

namespace Foundation.Networking.Locksmith.Configuration
{
    /// <summary>
    ///
    /// Configuration for the Locksmith certificate management system.
    ///
    /// Typically bound from appsettings.json section "Locksmith".
    ///
    /// </summary>
    public class LocksmithConfiguration
    {
        //
        // ── Inspector Settings ───────────────────────────────────────────
        //

        /// <summary>
        /// Timeout for TLS handshake when inspecting a certificate, in milliseconds.
        /// </summary>
        public int InspectTimeoutMs { get; set; } = 10000;

        //
        // ── Monitor Settings ─────────────────────────────────────────────
        //

        /// <summary>
        /// Certificate monitor configuration.
        /// </summary>
        public CertificateMonitorConfiguration Monitor { get; set; } = new CertificateMonitorConfiguration();

        //
        // ── ACME Settings ────────────────────────────────────────────────
        //

        /// <summary>
        /// ACME (Let's Encrypt) auto-provisioning configuration.
        /// </summary>
        public AcmeConfiguration Acme { get; set; } = new AcmeConfiguration();
    }


    /// <summary>
    ///
    /// Configuration for the background certificate monitor.
    ///
    /// </summary>
    public class CertificateMonitorConfiguration
    {
        /// <summary>
        /// Whether the certificate monitor is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// How often to check certificates, in hours.
        /// </summary>
        public int IntervalHours { get; set; } = 6;

        /// <summary>
        /// Number of days before expiry to raise a warning.
        /// </summary>
        public int WarningDays { get; set; } = 30;

        /// <summary>
        /// Number of days before expiry to raise a critical alert.
        /// </summary>
        public int CriticalDays { get; set; } = 7;

        /// <summary>
        /// Endpoints to monitor for certificate expiry.
        /// </summary>
        public List<CertificateEndpoint> Endpoints { get; set; } = new List<CertificateEndpoint>();
    }


    /// <summary>
    ///
    /// A single TLS endpoint to monitor.
    ///
    /// </summary>
    public class CertificateEndpoint
    {
        /// <summary>
        /// Hostname to connect to.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Port number (default 443 for HTTPS).
        /// </summary>
        public int Port { get; set; } = 443;

        /// <summary>
        /// Human-readable label for this endpoint.
        /// </summary>
        public string Label { get; set; } = string.Empty;
    }


    /// <summary>
    ///
    /// Configuration for ACME (Let's Encrypt / ZeroSSL) auto-provisioning.
    ///
    /// </summary>
    public class AcmeConfiguration
    {
        /// <summary>
        /// Whether ACME auto-provisioning is enabled.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// ACME directory URL (Let's Encrypt production or staging).
        /// </summary>
        public string DirectoryUrl { get; set; } = "https://acme-v02.api.letsencrypt.org/directory";

        /// <summary>
        /// Contact email for the ACME account.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Path to store provisioned certificates.
        /// </summary>
        public string CertificateStorePath { get; set; } = "./certs";

        /// <summary>
        /// Days before expiry to trigger auto-renewal.
        /// </summary>
        public int RenewalDays { get; set; } = 30;
    }
}
