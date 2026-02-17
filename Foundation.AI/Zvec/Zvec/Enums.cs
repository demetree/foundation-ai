namespace Foundation.AI.Zvec;

/// <summary>
/// Column data types for schema definitions.
/// Scalar types (Bool through Double) are used for filterable fields.
/// Vector types specify the storage precision for embedding vectors.
/// </summary>
public enum DataType
{
    Undefined = 0,
    Binary = 1,
    /// <summary>UTF-8 string. Supports equality and inequality filters.</summary>
    String = 2,
    /// <summary>Boolean. Supports equality filters.</summary>
    Bool = 3,
    /// <summary>Signed 32-bit integer. Supports all comparison operators.</summary>
    Int32 = 4,
    /// <summary>Signed 64-bit integer. Supports all comparison operators.</summary>
    Int64 = 5,
    /// <summary>Unsigned 32-bit integer.</summary>
    UInt32 = 6,
    /// <summary>Unsigned 64-bit integer.</summary>
    UInt64 = 7,
    /// <summary>32-bit floating point. Supports all comparison operators.</summary>
    Float = 8,
    /// <summary>64-bit floating point.</summary>
    Double = 9,
    VectorBinary32 = 20,
    VectorBinary64 = 21,
    VectorFP16 = 22,
    /// <summary>32-bit float vector — the standard embedding format. Most models output this.</summary>
    VectorFP32 = 23,
    VectorFP64 = 24,
    VectorInt4 = 25,
    VectorInt8 = 26,
    VectorInt16 = 27,
    SparseVectorFP16 = 30,
    SparseVectorFP32 = 31,
    ArrayBinary = 40,
    ArrayString = 41,
    ArrayBool = 42,
    ArrayInt32 = 43,
    ArrayInt64 = 44,
    ArrayUInt32 = 45,
    ArrayUInt64 = 46,
    ArrayFloat = 47,
    ArrayDouble = 48
}

/// <summary>
/// Distance metric types for vector similarity comparison.
/// Choose based on how your embedding model was trained.
/// </summary>
public enum MetricType
{
    Undefined = 0,
    /// <summary>Euclidean distance. Lower = more similar. Best for spatial/unnormalized vectors.</summary>
    L2 = 1,
    /// <summary>Inner Product (dot product). Higher = more similar. Best for normalized vectors.</summary>
    IP = 2,
    /// <summary>Cosine distance. Lower = more similar. Best for text embeddings (OpenAI, BERT).</summary>
    Cosine = 3,
    MipsL2 = 4
}

/// <summary>
/// Vector quantization types. Reduces memory at the cost of some accuracy.
/// </summary>
public enum QuantizeType
{
    /// <summary>No quantization — full FP32 precision.</summary>
    Undefined = 0,
    /// <summary>Half-precision — 2× compression, nearly lossless.</summary>
    FP16 = 1,
    /// <summary>8-bit integer — 4× compression, low error. Requires calibration.</summary>
    Int8 = 2,
    /// <summary>4-bit integer — 8× compression, moderate error. Best for coarse retrieval.</summary>
    Int4 = 3
}

/// <summary>
/// Vector index algorithm types.
/// </summary>
public enum IndexType
{
    Undefined = 0,
    /// <summary>HNSW graph index — best general-purpose ANN. O(log n) search.</summary>
    Hnsw = 1,
    /// <summary>IVF cluster index — partition-based ANN. Requires training.</summary>
    Ivf = 3,
    /// <summary>Flat brute-force — exact search. O(n) per query.</summary>
    Flat = 4,
    /// <summary>Inverted index for scalar field filtering.</summary>
    Invert = 10
}
