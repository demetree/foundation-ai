// Copyright 2025-present the zvec project — Pure C# Engine

namespace Foundation.AI.Zvec.Engine.Core;

/// <summary>
/// Data types for collection fields.
/// </summary>
public enum FieldDataType
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
    VectorFP32 = 23,
    VectorFP64 = 24,
    VectorInt8 = 26,
}

/// <summary>
/// Index algorithm types.
/// </summary>
public enum IndexType
{
    None = 0,
    Hnsw = 1,
    Ivf = 3,
    Flat = 4,
    Invert = 10,
}

/// <summary>
/// Quantization types for vector compression.
/// </summary>
public enum QuantizeType
{
    Undefined = 0,
    FP16 = 1,
    Int8 = 2,
    Int4 = 3,
}

/// <summary>
/// Defines a single field in a collection schema.
/// </summary>
public sealed class FieldSchema
{
    public required string Name { get; init; }
    public required FieldDataType DataType { get; init; }
    public bool Nullable { get; init; }

    /// <summary>Vector dimension (only for vector fields).</summary>
    public uint Dimension { get; init; }

    /// <summary>Index configuration (null = no index).</summary>
    public IndexConfig? IndexConfig { get; init; }

    public bool IsVectorField => DataType is FieldDataType.VectorFP32
                                 or FieldDataType.VectorFP64
                                 or FieldDataType.VectorInt8;
}

/// <summary>
/// Configuration for a field index.
/// </summary>
public sealed class IndexConfig
{
    public required IndexType Type { get; init; }
    public Math.MetricType Metric { get; init; } = Math.MetricType.InnerProduct;
    public QuantizeType Quantize { get; init; } = QuantizeType.Undefined;

    // HNSW-specific
    public int M { get; init; }
    public int EfConstruction { get; init; } = 200;

    // IVF-specific
    public int Nlist { get; init; }
    public int Nprobe { get; init; } = 8;
    public int NIters { get; init; }

    // Scalar invert-specific
    public bool EnableRangeOptimization { get; init; } = true;
}

/// <summary>
/// Defines the structure of a collection. Immutable once built.
/// </summary>
public sealed class CollectionSchemaDefinition
{
    public required string Name { get; init; }
    public required IReadOnlyList<FieldSchema> Fields { get; init; }
    public ulong MaxDocCountPerSegment { get; init; } = 1_000_000;

    /// <summary>Get a field by name.</summary>
    public FieldSchema? GetField(string name) =>
        Fields.FirstOrDefault(f => f.Name == name);

    /// <summary>Get all vector fields.</summary>
    public IEnumerable<FieldSchema> VectorFields =>
        Fields.Where(f => f.IsVectorField);

    /// <summary>Get all scalar (non-vector) fields.</summary>
    public IEnumerable<FieldSchema> ScalarFields =>
        Fields.Where(f => !f.IsVectorField);
}

/// <summary>
/// Builder for constructing a CollectionSchemaDefinition fluently.
/// </summary>
public sealed class SchemaBuilder
{
    private readonly string _name;
    private readonly List<FieldSchema> _fields = [];
    private ulong _maxDocCountPerSegment = 1_000_000;

    public SchemaBuilder(string name) => _name = name;

    public SchemaBuilder AddField(string name, FieldDataType dataType,
                                  bool nullable = false, IndexConfig? index = null)
    {
        _fields.Add(new FieldSchema
        {
            Name = name,
            DataType = dataType,
            Nullable = nullable,
            IndexConfig = index
        });
        return this;
    }

    public SchemaBuilder AddVector(string name, FieldDataType dataType,
                                   uint dimension, IndexConfig? index = null,
                                   bool nullable = false)
    {
        _fields.Add(new FieldSchema
        {
            Name = name,
            DataType = dataType,
            Nullable = nullable,
            Dimension = dimension,
            IndexConfig = index
        });
        return this;
    }

    public SchemaBuilder SetMaxDocCountPerSegment(ulong count)
    {
        _maxDocCountPerSegment = count;
        return this;
    }

    public CollectionSchemaDefinition Build() => new()
    {
        Name = _name,
        Fields = _fields.ToArray(),
        MaxDocCountPerSegment = _maxDocCountPerSegment
    };
}
