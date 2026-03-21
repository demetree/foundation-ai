// ============================================================================
//
// CertificateInfo.cs — Certificate inspection result models.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Foundation.Networking.Locksmith.Services
{
    /// <summary>
    /// Result of inspecting a TLS certificate on a host.
    /// </summary>
    public class CertificateInfo
    {
        /// <summary>
        /// The host that was inspected.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// The port that was inspected.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Whether the inspection was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if the inspection failed.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        //
        // ── Certificate Details ──────────────────────────────────────────
        //

        /// <summary>
        /// Certificate subject (e.g., "CN=example.com").
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Certificate issuer (e.g., "CN=Let's Encrypt Authority X3").
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// The common name extracted from the subject.
        /// </summary>
        public string CommonName { get; set; } = string.Empty;

        /// <summary>
        /// Subject Alternative Names (SANs) — all hostnames the cert is valid for.
        /// </summary>
        public List<string> SubjectAlternativeNames { get; set; } = new List<string>();

        /// <summary>
        /// Certificate serial number.
        /// </summary>
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// Certificate thumbprint (SHA-1 hash).
        /// </summary>
        public string Thumbprint { get; set; } = string.Empty;

        /// <summary>
        /// Certificate signature algorithm.
        /// </summary>
        public string SignatureAlgorithm { get; set; } = string.Empty;

        /// <summary>
        /// Public key algorithm and key size.
        /// </summary>
        public string PublicKeyInfo { get; set; } = string.Empty;

        //
        // ── Validity ─────────────────────────────────────────────────────
        //

        /// <summary>
        /// Certificate validity start date (UTC).
        /// </summary>
        public DateTime NotBeforeUtc { get; set; }

        /// <summary>
        /// Certificate expiry date (UTC).
        /// </summary>
        public DateTime NotAfterUtc { get; set; }

        /// <summary>
        /// Number of days until the certificate expires.
        /// Negative if already expired.
        /// </summary>
        public int DaysUntilExpiry { get; set; }

        /// <summary>
        /// Whether the certificate has expired.
        /// </summary>
        public bool IsExpired { get; set; }

        //
        // ── Chain Validation ─────────────────────────────────────────────
        //

        /// <summary>
        /// Whether the full certificate chain is valid.
        /// </summary>
        public bool ChainIsValid { get; set; }

        /// <summary>
        /// Chain validation status messages (if any issues).
        /// </summary>
        public List<string> ChainStatusMessages { get; set; } = new List<string>();

        /// <summary>
        /// Number of certificates in the chain.
        /// </summary>
        public int ChainLength { get; set; }

        //
        // ── Metadata ─────────────────────────────────────────────────────
        //

        /// <summary>
        /// Timestamp of when the inspection was performed.
        /// </summary>
        public DateTime InspectedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// TLS protocol version negotiated.
        /// </summary>
        public string TlsVersion { get; set; } = string.Empty;
    }


    /// <summary>
    /// Certificate health status for a monitored endpoint.
    /// </summary>
    public enum CertificateHealthStatus
    {
        /// <summary>
        /// Certificate is valid and not expiring soon.
        /// </summary>
        Healthy,

        /// <summary>
        /// Certificate will expire within the warning threshold.
        /// </summary>
        Warning,

        /// <summary>
        /// Certificate will expire within the critical threshold.
        /// </summary>
        Critical,

        /// <summary>
        /// Certificate has expired.
        /// </summary>
        Expired,

        /// <summary>
        /// Certificate chain validation failed.
        /// </summary>
        ChainInvalid,

        /// <summary>
        /// Could not connect to the endpoint.
        /// </summary>
        Unreachable,

        /// <summary>
        /// Status unknown (not yet checked).
        /// </summary>
        Unknown
    }


    /// <summary>
    /// Summary of a monitored certificate endpoint's health.
    /// </summary>
    public class CertificateEndpointStatus
    {
        /// <summary>
        /// The endpoint label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The endpoint host.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// The endpoint port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Current health status.
        /// </summary>
        public CertificateHealthStatus Status { get; set; } = CertificateHealthStatus.Unknown;

        /// <summary>
        /// Days until the certificate expires.
        /// </summary>
        public int DaysUntilExpiry { get; set; }

        /// <summary>
        /// Certificate subject.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Certificate issuer.
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Certificate expiry date.
        /// </summary>
        public DateTime NotAfterUtc { get; set; }

        /// <summary>
        /// Last inspection timestamp.
        /// </summary>
        public DateTime LastCheckedUtc { get; set; }

        /// <summary>
        /// Error message if the endpoint is unreachable.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
