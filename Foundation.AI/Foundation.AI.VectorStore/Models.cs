namespace Foundation.AI.VectorStore;

/// <summary>
/// A document in a vector collection — wraps an ID, embedding vector, and optional metadata.
/// </summary>
public sealed record VectorDocument(
    string Id,
    float[] Vector,
    Dictionary<string, object>? Metadata = null);

/// <summary>
/// A single search result with relevance score.
/// Score semantics depend on the metric: for cosine/L2, lower is better;
/// for inner product, higher is better. Results are always returned
/// best-first regardless of metric direction.
/// </summary>
public sealed record VectorSearchResult(
    string Id,
    float Score,
    Dictionary<string, object>? Metadata = null);
