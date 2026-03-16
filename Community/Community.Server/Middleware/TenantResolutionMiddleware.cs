using Foundation.Security.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace Foundation.Community.Middleware
{
    /// <summary>
    /// 
    /// Middleware that resolves the current tenant from the HTTP Host header.
    /// 
    /// Looks up SecurityTenant.hostName in the Security database, caches the result,
    /// and populates the scoped TenantContext for downstream use.
    /// 
    /// Requests to the public content API that cannot resolve a tenant will receive a 404.
    /// Requests to authenticated admin APIs (DataControllers) are unaffected — they use
    /// the tenant from the authenticated user's security context instead.
    /// 
    /// </summary>
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;

        //
        // Cache key prefix and duration for hostname → tenant lookups
        //
        private const string CACHE_KEY_PREFIX = "TenantByHost_";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(10);


        public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }


        public async Task InvokeAsync(HttpContext context, SecurityContext securityContext, TenantContext tenantContext, IMemoryCache cache)
        {
            //
            // Extract the hostname from the request (strip port)
            //
            string host = context.Request.Host.Host?.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(host))
            {
                await _next(context);
                return;
            }

            //
            // Try cache first
            //
            string cacheKey = CACHE_KEY_PREFIX + host;

            if (!cache.TryGetValue(cacheKey, out TenantCacheEntry entry))
            {
                //
                // Cache miss — query the Security database
                //
                var tenant = await securityContext.SecurityTenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.hostName != null
                        && t.hostName.ToLower() == host
                        && t.active
                        && !t.deleted);

                if (tenant != null)
                {
                    entry = new TenantCacheEntry
                    {
                        TenantGuid = tenant.objectGuid,
                        TenantId = tenant.id,
                        TenantName = tenant.name
                    };

                    _logger.LogInformation("Resolved tenant '{TenantName}' (guid: {TenantGuid}) for host '{Host}'.",
                        tenant.name, tenant.objectGuid, host);
                }
                else
                {
                    //
                    // No tenant found — cache a null entry to avoid repeated DB lookups
                    //
                    entry = null;

                    _logger.LogDebug("No tenant found for host '{Host}'.", host);
                }

                //
                // Cache regardless (null means "no tenant for this host")
                //
                cache.Set(cacheKey, entry, CACHE_DURATION);
            }

            //
            // Populate the scoped TenantContext if a tenant was resolved
            //
            if (entry != null)
            {
                tenantContext.TenantGuid = entry.TenantGuid;
                tenantContext.TenantId = entry.TenantId;
                tenantContext.TenantName = entry.TenantName;
            }

            await _next(context);
        }


        /// <summary>
        /// Lightweight cache entry to avoid holding full EF entities in memory.
        /// </summary>
        private class TenantCacheEntry
        {
            public Guid TenantGuid { get; set; }
            public int TenantId { get; set; }
            public string TenantName { get; set; }
        }
    }
}
