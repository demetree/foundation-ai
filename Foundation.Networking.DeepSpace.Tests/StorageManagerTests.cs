// ============================================================================
//
// StorageManagerTests.cs — Unit tests for StorageManager.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Foundation.Networking.DeepSpace.Configuration;
using Foundation.Networking.DeepSpace.Providers;

namespace Foundation.Networking.DeepSpace.Tests
{
    public class StorageManagerTests : IDisposable
    {
        private readonly string _testRoot;
        private readonly StorageManager _manager;


        public StorageManagerTests()
        {
            _testRoot = Path.Combine(Path.GetTempPath(), "deepspace_mgr_" + Guid.NewGuid().ToString("N").Substring(0, 8));

            DeepSpaceConfiguration config = new DeepSpaceConfiguration
            {
                DefaultProvider = "Local",
                LocalStorage = new LocalStorageConfig { RootPath = _testRoot }
            };

            _manager = new StorageManager(config, NullLogger<StorageManager>.Instance);
            _manager.RegisterProvider(new LocalStorageProvider(config.LocalStorage));
        }


        public void Dispose()
        {
            if (Directory.Exists(_testRoot))
            {
                Directory.Delete(_testRoot, true);
            }
        }


        // ── Provider Registration ────────────────────────────────────────


        [Fact]
        public void RegisterProvider_IncreasesCount()
        {
            Assert.Equal(1, _manager.ProviderCount);
        }


        [Fact]
        public void GetProvider_ByName_ReturnsCorrect()
        {
            IStorageProvider provider = _manager.GetProvider("Local");

            Assert.NotNull(provider);
            Assert.Equal("Local", provider.ProviderName);
        }


        [Fact]
        public void GetProvider_Missing_ReturnsNull()
        {
            Assert.Null(_manager.GetProvider("S3"));
        }


        // ── Unified Operations ───────────────────────────────────────────


        [Fact]
        public async Task Put_And_Get_WorksThroughManager()
        {
            byte[] data = Encoding.UTF8.GetBytes("managed data");

            StorageResult putResult = await _manager.PutAsync("managed/test.txt", data);
            Assert.True(putResult.Success);

            byte[] retrieved = await _manager.GetAsync("managed/test.txt");
            Assert.Equal("managed data", Encoding.UTF8.GetString(retrieved));
        }


        [Fact]
        public async Task Delete_WorksThroughManager()
        {
            await _manager.PutAsync("todelete.txt", Encoding.UTF8.GetBytes("bye"));

            bool deleted = await _manager.DeleteAsync("todelete.txt");

            Assert.True(deleted);
            Assert.False(await _manager.ExistsAsync("todelete.txt"));
        }


        [Fact]
        public async Task Exists_WorksThroughManager()
        {
            await _manager.PutAsync("checkme.txt", new byte[] { 1 });

            Assert.True(await _manager.ExistsAsync("checkme.txt"));
            Assert.False(await _manager.ExistsAsync("nope.txt"));
        }


        [Fact]
        public async Task List_WorksThroughManager()
        {
            await _manager.PutAsync("items/a.txt", new byte[] { 1 });
            await _manager.PutAsync("items/b.txt", new byte[] { 2 });

            ListResult result = await _manager.ListAsync("items/");

            Assert.True(result.Success);
            Assert.Equal(2, result.Objects.Count);
        }


        [Fact]
        public async Task Copy_WorksThroughManager()
        {
            await _manager.PutAsync("src.txt", Encoding.UTF8.GetBytes("copy this"));

            StorageResult result = await _manager.CopyAsync("src.txt", "dst.txt");

            Assert.True(result.Success);
            Assert.True(await _manager.ExistsAsync("dst.txt"));
        }


        [Fact]
        public async Task GetPresignedUrlAsync_WorksThroughManager()
        {
            string url = await _manager.GetPresignedUrlAsync("managed.txt", TimeSpan.FromMinutes(5));
            
            Assert.NotNull(url);
            Assert.Equal("/api/deepspace/download/local/managed.txt", url);
        }


        [Fact]
        public async Task GetPresignedUrlAsync_MissingProvider_ReturnsNull()
        {
            string url = await _manager.GetPresignedUrlAsync("managed.txt", TimeSpan.FromMinutes(5), providerName: "NonExistent");
            
            Assert.Null(url);
        }


        // ── Missing Provider ─────────────────────────────────────────────


        [Fact]
        public async Task Put_MissingProvider_ReturnsFail()
        {
            StorageResult result = await _manager.PutAsync("test.txt", new byte[] { 1 }, providerName: "NonExistent");

            Assert.False(result.Success);
            Assert.Contains("not found", result.Error);
        }


        [Fact]
        public async Task Get_MissingProvider_ReturnsNull()
        {
            byte[] result = await _manager.GetAsync("test.txt", providerName: "NonExistent");

            Assert.Null(result);
        }


        // ── Statistics ───────────────────────────────────────────────────


        [Fact]
        public async Task GetStatistics_TracksOperations()
        {
            await _manager.PutAsync("s1.txt", new byte[] { 1 });
            await _manager.PutAsync("s2.txt", new byte[] { 2 });
            await _manager.GetAsync("s1.txt");
            await _manager.DeleteAsync("s2.txt");

            StorageManagerStatistics stats = _manager.GetStatistics();

            Assert.Equal(2, stats.TotalPuts);
            Assert.Equal(1, stats.TotalGets);
            Assert.Equal(1, stats.TotalDeletes);
            Assert.Equal("Local", stats.DefaultProvider);
            Assert.Contains("Local", stats.ProviderNames);
        }


