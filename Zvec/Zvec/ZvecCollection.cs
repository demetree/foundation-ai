using System.Collections;
using Foundation.AI.Zvec.Engine.Core;

namespace Foundation.AI.Zvec;

/// <summary>
/// Options for opening or creating a collection.
/// </summary>
public sealed class CollectionOptions
{
    public bool ReadOnly { get; set; }
    public bool EnableMmap { get; set; }
    public uint MaxBufferSize { get; set; }

    /// <summary>
    /// When true, a collection whose on-disk store is corrupted will be
    /// quarantined (renamed to <c>{name}.corrupt-{utcTimestamp}</c>) and
    /// re-created empty rather than throwing. Enable only for derivative
    /// collections that can be rebuilt from an upstream source.
    /// </summary>
    public bool AllowDestructiveRecovery { get; set; }

    internal EngineCollectionOptions ToEngineOptions() => new()
    {
        ReadOnly = ReadOnly,
        EnableMmap = EnableMmap,
        MaxBufferSize = MaxBufferSize,
        AllowDestructiveRecovery = AllowDestructiveRecovery
    };
}

/// <summary>
/// Represents a zvec vector database collection.
/// This is the primary entry point for all database operations.
/// Now powered by the pure C# engine — no native dependencies.
/// </summary>
public sealed class ZvecCollection : IDisposable
{
    private Collection _engine;
    private bool _disposed;

    private ZvecCollection(Collection engine)
    {
        _engine = engine;
    }

    // =========================================================================
    // Static factory methods
    // =========================================================================

    /// <summary>
    /// Create a new collection and open it.
    /// </summary>
    public static ZvecCollection CreateAndOpen(
        string path, CollectionSchema schema, CollectionOptions? options = null)
    {
        var engineSchema = schema.BuildSchema();
        var engineOptions = options?.ToEngineOptions();
        var engine = Collection.CreateAndOpen(path, engineSchema, engineOptions);
        return new ZvecCollection(engine);
    }

    /// <summary>
    /// Open an existing collection.
    /// </summary>
    public static ZvecCollection Open(
        string path, CollectionOptions? options = null)
    {
        var engine = Collection.Open(path, options?.ToEngineOptions());
        return new ZvecCollection(engine);
    }

    /// <summary>
    /// Destroy a collection directory.
    /// </summary>
    public static void Destroy(string path)
    {
        Collection.Destroy(path);
    }

    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>Total document count (non-deleted).</summary>
    public ulong DocCount => _engine.DocCount;

    // =========================================================================
    // Write operations
    // =========================================================================

    /// <summary>Insert documents into the collection.</summary>
    public void Insert(params ZvecDoc[] docs)
    {
        var engineDocs = docs.Select(d => d.ToEngineDocument()).ToList();
        _engine.Insert(engineDocs);
    }

    /// <summary>Upsert documents (insert or update by primary key).</summary>
    public void Upsert(params ZvecDoc[] docs)
    {
        var engineDocs = docs.Select(d => d.ToEngineDocument()).ToList();
        _engine.Upsert(engineDocs);
    }

    /// <summary>Update existing documents.</summary>
    public void Update(params ZvecDoc[] docs)
    {
        var engineDocs = docs.Select(d => d.ToEngineDocument()).ToList();
        _engine.Update(engineDocs);
    }

    /// <summary>Delete documents by primary keys.</summary>
    public void Delete(params string[] pks)
    {
        _engine.DeleteByPks(pks);
    }

    /// <summary>Delete documents matching a filter expression.</summary>
    public ulong DeleteByFilter(string filter)
    {
        return _engine.DeleteByFilter(filter);
    }

    // =========================================================================
    // Query operations
    // =========================================================================

    /// <summary>
    /// Query the collection with a vector similarity search.
    /// </summary>
    public unsafe QueryResult Query(
        string fieldName,
        ReadOnlySpan<float> vector,
        int topk = 10,
        string? filter = null,
        bool includeVector = false,
        QueryParams? queryParams = null)
    {
        var engineQueryParams = queryParams?.ToEngineParams();
        var results = _engine.Query(fieldName, vector, topk, filter, includeVector, engineQueryParams);
        return new QueryResult(results);
    }

    /// <summary>
    /// Fetch documents by primary keys.
    /// </summary>
    public IReadOnlyList<ZvecDoc> Fetch(params string[] pks)
    {
        var results = _engine.Fetch(pks);
        return results.Select(d => new ZvecDoc(d)).ToList();
    }

    // =========================================================================
    // Collection operations
    // =========================================================================

    /// <summary>Flush pending writes to durable storage.</summary>
    public void Flush() => _engine.Flush();

    /// <summary>Optimize indexes for query performance.</summary>
    public void Optimize(int concurrency = 0) => _engine.Optimize(concurrency);

    /// <summary>Create an index on a column.</summary>
    public void CreateIndex(string columnName, IndexParams indexParams, int concurrency = 0)
    {
        _engine.CreateIndex(columnName, indexParams.ToEngineConfig(), concurrency);
    }

    /// <summary>Drop an index from a column.</summary>
    public void DropIndex(string columnName) => _engine.DropIndex(columnName);

    // =========================================================================
    // Dispose
    // =========================================================================

    public void Dispose()
    {
        if (!_disposed)
        {
            _engine.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~ZvecCollection() => Dispose();
}

// =========================================================================
// Query result wrapper
// =========================================================================

/// <summary>
/// Holds the results of a vector query. Implements IReadOnlyList for easy iteration.
/// Now wraps a managed list of documents — no native handles.
/// </summary>
public sealed class QueryResult : IDisposable, IReadOnlyList<ZvecDoc>
{
    private readonly List<ZvecDoc> _docs;
    private bool _disposed;

    internal QueryResult(IReadOnlyList<Document> engineDocs)
    {
        _docs = engineDocs.Select(d => new ZvecDoc(d)).ToList();
    }

    public int Count => _docs.Count;

    public ZvecDoc this[int index]
    {
        get
        {
            if (index < 0 || index >= _docs.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _docs[index];
        }
    }

    public IEnumerator<ZvecDoc> GetEnumerator() => _docs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~QueryResult() => Dispose();
}
