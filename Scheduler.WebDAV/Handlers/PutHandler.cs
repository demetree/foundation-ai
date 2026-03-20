//
// PutHandler.cs
//
// Handles PUT requests — uploads a new file or overwrites an existing one.
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
    public static class PutHandler
    {
        public static async Task HandleAsync(HttpContext context, WebDavContext davContext)
        {
            IFileStorageService fileService = context.RequestServices.GetRequiredService<IFileStorageService>();

            string path = context.Request.Path.Value ?? "/";

            //
            // Resolve path
            //
            PathResolver.ResolvedPath resolved = await PathResolver.ResolveAsync(
                path, davContext.TenantGuid, fileService, context.RequestAborted);


            //
            // PUT on a collection is not allowed
            //
            if (resolved.IsCollection && resolved.DocumentName == null)
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return;
            }

            string fileName = resolved.DocumentName;

            if (fileName == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            //
            // The parent folder must exist (409 Conflict if not)
            //
            if (!resolved.Found && resolved.FolderId == null && resolved.UnresolvedSegments?.Length > 1)
            {
                // There are intermediate folders that don't exist
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                return;
            }


            //
            // Read the request body
            //
            byte[] fileData;
            using (MemoryStream ms = new MemoryStream())
            {
                await context.Request.Body.CopyToAsync(ms, context.RequestAborted);
                fileData = ms.ToArray();
            }


            //
            // Check if a document already exists at this path
            //
            List<Document> docsInFolder = await fileService.GetDocumentsInFolderAsync(
                resolved.FolderId, davContext.TenantGuid, context.RequestAborted);

            Document existing = docsInFolder.FirstOrDefault(d =>
                string.Equals(d.fileName, fileName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(d.name, fileName, StringComparison.OrdinalIgnoreCase));


            if (existing != null)
            {
                //
                // Overwrite — fetch the full document (including binary) and update it
                //
                Document fullDoc = await fileService.GetDocumentByIdAsync(
                    existing.id, davContext.TenantGuid, context.RequestAborted);

                if (fullDoc != null)
                {
                    fullDoc.fileDataData = fileData;
                    fullDoc.fileSizeBytes = fileData.Length;
                    fullDoc.fileDataSize = fileData.Length;
                    fullDoc.mimeType = GuessMimeType(fileName);
                    fullDoc.fileDataMimeType = fullDoc.mimeType;
                    fullDoc.uploadedDate = DateTime.UtcNow;

                    await fileService.UpdateDocumentMetadataAsync(fullDoc, davContext.SecurityUserId, context.RequestAborted);

                    //
                    // Need to save the binary data separately since UpdateDocumentMetadataAsync
                    // doesn't touch binary.  Use the SchedulerContext directly.
                    //
                    SchedulerContext db = context.RequestServices.GetRequiredService<SchedulerContext>();
                    var entry = db.Entry(fullDoc);
                    entry.Property(nameof(Document.fileDataData)).IsModified = true;
                    entry.Property(nameof(Document.fileSizeBytes)).IsModified = true;
                    entry.Property(nameof(Document.fileDataSize)).IsModified = true;
                    entry.Property(nameof(Document.mimeType)).IsModified = true;
                    entry.Property(nameof(Document.fileDataMimeType)).IsModified = true;
                    await db.SaveChangesAsync(context.RequestAborted);

                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }
            }
            else
            {
                //
                // New file
                //
                Document newDoc = new Document
                {
                    tenantGuid = davContext.TenantGuid,
                    documentFolderId = resolved.FolderId,
                    name = Path.GetFileNameWithoutExtension(fileName),
                    fileName = fileName,
                    mimeType = GuessMimeType(fileName),
                    fileDataFileName = fileName,
                    fileDataData = fileData,
                    fileDataSize = fileData.Length,
                    fileDataMimeType = GuessMimeType(fileName),
                    fileSizeBytes = fileData.Length,
                    uploadedBy = davContext.User.accountName,
                    versionNumber = 1
                };

                await fileService.UploadDocumentAsync(newDoc, davContext.SecurityUserId, context.RequestAborted);

                context.Response.StatusCode = StatusCodes.Status201Created;
            }
        }


        /// <summary>
        /// Guesses the MIME type from a file extension.
        /// Delegates to the shared MimeTypes utility.
        /// </summary>
        private static string GuessMimeType(string fileName) => MimeTypes.FromFileName(fileName);
    }
}
