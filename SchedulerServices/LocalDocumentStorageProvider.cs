//
// LocalDocumentStorageProvider.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// IDocumentStorageProvider that stores binary content as flat files on the
// local filesystem. This is the "Lite" option — no DeepSpace dependency,
// no SQLite metadata DB, just files in a folder.
//
// Directory convention: {BasePath}/{storageKey}
// The storageKey is already a path-like string (e.g., "{tenantGuid}/documents/{objectGuid}/1/report.pdf")
// so subdirectories are created automatically.
//
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Scheduler.Server.Services
{
    /// <summary>
    /// Stores document binary content as flat files on the local filesystem.
    /// This is the lightweight option for small-scale deployments.
    /// </summary>
    public class LocalDocumentStorageProvider : IDocumentStorageProvider
    {
        private readonly string _basePath;
        private readonly ILogger<LocalDocumentStorageProvider> _logger;

        public string ProviderName => "Local";


        public LocalDocumentStorageProvider(string basePath, ILogger<LocalDocumentStorageProvider> logger)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _logger = logger;

            //
            // Ensure the base directory exists at startup
            //
            if (Directory.Exists(_basePath) == false)
            {
                Directory.CreateDirectory(_basePath);
                _logger.LogInformation("LocalDocumentStorageProvider: created base directory '{BasePath}'.", _basePath);
            }
        }


        public async Task<byte[]> GetContentAsync(string storageKey, CancellationToken ct = default)
        {
            string fullPath = GetFullPath(storageKey);

            if (File.Exists(fullPath) == false)
            {
                _logger.LogWarning("LocalDocumentStorageProvider: file not found at '{Path}'.", fullPath);
                return null;
            }

            return await File.ReadAllBytesAsync(fullPath, ct).ConfigureAwait(false);
        }


        public async Task<string> StoreContentAsync(string storageKey, byte[] data, string mimeType, CancellationToken ct = default)
        {
            string fullPath = GetFullPath(storageKey);

            //
            // Ensure the subdirectory structure exists
            //
            string directory = Path.GetDirectoryName(fullPath);
            if (directory != null && Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(fullPath, data, ct).ConfigureAwait(false);

            _logger.LogDebug("LocalDocumentStorageProvider: stored {Size} bytes at '{Key}'.", data.Length, storageKey);

            return storageKey;
        }


        public Task DeleteContentAsync(string storageKey, CancellationToken ct = default)
        {
            string fullPath = GetFullPath(storageKey);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogDebug("LocalDocumentStorageProvider: deleted file at '{Key}'.", storageKey);
            }

            return Task.CompletedTask;
        }


        public Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default)
        {
            string fullPath = GetFullPath(storageKey);
            return Task.FromResult(File.Exists(fullPath));
        }


        /// <summary>
        /// Resolves a storage key to a full filesystem path.
        /// Replaces forward slashes with the OS path separator.
        /// </summary>
        private string GetFullPath(string storageKey)
        {
            //
            // Normalize the key: replace forward slashes with OS separator
            //
            string normalizedKey = storageKey.Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(_basePath, normalizedKey);
        }
    }
}
