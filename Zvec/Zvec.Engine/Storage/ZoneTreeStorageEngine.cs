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
/// Persistent storage engine backed by ZoneTree, a .NET LSM-tree implementation.
///
/// <para><b>Why ZoneTree / LSM-tree?</b>
/// LSM-trees are write-optimized — inserts append to an in-memory buffer (memtable)
/// and WAL, then flush to sorted immutable segments on disk. This gives O(1)
/// amortized writes vs B-tree's O(log n). Reads are slightly slower (must check
/// memtable + disk segments) but ZoneTree uses bloom filters and sparse indexes
/// to mitigate this.</para>
///
/// <para><b>Dual-tree architecture:</b>
/// <c>_docTree</c> (long → byte[]): Maps docId to JSON-serialized document bytes.
/// <c>_pkTree</c> (string → long): Maps primary key to docId for O(1) PK lookups.
/// Both trees are independently maintained with their own WAL and compaction.</para>
///
/// <para><b>Document encoding:</b>
/// Documents are serialized to JSON byte arrays using System.Text.Json source
/// generators. The key is a 64-bit document ID (monotonically increasing).
/// Deleted documents are marked but not physically removed until compaction.</para>
///
/// <para><b>ACID guarantees:</b>
/// ZoneTree provides crash-safety via WAL recovery. Ongoing writes survive
/// process crashes; the WAL is replayed on next open to restore consistent state.</para>
/// </summary>
public sealed class ZoneTreeStorageEngine : IStorageEngine
{
    private readonly IZoneTree<long, byte[]> _docTree;
    private readonly IZoneTree<string, long> _pkTree;
    private readonly IMaintainer _docMaintainer;
    private readonly IMaintainer _pkMaintainer;
    private long _docCount;  // O(1) document count

    public ZoneTreeStorageEngine(string dataPath, bool allowDestructiveRecovery = false)
    {
        var docPath = Path.Combine(dataPath, "docs");
        var pkPath = Path.Combine(dataPath, "pk_index");

        //
        // Deferred-quarantine drain — MUST run before Directory.CreateDirectory
        // and before OpenWithRecovery. If a previous process died with
        // stuck mmap handles (see QuarantineSentinel.cs for the full story),
        // it left behind a {docPath}.pending-quarantine.json (and/or the
        // pk_index equivalent) describing a rename it couldn't complete.
        // This is a fresh process, so those handles no longer exist — we
        // can execute the queued rename cleanly right now. Only after
        // that's done does it make sense to ensure the sub-dir exists
        // and try to open it.
        //
        // If Drain itself throws (e.g. sentinel is unparseable, or an
        // external process is still holding files), the exception
        // propagates and the store fails to open — which is the correct
        // outcome: we'd rather surface the problem than silently open an
        // empty store while leaving corrupt data next to it.
        //
        if (allowDestructiveRecovery)
        {
            QuarantineSentinel.Drain(docPath);
            QuarantineSentinel.Drain(pkPath);
        }

        Directory.CreateDirectory(docPath);
        Directory.CreateDirectory(pkPath);

        _docTree = OpenWithRecovery(docPath, allowDestructiveRecovery, path =>
            new ZoneTreeFactory<long, byte[]>()
                .SetDataDirectory(path)
                .SetKeySerializer(new Int64Serializer())
                .SetValueSerializer(new BlobSerializer())
                .OpenOrCreate());

        _pkTree = OpenWithRecovery(pkPath, allowDestructiveRecovery, path =>
            new ZoneTreeFactory<string, long>()
                .SetDataDirectory(path)
                .SetKeySerializer(new Utf8StringSerializer())
                .SetValueSerializer(new Int64Serializer())
                .OpenOrCreate());

        _docMaintainer = _docTree.CreateMaintainer();
        _pkMaintainer = _pkTree.CreateMaintainer();

        // Initialize count by scanning once at startup
        _docCount = CountKeys();
    }

