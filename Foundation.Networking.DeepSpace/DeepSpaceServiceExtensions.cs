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
            // Conditionally register the S3 provider
            //
            if (string.IsNullOrEmpty(config.S3Storage.BucketName) == false)
            {
                services.AddSingleton<S3StorageProvider>(sp =>
                {
                    return new S3StorageProvider(config.S3Storage);
                });
            }

            //
            // Conditionally register the Azure Blob provider
            //
            if (string.IsNullOrEmpty(config.AzureBlob.ConnectionString) == false)
            {
                services.AddSingleton<AzureBlobStorageProvider>(sp =>
                {
                    return new AzureBlobStorageProvider(config.AzureBlob);
                });
            }

            //
            // Register the storage manager and auto-register all available providers
            //
            services.AddSingleton<StorageManager>(sp =>
            {
                StorageManager manager = new StorageManager(
                    config,
                    sp.GetRequiredService<ILogger<StorageManager>>());

                manager.RegisterProvider(sp.GetRequiredService<LocalStorageProvider>());

                //
                // Register optional providers if they were configured
                //
                S3StorageProvider s3 = sp.GetService<S3StorageProvider>();
                if (s3 != null)
                {
                    manager.RegisterProvider(s3);
                }

                AzureBlobStorageProvider azure = sp.GetService<AzureBlobStorageProvider>();
                if (azure != null)
                {
                    manager.RegisterProvider(azure);
                }

                return manager;
            });

            return services;
        }
    }
}

