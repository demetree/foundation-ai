//
// Session Tracking Service Interface
//
// Provides session tracking operations for compliance-grade audit trails.
// Records session metadata at token issuance time.
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foundation.Services
{
    /// <summary>
    /// Interface for tracking user sessions at token issuance time
    /// </summary>
    public interface ISessionTrackingService
    {
        /// <summary>
        /// Records a new session when a token is issued
        /// </summary>
        Task<int> RecordSessionAsync(SessionInfo sessionInfo);

        /// <summary>
        /// Revokes a specific session by ID
        /// </summary>
        Task<bool> RevokeSessionAsync(int sessionId, string revokedBy, string reason);

        /// <summary>
        /// Revokes all sessions for a specific user
        /// </summary>
        Task<int> RevokeAllUserSessionsAsync(int securityUserId, string revokedBy, string reason);

        /// <summary>
        /// Gets active (non-expired, non-revoked) sessions for display
        /// </summary>
        Task<List<SessionInfo>> GetActiveSessionsAsync();

        /// <summary>
        /// Gets all sessions for a specific user
        /// </summary>
        Task<List<SessionInfo>> GetUserSessionsAsync(int securityUserId);

        /// <summary>
        /// Checks if a session is still valid (not revoked, not expired)
        /// </summary>
        Task<bool> IsSessionValidAsync(int sessionId);

        /// <summary>
        /// Checks if a session is still valid by token ID
        /// </summary>
        Task<bool> IsSessionValidByTokenAsync(string tokenId);
    }


    /// <summary>
    /// Session information for tracking
    /// </summary>
    public class SessionInfo
    {
        public int Id { get; set; }
        public int SecurityUserId { get; set; }
        public Guid ObjectGuid { get; set; }
        public string TokenId { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string LoginMethod { get; set; }
        public string ClientApplication { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string RevokedBy { get; set; }
        public string RevokedReason { get; set; }
        
        // Enriched fields for display
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
    }
}
