//
// WebDavMiddleware.cs
//
// Routes incoming HTTP requests to the appropriate WebDAV handler based on
// the request method.  Non-WebDAV methods return 405 Method Not Allowed.
//
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduler.WebDAV.Handlers;
using Scheduler.WebDAV.Services;

namespace Scheduler.WebDAV.Middleware
{
    public class WebDavMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebDavMiddleware> _logger;

        public WebDavMiddleware(RequestDelegate next, ILogger<WebDavMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            string method = context.Request.Method.ToUpperInvariant();
            string path = context.Request.Path.Value ?? "/";

            _logger.LogDebug("WebDAV {Method} {Path}", method, path);

            //
            // Get the authenticated context (set by BasicAuthMiddleware)
            //
            WebDavContext davContext = context.Items.TryGetValue(WebDavContext.CONTEXT_KEY, out object ctx)
                ? ctx as WebDavContext
                : null;

            //
            // OPTIONS is allowed without auth context (for discovery)
            //
            if (method == "OPTIONS")
            {
                await OptionsHandler.HandleAsync(context);
                return;
            }

            //
            // All other methods require authentication
            //
            if (davContext == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            try
            {
                switch (method)
                {
                    case "PROPFIND":
                        await PropFindHandler.HandleAsync(context, davContext);
                        break;

                    case "GET":
                        await GetHandler.HandleAsync(context, davContext);
                        break;

                    case "HEAD":
                        await HeadHandler.HandleAsync(context, davContext);
                        break;

                    case "PUT":
                        await PutHandler.HandleAsync(context, davContext);
                        break;

                    case "DELETE":
                        await DeleteHandler.HandleAsync(context, davContext);
                        break;

                    case "MKCOL":
                        await MkColHandler.HandleAsync(context, davContext);
                        break;

                    case "MOVE":
                        await MoveHandler.HandleAsync(context, davContext);
                        break;

                    case "COPY":
                        await CopyHandler.HandleAsync(context, davContext);
                        break;

                    case "LOCK":
                        await LockHandler.HandleAsync(context, davContext);
                        break;

                    case "UNLOCK":
                        await UnlockHandler.HandleAsync(context, davContext);
                        break;

                    case "PROPPATCH":
                        await PropPatchHandler.HandleAsync(context, davContext);
                        break;

                    default:
                        context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                        context.Response.Headers["Allow"] = "OPTIONS, PROPFIND, PROPPATCH, GET, HEAD, PUT, DELETE, MKCOL, MOVE, COPY, LOCK, UNLOCK";
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebDAV error handling {Method} {Path}", method, path);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}
