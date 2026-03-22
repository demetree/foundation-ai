// ============================================================================
//
// Program.cs — Standalone Skynet Host
//
// Admin API:
//   GET  /api/health                 — health check
//   POST /api/skynet/evaluate        — evaluate a request (?ip=1.2.3.4&path=/api/test)
//   GET  /api/skynet/threats         — threat log summary
//   GET  /api/skynet/threats/recent  — recent threat entries
//   GET  /api/skynet/backends        — backend pool status
//
// ============================================================================

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Skynet;
using Foundation.Networking.Skynet.Firewall;
using Foundation.Networking.Skynet.Logging;
using Foundation.Networking.Skynet.Proxy;

namespace Foundation.Networking.Skynet.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSkynet(builder.Configuration);

            WebApplication app = builder.Build();

            MapAdminEndpoints(app);

            ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("SkynetHost");

            logger.LogInformation("═══════════════════════════════════════════════════");
            logger.LogInformation("  ☢️  Foundation Skynet");
            logger.LogInformation("  Edge proxy, firewall & rate limiting");
            logger.LogInformation("  Admin API: /api/skynet/*");
            logger.LogInformation("═══════════════════════════════════════════════════");

            app.Run();
        }

        private static void MapAdminEndpoints(WebApplication app)
        {
            app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Service = "Skynet" }));

            app.MapPost("/api/skynet/evaluate", (HttpContext ctx, FirewallEngine firewall) =>
            {
                string ip = ctx.Request.Query["ip"].ToString();
                string path = ctx.Request.Query["path"].ToString();
                string country = ctx.Request.Query["country"].ToString();

                if (string.IsNullOrEmpty(ip)) return Results.BadRequest("ip parameter required");

                FirewallDecision decision = firewall.Evaluate(ip, path ?? "/", country ?? "");
                return Results.Ok(decision);
            });

            app.MapGet("/api/skynet/threats", (ThreatLog log) =>
            {
                return Results.Ok(log.GetSummary());
            });

            app.MapGet("/api/skynet/threats/recent", (HttpContext ctx, ThreatLog log) =>
            {
                int count = 50;
                if (int.TryParse(ctx.Request.Query["count"], out int c)) count = c;

                return Results.Ok(log.GetRecent(count));
            });

            app.MapGet("/api/skynet/backends", (BackendPool pool) =>
            {
                return Results.Ok(new
                {
                    Backends = pool.AllBackends,
                    TotalCount = pool.TotalCount,
                    HealthyCount = pool.HealthyCount
                });
            });
        }
    }
}
