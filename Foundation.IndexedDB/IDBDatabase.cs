using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static Foundation.IndexedDB.IDBTransaction;

namespace Foundation.IndexedDB
{
    /// <summary>
    /// 
    /// This class models the database, allowing creation of object stores and transactions.
    /// 
    /// </summary>
    public class IDBDatabase : IDisposable
    {
        internal readonly IDBContext _context;

        /// <summary>
        /// Semaphore to ensure only one async operation runs against the DbContext at a time.
        /// EF Core DbContext is not thread-safe — concurrent operations cause InvalidOperationException.
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private string _name;

        private Dictionary<string, ObjectStoreConfig> _storeConfigs = new Dictionary<string, ObjectStoreConfig>();

        public string Name { get { return _name; } }
        internal Dictionary<string, ObjectStoreConfig> StoreConfigs { get { return _storeConfigs; } }


        private IDBTransaction _currentReadWriteTransaction = null;


        internal IDBDatabase(IDBContext context, string name)
        {
            _context = context;
            _name = name;
        }

        public async Task LoadConfigsAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                List<Metadata> storeMetas = await _context.Metadata.Where(m => m.Key.StartsWith("store_"))
                                                                    .ToListAsync()
                                                                    .ConfigureAwait(false);

                foreach (Metadata metaEntry in storeMetas)
                {
                    string storeName = metaEntry.Key.Substring(6);
                    _storeConfigs[storeName] = JsonSerializer.Deserialize<ObjectStoreConfig>(metaEntry.Value);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 
        /// This gets the version of the database from the Metadata table.  Non async property to allow it to be used in constructors easily.
        /// 
        /// Using property instead of function here so it's similar to the name property.
        /// 
        /// </summary>
        /// <returns></returns>
        public uint Version
        {
            get
            {
                _semaphore.Wait();
                try
                {
                    // Retrieve current version from metadata (default to 0 if absent).
                    string currentVersionString = _context.Metadata.Where(m => m.Key == "version")
                                                                   .Select(m => m.Value)
                                                                   .FirstOrDefault() ?? "0";

                    if (uint.TryParse(currentVersionString, out uint version) == true)
                    {
                        return version;
                    }
                    else
                    {
                        return 0;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }


        /// <summary>
        /// This gets the version of the database from the Metadata table.  Non async to allow it to be used in constructors easily
        /// </summary>
        /// <returns></returns>
        public async Task<uint> VersionAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                // Retrieve current version from metadata (default to 0 if absent).
                string currentVersionString = await _context.Metadata.Where(m => m.Key == "version")
                                                                     .Select(m => m.Value)
                                                                     .FirstOrDefaultAsync().ConfigureAwait(false) ?? "0";

                if (uint.TryParse(currentVersionString, out uint version) == true)
                {
                    return version;
                }
                else
                {
                    return 0;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }


        /// <summary>
        /// Updates metadata under the concurrency semaphore. Safe to call from non-locked contexts.
        /// </summary>
        internal async Task UpdateMetaAsync(string key, string value)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await UpdateMetaInternalAsync(key, value).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Updates metadata WITHOUT acquiring the semaphore. 
        /// Must only be called from within an already-locked context (e.g. inside ExecuteWithLockAsync).
        /// </summary>
        internal async Task UpdateMetaInternalAsync(string key, string value)
        {
            await _context.Database.ExecuteSqlRawAsync("INSERT OR REPLACE INTO Metadata (Key, Value) VALUES ({0}, {1})", key, value).ConfigureAwait(false);
        }


        public async Task<IDBObjectStore> CreateObjectStoreAsync(string name, ObjectStoreOptions options = null)
        {
            if (options == null)
            {
                options = new ObjectStoreOptions();
            }

            ObjectStoreConfig config = new ObjectStoreConfig
            {
                KeyPath = options.KeyPath,
                AutoIncrement = options.AutoIncrement,
                Indexes = new List<IndexConfig>()
            };

            // Save config — UpdateMetaAsync acquires its own lock
            await UpdateMetaAsync($"store_{name}", JsonSerializer.Serialize(config)).ConfigureAwait(false);

            // Create key unique index
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync($"CREATE UNIQUE INDEX IF NOT EXISTS uk_{name}_key ON Data (KeyJson) WHERE StoreName = '{name}'").ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }

            if (config.AutoIncrement)
            {
                await UpdateMetaAsync($"nextKey_{name}", "1").ConfigureAwait(false);
            }

            _storeConfigs[name] = config; // Cache

            return new IDBObjectStore(this, name);
        }


        public IDBTransaction Transaction(string objectStoreName, TransactionMode transactionMode)
        {
            return new IDBTransaction(this, new string[] { objectStoreName }, transactionMode);
        }


        public IDBTransaction Transaction(string[] objectStoreNames, TransactionMode transactionMode)
        {
            if (objectStoreNames == null || objectStoreNames.Length == 0)
            {
                throw new ArgumentException("At least one object store name must be provided.");
            }

            if (transactionMode == TransactionMode.ReadWrite)
            {
                if (_currentReadWriteTransaction != null &&
                    _currentReadWriteTransaction.IsWriteModeTransactionFinalized == false)
                {
                    throw new InvalidOperationException("A read-write transaction is already in progress. Nested read-write transactions are not supported.");
                }

                _currentReadWriteTransaction = new IDBTransaction(this, objectStoreNames, transactionMode);

                return _currentReadWriteTransaction;
            }


            return new IDBTransaction(this, objectStoreNames, transactionMode);
        }


        public async Task DeleteObjectStoreAsync(string name)
        {
            if (!_storeConfigs.ContainsKey(name))
            {
                throw new InvalidOperationException("Object store not found.");
            }

            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                // Remove all data entries for this store
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Data WHERE StoreName = {0}", name).ConfigureAwait(false);

                // Remove metadata entries
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Metadata WHERE Key = {0} OR Key = {1}", $"store_{name}", $"nextKey_{name}").ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }

            // Remove from cache
            _storeConfigs.Remove(name);
        }


        /// <summary>
        /// Convenience method to commit the current read-write transaction if one exists.  Ideally, this should be done on the transaction itself, but since we have only 
        /// one read-write transaction at a time, this is a convenience.
        /// </summary>
        public void CommitCurrentReadWriteTransaction()
        {
            if (_currentReadWriteTransaction != null && !_currentReadWriteTransaction.IsWriteModeTransactionFinalized)
            {
                _currentReadWriteTransaction.Commit();
                _currentReadWriteTransaction = null;
            }
        }


        /// <summary>
        /// Convenience method to abort the current read-write transaction if one exists.  Ideally, this should be done on the transaction itself, but since we have only 
        /// one read-write transaction at a time, this is a convenience.
        /// </summary>
        public void AbortCurrentReadWriteTransaction()
        {
            if (_currentReadWriteTransaction != null && !_currentReadWriteTransaction.IsWriteModeTransactionFinalized)
            {
                _currentReadWriteTransaction.Abort();
                _currentReadWriteTransaction = null;
            }
        }


        /// <summary>
        /// Provides direct access to the concurrency semaphore for internal use by cursors
        /// and other components that need to acquire/release the lock around individual operations.
        /// </summary>
        internal SemaphoreSlim Semaphore => _semaphore;


        /// <summary>
        /// Helper to execute database operations under the concurrency semaphore.
        /// </summary>
        internal async Task<T> ExecuteWithLockAsync<T>(Func<Task<T>> operation)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                return await operation().ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Helper to execute void database operations under the concurrency semaphore.
        /// </summary>
        internal async Task ExecuteWithLockAsync(Func<Task> operation)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await operation().ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }


        public void Dispose()
        {
            _context.Dispose();
            _semaphore.Dispose();
        }
    }


    internal class ObjectStoreConfig
    {
        [JsonPropertyName("keyPath")]
        public string KeyPath { get; set; }

        [JsonPropertyName("autoIncrement")]
        public bool AutoIncrement { get; set; }

        [JsonPropertyName("indexes")]
        public List<IndexConfig> Indexes { get; set; }
    }


    internal class IndexConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("unique")]
        public bool Unique { get; set; }

        /// <summary>
        /// 
        /// All JSON is serialized to camel case, so we need to ensure the path is also camel case.
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetSafePath()
        {

            string output = this.Path.Replace("'", "''"); // Basic sanitization

            output = ToCamelCase(output);

            return output;
        }


        /// <summary>
        /// 
        /// Converts the first character of a string to lowercase (camelCase).
        /// Inlined from Foundation.StringUtility to keep this library standalone.
        /// 
        /// </summary>
        private static string ToCamelCase(string data)
        {
            if (string.IsNullOrEmpty(data) == false && data.Length > 1)
            {
                return char.ToLowerInvariant(data[0]) + data.Substring(1);
            }

            return data;
        }
    }

    public class ObjectStoreOptions
    {
        [JsonPropertyName("keyPath")]
        public string KeyPath { get; set; }

        [JsonPropertyName("autoIncrement")]
        public bool AutoIncrement { get; set; }
    }
}