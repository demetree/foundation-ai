using Foundation.AI.Zvec.Engine.Core;

namespace Foundation.AI.Zvec;

/// <summary>
/// Defines a collection's structure. Supports a fluent builder pattern.
/// Now backed by the managed engine schema builder — no native handles.
/// </summary>
public sealed class CollectionSchema : IDisposable
{
    private readonly SchemaBuilder _builder;
    private bool _disposed;

    public CollectionSchema(string name)
    {
        _builder = new SchemaBuilder(name);
    }

    /// <summary>
    /// Add a scalar field (no dimension, no index).
    /// </summary>
    public CollectionSchema AddField(string name, DataType dataType, bool nullable = false)
    {
        _builder.AddField(name, MapDataType(dataType), nullable);
        return this;
    }

    /// <summary>
    /// Add a scalar field with an index.
    /// </summary>
    public CollectionSchema AddField(string name, DataType dataType, bool nullable, IndexParams indexParams)
    {
        _builder.AddField(name, MapDataType(dataType), nullable, indexParams.ToEngineConfig());
        return this;
    }

    /// <summary>
    /// Add a vector field with dimension and index.
    /// </summary>
    public CollectionSchema AddVector(string name, DataType dataType, uint dimension,
                                      IndexParams indexParams, bool nullable = false)
    {
        _builder.AddVector(name, MapDataType(dataType), dimension, indexParams.ToEngineConfig(), nullable);
        return this;
    }

    /// <summary>
    /// Set the maximum number of documents per segment.
    /// </summary>
    public CollectionSchema SetMaxDocCountPerSegment(ulong count)
    {
        _builder.SetMaxDocCountPerSegment(count);
        return this;
    }

    /// <summary>
    /// Build the internal engine schema definition.
    /// </summary>
    internal CollectionSchemaDefinition BuildSchema() => _builder.Build();

    /// <summary>
    /// Map public DataType enum to internal engine FieldDataType.
    /// </summary>
    private static FieldDataType MapDataType(DataType dt) => dt switch
    {
        DataType.Binary => FieldDataType.Binary,
        DataType.String => FieldDataType.String,
        DataType.Bool => FieldDataType.Bool,
        DataType.Int32 => FieldDataType.Int32,
        DataType.Int64 => FieldDataType.Int64,
        DataType.UInt32 => FieldDataType.UInt32,
        DataType.UInt64 => FieldDataType.UInt64,
        DataType.Float => FieldDataType.Float,
        DataType.Double => FieldDataType.Double,
        DataType.VectorFP32 => FieldDataType.VectorFP32,
        DataType.VectorFP64 => FieldDataType.VectorFP64,
        DataType.VectorInt8 => FieldDataType.VectorInt8,
        _ => FieldDataType.Undefined
    };

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~CollectionSchema() => Dispose();
}
