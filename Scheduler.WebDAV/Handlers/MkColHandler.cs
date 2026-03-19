//
// MkColHandler.cs
//
// Handles MKCOL requests — creates a new folder (collection).
//
using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation.Scheduler.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Server.Services;
using Scheduler.WebDAV.Services;

namespace Scheduler.WebDAV.Handlers
{
    public static class MkColHandler
    {
        public static async Task HandleAsync(HttpContext context, WebDavContext davContext)
        {
            IFileStorageService fileService = context.RequestServices.GetRequiredService<IFileStorageService>();

            string path = context.Request.Path.Value ?? "/";

            //
            // MKCOL with a request body is not supported (RFC 4918 §9.3.1)
            //
            if (context.Request.ContentLength > 0)
            {
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                return;
            }


            //
            // Resolve the path  — the LAST segment is the folder to create
            //
            PathResolver.ResolvedPath resolved = await PathResolver.ResolveAsync(
                path, davContext.TenantGuid, fileService, context.RequestAborted);


            //
            // If the path fully resolves, the folder already exists → 405
            //
            if (resolved.Found && resolved.IsCollection)
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return;
            }


            //
            // If there are unresolved intermediate segments → 409 Conflict
            // (parent must exist before creating the child)
            //
            if (!resolved.Found && resolved.UnresolvedSegments != null && resolved.UnresolvedSegments.Length > 1)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                return;
            }


            //
            // Get the new folder name
            //
            string newFolderName = null;

            if (!resolved.Found && resolved.UnresolvedSegments != null && resolved.UnresolvedSegments.Length == 1)
            {
                newFolderName = resolved.UnresolvedSegments[0];
            }
            else if (resolved.Found && resolved.DocumentName != null)
            {
                // Path didn't end with "/" but the last segment isn't an existing folder
                newFolderName = resolved.DocumentName;
            }

            if (string.IsNullOrWhiteSpace(newFolderName))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }


            //
            // Create the folder
            //
            DocumentFolder newFolder = new DocumentFolder
            {
                tenantGuid = davContext.TenantGuid,
                name = newFolderName,
                parentDocumentFolderId = resolved.FolderId,
                sequence = 0
            };

            await fileService.CreateFolderAsync(newFolder, davContext.SecurityUserId, context.RequestAborted);

            context.Response.StatusCode = StatusCodes.Status201Created;
        }
    }
}
