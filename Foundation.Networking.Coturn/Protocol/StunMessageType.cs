// ============================================================================
//
// StunMessageType.cs — STUN/TURN message type definitions.
//
// A STUN message type is a 14-bit field composed of a method (12 bits) and
// a class (2 bits, spread across bits C0 and C1 per RFC 5389 §6).
//
// The encoding splits the method bits around the class bits:
//   Bits:  M11..M7  C1  M6..M4  C0  M3..M0
//
// This class provides both the raw method/class constants and pre-composed
// message type values for all STUN and TURN methods.
//
// ============================================================================

namespace Foundation.Networking.Coturn.Protocol
{
    /// <summary>
    ///
    /// STUN/TURN message type constants.
    ///
    /// Message type is the combination of a method and a class, encoded into
    /// a 14-bit value as defined by RFC 5389 §6.
    ///
    /// </summary>
    public static class StunMessageType
    {
        //
        // STUN message classes (2 bits, spread across C0 and C1 in the type field)
        //
        public const ushort CLASS_REQUEST = 0x0000;
        public const ushort CLASS_INDICATION = 0x0010;
        public const ushort CLASS_SUCCESS_RESPONSE = 0x0100;
        public const ushort CLASS_ERROR_RESPONSE = 0x0110;

        //
        // STUN method: Binding (RFC 5389)
        //
        public const ushort METHOD_BINDING = 0x0001;

        //
        // TURN methods (RFC 5766 + RFC 6062)
        //
        public const ushort METHOD_ALLOCATE = 0x0003;
        public const ushort METHOD_REFRESH = 0x0004;
        public const ushort METHOD_SEND = 0x0006;
        public const ushort METHOD_DATA = 0x0007;
        public const ushort METHOD_CREATE_PERMISSION = 0x0008;
        public const ushort METHOD_CHANNEL_BIND = 0x0009;
        public const ushort METHOD_CONNECT = 0x000A;           // RFC 6062
        public const ushort METHOD_CONNECTION_BIND = 0x000B;    // RFC 6062
        public const ushort METHOD_CONNECTION_ATTEMPT = 0x000C; // RFC 6062

        //
        // Pre-composed STUN Binding message types
        //
        public const ushort BINDING_REQUEST = 0x0001;
        public const ushort BINDING_INDICATION = 0x0011;
        public const ushort BINDING_SUCCESS_RESPONSE = 0x0101;
        public const ushort BINDING_ERROR_RESPONSE = 0x0111;

        //
        // Pre-composed TURN Allocate message types
        //
        public const ushort ALLOCATE_REQUEST = 0x0003;
        public const ushort ALLOCATE_SUCCESS_RESPONSE = 0x0103;
        public const ushort ALLOCATE_ERROR_RESPONSE = 0x0113;

        //
        // Pre-composed TURN Refresh message types
        //
        public const ushort REFRESH_REQUEST = 0x0004;
        public const ushort REFRESH_SUCCESS_RESPONSE = 0x0104;
        public const ushort REFRESH_ERROR_RESPONSE = 0x0114;

        //
        // Pre-composed TURN Send/Data message types (indications only)
        //
        public const ushort SEND_INDICATION = 0x0016;
        public const ushort DATA_INDICATION = 0x0017;

        //
        // Pre-composed TURN CreatePermission message types
        //
        public const ushort CREATE_PERMISSION_REQUEST = 0x0008;
        public const ushort CREATE_PERMISSION_SUCCESS_RESPONSE = 0x0108;
        public const ushort CREATE_PERMISSION_ERROR_RESPONSE = 0x0118;

        //
        // Pre-composed TURN ChannelBind message types
        //
        public const ushort CHANNEL_BIND_REQUEST = 0x0009;
        public const ushort CHANNEL_BIND_SUCCESS_RESPONSE = 0x0109;
        public const ushort CHANNEL_BIND_ERROR_RESPONSE = 0x0119;


        /// <summary>
        /// Composes a STUN message type from a method and class.
        ///
        /// The STUN header encodes the method and class into 14 bits using the
        /// bit layout defined in RFC 5389 §6:
        ///   M11..M7  C1  M6..M4  C0  M3..M0
        /// </summary>
        public static ushort Compose(ushort method, ushort messageClass)
        {
            //
            // Extract the method bits and class bits, then interleave them
            // per the RFC 5389 encoding
            //
            int m = method & 0x0FFF;
            int c = messageClass & 0x0110;

            int result = (m & 0x000F)                 // M3..M0  (bits 0-3)
                       | (c & 0x0010)                  // C0      (bit 4)
                       | ((m & 0x0070) << 1)           // M6..M4  (bits 5-7)
                       | (c & 0x0100)                  // C1      (bit 8)
                       | ((m & 0x0F80) << 2);          // M11..M7 (bits 9-13)

            return (ushort)result;
        }


        /// <summary>
        /// Extracts the method from a composed STUN message type.
        /// </summary>
        public static ushort GetMethod(ushort messageType)
        {
            int method = (messageType & 0x000F)
                       | ((messageType & 0x00E0) >> 1)
                       | ((messageType & 0x3E00) >> 2);

            return (ushort)method;
        }


        /// <summary>
        /// Extracts the class from a composed STUN message type.
        /// </summary>
        public static ushort GetClass(ushort messageType)
        {
            int messageClass = (messageType & 0x0010)
                             | (messageType & 0x0100);

            return (ushort)messageClass;
        }


        /// <summary>
        /// Returns true if this message type represents a request.
        /// </summary>
        public static bool IsRequest(ushort messageType)
        {
            return GetClass(messageType) == CLASS_REQUEST;
        }


        /// <summary>
        /// Returns true if this message type represents a success response.
        /// </summary>
        public static bool IsSuccessResponse(ushort messageType)
        {
            return GetClass(messageType) == CLASS_SUCCESS_RESPONSE;
        }


        /// <summary>
        /// Returns true if this message type represents an error response.
        /// </summary>
        public static bool IsErrorResponse(ushort messageType)
        {
            return GetClass(messageType) == CLASS_ERROR_RESPONSE;
        }


        /// <summary>
        /// Returns true if this message type represents an indication.
        /// </summary>
        public static bool IsIndication(ushort messageType)
        {
            return GetClass(messageType) == CLASS_INDICATION;
        }
    }
}
