using Foundation.Cache;
using Foundation.Messaging;
using Foundation.Security.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Scheduler.Services
{
    /// <summary>
    /// 
    /// Caching decorator for IMessagingUserResolver that wraps the underlying resolver
    /// (typically CatalystUserResolver) with a MemoryCacheManager layer.
    /// 
    /// User profile data changes infrequently, so caching with a 10-minute TTL 
    /// eliminates the majority of database queries across all messaging services
    /// (ConversationService, PresenceService, NotificationService).
    /// 
    /// Cache keys:
    ///   msg_user:{tenantGuid}:{userId}           → MessagingUser by ID
    ///   msg_user_acct:{tenantGuid}:{accountName}  → MessagingUser by account name
    ///   msg_user_sec:{tenantGuid}:{objectGuid}    → MessagingUser by SecurityUser objectGuid
    /// 
    /// Invalidation:
    ///   Call InvalidateUser() when a user profile is updated.
    ///   Call InvalidateAllUsersForTenant() for bulk operations.
    ///   TTL (10 min) provides natural staleness protection.
    /// 
    /// </summary>
    public class CachingMessagingUserResolver : IMessagingUserResolver
    {
        private readonly IMessagingUserResolver _inner;
        private readonly MemoryCacheManager _cache;

        private const float CACHE_TTL_MINUTES = 10f;
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);


        public CachingMessagingUserResolver(IMessagingUserResolver inner)
        {
            _inner = inner;
            _cache = new MemoryCacheManager();
        }



        public async Task<MessagingUser> GetUserAsync(SecurityUser securityUser)
        {
            if (securityUser == null) return null;

            string cacheKey = $"msg_user_sec:{securityUser.securityTenant?.objectGuid}:{securityUser.objectGuid}";

            if (_cache.IsSet(cacheKey))
            {
                return _cache.Get<MessagingUser>(cacheKey);
            }

            await _cacheLock.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (_cache.IsSet(cacheKey))
                {
                    return _cache.Get<MessagingUser>(cacheKey);
                }

                MessagingUser user = await _inner.GetUserAsync(securityUser);

                if (user != null)
                {
                    _cache.Set(cacheKey, user, CACHE_TTL_MINUTES);

                    //
                    // Also cache by ID and account name so subsequent lookups are cached too
                    //
                    string idKey = $"msg_user:{user.tenantGuid}:{user.id}";
                    string acctKey = $"msg_user_acct:{user.tenantGuid}:{user.accountName}";

                    if (!_cache.IsSet(idKey)) _cache.Set(idKey, user, CACHE_TTL_MINUTES);
                    if (!_cache.IsSet(acctKey)) _cache.Set(acctKey, user, CACHE_TTL_MINUTES);
                }

                return user;
            }
            finally
            {
                _cacheLock.Release();
            }
        }



        public async Task<MessagingUser> GetUserByIdAsync(int userId, Guid tenantGuid)
        {
            string cacheKey = $"msg_user:{tenantGuid}:{userId}";

            if (_cache.IsSet(cacheKey))
            {
                return _cache.Get<MessagingUser>(cacheKey);
            }

            await _cacheLock.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (_cache.IsSet(cacheKey))
                {
                    return _cache.Get<MessagingUser>(cacheKey);
                }

                MessagingUser user = await _inner.GetUserByIdAsync(userId, tenantGuid);

                if (user != null)
                {
                    _cache.Set(cacheKey, user, CACHE_TTL_MINUTES);

                    //
                    // Cross-populate the account name cache
                    //
                    string acctKey = $"msg_user_acct:{tenantGuid}:{user.accountName}";
                    if (!_cache.IsSet(acctKey)) _cache.Set(acctKey, user, CACHE_TTL_MINUTES);
                }

                return user;
            }
            finally
            {
                _cacheLock.Release();
            }
        }



        public async Task<MessagingUser> GetUserByAccountNameAsync(string accountName, Guid tenantGuid)
        {
            string cacheKey = $"msg_user_acct:{tenantGuid}:{accountName}";

            if (_cache.IsSet(cacheKey))
            {
                return _cache.Get<MessagingUser>(cacheKey);
            }

            await _cacheLock.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (_cache.IsSet(cacheKey))
                {
                    return _cache.Get<MessagingUser>(cacheKey);
                }

                MessagingUser user = await _inner.GetUserByAccountNameAsync(accountName, tenantGuid);

                if (user != null)
                {
                    _cache.Set(cacheKey, user, CACHE_TTL_MINUTES);

                    //
                    // Cross-populate the ID cache
                    //
                    string idKey = $"msg_user:{tenantGuid}:{user.id}";
                    if (!_cache.IsSet(idKey)) _cache.Set(idKey, user, CACHE_TTL_MINUTES);
                }

                return user;
            }
            finally
            {
                _cacheLock.Release();
            }
        }



        public async Task<List<MessagingUser>> GetUsersByIdsAsync(List<int> userIds, Guid tenantGuid)
        {
            if (userIds == null || userIds.Count == 0) return new List<MessagingUser>();

            List<MessagingUser> results = new List<MessagingUser>();
            List<int> uncachedIds = new List<int>();

            //
            // Check cache for each ID
            //
            foreach (int userId in userIds)
            {
                string cacheKey = $"msg_user:{tenantGuid}:{userId}";
                var cached = _cache.Get<MessagingUser>(cacheKey);
                if (cached != null)
                {
                    results.Add(cached);
                }
                else
                {
                    uncachedIds.Add(userId);
                }
            }

            //
            // Batch-resolve any cache misses
            //
            if (uncachedIds.Count > 0)
            {
                List<MessagingUser> resolved = await _inner.GetUsersByIdsAsync(uncachedIds, tenantGuid);

                foreach (MessagingUser user in resolved)
                {
                    string idKey = $"msg_user:{tenantGuid}:{user.id}";
                    string acctKey = $"msg_user_acct:{tenantGuid}:{user.accountName}";

                    _cache.Set(idKey, user, CACHE_TTL_MINUTES);
                    _cache.Set(acctKey, user, CACHE_TTL_MINUTES);

                    results.Add(user);
                }
            }

            return results;
        }



        #region Cache Invalidation


        /// <summary>
        /// Invalidates all cached entries for a specific user.
        /// Call this when a user profile is updated (display name change, etc.).
        /// </summary>
        public void InvalidateUser(int userId, Guid tenantGuid)
        {
            _cache.RemoveByPattern($"msg_user:{tenantGuid}:{userId}");
            _cache.RemoveByPattern($"msg_user_acct:{tenantGuid}:.*");
            _cache.RemoveByPattern($"msg_user_sec:{tenantGuid}:.*");
        }


        /// <summary>
        /// Invalidates all cached user entries for a tenant.
        /// Call this for bulk operations (e.g., admin user imports).
        /// </summary>
        public void InvalidateAllUsersForTenant(Guid tenantGuid)
        {
            _cache.RemoveByPattern($"msg_user.*:{tenantGuid}:.*");
        }


        #endregion


        /// <summary>
        /// Resolves a SecurityUser from a MessagingUser ID.
        /// Not cached — SecurityUser settings may change frequently.
        /// </summary>
        public Task<SecurityUser> GetSecurityUserByMessagingUserIdAsync(int messagingUserId, Guid tenantGuid)
        {
            return _inner.GetSecurityUserByMessagingUserIdAsync(messagingUserId, tenantGuid);
        }


        /// <summary>
        /// Returns all users for a tenant.  Not cached — called infrequently (People Directory panel load).
        /// </summary>
        public Task<List<MessagingUser>> GetAllUsersAsync(Guid tenantGuid)
        {
            return _inner.GetAllUsersAsync(tenantGuid);
        }


        /// <summary>
        /// Searches users by name/account.  Not cached — search terms vary.
        /// </summary>
        public Task<List<MessagingUser>> SearchUsersAsync(string searchTerm, Guid tenantGuid, int maxResults = 20)
        {
            return _inner.SearchUsersAsync(searchTerm, tenantGuid, maxResults);
        }
    }
}
