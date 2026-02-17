// Copyright 2025-present the zvec project — Pure C# Engine
// ZoneTree-backed persistent storage engine

using System.Text.Json;
using Tenray.ZoneTree;
using Tenray.ZoneTree.Serializers;
using Foundation.AI.Zvec.Engine.Core;

namespace Foundation.AI.Zvec.Engine.Storage;

/// <summary>
/// Custom serializer for byte[] values in ZoneTree.
/// </summary>
internal sealed class BlobSerializer : ISerializer<byte[]>
{
    public byte[] Deserialize(Memory<byte> bytes) => bytes.ToArray();
    public Memory<byte> Serialize(in byte[] entry) => entry.AsMemory();
}

/// <summary>
/// Persistent storage engine backed by ZoneTree.
/// Provides durable document storage with ACID guarantees.
/// </summary>
public sealed class ZoneTreeStorageEngine : IStorageEngine
{
    private readonly IZoneTree<long, byte[]> _docTree;
    private readonly IZoneTree<string, long> _pkTree;
    private readonly IMaintainer _docMaintainer;
    private readonly IMaintainer _pkMaintainer;
    private long _docCount;  // O(1) document count

    public ZoneTreeStorageEngine(string dataPath)
    {
        var docPath = Path.Combine(dataPath, "docs");
        var pkPath = Path.Combine(dataPath, "pk_index");

        Directory.CreateDirectory(docPath);
        Directory.CreateDirectory(pkPath);

        _docTree = new ZoneTreeFactory<long, byte[]>()
            .SetDataDirectory(docPath)
            .SetKeySerializer(new Int64Serializer())
            .SetValueSerializer(new BlobSerializer())
            .OpenOrCreate();

        _pkTree = new ZoneTreeFactory<string, long>()
            .SetDataDirectory(pkPath)
            .SetKeySerializer(new Utf8StringSerializer())
            .SetValueSerializer(new Int64Serializer())
            .OpenOrCreate();

        _docMaintainer = _docTree.CreateMaintainer();
        _pkMaintainer = _pkTree.CreateMaintainer();

        // Initialize count by scanning once at startup
        _docCount = CountKeys();
    }

    private long CountKeys()
    {
        long count = 0;
        using var iter = _docTree.CreateIterator();
        iter.SeekFirst();
        while (iter.Next())
            count++;
        return count;
    }

    public void Put(long docId, Document doc)
    {
        bool isNew = !_docTree.ContainsKey(docId);
        var bytes = SerializeDocument(doc);
        _docTree.Upsert(docId, bytes);
        if (isNew) Interlocked.Increment(ref _docCount);
    }

    public Document? Get(long docId)
    {
        if (_docTree.TryGet(docId, out var bytes))
            return DeserializeDocument(bytes);
        return null;
    }

    public void Delete(long docId)
    {
        _docTree.ForceDelete(docId);
        Interlocked.Decrement(ref _docCount);
    }

    public void MapPrimaryKey(string pk, long docId)
    {
        _pkTree.Upsert(pk, docId);
    }

    public long? LookupPrimaryKey(string pk)
    {
        if (_pkTree.TryGet(pk, out long docId))
            return docId;
        return null;
    }

    public void RemovePrimaryKey(string pk)
    {
        _pkTree.ForceDelete(pk);
    }

    public void Flush()
    {
        _docMaintainer.EvictToDisk();
        _pkMaintainer.EvictToDisk();
    }

    public long DocumentCount => Interlocked.Read(ref _docCount);

    public IEnumerable<long> AllDocIds()
    {
        var ids = new List<long>();
        using var iter = _docTree.CreateIterator();
        iter.SeekFirst();
        while (iter.Next())
            ids.Add(iter.CurrentKey);
        return ids;
    }

    public void Dispose()
    {
        _docMaintainer.Dispose();
        _pkMaintainer.Dispose();
        _docTree.Dispose();
        _pkTree.Dispose();
    }

    // ── Serialization ───────────────────────────────────────────────

    private static byte[] SerializeDocument(Document doc)
    {
        var dto = new DocumentDto
        {
            PrimaryKey = doc.PrimaryKey,
            DocId = doc.DocId,
            Fields = doc.Fields,
            Vectors = doc.Vectors
        };
        return JsonSerializer.SerializeToUtf8Bytes(dto, DtoContext.Default.DocumentDto);
    }

    private static Document DeserializeDocument(byte[] bytes)
    {
        var dto = JsonSerializer.Deserialize(bytes, DtoContext.Default.DocumentDto)!;
        var doc = new Document
        {
            PrimaryKey = dto.PrimaryKey,
            DocId = dto.DocId
        };
        if (dto.Fields != null)
            foreach (var (k, v) in dto.Fields)
                doc.Fields[k] = ConvertJsonElement(v);
        if (dto.Vectors != null)
            foreach (var (k, v) in dto.Vectors)
                doc.Vectors[k] = v;
        return doc;
    }

