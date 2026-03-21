// ============================================================================
//
// WatchtowerServiceExtensions.cs — DI service registration for Watchtower.
//
// Follows the same pattern as:
//   Foundation.Networking.Coturn.TurnServerServiceExtensions
//
// Usage in Program.cs:
//
//   builder.Services.AddWatchtower(builder.Configuration);
//   // ... then after building:
//   // LatencyMonitorService starts automatically via IHostedService
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Watchtower.Configuration;
using Foundation.Networking.Watchtower.Services;

namespace Foundation.Networking.Watchtower
{
    /// <summary>
    ///
    /// Extension methods for registering Watchtower services in the DI container.
    ///
    /// </summary>
    public static class WatchtowerServiceExtensions
    {
        /// <summary>
        /// Section name in appsettings.json.
        /// </summary>
        public const string CONFIG_SECTION = "Watchtower";


        /// <summary>
        ///
        /// Registers all Watchtower services in the DI container.
        ///
        /// Binds configuration from the "Watchtower" section of appsettings.json.
        /// Registers PingService, TracerouteService, PortScannerService as singletons.
        /// Registers LatencyMonitorService as a hosted service (auto-starts).
        ///
        /// </summary>
        public static IServiceCollection AddWatchtower(this IServiceCollection services, IConfiguration configuration)
        {
            //
            // Bind the configuration
            //
            services.Configure<WatchtowerConfiguration>(configuration.GetSection(CONFIG_SECTION));

            WatchtowerConfiguration config = new WatchtowerConfiguration();
            configuration.GetSection(CONFIG_SECTION).Bind(config);

            //
            // Register the core services as singletons
            //
            services.AddSingleton<WatchtowerConfiguration>(config);

            services.AddSingleton<PingService>(sp =>
            {
                return new PingService(config);
            });

            services.AddSingleton<TracerouteService>(sp =>
            {
                return new TracerouteService(config);
            });

            services.AddSingleton<PortScannerService>(sp =>
            {
                return new PortScannerService(config);
            });

            //
            // Register the latency monitor as a hosted service
            // (it auto-starts when the host starts)
            //
            services.AddSingleton<LatencyMonitorService>(sp =>
            {
                return new LatencyMonitorService(
                    config,
                    sp.GetRequiredService<PingService>(),
                    sp.GetRequiredService<ILogger<LatencyMonitorService>>());
            });

            services.AddHostedService<LatencyMonitorService>(sp =>
            {
                return sp.GetRequiredService<LatencyMonitorService>();
            });

            return services;
        }
    }
}
