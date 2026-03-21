// ============================================================================
//
// Program.cs — Standalone TURN Server Host
//
// A lightweight, independently deployable TURN server.  Can run on a
// dedicated machine separate from Foundation.Server for production
// WebRTC infrastructure.
//
// Usage:
//
//   dotnet run --project Foundation.Networking.Coturn.Host
//
// Configuration via appsettings.json or environment variables:
//
//   TurnServer__ListenPort=3478
//   TurnServer__Realm=turn.example.com
//   TurnServer__SharedSecret=my-secret
//
// Admin API:
//
//   GET /api/turn/status       — server status
//   GET /api/turn/allocations  — active allocations
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Coturn;
using Foundation.Networking.Coturn.Configuration;
using Foundation.Networking.Coturn.Server;

namespace Foundation.Networking.Coturn.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            //
            // Register the TURN server
            //
            builder.Services.AddTurnServer(builder.Configuration);

            //
            // Build the app
            //
            WebApplication app = builder.Build();

            //
            // Map admin API endpoints
            //
            MapAdminEndpoints(app);

            //
            // Start the TURN server
            //
            app.Services.UseTurnServer();

            ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("TurnServerHost");

            TurnServerConfiguration config = new TurnServerConfiguration();
            builder.Configuration.GetSection("TurnServer").Bind(config);

            logger.LogInformation("═══════════════════════════════════════════════════");
            logger.LogInformation("  Foundation TURN Server");
            logger.LogInformation("  UDP: {address}:{port}", config.ListenAddress, config.ListenPort);

            if (config.TcpEnabled)
            {
                logger.LogInformation("  TCP: {address}:{port}", config.ListenAddress, config.TcpListenPort);
            }

            if (config.TlsEnabled)
            {
                logger.LogInformation("  TLS: {address}:{port}", config.ListenAddress, config.TlsListenPort);
            }

            logger.LogInformation("  Realm: {realm}", config.Realm);
            logger.LogInformation("  Relay ports: {min}-{max}", config.RelayPortMin, config.RelayPortMax);
            logger.LogInformation("  Admin API: http://{address}:{port}/api/turn/status",
                builder.Configuration["Kestrel:Endpoints:Http:Url"] ?? "localhost:5000");
            logger.LogInformation("═══════════════════════════════════════════════════");

            //
            // Run — blocks until shutdown
            //
            app.Run();

            //
            // Cleanup
            //
            TurnServer server = app.Services.GetService<TurnServer>();
            server?.Stop();
            server?.Dispose();
        }


        /// <summary>
        /// Maps the minimal API admin endpoints for the standalone host.
        /// </summary>
        private static void MapAdminEndpoints(WebApplication app)
        {
            //
            // GET /api/turn/status — server status
            //
            app.MapGet("/api/turn/status", (TurnServer server) =>
            {
                return Results.Ok(new
                {
                    IsRunning = server.IsRunning,
                    UdpEndpoint = server.ListenEndPoint?.ToString(),
                    TcpEndpoint = server.TcpListener?.ListenEndPoint?.ToString(),
                    TlsEndpoint = server.TlsListener?.ListenEndPoint?.ToString(),
                    AllocationCount = server.Allocations.AllocationCount
                });
            });


            //
            // GET /api/turn/allocations — active allocations
            //
            app.MapGet("/api/turn/allocations", (TurnServer server) =>
            {
                var allocations = new List<object>();

                foreach (var kvp in server.Allocations.GetAllAllocations())
                {
                    FiveTuple ft = kvp.Key;
                    TurnAllocation alloc = kvp.Value;

                    allocations.Add(new
                    {
                        FiveTuple = ft.ToString(),
                        Username = alloc.Username,
                        RelayPort = alloc.RelayPort,
                        LifetimeSeconds = alloc.LifetimeSeconds,
                        ExpiresAtUtc = alloc.ExpiresAtUtc,
                        PermissionCount = alloc.Permissions.Count,
                        ChannelCount = alloc.Channels.Count,
                        IsExpired = alloc.IsExpired()
                    });
                }

                return Results.Ok(new
                {
                    Allocations = allocations,
                    TotalCount = allocations.Count
                });
            });


            //
            // GET /api/turn/health — simple health check
            //
            app.MapGet("/api/turn/health", (TurnServer server) =>
            {
                if (server.IsRunning)
                {
                    return Results.Ok(new { Status = "Healthy" });
                }
                else
                {
                    return Results.StatusCode(503);
                }
            });
        }
    }
}
