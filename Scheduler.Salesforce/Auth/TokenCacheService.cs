//
// TokenCacheService.cs
//
// In-memory per-tenant token caching for Salesforce OAuth tokens.
// Uses IMemoryCache with per-tenant cache keys.
//
// AI Assisted Development:  This file was ported from the Compactica SalesForceIntegration project
// and adapted for multi-tenant Salesforce support.
//

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;


namespace Scheduler.Salesforce.Auth
{
    public class TokenCacheService : ITokenCacheService
    {
        private readonly IMemoryCache _cache;
        private const string TokenKeyPrefix = "SF_Token_";
        private const string InstanceUrlKeyPrefix = "SF_InstanceUrl_";


        public TokenCacheService(IMemoryCache memoryCache)
        {
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }


        public Task<string> GetAccessTokenAsync(Guid tenantGuid)
        {
            string key = TokenKeyPrefix + tenantGuid.ToString();

            if (_cache.TryGetValue(key, out string token) == true)
            {
                return Task.FromResult(token);
            }

            return Task.FromResult<string>(null);
        }


        public Task SetAccessTokenAsync(Guid tenantGuid, string token, DateTime expiration)
        {
            string key = TokenKeyPrefix + tenantGuid.ToString();
            _cache.Set(key, token, expiration);
            return Task.CompletedTask;
        }


        public Task<string> GetInstanceUrlAsync(Guid tenantGuid)
        {
            string key = InstanceUrlKeyPrefix + tenantGuid.ToString();

            if (_cache.TryGetValue(key, out string instanceUrl) == true)
            {
                return Task.FromResult(instanceUrl);
            }

            return Task.FromResult<string>(null);
        }


        public Task SetInstanceUrlAsync(Guid tenantGuid, string instanceUrl)
        {
            string key = InstanceUrlKeyPrefix + tenantGuid.ToString();
            _cache.Set(key, instanceUrl, TimeSpan.FromHours(4));
            return Task.CompletedTask;
        }


        public Task ClearTokenAsync(Guid tenantGuid)
        {
            _cache.Remove(TokenKeyPrefix + tenantGuid.ToString());
            _cache.Remove(InstanceUrlKeyPrefix + tenantGuid.ToString());
            return Task.CompletedTask;
        }
    }
}
