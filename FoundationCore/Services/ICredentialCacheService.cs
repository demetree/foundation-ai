using System;

namespace Foundation.Services
{
    /// <summary>
    /// Cached user credentials for cross-app authentication
    /// </summary>
    public class CachedCredentials
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service for securely caching user credentials in memory.
    /// Used for cross-app authentication where we need to obtain tokens from remote servers.
    /// </summary>
    public interface ICredentialCacheService
    {
        /// <summary>
        /// Caches the user's credentials (encrypted in memory).
        /// </summary>
        /// <param name="userObjectGuid">The user's object GUID (from sub claim)</param>
        /// <param name="username">The user's account name</param>
        /// <param name="password">The password to cache</param>
        void CacheCredentials(string userObjectGuid, string username, string password);

        /// <summary>
        /// Retrieves the cached credentials for a user.
        /// </summary>
        /// <param name="userObjectGuid">The user's object GUID (from sub claim)</param>
        /// <returns>The credentials, or null if not cached or expired</returns>
        CachedCredentials? GetCachedCredentials(string userObjectGuid);

        /// <summary>
        /// Removes cached credentials for a user (e.g., on logout).
        /// </summary>
        /// <param name="userObjectGuid">The user's object GUID</param>
        void RemoveCredentials(string userObjectGuid);
    }
}
