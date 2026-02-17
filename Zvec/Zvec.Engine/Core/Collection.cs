// Copyright 2025-present the zvec project — Pure C# Engine
// Core Collection engine — orchestrates storage, indexing, and queries

using Foundation.AI.Zvec.Engine.Filter;
using Foundation.AI.Zvec.Engine.Index;
using Foundation.AI.Zvec.Engine.Math;
using Foundation.AI.Zvec.Engine.Storage;

namespace Foundation.AI.Zvec.Engine.Core;

/// <summary>
/// Options for creating or opening a collection.
/// </summary>
public sealed class EngineCollectionOptions
{
    public bool ReadOnly { get; init; }
    public bool EnableMmap { get; init; } = true;
    public uint MaxBufferSize { get; init; }
}

/// <summary>
/// The core managed collection engine — the central orchestration layer.
///
/// <para><b>Responsibilities:</b>
/// Coordinates storage (ZoneTree), vector indexes (HNSW/IVF/Flat), and the filter
/// engine to provide unified document CRUD and vector similarity search.</para>
///
/// <para><b>Lifecycle:</b>
/// <c>CreateAndOpen</c> → initializes storage + schema + indexes from scratch.
/// <c>Open</c> → loads persisted metadata, reopens storage, restores index snapshots.
/// <c>Flush</c> → persists metadata, index snapshots, and flushes storage WAL.
/// <c>Dispose</c> → closes storage and releases resources.</para>
///
/// <para><b>Persistence model:</b>
/// Documents are stored in ZoneTree (LSM-tree with WAL). Vector indexes are
/// persisted as separate snapshot files (binary for HNSW, JSON for IVF).
/// Collection metadata (schema, nextDocId, index configs) is stored as JSON.</para>
///
/// <para><b>Concurrency:</b>
/// Uses ReaderWriterLockSlim — multiple concurrent reads (queries/fetches)
/// with single-writer mutations (insert/update/delete/optimize).</para>
/// </summary>
public sealed class Collection : IDisposable
{
    private readonly string _path;
    private readonly CollectionSchemaDefinition _schema;
    private readonly EngineCollectionOptions _options;
    private readonly IStorageEngine _storage;
    private readonly Dictionary<string, IVectorIndex> _vectorIndexes;
    private readonly ReaderWriterLockSlim _rwLock = new();
    private readonly HashSet<long> _deletedDocIds = [];

    private long _nextDocId;
    private bool _disposed;

    private Collection(string path, CollectionSchemaDefinition schema,
                       EngineCollectionOptions options, IStorageEngine storage)
    {
        _path = path;
        _schema = schema;
        _options = options;
        _storage = storage;
        _vectorIndexes = new Dictionary<string, IVectorIndex>();

        // Create default indexes for vector fields
        foreach (var field in schema.VectorFields)
        {
            var config = field.IndexConfig;
            var metric = config?.Metric ?? MetricType.InnerProduct;
            var indexType = config?.Type ?? IndexType.Flat;

            IVectorIndex index = indexType switch
            {
                IndexType.Hnsw => CreateHnswIndex(metric, config),
                IndexType.Ivf => CreateIvfIndex(metric, config),
                IndexType.Flat => new FlatIndex(metric),
                _ => new FlatIndex(metric)
            };

            _vectorIndexes[field.Name] = index;
        }
    }

    private static IVectorIndex CreateHnswIndex(MetricType metric, IndexConfig? config)
    {
        int m = config?.M > 0 ? config.M : 16;
        int efConstruction = config?.EfConstruction > 0 ? config.EfConstruction : 200;
        var quantize = config?.Quantize ?? QuantizeType.Undefined;
        return new HnswIndex(metric, m, efConstruction, quantize);
    }

    private static IVectorIndex CreateIvfIndex(MetricType metric, IndexConfig? config)
    {
        int nlist = config?.Nlist > 0 ? config.Nlist : 128;
        int nprobe = config?.Nprobe > 0 ? config.Nprobe : 8;
        var quantize = config?.Quantize ?? QuantizeType.Undefined;
        return new IvfIndex(metric, nlist, nprobe, quantize);
    }

    // =========================================================================
    // Static factory methods
    // =========================================================================

