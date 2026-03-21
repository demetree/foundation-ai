// ============================================================================
//
// StunAttributeType.cs — STUN and TURN attribute type constants.
//
// STUN messages carry a list of TLV (Type-Length-Value) attributes after the
// 20-byte header.  The type field is a 16-bit unsigned integer.  Attribute
// types 0x0000–0x7FFF are comprehension-required; 0x8000–0xFFFF are
// comprehension-optional.
//
// ============================================================================

namespace Foundation.Networking.Coturn.Protocol
{
    /// <summary>
    ///
    /// STUN and TURN attribute type constants per RFC 5389 and RFC 5766.
    ///
    /// </summary>
    public static class StunAttributeType
    {
        //
        // ── Comprehension-Required Attributes (RFC 5389) ─────────────────────
        //

        public const ushort MAPPED_ADDRESS = 0x0001;
        public const ushort RESPONSE_ADDRESS = 0x0002;     // Deprecated (RFC 3489)
        public const ushort CHANGE_REQUEST = 0x0003;        // Deprecated (RFC 3489)
        public const ushort SOURCE_ADDRESS = 0x0004;        // Deprecated (RFC 3489)
        public const ushort CHANGED_ADDRESS = 0x0005;       // Deprecated (RFC 3489)
        public const ushort USERNAME = 0x0006;
        public const ushort PASSWORD = 0x0007;              // Deprecated (RFC 3489)
        public const ushort MESSAGE_INTEGRITY = 0x0008;
        public const ushort ERROR_CODE = 0x0009;
        public const ushort UNKNOWN_ATTRIBUTES = 0x000A;
        public const ushort REFLECTED_FROM = 0x000B;        // Deprecated (RFC 3489)
        public const ushort REALM = 0x0014;
        public const ushort NONCE = 0x0015;
        public const ushort XOR_MAPPED_ADDRESS = 0x0020;

        //
        // ── Comprehension-Optional Attributes (RFC 5389) ─────────────────────
        //

        public const ushort SOFTWARE = 0x8022;
        public const ushort ALTERNATE_SERVER = 0x8023;
        public const ushort FINGERPRINT = 0x8028;

        //
        // ── TURN Attributes (RFC 5766) ───────────────────────────────────────
        //

        public const ushort CHANNEL_NUMBER = 0x000C;
        public const ushort LIFETIME = 0x000D;
        public const ushort XOR_PEER_ADDRESS = 0x0012;
        public const ushort DATA = 0x0013;
        public const ushort XOR_RELAYED_ADDRESS = 0x0016;
        public const ushort EVEN_PORT = 0x0018;
        public const ushort REQUESTED_TRANSPORT = 0x0019;
        public const ushort DONT_FRAGMENT = 0x001A;
        public const ushort RESERVATION_TOKEN = 0x0022;

        //
        // ── Additional Attributes (RFC 5780, RFC 6156, etc.) ─────────────────
        //

        public const ushort PADDING = 0x0026;
        public const ushort RESPONSE_PORT = 0x0027;
        public const ushort CONNECTION_ID = 0x002A;          // RFC 6062
        public const ushort REQUESTED_ADDRESS_FAMILY = 0x0017; // RFC 6156

        //
        // ── ICE-related Attributes ───────────────────────────────────────────
        //

        public const ushort PRIORITY = 0x0024;
        public const ushort USE_CANDIDATE = 0x0025;
        public const ushort ICE_CONTROLLED = 0x8029;
        public const ushort ICE_CONTROLLING = 0x802A;

        //
        // ── MESSAGE-INTEGRITY-SHA256 (RFC 8489) ──────────────────────────────
        //

        public const ushort MESSAGE_INTEGRITY_SHA256 = 0x001C;
        public const ushort PASSWORD_ALGORITHM = 0x001D;
        public const ushort PASSWORD_ALGORITHMS = 0x8002;
        public const ushort ALTERNATE_DOMAIN = 0x8003;


        /// <summary>
        /// Returns true if this attribute type is comprehension-required.
        /// Comprehension-required attributes have type values 0x0000–0x7FFF.
        /// </summary>
        public static bool IsComprehensionRequired(ushort attributeType)
        {
            return attributeType < 0x8000;
        }
    }
}
