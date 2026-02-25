using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Foundation.IndexedDB.Dexter
{
    /// <summary>
    /// 
    /// Base class for a Dexie-style database, providing access to typed object stores.
    /// Inherit from this class to define your database schema with typed tables.
    /// 
    /// </summary>
    public abstract class DexterDatabase : IDisposable
    {
        protected readonly IDBDatabase _indexedDB; // The underlying C# IndexedDB database

        protected uint _version = 0;                // The version number of the indexedDB schema that this wrapper has had its stores built from


        /// <summary>
        /// Initializes a new instance of the <see cref="DexterDatabase"/> class.
        /// </summary>
        /// <param name="indexedDB">The underlying <see cref="IDBDatabase"/> instance.</param>
        public DexterDatabase(IDBDatabase indexedDB)
        {
            _indexedDB = indexedDB ?? throw new ArgumentNullException(nameof(indexedDB));
        }

        public IDBDatabase IndexedDB 
        { 
            get { return _indexedDB; }
        }

        /// <summary>
        /// Defines the database schema and version. This method should be called
        /// in the constructor of your derived database class.
        /// </summary>
        /// <param name="version">The database version.</param>
        /// <returns>A <see cref="DexterVersionBuilder"/> for defining object stores.</returns>
        protected DexterVersionBuilder Version(uint version)
        {
            _version = version;

            return new DexterVersionBuilder(_indexedDB, version);
        }

        /// <summary>
        /// 
        /// Allows access to an object store as a <see cref="DexterTable{T, TKey}"/>.
        /// This is typically used internally by the Version builder.
        /// 
        /// </summary>
        /// <typeparam name="T">The type of the entity in the table.</typeparam>
        /// <typeparam name="TKey">The type of the primary key.</typeparam>
        /// <param name="storeName">The name of the object store.</param>
        /// <returns>A <see cref="DexterTable{T, TKey}"/> instance.</returns>
        public DexterTable<T, TKey> Table<T, TKey>(string storeName) where T : class
        {
            // Do we have the store name in the indexedDB stores?
            if (_indexedDB.StoreConfigs.ContainsKey(storeName) == true)
            {
                return new DexterTable<T, TKey>(new IDBObjectStore(_indexedDB, storeName), this);
            }
            else
            {
                throw new InvalidOperationException($"Object store '{storeName}' not found.");
            }
        }

        /// <summary>
        /// Starts a new transaction with the specified object stores and mode.
        /// </summary>
        /// <param name="storeNames">The names of the object stores to include in the transaction.</param>
        /// <param name="readWrite">True for read-write transaction, false for read-only.</param>
        /// <returns>An <see cref="IDBTransaction"/> instance.</returns>
        public IDBTransaction Transaction(string[] storeNames, bool readWrite = false)
        {
            if (readWrite == true)
            {
                return _indexedDB.Transaction(storeNames, IDBTransaction.TransactionMode.ReadWrite);
            }
            else
            {
                return _indexedDB.Transaction(storeNames, IDBTransaction.TransactionMode.ReadOnly);
            }
        }

        public void Dispose()
        {
            _indexedDB.Dispose();
        }
    }

    /// <summary>
    /// 
    /// Helper for defining database versions and object stores in a Dexie-like manner.
    /// 
    /// </summary>
    public class DexterVersionBuilder
    {
        private readonly IDBDatabase _indexedDB;
        private readonly uint _version;

        internal DexterVersionBuilder(IDBDatabase indexedDB, uint version)
        {
            _indexedDB = indexedDB;
            _version = version;
        }

        /// <summary>
        /// 
        /// Defines object stores for the current database version.
        /// 
        /// Rules are:
        /// 
        /// fields are sent in as a csv string.
        /// 
        /// field names starting with __ indicate that it is key field that is NOT auto incrementing
        /// field names starting with ++ indicate that it is key field that is auto incrementing
        /// field names starting with &amp; indicate that it is unique index field
        /// non prefixed strings become regular indexed fields
        /// 
        /// WARNING: Unlike Dexie.js, the first field is NOT automatically the primary key.
        /// You MUST use the __ or ++ prefix to designate a primary key. If no prefix is
        /// used, all fields become regular indexes and the store will have no keyPath,
        /// causing PutAsync/AddAsync to throw "Key required".
        /// 
        /// Examples:
        ///   "++id, name, &amp;email"    → auto-increment PK "id", regular index "name", unique index "email"
        ///   "__jobId, phase"        → string PK "jobId", regular index "phase"
        /// 
        /// </summary>
        /// <param name="schemaDefinition">A dictionary where keys are store names and values are schema strings (e.g., "++id, name, &email").</param>
        /// <returns>A Task representing the asynchronous operation of creating/updating stores.</returns>
        public async Task DefineStores(Dictionary<string, string> schemaDefinition)
        {
            foreach (var entry in schemaDefinition)
            {
                string storeName = entry.Key;
                string schema = entry.Value;

                // Parse schema string (e.g., "++id, name, &email")
                ObjectStoreOptions options = new ObjectStoreOptions();
                List<(string path, bool unique)> indexPaths = new List<(string, bool)>();

                string[] parts = schema.Split(',').Select(p => p.Trim()).ToArray();
                foreach (string part in parts)
                {
                    if (part.StartsWith("__"))
                    {
                        //
                        // Non auto incrementing primary key
                        //
                        options.KeyPath = part.Substring(2);
                        options.AutoIncrement = false;
                    }
                    else if (part.StartsWith("++"))
                    {
                        //
                        // Auto incrementing primary key
                        //
                        options.KeyPath = part.Substring(2);
                        options.AutoIncrement = true;
                    }
                    else if (part.StartsWith("&"))
                    {
                        //
                        // Unique indexed field
                        //
                        indexPaths.Add((part.Substring(1), true));
                    }
                    else if (!string.IsNullOrEmpty(part))
                    {
                        indexPaths.Add((part, false)); // Non-unique index
                    }
                }

                if (string.IsNullOrEmpty(options.KeyPath) && !string.IsNullOrEmpty(_indexedDB.StoreConfigs.GetValueOrDefault(storeName)?.KeyPath))
                {
                    // If no keyPath explicitly defined in new schema, but one exists, retain it.
                    options.KeyPath = _indexedDB.StoreConfigs[storeName].KeyPath;
                }

                // Create or update object store
                IDBObjectStore currentStore;
                if (!_indexedDB.StoreConfigs.ContainsKey(storeName))
                {
                    currentStore = await _indexedDB.CreateObjectStoreAsync(storeName, options).ConfigureAwait(false);
                }
                else
                {
                    currentStore = new IDBObjectStore(_indexedDB, storeName);
                    // Handle store schema upgrades:
                    // This is where real IndexedDB logic shines. For this prototype, we just update the config.
                    // In a full implementation, you'd compare old vs new schema and make necessary changes
                    // (e.g., delete/recreate indexes, or even migrate data if keyPath changes).
                    var existingConfig = _indexedDB.StoreConfigs[storeName];
                    existingConfig.KeyPath = options.KeyPath;
                    existingConfig.AutoIncrement = options.AutoIncrement;
                    existingConfig.Indexes.Clear(); // Clear existing indexes to re-create
                    await _indexedDB.UpdateMetaAsync($"store_{storeName}", JsonSerializer.Serialize(existingConfig, IDBCommon.JsonOptions)).ConfigureAwait(false);
                }

                // Create indexes
                foreach (var (indexPath, isUnique) in indexPaths)
                {
                    await currentStore.CreateIndexAsync(indexPath, indexPath, new IDBObjectStore.IndexOptions { Unique = isUnique }).ConfigureAwait(false);
                }
            }
        }
    }
}
