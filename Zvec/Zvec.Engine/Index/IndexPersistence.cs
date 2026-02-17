// Copyright 2025-present the zvec project — Pure C# Engine
// Index persistence — serializes/deserializes HNSW and IVF graphs to/from disk

using System.Text.Json;

namespace Foundation.AI.Zvec.Engine.Index;

/// <summary>
/// Persists vector index graphs to disk so they can be loaded
/// without rebuilding from stored documents.
/// </summary>
public static class IndexPersistence
{
    private const string HnswFile = "hnsw_index.bin";
    private const string IvfFile = "ivf_index.json";

    // ═══════════════════════════════════════════════════════════════════
    // HNSW — Binary format for performance
    // ═══════════════════════════════════════════════════════════════════
    //
    // Format:
    //   [4 bytes] magic: "HNSW"
    //   [4 bytes] version: 1
    //   [4 bytes] nodeCount
    //   [4 bytes] entryPoint
    //   [4 bytes] maxLayerReached
    //   For each node:
    //     [8 bytes] docId
    //     [4 bytes] level
    //     [1 byte]  isDeleted
    //     [4 bytes] vectorLength
    //     [vectorLength * 4 bytes] vector floats
    //     For each layer (0..level):
    //       [4 bytes] connectionCount
    //       [connectionCount * 4 bytes] neighbor indices

    public static void SaveHnsw(string indexPath, HnswIndex index)
    {
        var path = Path.Combine(indexPath, HnswFile);
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);

        var snapshot = index.GetSnapshot();

        // Header
        writer.Write((byte)'H'); writer.Write((byte)'N');
        writer.Write((byte)'S'); writer.Write((byte)'W');
        writer.Write(1); // version
        writer.Write(snapshot.Nodes.Count);
        writer.Write(snapshot.EntryPoint);
        writer.Write(snapshot.MaxLayer);

        // Nodes
        foreach (var node in snapshot.Nodes)
        {
            writer.Write(node.DocId);
            writer.Write(node.Level);
            writer.Write(node.IsDeleted);
            writer.Write(node.Vector.Length);

            foreach (float v in node.Vector)
                writer.Write(v);

            // Connections for each layer
            for (int layer = 0; layer <= node.Level; layer++)
            {
                var conns = node.GetConnections(layer);
                writer.Write(conns.Count);
                foreach (int c in conns)
                    writer.Write(c);
            }
        }
    }

    public static void LoadHnsw(string indexPath, HnswIndex index)
    {
        var path = Path.Combine(indexPath, HnswFile);
        if (!File.Exists(path)) return;

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        // Verify magic
        byte h = reader.ReadByte(), n = reader.ReadByte(),
             s = reader.ReadByte(), w = reader.ReadByte();
        if (h != 'H' || n != 'N' || s != 'S' || w != 'W')
            throw new InvalidDataException("Invalid HNSW index file");

        int version = reader.ReadInt32();
        if (version != 1)
            throw new InvalidDataException($"Unsupported HNSW index version: {version}");

        int nodeCount = reader.ReadInt32();
        int entryPoint = reader.ReadInt32();
        int maxLayer = reader.ReadInt32();

        // Read nodes
        var nodes = new List<HnswNodeSnapshot>(nodeCount);
        for (int i = 0; i < nodeCount; i++)
        {
            long docId = reader.ReadInt64();
            int level = reader.ReadInt32();
            bool isDeleted = reader.ReadBoolean();
            int vecLen = reader.ReadInt32();

            var vector = new float[vecLen];
            for (int v = 0; v < vecLen; v++)
                vector[v] = reader.ReadSingle();

            var connections = new List<int>[level + 1];
            for (int layer = 0; layer <= level; layer++)
            {
                int connCount = reader.ReadInt32();
                connections[layer] = new List<int>(connCount);
                for (int c = 0; c < connCount; c++)
                    connections[layer].Add(reader.ReadInt32());
            }

            nodes.Add(new HnswNodeSnapshot(docId, vector, level, isDeleted, connections));
        }

        index.LoadSnapshot(new HnswSnapshot(nodes, entryPoint, maxLayer));
    }

    public static bool HasHnswIndex(string indexPath)
        => File.Exists(Path.Combine(indexPath, HnswFile));

    // ═══════════════════════════════════════════════════════════════════
    // IVF — JSON format (centroids + assignments)
    // ═══════════════════════════════════════════════════════════════════

    public static void SaveIvf(string indexPath, IvfIndex index)
    {
        var path = Path.Combine(indexPath, IvfFile);
        var snapshot = index.GetSnapshot();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(snapshot, IvfSerializerContext.Default.IvfSnapshot);
        File.WriteAllBytes(path, bytes);
    }

    public static void LoadIvf(string indexPath, IvfIndex index)
    {
        var path = Path.Combine(indexPath, IvfFile);
        if (!File.Exists(path)) return;

        var bytes = File.ReadAllBytes(path);
        var snapshot = JsonSerializer.Deserialize(bytes, IvfSerializerContext.Default.IvfSnapshot);
        if (snapshot != null)
            index.LoadSnapshot(snapshot);
    }

    public static bool HasIvfIndex(string indexPath)
        => File.Exists(Path.Combine(indexPath, IvfFile));
}

// ── Snapshot types ──────────────────────────────────────────────────────

/// <summary>Serializable snapshot of an HNSW node.</summary>
public sealed class HnswNodeSnapshot
{
    public long DocId { get; }
    public float[] Vector { get; }
    public int Level { get; }
    public bool IsDeleted { get; }
    private readonly List<int>[] _connections;

    public HnswNodeSnapshot(long docId, float[] vector, int level, bool isDeleted, List<int>[] connections)
    {
        DocId = docId;
        Vector = vector;
        Level = level;
        IsDeleted = isDeleted;
        _connections = connections;
    }

    public List<int> GetConnections(int layer) =>
        layer < _connections.Length ? _connections[layer] : [];
}

/// <summary>Complete serializable snapshot of an HNSW index.</summary>
public sealed class HnswSnapshot
{
    public IReadOnlyList<HnswNodeSnapshot> Nodes { get; }
    public int EntryPoint { get; }
    public int MaxLayer { get; }

    public HnswSnapshot(IReadOnlyList<HnswNodeSnapshot> nodes, int entryPoint, int maxLayer)
    {
        Nodes = nodes;
        EntryPoint = entryPoint;
        MaxLayer = maxLayer;
    }
}

/// <summary>Serializable snapshot of an IVF index.</summary>
public sealed class IvfSnapshot
{
    public float[][] Centroids { get; set; } = [];
    public List<IvfEntry>[] Lists { get; set; } = [];
    public bool Trained { get; set; }
}

public sealed class IvfEntry
{
    public long DocId { get; set; }
    public float[] Vector { get; set; } = [];
}

[System.Text.Json.Serialization.JsonSerializable(typeof(IvfSnapshot))]
internal partial class IvfSerializerContext : System.Text.Json.Serialization.JsonSerializerContext { }
