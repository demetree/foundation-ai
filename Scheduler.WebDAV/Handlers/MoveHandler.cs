//
// MoveHandler.cs
//
// Handles MOVE requests — moves or renames a file or folder.
// Reads the Destination header to determine the target path.
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
    public static class MoveHandler
    {
        public static async Task HandleAsync(HttpContext context, WebDavContext davContext)
        {
            IFileStorageService fileService = context.RequestServices.GetRequiredService<IFileStorageService>();

            string sourcePath = context.Request.Path.Value ?? "/";

            //
            // The Destination header is required for MOVE
            //
            string destinationHeader = context.Request.Headers["Destination"].FirstOrDefault();

            if (string.IsNullOrEmpty(destinationHeader))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            //
            // Parse Destination into a path (may be absolute URL or relative path)
            //
            string destPath;
            if (Uri.TryCreate(destinationHeader, UriKind.Absolute, out Uri destUri))
            {
                destPath = destUri.AbsolutePath;
            }
            else
            {
                destPath = destinationHeader;
            }

            bool overwrite = !string.Equals(
                context.Request.Headers["Overwrite"].FirstOrDefault(), "F",
                StringComparison.OrdinalIgnoreCase);


            //
            // Resolve source and destination
            //
            PathResolver.ResolvedPath sourceResolved = await PathResolver.ResolveAsync(
                sourcePath, davContext.TenantGuid, fileService, context.RequestAborted);

            PathResolver.ResolvedPath destResolved = await PathResolver.ResolveAsync(
                destPath, davContext.TenantGuid, fileService, context.RequestAborted);


            //
            // Source must exist
            //
            if (!sourceResolved.Found && sourceResolved.DocumentName == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }


            //
            // Moving a folder
            //
            if (sourceResolved.IsCollection && sourceResolved.FolderId.HasValue)
            {
                DocumentFolder folder = sourceResolved.Folder;

                if (folder == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                //
                // Determine new parent and new name
                //
                string newName = null;

                if (!destResolved.Found && destResolved.UnresolvedSegments?.Length == 1)
                {
                    // Destination parent exists, last segment is the new name
                    newName = destResolved.UnresolvedSegments[0];
                }
                else if (destResolved.Found && destResolved.DocumentName != null)
                {
                    newName = destResolved.DocumentName;
                }

                folder.parentDocumentFolderId = destResolved.FolderId;

                if (newName != null)
                {
                    folder.name = newName;
                }

                await fileService.UpdateFolderAsync(folder, davContext.SecurityUserId, context.RequestAborted);

                context.Response.StatusCode = StatusCodes.Status201Created;
                return;
            }


            //
            // Moving a document
            //
            if (sourceResolved.DocumentName != null)
            {
                List<Document> sourceDocs = await fileService.GetDocumentsInFolderAsync(
                    sourceResolved.FolderId, davContext.TenantGuid, context.RequestAborted);

                Document doc = sourceDocs.FirstOrDefault(d =>
                    string.Equals(d.fileName, sourceResolved.DocumentName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(d.name, sourceResolved.DocumentName, StringComparison.OrdinalIgnoreCase));

                if (doc == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                //
                // Move the document to the destination folder
                //
                await fileService.MoveDocumentAsync(
                    doc.id, destResolved.FolderId, davContext.TenantGuid,
                    davContext.SecurityUserId, context.RequestAborted);

                //
                // If the destination has a different file name, rename
                //
                string newFileName = destResolved.DocumentName;

                if (!destResolved.Found && destResolved.UnresolvedSegments?.Length == 1)
                {
                    newFileName = destResolved.UnresolvedSegments[0];
                }

                if (newFileName != null &&
                    !string.Equals(newFileName, doc.fileName, StringComparison.OrdinalIgnoreCase))
                {
                    Document fullDoc = await fileService.GetDocumentByIdAsync(
                        doc.id, davContext.TenantGuid, context.RequestAborted);

                    if (fullDoc != null)
                    {
                        fullDoc.fileName = newFileName;
                        fullDoc.name = System.IO.Path.GetFileNameWithoutExtension(newFileName);
                        fullDoc.fileDataFileName = newFileName;

                        await fileService.UpdateDocumentMetadataAsync(
                            fullDoc, davContext.SecurityUserId, context.RequestAborted);
                    }
                }

                context.Response.StatusCode = StatusCodes.Status201Created;
                return;
            }


            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
