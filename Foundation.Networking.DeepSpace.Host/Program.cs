// ============================================================================
//
// Program.cs — Standalone Deep Space Host
//
// Admin API:
//   GET    /api/health                    — health check
//   GET    /api/deepspace/stats           — storage statistics
//   GET    /api/deepspace/list            — list objects  (?prefix=docs/)
//   POST   /api/deepspace/put             — put object  (?key=path/file.txt)
//   GET    /api/deepspace/get             — get object   (?key=path/file.txt)
//   DELETE /api/deepspace/delete          — delete object (?key=path/file.txt)
//   GET    /api/deepspace/exists          — check exists  (?key=path/file.txt)
//
// ============================================================================

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Foundation.Networking.DeepSpace;
using Foundation.Networking.DeepSpace.Providers;

namespace Foundation.Networking.DeepSpace.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDeepSpace(builder.Configuration);

            WebApplication app = builder.Build();

            MapAdminEndpoints(app);

            ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DeepSpaceHost");

            logger.LogInformation("═══════════════════════════════════════════════════");
            logger.LogInformation("  🚀  Foundation Deep Space");
            logger.LogInformation("  Cloud storage abstraction");
            logger.LogInformation("  Admin API: /api/deepspace/*");
            logger.LogInformation("═══════════════════════════════════════════════════");

            app.Run();
        }

        private static void MapAdminEndpoints(WebApplication app)
        {
            app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Service = "DeepSpace" }));

            app.MapGet("/api/deepspace/stats", (StorageManager mgr) =>
            {
                return Results.Ok(mgr.GetStatistics());
            });

            app.MapGet("/api/deepspace/list", async (HttpContext ctx, StorageManager mgr) =>
            {
                string prefix = ctx.Request.Query["prefix"].ToString();
                ListResult result = await mgr.ListAsync(prefix ?? "");
                return Results.Ok(result);
            });

            app.MapPost("/api/deepspace/put", async (HttpContext ctx, StorageManager mgr) =>
            {
                string key = ctx.Request.Query["key"].ToString();
                if (string.IsNullOrEmpty(key)) return Results.BadRequest("key parameter required");

                using (MemoryStream ms = new MemoryStream())
                {
                    await ctx.Request.Body.CopyToAsync(ms);
                    byte[] data = ms.ToArray();

                    StorageResult result = await mgr.PutAsync(key, data, ctx.Request.ContentType);
                    return result.Success ? Results.Ok(result) : Results.StatusCode(500);
                }
            });

            app.MapGet("/api/deepspace/get", async (HttpContext ctx, StorageManager mgr) =>
            {
                string key = ctx.Request.Query["key"].ToString();
                if (string.IsNullOrEmpty(key)) return Results.BadRequest("key parameter required");

                byte[] data = await mgr.GetAsync(key);
                if (data == null) return Results.NotFound(new { Key = key });

                return Results.Bytes(data);
            });

            app.MapDelete("/api/deepspace/delete", async (HttpContext ctx, StorageManager mgr) =>
            {
                string key = ctx.Request.Query["key"].ToString();
                if (string.IsNullOrEmpty(key)) return Results.BadRequest("key parameter required");

                bool deleted = await mgr.DeleteAsync(key);
                return Results.Ok(new { Key = key, Deleted = deleted });
            });

            app.MapGet("/api/deepspace/exists", async (HttpContext ctx, StorageManager mgr) =>
            {
                string key = ctx.Request.Query["key"].ToString();
                if (string.IsNullOrEmpty(key)) return Results.BadRequest("key parameter required");

                bool exists = await mgr.ExistsAsync(key);
                return Results.Ok(new { Key = key, Exists = exists });
            });


            //
            // Admin: rebuild metadata database from sidecar files
            //
            app.MapPost("/api/deepspace/admin/rebuild", async (StorageManager mgr) =>
            {
                int recovered = await mgr.RebuildFromProvidersAsync();
                return Results.Ok(new { Recovered = recovered, Message = $"Recovery complete. {recovered} objects recovered from sidecar files." });
            });
        }
    }
}
