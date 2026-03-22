// ============================================================================
//
// ConduitServiceExtensions.cs — DI service registration for Conduit.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Foundation.Networking.Conduit.Channels;
using Foundation.Networking.Conduit.Configuration;
using Foundation.Networking.Conduit.Connections;

namespace Foundation.Networking.Conduit
{
    /// <summary>
    ///
    /// Extension methods for registering Conduit services in the DI container.
    ///
    /// </summary>
    public static class ConduitServiceExtensions
    {
        public const string CONFIG_SECTION = "Conduit";


        public static IServiceCollection AddConduit(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConduitConfiguration>(configuration.GetSection(CONFIG_SECTION));

            ConduitConfiguration config = new ConduitConfiguration();
            configuration.GetSection(CONFIG_SECTION).Bind(config);

            services.AddSingleton<ConduitConfiguration>(config);
            services.AddSingleton<ConnectionManager>(sp => new ConnectionManager(config));
            services.AddSingleton<ChannelManager>(sp =>
                new ChannelManager(config, sp.GetRequiredService<ConnectionManager>()));

            return services;
        }
    }
}
