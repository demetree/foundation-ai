using Foundation.AI.VectorStore;
using Foundation.AI.VectorStore.Zvec;

namespace Foundation.AI.Test;

/// <summary>
/// Integration tests for IVectorStore + ZvecVectorStore.
/// Tests the full pipeline: create → upsert → search → delete.
/// </summary>
public class VectorStoreTests : IAsyncLifetime
{
    private readonly string _testPath;
    private IVectorStore _store = null!;

    public VectorStoreTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), $"vectorstore_test_{Guid.NewGuid():N}");
    }

    public Task InitializeAsync()
    {
        _store = new ZvecVectorStore(new ZvecVectorStoreConfig
        {
            BasePath = _testPath
        });
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _store.DisposeAsync();
        if (Directory.Exists(_testPath))
            Directory.Delete(_testPath, recursive: true);
    }

    [Fact]
    public async Task CreateCollection_ShouldCreateDirectory()
    {
        await _store.CreateCollectionAsync("test_create", 4);

        var exists = await _store.CollectionExistsAsync("test_create");
        Assert.True(exists);
    }

    [Fact]
    public async Task UpsertAndSearch_ShouldReturnSimilarVectors()
    {
        const int dim = 4;
        const string collection = "test_search";

        await _store.CreateCollectionAsync(collection, dim, new VectorStoreOptions
        {
            Metric = VectorMetricType.L2,
            IndexType = VectorIndexType.Flat
        });

        // Insert some vectors
        await _store.UpsertAsync(collection, "vec-1", [1f, 0f, 0f, 0f]);
        await _store.UpsertAsync(collection, "vec-2", [0f, 1f, 0f, 0f]);
        await _store.UpsertAsync(collection, "vec-3", [0.9f, 0.1f, 0f, 0f]);

        // Search for vector closest to [1, 0, 0, 0]
        var results = await _store.SearchAsync(collection, [1f, 0f, 0f, 0f], topK: 3);

        Assert.Equal(3, results.Count);
        // Closest should be vec-1 (exact match)
        Assert.Equal("vec-1", results[0].Id);
        // Second closest should be vec-3 (0.9, 0.1, 0, 0 is closer to (1,0,0,0) than (0,1,0,0))
        Assert.Equal("vec-3", results[1].Id);
    }

    [Fact]
    public async Task UpsertBatch_ShouldInsertMultipleDocuments()
    {
        const int dim = 4;
        const string collection = "test_batch";

        await _store.CreateCollectionAsync(collection, dim, new VectorStoreOptions
        {
            Metric = VectorMetricType.L2,
            IndexType = VectorIndexType.Flat
        });

        var docs = new List<VectorDocument>
        {
            new("batch-1", [1f, 0f, 0f, 0f]),
            new("batch-2", [0f, 1f, 0f, 0f]),
            new("batch-3", [0f, 0f, 1f, 0f]),
        };

        await _store.UpsertBatchAsync(collection, docs);
        var results = await _store.SearchAsync(collection, [1f, 0f, 0f, 0f], topK: 3);

        Assert.Equal(3, results.Count);
        Assert.Equal("batch-1", results[0].Id);
    }

    [Fact]
    public async Task Delete_ShouldRemoveDocument()
    {
        const int dim = 4;
        const string collection = "test_delete";

        await _store.CreateCollectionAsync(collection, dim, new VectorStoreOptions
        {
            Metric = VectorMetricType.L2,
            IndexType = VectorIndexType.Flat
        });

        await _store.UpsertAsync(collection, "to-delete", [1f, 0f, 0f, 0f]);
        await _store.UpsertAsync(collection, "to-keep", [0f, 1f, 0f, 0f]);

        await _store.DeleteAsync(collection, "to-delete");
        await _store.FlushAsync(collection);

        var results = await _store.SearchAsync(collection, [1f, 0f, 0f, 0f], topK: 10);

        // Only "to-keep" should remain
        Assert.Single(results);
        Assert.Equal("to-keep", results[0].Id);
    }

    [Fact]
    public async Task DeleteCollection_ShouldRemoveAllData()
    {
        const string collection = "test_destroy";

        await _store.CreateCollectionAsync(collection, 4);
        await _store.UpsertAsync(collection, "some-doc", [1f, 0f, 0f, 0f]);

        await _store.DeleteCollectionAsync(collection);

        var exists = await _store.CollectionExistsAsync(collection);
        Assert.False(exists);
    }
}
