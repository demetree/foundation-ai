// ============================================================================
//
// TurnServerConfiguration.cs — Configuration for the in-process TURN server.
//
// Extends TurnServerOptions with server-specific settings like listen address,
// port ranges, allocation quotas, and lifetimes.
//
// ============================================================================

namespace Foundation.Networking.Coturn.Configuration
{
    /// <summary>
    ///
    /// Configuration for the in-process TURN server.
    ///
    /// Typically bound from appsettings.json section "TurnServer".
    ///
    /// </summary>
    public class TurnServerConfiguration
    {
        //
        // ── Listener ─────────────────────────────────────────────────────
        //

        /// <summary>
        /// The address to listen on.  Default "0.0.0.0" binds all interfaces.
        /// </summary>
        public string ListenAddress { get; set; } = "0.0.0.0";

        /// <summary>
        /// The UDP/TCP port to listen on.  Default 3478 per RFC 5766.
        /// </summary>
        public int ListenPort { get; set; } = 3478;

        //
        // ── Relay ────────────────────────────────────────────────────────
        //

        /// <summary>
        /// The address to use for relay sockets.  Typically the server's public IP.
        /// Default "0.0.0.0" binds all interfaces.
        /// </summary>
        public string RelayAddress { get; set; } = "0.0.0.0";

        /// <summary>
        /// The external/public IP address to report in XOR-RELAYED-ADDRESS.
        /// Must be set to the server's public IP for NAT traversal to work.
        /// </summary>
        public string ExternalAddress { get; set; } = "0.0.0.0";

        /// <summary>
        /// Minimum relay port number (inclusive).
        /// </summary>
        public int RelayPortMin { get; set; } = 49152;

        /// <summary>
        /// Maximum relay port number (inclusive).
        /// </summary>
        public int RelayPortMax { get; set; } = 65535;

        //
        // ── Quotas ───────────────────────────────────────────────────────
        //

        /// <summary>
        /// Maximum allocations per authenticated username.  0 = unlimited.
        /// </summary>
        public int MaxAllocationsPerUser { get; set; } = 10;

        //
        // ── Lifetimes ────────────────────────────────────────────────────
        //

        /// <summary>
        /// Default allocation lifetime in seconds if the client doesn't specify one.
        /// </summary>
        public int DefaultLifetime { get; set; } = 600;

        /// <summary>
        /// Maximum allocation lifetime the server will grant, in seconds.
        /// </summary>
        public int MaxLifetime { get; set; } = 3600;

        //
        // ── Authentication ───────────────────────────────────────────────
        //

        /// <summary>
        /// The TURN realm.
        /// </summary>
        public string Realm { get; set; } = "turn.foundation.local";

        /// <summary>
        /// The shared secret for TURN REST API credential validation.
        /// Must match what CoturnTurnServerProvider uses to generate credentials.
        /// </summary>
        public string SharedSecret { get; set; } = string.Empty;

        /// <summary>
        /// Nonce lifetime in seconds.  After this, the nonce is considered stale
        /// and the client must obtain a new one.
        /// </summary>
        public int NonceLifetimeSeconds { get; set; } = 3600;

        //
        // ── Cleanup ──────────────────────────────────────────────────────
        //

        /// <summary>
        /// How often to run the expiry cleanup sweep, in seconds.
        /// </summary>
        public int CleanupIntervalSeconds { get; set; } = 30;

        //
        // ── Software Tag ─────────────────────────────────────────────────
        //

        /// <summary>
        /// The SOFTWARE attribute value included in responses.
        /// </summary>
        public string Software { get; set; } = "Foundation.Networking.Coturn/1.0";

        //
        // ── TCP Transport ────────────────────────────────────────────────
        //

        /// <summary>
        /// Whether to enable TCP transport alongside UDP.
        /// </summary>
        public bool TcpEnabled { get; set; } = true;

        /// <summary>
        /// TCP listen port.  Default 3478 (same as UDP) per RFC 5766.
        /// </summary>
        public int TcpListenPort { get; set; } = 3478;

        //
        // ── TLS Transport ────────────────────────────────────────────────
        //

        /// <summary>
        /// Whether to enable TLS transport (STUN/TURN-over-TLS).
        /// </summary>
        public bool TlsEnabled { get; set; } = false;

        /// <summary>
        /// TLS listen port.  Default 5349 per RFC 5766.
        /// </summary>
        public int TlsListenPort { get; set; } = 5349;

        /// <summary>
        /// Path to the TLS certificate file (PFX/PKCS#12 format).
        /// </summary>
        public string TlsCertificatePath { get; set; } = string.Empty;

        /// <summary>
        /// Password for the TLS certificate file (if encrypted).
        /// </summary>
        public string TlsCertificatePassword { get; set; } = string.Empty;
    }
}
