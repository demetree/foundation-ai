// ============================================================================
//
// Program.cs — Standalone Conduit Host
//
// Admin API:
//   GET  /api/health                           — health check
//   GET  /api/conduit/connections               — all active connections
//   GET  /api/conduit/connections/stats          — connection statistics
//   POST /api/conduit/connections/connect        — simulate a connection (?userId=u&ip=1.2.3.4)
//   POST /api/conduit/connections/disconnect     — disconnect  (?id=connId)
//   GET  /api/conduit/channels                  — all channels
//   GET  /api/conduit/channels/info             — channel info  (?channel=ch)
//   POST /api/conduit/channels/subscribe        — subscribe  (?connId=c&channel=ch)
//   POST /api/conduit/channels/broadcast        — broadcast  (?channel=ch&sender=c&message=msg)
//   GET  /api/conduit/channels/history          — message history (?channel=ch&count=50)
//
// ============================================================================

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Conduit;
using Foundation.Networking.Conduit.Channels;
using Foundation.Networking.Conduit.Connections;

namespace Foundation.Networking.Conduit.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddConduit(builder.Configuration);

            WebApplication app = builder.Build();

            MapAdminEndpoints(app);

            ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ConduitHost");

            logger.LogInformation("═══════════════════════════════════════════════════");
            logger.LogInformation("  🔌  Foundation Conduit");
            logger.LogInformation("  WebSocket gateway & connection management");
            logger.LogInformation("  Admin API: /api/conduit/*");
            logger.LogInformation("═══════════════════════════════════════════════════");

            app.Run();
        }

        private static void MapAdminEndpoints(WebApplication app)
        {
            app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Service = "Conduit" }));

            // ── Connections ───────────────────────────────────────

            app.MapGet("/api/conduit/connections", (ConnectionManager mgr) =>
            {
                return Results.Ok(mgr.GetAll());
            });

            app.MapGet("/api/conduit/connections/stats", (ConnectionManager mgr) =>
            {
                return Results.Ok(mgr.GetStatistics());
            });

            app.MapPost("/api/conduit/connections/connect", (HttpContext ctx, ConnectionManager mgr) =>
            {
                string userId = ctx.Request.Query["userId"].ToString();
                string ip = ctx.Request.Query["ip"].ToString();

                ClientConnection conn = mgr.Connect(null, ip ?? "", userId ?? "");

                if (conn == null)
                {
                    return Results.StatusCode(503);
                }

                return Results.Ok(new
                {
                    conn.ConnectionId,
                    conn.UserId,
                    conn.ClientIp,
                    conn.ConnectedUtc
                });
            });

            app.MapPost("/api/conduit/connections/disconnect", (HttpContext ctx, ConnectionManager mgr) =>
            {
                string id = ctx.Request.Query["id"].ToString();
                if (string.IsNullOrEmpty(id)) return Results.BadRequest("id parameter required");

                bool removed = mgr.Disconnect(id);
                return Results.Ok(new { ConnectionId = id, Disconnected = removed });
            });

            // ── Channels ──────────────────────────────────────────

            app.MapGet("/api/conduit/channels", (ChannelManager ch) =>
            {
                return Results.Ok(ch.GetChannelNames());
            });

            app.MapGet("/api/conduit/channels/info", (HttpContext ctx, ChannelManager ch) =>
            {
                string channel = ctx.Request.Query["channel"].ToString();
                if (string.IsNullOrEmpty(channel)) return Results.BadRequest("channel parameter required");

                ChannelInfo info = ch.GetChannelInfo(channel);
                return info != null ? Results.Ok(info) : Results.NotFound();
            });

            app.MapPost("/api/conduit/channels/subscribe", (HttpContext ctx, ChannelManager ch) =>
            {
                string connId = ctx.Request.Query["connId"].ToString();
                string channel = ctx.Request.Query["channel"].ToString();

                if (string.IsNullOrEmpty(connId) || string.IsNullOrEmpty(channel))
                    return Results.BadRequest("connId and channel parameters required");

                bool result = ch.Subscribe(connId, channel);
                return Results.Ok(new { ConnectionId = connId, Channel = channel, Subscribed = result });
            });

            app.MapPost("/api/conduit/channels/broadcast", (HttpContext ctx, ChannelManager ch) =>
            {
                string channel = ctx.Request.Query["channel"].ToString();
                string sender = ctx.Request.Query["sender"].ToString();
                string message = ctx.Request.Query["message"].ToString();

                if (string.IsNullOrEmpty(channel)) return Results.BadRequest("channel parameter required");

                List<string> recipients = ch.Broadcast(channel, sender ?? "", message ?? "");
                return Results.Ok(new { Channel = channel, RecipientsCount = recipients.Count });
            });

            app.MapGet("/api/conduit/channels/history", (HttpContext ctx, ChannelManager ch) =>
            {
                string channel = ctx.Request.Query["channel"].ToString();
                if (string.IsNullOrEmpty(channel)) return Results.BadRequest("channel parameter required");

                int count = 50;
                if (int.TryParse(ctx.Request.Query["count"], out int c)) count = c;

                return Results.Ok(ch.GetHistory(channel, count));
            });
        }
    }
}
