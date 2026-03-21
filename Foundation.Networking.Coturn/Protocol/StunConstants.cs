// ============================================================================
//
// StunConstants.cs — Core STUN/TURN protocol constants.
//
// Defines the fundamental constants used across the STUN and TURN protocol
// stack per RFC 5389 and RFC 5766.  These are the binary framing values that
// appear in every STUN message header and integrity check.
//
// ============================================================================

namespace Foundation.Networking.Coturn.Protocol
{
    /// <summary>
    ///
    /// Core STUN/TURN protocol constants from RFC 5389 and RFC 5766.
    ///
    /// </summary>
    public static class StunConstants
    {
        //
        // STUN message header layout
        //
        public const int HEADER_SIZE = 20;
        public const int TRANSACTION_ID_SIZE = 12;
        public const uint MAGIC_COOKIE = 0x2112A442;

        //
        // Fingerprint XOR value — the ASCII encoding of "STUN"
        //
        public const uint FINGERPRINT_XOR = 0x5354554E;

        //
        // Integrity and fingerprint sizes
        //
        public const int HMAC_SHA1_LENGTH = 20;
        public const int FINGERPRINT_LENGTH = 4;
        public const int CRC32_LENGTH = 4;

        //
        // Attribute header is always 4 bytes: 2 bytes type + 2 bytes length
        //
        public const int ATTRIBUTE_HEADER_SIZE = 4;

        //
        // STUN requires all attributes to be padded to 4-byte boundaries
        //
        public const int ATTRIBUTE_ALIGNMENT = 4;

        //
        // Maximum reasonable message size (coturn default)
        //
        public const int MAX_MESSAGE_SIZE = 65535;

        //
        // TURN default allocation lifetime in seconds (RFC 5766 §6.2)
        //
        public const int DEFAULT_LIFETIME_SECONDS = 600;

        //
        // Transport protocol numbers used in REQUESTED-TRANSPORT attribute
        //
        public const byte TRANSPORT_UDP = 17;
        public const byte TRANSPORT_TCP = 6;
    }
}
