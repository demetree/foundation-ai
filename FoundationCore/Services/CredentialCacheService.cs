using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Foundation.Services
{
    /// <summary>
    /// Service for securely caching user credentials in memory.
    /// Uses Data Protection API to encrypt passwords and IMemoryCache for storage.
    /// </summary>
    public class CredentialCacheService : ICredentialCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IDataProtector _protector;
        private readonly ILogger<CredentialCacheService> _logger;
        
        // Cache key prefix to avoid collisions with other cached data
        private const string CacheKeyPrefix = "UserCredential_";
        
        // How long to keep credentials cached (matches typical session duration)
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(8);

        public CredentialCacheService(
            IMemoryCache cache,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<CredentialCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _protector = dataProtectionProvider.CreateProtector("Foundation.CredentialCache");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void CacheCredentials(string userObjectGuid, string username, string password)
        {
            if (string.IsNullOrEmpty(userObjectGuid))
            {
                throw new ArgumentNullException(nameof(userObjectGuid));
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            try
            {
                // Encrypt the password before storing
                string encryptedPassword = _protector.Protect(password);
                
                var credentials = new CachedCredentials
                {
                    Username = username,
                    Password = encryptedPassword
                };

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheDuration,
                    Priority = CacheItemPriority.High
                };

                string cacheKey = GetCacheKey(userObjectGuid);
                _cache.Set(cacheKey, credentials, cacheOptions);
                
                _logger.LogDebug("Cached credentials for user {UserObjectGuid} ({Username})", userObjectGuid, username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache credentials for user {UserObjectGuid}", userObjectGuid);
                throw;
            }
        }

        /// <inheritdoc/>
        public CachedCredentials? GetCachedCredentials(string userObjectGuid)
        {
            if (string.IsNullOrEmpty(userObjectGuid))
            {
                return null;
            }

            try
            {
                string cacheKey = GetCacheKey(userObjectGuid);
                
                if (_cache.TryGetValue(cacheKey, out CachedCredentials? cachedCreds) && 
                    cachedCreds != null)
                {
                    // Decrypt the password before returning
                    var result = new CachedCredentials
                    {
                        Username = cachedCreds.Username,
                        Password = _protector.Unprotect(cachedCreds.Password)
                    };
                    
                    _logger.LogDebug("Retrieved cached credentials for user {UserObjectGuid}", userObjectGuid);
                    return result;
                }

                _logger.LogDebug("No cached credentials found for user {UserObjectGuid}", userObjectGuid);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve cached credentials for user {UserObjectGuid}", userObjectGuid);
                return null;
            }
        }

        /// <inheritdoc/>
        public void RemoveCredentials(string userObjectGuid)
        {
            if (string.IsNullOrEmpty(userObjectGuid))
            {
                return;
            }

            string cacheKey = GetCacheKey(userObjectGuid);
            _cache.Remove(cacheKey);
            _logger.LogDebug("Removed cached credentials for user {UserObjectGuid}", userObjectGuid);
        }

        private static string GetCacheKey(string userObjectGuid)
        {
            return $"{CacheKeyPrefix}{userObjectGuid}";
        }
    }
}
