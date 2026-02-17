// Copyright 2025-present the zvec project — Pure C# Engine
// Index persistence — serializes/deserializes HNSW and IVF graphs to/from disk

using System.Text.Json;
using Foundation.AI.Zvec.Engine.Core;
using Foundation.AI.Zvec.Engine.Math;

namespace Foundation.AI.Zvec.Engine.Index;

/// <summary>
/// Persists vector index graphs to disk for fast recovery without rebuilding.
///
/// <para><b>Strategy:</b>
/// Indexes are the most expensive data structure to reconstruct (HNSW graph
/// construction is O(n log n)). By persisting index snapshots alongside the
/// document store, collection reopening is O(n) for deserialization rather
/// than O(n log n) for rebuild.</para>
///
/// <para><b>HNSW format:</b> Custom binary format for performance.
/// Stores the full graph topology (node vectors, layer connections, entry point)
/// in a compact sequential layout. Binary format is ~3× faster to read/write
/// than equivalent JSON for large graphs.</para>
///
/// <para><b>IVF format:</b> JSON via System.Text.Json source generators.
/// IVF indexes are typically smaller (just centroids + doc assignments),
/// so JSON's readability advantage outweighs the performance cost.
/// Includes quantization calibration state for persistence across sessions.</para>
///
/// <para><b>Versioning:</b>
/// Both formats include version numbers. The HNSW format is currently at v2
/// (added quantized vector support). Older v1 files load with null quantization.</para>
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
        writer.Write(2); // version 2 — adds quantization
        writer.Write(snapshot.Nodes.Count);
        writer.Write(snapshot.EntryPoint);
        writer.Write(snapshot.MaxLayer);

        // Quantization header
        writer.Write((byte)snapshot.Quantize);
        writer.Write(snapshot.Calibrated);
        writer.Write(snapshot.Dimension);
        writer.Write(snapshot.Int8Cal.MinVal);
        writer.Write(snapshot.Int8Cal.MaxVal);
        writer.Write(snapshot.Int4Cal.MinVal);
        writer.Write(snapshot.Int4Cal.MaxVal);

        // Nodes
        foreach (var node in snapshot.Nodes)
        {
            writer.Write(node.DocId);
            writer.Write(node.Level);
            writer.Write(node.IsDeleted);

            // Vector (nullable)
            if (node.Vector != null)
            {
                writer.Write(node.Vector.Length);
                foreach (float v in node.Vector)
                    writer.Write(v);
            }
            else
            {
                writer.Write(0); // zero-length = no FP32 vector
            }

            // Quantized vector
            if (node.QuantizedVector != null)
            {
                writer.Write(node.QuantizedVector.Length);
                writer.Write(node.QuantizedVector);
            }
            else
            {
                writer.Write(0);
            }

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
        if (version < 1 || version > 2)
            throw new InvalidDataException($"Unsupported HNSW index version: {version}");

        int nodeCount = reader.ReadInt32();
        int entryPoint = reader.ReadInt32();
        int maxLayer = reader.ReadInt32();

        // Quantization header (v2+)
        var quantize = QuantizeType.Undefined;
        bool calibrated = false;
        int dimension = 0;
        var int8Cal = new Quantization.Int8Calibration();
        var int4Cal = new Quantization.Int4Calibration();

        if (version >= 2)
        {
            quantize = (QuantizeType)reader.ReadByte();
            calibrated = reader.ReadBoolean();
            dimension = reader.ReadInt32();
            int8Cal = new Quantization.Int8Calibration
            {
                MinVal = reader.ReadSingle(),
                MaxVal = reader.ReadSingle()
            };
            int4Cal = new Quantization.Int4Calibration
            {
                MinVal = reader.ReadSingle(),
                MaxVal = reader.ReadSingle()
            };
        }

        // Read nodes
        var nodes = new List<HnswNodeSnapshot>(nodeCount);
        for (int i = 0; i < nodeCount; i++)
        {
            long docId = reader.ReadInt64();
            int level = reader.ReadInt32();
            bool isDeleted = reader.ReadBoolean();
            int vecLen = reader.ReadInt32();

            float[]? vector = vecLen > 0 ? new float[vecLen] : null;
            if (vector != null)
            {
                for (int v = 0; v < vecLen; v++)
                    vector[v] = reader.ReadSingle();
            }

            // Quantized vector (v2+)
            byte[]? qvec = null;
            if (version >= 2)
            {
                int qLen = reader.ReadInt32();
                if (qLen > 0)
                    qvec = reader.ReadBytes(qLen);
            }

            var connections = new List<int>[level + 1];
            for (int layer = 0; layer <= level; layer++)
            {
                int connCount = reader.ReadInt32();
                connections[layer] = new List<int>(connCount);
                for (int c = 0; c < connCount; c++)
                    connections[layer].Add(reader.ReadInt32());
            }

            nodes.Add(new HnswNodeSnapshot(docId, vector, level, isDeleted, connections, qvec));
        }

        index.LoadSnapshot(new HnswSnapshot(nodes, entryPoint, maxLayer,
            quantize, int8Cal, int4Cal, calibrated, dimension));
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
    public float[]? Vector { get; }
    public int Level { get; }
    public bool IsDeleted { get; }
    public byte[]? QuantizedVector { get; }
    private readonly List<int>[] _connections;

    public HnswNodeSnapshot(long docId, float[]? vector, int level, bool isDeleted,
                            List<int>[] connections, byte[]? quantizedVector = null)
    {
        DocId = docId;
        Vector = vector;
        Level = level;
        IsDeleted = isDeleted;
        _connections = connections;
        QuantizedVector = quantizedVector;
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
    public QuantizeType Quantize { get; }
    public Quantization.Int8Calibration Int8Cal { get; }
    public Quantization.Int4Calibration Int4Cal { get; }
    public bool Calibrated { get; }
    public int Dimension { get; }

    public HnswSnapshot(IReadOnlyList<HnswNodeSnapshot> nodes, int entryPoint, int maxLayer,
        QuantizeType quantize = QuantizeType.Undefined,
        Quantization.Int8Calibration int8Cal = default,
        Quantization.Int4Calibration int4Cal = default,
        bool calibrated = false, int dimension = 0)
    {
        Nodes = nodes;
        EntryPoint = entryPoint;
        MaxLayer = maxLayer;
        Quantize = quantize;
        Int8Cal = int8Cal;
        Int4Cal = int4Cal;
        Calibrated = calibrated;
        Dimension = dimension;
    }
}

/// <summary>Serializable snapshot of an IVF index.</summary>
public sealed class IvfSnapshot
{
    public float[][] Centroids { get; set; } = [];
    public List<IvfEntry>[] Lists { get; set; } = [];
    public bool Trained { get; set; }

    // Quantization state
    public float Int8CalMin { get; set; }
    public float Int8CalMax { get; set; }
    public float Int4CalMin { get; set; }
    public float Int4CalMax { get; set; }
    public bool QCalibrated { get; set; }
    public int Dimension { get; set; }
}

public sealed class IvfEntry
{
    public long DocId { get; set; }
    public float[] Vector { get; set; } = [];
    public byte[]? QVec { get; set; }
}

[System.Text.Json.Serialization.JsonSerializable(typeof(IvfSnapshot))]
internal partial class IvfSerializerContext : System.Text.Json.Serialization.JsonSerializerContext { }
