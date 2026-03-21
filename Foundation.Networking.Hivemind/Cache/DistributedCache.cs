// ============================================================================
//
// DistributedCache.cs — In-process distributed cache with TTL and eviction.
//
// Provides a thread-safe key-value cache with time-to-live, LRU eviction,
// and periodic cleanup of expired entries.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;

using Foundation.Networking.Hivemind.Configuration;

namespace Foundation.Networking.Hivemind.Cache
{
    /// <summary>
    /// A single cache entry with metadata.
    /// </summary>
    public class CacheEntry
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresUtc { get; set; }
        public DateTime LastAccessedUtc { get; set; } = DateTime.UtcNow;
        public long AccessCount { get; set; }
        public long SizeBytes { get; set; }
    }


    /// <summary>
    /// Cache statistics.
    /// </summary>
    public class CacheStatistics
    {
        public int EntryCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public long Hits { get; set; }
        public long Misses { get; set; }
        public double HitRate { get; set; }
        public long EvictionCount { get; set; }
        public int MaxEntries { get; set; }
    }


    /// <summary>
    ///
    /// Thread-safe in-process cache with TTL, LRU eviction, and statistics.
    ///
    /// </summary>
    public class DistributedCache : IDisposable
    {
        private readonly HivemindConfiguration _config;
        private readonly ConcurrentDictionary<string, CacheEntry> _entries;
        private readonly object _statsLock = new object();

        private long _hits;
        private long _misses;
        private long _evictions;
        private Timer _cleanupTimer;
        private bool _disposed = false;


        public DistributedCache(HivemindConfiguration config)
        {
            _config = config;
            _entries = new ConcurrentDictionary<string, CacheEntry>();

            //
            // Start periodic cleanup
            //
            _cleanupTimer = new Timer(
                CleanupCallback,
                null,
                TimeSpan.FromSeconds(config.CleanupIntervalSeconds),
                TimeSpan.FromSeconds(config.CleanupIntervalSeconds));
        }


        /// <summary>
        /// Number of entries in the cache.
        /// </summary>
        public int Count => _entries.Count;


        // ── Core Operations ───────────────────────────────────────────────


        /// <summary>
        /// Sets a value in the cache with the default TTL.
        /// </summary>
        public void Set(string key, string value)
        {
            Set(key, value, TimeSpan.FromSeconds(_config.DefaultTtlSeconds));
        }


        /// <summary>
        /// Sets a value in the cache with a specific TTL.
        /// </summary>
        public void Set(string key, string value, TimeSpan ttl)
        {
            CacheEntry entry = new CacheEntry
            {
                Key = key,
                Value = value,
                CreatedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(ttl),
                LastAccessedUtc = DateTime.UtcNow,
                SizeBytes = (value != null) ? value.Length * 2 : 0
            };

            _entries[key] = entry;

            //
            // Check if eviction is needed
            //
            if (_entries.Count > _config.MaxCacheEntries)
            {
                Evict();
            }
        }


        /// <summary>
        /// Sets a typed value (serialized to JSON).
        /// </summary>
        public void Set<T>(string key, T value, TimeSpan ttl)
        {
            string json = JsonSerializer.Serialize(value);
            Set(key, json, ttl);
        }


        /// <summary>
        /// Gets a value from the cache.  Returns null if not found or expired.
        /// </summary>
        public string Get(string key)
        {
            if (_entries.TryGetValue(key, out CacheEntry entry))
            {
                //
                // Check expiry
                //
                if (DateTime.UtcNow > entry.ExpiresUtc)
                {
                    _entries.TryRemove(key, out _);
                    Interlocked.Increment(ref _misses);
                    return null;
                }

                entry.LastAccessedUtc = DateTime.UtcNow;
                entry.AccessCount++;
                Interlocked.Increment(ref _hits);

                return entry.Value;
            }

            Interlocked.Increment(ref _misses);
            return null;
        }


        /// <summary>
        /// Gets a typed value (deserialized from JSON).
        /// </summary>
        public T Get<T>(string key)
        {
            string value = Get(key);

            if (value == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value);
        }


        /// <summary>
        /// Gets a value or creates it if not present.
        /// </summary>
        public string GetOrSet(string key, Func<string> factory, TimeSpan ttl)
        {
            string existing = Get(key);

            if (existing != null)
            {
                return existing;
            }

            string value = factory();
            Set(key, value, ttl);
            return value;
        }


        /// <summary>
        /// Checks if a key exists and is not expired.
        /// </summary>
        public bool ContainsKey(string key)
        {
            if (_entries.TryGetValue(key, out CacheEntry entry))
            {
                if (DateTime.UtcNow > entry.ExpiresUtc)
                {
                    _entries.TryRemove(key, out _);
                    return false;
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Removes a key from the cache.
        /// </summary>
        public bool Remove(string key)
        {
            return _entries.TryRemove(key, out _);
        }


        /// <summary>
        /// Clears all entries.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
        }


        // ── Statistics ────────────────────────────────────────────────────


        /// <summary>
        /// Gets cache statistics.
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            long hits = Interlocked.Read(ref _hits);
            long misses = Interlocked.Read(ref _misses);
            long total = hits + misses;

            return new CacheStatistics
            {
                EntryCount = _entries.Count,
                TotalSizeBytes = _entries.Values.Sum(e => e.SizeBytes),
                Hits = hits,
                Misses = misses,
                HitRate = total > 0 ? (double)hits / total * 100.0 : 0,
                EvictionCount = Interlocked.Read(ref _evictions),
                MaxEntries = _config.MaxCacheEntries
            };
        }


        // ── Eviction ──────────────────────────────────────────────────────


        private void Evict()
        {
            //
            // LRU: remove the least recently accessed entries
            //
            int toRemove = _entries.Count - _config.MaxCacheEntries + (_config.MaxCacheEntries / 10);

            if (toRemove <= 0)
            {
                return;
            }

            List<string> keysToRemove = _entries
                .OrderBy(e => e.Value.LastAccessedUtc)
                .Take(toRemove)
                .Select(e => e.Key)
                .ToList();

            foreach (string key in keysToRemove)
            {
                if (_entries.TryRemove(key, out _))
                {
                    Interlocked.Increment(ref _evictions);
                }
            }
        }


        private void CleanupCallback(object state)
        {
            DateTime now = DateTime.UtcNow;
            List<string> expired = new List<string>();

            foreach (var kvp in _entries)
            {
                if (now > kvp.Value.ExpiresUtc)
                {
                    expired.Add(kvp.Key);
                }
            }

            foreach (string key in expired)
            {
                _entries.TryRemove(key, out _);
            }
        }


        // ── IDisposable ───────────────────────────────────────────────────


        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;

                if (_cleanupTimer != null)
                {
                    _cleanupTimer.Dispose();
                    _cleanupTimer = null;
                }
            }
        }
    }
}
