//
// PropFindHandler.cs
//
// Handles PROPFIND requests — the workhorse of WebDAV browsing.
// Returns a 207 Multi-Status XML response listing properties of the requested
// resource and optionally its children (Depth: 0 or 1).
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
using Scheduler.Server.Services;
using Scheduler.WebDAV.Services;
using Scheduler.WebDAV.Xml;

namespace Scheduler.WebDAV.Handlers
{
    public static class PropFindHandler
    {
        public static async Task HandleAsync(HttpContext context, WebDavContext davContext)
        {
            IFileStorageService fileService = context.RequestServices.GetRequiredService<IFileStorageService>();

            string path = context.Request.Path.Value ?? "/";
            string depthHeader = context.Request.Headers["Depth"].FirstOrDefault() ?? "1";

            //
            // Parse depth — only 0 and 1 are supported in Phase 1 (no infinity)
            //
            int depth = depthHeader == "0" ? 0 : 1;


            //
            // Resolve the requested path to a folder or document
            //
            PathResolver.ResolvedPath resolved = await PathResolver.ResolveAsync(
                path, davContext.TenantGuid, fileService, context.RequestAborted);


            //
            // Get all folders (needed for path building)
            //
            List<DocumentFolder> allFolders = await fileService.GetFoldersAsync(
                davContext.TenantGuid, context.RequestAborted);

            List<XElement> responses = new List<XElement>();


            if (!resolved.Found && resolved.DocumentName != null)
            {
                //
                // Could be a file at the resolved parent folder
                //
                List<Document> docsInFolder = await fileService.GetDocumentsInFolderAsync(
                    resolved.FolderId, davContext.TenantGuid, context.RequestAborted);

                Document doc = docsInFolder.FirstOrDefault(d =>
                    string.Equals(d.fileName, resolved.DocumentName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(d.name, resolved.DocumentName, StringComparison.OrdinalIgnoreCase));

                if (doc != null)
                {
                    //
                    // Single file response
                    //
                    string docPath = PathResolver.BuildDocumentPath(doc, allFolders);

                    responses.Add(DavXmlBuilder.FileResponse(
                        docPath,
                        doc.fileName ?? doc.name,
                        doc.fileSizeBytes,
                        doc.mimeType ?? doc.fileDataMimeType ?? "application/octet-stream",
                        doc.uploadedDate,
                        doc.uploadedDate,
                        doc.objectGuid.ToString("N")));
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
            }
            else if (!resolved.Found)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            else if (resolved.IsCollection)
            {
                //
                // Collection (folder) response
                //

                // Add the folder itself
                if (resolved.FolderId == null)
                {
                    // Root collection
                    responses.Add(DavXmlBuilder.CollectionResponse("/", "/", DateTime.UtcNow));
                }
                else
                {
                    string folderPath = PathResolver.BuildFolderPath(resolved.Folder, allFolders);
                    responses.Add(DavXmlBuilder.CollectionResponse(
                        folderPath,
                        resolved.Folder.name,
                        DateTime.UtcNow));
                }


                if (depth == 1)
                {
                    //
                    // Add child folders
                    //
                    List<DocumentFolder> childFolders = allFolders
                        .Where(f => f.parentDocumentFolderId == resolved.FolderId)
                        .OrderBy(f => f.sequence)
                        .ThenBy(f => f.name)
                        .ToList();

                    foreach (DocumentFolder childFolder in childFolders)
                    {
                        string childPath = PathResolver.BuildFolderPath(childFolder, allFolders);
                        responses.Add(DavXmlBuilder.CollectionResponse(
                            childPath,
                            childFolder.name,
                            DateTime.UtcNow));
                    }

                    //
                    // Add child documents
                    //
                    List<Document> documents = await fileService.GetDocumentsInFolderAsync(
                        resolved.FolderId, davContext.TenantGuid, context.RequestAborted);

                    foreach (Document doc in documents)
                    {
                        string docPath = PathResolver.BuildDocumentPath(doc, allFolders);
                        responses.Add(DavXmlBuilder.FileResponse(
                            docPath,
                            doc.fileName ?? doc.name,
                            doc.fileSizeBytes,
                            doc.mimeType ?? doc.fileDataMimeType ?? "application/octet-stream",
                            doc.uploadedDate,
                            doc.uploadedDate,
                            doc.objectGuid.ToString("N")));
                    }
                }
            }
            else if (resolved.DocumentName != null)
            {
                //
                // Path points to a potential file name within a resolved folder
                //
                List<Document> docsInFolder = await fileService.GetDocumentsInFolderAsync(
                    resolved.FolderId, davContext.TenantGuid, context.RequestAborted);

                Document doc = docsInFolder.FirstOrDefault(d =>
                    string.Equals(d.fileName, resolved.DocumentName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(d.name, resolved.DocumentName, StringComparison.OrdinalIgnoreCase));

                if (doc != null)
                {
                    string docPath = PathResolver.BuildDocumentPath(doc, allFolders);
                    responses.Add(DavXmlBuilder.FileResponse(
                        docPath,
                        doc.fileName ?? doc.name,
                        doc.fileSizeBytes,
                        doc.mimeType ?? doc.fileDataMimeType ?? "application/octet-stream",
                        doc.uploadedDate,
                        doc.uploadedDate,
                        doc.objectGuid.ToString("N")));
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }


            //
            // Write the 207 Multi-Status XML response
            //
            XDocument xml = DavXmlBuilder.MultiStatus(responses);

            context.Response.StatusCode = 207; // Multi-Status
            context.Response.ContentType = "application/xml; charset=utf-8";

            using (MemoryStream ms = new MemoryStream())
            {
                xml.Save(ms);
                context.Response.ContentLength = ms.Length;
                ms.Position = 0;
                await ms.CopyToAsync(context.Response.Body, context.RequestAborted);
            }
        }
    }
}
