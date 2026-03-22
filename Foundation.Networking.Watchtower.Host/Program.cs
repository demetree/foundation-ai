// ============================================================================
//
// Program.cs — Standalone Watchtower Host
//
// A lightweight, independently deployable network diagnostics server.
//
// Usage:
//   dotnet run --project Foundation.Networking.Watchtower.Host
//
// Admin API:
//   GET  /api/health                — health check
//   POST /api/watchtower/ping       — ping a host  (?host=google.com&count=4)
//   POST /api/watchtower/traceroute — traceroute    (?host=google.com)
//   POST /api/watchtower/portscan   — port scan     (?host=localhost&ports=80,443)
//   GET  /api/watchtower/latency    — latency stats
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Watchtower;
using Foundation.Networking.Watchtower.Services;

namespace Foundation.Networking.Watchtower.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddWatchtower(builder.Configuration);

            WebApplication app = builder.Build();

            MapAdminEndpoints(app);

            ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("WatchtowerHost");

            logger.LogInformation("═══════════════════════════════════════════════════");
            logger.LogInformation("  🔭  Foundation Watchtower");
            logger.LogInformation("  Network diagnostics: ping, traceroute, port scan");
            logger.LogInformation("  Admin API: /api/watchtower/*");
            logger.LogInformation("═══════════════════════════════════════════════════");

            app.Run();
        }


        private static void MapAdminEndpoints(WebApplication app)
        {
            app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Service = "Watchtower" }));

            app.MapPost("/api/watchtower/ping", async (HttpContext ctx, PingService ping) =>
            {
                string host = ctx.Request.Query["host"].ToString();

                if (string.IsNullOrEmpty(host))
                {
                    return Results.BadRequest("host parameter required");
                }

                int count = 4;
                if (int.TryParse(ctx.Request.Query["count"], out int c)) count = c;

                PingStatistics result = await ping.PingWithStatisticsAsync(host, count);
                return Results.Ok(result);
            });

            app.MapPost("/api/watchtower/traceroute", async (HttpContext ctx, TracerouteService traceroute) =>
            {
                string host = ctx.Request.Query["host"].ToString();

                if (string.IsNullOrEmpty(host))
                {
                    return Results.BadRequest("host parameter required");
                }

                TracerouteResult result = await traceroute.TraceAsync(host);
                return Results.Ok(result);
            });

            app.MapPost("/api/watchtower/portscan", async (HttpContext ctx, PortScannerService scanner) =>
            {
                string host = ctx.Request.Query["host"].ToString();

                if (string.IsNullOrEmpty(host))
                {
                    return Results.BadRequest("host parameter required");
                }

                PortScanReport result = await scanner.ScanCommonPortsAsync(host);
                return Results.Ok(result);
            });

            app.MapGet("/api/watchtower/latency", (LatencyMonitorService monitor) =>
            {
                return Results.Ok(monitor.GetAllSummaries());
            });
        }
    }
}
