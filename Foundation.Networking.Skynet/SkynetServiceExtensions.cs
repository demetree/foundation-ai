// ============================================================================
//
// SkynetServiceExtensions.cs — DI service registration for Skynet.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Skynet.Configuration;
using Foundation.Networking.Skynet.Firewall;
using Foundation.Networking.Skynet.Logging;
using Foundation.Networking.Skynet.Proxy;

namespace Foundation.Networking.Skynet
{
    /// <summary>
    ///
    /// Extension methods for registering Skynet services in the DI container.
    ///
    /// </summary>
    public static class SkynetServiceExtensions
    {
        /// <summary>
        /// Section name in appsettings.json.
        /// </summary>
        public const string CONFIG_SECTION = "Skynet";


        /// <summary>
        ///
        /// Registers all Skynet services in the DI container.
        ///
        /// </summary>
        public static IServiceCollection AddSkynet(this IServiceCollection services, IConfiguration configuration)
        {
            //
            // Bind the configuration
            //
            services.Configure<SkynetConfiguration>(configuration.GetSection(CONFIG_SECTION));

            SkynetConfiguration config = new SkynetConfiguration();
            configuration.GetSection(CONFIG_SECTION).Bind(config);

            //
            // Register configuration and services
            //
            services.AddSingleton<SkynetConfiguration>(config);

            services.AddSingleton<FirewallEngine>(sp =>
            {
                return new FirewallEngine(config);
            });

            services.AddSingleton<ThreatLog>(sp =>
            {
                return new ThreatLog(config);
            });

            services.AddSingleton<BackendPool>(sp =>
            {
                return new BackendPool(
                    config,
                    sp.GetRequiredService<ILogger<BackendPool>>());
            });

            return services;
        }
    }
}
