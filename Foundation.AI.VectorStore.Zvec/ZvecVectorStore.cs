using System.Collections.Concurrent;
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
/// <para><b>Thread safety:</b> Each <see cref="ZvecCollection"/> is opened exactly
/// once per process via a <see cref="Lazy{T}"/> held in a
/// <see cref="ConcurrentDictionary{TKey, TValue}"/>. Concurrent callers for the
/// same collection name coalesce onto one open operation — this prevents the
/// "file is being used by another process" race that otherwise occurs when a
/// background write path and a foreground read path both call
/// <c>ZvecCollection.Open</c> at the same time against a still-initialising
/// ZoneTree WAL (the OS rejects the second handle's incompatible share mode
/// even though both are inside one process). Callers for different collection
/// names do not block each other.</para>
/// </summary>
public sealed class ZvecVectorStore : IVectorStore
{
    private readonly ZvecVectorStoreConfig _config;

    //
    // Lazy per-collection so concurrent requests for the same collection
    // name coalesce to a single ZvecCollection.Open / CreateAndOpen call.
    // LazyThreadSafetyMode.ExecutionAndPublication guarantees the factory
    // runs at most once even when many threads race to access .Value.
    //
    private readonly ConcurrentDictionary<string, Lazy<ZvecCollection>> _collections = new();

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
        var metaPath = Path.Combine(path, "collection_meta.json");

        if (File.Exists(metaPath))
            throw new InvalidOperationException($"Collection '{name}' already exists.");

        if (Directory.Exists(path) && !File.Exists(metaPath))
        {
            Directory.Delete(path, true);
        }

        var schema = new CollectionSchema(name)
            .AddVector(VectorFieldName, DataType.VectorFP32, (uint)dimension,
                MapIndexParams(options));

        //
        // Register the Lazy BEFORE calling CreateAndOpen. If another thread
        // concurrently calls GetOrOpenCollection for this name (e.g. a search
        // while indexing is starting), it will race us to TryAdd, but only
        // one Lazy wins — both threads end up awaiting the same factory and
        // receive the same ZvecCollection. That's the whole point of the
        // ConcurrentDictionary+Lazy pattern: there is no window where two
        // threads both try to open the WAL files.
        //
        var lazy = new Lazy<ZvecCollection>(
            () => ZvecCollection.CreateAndOpen(path, schema),
            LazyThreadSafetyMode.ExecutionAndPublication);

        if (!_collections.TryAdd(name, lazy))
        {
            // Another caller beat us to CreateCollection — respect their op
            // and surface the same "already exists" semantics as before.
            throw new InvalidOperationException($"Collection '{name}' already exists.");
        }

        //
        // Force the factory eagerly so CreateAndOpen failures surface here
        // rather than at first Upsert time. On failure we evict the Lazy so
        // a retry can start from a clean slate (leaving a failed Lazy cached
        // would make every future call throw the same exception).
        //
        try
        {
            _ = lazy.Value;
        }
        catch
        {
            _collections.TryRemove(name, out _);
            throw;
        }

        return Task.CompletedTask;
    }

    public Task<bool> CollectionExistsAsync(string name, CancellationToken ct = default)
    {
        var path = GetCollectionPath(name);
        var metaPath = Path.Combine(path, "collection_meta.json");
        return Task.FromResult(File.Exists(metaPath));
    }

    public Task DeleteCollectionAsync(string name, CancellationToken ct = default)
    {
        if (_collections.TryRemove(name, out var lazy) && lazy.IsValueCreated)
        {
            try { lazy.Value.Dispose(); } catch { /* best effort */ }
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
        // Snapshot and clear first so in-flight callers see empty and any
        // new calls fail fast rather than racing with disposal.
        var snapshot = _collections.ToArray();
        _collections.Clear();

        foreach (var (_, lazy) in snapshot)
        {
            // Only dispose collections that actually materialised — an
            // un-forced Lazy never opened a file and has nothing to clean up.
            if (!lazy.IsValueCreated) continue;

            ZvecCollection coll;
            try { coll = lazy.Value; }
            catch { continue; } // factory itself threw — nothing to dispose

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
        //
        // The previous implementation had a check-open-check window where two
        // threads could both call ZvecCollection.Open(path) for the same
        // collection name; the second Open would hit the OS file-share error
        // because the first was still acquiring ZoneTree WAL handles. Switch
        // to ConcurrentDictionary<string, Lazy<T>> so the factory runs at
        // most once per collection name. Different names never contend.
        //
        var lazy = _collections.GetOrAdd(name, n => new Lazy<ZvecCollection>(
            () =>
            {
                var path = GetCollectionPath(n);
                var metaPath = Path.Combine(path, "collection_meta.json");
                if (!File.Exists(metaPath))
                    throw new InvalidOperationException(
                        $"Collection '{n}' does not exist or is missing metadata.");
                return ZvecCollection.Open(path);
            },
            LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            return lazy.Value;
        }
        catch
        {
            // Evict a failed Lazy so callers can retry. A cached failed Lazy
            // would cause every subsequent call to throw the same exception
            // even after the underlying condition (e.g. missing meta file) is
            // resolved.
            _collections.TryRemove(name, out _);
            throw;
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
        var fields = doc.ReadOnlyFields;
        if (fields.Count == 0)
            return null;

        var metadata = new Dictionary<string, object>(fields.Count);
        foreach (var (key, value) in fields)
        {
            if (value != null)
                metadata[key] = value;
        }

        return metadata.Count > 0 ? metadata : null;
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
