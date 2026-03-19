//
// IFileStorageService.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Abstraction layer for file storage operations used by the File Manager feature.
//
// The initial implementation (SqlFileStorageService) stores files as binary data in
// the Document table.  This interface allows a future cloud-storage implementation
// (e.g., Azure Blob Storage) without changing the controller or UI surface.
//
// All methods are tenant-scoped — callers must supply the tenant GUID to ensure
// multi-tenant isolation.
//
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Scheduler.Database;

namespace Scheduler.Server.Services
{
    /// <summary>
    /// Defines the contract for file storage operations backing the Document Manager feature.
    /// </summary>
    public interface IFileStorageService
    {
        // ─── Folder Operations ───────────────────────────────────────────────

        /// <summary>
        /// Returns all folders (non-deleted) for the given tenant.
        /// The caller is responsible for assembling the hierarchy on the client side.
        /// </summary>
        Task<List<DocumentFolder>> GetFoldersAsync(Guid tenantGuid, CancellationToken ct = default);

        /// <summary>
        /// Returns a single folder by ID, including its parent nav property.
        /// </summary>
        Task<DocumentFolder> GetFolderByIdAsync(int folderId, Guid tenantGuid, CancellationToken ct = default);

        /// <summary>
        /// Creates a new folder and returns the saved entity.
        /// </summary>
        Task<DocumentFolder> CreateFolderAsync(DocumentFolder folder, int securityUserId, CancellationToken ct = default);

        /// <summary>
        /// Updates an existing folder (name, description, parentId, color, icon, etc.) and returns it.
        /// </summary>
        Task<DocumentFolder> UpdateFolderAsync(DocumentFolder folder, int securityUserId, CancellationToken ct = default);

        /// <summary>
        /// Soft-deletes a folder (sets deleted = true).  Optionally cascades to child folders/documents.
        /// </summary>
        Task DeleteFolderAsync(int folderId, Guid tenantGuid, int securityUserId, bool cascade = false, CancellationToken ct = default);


        // ─── Document / File Operations ──────────────────────────────────────

        /// <summary>
        /// Returns documents (metadata only — no binary data) in a specific folder, or at the root level when folderId is null.
        /// </summary>
        Task<List<Document>> GetDocumentsInFolderAsync(int? folderId, Guid tenantGuid, CancellationToken ct = default);

        /// <summary>
        /// Returns a single document by ID, including binary content.
        /// </summary>
        Task<Document> GetDocumentByIdAsync(int documentId, Guid tenantGuid, CancellationToken ct = default);

        /// <summary>
        /// Stores a new document (metadata + binary) and returns the saved entity.
        /// </summary>
        Task<Document> UploadDocumentAsync(Document document, int securityUserId, CancellationToken ct = default);

        /// <summary>
        /// Updates document metadata (name, description, folder, tags, entity links, etc.) without re-uploading binary.
        /// </summary>
        Task<Document> UpdateDocumentMetadataAsync(Document document, int securityUserId, CancellationToken ct = default);

        /// <summary>
        /// Moves a document to a different folder.
        /// </summary>
        Task MoveDocumentAsync(int documentId, int? targetFolderId, Guid tenantGuid, int securityUserId, CancellationToken ct = default);

        /// <summary>
        /// Soft-deletes a document.
        /// </summary>
        Task DeleteDocumentAsync(int documentId, Guid tenantGuid, int securityUserId, CancellationToken ct = default);


        // ─── Search ──────────────────────────────────────────────────────────

        /// <summary>
        /// Searches documents by name, fileName, or description (case-insensitive contains).
        /// Returns metadata only — no binary data.
        /// </summary>
        Task<List<Document>> SearchDocumentsAsync(string query, Guid tenantGuid, CancellationToken ct = default);


        // ─── Tag Operations ──────────────────────────────────────────────────

        /// <summary>
        /// Returns all tags for the tenant.
        /// </summary>
        Task<List<DocumentTag>> GetTagsAsync(Guid tenantGuid, CancellationToken ct = default);

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        Task<DocumentTag> CreateTagAsync(DocumentTag tag, int securityUserId, CancellationToken ct = default);

        /// <summary>
        /// Adds a tag to a document.
        /// </summary>
        Task AddTagToDocumentAsync(int documentId, int tagId, Guid tenantGuid, int securityUserId, CancellationToken ct = default);

        /// <summary>
        /// Removes a tag from a document.
        /// </summary>
        Task RemoveTagFromDocumentAsync(int documentId, int tagId, Guid tenantGuid, int securityUserId, CancellationToken ct = default);

        /// <summary>
        /// Returns all tags attached to a specific document.
        /// </summary>
        Task<List<DocumentTag>> GetTagsForDocumentAsync(int documentId, Guid tenantGuid, CancellationToken ct = default);
    }
}
