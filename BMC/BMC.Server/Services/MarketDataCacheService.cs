using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Foundation.BMC.Database;

namespace Foundation.BMC.Services
{
    /// <summary>
    /// Database-backed caching service for marketplace API responses.
    ///
    /// Caches serialized JSON responses from BrickLink, BrickEconomy, and BrickOwl
    /// to reduce external API calls and improve Brickberg Terminal load times.
    ///
    /// Cache entries are keyed by (source, itemType, itemNumber, condition) and
    /// expire based on configurable per-source TTLs.
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public class MarketDataCacheService
    {
        private readonly BMCContext _context;
        private readonly MarketDataCacheOptions _options;
        private readonly ILogger<MarketDataCacheService> _logger;

        // ── Thread-safe in-memory metrics (reset on app restart) ──
        private static long _cacheHits;
        private static long _cacheMisses;
        private static long _cacheErrors;


        public MarketDataCacheService(
            BMCContext context,
            IOptions<MarketDataCacheOptions> options,
            ILogger<MarketDataCacheService> logger)
        {
            _context = context;
            _options = options.Value;
            _logger = logger;
        }


        /// <summary>
        /// Returns a cached response if available and not expired, otherwise calls fetchFunc
        /// to retrieve fresh data, caches it, and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the API response object.</typeparam>
        /// <param name="source">Marketplace source: "BrickLink", "BrickEconomy", "BrickOwl"</param>
        /// <param name="itemType">Item type: "SET", "MINIFIG", "PART"</param>
        /// <param name="itemNumber">Item identifier (e.g. "42131-1")</param>
        /// <param name="condition">Condition: "N", "U", or null for non-BrickLink sources</param>
        /// <param name="fetchFunc">Async function to call if the cache misses</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The cached or freshly fetched response</returns>
        public async Task<T> GetOrFetchAsync<T>(
            string source,
            string itemType,
            string itemNumber,
            string condition,
            Func<Task<T>> fetchFunc,
            CancellationToken cancellationToken = default) where T : class
        {
            var now = DateTime.UtcNow;

            // ── Check for a valid (non-expired) cache entry ──
            var cached = await _context.MarketDataCaches
                .AsNoTracking()
                .Where(c =>
                    c.source == source
                    && c.itemType == itemType
                    && c.itemNumber == itemNumber
                    && c.condition == condition
                    && c.active == true
                    && c.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (cached != null && cached.expiresDate > now)
            {
                // Cache hit
                Interlocked.Increment(ref _cacheHits);
                _logger.LogDebug(
                    "Cache hit: {Source}/{ItemType}/{ItemNumber}/{Condition} (expires {Expires})",
                    source, itemType, itemNumber, condition, cached.expiresDate);

                try
                {
                    return JsonSerializer.Deserialize<T>(cached.responseJson);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize cached response for {Source}/{ItemNumber} — refetching", source, itemNumber);
                    // Fall through to re-fetch
                }
            }

            // ── Cache miss or expired — fetch fresh data ──
            Interlocked.Increment(ref _cacheMisses);
            T result = await fetchFunc();

            if (result != null)
            {
                int ttlMinutes = _options.GetTtlMinutes(source);
                string json = JsonSerializer.Serialize(result);

                try
                {
                    if (cached != null)
                    {
                        // Update existing entry (re-attach tracked)
                        var tracked = await _context.MarketDataCaches.FindAsync(new object[] { cached.id }, cancellationToken);
                        if (tracked != null)
                        {
                            tracked.responseJson = json;
                            tracked.fetchedDate = now;
                            tracked.expiresDate = now.AddMinutes(ttlMinutes);
                            tracked.ttlMinutes = ttlMinutes;
                        }
                    }
                    else
                    {
                        // Insert new entry
                        _context.MarketDataCaches.Add(new MarketDataCache
                        {
                            source = source,
                            itemType = itemType,
                            itemNumber = itemNumber,
                            condition = condition,
                            responseJson = json,
                            fetchedDate = now,
                            expiresDate = now.AddMinutes(ttlMinutes),
                            ttlMinutes = ttlMinutes,
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false
                        });
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogDebug(
                        "Cached {Source}/{ItemType}/{ItemNumber}/{Condition} (TTL={Ttl}min)",
                        source, itemType, itemNumber, condition, ttlMinutes);
                }
                catch (Exception ex)
                {
                    // Cache write failures should never break the user request
                    Interlocked.Increment(ref _cacheErrors);
                    _logger.LogWarning(ex, "Failed to write cache entry for {Source}/{ItemNumber}", source, itemNumber);
                }
            }

            return result;
        }


        /// <summary>
        /// Returns in-memory metrics and database-level cache statistics.
        /// </summary>
        public async Task<Dictionary<string, object>> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var entries = await _context.MarketDataCaches
                .AsNoTracking()
                .Where(c => c.active == true && c.deleted == false)
                .Select(c => new { c.source, c.fetchedDate, c.expiresDate })
                .ToListAsync(cancellationToken);

            int totalEntries = entries.Count;
            int activeEntries = entries.Count(e => e.expiresDate > now);
            int expiredEntries = entries.Count(e => e.expiresDate <= now);

            var bySource = entries
                .GroupBy(e => e.source)
                .ToDictionary(
                    g => g.Key,
                    g => (object)new { total = g.Count(), active = g.Count(e => e.expiresDate > now) }
                );

            long hits = Interlocked.Read(ref _cacheHits);
            long misses = Interlocked.Read(ref _cacheMisses);

            return new Dictionary<string, object>
            {
                ["hits"] = hits,
                ["misses"] = misses,
                ["errors"] = Interlocked.Read(ref _cacheErrors),
                ["hitRate"] = (hits + misses) > 0
                    ? Math.Round((double)hits / (hits + misses) * 100, 1)
                    : 0.0,
                ["totalEntries"] = totalEntries,
                ["activeEntries"] = activeEntries,
                ["expiredEntries"] = expiredEntries,
                ["bySource"] = bySource,
                ["oldestEntry"] = entries.Count > 0 ? entries.Min(e => (DateTime?)e.fetchedDate) : null,
                ["newestEntry"] = entries.Count > 0 ? entries.Max(e => (DateTime?)e.fetchedDate) : null
            };
        }


        /// <summary>
        /// Removes all expired cache entries from the database.
        /// Intended to be called periodically (e.g. daily) to keep the table lean.
        /// </summary>
        public async Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            int purged = await _context.MarketDataCaches
                .Where(c => c.expiresDate <= now)
                .ExecuteDeleteAsync(cancellationToken);

            if (purged > 0)
            {
                _logger.LogInformation("Purged {Count} expired market data cache entries", purged);
            }

            return purged;
        }
    }
}