    /// <summary>
    /// Convert JsonElement values back to CLR types after deserialization.
    /// </summary>
    private static object? ConvertJsonElement(object? value)
    {
        if (value is JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.String => je.GetString(),
                JsonValueKind.Number when je.TryGetInt32(out int i) => i,
                JsonValueKind.Number when je.TryGetInt64(out long l) => l,
                JsonValueKind.Number => je.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => je.ToString()
            };
        }
        return value;
    }
}

// ── Serialization DTOs ──────────────────────────────────────────────

internal sealed class DocumentDto
{
    public string PrimaryKey { get; set; } = "";
    public long DocId { get; set; }
    public Dictionary<string, object?>? Fields { get; set; }
    public Dictionary<string, float[]>? Vectors { get; set; }
}

/// <summary>
/// Source-generated JSON serializer context for AOT-compatible serialization.
/// </summary>
[System.Text.Json.Serialization.JsonSerializable(typeof(DocumentDto))]
[System.Text.Json.Serialization.JsonSerializable(typeof(CollectionMetadata))]
internal partial class DtoContext : System.Text.Json.Serialization.JsonSerializerContext { }

// ── Collection metadata ─────────────────────────────────────────────

/// <summary>
/// Persisted metadata for reopening a collection.
/// </summary>
public sealed class CollectionMetadata
{
    public string Name { get; set; } = "";
    public long NextDocId { get; set; }
    public List<FieldMetadata> Fields { get; set; } = [];
}

public sealed class FieldMetadata
{
    public string Name { get; set; } = "";
    public string DataType { get; set; } = "";
    public bool Nullable { get; set; }
    public uint Dimension { get; set; }
    public bool IsVector { get; set; }
    public string? IndexType { get; set; }
    public string? Metric { get; set; }
    public int M { get; set; }
    public int EfConstruction { get; set; }
}

/// <summary>
/// Helpers for persisting and loading collection metadata.
/// </summary>
public static class MetadataPersistence
{
    private const string MetadataFile = "collection_meta.json";

    public static void Save(string collectionPath, CollectionMetadata metadata)
    {
        var path = Path.Combine(collectionPath, MetadataFile);
        var json = JsonSerializer.SerializeToUtf8Bytes(metadata, DtoContext.Default.CollectionMetadata);
        File.WriteAllBytes(path, json);
    }

    public static CollectionMetadata? Load(string collectionPath)
    {
        var path = Path.Combine(collectionPath, MetadataFile);
        if (!File.Exists(path)) return null;
        var bytes = File.ReadAllBytes(path);
        return JsonSerializer.Deserialize(bytes, DtoContext.Default.CollectionMetadata);
    }

    public static CollectionMetadata FromSchema(CollectionSchemaDefinition schema, long nextDocId)
    {
        var meta = new CollectionMetadata
        {
            Name = schema.Name,
            NextDocId = nextDocId
        };

        foreach (var field in schema.Fields)
        {
            meta.Fields.Add(new FieldMetadata
            {
                Name = field.Name,
                DataType = field.DataType.ToString(),
                Nullable = field.Nullable,
                Dimension = field.Dimension,
                IsVector = field.IsVectorField,
                IndexType = field.IndexConfig?.Type.ToString(),
                Metric = field.IndexConfig?.Metric.ToString(),
                M = field.IndexConfig?.M ?? 0,
                EfConstruction = field.IndexConfig?.EfConstruction ?? 0
            });
        }

        return meta;
    }

    public static CollectionSchemaDefinition ToSchema(CollectionMetadata metadata)
    {
        var fields = new List<FieldSchema>();
        foreach (var fm in metadata.Fields)
        {
            var dataType = Enum.TryParse<FieldDataType>(fm.DataType, out var dt) ? dt : FieldDataType.Undefined;
            IndexConfig? indexConfig = null;

            if (fm.IndexType != null && Enum.TryParse<Core.IndexType>(fm.IndexType, out var it))
            {
                var metric = Enum.TryParse<Math.MetricType>(fm.Metric, out var m)
                    ? m : Math.MetricType.InnerProduct;
                indexConfig = new IndexConfig
                {
                    Type = it,
                    Metric = metric,
                    M = fm.M,
                    EfConstruction = fm.EfConstruction
                };
            }

            fields.Add(new FieldSchema
            {
                Name = fm.Name,
                DataType = dataType,
                Nullable = fm.Nullable,
                Dimension = fm.Dimension,
                IndexConfig = indexConfig
            });
        }

        return new CollectionSchemaDefinition
        {
            Name = metadata.Name,
            Fields = fields
        };
    }
}
