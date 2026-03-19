//
// BasicAuthCredentialCache.cs
//
// In-memory credential cache to avoid hitting the Security database on every
// WebDAV request.  OS-level WebDAV clients (Windows Explorer, macOS Finder)
// are extremely chatty — a single folder browse can generate dozens of
// PROPFIND requests.  Without caching, each one would trigger a full
// AuthenticateLocalCredentials round-trip.
//
// Cache entries expire after a configurable TTL (default 5 minutes).
//
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Foundation.Security.Database;

namespace Scheduler.WebDAV.Middleware
{
    /// <summary>
    /// Thread-safe in-memory cache for validated Basic Auth credentials.
    /// </summary>
    public class BasicAuthCredentialCache
    {
        private readonly ConcurrentDictionary<string, CachedCredential> _cache = new();
        private readonly TimeSpan _ttl;

        public BasicAuthCredentialCache()
        {
            _ttl = TimeSpan.FromMinutes(5);
        }

        public BasicAuthCredentialCache(int ttlMinutes)
        {
            _ttl = TimeSpan.FromMinutes(ttlMinutes);
        }


        /// <summary>
        /// Tries to get a cached credential entry for the given username and password.
        /// Returns null if not cached or expired.
        /// </summary>
        public CachedCredential TryGet(string username, string password)
        {
            string key = BuildKey(username, password);

            if (_cache.TryGetValue(key, out CachedCredential cached))
            {
                if (cached.ExpiresAt > DateTime.UtcNow)
                {
                    return cached;
                }
                else
                {
                    // Expired — remove it
                    _cache.TryRemove(key, out _);
                }
            }

            return null;
        }


        /// <summary>
        /// Stores a validated credential in the cache.
        /// </summary>
        public void Set(string username, string password, SecurityUser user, Guid tenantGuid)
        {
            string key = BuildKey(username, password);

            _cache[key] = new CachedCredential
            {
                User = user,
                TenantGuid = tenantGuid,
                ExpiresAt = DateTime.UtcNow.Add(_ttl)
            };
        }


        /// <summary>
        /// Builds a cache key from username + password hash.
        /// We hash the password so it's not stored in plain text in memory.
        /// </summary>
        private static string BuildKey(string username, string password)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            string passwordHash = Convert.ToBase64String(hash);
            return $"{username.ToUpperInvariant()}::{passwordHash}";
        }


        public class CachedCredential
        {
            public SecurityUser User { get; set; }
            public Guid TenantGuid { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
