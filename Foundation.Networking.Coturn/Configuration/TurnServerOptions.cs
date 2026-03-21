// ============================================================================
//
// TurnServerOptions.cs — Configuration for the TURN server provider.
//
// These options configure where the TURN server is and how to generate
// time-limited credentials for WebRTC clients.  Read from appsettings.json
// via the standard IOptions<T> pattern.
//
// ============================================================================

using System.Collections.Generic;

namespace Foundation.Networking.Coturn.Configuration
{
    /// <summary>
    ///
    /// Configuration options for the TURN server provider.
    ///
    /// Typically bound from appsettings.json section "TurnServer".
    ///
    /// Example appsettings.json:
    /// {
    ///   "TurnServer": {
    ///     "Enabled": true,
    ///     "StunUrls": ["stun:turn.example.com:3478"],
    ///     "TurnUrls": ["turn:turn.example.com:3478", "turns:turn.example.com:5349"],
    ///     "SharedSecret": "your-shared-secret-here",
    ///     "CredentialTtlSeconds": 86400,
    ///     "Realm": "example.com"
    ///   }
    /// }
    ///
    /// </summary>
    public class TurnServerOptions
    {
        //
        // Whether the TURN provider is enabled
        //
        public bool Enabled { get; set; } = false;

        //
        // STUN server URLs (e.g., "stun:turn.example.com:3478")
        //
        public List<string> StunUrls { get; set; } = new List<string>();

        //
        // TURN server URLs (e.g., "turn:turn.example.com:3478", "turns:turn.example.com:5349")
        //
        public List<string> TurnUrls { get; set; } = new List<string>();

        //
        // Shared secret for TURN REST API time-limited credential generation.
        // This must match the static-auth-secret configured on the coturn server.
        //
        public string SharedSecret { get; set; } = string.Empty;

        //
        // How long the generated credentials are valid, in seconds.
        // Default: 86400 (24 hours).  coturn validates this server-side.
        //
        public int CredentialTtlSeconds { get; set; } = 86400;

        //
        // The TURN realm.  Used in long-term credential generation.
        //
        public string Realm { get; set; } = string.Empty;
    }
}
