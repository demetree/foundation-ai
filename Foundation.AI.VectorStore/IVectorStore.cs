namespace Foundation.AI.VectorStore;

/// <summary>
/// Unified vector storage abstraction with pluggable backends.
///
/// <para><b>Purpose:</b>
/// Provides a single interface for storing and searching vector embeddings,
/// regardless of the underlying storage engine (Zvec, SQLite-vec, cloud services).
/// Applications depend on this interface; providers are swapped via DI configuration.</para>
///
/// <para><b>Usage pattern:</b>
/// <code>
/// // Store
/// await store.UpsertAsync("projects", "proj-42", embedding, metadata);
///
/// // Search
/// var results = await store.SearchAsync("projects", queryVector, topK: 10,
///                                        filter: "status == \"active\"");
/// </code></para>
/// </summary>
public interface IVectorStore : IAsyncDisposable
{
    // ─── Collection Management ──────────────────────────────────────

    /// <summary>
    /// Create a new vector collection with the specified dimensionality.
    /// </summary>
    /// <param name="name">Unique collection name (alphanumeric + underscores).</param>
    /// <param name="dimension">Vector dimensionality (must match your embedding model output).</param>
    /// <param name="options">Optional index and metric configuration.</param>
    Task CreateCollectionAsync(string name, int dimension, VectorStoreOptions? options = null,
                               CancellationToken ct = default);

    /// <summary>Check whether a named collection exists.</summary>
    Task<bool> CollectionExistsAsync(string name, CancellationToken ct = default);

    /// <summary>Delete a collection and all its data. This is irreversible.</summary>
    Task DeleteCollectionAsync(string name, CancellationToken ct = default);

    // ─── Document Operations ────────────────────────────────────────

    /// <summary>
    /// Insert or update a single vector document.
    /// If a document with the same <paramref name="id"/> exists, it is replaced.
    /// </summary>
    Task UpsertAsync(string collection, string id, float[] vector,
                     Dictionary<string, object>? metadata = null,
                     CancellationToken ct = default);

    /// <summary>
    /// Batch insert or update multiple documents for throughput.
    /// Implementations should optimize for bulk operations where possible.
    /// </summary>
    Task UpsertBatchAsync(string collection, IReadOnlyList<VectorDocument> documents,
                          CancellationToken ct = default);

    /// <summary>Delete a document by ID.</summary>
    Task DeleteAsync(string collection, string id, CancellationToken ct = default);

    // ─── Search ─────────────────────────────────────────────────────

    /// <summary>
    /// Find the most similar vectors to a query vector.
    /// </summary>
    /// <param name="collection">Collection to search.</param>
    /// <param name="query">Query vector (must match collection dimensionality).</param>
    /// <param name="topK">Maximum number of results to return.</param>
    /// <param name="filter">Optional filter expression (provider-specific syntax).</param>
    /// <returns>Results ordered by similarity (best first).</returns>
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string collection, float[] query, int topK = 10,
        string? filter = null, CancellationToken ct = default);

    // ─── Maintenance ────────────────────────────────────────────────

    /// <summary>
    /// Flush any buffered writes to durable storage.
    /// Called automatically on dispose, but can be invoked manually
    /// after bulk operations.
    /// </summary>
    Task FlushAsync(string collection, CancellationToken ct = default);

    /// <summary>
    /// Optimize the index for search performance.
    /// For HNSW this is a no-op; for IVF this triggers k-means training.
    /// </summary>
    Task OptimizeAsync(string collection, CancellationToken ct = default);
}
