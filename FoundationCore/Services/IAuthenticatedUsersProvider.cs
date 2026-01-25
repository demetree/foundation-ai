//
// Authenticated Users Provider Interface
//
// Provides access to information about currently authenticated users.
// Used by System Health dashboard to display active sessions.
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foundation.Services
{
    /// <summary>
    /// Interface for providers that can retrieve authenticated user session information
    /// </summary>
    public interface IAuthenticatedUsersProvider
    {
        /// <summary>
        /// Gets information about currently authenticated users
        /// </summary>
        Task<AuthenticatedUsersInfo> GetAuthenticatedUsersAsync();
    }


    /// <summary>
    /// Contains summary information about authenticated users
    /// </summary>
    public class AuthenticatedUsersInfo
    {
        /// <summary>
        /// List of active user sessions
        /// </summary>
        public List<AuthenticatedUserSession> Sessions { get; set; } = new List<AuthenticatedUserSession>();

        /// <summary>
        /// Total count of active sessions
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Timestamp when this data was retrieved
        /// </summary>
        public DateTime AsOf { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Error message if retrieval failed, null otherwise
        /// </summary>
        public string ErrorMessage { get; set; }
    }


    /// <summary>
    /// Represents a single authenticated user session
    /// </summary>
    public class AuthenticatedUserSession
    {
        /// <summary>
        /// Username of the authenticated user
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Display name of the user (if available)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The client application the user authenticated to
        /// </summary>
        public string ClientApplication { get; set; }

        /// <summary>
        /// When the session/token was created
        /// </summary>
        public DateTime SessionStart { get; set; }

        /// <summary>
        /// When the token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Whether the token has expired
        /// </summary>
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        /// <summary>
        /// User's email address (if available)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// IP address from which the user logged in (if available)
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User agent/browser info from login (if available)
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Login method (e.g., "Local", "Microsoft", "Google")
        /// </summary>
        public string LoginMethod { get; set; }
    }
}
