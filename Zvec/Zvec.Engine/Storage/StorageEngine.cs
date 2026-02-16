// Copyright 2025-present the zvec project — Pure C# Engine
// Storage engine abstraction and in-memory implementation

using Foundation.AI.Zvec.Engine.Core;

namespace Foundation.AI.Zvec.Engine.Storage;

/// <summary>
/// Abstraction over the persistent storage backend.
/// </summary>
public interface IStorageEngine : IDisposable
{
    /// <summary>Store a document by its internal doc ID.</summary>
    void Put(long docId, Document doc);

    /// <summary>Retrieve a document by its internal doc ID.</summary>
    Document? Get(long docId);

    /// <summary>Delete a document by its internal doc ID.</summary>
    void Delete(long docId);

    /// <summary>Map a primary key to an internal doc ID.</summary>
    void MapPrimaryKey(string pk, long docId);

    /// <summary>Lookup the doc ID for a primary key.</summary>
    long? LookupPrimaryKey(string pk);

    /// <summary>Remove a primary key mapping.</summary>
    void RemovePrimaryKey(string pk);

    /// <summary>Flush all pending writes to durable storage.</summary>
    void Flush();

    /// <summary>Get the count of stored documents.</summary>
    long DocumentCount { get; }

    /// <summary>Get all doc IDs.</summary>
    IEnumerable<long> AllDocIds();
}

/// <summary>
/// In-memory storage engine. Serves as the initial implementation and as the
/// writing segment buffer before data is persisted.
/// </summary>
public sealed class InMemoryStorageEngine : IStorageEngine
{
    private readonly Dictionary<long, Document> _docs = new();
    private readonly Dictionary<string, long> _pkIndex = new();
    private readonly object _lock = new();

    public long DocumentCount
    {
        get { lock (_lock) return _docs.Count; }
    }

    public void Put(long docId, Document doc)
    {
        lock (_lock)
        {
            _docs[docId] = doc;
        }
    }

    public Document? Get(long docId)
    {
        lock (_lock)
        {
            return _docs.TryGetValue(docId, out var doc) ? doc : null;
        }
    }

    public void Delete(long docId)
    {
        lock (_lock)
        {
            _docs.Remove(docId);
        }
    }

    public void MapPrimaryKey(string pk, long docId)
    {
        lock (_lock)
        {
            _pkIndex[pk] = docId;
        }
    }

    public long? LookupPrimaryKey(string pk)
    {
        lock (_lock)
        {
            return _pkIndex.TryGetValue(pk, out var id) ? id : null;
        }
    }

    public void RemovePrimaryKey(string pk)
    {
        lock (_lock)
        {
            _pkIndex.Remove(pk);
        }
    }

    public void Flush()
    {
        // No-op for in-memory storage
    }

    public IEnumerable<long> AllDocIds()
    {
        lock (_lock)
        {
            return _docs.Keys.ToList();
        }
    }

    public void Dispose()
    {
        // No unmanaged resources
    }
}
