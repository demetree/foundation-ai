using Foundation.AI.Zvec;
using ZvecMetricType = Foundation.AI.Zvec.MetricType;
using ZvecQuantizeType = Foundation.AI.Zvec.QuantizeType;

namespace Foundation.AI.VectorStore.Zvec;

/// <summary>
/// Configuration for the Zvec vector store provider.
/// </summary>
public sealed class ZvecVectorStoreConfig
{
    /// <summary>
    /// Base directory where collection data is stored.
    /// Each collection creates a subdirectory under this path.
    /// Default: "./ai-data/vectors"
    /// </summary>
    public string BasePath { get; set; } = "./ai-data/vectors";
}

/// <summary>
/// IVectorStore implementation backed by the Zvec pure C# vector engine.
///
/// <para><b>Best for:</b> Dedicated vector workloads with high throughput requirements.
/// Supports HNSW, IVF, and Flat indexes with optional quantization (FP16/INT8/INT4).</para>
///
/// <para><b>Thread safety:</b> Each collection uses internal ReaderWriterLockSlim.
/// Multiple concurrent searches are supported; writes are serialized.</para>
/// </summary>
public sealed class ZvecVectorStore : IVectorStore
{
    private readonly ZvecVectorStoreConfig _config;
    private readonly Dictionary<string, ZvecCollection> _collections = new();
    private readonly Lock _lock = new();

    /// <summary>
    /// The default vector field name used internally by this provider.
    /// Collections created through this interface always use a single vector field.
    /// </summary>
    private const string VectorFieldName = "vector";
    private const string PkFieldName = "id";

    public ZvecVectorStore(ZvecVectorStoreConfig config)
    {
        _config = config;
        Directory.CreateDirectory(config.BasePath);
    }

    // ─── Collection Management ──────────────────────────────────────

    public Task CreateCollectionAsync(string name, int dimension,
        VectorStoreOptions? options = null, CancellationToken ct = default)
    {
        options ??= new VectorStoreOptions();
        var path = GetCollectionPath(name);

        if (Directory.Exists(path))
            throw new InvalidOperationException($"Collection '{name}' already exists.");

        var schema = new CollectionSchema(name)
            .AddVector(VectorFieldName, DataType.VectorFP32, (uint)dimension,
                MapIndexParams(options));

        var collection = ZvecCollection.CreateAndOpen(path, schema);

        lock (_lock)
        {
            _collections[name] = collection;
        }

        return Task.CompletedTask;
    }

    public Task<bool> CollectionExistsAsync(string name, CancellationToken ct = default)
    {
        var path = GetCollectionPath(name);
        return Task.FromResult(Directory.Exists(path));
    }

