// Copyright 2025-present the zvec project — Pure C# Engine

namespace Foundation.AI.Zvec.Engine.Index;

/// <summary>
/// Represents a single search result from a vector index.
/// </summary>
public readonly record struct SearchHit(long DocId, float Score);

/// <summary>
/// Query parameters for vector search.
/// </summary>
public sealed class VectorQueryParams
{
    /// <summary>Metric-specific radius threshold (0 = disabled).</summary>
    public float Radius { get; init; }

    // HNSW-specific
    public int Ef { get; init; }
    public bool UseLinearSearch { get; init; }

    // IVF-specific
    public int NProbe { get; init; }
}

/// <summary>
/// Interface for all vector index implementations (HNSW, Flat, IVF).
/// </summary>
public interface IVectorIndex : IDisposable
{
    /// <summary>The index type.</summary>
    Core.IndexType Type { get; }

    /// <summary>Number of vectors in the index.</summary>
    int Count { get; }

    /// <summary>
    /// Add a vector to the index.
    /// </summary>
    /// <param name="docId">The document ID associated with this vector.</param>
    /// <param name="vector">The vector data.</param>
    void Add(long docId, ReadOnlySpan<float> vector);

    /// <summary>
    /// Search for the top-k nearest vectors to the query.
    /// </summary>
    /// <param name="query">The query vector.</param>
    /// <param name="topk">Number of results to return.</param>
    /// <param name="queryParams">Optional query-time parameters.</param>
    /// <param name="deletedFilter">Optional set of deleted doc IDs to exclude.</param>
    /// <returns>Search results sorted by relevance.</returns>
    IReadOnlyList<SearchHit> Search(
        ReadOnlySpan<float> query,
        int topk,
        VectorQueryParams? queryParams = null,
        HashSet<long>? deletedFilter = null);

    /// <summary>
    /// Remove a vector by its document ID.
    /// </summary>
    void Remove(long docId);
}
