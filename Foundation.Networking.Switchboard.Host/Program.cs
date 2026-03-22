// ============================================================================
//
// Program.cs — Standalone Switchboard Host
//
// Admin API:
//   GET  /api/health                     — health check
//   GET  /api/switchboard/status         — load balancer status
//   POST /api/switchboard/select         — select a backend  (?clientIp=1.2.3.4)
//   GET  /api/switchboard/registry       — registered services
//   POST /api/switchboard/registry/register — register a service
//
// ============================================================================

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Switchboard;
using Foundation.Networking.Switchboard.Balancing;
using Foundation.Networking.Switchboard.Registry;

namespace Foundation.Networking.Switchboard.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSwitchboard(builder.Configuration);

            WebApplication app = builder.Build();

            MapAdminEndpoints(app);

            ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("SwitchboardHost");

            logger.LogInformation("═══════════════════════════════════════════════════");
            logger.LogInformation("  ⚡  Foundation Switchboard");
            logger.LogInformation("  Load balancer & service registry");
            logger.LogInformation("  Admin API: /api/switchboard/*");
            logger.LogInformation("═══════════════════════════════════════════════════");

            app.Run();
        }

        private static void MapAdminEndpoints(WebApplication app)
        {
            app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Service = "Switchboard" }));

            app.MapGet("/api/switchboard/status", (LoadBalancer lb) =>
            {
                return Results.Ok(new
                {
                    Strategy = lb.StrategyName,
                    TotalBackends = lb.TotalBackends,
                    HealthyBackends = lb.HealthyBackends
                });
            });

            app.MapPost("/api/switchboard/select", (HttpContext ctx, LoadBalancer lb) =>
            {
                string clientIp = ctx.Request.Query["clientIp"].ToString();

                BackendNode backend = lb.Select(clientIp);

                if (backend == null)
                {
                    return Results.StatusCode(503);
                }

                return Results.Ok(new
                {
                    backend.Id,
                    backend.Label,
                    backend.Url,
                    backend.ActiveConnections,
                    backend.TotalRequests
                });
            });

            app.MapGet("/api/switchboard/registry", (ServiceRegistry registry) =>
            {
                return Results.Ok(registry.GetAll());
            });

            app.MapPost("/api/switchboard/registry/register", async (HttpContext ctx, ServiceRegistry registry) =>
            {
                string serviceName = ctx.Request.Query["service"].ToString();
                string url = ctx.Request.Query["url"].ToString();

                if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(url))
                {
                    return Results.BadRequest("service and url parameters required");
                }

                ServiceInstance instance = new ServiceInstance
                {
                    ServiceName = serviceName,
                    Url = url
                };

                registry.Register(instance);

                return Results.Ok(new { instance.InstanceId, instance.ServiceName, instance.Url });
            });
        }
    }
}
