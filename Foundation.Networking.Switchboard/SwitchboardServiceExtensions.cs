// ============================================================================
//
// SwitchboardServiceExtensions.cs — DI service registration for Switchboard.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Switchboard.Balancing;
using Foundation.Networking.Switchboard.Configuration;
using Foundation.Networking.Switchboard.Health;
using Foundation.Networking.Switchboard.Registry;

namespace Foundation.Networking.Switchboard
{
    /// <summary>
    ///
    /// Extension methods for registering Switchboard services in the DI container.
    ///
    /// </summary>
    public static class SwitchboardServiceExtensions
    {
        /// <summary>
        /// Section name in appsettings.json.
        /// </summary>
        public const string CONFIG_SECTION = "Switchboard";


        /// <summary>
        ///
        /// Registers all Switchboard services in the DI container.
        ///
        /// </summary>
        public static IServiceCollection AddSwitchboard(this IServiceCollection services, IConfiguration configuration)
        {
            //
            // Bind the configuration
            //
            services.Configure<SwitchboardConfiguration>(configuration.GetSection(CONFIG_SECTION));

            SwitchboardConfiguration config = new SwitchboardConfiguration();
            configuration.GetSection(CONFIG_SECTION).Bind(config);

            //
            // Register configuration
            //
            services.AddSingleton<SwitchboardConfiguration>(config);

            //
            // Register the load balancer
            //
            services.AddSingleton<LoadBalancer>(sp =>
            {
                return new LoadBalancer(config);
            });

            //
            // Register the service registry (linked to the load balancer)
            //
            services.AddSingleton<ServiceRegistry>(sp =>
            {
                return new ServiceRegistry(sp.GetRequiredService<LoadBalancer>());
            });

            //
            // Register the health prober as a hosted service
            //
            services.AddSingleton<HealthProber>(sp =>
            {
                return new HealthProber(
                    config,
                    sp.GetRequiredService<LoadBalancer>(),
                    sp.GetRequiredService<ILogger<HealthProber>>());
            });

            services.AddHostedService<HealthProber>(sp =>
            {
                return sp.GetRequiredService<HealthProber>();
            });

            return services;
        }
    }
}
