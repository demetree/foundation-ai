//
// UnlockHandler.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Handles UNLOCK requests — releases a lock on a document (RFC 4918 §9.11).
//
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduler.WebDAV.Services;

namespace Scheduler.WebDAV.Handlers
{
    public static class UnlockHandler
    {
        public static async Task HandleAsync(HttpContext context, WebDavContext davContext)
        {
            WebDavLockDatabase lockDb = context.RequestServices.GetRequiredService<WebDavLockDatabase>();
            ILogger logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("UnlockHandler");

            //
            // The Lock-Token header is required for UNLOCK
            //
            string lockTokenHeader = context.Request.Headers["Lock-Token"].FirstOrDefault();

            if (string.IsNullOrEmpty(lockTokenHeader))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            //
            // Strip angle brackets: "<urn:uuid:xxxx>" → "urn:uuid:xxxx"
            //
            string lockToken = lockTokenHeader.Trim('<', '>', ' ');

            //
            // Release the lock
            //
            string owner = davContext.User?.accountName ?? "unknown";
            bool released = await lockDb.ReleaseLockAsync(lockToken, owner);

            if (released)
            {
                logger.LogDebug("Released lock {LockToken} for user {Owner}.", lockToken, owner);
                context.Response.StatusCode = StatusCodes.Status204NoContent;
            }
            else
            {
                logger.LogWarning("UNLOCK failed — token {LockToken} not found or not owned by {Owner}.", lockToken, owner);
                context.Response.StatusCode = StatusCodes.Status409Conflict;
            }
        }
    }
}
