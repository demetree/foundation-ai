//
// Session Tracking Service Implementation
//
// Records session metadata at token issuance time for compliance-grade audit trails.
// Uses raw SQL to work before and after entity scaffolding.
//
using Foundation.Security.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foundation.Services
{
    /// <summary>
    /// Provides session tracking operations for compliance-grade audit trails
    /// </summary>
    public class SessionTrackingService : ISessionTrackingService
    {
        private readonly ILogger<SessionTrackingService> _logger;

        public SessionTrackingService(ILogger<SessionTrackingService> logger)
        {
            _logger = logger;
        }


        public async Task<int> RecordSessionAsync(SessionInfo sessionInfo)
        {
            try
            {
                await using var context = new SecurityContext();

                var sql = @"
                    INSERT INTO Security.UserSession 
                        (securityUserId, objectGuid, tokenId, sessionStart, expiresAt, 
                         ipAddress, userAgent, loginMethod, clientApplication, 
                         isRevoked, active, deleted)
                    OUTPUT INSERTED.id
                    VALUES 
                        (@securityUserId, @objectGuid, @tokenId, @sessionStart, @expiresAt,
                         @ipAddress, @userAgent, @loginMethod, @clientApplication,
                         0, 1, 0)";

                var parameters = new[]
                {
                    new SqlParameter("@securityUserId", sessionInfo.SecurityUserId),
                    new SqlParameter("@objectGuid", sessionInfo.ObjectGuid),
                    new SqlParameter("@tokenId", (object)sessionInfo.TokenId ?? DBNull.Value),
                    new SqlParameter("@sessionStart", sessionInfo.SessionStart),
                    new SqlParameter("@expiresAt", sessionInfo.ExpiresAt),
                    new SqlParameter("@ipAddress", (object)sessionInfo.IpAddress ?? DBNull.Value),
                    new SqlParameter("@userAgent", (object)TruncateUserAgent(sessionInfo.UserAgent) ?? DBNull.Value),
                    new SqlParameter("@loginMethod", (object)sessionInfo.LoginMethod ?? DBNull.Value),
                    new SqlParameter("@clientApplication", (object)sessionInfo.ClientApplication ?? DBNull.Value)
                };

                var result = await context.Database
                    .SqlQueryRaw<int>(sql, parameters)
                    .ToListAsync()
                    .ConfigureAwait(false);

                return result.Count > 0 ? result[0] : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording session for user {UserId}", sessionInfo.SecurityUserId);
                return 0;
            }
        }


        public async Task<bool> RevokeSessionAsync(int sessionId, string revokedBy, string reason)
        {
            try
            {
                await using var context = new SecurityContext();
                var now = DateTime.UtcNow;

                var sql = @"
                    UPDATE Security.UserSession
                    SET isRevoked = 1, revokedAt = @revokedAt, revokedBy = @revokedBy, revokedReason = @revokedReason
                    WHERE id = @sessionId AND isRevoked = 0";

                var rowsAffected = await context.Database.ExecuteSqlRawAsync(
                    sql,
                    new SqlParameter("@sessionId", sessionId),
                    new SqlParameter("@revokedAt", now),
                    new SqlParameter("@revokedBy", (object)revokedBy ?? DBNull.Value),
                    new SqlParameter("@revokedReason", (object)reason ?? DBNull.Value)
                ).ConfigureAwait(false);

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Session {SessionId} revoked by {RevokedBy}: {Reason}", sessionId, revokedBy, reason);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking session {SessionId}", sessionId);
                return false;
            }
        }


        public async Task<int> RevokeAllUserSessionsAsync(int securityUserId, string revokedBy, string reason)
        {
            try
            {
                await using var context = new SecurityContext();
                var now = DateTime.UtcNow;

                var sql = @"
                    UPDATE Security.UserSession
                    SET isRevoked = 1, revokedAt = @revokedAt, revokedBy = @revokedBy, revokedReason = @revokedReason
                    WHERE securityUserId = @securityUserId AND isRevoked = 0";

                var rowsAffected = await context.Database.ExecuteSqlRawAsync(
                    sql,
                    new SqlParameter("@securityUserId", securityUserId),
                    new SqlParameter("@revokedAt", now),
                    new SqlParameter("@revokedBy", (object)revokedBy ?? DBNull.Value),
                    new SqlParameter("@revokedReason", (object)reason ?? DBNull.Value)
                ).ConfigureAwait(false);

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Revoked {Count} sessions for user {UserId} by {RevokedBy}: {Reason}", 
                        rowsAffected, securityUserId, revokedBy, reason);
                }

                return rowsAffected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all sessions for user {UserId}", securityUserId);
                return 0;
            }
        }


        public async Task<List<SessionInfo>> GetActiveSessionsAsync()
        {
            var sessions = new List<SessionInfo>();

            try
            {
                await using var context = new SecurityContext();
                var now = DateTime.UtcNow;

                var sql = @"
                    SELECT 
                        s.id, s.securityUserId, s.objectGuid, s.tokenId,
                        s.sessionStart, s.expiresAt, s.ipAddress, s.userAgent,
                        s.loginMethod, s.clientApplication, s.isRevoked,
                        s.revokedAt, s.revokedBy, s.revokedReason,
                        u.accountName AS Username,
                        COALESCE(NULLIF(TRIM(COALESCE(u.firstName, '') + ' ' + COALESCE(u.lastName, '')), ''), u.accountName) AS DisplayName,
                        u.emailAddress AS Email
                    FROM Security.UserSession s
                    INNER JOIN Security.SecurityUser u ON s.securityUserId = u.id
                    WHERE s.expiresAt > @now
                      AND s.isRevoked = 0
                      AND s.active = 1 AND s.deleted = 0
                      AND u.active = 1 AND u.deleted = 0
                    ORDER BY s.sessionStart DESC";

                var results = await context.Database
                    .SqlQueryRaw<SessionQueryResult>(sql, new SqlParameter("@now", now))
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (var r in results)
                {
                    sessions.Add(new SessionInfo
                    {
                        Id = r.id,
                        SecurityUserId = r.securityUserId,
                        ObjectGuid = r.objectGuid,
                        TokenId = r.tokenId,
                        SessionStart = r.sessionStart,
                        ExpiresAt = r.expiresAt,
                        IpAddress = r.ipAddress,
                        UserAgent = r.userAgent,
                        LoginMethod = r.loginMethod,
                        ClientApplication = r.clientApplication,
                        IsRevoked = r.isRevoked,
                        RevokedAt = r.revokedAt,
                        RevokedBy = r.revokedBy,
                        RevokedReason = r.revokedReason,
                        Username = r.Username,
                        DisplayName = r.DisplayName,
                        Email = r.Email
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sessions");
            }

            return sessions;
        }


        public async Task<List<SessionInfo>> GetUserSessionsAsync(int securityUserId)
        {
            var sessions = new List<SessionInfo>();

            try
            {
                await using var context = new SecurityContext();

                var sql = @"
                    SELECT 
                        s.id, s.securityUserId, s.objectGuid, s.tokenId,
                        s.sessionStart, s.expiresAt, s.ipAddress, s.userAgent,
                        s.loginMethod, s.clientApplication, s.isRevoked,
                        s.revokedAt, s.revokedBy, s.revokedReason,
                        u.accountName AS Username,
                        COALESCE(NULLIF(TRIM(COALESCE(u.firstName, '') + ' ' + COALESCE(u.lastName, '')), ''), u.accountName) AS DisplayName,
                        u.emailAddress AS Email
                    FROM Security.UserSession s
                    INNER JOIN Security.SecurityUser u ON s.securityUserId = u.id
                    WHERE s.securityUserId = @securityUserId
                      AND s.active = 1 AND s.deleted = 0
                    ORDER BY s.sessionStart DESC";

                var results = await context.Database
                    .SqlQueryRaw<SessionQueryResult>(sql, new SqlParameter("@securityUserId", securityUserId))
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (var r in results)
                {
                    sessions.Add(new SessionInfo
                    {
                        Id = r.id,
                        SecurityUserId = r.securityUserId,
                        ObjectGuid = r.objectGuid,
                        TokenId = r.tokenId,
                        SessionStart = r.sessionStart,
                        ExpiresAt = r.expiresAt,
                        IpAddress = r.ipAddress,
                        UserAgent = r.userAgent,
                        LoginMethod = r.loginMethod,
                        ClientApplication = r.clientApplication,
                        IsRevoked = r.isRevoked,
                        RevokedAt = r.revokedAt,
                        RevokedBy = r.revokedBy,
                        RevokedReason = r.revokedReason,
                        Username = r.Username,
                        DisplayName = r.DisplayName,
                        Email = r.Email
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sessions for user {UserId}", securityUserId);
            }

            return sessions;
        }


        private string TruncateUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return null;
            }
            return userAgent.Length > 500 ? userAgent.Substring(0, 500) : userAgent;
        }


        /// <summary>
        /// DTO for SQL query results - must be public for EF Core
        /// </summary>
        public class SessionQueryResult
        {
            public int id { get; set; }
            public int securityUserId { get; set; }
            public Guid objectGuid { get; set; }
            public string tokenId { get; set; }
            public DateTime sessionStart { get; set; }
            public DateTime expiresAt { get; set; }
            public string ipAddress { get; set; }
            public string userAgent { get; set; }
            public string loginMethod { get; set; }
            public string clientApplication { get; set; }
            public bool isRevoked { get; set; }
            public DateTime? revokedAt { get; set; }
            public string revokedBy { get; set; }
            public string revokedReason { get; set; }
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string Email { get; set; }
        }
    }
}
