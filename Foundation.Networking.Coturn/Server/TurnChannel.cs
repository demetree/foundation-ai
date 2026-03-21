// ============================================================================
//
// TurnChannel.cs — TURN channel binding.
//
// A channel binding maps a 16-bit channel number (0x4000–0x7FFE) to a
// specific peer transport address.  Once bound, the client can send data
// using the more compact 4-byte ChannelData header instead of the full
// STUN Send Indication format.
//
// Channel bindings expire after 10 minutes per RFC 5766 §11 and must be
// refreshed by the client via ChannelBind requests.
//
// ============================================================================

using System;
using System.Net;

namespace Foundation.Networking.Coturn.Server
{
    /// <summary>
    ///
    /// Represents a TURN channel binding — a mapping from a channel number
    /// to a peer transport address for compact data relay.
    ///
    /// </summary>
    public class TurnChannel
    {
        //
        // Default channel binding lifetime: 10 minutes per RFC 5766 §11
        //
        public const int DEFAULT_LIFETIME_SECONDS = 600;

        //
        // Valid channel number range per RFC 5766 §11
        //
        public const ushort MIN_CHANNEL_NUMBER = 0x4000;
        public const ushort MAX_CHANNEL_NUMBER = 0x7FFE;


        //
        // The channel number assigned to this binding
        //
        public ushort ChannelNumber { get; set; }

        //
        // The peer transport address this channel is bound to
        //
        public IPEndPoint PeerEndPoint { get; set; }

        //
        // When this channel binding expires (UTC)
        //
        public DateTime ExpiresAtUtc { get; set; }


        public TurnChannel(ushort channelNumber, IPEndPoint peerEndPoint)
        {
            ChannelNumber = channelNumber;
            PeerEndPoint = peerEndPoint;
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(DEFAULT_LIFETIME_SECONDS);
        }


        /// <summary>
        /// Returns true if this channel binding has expired.
        /// </summary>
        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpiresAtUtc;
        }


        /// <summary>
        /// Refreshes the channel binding — resets the expiry to 10 minutes from now.
        /// </summary>
        public void Refresh()
        {
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(DEFAULT_LIFETIME_SECONDS);
        }


        /// <summary>
        /// Returns true if the given channel number is in the valid range.
        /// </summary>
        public static bool IsValidChannelNumber(ushort channelNumber)
        {
            return channelNumber >= MIN_CHANNEL_NUMBER && channelNumber <= MAX_CHANNEL_NUMBER;
        }
    }
}
