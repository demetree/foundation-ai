// ============================================================================
//
// StorageManager.cs — Multi-provider storage manager.
//
// Provides a unified API for storage operations, routing requests
// to the configured default provider or a specific named provider.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Foundation.Networking.DeepSpace.Configuration;
using Foundation.Networking.DeepSpace.Providers;

using DeepSpaceDatabaseManager = Foundation.DeepSpace.Database.DeepSpaceDatabaseManager;

namespace Foundation.Networking.DeepSpace
{
    /// <summary>
    /// Storage operation statistics.
    /// </summary>
    public class StorageManagerStatistics
    {
        public string DefaultProvider { get; set; } = string.Empty;
        public int ProviderCount { get; set; }
        public List<string> ProviderNames { get; set; } = new List<string>();
        public long TotalPuts { get; set; }
        public long TotalGets { get; set; }
        public long TotalDeletes { get; set; }
        public long TotalErrors { get; set; }
    }


    /// <summary>
    ///
    /// Unified storage manager that routes to registered providers.
    ///
    /// </summary>
    public class StorageManager
    {
        private readonly DeepSpaceConfiguration _config;
        private readonly ILogger<StorageManager> _logger;
        private readonly ConcurrentDictionary<string, IStorageProvider> _providers;
        private readonly string _defaultProviderName;
        private readonly DeepSpaceDatabaseManager _databaseManager;

        private long _totalPuts;
        private long _totalGets;
        private long _totalDeletes;
        private long _totalErrors;


        /// <summary>
        /// The DeepSpace metadata database manager, if available.
        /// </summary>
        public DeepSpaceDatabaseManager DatabaseManager => _databaseManager;


        public StorageManager(DeepSpaceConfiguration config, ILogger<StorageManager> logger, DeepSpaceDatabaseManager databaseManager = null)
        {
            _config = config;
            _logger = logger;
            _databaseManager = databaseManager;
            _providers = new ConcurrentDictionary<string, IStorageProvider>(StringComparer.OrdinalIgnoreCase);
            _defaultProviderName = config.DefaultProvider ?? "Local";

            if (_databaseManager != null)
            {
                _logger.LogInformation("DeepSpace: metadata database is available at {Path}", _databaseManager.DatabaseFilePath);
            }
            else
            {
                _logger.LogWarning("DeepSpace: operating without metadata database — object tracking is disabled.");
            }
        }


        /// <summary>
        /// Registers a storage provider.
        /// </summary>
        public void RegisterProvider(IStorageProvider provider)
        {
            _providers[provider.ProviderName] = provider;

            _logger.LogInformation("DeepSpace: registered provider '{name}'", provider.ProviderName);
        }


        /// <summary>
        /// Gets a provider by name.
        /// </summary>
        public IStorageProvider GetProvider(string name = null)
        {
            string providerName = name ?? _defaultProviderName;

            if (_providers.TryGetValue(providerName, out IStorageProvider provider))
            {
                return provider;
            }

            return null;
        }


        /// <summary>
        /// Number of registered providers.
        /// </summary>
        public int ProviderCount => _providers.Count;


        // ── Unified Operations ────────────────────────────────────────────


        /// <summary>
        /// Stores data using the default (or named) provider.
        /// </summary>
        public async Task<StorageResult> PutAsync(
            string key, byte[] data, string contentType = null,
            Dictionary<string, string> metadata = null,
            string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);

            if (provider == null)
            {
                return new StorageResult { Success = false, Error = "Provider not found: " + (providerName ?? _defaultProviderName) };
            }

