//
// Session Cache Service Collection Extensions
//
// Extension method to register the IndexedDB-backed session cache
// in any Foundation app's service collection. Call this after
// BuildFoundationServices to replace the default SessionTrackingService
// with the CachedSessionTrackingService decorator.
//
// AI-assisted development - February 2026
//
using Foundation.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Web.Services
{
    /// <summary>
    /// Extension methods for registering the session cache services.
    /// </summary>
    public static class SessionCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Replaces the default <see cref="SessionTrackingService"/> registration with a 
        /// <see cref="CachedSessionTrackingService"/> decorator that uses local IndexedDB 
        /// for fast per-request session validation.
        ///
        /// Call this AFTER <c>StartupBasics.BuildFoundationServices</c> to override the 
        /// default scoped <see cref="ISessionTrackingService"/> registration.
        /// </summary>
        public static IServiceCollection AddSessionCache(this IServiceCollection services)
        {
            //
            // 1. Local session event buffer (singleton — persists across scoped lifetimes)
            //
            services.AddSingleton<ISessionEventBuffer, SessionEventBuffer>();

            //
            // 2. Inner service (scoped — SessionTrackingService already registered by StartupBasics,
            //    but we need a concrete type registration for the decorator to inject)
            //
            services.AddScoped<SessionTrackingService>();

            //
            // 3. Replace the ISessionTrackingService registration with the cached decorator.
            //    This overrides the one set in StartupBasics.BuildFoundationServices.
            //    In the Microsoft DI container, later registrations win for the same interface.
            //
            services.AddScoped<ISessionTrackingService, CachedSessionTrackingService>();

            //
            // 4. Background cleanup worker
            //
            services.AddHostedService<SessionCacheSyncWorker>();

            return services;
        }
    }
}
