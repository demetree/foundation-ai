//
// Security Context Authenticated Users Provider
//
// Retrieves authenticated user sessions from OpenIddict token tables.
// Queries OpenIddictTokens/Authorizations joined with SecurityUsers.
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
    /// Provides authenticated user information from OpenIddict token stores
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
                // Query OpenIddictTokens joined with SecurityUsers
                // Subject column contains the user's objectGuid as string
                // OpenIddict tables are in dbo schema, SecurityUser is in Security schema
                //
                var sql = @"
                    SELECT DISTINCT
                        u.accountName AS Username,
                        COALESCE(NULLIF(TRIM(COALESCE(u.firstName, '') + ' ' + COALESCE(u.lastName, '')), ''), u.accountName) AS DisplayName,
                        u.emailAddress AS Email,
                        t.CreationDate AS SessionStart,
                        t.ExpirationDate AS ExpiresAt,
                        a.[Type] AS ClientApplication
                    FROM dbo.OpenIddictTokens t
                    LEFT JOIN dbo.OpenIddictAuthorizations a ON t.AuthorizationId = a.Id
                    INNER JOIN Security.SecurityUser u ON LOWER(t.Subject) = LOWER(CAST(u.objectGuid AS NVARCHAR(36)))
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
                        ClientApplication = session.ClientApplication
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
        }
    }
}
