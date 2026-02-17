namespace Foundation.AI.VectorStore;

/// <summary>
/// Configuration options for creating a vector collection.
/// Not all options are supported by every provider — unsupported options
/// are silently ignored with a warning log.
/// </summary>
public sealed class VectorStoreOptions
{
    /// <summary>
    /// Distance metric for similarity comparison.
    /// Default: Cosine (best for text embeddings from most models).
    /// </summary>
    public VectorMetricType Metric { get; set; } = VectorMetricType.Cosine;

    /// <summary>
    /// Index algorithm type. Default: Hnsw (best general-purpose).
    /// </summary>
    public VectorIndexType IndexType { get; set; } = VectorIndexType.Hnsw;

    /// <summary>
    /// Optional quantization to reduce memory usage.
    /// Default: None (full FP32 precision).
    /// </summary>
    public VectorQuantizeType Quantize { get; set; } = VectorQuantizeType.None;

    /// <summary>
    /// HNSW M parameter — max connections per node. Default: 16.
    /// Higher = better recall, more memory. Range: 12–48.
    /// </summary>
    public int HnswM { get; set; } = 16;

    /// <summary>
    /// HNSW efConstruction — build-time beam width. Default: 200.
    /// Higher = better graph quality, slower build. Range: 100–500.
    /// </summary>
    public int HnswEfConstruction { get; set; } = 200;

    /// <summary>
    /// IVF nlist — number of clusters. Default: 128.
    /// Rule of thumb: √n where n = expected number of vectors.
    /// </summary>
    public int IvfNlist { get; set; } = 128;
}

/// <summary>Distance metric types (provider-agnostic).</summary>
public enum VectorMetricType
{
    /// <summary>Cosine distance. Lower = more similar. Best for text embeddings.</summary>
    Cosine = 0,
    /// <summary>Euclidean (L2) distance. Lower = more similar.</summary>
    L2 = 1,
    /// <summary>Inner product. Higher = more similar.</summary>
    InnerProduct = 2
}

/// <summary>Index algorithm types (provider-agnostic).</summary>
public enum VectorIndexType
{
    /// <summary>HNSW graph index — best general-purpose. O(log n) search.</summary>
    Hnsw = 0,
    /// <summary>IVF cluster index — good for large datasets with training.</summary>
    Ivf = 1,
    /// <summary>Flat brute-force — exact search. O(n) per query.</summary>
    Flat = 2
}

/// <summary>Quantization types (provider-agnostic).</summary>
public enum VectorQuantizeType
{
    /// <summary>No quantization — full FP32 precision.</summary>
    None = 0,
    /// <summary>Half-precision — 2× compression, nearly lossless.</summary>
    FP16 = 1,
    /// <summary>8-bit integer — 4× compression, low error.</summary>
    Int8 = 2,
    /// <summary>4-bit integer — 8× compression, moderate error.</summary>
    Int4 = 3
}
