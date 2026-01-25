//
// Security Context Authenticated Users Provider
//
// Retrieves authenticated user sessions from Security database OAuth tokens.
// Queries OAUTHTokens table for active, non-expired tokens.
//
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Foundation.Services
{
    /// <summary>
    /// Provides authenticated user information from SecurityContext OAuth tokens
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
                // Query active, non-expired OAuth tokens
                //
                var tokens = await context.OAUTHTokens
                    .Where(t => t.active && !t.deleted && t.expiryDateTime > now)
                    .OrderByDescending(t => t.expiryDateTime)
                    .Take(100) // Limit for safety
                    .ToListAsync()
                    .ConfigureAwait(false);

                //
                // Parse userData JSON to extract user information
                //
                foreach (var token in tokens)
                {
                    try
                    {
                        var session = ParseTokenToSession(token);
                        if (session != null)
                        {
                            result.Sessions.Add(session);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse OAuth token {Id}", token.id);
                    }
                }

                //
                // Deduplicate by username, keeping the session with longest expiry
                //
                result.Sessions = result.Sessions
                    .GroupBy(s => s.Username?.ToLowerInvariant() ?? "unknown")
                    .Select(g => g.OrderByDescending(s => s.ExpiresAt).First())
                    .OrderBy(s => s.Username)
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


        /// <summary>
        /// Parses an OAUTHToken's userData JSON to extract session information
        /// </summary>
        private AuthenticatedUserSession ParseTokenToSession(OAUTHToken token)
        {
            if (string.IsNullOrEmpty(token.userData))
            {
                return null;
            }

            try
            {
                var userDataJson = JsonDocument.Parse(token.userData);
                var root = userDataJson.RootElement;

                var session = new AuthenticatedUserSession
                {
                    ExpiresAt = token.expiryDateTime,
                    SessionStart = token.expiryDateTime.AddHours(-1) // Estimate based on typical token lifetime
                };

                //
                // Extract user information from various possible JSON structures
                //
                if (root.TryGetProperty("sub", out var subElement))
                {
                    session.Username = subElement.GetString();
                }
                else if (root.TryGetProperty("username", out var usernameElement))
                {
                    session.Username = usernameElement.GetString();
                }
                else if (root.TryGetProperty("name", out var nameElement))
                {
                    session.Username = nameElement.GetString();
                }

                if (root.TryGetProperty("name", out var displayNameElement))
                {
                    session.DisplayName = displayNameElement.GetString();
                }
                else if (root.TryGetProperty("displayName", out var dnElement))
                {
                    session.DisplayName = dnElement.GetString();
                }

                if (root.TryGetProperty("email", out var emailElement))
                {
                    session.Email = emailElement.GetString();
                }

                if (root.TryGetProperty("client_id", out var clientElement))
                {
                    session.ClientApplication = clientElement.GetString();
                }
                else if (root.TryGetProperty("aud", out var audElement))
                {
                    session.ClientApplication = audElement.GetString();
                }

                //
                // Skip if we couldn't extract a username
                //
                if (string.IsNullOrEmpty(session.Username))
                {
                    return null;
                }

                return session;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
