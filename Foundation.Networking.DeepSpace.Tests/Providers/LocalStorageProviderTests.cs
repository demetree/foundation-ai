// ============================================================================
//
// LocalStorageProviderTests.cs — Unit tests for LocalStorageProvider.
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

using Foundation.Networking.DeepSpace.Configuration;
using Foundation.Networking.DeepSpace.Providers;

namespace Foundation.Networking.DeepSpace.Tests.Providers
{
    public class LocalStorageProviderTests : IDisposable
    {
        private readonly string _testRoot;
        private readonly LocalStorageProvider _provider;


        public LocalStorageProviderTests()
        {
            _testRoot = Path.Combine(Path.GetTempPath(), "deepspace_test_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            _provider = new LocalStorageProvider(new LocalStorageConfig { RootPath = _testRoot });
        }


        public void Dispose()
        {
            if (Directory.Exists(_testRoot))
            {
                Directory.Delete(_testRoot, true);
            }
        }


        // ── Put / Get ────────────────────────────────────────────────────


        [Fact]
        public async Task PutBytes_And_GetBytes_RoundTrips()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello, Deep Space!");

            StorageResult result = await _provider.PutBytesAsync("test/hello.txt", data);

            Assert.True(result.Success);
            Assert.Equal("test/hello.txt", result.Object.Key);

            byte[] retrieved = await _provider.GetBytesAsync("test/hello.txt");

            Assert.Equal("Hello, Deep Space!", Encoding.UTF8.GetString(retrieved));
        }


        [Fact]
        public async Task PutBytes_SetsContentType()
        {
            byte[] data = Encoding.UTF8.GetBytes("{}");

            StorageResult result = await _provider.PutBytesAsync("config.json", data, "application/json");

            Assert.True(result.Success);
        }


        [Fact]
        public async Task GetBytes_NonExistent_ReturnsNull()
        {
            byte[] result = await _provider.GetBytesAsync("nonexistent.txt");

            Assert.Null(result);
        }


        [Fact]
        public async Task GetStream_ReturnsReadableStream()
        {
            byte[] data = Encoding.UTF8.GetBytes("stream data");
            await _provider.PutBytesAsync("stream.txt", data);

            using (Stream stream = await _provider.GetStreamAsync("stream.txt"))
            {
                Assert.NotNull(stream);

                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = await reader.ReadToEndAsync();
                    Assert.Equal("stream data", content);
                }
            }
        }


        [Fact]
        public async Task GetStream_NonExistent_ReturnsNull()
        {
            Stream result = await _provider.GetStreamAsync("missing.txt");

            Assert.Null(result);
        }


        // ── Metadata ─────────────────────────────────────────────────────


        [Fact]
        public async Task PutWithMetadata_And_GetMetadata_Works()
        {
            byte[] data = Encoding.UTF8.GetBytes("test");
            Dictionary<string, string> metadata = new Dictionary<string, string>
            {
                { "author", "demetree" },
                { "version", "1.0" }
            };

            await _provider.PutBytesAsync("meta.txt", data, null, metadata);

            StorageObject obj = await _provider.GetMetadataAsync("meta.txt");

            Assert.NotNull(obj);
            Assert.Equal("meta.txt", obj.Key);
            Assert.Equal("demetree", obj.Metadata["author"]);
            Assert.Equal("1.0", obj.Metadata["version"]);
        }


        [Fact]
        public async Task GetMetadata_NonExistent_ReturnsNull()
        {
            StorageObject result = await _provider.GetMetadataAsync("missing.txt");

            Assert.Null(result);
        }


        [Fact]
        public async Task GetMetadata_SetsSize()
        {
            byte[] data = Encoding.UTF8.GetBytes("12345");
            await _provider.PutBytesAsync("sized.txt", data);

            StorageObject obj = await _provider.GetMetadataAsync("sized.txt");

            Assert.Equal(5, obj.SizeBytes);
        }


        // ── Exists / Delete ──────────────────────────────────────────────


        [Fact]
        public async Task Exists_Present_ReturnsTrue()
        {
            await _provider.PutBytesAsync("exists.txt", new byte[] { 1 });

            Assert.True(await _provider.ExistsAsync("exists.txt"));
        }


        [Fact]
        public async Task Exists_Missing_ReturnsFalse()
        {
            Assert.False(await _provider.ExistsAsync("missing.txt"));
        }


        [Fact]
        public async Task Delete_RemovesFile()
        {
            await _provider.PutBytesAsync("delete.txt", new byte[] { 1 });

            bool deleted = await _provider.DeleteAsync("delete.txt");

            Assert.True(deleted);
            Assert.False(await _provider.ExistsAsync("delete.txt"));
        }


        [Fact]
        public async Task Delete_NonExistent_ReturnsFalse()
        {
            Assert.False(await _provider.DeleteAsync("missing.txt"));
        }


        [Fact]
        public async Task Delete_AlsoRemovesMetadata()
        {
            Dictionary<string, string> meta = new Dictionary<string, string> { { "key", "value" } };
            await _provider.PutBytesAsync("withmeta.txt", new byte[] { 1 }, null, meta);
            await _provider.DeleteAsync("withmeta.txt");

            Assert.False(File.Exists(Path.Combine(_testRoot, "withmeta.txt.meta")));
        }


        // ── List ─────────────────────────────────────────────────────────


        [Fact]
        public async Task List_ReturnsObjects()
        {
            await _provider.PutBytesAsync("docs/a.txt", new byte[] { 1 });
            await _provider.PutBytesAsync("docs/b.txt", new byte[] { 2 });
            await _provider.PutBytesAsync("imgs/c.png", new byte[] { 3 });

            ListResult result = await _provider.ListAsync();

            Assert.True(result.Success);
            Assert.True(result.Objects.Count >= 3);
        }


        [Fact]
        public async Task List_WithPrefix_Filters()
        {
            await _provider.PutBytesAsync("docs/x.txt", new byte[] { 1 });
            await _provider.PutBytesAsync("docs/y.txt", new byte[] { 2 });
            await _provider.PutBytesAsync("other/z.txt", new byte[] { 3 });

            ListResult result = await _provider.ListAsync("docs/");

            Assert.True(result.Success);
            Assert.Equal(2, result.Objects.Count);
        }


        // ── Copy ─────────────────────────────────────────────────────────


        [Fact]
        public async Task Copy_DuplicatesFile()
        {
            await _provider.PutBytesAsync("original.txt", Encoding.UTF8.GetBytes("copy me"));

            StorageResult result = await _provider.CopyAsync("original.txt", "copied.txt");

            Assert.True(result.Success);

            byte[] copiedData = await _provider.GetBytesAsync("copied.txt");
            Assert.Equal("copy me", Encoding.UTF8.GetString(copiedData));
        }


        [Fact]
        public async Task Copy_NonExistentSource_Fails()
        {
            StorageResult result = await _provider.CopyAsync("missing.txt", "dest.txt");

            Assert.False(result.Success);
        }


        // ── Provider Name ────────────────────────────────────────────────


        [Fact]
        public void ProviderName_IsLocal()
        {
            Assert.Equal("Local", _provider.ProviderName);
        }


        // ── Nested Directories ───────────────────────────────────────────


        [Fact]
        public async Task Put_CreatesNestedDirectories()
        {
            byte[] data = Encoding.UTF8.GetBytes("nested");

            StorageResult result = await _provider.PutBytesAsync("a/b/c/d/nested.txt", data);

            Assert.True(result.Success);
            Assert.True(await _provider.ExistsAsync("a/b/c/d/nested.txt"));
        }
    }
}
