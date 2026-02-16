//
// Local Session Record
//
// POCO for the IndexedDB local session cache. Flat structure optimized
// for JSON serialization — no EF navigation properties.
//
// AI-assisted development - February 2026
//
using System;

namespace Foundation.Services
{
    /// <summary>
    /// Represents a user session cached in the local IndexedDB store.
    /// Used for fast per-request session validation without SQL Server round-trips.
    /// </summary>
    public class LocalSessionRecord
    {
        /// <summary>
        /// Session ID from SQL Server (non-auto-increment primary key).
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// The user's SecurityUser.id in SQL Server.
        /// </summary>
        public int SecurityUserId { get; set; }

        /// <summary>
        /// The user's objectGuid as string.
        /// </summary>
        public string ObjectGuid { get; set; }

        /// <summary>
        /// The OIDC token ID, used for token-based session lookups.
        /// </summary>
        public string TokenId { get; set; }

        /// <summary>
        /// When the session started (UTC).
        /// </summary>
        public DateTime SessionStart { get; set; }

        /// <summary>
        /// When the session expires (UTC).
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Whether the session has been revoked.
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Cached validity result (not revoked, not expired, user active).
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// When this record was last verified against SQL Server (UTC).
        /// </summary>
        public DateTime LastVerifiedAt { get; set; }
    }
}
