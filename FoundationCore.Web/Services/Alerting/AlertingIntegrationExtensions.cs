//
// Alerting Integration Service Collection Extensions
//
// DI registration extension for easy integration setup.
//
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Web.Services.Alerting
{
    /// <summary>
    /// Extension methods for registering Alerting integration services.
    /// </summary>
    public static class AlertingIntegrationExtensions
    {
        /// <summary>
        /// 
        /// Add Alerting integration services to the DI container.
        /// 
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">Configuration containing "Alerting" section.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddAlertingIntegration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind configuration
            services.Configure<AlertingIntegrationOptions>(configuration.GetSection(AlertingIntegrationOptions.SectionName));

            // Register HTTP client with retry policy
            services.AddHttpClient<IAlertingIntegrationService, AlertingIntegrationService>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var options = configuration.GetSection(AlertingIntegrationOptions.SectionName)
                        .Get<AlertingIntegrationOptions>();
                    
                    if (!string.IsNullOrEmpty(options?.BaseUrl))
                    {
                        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                    }

                    client.Timeout = TimeSpan.FromSeconds(options?.TimeoutSeconds ?? 30);
                });

            return services;
        }

        /// <summary>
        /// Add Alerting integration services with custom configuration action.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Configuration action.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddAlertingIntegration(this IServiceCollection services, Action<AlertingIntegrationOptions> configure)
        {
            services.Configure(configure);

            services.AddHttpClient<IAlertingIntegrationService, AlertingIntegrationService>();

            return services;
        }
    }
}
