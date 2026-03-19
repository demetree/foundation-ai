//
// Program.cs — Scheduler WebDAV Server
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Standalone Kestrel server that exposes the Scheduler's file storage as a
// WebDAV filesystem.  Uses HTTP Basic Auth validated against the same Security
// database as the main Scheduler server.  Tenant is automatically resolved
// from the authenticated user — no tenant GUID in the URL.
//
// Designed to run behind Cloudflare (e.g., files.k2research.ca) on its own port.
//
using Foundation;
using Foundation.Scheduler.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scheduler.Server.Services;
using Scheduler.WebDAV.Middleware;
using System;
using System.IO;
using System.Reflection;
using static Foundation.Configuration;
using static Foundation.StartupBasics;

namespace Scheduler.WebDAV
{
    public class Program
    {
        private const string LOG_DIRECTORY = "Log";
        private const string LOG_FILENAME = "WebDAV";


        private static async System.Threading.Tasks.Task Main(string[] args)
        {
            IConfigurationRoot config = GetConfiguration();

            Logger logger = SetupLogger(config);

            logger.LogSystem("Scheduler WebDAV Server is starting.");

            //
            // Configure the system auditor
            //
            ConfigureAuditor();

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            try
            {
                //
                // Configure Kestrel
                //
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = 1073741824; // 1 GB
                });

                //
                // Add the logger to the pipeline
                //
                builder.Services.AddLogging(b =>
                {
                    b.AddProvider(logger);
                });

                //
                // Configure the Foundation services (Security, Auditor, OIDC infrastructure)
                //
                BuildFoundationServices(builder, logger);

                //
                // Add the Scheduler Database context
                //
                builder.Services.AddDbContext<SchedulerContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("Scheduler"))
                           .UseLazyLoadingProxies(false)
                           .AddInterceptors(UtcDateTimeInterceptor.Instance);
                });

                //
                // Register File Storage Service
                //
                builder.Services.AddScoped<IFileStorageService, SqlFileStorageService>();

                //
                // Register the credential cache for Basic Auth (in-memory, 5-minute TTL)
                //
                builder.Services.AddSingleton<BasicAuthCredentialCache>();


                builder.Services.AddSingleton<Foundation.Logger>(logger);

                logger.LogInformation("Services have been configured.");


                var app = builder.Build();

                logger.LogInformation("Completed building web application.");


                //
                // Middleware pipeline — order matters
                //
                // 1. Basic Auth (every request must be authenticated)
                // 2. WebDAV method routing
                //
                app.UseMiddleware<BasicAuthMiddleware>();
                app.UseMiddleware<WebDavMiddleware>();


                //
                // Run
                //
                logger.LogSystem("About to run Scheduler WebDAV server.");

                app.Run();
            }
            catch (Exception ex)
            {
                logger.LogException($"WebDAV server is exiting because of exception {ex.Message}", ex);
            }
            finally
            {
                Logger.TerminateApplicationLogging();
            }
        }


        private static Logger SetupLogger(IConfigurationRoot config)
        {
            Logger logger = new Logger();

            Logger.SetCommonLogger(logger);

            string logLevel;
            try
            {
                logLevel = config.GetValue<string>("Logging:LogLevel:Default");
            }
            catch
            {
                logLevel = "Information";
            }

            Logger.LogLevels logLevelToUse;

            if (Logger.LogLevelFromString(logLevel, out logLevelToUse) == false)
            {
                logLevelToUse = Logger.LogLevels.Information;
            }

            logger.Level = logLevelToUse;

            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";

            logger.SetDirectory(Path.Combine(currentPath, LOG_DIRECTORY));
            logger.SetFileName(LOG_FILENAME);

            return logger;
        }
    }
}
