//
// SalesforceServiceRegistration.cs
//
// Extension method to register all Salesforce integration services with DI.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration.
//

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Scheduler.Salesforce.Api;
using Scheduler.Salesforce.Auth;
using Scheduler.Salesforce.Sync;


namespace Foundation.Scheduler.Services.Salesforce
{
    public static class SalesforceServiceRegistration
    {
        /// <summary>
        ///
        /// Registers all Salesforce integration services including:
        ///   - SalesforceClient (typed HttpClient)
        ///   - SalesforceWebApiService (typed HttpClient)
        ///   - TokenCacheService (singleton)
        ///   - SalesforceSyncService (scoped — needs SchedulerContext)
        ///   - SalesforcePeriodicPullService (hosted background service)
        ///   - SalesforceSyncQueueProcessor (hosted background service)
        ///
        /// </summary>
        public static IServiceCollection AddSalesforceIntegration(this IServiceCollection services)
        {
            // Auth — singleton (IMemoryCache is already registered by ASP.NET Core)
            services.AddSingleton<ITokenCacheService, TokenCacheService>();

            // Data protection for credential encryption
            services.AddDataProtection();
            services.AddSingleton<SalesforceCredentialProtector>();

            // SaveChanges interceptor — auto-enqueues sync operations for Client/Contact/ScheduledEvent writes
            services.AddSingleton<SalesforceSyncInterceptor>();

            // API layer — typed HttpClients
            services.AddHttpClient<SalesforceClient>();
            services.AddHttpClient<SalesforceWebApiService>();

            // Core sync service — scoped (needs SchedulerContext)
            services.AddScoped<SalesforceSyncService>();

            // Background services
            services.AddHostedService<SalesforcePeriodicPullService>();
            services.AddHostedService<SalesforceSyncQueueProcessor>();

            return services;
        }
    }
}
