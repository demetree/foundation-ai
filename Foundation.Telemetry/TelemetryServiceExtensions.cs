using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Foundation.Services;

namespace Foundation.Telemetry
{
    /// <summary>
    /// Extension methods for registering Telemetry services in the DI container.
    /// </summary>
    public static class TelemetryServiceExtensions
    {
        /// <summary>
        /// Adds Telemetry collection services to the service collection.
        /// Call this in Program.cs after AddFoundationServices().
        /// </summary>
        public static IServiceCollection AddTelemetryServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind configuration
            services.Configure<TelemetryConfiguration>(configuration.GetSection("Telemetry"));

            // Register the collector service as singleton
            services.AddSingleton<TelemetryCollectorService>();

            // Register log error provider for log file parsing
            // Uses Telemetry:LogDirectory if set, otherwise falls back to first LogViewer folder
            var logDirectory = configuration.GetValue<string>("Telemetry:LogDirectory");
            if (string.IsNullOrEmpty(logDirectory))
            {
                // Fall back to first LogViewer folder if configured
                var logFolders = configuration.GetSection("LogViewer:LogFolders").GetChildren();
                foreach (var folder in logFolders)
                {
                    logDirectory = folder.GetValue<string>("Path");
                    if (!string.IsNullOrEmpty(logDirectory))
                        break;
                }
            }
            
            // Default to "Log" if nothing configured
            logDirectory ??= "Log";

            services.AddSingleton<ILogErrorProvider>(sp =>
                new FileLogErrorProvider(
                    sp.GetRequiredService<ILogger<FileLogErrorProvider>>(),
                    logDirectory
                ));

            return services;
        }

        /// <summary>
        /// Initializes and starts the Telemetry collector.
        /// Call this in Program.cs after the application is built.
        /// </summary>
        public static void UseTelemetryCollector(this IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<TelemetryCollectorService>>();

            try
            {
                var collector = serviceProvider.GetService<TelemetryCollectorService>();
                collector?.Initialize();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to initialize Telemetry Collector");
            }
        }
    }
}
