using Foundation.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenIddict.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Security.OIDC
{
    /// <summary>
    /// 
    /// A caching subclass of the OpenIddict EF Core application store.
    /// 
    /// Overrides FindByClientIdAsync to use the Foundation MemoryCacheManager (process-global, singleton)
    /// so that client ID lookups are cached across requests.  OpenIddict's built-in entity cache is 
    /// scoped per-request, meaning every token exchange/refresh hits the database.  This store eliminates
    /// that database round-trip for the most common lookup.
    ///
    /// Currently caches for 30 minutes — since foundation system generally only have 2 client applications (<application>_spa, swagger_ui) are common.
    /// that are registered at startup and never change, this is effectively a warm lookup for the lifetime
    /// of the process.
    ///
    /// Safety: SecurityContext is configured with UseLazyLoadingProxies(false), so cached entities are
    /// plain POCOs with no proxy references.  FindByClientIdAsync results are only read (never tracked 
    /// or modified) by the OpenIddict validation pipeline.
    /// 
    /// </summary>
    public class CachingOpenIddictApplicationStore : OpenIddictEntityFrameworkCoreApplicationStore
    {
        private const float CACHE_MINUTES = 30f;
        private const string CACHE_PREFIX = "OIDC_FindByClientId_";

        public CachingOpenIddictApplicationStore(
            IMemoryCache cache,
            IOpenIddictEntityFrameworkCoreContext context,
            IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options)
            : base(cache, context, options)
        {
        }


        /// <summary>
        /// Overrides the default FindByClientIdAsync to check the process-global MemoryCacheManager 
        /// before hitting the database.  On a cache miss, delegates to the base EF Core implementation
        /// and caches the result.
        /// </summary>
        public override async ValueTask<OpenIddictEntityFrameworkCoreApplication?> FindByClientIdAsync(
            string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier) == true)
            {
                return await base.FindByClientIdAsync(identifier, cancellationToken);
            }

            MemoryCacheManager mcm = new MemoryCacheManager();

            string cacheKey = CACHE_PREFIX + identifier;

            if (mcm.IsSet(cacheKey) == true)
            {
                return mcm.Get<OpenIddictEntityFrameworkCoreApplication>(cacheKey);
            }

            OpenIddictEntityFrameworkCoreApplication result = await base.FindByClientIdAsync(identifier, cancellationToken);

            if (result != null)
            {
                mcm.Set(cacheKey, result, CACHE_MINUTES);
            }

            return result;
        }
    }
}
