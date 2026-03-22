// ============================================================================
//
// BeaconServiceExtensions.cs — DI service registration for Beacon.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Beacon.Configuration;
using Foundation.Networking.Beacon.Discovery;
using Foundation.Networking.Beacon.Dns;

namespace Foundation.Networking.Beacon
{
    /// <summary>
    ///
    /// Extension methods for registering Beacon services in the DI container.
    ///
    /// </summary>
    public static class BeaconServiceExtensions
    {
        public const string CONFIG_SECTION = "Beacon";


        public static IServiceCollection AddBeacon(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BeaconConfiguration>(configuration.GetSection(CONFIG_SECTION));

            BeaconConfiguration config = new BeaconConfiguration();
            configuration.GetSection(CONFIG_SECTION).Bind(config);

            services.AddSingleton<BeaconConfiguration>(config);

            services.AddSingleton<DnsResolver>(sp =>
                new DnsResolver(config, sp.GetRequiredService<ILogger<DnsResolver>>()));

            services.AddSingleton<DnsZoneManager>(sp =>
                new DnsZoneManager(config));

            services.AddSingleton<ServiceDirectory>(sp =>
                new ServiceDirectory(config, sp.GetRequiredService<ILogger<ServiceDirectory>>()));

            services.AddHostedService<ServiceDirectory>(sp =>
                sp.GetRequiredService<ServiceDirectory>());

            return services;
        }
    }
}
