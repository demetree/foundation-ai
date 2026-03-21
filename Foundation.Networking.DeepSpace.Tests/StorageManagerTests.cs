// ============================================================================
//
// StorageManagerTests.cs — Unit tests for StorageManager.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
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
    }
}