    /// <summary>
    /// Create a new collection and open it.
    /// </summary>
    public static Collection CreateAndOpen(
        string path,
        CollectionSchemaDefinition schema,
        EngineCollectionOptions? options = null)
    {
        var opts = options ?? new EngineCollectionOptions();

        // Create directory if needed
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        // Use ZoneTree for persistent storage
        var storage = new ZoneTreeStorageEngine(path);

        var collection = new Collection(path, schema, opts, storage);

        // Persist metadata for reopening
        MetadataPersistence.Save(path,
            MetadataPersistence.FromSchema(schema, collection._nextDocId));

        return collection;
    }

    /// <summary>
    /// Open an existing collection from disk.
    /// </summary>
    public static Collection Open(string path, EngineCollectionOptions? options = null)
    {
        var opts = options ?? new EngineCollectionOptions();

        if (!Directory.Exists(path))
            throw new InvalidOperationException($"Collection path does not exist: {path}");

        // Load persisted metadata
        var metadata = MetadataPersistence.Load(path)
            ?? throw new InvalidOperationException(
                $"No collection metadata found at: {path}");

        var schema = MetadataPersistence.ToSchema(metadata);
        var storage = new ZoneTreeStorageEngine(path);

        var collection = new Collection(path, schema, opts, storage);
        collection._nextDocId = metadata.NextDocId;

        // Try to load persisted indexes first (much faster than rebuild)
        bool indexesLoaded = false;
        foreach (var (fieldName, index) in collection._vectorIndexes)
        {
            if (index is HnswIndex hnsw && IndexPersistence.HasHnswIndex(path))
            {
                IndexPersistence.LoadHnsw(path, hnsw);
                indexesLoaded = true;
            }
            else if (index is IvfIndex ivf && IndexPersistence.HasIvfIndex(path))
            {
                IndexPersistence.LoadIvf(path, ivf);
                indexesLoaded = true;
            }
        }

        // Fallback: rebuild indexes from stored documents if no persisted index
        if (!indexesLoaded)
        {
            foreach (var docId in storage.AllDocIds())
            {
                var doc = storage.Get(docId);
                if (doc == null) continue;

                foreach (var (fieldName, vector) in doc.Vectors)
                {
                    if (collection._vectorIndexes.TryGetValue(fieldName, out var index))
                        index.Add(docId, vector);
                }

                // Track the highest docId for ID generation
                if (docId > collection._nextDocId)
                    collection._nextDocId = docId;
            }
        }
        // Note: _nextDocId is already restored from persisted metadata (line 131),
        // so no need to scan AllDocIds() again when indexes loaded from disk.

        return collection;
    }

