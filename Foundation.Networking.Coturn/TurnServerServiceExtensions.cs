// ============================================================================
//
// TurnServerServiceExtensions.cs — DI service registration for the TURN server.
//
// Follows the same pattern as:
//   Foundation.Telemetry.TelemetryServiceExtensions.AddTelemetryServices()
//   Foundation.Web.Services.Alerting.AlertingIntegrationExtensions.AddAlertingIntegration()
//
// Usage in Program.cs:
//
//   builder.Services.AddTurnServer(builder.Configuration);
//   ...
//   app.Services.UseTurnServer();
//
// ============================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Server;

namespace Foundation.Networking.Coturn
{
    /// <summary>
    ///
    /// Extension methods for registering the TURN server in the DI container.
    ///
    /// </summary>
    public static class TurnServerServiceExtensions
    {
        /// <summary>
        /// Section name in appsettings.json.
        /// </summary>
        public const string CONFIG_SECTION = "TurnServer";


        /// <summary>
        ///
        /// Registers the TURN server as a singleton in the DI container.
        /// Binds configuration from the "TurnServer" section of appsettings.json.
        ///
        /// Call this in Program.cs during service configuration.
        ///
        /// </summary>
        public static IServiceCollection AddTurnServer(this IServiceCollection services, IConfiguration configuration)
        {
            //
            // Bind the config section
            //
            services.Configure<TurnServerConfiguration>(configuration.GetSection(CONFIG_SECTION));

            //
            // Build the configuration object for the server constructor
            //
            TurnServerConfiguration turnConfig = new TurnServerConfiguration();
            configuration.GetSection(CONFIG_SECTION).Bind(turnConfig);

            //
            // Register the TurnServer as a singleton
            //
            services.AddSingleton<TurnServer>(sp =>
            {
                return new TurnServer(turnConfig);
            });

            return services;
        }


        /// <summary>
        ///
        /// Starts the TURN server.
        /// Call this in Program.cs after the application is built.
        ///
        /// </summary>
        public static void UseTurnServer(this IServiceProvider serviceProvider)
        {
            ILogger<TurnServer> logger = serviceProvider.GetService<ILogger<TurnServer>>();

            try
            {
                TurnServer server = serviceProvider.GetService<TurnServer>();

                if (server != null)
                {
                    server.Start();

                    logger?.LogInformation(
                        "TURN server started on {endpoint} (realm: {realm})",
                        server.ListenEndPoint,
                        "foundation");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to start TURN server");
            }
        }
    }
}
