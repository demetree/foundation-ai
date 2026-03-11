using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Foundation.IndexedDB
{


    /// <summary>
    /// 
    /// This is a concept service for building caching on top of IndexedDB.  It hasn't been tried yet, and needs work to be functional, especially in the area of eviction.
    /// 
    /// I like the patterns it uses, so build on this when I get to building caching functionality. 
    /// 
    /// </summary>
    public class IndexedDbCacheService : IDisposable
    {
        private readonly IDBFactory _factory;
        private readonly ILogger<IndexedDbCacheService> _logger;
        private readonly string _dbName; // e.g., "ApiCache"
        private readonly TimeSpan _maxAge; // Configurable freshness threshold
        private IDBDatabase _db;

        public IndexedDbCacheService(IDBFactory factory, ILogger<IndexedDbCacheService> logger, IConfiguration config)
        {
            _factory = factory;
            _logger = logger;

            try
            {
                _dbName = config.GetValue<string>("Cache:DbName") ?? "ApiCache";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Cache:DbName from configuration. Using default 'ApiCache'.");
                _dbName = "ApiCache";
            }

            try
            {
                _maxAge = config.GetValue<TimeSpan>("Cache:MaxAge");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Cache:MaxAge from configuration. Using default 5 minutes.");
                _maxAge = TimeSpan.FromMinutes(5);
            }
        }


        private async Task<IDBDatabase> GetDatabaseAsync()
        {
            if (_db != null) return _db; // Reuse for perf

            var request = await _factory.OpenAsync(_dbName, version: 1, upgradeNeededHandler: (db, oldVer, newVer) =>
            {
                // Schema setup: One store for cache entries, indexed on timestamp
                var store = db.CreateObjectStoreAsync("entries", new ObjectStoreOptions { KeyPath = "key", AutoIncrement = false }).Result;
                store.CreateIndexAsync("byTimestamp", "timestamp", new IDBObjectStore.IndexOptions { Unique = false }).Wait();
            }).ConfigureAwait(false);

            _db = request.Result;
            return _db;
        }

        // High-level method: Check cache, compute if stale/missing, update
        public async Task<T> GetOrComputeAsync<T>(string key, Func<Task<T>> computeFunc, bool forceRefresh = false)
        {
            var db = await GetDatabaseAsync().ConfigureAwait(false);

            using var tx = db.Transaction("entries", IDBTransaction.TransactionMode.ReadWrite);
            var store = tx.ObjectStore("entries");
            var index = store.Index("byTimestamp"); // For freshness check

            try
            {
                // Get most recent by timestamp (descending cursor for latest)
                using var cursor = index.OpenCursor<T>(IDBKeyRange.LowerBound(DateTimeOffset.UtcNow.Add(-_maxAge)), "prev");
                if (await cursor.ContinueAsync().ConfigureAwait(false) && cursor.Key.ToString() == key && !forceRefresh)
                {
                    // Cache hit: Deserialize value
                    var cached = cursor.Value as Dictionary<string, object>; // Assuming JSON dict
                    if (cached?.TryGetValue("value", out var valueJson) == true)
                    {
                        return JsonSerializer.Deserialize<T>((string)valueJson, IDBCommon.JsonOptions);
                    }
                }

                // Cache miss/stale: Compute and cache
                var result = await computeFunc().ConfigureAwait(false);
                var entry = new { key, timestamp = DateTimeOffset.UtcNow, value = JsonSerializer.Serialize(result, IDBCommon.JsonOptions) };
                await store.PutAsync(entry).ConfigureAwait(false);

                tx.Commit();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache operation failed for key {Key}; falling back to compute", key);
                tx.Abort(); // Rollback on error
                return await computeFunc().ConfigureAwait(false); // Fallback without caching
            }
        }

        // Eviction: Run periodically (e.g., via IHostedService)
        public async Task EvictOldAsync<T>()
        {
            var db = await GetDatabaseAsync().ConfigureAwait(false);
            using var tx = db.Transaction("entries", IDBTransaction.TransactionMode.ReadWrite);
            var index = tx.ObjectStore("entries").Index("byTimestamp");
            var cutoff = DateTimeOffset.UtcNow.Add(-_maxAge);
            var range = IDBKeyRange.UpperBound(cutoff);

            using var cursor = index.OpenCursor<T>(range);
            while (await cursor.ContinueAsync().ConfigureAwait(false))
            {

                // await cursor.DeleteAsync(); // TODO - add this extension
            }
            tx.Commit();
        }

        public void Dispose() => _db?.Dispose();
    }
}