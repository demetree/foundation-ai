// ============================================================================
//
// DeepSpaceServiceExtensions.cs — DI service registration for Deep Space.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.DeepSpace.Configuration;
using Foundation.Networking.DeepSpace.Providers;

namespace Foundation.Networking.DeepSpace
{
    /// <summary>
    ///
    /// Extension methods for registering Deep Space services in the DI container.
    ///
    /// </summary>
    public static class DeepSpaceServiceExtensions
    {
        /// <summary>
        /// Section name in appsettings.json.
        /// </summary>
        public const string CONFIG_SECTION = "DeepSpace";


        /// <summary>
        ///
        /// Registers all Deep Space services in the DI container.
        ///
        /// </summary>
        public static IServiceCollection AddDeepSpace(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DeepSpaceConfiguration>(configuration.GetSection(CONFIG_SECTION));

            DeepSpaceConfiguration config = new DeepSpaceConfiguration();
            configuration.GetSection(CONFIG_SECTION).Bind(config);

            services.AddSingleton<DeepSpaceConfiguration>(config);

            //
            // Register the local storage provider
            //
            services.AddSingleton<LocalStorageProvider>(sp =>
            {
                return new LocalStorageProvider(config.LocalStorage);
            });

            //
            // Register the storage manager and auto-register local provider
            //
            services.AddSingleton<StorageManager>(sp =>
            {
                StorageManager manager = new StorageManager(
                    config,
                    sp.GetRequiredService<ILogger<StorageManager>>());

                manager.RegisterProvider(sp.GetRequiredService<LocalStorageProvider>());

                return manager;
            });

            return services;
        }
    }
}
