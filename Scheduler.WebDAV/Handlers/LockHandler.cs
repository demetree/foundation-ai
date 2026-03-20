//
// LockHandler.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Handles LOCK requests — creates or refreshes a lock on a document (RFC 4918 §9.10).
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Foundation.Scheduler.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduler.Server.Services;
using Scheduler.WebDAV.Services;
using Scheduler.WebDAV.Xml;

namespace Scheduler.WebDAV.Handlers
{
    public static class LockHandler
    {
        private static readonly XNamespace DAV = "DAV:";

        public static async Task HandleAsync(HttpContext context, WebDavContext davContext)
        {
            IFileStorageService fileService = context.RequestServices.GetRequiredService<IFileStorageService>();
            WebDavLockDatabase lockDb = context.RequestServices.GetRequiredService<WebDavLockDatabase>();
            ILogger logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("LockHandler");

            string path = context.Request.Path.Value ?? "/";
            string ifHeader = context.Request.Headers["If"].FirstOrDefault();
            string timeoutHeader = context.Request.Headers["Timeout"].FirstOrDefault();

            int timeoutSeconds = ParseTimeout(timeoutHeader);


            //
            // Resolve the path to a document
            //
            var (doc, statusCode) = await PathResolver.ResolveDocumentAsync(
                path, davContext.TenantGuid, fileService, context.RequestAborted);

            if (doc == null)
            {
                context.Response.StatusCode = statusCode;
                return;
            }


            //
            // If the request has a body, parse the lockinfo XML
            // If no body, this is a lock refresh (requires If header)
            //
            string scope = "exclusive";
            string owner = davContext.User?.accountName ?? "unknown";

            if (context.Request.ContentLength > 0)
            {
                try
                {
                    using MemoryStream ms = new MemoryStream();
                    await context.Request.Body.CopyToAsync(ms, context.RequestAborted);
                    ms.Position = 0;

                    XDocument lockInfoDoc = await XDocument.LoadAsync(ms, LoadOptions.None, context.RequestAborted);
                    XElement lockInfoEl = lockInfoDoc.Root;

                    if (lockInfoEl != null)
                    {
                        XElement lockScopeEl = lockInfoEl.Element(DAV + "lockscope");
                        if (lockScopeEl != null)
                        {
                            if (lockScopeEl.Element(DAV + "shared") != null)
                                scope = "shared";
                            else
                                scope = "exclusive";
                        }

                        XElement ownerEl = lockInfoEl.Element(DAV + "owner");
                        if (ownerEl != null)
                        {
                            XElement hrefEl = ownerEl.Element(DAV + "href");
                            owner = hrefEl?.Value ?? ownerEl.Value ?? owner;
                        }
                    }
                }
                catch
                {
                    // Malformed XML — use defaults
                }
            }
            else if (!string.IsNullOrEmpty(ifHeader))
            {
                //
                // Lock refresh — extract lock token from If header
                //
                string token = ExtractLockToken(ifHeader);
                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                WebDavLock refreshed = await lockDb.RefreshLockAsync(token, owner, timeoutSeconds);
                if (refreshed == null)
                {
                    context.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
                    return;
                }

                await WriteLockResponse(context, refreshed, StatusCodes.Status200OK);
                return;
            }


            //
            // Acquire the lock
            //
            try
            {
                WebDavLock newLock = await lockDb.AcquireLockAsync(
                    doc.id,
                    davContext.TenantGuid,
                    owner,
                    scope,
                    depth: 0,
                    timeoutSeconds: timeoutSeconds);

                await WriteLockResponse(context, newLock, StatusCodes.Status200OK);
            }
            catch (InvalidOperationException ex)
            {
                //
                // Already locked by another user
                //
                logger.LogWarning("Lock conflict on document {DocId}: {Message}", doc.id, ex.Message);
                context.Response.StatusCode = StatusCodes.Status423Locked;
            }
        }


        /// <summary>
        /// Writes a LOCK response with the Lock-Token header and lockdiscovery XML body.
        /// </summary>
        private static async Task WriteLockResponse(HttpContext context, WebDavLock lockInfo, int statusCode)
        {
            XDocument xml = DavXmlBuilder.LockDiscoveryResponse(lockInfo);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/xml; charset=utf-8";
            context.Response.Headers["Lock-Token"] = $"<{lockInfo.LockToken}>";

            using MemoryStream ms = new MemoryStream();
            xml.Save(ms);
            context.Response.ContentLength = ms.Length;
            ms.Position = 0;
            await ms.CopyToAsync(context.Response.Body, context.RequestAborted);
        }


        /// <summary>
        /// Parses a Timeout header value (e.g., "Second-300" or "Infinite").
        /// </summary>
        private static int ParseTimeout(string timeoutHeader)
        {
            if (string.IsNullOrEmpty(timeoutHeader))
                return WebDavLockDatabase.DEFAULT_TIMEOUT_SECONDS;

            if (timeoutHeader.StartsWith("Second-", StringComparison.OrdinalIgnoreCase))
            {
                string seconds = timeoutHeader.Substring("Second-".Length);
                if (int.TryParse(seconds, out int parsed) && parsed > 0)
                    return Math.Min(parsed, WebDavLockDatabase.MAX_TIMEOUT_SECONDS);
            }

            // "Infinite" or unparseable — use max
            return WebDavLockDatabase.MAX_TIMEOUT_SECONDS;
        }


        /// <summary>
        /// Extracts a lock token from an If header.
        /// Example: "(<urn:uuid:xxxx>)" → "urn:uuid:xxxx"
        /// </summary>
        internal static string ExtractLockToken(string ifHeader)
        {
            if (string.IsNullOrEmpty(ifHeader)) return null;

            // Simple extraction — look for urn:uuid: between angle brackets or parentheses
            int start = ifHeader.IndexOf("urn:uuid:", StringComparison.OrdinalIgnoreCase);
            if (start < 0) return null;

            // Find the end delimiter (> or ) or space)
            int end = ifHeader.Length;
            for (int i = start; i < ifHeader.Length; i++)
            {
                if (ifHeader[i] == '>' || ifHeader[i] == ')' || ifHeader[i] == ' ')
                {
                    end = i;
                    break;
                }
            }

            return ifHeader.Substring(start, end - start);
        }
    }
}
