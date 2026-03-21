// ============================================================================
//
// LocksmithServiceExtensions.cs — DI service registration for Locksmith.
//
// Follows the same pattern as:
//   Foundation.Networking.Coturn.TurnServerServiceExtensions
//   Foundation.Networking.Watchtower.WatchtowerServiceExtensions
//
// Usage in Program.cs:
//
//   builder.Services.AddLocksmith(builder.Configuration);
//   // CertificateMonitorService starts automatically via IHostedService
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Locksmith.Configuration;
using Foundation.Networking.Locksmith.Services;

namespace Foundation.Networking.Locksmith
{
    /// <summary>
    ///
    /// Extension methods for registering Locksmith services in the DI container.
    ///
    /// </summary>
    public static class LocksmithServiceExtensions
    {
        /// <summary>
        /// Section name in appsettings.json.
        /// </summary>
        public const string CONFIG_SECTION = "Locksmith";


        /// <summary>
        ///
        /// Registers all Locksmith services in the DI container.
        ///
        /// Binds configuration from the "Locksmith" section of appsettings.json.
        /// Registers CertificateInspector as a singleton.
        /// Registers CertificateMonitorService as a hosted service (auto-starts).
        ///
        /// </summary>
        public static IServiceCollection AddLocksmith(this IServiceCollection services, IConfiguration configuration)
        {
            //
            // Bind the configuration
            //
            services.Configure<LocksmithConfiguration>(configuration.GetSection(CONFIG_SECTION));

            LocksmithConfiguration config = new LocksmithConfiguration();
            configuration.GetSection(CONFIG_SECTION).Bind(config);

            //
            // Register the configuration and core services
            //
            services.AddSingleton<LocksmithConfiguration>(config);

            services.AddSingleton<CertificateInspector>(sp =>
            {
                return new CertificateInspector(config);
            });

            //
            // Register the certificate monitor as a hosted service
            //
            services.AddSingleton<CertificateMonitorService>(sp =>
            {
                return new CertificateMonitorService(
                    config,
                    sp.GetRequiredService<CertificateInspector>(),
                    sp.GetRequiredService<ILogger<CertificateMonitorService>>());
            });

            services.AddHostedService<CertificateMonitorService>(sp =>
            {
                return sp.GetRequiredService<CertificateMonitorService>();
            });

            return services;
        }
    }
}
