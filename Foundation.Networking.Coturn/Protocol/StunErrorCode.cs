// ============================================================================
//
// StunErrorCode.cs — Standard STUN and TURN error codes.
//
// Error codes are carried in the ERROR-CODE attribute (0x0009) as a 3-digit
// number split into a "class" (hundreds digit) and a "number" (tens+ones).
//
// ============================================================================

namespace Foundation.Networking.Coturn.Protocol
{
    /// <summary>
    ///
    /// Standard STUN and TURN error codes per RFC 5389 and RFC 5766.
    ///
    /// </summary>
    public static class StunErrorCode
    {
        //
        // ── 3xx — Redirection ────────────────────────────────────────────────
        //

        /// <summary>Try Alternate — the client should use the alternate server.</summary>
        public const int TRY_ALTERNATE = 300;

        //
        // ── 4xx — Client Errors ──────────────────────────────────────────────
        //

        /// <summary>Bad Request — malformed request.</summary>
        public const int BAD_REQUEST = 400;

        /// <summary>Unauthorized — missing or invalid credentials.</summary>
        public const int UNAUTHORIZED = 401;

        /// <summary>Forbidden — request denied by policy.</summary>
        public const int FORBIDDEN = 403;

        /// <summary>Unknown Attribute — comprehension-required attribute not understood.</summary>
        public const int UNKNOWN_ATTRIBUTE = 420;

        /// <summary>Allocation Mismatch — 5-tuple already has an allocation.</summary>
        public const int ALLOCATION_MISMATCH = 437;

        /// <summary>Stale Nonce — the nonce has expired, retry with the new one.</summary>
        public const int STALE_NONCE = 438;

        /// <summary>Address Family not Supported.</summary>
        public const int ADDRESS_FAMILY_NOT_SUPPORTED = 440;

        /// <summary>Wrong Credentials — the credentials do not match the allocation.</summary>
        public const int WRONG_CREDENTIALS = 441;

        /// <summary>Unsupported Transport Protocol.</summary>
        public const int UNSUPPORTED_TRANSPORT_PROTOCOL = 442;

        /// <summary>Peer Address Family Mismatch.</summary>
        public const int PEER_ADDRESS_FAMILY_MISMATCH = 443;

        //
        // ── 4xx — Quota Errors ───────────────────────────────────────────────
        //

        /// <summary>Allocation Quota Reached.</summary>
        public const int ALLOCATION_QUOTA_REACHED = 486;

        //
        // ── 5xx — Server Errors ──────────────────────────────────────────────
        //

        /// <summary>Server Error — internal server error.</summary>
        public const int SERVER_ERROR = 500;

        /// <summary>Insufficient Capacity — the server cannot fulfill the allocation.</summary>
        public const int INSUFFICIENT_CAPACITY = 508;
    }
}
