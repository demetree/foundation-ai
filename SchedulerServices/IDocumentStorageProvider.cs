//
// IDocumentStorageProvider.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Abstraction for binary document content storage. Separates the physical
// location of file content from the metadata layer (Document table in SQL).
//
// Implementations:
//   - SqlDocumentStorageProvider: stores binary inline in Document.fileDataData (current default)
//   - LocalDocumentStorageProvider: stores binary as flat files on disk (lite mode)
//   - DeepSpaceDocumentStorageProvider: stores via DeepSpace StorageManager (standard/enterprise)
//
// The storageKey is the unique identifier for the content in the provider.
// For SQL storage, it maps to a document ID. For file/DeepSpace, it's a path-like key.
//
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler.Server.Services
{
    /// <summary>
    /// Defines the contract for binary document content storage, decoupled from metadata.
    /// </summary>
    public interface IDocumentStorageProvider
    {
        /// <summary>
        /// The name of this provider (e.g., "Sql", "Local", "DeepSpace").
        /// </summary>
        string ProviderName { get; }


        /// <summary>
        /// Retrieves binary content for a document by its storage key.
        /// Returns null if the content is not found.
        /// </summary>
        Task<byte[]> GetContentAsync(string storageKey, CancellationToken ct = default);


        /// <summary>
        /// Stores binary content and returns the storage key that can be used to retrieve it.
        /// If the storageKey already exists, the content is overwritten.
        /// </summary>
        Task<string> StoreContentAsync(string storageKey, byte[] data, string mimeType, CancellationToken ct = default);


        /// <summary>
        /// Deletes binary content from the provider.
        /// Returns silently if the key does not exist.
        /// </summary>
        Task DeleteContentAsync(string storageKey, CancellationToken ct = default);


        /// <summary>
        /// Returns true if content exists at the given storage key.
        /// </summary>
        Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default);
    }
}
