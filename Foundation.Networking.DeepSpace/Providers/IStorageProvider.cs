// ============================================================================
//
// IStorageProvider.cs — Storage provider interface and models.
//
// Defines the contract that all storage providers (Local, S3, Azure)
// must implement, plus common result models.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Networking.DeepSpace.Providers
{
    /// <summary>
    /// Metadata about a stored object.
    /// </summary>
    public class StorageObject
    {
        /// <summary>
        /// Object key (path within the storage container).
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Object size in bytes.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Content type (MIME).
        /// </summary>
        public string ContentType { get; set; } = "application/octet-stream";

        /// <summary>
        /// When the object was last modified.
        /// </summary>
        public DateTime LastModifiedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ETag or hash for change detection.
        /// </summary>
        public string ETag { get; set; } = string.Empty;

        /// <summary>
        /// Custom metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }


    /// <summary>
    /// Result of a storage operation.
    /// </summary>
    public class StorageResult
    {
        /// <summary>
        /// Whether the operation succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if failed.
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Object metadata (for put/get operations).
        /// </summary>
        public StorageObject Object { get; set; }
    }


    /// <summary>
    /// Result of a list operation.
    /// </summary>
    public class ListResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public List<StorageObject> Objects { get; set; } = new List<StorageObject>();
        public bool HasMore { get; set; } = false;
        public string ContinuationToken { get; set; } = string.Empty;
    }


    /// <summary>
    ///
    /// Interface for storage providers.
    ///
    /// All providers must implement this interface to be pluggable
    /// into the Deep Space storage manager.
    ///
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Provider name (e.g., "Local", "S3", "AzureBlob").
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Stores data from a stream.
        /// </summary>
        Task<StorageResult> PutAsync(string key, Stream data, string contentType = null, Dictionary<string, string> metadata = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores data from bytes.
        /// </summary>
        Task<StorageResult> PutBytesAsync(string key, byte[] data, string contentType = null, Dictionary<string, string> metadata = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets data as a stream.
        /// </summary>
        Task<Stream> GetStreamAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets data as bytes.
        /// </summary>
        Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets object metadata without downloading the content.
        /// </summary>
        Task<StorageObject> GetMetadataAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether an object exists.
        /// </summary>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an object.
        /// </summary>
        Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists objects with an optional prefix filter.
        /// </summary>
        Task<ListResult> ListAsync(string prefix = "", int maxResults = 1000, CancellationToken cancellationToken = default);

        /// <summary>
        /// Copies an object to a new key.
        /// </summary>
        Task<StorageResult> CopyAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default);
    }
}
