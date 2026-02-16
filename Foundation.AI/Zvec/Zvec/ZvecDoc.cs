using Foundation.AI.Zvec.Engine.Core;

namespace Foundation.AI.Zvec;

/// <summary>
/// Represents a document in a zvec collection.
/// Supports fluent Set() calls for building documents.
/// Now backed by a managed Dictionary — no native handles.
/// </summary>
public sealed class ZvecDoc : IDisposable
{
    internal Dictionary<string, object?> Fields { get; } = new();
    internal Dictionary<string, float[]> Vectors { get; } = new();
    private bool _disposed;

    private string _pk = "";
    private float _score;
    private ulong _docId;

    /// <summary>Create a new empty document.</summary>
    public ZvecDoc() { }

    /// <summary>Create a new document with a primary key.</summary>
    public ZvecDoc(string pk)
    {
        _pk = pk;
    }

    /// <summary>Internal constructor wrapping an engine Document (for query results).</summary>
    internal ZvecDoc(Document engineDoc)
    {
        _pk = engineDoc.PrimaryKey;
        _docId = (ulong)engineDoc.DocId;
        _score = engineDoc.Score;

        foreach (var (k, v) in engineDoc.Fields)
            Fields[k] = v;
        foreach (var (k, v) in engineDoc.Vectors)
            Vectors[k] = v;
    }

    // =========================================================================
    // Primary Key, Score, DocId
    // =========================================================================

    public string PrimaryKey
    {
        get => _pk;
        set => _pk = value;
    }

    public float Score => _score;
    public ulong DocId => _docId;

    // =========================================================================
    // Fluent field setters
    // =========================================================================

    public ZvecDoc Set(string field, bool value) { Fields[field] = value; return this; }
    public ZvecDoc Set(string field, int value) { Fields[field] = value; return this; }
    public ZvecDoc Set(string field, uint value) { Fields[field] = value; return this; }
    public ZvecDoc Set(string field, long value) { Fields[field] = value; return this; }
    public ZvecDoc Set(string field, ulong value) { Fields[field] = value; return this; }
    public ZvecDoc Set(string field, float value) { Fields[field] = value; return this; }
    public ZvecDoc Set(string field, double value) { Fields[field] = value; return this; }
    public ZvecDoc Set(string field, string value) { Fields[field] = value; return this; }

    public ZvecDoc Set(string field, float[] vector) { Vectors[field] = vector; return this; }

    public ZvecDoc Set(string field, ReadOnlySpan<float> vector)
    {
        Vectors[field] = vector.ToArray();
        return this;
    }

    public ZvecDoc Set(string field, double[] vector)
    {
        // Store as float[] for engine compatibility
        var fp32 = new float[vector.Length];
        for (int i = 0; i < vector.Length; i++)
            fp32[i] = (float)vector[i];
        Vectors[field] = fp32;
        return this;
    }

    public ZvecDoc Set(string field, sbyte[] vector)
    {
        // Store as float[] for engine compatibility
        var fp32 = new float[vector.Length];
        for (int i = 0; i < vector.Length; i++)
            fp32[i] = vector[i];
        Vectors[field] = fp32;
        return this;
    }

    // =========================================================================
    // Field getters
    // =========================================================================

    public bool GetBool(string field) =>
        Fields.TryGetValue(field, out var v) && v is bool b ? b : false;

    public int GetInt32(string field) =>
        Fields.TryGetValue(field, out var v) && v is IConvertible c ? c.ToInt32(null) : 0;

    public long GetInt64(string field) =>
        Fields.TryGetValue(field, out var v) && v is IConvertible c ? c.ToInt64(null) : 0;

    public float GetFloat(string field) =>
        Fields.TryGetValue(field, out var v) && v is IConvertible c ? c.ToSingle(null) : 0f;

    public double GetDouble(string field) =>
        Fields.TryGetValue(field, out var v) && v is IConvertible c ? c.ToDouble(null) : 0.0;

    public string GetString(string field) =>
        Fields.TryGetValue(field, out var v) && v is string s ? s : "";

    public float[] GetVectorFP32(string field)
    {
        return Vectors.TryGetValue(field, out var vec) ? vec : [];
    }

    // =========================================================================
    // Engine conversion
    // =========================================================================

    /// <summary>
    /// Convert to an engine Document for internal processing.
    /// </summary>
    internal Document ToEngineDocument()
    {
        var doc = new Document { PrimaryKey = _pk };

        foreach (var (k, v) in Fields)
            doc.Fields[k] = v;
        foreach (var (k, v) in Vectors)
            doc.Vectors[k] = v;

        return doc;
    }

    // =========================================================================
    // Dispose
    // =========================================================================

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~ZvecDoc() => Dispose();
}
