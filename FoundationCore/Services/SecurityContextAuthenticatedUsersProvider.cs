//
// Security Context Authenticated Users Provider
//
// Retrieves authenticated user sessions from OpenIddict token tables.
// Enriches with LoginAttempt data for IP address and user agent info.
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
    /// Provides authenticated user information from OpenIddict token stores,
    /// enriched with login event data for IP and user agent info.
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
                // Query OpenIddictTokens joined with SecurityUsers and LoginAttempts
                // Subject column contains the user's objectGuid as string
                // OpenIddict tables are in dbo schema, Security tables in Security schema
                // LoginAttempt is correlated by username and closest timeStamp to token creation
                //
                var sql = @"
                    SELECT 
                        u.accountName AS Username,
                        COALESCE(NULLIF(TRIM(COALESCE(u.firstName, '') + ' ' + COALESCE(u.lastName, '')), ''), u.accountName) AS DisplayName,
                        u.emailAddress AS Email,
                        t.CreationDate AS SessionStart,
                        t.ExpirationDate AS ExpiresAt,
                        a.[Type] AS ClientApplication,
                        la.ipAddress AS IpAddress,
                        la.userAgent AS UserAgent
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

                var sessions = await context.Database
                    .SqlQueryRaw<OpenIddictSessionResult>(sql, now)
                    .ToListAsync()
                    .ConfigureAwait(false);

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
                        Username = session.Username,
                        DisplayName = session.DisplayName,
                        Email = session.Email,
                        SessionStart = session.SessionStart ?? DateTime.MinValue,
                        ExpiresAt = session.ExpiresAt ?? DateTime.MaxValue,
                        ClientApplication = session.ClientApplication,
                        IpAddress = session.IpAddress,
                        UserAgent = TruncateUserAgent(session.UserAgent),
                        LoginMethod = DetermineLoginMethod(session.ClientApplication, session.UserAgent)
                    });
                }

                result.Sessions = result.Sessions
                    .OrderByDescending(s => s.SessionStart)
                    .ToList();

                result.TotalCount = result.Sessions.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving authenticated users from OpenIddict");
                result.ErrorMessage = $"Failed to retrieve authenticated users: {ex.Message}";
            }

            return result;
        }


        /// <summary>
        /// Truncates long user agent strings for display
        /// </summary>
        private string TruncateUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return null;
            }

            // Extract browser name from user agent if possible
            if (userAgent.Contains("Edge"))
                return "Edge";
            if (userAgent.Contains("Chrome"))
                return "Chrome";
            if (userAgent.Contains("Firefox"))
                return "Firefox";
            if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
                return "Safari";

            // Fallback: truncate to 30 chars
            return userAgent.Length > 30 ? userAgent.Substring(0, 30) + "..." : userAgent;
        }


        /// <summary>
        /// Determines the login method based on available data
        /// </summary>
        private string DetermineLoginMethod(string clientApplication, string userAgent)
        {
            if (string.IsNullOrEmpty(clientApplication))
            {
                return "Unknown";
            }

            // OpenIddict authorization type often indicates the flow
            if (clientApplication.Equals("permanent", StringComparison.OrdinalIgnoreCase))
            {
                return "SSO";
            }

            return clientApplication;
        }


        /// <summary>
        /// DTO for raw SQL query results - must be public for EF Core proxy generation
        /// </summary>
        public class OpenIddictSessionResult
        {
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string Email { get; set; }
            public DateTime? SessionStart { get; set; }
            public DateTime? ExpiresAt { get; set; }
            public string ClientApplication { get; set; }
            public string IpAddress { get; set; }
            public string UserAgent { get; set; }
        }
    }
}
