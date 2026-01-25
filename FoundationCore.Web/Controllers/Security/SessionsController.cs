//
// Sessions Controller
//
// API endpoints for session management including revocation.
// Follows Foundation security patterns with permission level checks and heavy auditing.
// Requires administrative privileges (writePermissionLevel = 255) or Security Administrator role.
//
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Security.Controllers.WebAPI
{
    /// <summary>
    /// API controller for session management and revocation.
    /// 
    /// Uses Foundation security framework:
    /// - Extends SecureWebAPIController for module-based security checks
    /// - Requires writePermissionLevel = 255 for all revocation operations
    /// - Alternatively allows 'Security Administrator' role
    /// - Heavy auditing for compliance
    /// </summary>
    public class SessionsController : SecureWebAPIController
    {
        //
        // Permission levels required for session operations
        // 255 = administrative level (highest)
        //
        private const int READ_PERMISSION_LEVEL_REQUIRED = 255;
        private const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

        private readonly ISessionTrackingService _sessionTracking;
        private readonly ILogger<SessionsController> _logger;
        private readonly SecurityContext _context;

        public SessionsController(
            ISessionTrackingService sessionTracking,
            SecurityContext context,
            ILogger<SessionsController> logger) : base("Security", "UserSession")
        {
            _sessionTracking = sessionTracking;
            _context = context;
            _logger = logger;
        }


        /// <summary>
        /// Gets all active sessions for the system health dashboard.
        /// Requires readPermissionLevel = 255 OR Security Administrator role.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/sessions")]
        public async Task<IActionResult> GetActiveSessions(CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            SecurityUser user = await GetSecurityUserAsync(cancellationToken);

            //
            // Check permission: must have admin-level read permission OR be a Security Administrator
            //
            bool hasAdminRead = await UserCanReadAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            bool isSecurityAdmin = await UserCanAdministerSecurityModuleAsync(user, cancellationToken);

            if (!hasAdminRead && !isSecurityAdmin)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.UnauthorizedAccessAttempt,
                    $"User '{user?.accountName ?? "Unknown"}' attempted to view active sessions without sufficient privileges.");
                return Unauthorized();
            }

            try
            {
                var sessions = await _sessionTracking.GetActiveSessionsAsync();

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.ReadList,
                    isSecurityAdmin
                        ? $"Active sessions list read by Security Administrator '{user?.accountName}'. Returned {sessions.Count} sessions."
                        : $"Active sessions list read by admin user '{user?.accountName}'. Returned {sessions.Count} sessions.");

                return Ok(sessions);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    $"Error retrieving active sessions for user '{user?.accountName}'",
                    null,
                    ex);

                _logger.LogError(ex, "Error retrieving active sessions");
                return Problem("Failed to retrieve active sessions");
            }
        }


        /// <summary>
        /// Gets sessions for a specific user.
        /// Requires readPermissionLevel = 255 OR Security Administrator role.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/sessions/user/{userId}")]
        public async Task<IActionResult> GetUserSessions(int userId, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            SecurityUser user = await GetSecurityUserAsync(cancellationToken);

            //
            // Check permission
            //
            bool hasAdminRead = await UserCanReadAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            bool isSecurityAdmin = await UserCanAdministerSecurityModuleAsync(user, cancellationToken);

            if (!hasAdminRead && !isSecurityAdmin)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.UnauthorizedAccessAttempt,
                    $"User '{user?.accountName ?? "Unknown"}' attempted to view sessions for user ID {userId} without sufficient privileges.");
                return Unauthorized();
            }

            try
            {
                var sessions = await _sessionTracking.GetUserSessionsAsync(userId);

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.ReadList,
                    $"Sessions for user ID {userId} read by '{user?.accountName}'. Returned {sessions.Count} sessions.");

                return Ok(sessions);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    $"Error retrieving sessions for user ID {userId}",
                    userId.ToString(),
                    ex);

                _logger.LogError(ex, "Error retrieving sessions for user {UserId}", userId);
                return Problem("Failed to retrieve user sessions");
            }
        }


        /// <summary>
        /// Revokes a specific session by ID.
        /// Requires writePermissionLevel = 255 OR Security Administrator role.
        /// This is a security-sensitive operation with heavy auditing.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/sessions/{sessionId}/revoke")]
        public async Task<IActionResult> RevokeSession(int sessionId, [FromBody] RevokeSessionRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            SecurityUser user = await GetSecurityUserAsync(cancellationToken);

            //
            // Check permission: must have admin-level write permission OR be a Security Administrator
            //
            bool hasAdminWrite = await UserCanWriteAsync(user, WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            bool isSecurityAdmin = await UserCanAdministerSecurityModuleAsync(user, cancellationToken);

            if (!hasAdminWrite && !isSecurityAdmin)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.UnauthorizedAccessAttempt,
                    $"User '{user?.accountName ?? "Unknown"}' attempted to revoke session {sessionId} without sufficient privileges.",
                    sessionId.ToString());

                DestroySessionAndAuthentication();
                return Unauthorized();
            }

            string adminUsername = user?.accountName ?? "Unknown";
            string reason = request?.Reason ?? "Administrative action";

            try
            {
                //
                // Get the session to find the target user for event logging
                //
                var session = await _context.UserSessions
                    .Include(s => s.securityUser)
                    .FirstOrDefaultAsync(s => s.id == sessionId && s.active && !s.deleted, cancellationToken);

                if (session == null)
                {
                    await CreateAuditEventAsync(
                        AuditEngine.AuditType.Error,
                        $"Attempted to revoke session {sessionId} but session was not found. Admin: '{adminUsername}'",
                        sessionId.ToString());

                    return NotFound(new { success = false, message = "Session not found" });
                }

                var targetUser = session.securityUser;
                int targetUserId = targetUser?.id ?? 0;
                string targetUsername = targetUser?.accountName ?? "Unknown";
                
                var success = await _sessionTracking.RevokeSessionAsync(sessionId, adminUsername, reason);

                if (success)
                {
                    //
                    // Log SecurityUserEvent for the target user
                    //
                    if (targetUser != null)
                    {
                        SecurityLogic.AddUserEvent(
                            targetUser,
                            SecurityLogic.SecurityUserEventTypes.SessionRevoked,
                            $"Session revoked by '{adminUsername}'. Reason: {reason}");
                    }

                    //
                    // Audit the successful revocation with full details
                    //
                    await CreateAuditEventAsync(
                        AuditEngine.AuditType.UpdateEntity,
                        $"Session {sessionId} for user '{targetUsername}' successfully REVOKED by '{adminUsername}'. Reason: {reason}",
                        true,
                        sessionId.ToString(),
                        JsonSerializer.Serialize(new { sessionId, userId = targetUserId, username = targetUsername, status = "active" }),
                        JsonSerializer.Serialize(new { sessionId, userId = targetUserId, username = targetUsername, status = "revoked", revokedBy = adminUsername, revokedAt = DateTime.UtcNow, reason }),
                        null);

                    _logger.LogWarning(
                        "SECURITY: Session {SessionId} for user '{TargetUser}' revoked by {Admin}. Reason: {Reason}",
                        sessionId, targetUsername, adminUsername, reason);

                    return Ok(new { success = true, message = "Session revoked successfully", targetUsername });
                }
                else
                {
                    await CreateAuditEventAsync(
                        AuditEngine.AuditType.Error,
                        $"Attempted to revoke session {sessionId} but session was already revoked. Admin: '{adminUsername}'",
                        sessionId.ToString());

                    return NotFound(new { success = false, message = "Session already revoked" });
                }
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    $"Error revoking session {sessionId} by '{adminUsername}'",
                    sessionId.ToString(),
                    ex);

                _logger.LogError(ex, "Error revoking session {SessionId}", sessionId);
                return Problem("Failed to revoke session");
            }
        }


        /// <summary>
        /// Revokes all sessions for a specific user.
        /// Requires writePermissionLevel = 255 OR Security Administrator role.
        /// This is a HIGH SECURITY operation with full audit trail.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/sessions/user/{userId}/revoke-all")]
        public async Task<IActionResult> RevokeAllUserSessions(int userId, [FromBody] RevokeSessionRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            SecurityUser user = await GetSecurityUserAsync(cancellationToken);

            //
            // Check permission
            //
            bool hasAdminWrite = await UserCanWriteAsync(user, WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            bool isSecurityAdmin = await UserCanAdministerSecurityModuleAsync(user, cancellationToken);

            if (!hasAdminWrite && !isSecurityAdmin)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.UnauthorizedAccessAttempt,
                    $"User '{user?.accountName ?? "Unknown"}' attempted to revoke ALL sessions for user ID {userId} without sufficient privileges.",
                    userId.ToString());

                DestroySessionAndAuthentication();
                return Unauthorized();
            }

            string adminUsername = user?.accountName ?? "Unknown";
            string reason = request?.Reason ?? "Administrative action - all sessions revoked";

            try
            {
                //
                // Get target user info for audit trail
                //
                string targetUsername = "Unknown";
                try
                {
                    var targetUser = await SecurityLogic.GetUserRecordAsync(userId.ToString(), cancellationToken);
                    targetUsername = targetUser?.accountName ?? "Unknown";
                }
                catch { /* Continue even if we can't get target username */ }

                var count = await _sessionTracking.RevokeAllUserSessionsAsync(userId, adminUsername, reason);

                //
                // Full audit trail for this security-sensitive operation
                //
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.UpdateEntity,
                    $"ALL SESSIONS for user '{targetUsername}' (ID: {userId}) were REVOKED by '{adminUsername}'. " +
                    $"Total sessions revoked: {count}. Reason: {reason}",
                    true,
                    userId.ToString(),
                    JsonSerializer.Serialize(new { userId, targetUsername, sessionsCount = count, status = "active" }),
                    JsonSerializer.Serialize(new { userId, targetUsername, sessionsCount = count, status = "all_revoked", revokedBy = adminUsername, revokedAt = DateTime.UtcNow, reason }),
                    null);

                _logger.LogWarning(
                    "SECURITY: All {Count} sessions for user '{TargetUser}' (ID: {UserId}) revoked by {Admin}. Reason: {Reason}",
                    count, targetUsername, userId, adminUsername, reason);

                return Ok(new { success = true, revokedCount = count, message = $"Revoked {count} session(s)" });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    $"Error revoking all sessions for user ID {userId} by '{adminUsername}'",
                    userId.ToString(),
                    ex);

                _logger.LogError(ex, "Error revoking all sessions for user {UserId}", userId);
                return Problem("Failed to revoke sessions");
            }
        }


        /// <summary>
        /// Revokes all sessions for a user AND locks their account (sets canLogin = false).
        /// Requires writePermissionLevel = 255 OR Security Administrator role.
        /// This is the HIGHEST SECURITY operation - use with extreme caution.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/sessions/{sessionId}/revoke-and-lock")]
        public async Task<IActionResult> RevokeSessionAndLockAccount(int sessionId, [FromBody] RevokeSessionRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            SecurityUser adminUser = await GetSecurityUserAsync(cancellationToken);

            //
            // Check permission
            //
            bool hasAdminWrite = await UserCanWriteAsync(adminUser, WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken);
            bool isSecurityAdmin = await UserCanAdministerSecurityModuleAsync(adminUser, cancellationToken);

            if (!hasAdminWrite && !isSecurityAdmin)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.UnauthorizedAccessAttempt,
                    $"User '{adminUser?.accountName ?? "Unknown"}' attempted to REVOKE AND LOCK via session {sessionId} without sufficient privileges.",
                    sessionId.ToString());

                DestroySessionAndAuthentication();
                return Unauthorized();
            }

            string adminUsername = adminUser?.accountName ?? "Unknown";
            string reason = request?.Reason ?? "Security action - session revoked and account locked";

            try
            {
                //
                // Get the session to find the user
                //
                var session = await _context.UserSessions
                    .Include(s => s.securityUser)
                    .FirstOrDefaultAsync(s => s.id == sessionId && s.active && !s.deleted, cancellationToken);

                if (session == null)
                {
                    await CreateAuditEventAsync(
                        AuditEngine.AuditType.Error,
                        $"Attempted to revoke-and-lock session {sessionId} but session was not found. Admin: '{adminUsername}'",
                        sessionId.ToString());

                    return NotFound(new { success = false, message = "Session not found" });
                }

                var targetUser = session.securityUser;
                string targetUsername = targetUser?.accountName ?? "Unknown";
                int targetUserId = targetUser?.id ?? 0;

                //
                // Capture before state for audit
                //
                var beforeState = new
                {
                    sessionId,
                    userId = targetUserId,
                    username = targetUsername,
                    canLogin = targetUser?.canLogin ?? false,
                    sessionStatus = "active"
                };

                //
                // Step 1: Revoke the specific session
                //
                await _sessionTracking.RevokeSessionAsync(sessionId, adminUsername, reason);

                //
                // Step 2: Revoke all other sessions for this user
                //
                int revokedCount = await _sessionTracking.RevokeAllUserSessionsAsync(targetUserId, adminUsername, reason);

                //
                // Step 3: Lock the account (set canLogin = false)
                //
                if (targetUser != null)
                {
                    targetUser.canLogin = false;
                    await _context.SaveChangesAsync(cancellationToken);

                    //
                    // Log SecurityUserEvent for the target user
                    //
                    SecurityLogic.AddUserEvent(
                        targetUser,
                        SecurityLogic.SecurityUserEventTypes.SessionRevokedWithAccountLock,
                        $"All sessions revoked and account locked by '{adminUsername}'. {revokedCount} session(s) terminated. Reason: {reason}");
                }

                //
                // Capture after state
                //
                var afterState = new
                {
                    sessionId,
                    userId = targetUserId,
                    username = targetUsername,
                    canLogin = false,
                    sessionStatus = "revoked",
                    allSessionsRevokedCount = revokedCount,
                    accountLocked = true,
                    revokedBy = adminUsername,
                    revokedAt = DateTime.UtcNow,
                    reason
                };

                //
                // Heavy audit trail for this critical security operation
                //
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.UpdateEntity,
                    $"CRITICAL SECURITY ACTION: User '{targetUsername}' (ID: {targetUserId}) was REVOKED AND LOCKED by '{adminUsername}'. " +
                    $"Total sessions revoked: {revokedCount}. Account canLogin set to FALSE. Reason: {reason}",
                    true,
                    targetUserId.ToString(),
                    JsonSerializer.Serialize(beforeState),
                    JsonSerializer.Serialize(afterState),
                    null);

                _logger.LogWarning(
                    "CRITICAL SECURITY: User '{TargetUser}' (ID: {UserId}) REVOKED AND LOCKED by {Admin}. Sessions revoked: {Count}. Reason: {Reason}",
                    targetUsername, targetUserId, adminUsername, revokedCount, reason);

                //
                // Clear security caches
                //
                SecurityFramework.ClearSecurityCaches();
                SecurityLogic.ClearSecurityCaches();

                return Ok(new
                {
                    success = true,
                    message = $"Session revoked, {revokedCount} total session(s) revoked, and account locked",
                    revokedCount,
                    accountLocked = true,
                    targetUsername
                });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    $"Error during revoke-and-lock for session {sessionId} by '{adminUsername}'",
                    sessionId.ToString(),
                    ex);

                _logger.LogError(ex, "Error during revoke-and-lock for session {SessionId}", sessionId);
                return Problem("Failed to revoke session and lock account");
            }
        }
    }


    /// <summary>
    /// Request body for session revocation
    /// </summary>
    public class RevokeSessionRequest
    {
        /// <summary>
        /// Reason for revoking the session (recorded in audit trail).
        /// Required for compliance reporting.
        /// </summary>
        [MaxLength(500)]
        public string Reason { get; set; }

        /// <summary>
        /// Optional: Whether to also lock the user's account
        /// </summary>
        public bool LockAccount { get; set; } = false;
    }
}