    /// <summary>
    /// Destroy a collection by deleting its directory.
    /// </summary>
    public static void Destroy(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>Collection path on disk.</summary>
    public string Path => _path;

    /// <summary>Number of (non-deleted) documents.</summary>
    public ulong DocCount
    {
        get
        {
            _rwLock.EnterReadLock();
            try
            {
                return (ulong)(_storage.DocumentCount - _deletedDocIds.Count);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
    }

    /// <summary>The collection schema.</summary>
    public CollectionSchemaDefinition Schema => _schema;

    // =========================================================================
    // Write operations
    // =========================================================================

    /// <summary>
    /// Insert one or more documents.
    /// </summary>
    public void Insert(IReadOnlyList<Document> docs)
    {
        _rwLock.EnterWriteLock();
        try
        {
            foreach (var doc in docs)
            {
                var docId = Interlocked.Increment(ref _nextDocId);
                doc.DocId = docId;

                // Store document
                _storage.Put(docId, doc);
                _storage.MapPrimaryKey(doc.PrimaryKey, docId);

                // Index vectors
                foreach (var (fieldName, vector) in doc.Vectors)
                {
                    if (_vectorIndexes.TryGetValue(fieldName, out var index))
                    {
                        index.Add(docId, vector);
                    }
                }
            }
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Upsert documents (insert or update by primary key).
    /// </summary>
    public void Upsert(IReadOnlyList<Document> docs)
    {
        _rwLock.EnterWriteLock();
        try
        {
            foreach (var doc in docs)
            {
                var existingId = _storage.LookupPrimaryKey(doc.PrimaryKey);
                if (existingId.HasValue)
                {
                    // Remove old from indexes
                    RemoveFromIndexes(existingId.Value);
                    _storage.Delete(existingId.Value);
                    _storage.RemovePrimaryKey(doc.PrimaryKey);
                    _deletedDocIds.Remove(existingId.Value);
                }

                var docId = Interlocked.Increment(ref _nextDocId);
                doc.DocId = docId;
                _storage.Put(docId, doc);
                _storage.MapPrimaryKey(doc.PrimaryKey, docId);

                foreach (var (fieldName, vector) in doc.Vectors)
                {
                    if (_vectorIndexes.TryGetValue(fieldName, out var index))
                        index.Add(docId, vector);
                }
            }
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Update existing documents.
    /// </summary>
    public void Update(IReadOnlyList<Document> docs)
    {
        _rwLock.EnterWriteLock();
        try
        {
            foreach (var doc in docs)
            {
                var existingId = _storage.LookupPrimaryKey(doc.PrimaryKey);
                if (!existingId.HasValue)
                    continue; // skip docs that don't exist

                RemoveFromIndexes(existingId.Value);
                _storage.Delete(existingId.Value);

                var docId = Interlocked.Increment(ref _nextDocId);
                doc.DocId = docId;
                _storage.Put(docId, doc);
                _storage.MapPrimaryKey(doc.PrimaryKey, docId);

                foreach (var (fieldName, vector) in doc.Vectors)
                {
                    if (_vectorIndexes.TryGetValue(fieldName, out var index))
                        index.Add(docId, vector);
                }
            }
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Delete documents by primary keys.
    /// </summary>
    public void DeleteByPks(IReadOnlyList<string> pks)
    {
        _rwLock.EnterWriteLock();
        try
        {
            foreach (var pk in pks)
            {
                var docId = _storage.LookupPrimaryKey(pk);
                if (!docId.HasValue) continue;

                RemoveFromIndexes(docId.Value);
                _storage.Delete(docId.Value);
                _storage.RemovePrimaryKey(pk);
            }
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Delete documents matching a filter expression.
    /// </summary>
    public ulong DeleteByFilter(string filter)
    {
        var filterNode = FilterParser.Parse(filter);

        _rwLock.EnterWriteLock();
        try
        {
            ulong deleted = 0;
            var toDelete = new List<(long docId, string pk)>();

            foreach (var docId in _storage.AllDocIds())
            {
                if (_deletedDocIds.Contains(docId)) continue;
                var doc = _storage.Get(docId);
                if (doc == null) continue;

                if (filterNode.Evaluate(doc))
                    toDelete.Add((docId, doc.PrimaryKey));
            }

            foreach (var (docId, pk) in toDelete)
            {
                RemoveFromIndexes(docId);
                _storage.Delete(docId);
                _storage.RemovePrimaryKey(pk);
                deleted++;
            }

            return deleted;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    // =========================================================================
    // Read operations
    // =========================================================================

    /// <summary>
    /// Execute a vector similarity search.
    /// </summary>
    public IReadOnlyList<Document> Query(
        string fieldName,
        ReadOnlySpan<float> vector,
        int topk = 10,
        string? filter = null,
        bool includeVector = false,
        VectorQueryParams? queryParams = null)
    {
        _rwLock.EnterReadLock();
        try
        {
            if (!_vectorIndexes.TryGetValue(fieldName, out var index))
                throw new InvalidOperationException($"No vector index for field '{fieldName}'");

            // Parse filter if provided
            IFilterNode? filterNode = null;
            if (!string.IsNullOrWhiteSpace(filter))
                filterNode = FilterParser.Parse(filter);

            // Request extra candidates if filtering (we may need to discard some)
            int searchK = filterNode != null ? topk * 4 : topk;
            var hits = index.Search(vector, searchK, queryParams, _deletedDocIds);

            var results = new List<Document>(hits.Count);
            foreach (var hit in hits)
            {
                var doc = _storage.Get(hit.DocId);
                if (doc == null) continue;

                // Apply scalar filter
                if (filterNode != null && !filterNode.Evaluate(doc))
                    continue;

                var result = includeVector ? doc.Clone() : new Document
                {
                    PrimaryKey = doc.PrimaryKey,
                    DocId = doc.DocId,
                    Score = hit.Score
                };

                // Always copy scalar fields
                foreach (var (k, v) in doc.Fields)
                    result.Fields[k] = v;

                if (!includeVector)
                    result.Vectors.Clear();

                result.Score = hit.Score;
                results.Add(result);

                if (results.Count >= topk)
                    break;
            }

            return results;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Fetch documents by primary keys.
    /// </summary>
    public IReadOnlyList<Document> Fetch(IReadOnlyList<string> pks)
    {
        _rwLock.EnterReadLock();
        try
        {
            var results = new List<Document>(pks.Count);
            foreach (var pk in pks)
            {
                var docId = _storage.LookupPrimaryKey(pk);
                if (!docId.HasValue) continue;
                if (_deletedDocIds.Contains(docId.Value)) continue;

                var doc = _storage.Get(docId.Value);
                if (doc != null)
                    results.Add(doc);
            }
            return results;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    // =========================================================================
    // Collection operations
    // =========================================================================

    /// <summary>Flush pending writes to durable storage.</summary>
    public void Flush()
    {
        _storage.Flush();

        // Persist vector indexes
        foreach (var (_, index) in _vectorIndexes)
        {
            if (index is HnswIndex hnsw)
                IndexPersistence.SaveHnsw(_path, hnsw);
            else if (index is IvfIndex ivf)
                IndexPersistence.SaveIvf(_path, ivf);
        }

        // Update metadata with current state
        MetadataPersistence.Save(_path,
            MetadataPersistence.FromSchema(_schema, _nextDocId));
    }

    /// <summary>Optimize indexes for query performance.</summary>
    public void Optimize(int concurrency = 0)
    {
        _rwLock.EnterWriteLock();
        try
        {
            foreach (var (_, index) in _vectorIndexes)
            {
                if (index is IvfIndex ivf)
                    ivf.Train();

                // Recalibrate quantization with global min/max
                if (index is HnswIndex hnsw)
                    hnsw.Recalibrate();
            }
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Create an index on a column.
    /// </summary>
    public void CreateIndex(string columnName, IndexConfig indexConfig, int concurrency = 0)
    {
        _rwLock.EnterWriteLock();
        try
        {
            var field = _schema.GetField(columnName);
            if (field == null)
                throw new InvalidOperationException($"Field '{columnName}' not found in schema");

            if (!field.IsVectorField)
            {
                // TODO: Scalar inverted index
                return;
            }

            var metric = indexConfig.Metric;
            IVectorIndex newIndex = indexConfig.Type switch
            {
                IndexType.Hnsw => CreateHnswIndex(metric, indexConfig),
                IndexType.Ivf => CreateIvfIndex(metric, indexConfig),
                IndexType.Flat => new FlatIndex(metric),
                _ => new FlatIndex(metric)
            };

            // Rebuild: re-add all vectors from existing storage
            foreach (var docId in _storage.AllDocIds())
            {
                var doc = _storage.Get(docId);
                if (doc == null || _deletedDocIds.Contains(docId)) continue;

                if (doc.Vectors.TryGetValue(columnName, out var vec))
                    newIndex.Add(docId, vec);
            }

            // Dispose old index if exists
            if (_vectorIndexes.TryGetValue(columnName, out var old))
                old.Dispose();

            _vectorIndexes[columnName] = newIndex;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Drop an index from a column.
    /// </summary>
    public void DropIndex(string columnName)
    {
        _rwLock.EnterWriteLock();
        try
        {
            if (_vectorIndexes.TryGetValue(columnName, out var index))
            {
                index.Dispose();
                _vectorIndexes.Remove(columnName);
            }
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private void RemoveFromIndexes(long docId)
    {
        foreach (var (_, index) in _vectorIndexes)
        {
            index.Remove(docId);
        }
    }

    // =========================================================================
    // Dispose
    // =========================================================================

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var (_, index) in _vectorIndexes)
            index.Dispose();
        _vectorIndexes.Clear();

        _storage.Dispose();
        _rwLock.Dispose();

        _disposed = true;
    }
}