        // ── Stream Operations ────────────────────────────────────────────


        [Fact]
        public async Task PutStreamAsync_And_GetStreamAsync_WorksThroughManager()
        {
            byte[] data = Encoding.UTF8.GetBytes("streamed data");

            using (MemoryStream ms = new MemoryStream(data))
            {
                StorageResult putResult = await _manager.PutStreamAsync("stream/test.txt", ms);
                Assert.True(putResult.Success);
            }

            using (Stream stream = await _manager.GetStreamAsync("stream/test.txt"))
            {
                Assert.NotNull(stream);

                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = await reader.ReadToEndAsync();
                    Assert.Equal("streamed data", content);
                }
            }
        }


        [Fact]
        public async Task GetStreamAsync_MissingProvider_ReturnsNull()
        {
            Stream result = await _manager.GetStreamAsync("test.txt", providerName: "NonExistent");

            Assert.Null(result);
        }


        [Fact]
        public async Task PutStreamAsync_MissingProvider_ReturnsFail()
        {
            using (MemoryStream ms = new MemoryStream(new byte[] { 1 }))
            {
                StorageResult result = await _manager.PutStreamAsync("test.txt", ms, providerName: "NonExistent");

                Assert.False(result.Success);
                Assert.Contains("not found", result.Error);
            }
        }


        // ── Metadata ─────────────────────────────────────────────────────


        [Fact]
        public async Task GetMetadataAsync_WorksThroughManager()
        {
            await _manager.PutAsync("meta.txt", Encoding.UTF8.GetBytes("meta test"));

            StorageObject obj = await _manager.GetMetadataAsync("meta.txt");

            Assert.NotNull(obj);
            Assert.Equal("meta.txt", obj.Key);
            Assert.True(obj.SizeBytes > 0);
        }


        [Fact]
        public async Task GetMetadataAsync_NonExistent_ReturnsNull()
        {
            StorageObject result = await _manager.GetMetadataAsync("missing.txt");

            Assert.Null(result);
        }


        [Fact]
        public async Task GetMetadataAsync_MissingProvider_ReturnsNull()
        {
            StorageObject result = await _manager.GetMetadataAsync("test.txt", providerName: "NonExistent");

            Assert.Null(result);
        }


        [Fact]
        public async Task PutStreamAsync_TracksStatistics()
        {
            using (MemoryStream ms = new MemoryStream(new byte[] { 1 }))
            {
                await _manager.PutStreamAsync("stats_stream.txt", ms);
            }

            StorageManagerStatistics stats = _manager.GetStatistics();
            Assert.True(stats.TotalPuts >= 1);
        }


        // ── Sidecar Operations ──────────────────────────────────────────


        [Fact]
        public async Task List_ExcludesSidecarFiles()
        {
            //
            // Store a regular object (which creates a sidecar since no DB manager)
            // and a fake sidecar file, then verify sidecars are filtered out of list results
            //
            await _manager.PutAsync("items/doc.txt", Encoding.UTF8.GetBytes("content"));

            //
            // Manually store a sidecar-named object via the provider
            //
            IStorageProvider provider = _manager.GetProvider("Local");
            await provider.PutBytesAsync("items/doc.txt.deepspace.json", Encoding.UTF8.GetBytes("{}"));

            ListResult result = await _manager.ListAsync("items/");

            Assert.True(result.Success);

            //
            // Verify no sidecar files appear in the results
            //
            foreach (StorageObject obj in result.Objects)
            {
                Assert.False(obj.Key.EndsWith(".deepspace.json"), "Sidecar file should be filtered: " + obj.Key);
            }
        }


        [Fact]
        public async Task PutAsync_SidecarKeyRejected()
        {
            //
            // Attempting to store an object with the .deepspace.json extension should be rejected
            //
            StorageResult result = await _manager.PutAsync("test.deepspace.json", new byte[] { 1 });

            Assert.False(result.Success);
            Assert.Contains("reserved", result.Error);
        }


        // ── Bucket Operations ───────────────────────────────────────────


        [Fact]
        public async Task CreateBucket_WorksThroughManager()
        {
            bool created = await _manager.CreateBucketAsync("mgr-bucket");

            Assert.True(created);
        }


        [Fact]
        public async Task ListBuckets_WorksThroughManager()
        {
            await _manager.CreateBucketAsync("list-bucket");

            List<string> buckets = await _manager.ListBucketsAsync();

            Assert.Contains("list-bucket", buckets);
        }


        [Fact]
        public async Task DeleteBucket_WorksThroughManager()
        {
            await _manager.CreateBucketAsync("delete-bucket");

            bool deleted = await _manager.DeleteBucketAsync("delete-bucket");

            Assert.True(deleted);
        }


        // ── Update Metadata ─────────────────────────────────────────────


        [Fact]
        public async Task UpdateMetadata_WorksThroughManager()
        {
            await _manager.PutAsync("metamgr.txt", Encoding.UTF8.GetBytes("test"));

            Dictionary<string, string> meta = new Dictionary<string, string>
            {
                { "tag", "important" }
            };

            bool updated = await _manager.UpdateMetadataAsync("metamgr.txt", meta);

            Assert.True(updated);
        }
    }
}
