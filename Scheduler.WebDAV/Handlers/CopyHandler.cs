//
// CopyHandler.cs
//
// Handles COPY requests — creates a copy of a file at the Destination path.
// Folder copy is not supported in Phase 1 (returns 403).
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation.Scheduler.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Server.Services;
using Scheduler.WebDAV.Services;

namespace Scheduler.WebDAV.Handlers
{
    public static class CopyHandler
    {
        public static async Task HandleAsync(HttpContext context, WebDavContext davContext)
        {
            IFileStorageService fileService = context.RequestServices.GetRequiredService<IFileStorageService>();

            string sourcePath = context.Request.Path.Value ?? "/";

            //
            // The Destination header is required
            //
            string destinationHeader = context.Request.Headers["Destination"].FirstOrDefault();

            if (string.IsNullOrEmpty(destinationHeader))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            //
            // Parse Destination
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


            //
            // Resolve source
            //
            PathResolver.ResolvedPath sourceResolved = await PathResolver.ResolveAsync(
                sourcePath, davContext.TenantGuid, fileService, context.RequestAborted);


            //
            // Folder copy is not supported in Phase 1
            //
            if (sourceResolved.IsCollection)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }


            //
            // Find the source document
            //
            if (sourceResolved.DocumentName == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            List<Document> sourceDocs = await fileService.GetDocumentsInFolderAsync(
                sourceResolved.FolderId, davContext.TenantGuid, context.RequestAborted);

            Document sourceDoc = sourceDocs.FirstOrDefault(d =>
                string.Equals(d.fileName, sourceResolved.DocumentName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(d.name, sourceResolved.DocumentName, StringComparison.OrdinalIgnoreCase));

            if (sourceDoc == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }


            //
            // Fetch the full source document (including binary)
            //
            Document fullSource = await fileService.GetDocumentByIdAsync(
                sourceDoc.id, davContext.TenantGuid, context.RequestAborted);

            if (fullSource == null || fullSource.fileDataData == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }


            //
            // Resolve destination
            //
            PathResolver.ResolvedPath destResolved = await PathResolver.ResolveAsync(
                destPath, davContext.TenantGuid, fileService, context.RequestAborted);


            //
            // Determine destination file name
            //
            string destFileName = destResolved.DocumentName;

            if (destFileName == null && !destResolved.Found && destResolved.UnresolvedSegments?.Length == 1)
            {
                destFileName = destResolved.UnresolvedSegments[0];
            }

            if (destFileName == null)
            {
                // Copying into a folder with the same file name
                destFileName = fullSource.fileName;
            }


            //
            // Create the copy
            //
            Document newDoc = new Document
            {
                tenantGuid = davContext.TenantGuid,
                documentFolderId = destResolved.FolderId,
                name = Path.GetFileNameWithoutExtension(destFileName),
                fileName = destFileName,
                mimeType = fullSource.mimeType,
                fileDataFileName = destFileName,
                fileDataData = fullSource.fileDataData,
                fileDataSize = fullSource.fileDataSize,
                fileDataMimeType = fullSource.fileDataMimeType,
                fileSizeBytes = fullSource.fileSizeBytes,
                uploadedBy = davContext.User.accountName,
                description = fullSource.description,
                documentTypeId = fullSource.documentTypeId,
                versionNumber = 1
            };

            await fileService.UploadDocumentAsync(newDoc, davContext.SecurityUserId, context.RequestAborted);

            context.Response.StatusCode = StatusCodes.Status201Created;
        }
    }
}
