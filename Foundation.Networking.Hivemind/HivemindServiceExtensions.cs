// ============================================================================
//
// HivemindServiceExtensions.cs — DI service registration for Hivemind.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Foundation.Networking.Hivemind.Cache;
using Foundation.Networking.Hivemind.Configuration;
using Foundation.Networking.Hivemind.PubSub;
using Foundation.Networking.Hivemind.Sessions;

namespace Foundation.Networking.Hivemind
{
    /// <summary>
    ///
    /// Extension methods for registering Hivemind services in the DI container.
    ///
    /// </summary>
    public static class HivemindServiceExtensions
    {
        /// <summary>
        /// Section name in appsettings.json.
        /// </summary>
        public const string CONFIG_SECTION = "Hivemind";


        /// <summary>
        ///
        /// Registers all Hivemind services in the DI container.
        ///
        /// </summary>
        public static IServiceCollection AddHivemind(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<HivemindConfiguration>(configuration.GetSection(CONFIG_SECTION));

            HivemindConfiguration config = new HivemindConfiguration();
            configuration.GetSection(CONFIG_SECTION).Bind(config);

            services.AddSingleton<HivemindConfiguration>(config);
            services.AddSingleton<DistributedCache>(sp => new DistributedCache(config));
            services.AddSingleton<SessionStore>(sp => new SessionStore(config));
            services.AddSingleton<MessageBus>(sp => new MessageBus(config));

            return services;
        }
    }
}
