// ============================================================================
//
// DistributedCacheTests.cs — Unit tests for DistributedCache.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using Xunit;

using Foundation.Networking.Hivemind.Cache;
using Foundation.Networking.Hivemind.Configuration;

namespace Foundation.Networking.Hivemind.Tests.Cache
{
    public class DistributedCacheTests
    {
        private HivemindConfiguration CreateConfig()
        {
            return new HivemindConfiguration
            {
                DefaultTtlSeconds = 300,
                MaxCacheEntries = 100,
                CleanupIntervalSeconds = 3600
            };
        }


        // ── Basic Operations ─────────────────────────────────────────────


        [Fact]
        public void Set_And_Get_ReturnsValue()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                cache.Set("key1", "value1");

                Assert.Equal("value1", cache.Get("key1"));
            }
        }


        [Fact]
        public void Get_NonExistent_ReturnsNull()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                Assert.Null(cache.Get("missing"));
            }
        }


        [Fact]
        public void Set_OverwritesExistingValue()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                cache.Set("key1", "original");
                cache.Set("key1", "updated");

                Assert.Equal("updated", cache.Get("key1"));
            }
        }


        [Fact]
        public void Count_ReflectsEntries()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                cache.Set("a", "1");
                cache.Set("b", "2");
                cache.Set("c", "3");

                Assert.Equal(3, cache.Count);
            }
        }


        // ── TTL ──────────────────────────────────────────────────────────


        [Fact]
        public void Get_Expired_ReturnsNull()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                cache.Set("key1", "value1", TimeSpan.FromMilliseconds(1));
                System.Threading.Thread.Sleep(50);

                Assert.Null(cache.Get("key1"));
            }
        }


        [Fact]
        public void ContainsKey_Expired_ReturnsFalse()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                cache.Set("key1", "value1", TimeSpan.FromMilliseconds(1));
                System.Threading.Thread.Sleep(50);

                Assert.False(cache.ContainsKey("key1"));
            }
        }


        [Fact]
        public void ContainsKey_Valid_ReturnsTrue()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                cache.Set("key1", "value1");

                Assert.True(cache.ContainsKey("key1"));
            }
        }


        // ── Typed Values ─────────────────────────────────────────────────


        [Fact]
        public void SetTyped_And_GetTyped_Works()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                var data = new { Name = "Test", Count = 42 };
                cache.Set("typed", data, TimeSpan.FromMinutes(5));

                string raw = cache.Get("typed");

                Assert.Contains("Test", raw);
                Assert.Contains("42", raw);
            }
        }


        // ── Remove ───────────────────────────────────────────────────────


        [Fact]
        public void Remove_DeletesEntry()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                cache.Set("key1", "value1");
                bool removed = cache.Remove("key1");

                Assert.True(removed);
                Assert.Null(cache.Get("key1"));
            }
        }


        [Fact]
        public void Remove_NonExistent_ReturnsFalse()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                Assert.False(cache.Remove("missing"));
            }
        }


        // ── Clear ────────────────────────────────────────────────────────


        [Fact]
        public void Clear_RemovesAllEntries()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                cache.Set("a", "1");
                cache.Set("b", "2");

                cache.Clear();

                Assert.Equal(0, cache.Count);
            }
        }


        // ── GetOrSet ─────────────────────────────────────────────────────


        [Fact]
        public void GetOrSet_MissCallsFactory()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                bool factoryCalled = false;

                string value = cache.GetOrSet("key1", () =>
                {
                    factoryCalled = true;
                    return "computed";
                }, TimeSpan.FromMinutes(5));

                Assert.True(factoryCalled);
                Assert.Equal("computed", value);
            }
        }


        [Fact]
        public void GetOrSet_HitDoesNotCallFactory()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                cache.Set("key1", "existing");

                bool factoryCalled = false;
                string value = cache.GetOrSet("key1", () =>
                {
                    factoryCalled = true;
                    return "never";
                }, TimeSpan.FromMinutes(5));

                Assert.False(factoryCalled);
                Assert.Equal("existing", value);
            }
        }


        // ── Eviction ─────────────────────────────────────────────────────


        [Fact]
        public void Set_ExceedsMax_EvictsEntries()
        {
            HivemindConfiguration config = CreateConfig();
            config.MaxCacheEntries = 10;

            using (DistributedCache cache = new DistributedCache(config))
            {
                for (int i = 0; i < 15; i++)
                {
                    cache.Set("key" + i, "value" + i);
                }

                Assert.True(cache.Count <= 10);
            }
        }


        // ── Statistics ───────────────────────────────────────────────────


        [Fact]
        public void GetStatistics_TracksHitsAndMisses()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                cache.Set("key1", "value1");

                cache.Get("key1");
                cache.Get("key1");
                cache.Get("missing");

                CacheStatistics stats = cache.GetStatistics();

                Assert.Equal(2, stats.Hits);
                Assert.Equal(1, stats.Misses);
                Assert.True(stats.HitRate > 60);
            }
        }


        [Fact]
        public void GetStatistics_ReportsEntryCount()
        {
            using (DistributedCache cache = new DistributedCache(CreateConfig()))
            {
                cache.Set("a", "1");
                cache.Set("b", "2");

                CacheStatistics stats = cache.GetStatistics();

                Assert.Equal(2, stats.EntryCount);
                Assert.Equal(100, stats.MaxEntries);
            }
        }
    }
}
