using System;
using System.IO;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    ///
    /// Pluggable storage backend interface for conversation message and notification attachments.
    ///
    /// Implementations can store attachment content on the file system, in IndexedDB/SQLite,
    /// cloud blob storage, or any other backend.  The consuming code interacts only with this
    /// interface and never with the underlying storage directly.
    ///
    /// Each attachment is identified by a tenant GUID and an attachment GUID, enabling
    /// multi-tenant isolation.
    ///
    /// AI-developed as part of Foundation.Messaging refactoring, March 2026.
    ///
    /// </summary>
    public interface IAttachmentStorageProvider
    {
        /// <summary>
        /// Stores the content of an attachment.  Returns metadata about the stored item.
        /// </summary>
        Task<AttachmentStorageResult> StoreAsync(Guid tenantGuid, string fileName, string mimeType, Stream content);

        /// <summary>
        /// Retrieves the content of a previously stored attachment as a readable stream.
        /// Returns null if the attachment was not found.
        /// </summary>
        Task<Stream> RetrieveAsync(Guid tenantGuid, Guid attachmentGuid);

        /// <summary>
        /// Deletes a previously stored attachment.
        /// Returns true if the attachment was found and deleted, false if it was not found.
        /// </summary>
        Task<bool> DeleteAsync(Guid tenantGuid, Guid attachmentGuid);

        /// <summary>
        /// Checks whether a previously stored attachment exists.
        /// </summary>
        Task<bool> ExistsAsync(Guid tenantGuid, Guid attachmentGuid);
    }


    /// <summary>
    /// Returned by IAttachmentStorageProvider.StoreAsync to provide metadata about the stored attachment.
    /// </summary>
    public class AttachmentStorageResult
    {
        /// <summary>
        /// Unique identifier for locating the stored attachment.
        /// </summary>
        public Guid storageGuid { get; set; }

        /// <summary>
        /// Size of the stored content in bytes.
        /// </summary>
        public long contentSize { get; set; }

        /// <summary>
        /// Provider-specific storage path or key (e.g., file path, IndexedDB key).
        /// Informational only — callers should use storageGuid for retrieval.
        /// </summary>
        public string storagePath { get; set; }
    }
}
