// ============================================================================
//
// Program.cs — Standalone Hivemind Host
//
// Admin API:
//   GET  /api/health                      — health check
//   GET  /api/hivemind/cache/stats        — cache statistics
//   POST /api/hivemind/cache/set          — set a key  (?key=k&value=v&ttl=300)
//   GET  /api/hivemind/cache/get          — get a key  (?key=k)
//   GET  /api/hivemind/sessions/stats     — session statistics
//   POST /api/hivemind/sessions/create    — create a session
//   GET  /api/hivemind/pubsub/stats       — pub/sub statistics
//   POST /api/hivemind/pubsub/publish     — publish  (?channel=ch&message=msg)
//
// ============================================================================

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.Hivemind;
using Foundation.Networking.Hivemind.Cache;
using Foundation.Networking.Hivemind.Sessions;
using Foundation.Networking.Hivemind.PubSub;

namespace Foundation.Networking.Hivemind.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHivemind(builder.Configuration);

            WebApplication app = builder.Build();

            MapAdminEndpoints(app);

            ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("HivemindHost");

            logger.LogInformation("═══════════════════════════════════════════════════");
            logger.LogInformation("  🧠  Foundation Hivemind");
            logger.LogInformation("  Distributed cache, session store & pub/sub");
            logger.LogInformation("  Admin API: /api/hivemind/*");
            logger.LogInformation("═══════════════════════════════════════════════════");

            app.Run();
        }

        private static void MapAdminEndpoints(WebApplication app)
        {
            app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Service = "Hivemind" }));

            // ── Cache ─────────────────────────────────────────────

            app.MapGet("/api/hivemind/cache/stats", (DistributedCache cache) =>
            {
                return Results.Ok(cache.GetStatistics());
            });

            app.MapPost("/api/hivemind/cache/set", (HttpContext ctx, DistributedCache cache) =>
            {
                string key = ctx.Request.Query["key"].ToString();
                string value = ctx.Request.Query["value"].ToString();

                if (string.IsNullOrEmpty(key)) return Results.BadRequest("key parameter required");

                int ttl = 300;
                if (int.TryParse(ctx.Request.Query["ttl"], out int t)) ttl = t;

                cache.Set(key, value, TimeSpan.FromSeconds(ttl));

                return Results.Ok(new { Key = key, Cached = true });
            });

            app.MapGet("/api/hivemind/cache/get", (HttpContext ctx, DistributedCache cache) =>
            {
                string key = ctx.Request.Query["key"].ToString();
                if (string.IsNullOrEmpty(key)) return Results.BadRequest("key parameter required");

                string value = cache.Get<string>(key);
                if (value == null) return Results.NotFound(new { Key = key });

                return Results.Ok(new { Key = key, Value = value });
            });

            // ── Sessions ──────────────────────────────────────────

            app.MapGet("/api/hivemind/sessions/stats", (SessionStore sessions) =>
            {
                return Results.Ok(sessions.GetStatistics());
            });

            app.MapPost("/api/hivemind/sessions/create", (SessionStore sessions) =>
            {
                string sessionId = sessions.CreateSession();
                return Results.Ok(new { SessionId = sessionId });
            });

            // ── PubSub ────────────────────────────────────────────

            app.MapGet("/api/hivemind/pubsub/stats", (MessageBus bus) =>
            {
                return Results.Ok(bus.GetStatistics());
            });

            app.MapPost("/api/hivemind/pubsub/publish", (HttpContext ctx, MessageBus bus) =>
            {
                string channel = ctx.Request.Query["channel"].ToString();
                string message = ctx.Request.Query["message"].ToString();

                if (string.IsNullOrEmpty(channel)) return Results.BadRequest("channel parameter required");

                bus.Publish(channel, message ?? "");
                return Results.Ok(new { Channel = channel, Published = true });
            });
        }
    }
}
