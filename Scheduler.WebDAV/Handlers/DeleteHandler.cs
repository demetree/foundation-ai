//
// DeleteHandler.cs
//
// Handles DELETE requests — soft-deletes a file or folder.
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
    public static class DeleteHandler
    {
        public static async Task HandleAsync(HttpContext context, WebDavContext davContext)
        {
            IFileStorageService fileService = context.RequestServices.GetRequiredService<IFileStorageService>();

            string path = context.Request.Path.Value ?? "/";

            PathResolver.ResolvedPath resolved = await PathResolver.ResolveAsync(
                path, davContext.TenantGuid, fileService, context.RequestAborted);


            //
            // Cannot delete root
            //
            if (resolved.IsCollection && resolved.FolderId == null)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }


            if (resolved.IsCollection && resolved.FolderId.HasValue)
            {
                //
                // Delete a folder (cascade)
                //
                await fileService.DeleteFolderAsync(
                    resolved.FolderId.Value,
                    davContext.TenantGuid,
                    davContext.SecurityUserId,
                    cascade: true,
                    context.RequestAborted);

                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }


            if (resolved.DocumentName != null)
            {
                //
                // Delete a document
                //
                List<Document> docsInFolder = await fileService.GetDocumentsInFolderAsync(
                    resolved.FolderId, davContext.TenantGuid, context.RequestAborted);

                Document doc = docsInFolder.FirstOrDefault(d =>
                    string.Equals(d.fileName, resolved.DocumentName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(d.name, resolved.DocumentName, StringComparison.OrdinalIgnoreCase));

                if (doc == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                await fileService.DeleteDocumentAsync(
                    doc.id,
                    davContext.TenantGuid,
                    davContext.SecurityUserId,
                    context.RequestAborted);

                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }


            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}