    /// <summary>
    /// Invokes <paramref name="open"/> to materialise a ZoneTree. If opening
    /// fails because on-disk state is inconsistent (missing segment files,
    /// truncated WAL, etc.), either quarantines the directory and retries on
    /// a fresh slate — when <paramref name="allowDestructiveRecovery"/> is
    /// true — or rethrows as <see cref="CorruptedStoreException"/> with a
    /// clear message. The quarantined directory is renamed to
    /// <c>{path}.corrupt-{utc-timestamp}</c> so the bad state is preserved
    /// for post-mortem instead of being silently deleted.
    /// </summary>
    private static IZoneTree<TKey, TValue> OpenWithRecovery<TKey, TValue>(
        string path,
        bool allowDestructiveRecovery,
        Func<string, IZoneTree<TKey, TValue>> open)
    {
        try
        {
            return open(path);
        }
        catch (Exception ex) when (IsLikelyCorruption(ex))
        {
            if (!allowDestructiveRecovery)
            {
                throw new CorruptedStoreException(
                    path,
                    $"ZoneTree store at '{path}' appears corrupted (likely a crash or " +
                    $"debugger-stop mid-compaction left the manifest pointing at segment " +
                    $"files that were never persisted). Pass AllowDestructiveRecovery=true " +
                    $"to quarantine and recreate, or manually move the directory aside.",
                    ex);
            }

            string quarantined;
            try
            {
                quarantined = QuarantineDirectory(path);
            }
            catch (Exception quarantineEx)
            {
                //
                // Quarantine failed — almost always because the failing
                // OpenOrCreate above left mmap handles on files inside the
                // directory, and Windows refuses the rename until those
                // handles drop. Those handles are owned by *this* process
                // and won't release until GC finalizes them, which GC.Collect
                // + WaitForPendingFinalizers (already tried inside
                // QuarantineDirectory) can't always force for mmap'd files.
                //
                // So we fall back to the deferred-retry pattern: drop a
                // sentinel JSON next to the source describing the rename we
                // wanted, and let the *next* process startup (no stuck
                // handles there) execute it during ZoneTreeStorageEngine's
                // ctor. See QuarantineSentinel.cs for the full design note.
                //
                // The current startup still fails — we can't usefully
                // continue against a corrupt store whose files we couldn't
                // move aside — but the operator gets a clear message saying
                // "just restart and it'll self-heal", and the second
                // startup actually works.
                //
                var deferredTarget = ComputeQuarantineTarget(path);
                QuarantineSentinel.Write(path, deferredTarget, ex.GetType().Name + ": " + ex.Message);

                throw new CorruptedStoreException(
                    path,
                    $"ZoneTree store at '{path}' is corrupted. Destructive recovery is " +
                    $"enabled but the quarantine rename failed in this process — typically " +
                    $"because the failed OpenOrCreate attempt above left memory-mapped " +
                    $"handles on the corrupt files that won't release until the process " +
                    $"ends. A deferred-quarantine sentinel has been written to " +
                    $"'{QuarantineSentinel.SentinelPathFor(path)}' — restart the process " +
                    $"and startup will complete the rename automatically. If the problem " +
                    $"persists after restart, an external process (Visual Studio, Explorer, " +
                    $"antivirus) is holding the files; close it and retry.",
                    quarantineEx);
            }

            Directory.CreateDirectory(path);
            System.Diagnostics.Trace.TraceWarning(
                $"Zvec: Quarantined corrupted store '{path}' -> '{quarantined}' and recreated empty. " +
                $"Original error: {ex.Message}");

            try
            {
                return open(path);
            }
            catch (Exception retryEx)
            {
                throw new CorruptedStoreException(
                    path, quarantined,
                    $"Failed to open a fresh ZoneTree store at '{path}' even after " +
                    $"quarantining the previous contents to '{quarantined}'.",
                    retryEx);
            }
        }
    }

    /// <summary>
    /// Heuristic for "is this exception a sign of on-disk corruption that we
    /// can recover from by wiping, as opposed to a programmer error we'd
    /// rather surface loudly?" We treat missing files, truncated/unreadable
    /// files, and WAL/segment-checksum failures as recoverable. Everything
    /// else bubbles up unchanged.
    /// </summary>
    private static bool IsLikelyCorruption(Exception ex)
    {
        // Walk the whole chain — ZoneTree often wraps the root cause.
        for (var e = ex; e != null; e = e.InnerException)
        {
            if (e is FileNotFoundException
                or DirectoryNotFoundException
                or EndOfStreamException
                or System.IO.InvalidDataException)
                return true;

            if (e is IOException)
                return true;

            // ZoneTree-specific names vary by version; match on type-name to
            // avoid a hard reference and to tolerate minor version drift.
            var name = e.GetType().FullName ?? "";
            if (name.StartsWith("Tenray.ZoneTree.Exceptions.", StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Pure path computation — what the quarantine target should be named,
    /// given the source path. Factored out so that the in-process rename
    /// path and the deferred-sentinel fallback path agree on the exact same
    /// naming scheme: <c>{name}.corrupt-{yyyyMMddTHHmmssfff}</c> (UTC).
    /// The timestamp granularity to milliseconds keeps repeated failures
    /// in the same second distinguishable. Collision disambiguation (the
    /// <c>-{n}</c> suffix) happens at rename time, not here, so the name
    /// captured in a sentinel is a stable prediction of where the files
    /// will land — any collision suffix on the eventual rename doesn't
    /// invalidate the sentinel.
    /// </summary>
    private static string ComputeQuarantineTarget(string sourcePath)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff");
        var parent = Path.GetDirectoryName(sourcePath)!;
        var name = Path.GetFileName(sourcePath);
        return Path.Combine(parent, $"{name}.corrupt-{timestamp}");
    }

    private static string QuarantineDirectory(string path)
    {
        var quarantined = ComputeQuarantineTarget(path);

        // Defensive: if the target somehow exists, append a suffix.
        int n = 1;
        var target = quarantined;
        while (Directory.Exists(target) || File.Exists(target))
            target = $"{quarantined}-{n++}";

        //
        // A failed OpenOrCreate typically mmap'd a handful of segment/WAL
        // files before throwing on the missing-segment file, and on Windows
        // those memory-mapped-file handles keep Directory.Move blocked with
        // "access denied" until the finalizers release them. Force a GC +
        // finalizer drain, then retry the rename a few times with short
        // backoff — this is enough to clear *some* orphaned-handle cases
        // without waiting on the natural GC cycle.
        //
        // If all retries fail, the caller falls back to the deferred-
        // quarantine sentinel path (see OpenWithRecovery) — the next
        // process startup runs the rename against unlocked files.
        //
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        const int maxAttempts = 5;
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                Directory.Move(path, target);
                return target;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(200);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(200);
            }
        }
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
        {
            try
            {
                return DeserializeDocument(bytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning(
                    $"Zvec: Failed to deserialize document {docId} ({bytes?.Length ?? 0} bytes), skipping. {ex.Message}");
                return null;
            }
        }
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

    private static Document? DeserializeDocument(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

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
