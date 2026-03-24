using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Foundation TURN Server Provider Interface — abstracts STUN/TURN server configuration.
    /// 
    /// Different deployments may use different TURN servers:
    ///   - Self-hosted Coturn
    ///   - Twilio TURN
    ///   - Xirsys
    ///   - Azure Communication Services (manages its own)
    /// 
    /// This interface lets each deployment configure their preferred TURN infrastructure
    /// independently of the call provider.
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 4 (Calling), March 2026.
    /// 
    /// </summary>
    public interface ITurnServerProvider
    {
        /// <summary>
        /// Gets the list of ICE servers (STUN and TURN) for WebRTC connections.
        /// Credentials may be time-limited and rotated.
        /// </summary>
        Task<List<IceServer>> GetIceServersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Whether this provider is enabled and configured.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Validates the TURN server configuration.
        /// </summary>
        bool ValidateConfiguration();
    }


    /// <summary>
    /// Represents a STUN or TURN server for WebRTC ICE negotiation.
    /// Maps directly to the RTCIceServer interface in WebRTC.
    /// </summary>
    public class IceServer
    {
        /// <summary>
        /// Server URLs (e.g., "stun:stun.l.google.com:19302", "turn:turn.example.com:3478").
        /// </summary>
        public List<string> Urls { get; set; }

        /// <summary>
        /// Username for TURN authentication (not needed for STUN).
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Credential for TURN authentication.
        /// </summary>
        public string Credential { get; set; }

        /// <summary>
        /// Credential type — typically "password".
        /// </summary>
        public string CredentialType { get; set; } = "password";
    }
}
