//
// HeadHandler.cs
//
// Handles HEAD requests — returns file metadata headers without the body.
// Identical to GET but does not write the response body.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation.Scheduler.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Server.Services;
using Scheduler.WebDAV.Services;

namespace Scheduler.WebDAV.Handlers
{
    public static class HeadHandler
    {
        public static async Task HandleAsync(HttpContext context, WebDavContext davContext)
        {
            IFileStorageService fileService = context.RequestServices.GetRequiredService<IFileStorageService>();

            string path = context.Request.Path.Value ?? "/";

            PathResolver.ResolvedPath resolved = await PathResolver.ResolveAsync(
                path, davContext.TenantGuid, fileService, context.RequestAborted);


            if (resolved.IsCollection && resolved.DocumentName == null)
            {
                //
                // HEAD on a collection — return 200 with no body
                //
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentLength = 0;
                return;
            }

            string fileName = resolved.DocumentName;

            if (fileName == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            List<Document> docsInFolder = await fileService.GetDocumentsInFolderAsync(
                resolved.FolderId, davContext.TenantGuid, context.RequestAborted);

            Document doc = docsInFolder.FirstOrDefault(d =>
                string.Equals(d.fileName, fileName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(d.name, fileName, StringComparison.OrdinalIgnoreCase));

            if (doc == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }


            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = doc.mimeType ?? doc.fileDataMimeType ?? "application/octet-stream";
            context.Response.ContentLength = doc.fileSizeBytes;
            context.Response.Headers["ETag"] = $"\"{doc.id}-{doc.uploadedDate.Ticks:X}\"";
            context.Response.Headers["Last-Modified"] = doc.uploadedDate.ToUniversalTime().ToString("R");

            // No body for HEAD
        }
    }
}
