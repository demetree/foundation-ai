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
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Foundation.Networking.DeepSpace.Configuration;
using Foundation.Networking.DeepSpace.Providers;

using DeepSpaceDatabaseManager = Foundation.DeepSpace.Database.DeepSpaceDatabaseManager;
using DbStorageObject = Foundation.DeepSpace.Database.StorageObject;
using DbStorageObjectVersion = Foundation.DeepSpace.Database.StorageObjectVersion;
using DbStorageProvider = Foundation.DeepSpace.Database.StorageProvider;
using DbStorageTier = Foundation.DeepSpace.Database.StorageTier;

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
        /// Gets a short-lived presigned URL for direct client download.
        /// </summary>
        public async Task<string> GetPresignedUrlAsync(
            string key, TimeSpan expires, string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);

            if (provider == null)
            {
                return null;
            }

            try
            {
                string url = await provider.GetPresignedUrlAsync(key, expires, cancellationToken);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("DeepSpace: get presigned URL failed for '{key}': {error}", key, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Stores data using the default (or named) provider.
        /// </summary>
        public async Task<StorageResult> PutAsync(
            string key, byte[] data, string contentType = null,
            Dictionary<string, string> metadata = null,
            string providerName = null,
            CancellationToken cancellationToken = default)
        {
            if (StorageObjectSidecar.IsSidecarKey(key))
            {
                return new StorageResult { Success = false, Error = "Cannot store objects with the reserved .deepspace.json extension." };
            }

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
                else
                {
                    string md5 = ComputeMd5Hash(data);
                    RecordPutMetadata(key, provider.ProviderName, contentType, data.Length, md5);
                    await WriteSidecarAsync(key, provider, contentType, data.Length, md5, cancellationToken);
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

                if (data != null)
                {
                    RecordAccessMetadata(key, provider.ProviderName);
                }

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

                if (deleted)
                {
                    RecordDeleteMetadata(key, provider.ProviderName);

                    //
                    // Remove the sidecar file
                    //
                    try
                    {
                        string sidecarKey = StorageObjectSidecar.GetSidecarKey(key);
                        await provider.DeleteAsync(sidecarKey, cancellationToken);
                    }
                    catch (Exception sidecarEx)
                    {
                        _logger.LogWarning("DeepSpace: sidecar delete failed for '{key}': {error}", key, sidecarEx.Message);
                    }
                }

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
        /// Sidecar metadata files (.deepspace.json) are automatically excluded.
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

            ListResult result = await provider.ListAsync(prefix, maxResults, cancellationToken);

            if (result.Success && result.Objects != null)
            {
                result.Objects = result.Objects
                    .Where(o => StorageObjectSidecar.IsSidecarKey(o.Key) == false)
                    .ToList();
            }

            return result;
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


        // ── Advanced Operations (Bucket, Lifecycle, Metadata) ─────────────


        /// <summary>
        /// Creates a new storage bucket/container using the default (or named) provider.
        /// </summary>
        public async Task<bool> CreateBucketAsync(
            string bucketName, string providerName = null, 
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);
            if (provider == null) return false;
            return await provider.CreateBucketAsync(bucketName, cancellationToken);
        }

        /// <summary>
        /// Lists all existing buckets/containers.
        /// </summary>
        public async Task<List<string>> ListBucketsAsync(
            string providerName = null, 
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);
            if (provider == null) return new List<string>();
            return await provider.ListBucketsAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes an empty storage bucket/container.
        /// </summary>
        public async Task<bool> DeleteBucketAsync(
            string bucketName, string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);
            if (provider == null) return false;
            return await provider.DeleteBucketAsync(bucketName, cancellationToken);
        }

        /// <summary>
        /// Sets an expiration lifecycle rule on the configured default bucket.
        /// </summary>
        public async Task<bool> SetExpirationLifecycleAsync(
            int expirationDays, string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);
            if (provider == null) return false;
            return await provider.SetExpirationLifecycleAsync(expirationDays, cancellationToken);
        }

        /// <summary>
        /// Updates custom metadata for an existing object.
        /// </summary>
        public async Task<bool> UpdateMetadataAsync(
            string key, Dictionary<string, string> metadata, 
            string providerName = null,
            CancellationToken cancellationToken = default)
        {
            IStorageProvider provider = GetProvider(providerName);
            if (provider == null) return false;
            return await provider.UpdateMetadataAsync(key, metadata, cancellationToken);
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
            if (StorageObjectSidecar.IsSidecarKey(key))
            {
                return new StorageResult { Success = false, Error = "Cannot store objects with the reserved .deepspace.json extension." };
            }

            IStorageProvider provider = GetProvider(providerName);

            if (provider == null)
            {
                return new StorageResult { Success = false, Error = "Provider not found: " + (providerName ?? _defaultProviderName) };
            }

            try
            {
                //
                // Capture stream length before the provider consumes it
                //
                long streamLength = 0;
                string md5 = null;

                if (data.CanSeek)
                {
                    streamLength = data.Length;
                    md5 = ComputeMd5Hash(data);
                    data.Position = 0;
                }

                StorageResult result = await provider.PutAsync(key, data, contentType, metadata, cancellationToken);
                Interlocked.Increment(ref _totalPuts);

                if (result.Success == false)
                {
                    Interlocked.Increment(ref _totalErrors);
                }
                else
                {
                    RecordPutMetadata(key, provider.ProviderName, contentType, streamLength, md5);
                    await WriteSidecarAsync(key, provider, contentType, streamLength, md5, cancellationToken);
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

                if (stream != null)
                {
                    RecordAccessMetadata(key, provider.ProviderName);
                }

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


        // ── Persistence Helpers ────────────────────────────────────────────


        /// <summary>
        /// Records metadata to the SQLite database after a successful Put.
        /// Fire-and-forget: errors are logged but do not fail the operation.
        /// </summary>
        private void RecordPutMetadata(string key, string providerName, string contentType, long sizeBytes, string md5Hash)
        {
            if (_databaseManager == null) return;

            try
            {
                _databaseManager.ExecuteWrite(context =>
                {
                    //
                    // Ensure the StorageProvider row exists
                    //
                    DbStorageProvider dbProvider = context.StorageProviders
                        .FirstOrDefault(p => p.name == providerName);

                    if (dbProvider == null)
                    {
                        dbProvider = new DbStorageProvider
                        {
                            name = providerName,
                            description = providerName + " (auto-created)",
                            storageProviderTypeId = 1,
                            isEnabled = true,
                            isDefault = providerName.Equals(_defaultProviderName, StringComparison.OrdinalIgnoreCase),
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false
                        };
                        context.StorageProviders.Add(dbProvider);
                        context.SaveChanges();
                    }


                    //
                    // Ensure a default StorageTier exists
                    //
                    DbStorageTier dbTier = context.StorageTiers.FirstOrDefault();

                    if (dbTier == null)
                    {
                        dbTier = new DbStorageTier
                        {
                            name = "Standard",
                            description = "Default storage tier",
                            sequence = 1,
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false
                        };
                        context.StorageTiers.Add(dbTier);
                        context.SaveChanges();
                    }


                    //
                    // Find or create the StorageObject
                    //
                    DbStorageObject dbObject = context.StorageObjects
                        .FirstOrDefault(o => o.key == key && o.storageProviderId == dbProvider.id);

                    if (dbObject == null)
                    {
                        dbObject = new DbStorageObject
                        {
                            key = key,
                            storageProviderId = dbProvider.id,
                            storageTierId = dbTier.id,
                            sizeBytes = (int)Math.Min(sizeBytes, int.MaxValue),
                            contentType = contentType,
                            md5Hash = md5Hash,
                            versionNumber = 1,
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false,
                            isDeleted = false,
                            accessCount = 0
                        };
                        context.StorageObjects.Add(dbObject);
                    }
                    else
                    {
                        dbObject.sizeBytes = (int)Math.Min(sizeBytes, int.MaxValue);
                        dbObject.contentType = contentType;
                        dbObject.md5Hash = md5Hash;
                        dbObject.versionNumber++;
                        dbObject.isDeleted = false;
                        dbObject.deletedUtc = null;
                        dbObject.deletedByUserGuid = null;
                    }

                    context.SaveChanges();


                    //
                    // Create a StorageObjectVersion snapshot
                    //
                    DbStorageObjectVersion dbVersion = new DbStorageObjectVersion
                    {
                        storageObjectId = dbObject.id,
                        versionNumber = dbObject.versionNumber,
                        storageProviderId = dbProvider.id,
                        providerKey = key,
                        sizeBytes = (int)Math.Min(sizeBytes, int.MaxValue),
                        md5Hash = md5Hash,
                        createdUtc = DateTime.UtcNow,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    context.StorageObjectVersions.Add(dbVersion);

                    context.SaveChanges();
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("DeepSpace: metadata persistence failed for put '{key}': {error}", key, ex.Message);
            }
        }


        /// <summary>
        /// Updates access tracking metadata after a successful Get.
        /// </summary>
        private void RecordAccessMetadata(string key, string providerName)
        {
            if (_databaseManager == null) return;

            try
            {
                _databaseManager.ExecuteWrite(context =>
                {
                    DbStorageProvider dbProvider = context.StorageProviders
                        .FirstOrDefault(p => p.name == providerName);

                    if (dbProvider == null) return;

                    DbStorageObject dbObject = context.StorageObjects
                        .FirstOrDefault(o => o.key == key && o.storageProviderId == dbProvider.id);

                    if (dbObject != null)
                    {
                        dbObject.lastAccessedUtc = DateTime.UtcNow;
                        dbObject.accessCount++;
                        context.SaveChanges();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("DeepSpace: access tracking failed for '{key}': {error}", key, ex.Message);
            }
        }


        /// <summary>
        /// Soft-deletes the StorageObject record after a successful provider delete.
        /// </summary>
        private void RecordDeleteMetadata(string key, string providerName)
        {
            if (_databaseManager == null) return;

            try
            {
                _databaseManager.ExecuteWrite(context =>
                {
                    DbStorageProvider dbProvider = context.StorageProviders
                        .FirstOrDefault(p => p.name == providerName);

                    if (dbProvider == null) return;

                    DbStorageObject dbObject = context.StorageObjects
                        .FirstOrDefault(o => o.key == key && o.storageProviderId == dbProvider.id);

                    if (dbObject != null)
                    {
                        dbObject.isDeleted = true;
                        dbObject.deletedUtc = DateTime.UtcNow;
                        dbObject.versionNumber++;
                        context.SaveChanges();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("DeepSpace: delete tracking failed for '{key}': {error}", key, ex.Message);
            }
        }


        /// <summary>
        /// Writes a .deepspace.json sidecar file alongside the stored object.
        /// </summary>
        private async Task WriteSidecarAsync(
            string key, IStorageProvider provider, string contentType,
            long sizeBytes, string md5Hash,
            CancellationToken cancellationToken)
        {
            try
            {
                //
                // Read current version from DB to include in sidecar
                //
                int versionNumber = 1;
                Guid objectGuid = Guid.NewGuid();
                DateTime createdUtc = DateTime.UtcNow;

                if (_databaseManager != null)
                {
                    _databaseManager.ExecuteRead(context =>
                    {
                        DbStorageProvider dbProvider = context.StorageProviders
                            .FirstOrDefault(p => p.name == provider.ProviderName);

                        if (dbProvider != null)
                        {
                            DbStorageObject dbObject = context.StorageObjects
                                .FirstOrDefault(o => o.key == key && o.storageProviderId == dbProvider.id);

                            if (dbObject != null)
                            {
                                versionNumber = dbObject.versionNumber;
                                objectGuid = dbObject.objectGuid;
                            }
                        }

                        return 0;
                    });
                }

                StorageObjectSidecar sidecar = new StorageObjectSidecar
                {
                    Key = key,
                    ContentType = contentType,
                    SizeBytes = sizeBytes,
                    Md5Hash = md5Hash,
                    Provider = provider.ProviderName,
                    Tier = "Standard",
                    VersionNumber = versionNumber,
                    CreatedUtc = createdUtc,
                    ObjectGuid = objectGuid,
                    LastUpdatedUtc = DateTime.UtcNow
                };

                byte[] sidecarBytes = sidecar.ToJsonBytes();
                string sidecarKey = StorageObjectSidecar.GetSidecarKey(key);

                await provider.PutBytesAsync(sidecarKey, sidecarBytes, "application/json", null, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("DeepSpace: sidecar write failed for '{key}': {error}", key, ex.Message);
            }
        }


        /// <summary>
        /// Computes an MD5 hash from a byte array.
        /// </summary>
        private static string ComputeMd5Hash(byte[] data)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
        }


        /// <summary>
        /// Computes an MD5 hash from a seekable stream (resets position after).
        /// </summary>
        private static string ComputeMd5Hash(Stream data)
        {
            long originalPosition = data.Position;

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                data.Position = originalPosition;
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
        }


        // ── Disaster Recovery ─────────────────────────────────────────────


        /// <summary>
        /// Scans all registered storage providers for .deepspace.json sidecar files
        /// and rebuilds the metadata database from them.
        /// 
        /// This is an admin-only disaster recovery tool — use when the SQLite
        /// database is lost or corrupted. It creates fresh StorageProvider,
        /// StorageObject, and StorageObjectVersion rows for each sidecar found.
        /// </summary>
        public async Task<int> RebuildFromProvidersAsync(CancellationToken cancellationToken = default)
        {
            if (_databaseManager == null)
            {
                _logger.LogError("DeepSpace: cannot rebuild — database manager is not available.");
                return 0;
            }

            int totalRecovered = 0;

            foreach (var kvp in _providers)
            {
                string providerName = kvp.Key;
                IStorageProvider provider = kvp.Value;

                _logger.LogInformation("DeepSpace: scanning provider '{provider}' for sidecar files...", providerName);

                try
                {
                    //
                    // List all objects (including sidecars, since we need them)
                    //
                    ListResult listResult = await provider.ListAsync("", int.MaxValue, cancellationToken);

                    if (listResult.Success == false)
                    {
                        _logger.LogWarning("DeepSpace: could not list provider '{provider}': {error}", providerName, listResult.Error);
                        continue;
                    }

                    List<Providers.StorageObject> sidecarObjects = listResult.Objects
                        .Where(o => StorageObjectSidecar.IsSidecarKey(o.Key))
                        .ToList();

                    _logger.LogInformation("DeepSpace: found {count} sidecar files in provider '{provider}'.", sidecarObjects.Count, providerName);

                    foreach (Providers.StorageObject sidecarObj in sidecarObjects)
                    {
                        try
                        {
                            byte[] sidecarBytes = await provider.GetBytesAsync(sidecarObj.Key, cancellationToken);

                            if (sidecarBytes == null) continue;

                            StorageObjectSidecar sidecar = StorageObjectSidecar.FromJsonBytes(sidecarBytes);

                            _databaseManager.ExecuteWrite(context =>
                            {
                                //
                                // Ensure provider row
                                //
                                DbStorageProvider dbProvider = context.StorageProviders
                                    .FirstOrDefault(p => p.name == sidecar.Provider);

                                if (dbProvider == null)
                                {
                                    dbProvider = new DbStorageProvider
                                    {
                                        name = sidecar.Provider,
                                        description = sidecar.Provider + " (recovered)",
                                        storageProviderTypeId = 1,
                                        isEnabled = true,
                                        isDefault = sidecar.Provider.Equals(_defaultProviderName, StringComparison.OrdinalIgnoreCase),
                                        objectGuid = Guid.NewGuid(),
                                        active = true,
                                        deleted = false
                                    };
                                    context.StorageProviders.Add(dbProvider);
                                    context.SaveChanges();
                                }


                                //
                                // Ensure tier row
                                //
                                DbStorageTier dbTier = context.StorageTiers.FirstOrDefault(t => t.name == sidecar.Tier);

                                if (dbTier == null)
                                {
                                    dbTier = new DbStorageTier
                                    {
                                        name = sidecar.Tier ?? "Standard",
                                        description = "Recovered tier",
                                        sequence = 1,
                                        objectGuid = Guid.NewGuid(),
                                        active = true,
                                        deleted = false
                                    };
                                    context.StorageTiers.Add(dbTier);
                                    context.SaveChanges();
                                }


                                //
                                // Skip if already recovered
                                //
                                DbStorageObject existing = context.StorageObjects
                                    .FirstOrDefault(o => o.key == sidecar.Key && o.storageProviderId == dbProvider.id);

                                if (existing != null) return;


                                //
                                // Create StorageObject
                                //
                                DbStorageObject dbObject = new DbStorageObject
                                {
                                    key = sidecar.Key,
                                    storageProviderId = dbProvider.id,
                                    storageTierId = dbTier.id,
                                    sizeBytes = (int)Math.Min(sidecar.SizeBytes, int.MaxValue),
                                    contentType = sidecar.ContentType,
                                    md5Hash = sidecar.Md5Hash,
                                    sha256Hash = sidecar.Sha256Hash,
                                    createdByUserGuid = sidecar.CreatedByUserGuid,
                                    versionNumber = sidecar.VersionNumber,
                                    objectGuid = sidecar.ObjectGuid,
                                    active = true,
                                    deleted = false,
                                    isDeleted = false,
                                    accessCount = 0
                                };
                                context.StorageObjects.Add(dbObject);
                                context.SaveChanges();


                                //
                                // Create a single StorageObjectVersion for the current state
                                //
                                DbStorageObjectVersion dbVersion = new DbStorageObjectVersion
                                {
                                    storageObjectId = dbObject.id,
                                    versionNumber = sidecar.VersionNumber,
                                    storageProviderId = dbProvider.id,
                                    providerKey = sidecar.Key,
                                    sizeBytes = (int)Math.Min(sidecar.SizeBytes, int.MaxValue),
                                    md5Hash = sidecar.Md5Hash,
                                    createdByUserGuid = sidecar.CreatedByUserGuid,
                                    createdUtc = sidecar.CreatedUtc,
                                    objectGuid = Guid.NewGuid(),
                                    active = true,
                                    deleted = false
                                };
                                context.StorageObjectVersions.Add(dbVersion);
                                context.SaveChanges();
                            });

                            totalRecovered++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("DeepSpace: failed to recover sidecar '{key}': {error}", sidecarObj.Key, ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("DeepSpace: provider scan failed for '{provider}': {error}", providerName, ex.Message);
                }
            }

            _logger.LogInformation("DeepSpace: recovery complete. {count} objects recovered from sidecar files.", totalRecovered);
            return totalRecovered;
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
