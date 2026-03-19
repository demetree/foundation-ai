//
// PathResolver.cs
//
// Utility class that resolves a WebDAV URL path (e.g., "/Reports/2024/invoice.pdf")
// into a folder chain and optional document name relative to the tenant's file storage.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Scheduler.Database;
using Scheduler.Server.Services;

namespace Scheduler.WebDAV.Handlers
{
    /// <summary>
    /// Resolves WebDAV paths to folder IDs and document records.
    /// </summary>
    public static class PathResolver
    {
        /// <summary>
        /// Result of resolving a WebDAV path.
        /// </summary>
        public class ResolvedPath
        {
            /// <summary>
            /// The resolved parent folder ID.  Null means root.
            /// </summary>
            public int? FolderId { get; set; }

            /// <summary>
            /// The resolved DocumentFolder entity (null when at root).
            /// </summary>
            public DocumentFolder Folder { get; set; }

            /// <summary>
            /// If the path points to a document, this is the document name (last segment).
            /// Null if the path ends with '/' (i.e., points to a collection).
            /// </summary>
            public string DocumentName { get; set; }

            /// <summary>
            /// True if the path ends with '/' (treated as a collection request).
            /// </summary>
            public bool IsCollection { get; set; }

            /// <summary>
            /// True if the path could be fully resolved (all folders exist).
            /// </summary>
            public bool Found { get; set; }

            /// <summary>
            /// The segments that could not be resolved (useful for MKCOL / PUT).
            /// </summary>
            public string[] UnresolvedSegments { get; set; }
        }


        /// <summary>
        /// Resolves a URL path to a folder chain.
        /// 
        /// For a path like "/Reports/2024/invoice.pdf":
        ///   - Walks: root → "Reports" → "2024"
        ///   - Sets DocumentName = "invoice.pdf"
        /// 
        /// For a path like "/Reports/2024/":
        ///   - Walks: root → "Reports" → "2024"
        ///   - IsCollection = true, DocumentName = null
        /// </summary>
        public static async Task<ResolvedPath> ResolveAsync(
            string urlPath,
            Guid tenantGuid,
            IFileStorageService fileService,
            CancellationToken ct = default)
        {
            //
            // Normalize path: trim leading/trailing slashes, split into segments
            //
            bool endsWithSlash = urlPath.EndsWith("/");
            string trimmed = urlPath.Trim('/');

            if (string.IsNullOrEmpty(trimmed))
            {
                // Root
                return new ResolvedPath
                {
                    FolderId = null,
                    Folder = null,
                    DocumentName = null,
                    IsCollection = true,
                    Found = true,
                    UnresolvedSegments = Array.Empty<string>()
                };
            }

            //
            // URL-decode each segment
            //
            string[] rawSegments = trimmed.Split('/');
            string[] segments = new string[rawSegments.Length];
            for (int i = 0; i < rawSegments.Length; i++)
            {
                segments[i] = Uri.UnescapeDataString(rawSegments[i]);
            }


            //
            // Get all folders for this tenant
            //
            List<DocumentFolder> allFolders = await fileService.GetFoldersAsync(tenantGuid, ct);


            //
            // Determine how many segments are folder names vs. the last segment being a file
            //
            int folderSegmentCount = endsWithSlash ? segments.Length : segments.Length - 1;
            string potentialDocName = endsWithSlash ? null : segments[segments.Length - 1];


            //
            // Walk the folder tree
            //
            int? currentParentId = null;
            DocumentFolder currentFolder = null;

            for (int i = 0; i < folderSegmentCount; i++)
            {
                DocumentFolder match = allFolders.FirstOrDefault(f =>
                    f.parentDocumentFolderId == currentParentId &&
                    string.Equals(f.name, segments[i], StringComparison.OrdinalIgnoreCase));

                if (match == null)
                {
                    //
                    // Folder not found at this level
                    //
                    string[] unresolved = new string[segments.Length - i];
                    Array.Copy(segments, i, unresolved, 0, unresolved.Length);

                    return new ResolvedPath
                    {
                        FolderId = currentParentId,
                        Folder = currentFolder,
                        DocumentName = null,
                        IsCollection = false,
                        Found = false,
                        UnresolvedSegments = unresolved
                    };
                }

                currentParentId = match.id;
                currentFolder = match;
            }


            //
            // If the path doesn't end with "/" and the last segment might be a folder too,
            // check if it matches a folder (WebDAV clients sometimes omit the trailing slash)
            //
            if (!endsWithSlash && potentialDocName != null)
            {
                DocumentFolder possibleFolder = allFolders.FirstOrDefault(f =>
                    f.parentDocumentFolderId == currentParentId &&
                    string.Equals(f.name, potentialDocName, StringComparison.OrdinalIgnoreCase));

                if (possibleFolder != null)
                {
                    return new ResolvedPath
                    {
                        FolderId = possibleFolder.id,
                        Folder = possibleFolder,
                        DocumentName = null,
                        IsCollection = true,
                        Found = true,
                        UnresolvedSegments = Array.Empty<string>()
                    };
                }
            }


            return new ResolvedPath
            {
                FolderId = currentParentId,
                Folder = currentFolder,
                DocumentName = potentialDocName,
                IsCollection = endsWithSlash || potentialDocName == null,
                Found = true,
                UnresolvedSegments = Array.Empty<string>()
            };
        }


        /// <summary>
        /// Builds a WebDAV path string from a folder's ancestor chain.
        /// Example: folder "2024" under "Reports" → "/Reports/2024/"
        /// </summary>
        public static string BuildFolderPath(DocumentFolder folder, List<DocumentFolder> allFolders)
        {
            List<string> parts = new List<string>();
            DocumentFolder current = folder;

            while (current != null)
            {
                parts.Insert(0, Uri.EscapeDataString(current.name));

                current = current.parentDocumentFolderId.HasValue
                    ? allFolders.FirstOrDefault(f => f.id == current.parentDocumentFolderId.Value)
                    : null;
            }

            return "/" + string.Join("/", parts) + "/";
        }


        /// <summary>
        /// Builds a WebDAV path string for a document in a folder.
        /// Example: "invoice.pdf" in folder "2024" under "Reports" → "/Reports/2024/invoice.pdf"
        /// </summary>
        public static string BuildDocumentPath(Document document, List<DocumentFolder> allFolders)
        {
            if (document.documentFolderId.HasValue)
            {
                DocumentFolder folder = allFolders.FirstOrDefault(f => f.id == document.documentFolderId.Value);
                if (folder != null)
                {
                    string folderPath = BuildFolderPath(folder, allFolders);
                    return folderPath + Uri.EscapeDataString(document.fileName ?? document.name);
                }
            }

            return "/" + Uri.EscapeDataString(document.fileName ?? document.name);
        }
    }
}