    public Task DeleteCollectionAsync(string name, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_collections.TryGetValue(name, out var collection))
            {
                collection.Dispose();
                _collections.Remove(name);
            }
        }

        var path = GetCollectionPath(name);
        if (Directory.Exists(path))
            ZvecCollection.Destroy(path);

        return Task.CompletedTask;
    }

    // ─── Document Operations ────────────────────────────────────────

    public Task UpsertAsync(string collection, string id, float[] vector,
        Dictionary<string, object>? metadata = null, CancellationToken ct = default)
    {
        var coll = GetOrOpenCollection(collection);
        var doc = BuildDoc(id, vector, metadata);
        coll.Upsert(doc);
        return Task.CompletedTask;
    }

    public Task UpsertBatchAsync(string collection,
        IReadOnlyList<VectorDocument> documents, CancellationToken ct = default)
    {
        var coll = GetOrOpenCollection(collection);
        var docs = documents.Select(d => BuildDoc(d.Id, d.Vector, d.Metadata)).ToArray();
        coll.Upsert(docs);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string collection, string id, CancellationToken ct = default)
    {
        var coll = GetOrOpenCollection(collection);
        coll.Delete(id);
        return Task.CompletedTask;
    }

    // ─── Search ─────────────────────────────────────────────────────

    public Task<IReadOnlyList<VectorSearchResult>> SearchAsync(string collection,
        float[] query, int topK = 10, string? filter = null,
        CancellationToken ct = default)
    {
        var coll = GetOrOpenCollection(collection);
        using var results = coll.Query(VectorFieldName, query, topK, filter);

        var searchResults = new List<VectorSearchResult>(results.Count);
        for (int i = 0; i < results.Count; i++)
        {
            var doc = results[i];
            var pk = doc.PrimaryKey;
            var score = doc.Score;
            var metadata = ExtractMetadata(doc);
            searchResults.Add(new VectorSearchResult(pk, score, metadata));
        }

        return Task.FromResult<IReadOnlyList<VectorSearchResult>>(searchResults);
    }

    // ─── Maintenance ────────────────────────────────────────────────

    public Task FlushAsync(string collection, CancellationToken ct = default)
    {
        var coll = GetOrOpenCollection(collection);
        coll.Flush();
        return Task.CompletedTask;
    }

    public Task OptimizeAsync(string collection, CancellationToken ct = default)
    {
        var coll = GetOrOpenCollection(collection);
        coll.Optimize();
        return Task.CompletedTask;
    }

    // ─── IAsyncDisposable ───────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        List<ZvecCollection> toDispose;
        lock (_lock)
        {
            toDispose = [.. _collections.Values];
            _collections.Clear();
        }

        foreach (var coll in toDispose)
        {
            try { coll.Flush(); } catch { /* best effort */ }
            coll.Dispose();
        }

        await Task.CompletedTask;
    }

    // ─── Private Helpers ────────────────────────────────────────────

    private string GetCollectionPath(string name) =>
        Path.Combine(_config.BasePath, name);

    private ZvecCollection GetOrOpenCollection(string name)
    {
        lock (_lock)
        {
            if (_collections.TryGetValue(name, out var existing))
                return existing;
        }

        var path = GetCollectionPath(name);
        if (!Directory.Exists(path))
            throw new InvalidOperationException($"Collection '{name}' does not exist.");

        var collection = ZvecCollection.Open(path);

        lock (_lock)
        {
            // Double-check — another thread may have opened it
            if (_collections.TryGetValue(name, out var existing))
            {
                collection.Dispose();
                return existing;
            }
            _collections[name] = collection;
            return collection;
        }
    }

    private static ZvecDoc BuildDoc(string id, float[] vector,
        Dictionary<string, object>? metadata)
    {
        var doc = new ZvecDoc()
        {
            PrimaryKey = id
        };
        doc.Set(VectorFieldName, vector);

        if (metadata != null)
        {
            foreach (var (key, value) in metadata)
            {
                switch (value)
                {
                    case string s: doc.Set(key, s); break;
                    case int i: doc.Set(key, i); break;
                    case long l: doc.Set(key, l); break;
                    case float f: doc.Set(key, f); break;
                    case double d: doc.Set(key, d); break;
                    case bool b: doc.Set(key, b); break;
                }
            }
        }

        return doc;
    }

    private static Dictionary<string, object>? ExtractMetadata(ZvecDoc doc)
    {
        // The Zvec SDK doesn't expose a field enumeration API,
        // so metadata round-tripping relies on the caller knowing their keys.
        // This is a known limitation — future versions could store a field manifest.
        return null;
    }

    private static IndexParams MapIndexParams(VectorStoreOptions options)
    {
        return options.IndexType switch
        {
            VectorIndexType.Hnsw => new HnswIndexParams(
                metric: MapMetric(options.Metric),
                m: options.HnswM,
                efConstruction: options.HnswEfConstruction,
                quantize: MapQuantize(options.Quantize)),

            VectorIndexType.Ivf => new IvfIndexParams(
                metric: MapMetric(options.Metric),
                nList: options.IvfNlist,
                quantize: MapQuantize(options.Quantize)),

            VectorIndexType.Flat => new FlatIndexParams(
                metric: MapMetric(options.Metric),
                quantize: MapQuantize(options.Quantize)),

            _ => new HnswIndexParams(metric: MapMetric(options.Metric))
        };
    }

    private static ZvecMetricType MapMetric(VectorMetricType metric) => metric switch
    {
        VectorMetricType.Cosine => ZvecMetricType.Cosine,
        VectorMetricType.L2 => ZvecMetricType.L2,
        VectorMetricType.InnerProduct => ZvecMetricType.IP,
        _ => ZvecMetricType.Cosine
    };

    private static ZvecQuantizeType MapQuantize(VectorQuantizeType q) => q switch
    {
        VectorQuantizeType.FP16 => ZvecQuantizeType.FP16,
        VectorQuantizeType.Int8 => ZvecQuantizeType.Int8,
        VectorQuantizeType.Int4 => ZvecQuantizeType.Int4,
        _ => ZvecQuantizeType.Undefined
    };
}
