// ============================================================================
//
// Program.cs — Standalone Beacon Host
//
// Admin API:
//   GET  /api/health                      — health check
//   POST /api/beacon/dns/resolve          — resolve hostname (?host=google.com)
//   POST /api/beacon/dns/reverse          — reverse DNS  (?ip=8.8.8.8)
//   GET  /api/beacon/zone/records         — list zone records
//   POST /api/beacon/zone/add             — add a record (?host=x&type=A&value=y)
//   GET  /api/beacon/discovery/services   — list service names
//   POST /api/beacon/discovery/register   — register service
//   GET  /api/beacon/discovery/discover   — discover service (?service=Name)
//   GET  /api/beacon/discovery/stats      — directory statistics
//
// ============================================================================

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Beacon;
using Foundation.Networking.Beacon.Dns;
using Foundation.Networking.Beacon.Discovery;

namespace Foundation.Networking.Beacon.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddBeacon(builder.Configuration);

            WebApplication app = builder.Build();

            MapAdminEndpoints(app);

            ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("BeaconHost");

            logger.LogInformation("═══════════════════════════════════════════════════");
            logger.LogInformation("  📡  Foundation Beacon");
            logger.LogInformation("  DNS management & service discovery");
            logger.LogInformation("  Admin API: /api/beacon/*");
            logger.LogInformation("═══════════════════════════════════════════════════");

            app.Run();
        }

        private static void MapAdminEndpoints(WebApplication app)
        {
            app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Service = "Beacon" }));

            // ── DNS ───────────────────────────────────────────────

            app.MapPost("/api/beacon/dns/resolve", async (HttpContext ctx, DnsResolver resolver) =>
            {
                string host = ctx.Request.Query["host"].ToString();
                if (string.IsNullOrEmpty(host)) return Results.BadRequest("host parameter required");

                DnsLookupResult result = await resolver.ResolveAsync(host);
                return Results.Ok(result);
            });

            app.MapPost("/api/beacon/dns/reverse", async (HttpContext ctx, DnsResolver resolver) =>
            {
                string ip = ctx.Request.Query["ip"].ToString();
                if (string.IsNullOrEmpty(ip)) return Results.BadRequest("ip parameter required");

                ReverseDnsResult result = await resolver.ReverseLookupAsync(ip);
                return Results.Ok(result);
            });

            // ── Zone ──────────────────────────────────────────────

            app.MapGet("/api/beacon/zone/records", (DnsZoneManager zone) =>
            {
                return Results.Ok(zone.GetAllRecords());
            });

            app.MapPost("/api/beacon/zone/add", (HttpContext ctx, DnsZoneManager zone) =>
            {
                string host = ctx.Request.Query["host"].ToString();
                string type = ctx.Request.Query["type"].ToString();
                string value = ctx.Request.Query["value"].ToString();

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(value))
                    return Results.BadRequest("host and value parameters required");

                zone.AddRecord(new DnsRecord
                {
                    Hostname = host,
                    RecordType = string.IsNullOrEmpty(type) ? "A" : type,
                    Value = value
                });

                return Results.Ok(new { Host = host, Added = true });
            });

            // ── Discovery ─────────────────────────────────────────

            app.MapGet("/api/beacon/discovery/services", (ServiceDirectory dir) =>
            {
                return Results.Ok(dir.GetServiceNames());
            });

            app.MapPost("/api/beacon/discovery/register", (HttpContext ctx, ServiceDirectory dir) =>
            {
                string service = ctx.Request.Query["service"].ToString();
                string host = ctx.Request.Query["host"].ToString();

                if (string.IsNullOrEmpty(service)) return Results.BadRequest("service parameter required");

                int port = 5000;
                if (int.TryParse(ctx.Request.Query["port"], out int p)) port = p;

                string id = dir.Register(new ServiceEndpoint
                {
                    ServiceName = service,
                    Host = host ?? "localhost",
                    Port = port
                });

                return Results.Ok(new { InstanceId = id, ServiceName = service });
            });

            app.MapGet("/api/beacon/discovery/discover", (HttpContext ctx, ServiceDirectory dir) =>
            {
                string service = ctx.Request.Query["service"].ToString();
                if (string.IsNullOrEmpty(service)) return Results.BadRequest("service parameter required");

                return Results.Ok(dir.Discover(service));
            });

            app.MapGet("/api/beacon/discovery/stats", (ServiceDirectory dir) =>
            {
                return Results.Ok(dir.GetStatistics());
            });
        }
    }
}
