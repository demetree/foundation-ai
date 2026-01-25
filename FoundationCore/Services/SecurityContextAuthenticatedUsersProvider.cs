//
// Security Context Authenticated Users Provider
//
// Retrieves authenticated user sessions from UserSession table (primary)
// with fallback to OpenIddict tables if UserSession is empty (migration period).
//
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foundation.Services
{
    /// <summary>
    /// Provides authenticated user information from UserSession table,
    /// with fallback to OpenIddict for backwards compatibility.
    /// </summary>
    public class SecurityContextAuthenticatedUsersProvider : IAuthenticatedUsersProvider
    {
        private readonly ILogger<SecurityContextAuthenticatedUsersProvider> _logger;

        public SecurityContextAuthenticatedUsersProvider(ILogger<SecurityContextAuthenticatedUsersProvider> logger)
        {
            _logger = logger;
        }


        public async Task<AuthenticatedUsersInfo> GetAuthenticatedUsersAsync()
        {
            var result = new AuthenticatedUsersInfo();

            try
            {
                await using var context = new SecurityContext();
                var now = DateTime.UtcNow;

                //
                // Try UserSession table first (primary source after migration)
                //
                var userSessionSql = @"
                    SELECT 
                        s.id AS SessionId,
                        u.accountName AS Username,
                        COALESCE(NULLIF(TRIM(COALESCE(u.firstName, '') + ' ' + COALESCE(u.lastName, '')), ''), u.accountName) AS DisplayName,
                        u.emailAddress AS Email,
                        s.sessionStart AS SessionStart,
                        s.expiresAt AS ExpiresAt,
                        s.loginMethod AS LoginMethod,
                        s.ipAddress AS IpAddress,
                        s.userAgent AS UserAgent,
                        s.clientApplication AS ClientApplication
                    FROM Security.UserSession s
                    INNER JOIN Security.SecurityUser u ON s.securityUserId = u.id
                    WHERE s.expiresAt > @p0
                      AND s.isRevoked = 0
                      AND s.active = 1 AND s.deleted = 0
                      AND u.active = 1 AND u.deleted = 0
                    ORDER BY s.sessionStart DESC";

                List<SessionQueryResult> sessions = null;

                try
                {
                    sessions = await context.Database
                        .SqlQueryRaw<SessionQueryResult>(userSessionSql, now)
                        .ToListAsync()
                        .ConfigureAwait(false);
                }
                catch
                {
                    // UserSession table may not exist yet - fall back to OpenIddict
                    sessions = null;
                }

                //
                // If no sessions from UserSession, fall back to OpenIddict
                //
                if (sessions == null || sessions.Count == 0)
                {
                    sessions = await GetSessionsFromOpenIddict(context, now);
                }

                //
                // Deduplicate by username, keeping the most recent session
                //
                var uniqueSessions = sessions
                    .GroupBy(s => s.Username?.ToLowerInvariant() ?? "unknown")
                    .Select(g => g.OrderByDescending(s => s.SessionStart).First())
                    .ToList();

                foreach (var session in uniqueSessions)
                {
                    result.Sessions.Add(new AuthenticatedUserSession
                    {
                        SessionId = session.SessionId > 0 ? session.SessionId : null,
                        Username = session.Username,
                        DisplayName = session.DisplayName,
                        Email = session.Email,
                        SessionStart = session.SessionStart ?? DateTime.MinValue,
                        ExpiresAt = session.ExpiresAt ?? DateTime.MaxValue,
                        ClientApplication = session.ClientApplication,
                        IpAddress = session.IpAddress,
                        UserAgent = TruncateUserAgent(session.UserAgent),
                        LoginMethod = session.LoginMethod ?? "Unknown"
                    });
                }

                result.Sessions = result.Sessions
                    .OrderByDescending(s => s.SessionStart)
                    .ToList();

                result.TotalCount = result.Sessions.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving authenticated users");
                result.ErrorMessage = $"Failed to retrieve authenticated users: {ex.Message}";
            }

            return result;
        }


        private async Task<List<SessionQueryResult>> GetSessionsFromOpenIddict(SecurityContext context, DateTime now)
        {
            //
            // Fallback: Query OpenIddictTokens joined with SecurityUsers and LoginAttempts
            //
            var sql = @"
                SELECT 
                    0 AS SessionId,
                    u.accountName AS Username,
                    COALESCE(NULLIF(TRIM(COALESCE(u.firstName, '') + ' ' + COALESCE(u.lastName, '')), ''), u.accountName) AS DisplayName,
                    u.emailAddress AS Email,
                    t.CreationDate AS SessionStart,
                    t.ExpirationDate AS ExpiresAt,
                    CASE 
                        WHEN a.[Type] = 'permanent' THEN 'SSO'
                        ELSE COALESCE(a.[Type], 'Unknown')
                    END AS LoginMethod,
                    la.ipAddress AS IpAddress,
                    la.userAgent AS UserAgent,
                    a.[Type] AS ClientApplication
                FROM dbo.OpenIddictTokens t
                LEFT JOIN dbo.OpenIddictAuthorizations a ON t.AuthorizationId = a.Id
                INNER JOIN Security.SecurityUser u ON LOWER(t.Subject) = LOWER(CAST(u.objectGuid AS NVARCHAR(36)))
                OUTER APPLY (
                    SELECT TOP 1 l.ipAddress, l.userAgent
                    FROM Security.LoginAttempt l
                    WHERE LOWER(l.userName) = LOWER(u.accountName)
                      AND l.timeStamp <= t.CreationDate
                      AND l.active = 1 AND l.deleted = 0
                    ORDER BY l.timeStamp DESC
                ) la
                WHERE t.ExpirationDate > @p0
                  AND t.Status = 'valid'
                  AND u.active = 1
                  AND u.deleted = 0
                ORDER BY t.CreationDate DESC";

            return await context.Database
                .SqlQueryRaw<SessionQueryResult>(sql, now)
                .ToListAsync()
                .ConfigureAwait(false);
        }


        private string TruncateUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return null;
            }

            if (userAgent.Contains("Edge"))
                return "Edge";
            if (userAgent.Contains("Chrome"))
                return "Chrome";
            if (userAgent.Contains("Firefox"))
                return "Firefox";
            if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
                return "Safari";

            return userAgent.Length > 30 ? userAgent.Substring(0, 30) + "..." : userAgent;
        }


        /// <summary>
        /// DTO for raw SQL query results - must be public for EF Core proxy generation
        /// </summary>
        public class SessionQueryResult
        {
            public int SessionId { get; set; }
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string Email { get; set; }
            public DateTime? SessionStart { get; set; }
            public DateTime? ExpiresAt { get; set; }
            public string LoginMethod { get; set; }
            public string IpAddress { get; set; }
            public string UserAgent { get; set; }
            public string ClientApplication { get; set; }
        }
    }
}
