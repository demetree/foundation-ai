// ============================================================================
//
// Program.cs — Standalone Locksmith Host
//
// Admin API:
//   GET  /api/health                     — health check
//   POST /api/locksmith/inspect          — inspect a TLS cert  (?host=google.com&port=443)
//   GET  /api/locksmith/monitor/status   — all monitored endpoint statuses
//   POST /api/locksmith/monitor/check    — force an immediate check
//
// ============================================================================

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Locksmith;
using Foundation.Networking.Locksmith.Services;

namespace Foundation.Networking.Locksmith.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLocksmith(builder.Configuration);

            WebApplication app = builder.Build();

            MapAdminEndpoints(app);

            ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("LocksmithHost");

            logger.LogInformation("═══════════════════════════════════════════════════");
            logger.LogInformation("  🔑  Foundation Locksmith");
            logger.LogInformation("  Certificate inspection & monitoring");
            logger.LogInformation("  Admin API: /api/locksmith/*");
            logger.LogInformation("═══════════════════════════════════════════════════");

            app.Run();
        }

        private static void MapAdminEndpoints(WebApplication app)
        {
            app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Service = "Locksmith" }));

            app.MapPost("/api/locksmith/inspect", async (HttpContext ctx, CertificateInspector inspector) =>
            {
                string host = ctx.Request.Query["host"].ToString();
                if (string.IsNullOrEmpty(host)) return Results.BadRequest("host parameter required");

                int port = 443;
                if (int.TryParse(ctx.Request.Query["port"], out int p)) port = p;

                CertificateInfo info = await inspector.InspectAsync(host, port);
                return Results.Ok(info);
            });

            app.MapGet("/api/locksmith/monitor/status", (CertificateMonitorService monitor) =>
            {
                return Results.Ok(monitor.GetAllStatuses());
            });

            app.MapPost("/api/locksmith/monitor/check", async (CertificateMonitorService monitor) =>
            {
                await monitor.ForceCheckAsync();
                return Results.Ok(new { Message = "Check completed", Statuses = monitor.GetAllStatuses() });
            });
        }
    }
}
