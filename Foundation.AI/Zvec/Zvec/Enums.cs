namespace Foundation.AI.Zvec;

/// <summary>
/// Column data types. Values match zvec::DataType exactly.
/// </summary>
public enum DataType
{
    Undefined = 0,
    Binary = 1,
    String = 2,
    Bool = 3,
    Int32 = 4,
    Int64 = 5,
    UInt32 = 6,
    UInt64 = 7,
    Float = 8,
    Double = 9,
    VectorBinary32 = 20,
    VectorBinary64 = 21,
    VectorFP16 = 22,
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
/// Distance metric types. Values match zvec::MetricType exactly.
/// </summary>
public enum MetricType
{
    Undefined = 0,
    L2 = 1,
    IP = 2,
    Cosine = 3,
    MipsL2 = 4
}

/// <summary>
/// Quantization types. Values match zvec::QuantizeType exactly.
/// </summary>
public enum QuantizeType
{
    Undefined = 0,
    FP16 = 1,
    Int8 = 2,
    Int4 = 3
}

/// <summary>
/// Index types. Values match zvec::IndexType exactly.
/// </summary>
public enum IndexType
{
    Undefined = 0,
    Hnsw = 1,
    Ivf = 3,
    Flat = 4,
    Invert = 10
}
