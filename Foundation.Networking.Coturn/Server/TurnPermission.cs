// ============================================================================
//
// TurnPermission.cs — TURN allocation permission.
//
// Each allocation can have zero or more permissions.  A permission consists
// of an IP address (no port) and a lifetime.  Permissions expire after 5
// minutes (300 seconds) per RFC 5766 §8 and must be refreshed by the client.
//
// Permissions gate which peers can send data to the client through the relay.
//
// ============================================================================

using System;
using System.Net;

namespace Foundation.Networking.Coturn.Server
{
    /// <summary>
    ///
    /// Represents a TURN permission — allows a specific peer IP to send data
    /// through the relay to the client.
    ///
    /// </summary>
    public class TurnPermission
    {
        //
        // Default permission lifetime: 5 minutes per RFC 5766 §8
        //
        public const int DEFAULT_LIFETIME_SECONDS = 300;


        //
        // The permitted peer IP address (port is not part of permission checks)
        //
        public IPAddress PeerAddress { get; set; }

        //
        // When this permission expires (UTC)
        //
        public DateTime ExpiresAtUtc { get; set; }


        public TurnPermission(IPAddress peerAddress)
        {
            PeerAddress = peerAddress;
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(DEFAULT_LIFETIME_SECONDS);
        }


        /// <summary>
        /// Returns true if this permission has expired.
        /// </summary>
        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpiresAtUtc;
        }


        /// <summary>
        /// Refreshes the permission — resets the expiry to 5 minutes from now.
        /// </summary>
        public void Refresh()
        {
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(DEFAULT_LIFETIME_SECONDS);
        }
    }
}