            try
            {
                StorageResult result = await provider.PutBytesAsync(key, data, contentType, metadata, cancellationToken);
                Interlocked.Increment(ref _totalPuts);

                if (result.Success == false)
                {
                    Interlocked.Increment(ref _totalErrors);
                }

                return result;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _totalErrors);
                return new StorageResult { Success = false, Error = ex.Message };
            }
        }


        /// <summary>
        /// Gets data as bytes using the default (or named) provider.
        /// </summary>
        public async Task<byte[]> GetAsync(
            string key, string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);

            if (provider == null)
            {
                return null;
            }

            try
            {
                byte[] data = await provider.GetBytesAsync(key, cancellationToken);
                Interlocked.Increment(ref _totalGets);
                return data;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _totalErrors);
                _logger.LogWarning("DeepSpace: get failed for '{key}': {error}", key, ex.Message);
                return null;
            }
        }


        /// <summary>
        /// Deletes an object using the default (or named) provider.
        /// </summary>
        public async Task<bool> DeleteAsync(
            string key, string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);

            if (provider == null)
            {
                return false;
            }

            try
            {
                bool deleted = await provider.DeleteAsync(key, cancellationToken);
                Interlocked.Increment(ref _totalDeletes);
                return deleted;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _totalErrors);
                _logger.LogWarning("DeepSpace: delete failed for '{key}': {error}", key, ex.Message);
                return false;
            }
        }


        /// <summary>
        /// Lists objects using the default (or named) provider.
        /// </summary>
        public async Task<ListResult> ListAsync(
            string prefix = "", int maxResults = 1000,
            string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);

            if (provider == null)
            {
                return new ListResult { Success = false, Error = "Provider not found" };
            }

            return await provider.ListAsync(prefix, maxResults, cancellationToken);
        }


        /// <summary>
        /// Checks if an object exists.
        /// </summary>
        public async Task<bool> ExistsAsync(
            string key, string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);

            if (provider == null)
            {
                return false;
            }

            return await provider.ExistsAsync(key, cancellationToken);
        }


        /// <summary>
        /// Copies an object within the same provider.
        /// </summary>
        public async Task<StorageResult> CopyAsync(
            string sourceKey, string destinationKey,
            string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);

            if (provider == null)
            {
                return new StorageResult { Success = false, Error = "Provider not found" };
            }

            return await provider.CopyAsync(sourceKey, destinationKey, cancellationToken);
        }


        /// <summary>
        /// Stores data from a stream using the default (or named) provider.
        /// </summary>
        public async Task<StorageResult> PutStreamAsync(
            string key, Stream data, string contentType = null,
            Dictionary<string, string> metadata = null,
            string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);

            if (provider == null)
            {
                return new StorageResult { Success = false, Error = "Provider not found: " + (providerName ?? _defaultProviderName) };
            }

            try
            {
                StorageResult result = await provider.PutAsync(key, data, contentType, metadata, cancellationToken);
                Interlocked.Increment(ref _totalPuts);

                if (result.Success == false)
                {
                    Interlocked.Increment(ref _totalErrors);
                }

                return result;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _totalErrors);
                return new StorageResult { Success = false, Error = ex.Message };
            }
        }


        /// <summary>
        /// Gets data as a stream using the default (or named) provider.
        /// </summary>
        public async Task<Stream> GetStreamAsync(
            string key, string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);

            if (provider == null)
            {
                return null;
            }

            try
            {
                Stream stream = await provider.GetStreamAsync(key, cancellationToken);
                Interlocked.Increment(ref _totalGets);
                return stream;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _totalErrors);
                _logger.LogWarning("DeepSpace: get stream failed for '{key}': {error}", key, ex.Message);
                return null;
            }
        }


        /// <summary>
        /// Gets object metadata without downloading the content.
        /// </summary>
        public async Task<StorageObject> GetMetadataAsync(
            string key, string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);

            if (provider == null)
            {
                return null;
            }

            try
            {
                return await provider.GetMetadataAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("DeepSpace: get metadata failed for '{key}': {error}", key, ex.Message);
                return null;
            }
        }


        // ── Statistics ────────────────────────────────────────────────────


        public StorageManagerStatistics GetStatistics()
        {
            return new StorageManagerStatistics
            {
                DefaultProvider = _defaultProviderName,
                ProviderCount = _providers.Count,
                ProviderNames = _providers.Keys.ToList(),
                TotalPuts = Interlocked.Read(ref _totalPuts),
                TotalGets = Interlocked.Read(ref _totalGets),
                TotalDeletes = Interlocked.Read(ref _totalDeletes),
                TotalErrors = Interlocked.Read(ref _totalErrors)
            };
        }
    }
}
