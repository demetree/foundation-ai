// Copyright 2025-present the zvec project — Pure C# Engine

namespace Foundation.AI.Zvec.Engine.Core;

/// <summary>
/// Internal document representation. A document is a set of named fields
/// (scalar values and vector arrays) identified by a primary key.
/// </summary>
public sealed class Document
{
    /// <summary>Primary key (unique identifier).</summary>
    public string PrimaryKey { get; set; } = "";

    /// <summary>Internal document ID assigned by the engine.</summary>
    public long DocId { get; set; }

    /// <summary>Relevance score (populated during query results).</summary>
    public float Score { get; set; }

    /// <summary>Scalar field values keyed by field name.</summary>
    public Dictionary<string, object?> Fields { get; } = new();

    /// <summary>Vector field values keyed by field name.</summary>
    public Dictionary<string, float[]> Vectors { get; } = new();

    // =========================================================================
    // Scalar field setters
    // =========================================================================

    public Document SetField(string name, object? value)
    {
        Fields[name] = value;
        return this;
    }

    public Document SetVector(string name, float[] vector)
    {
        Vectors[name] = vector;
        return this;
    }

    // =========================================================================
    // Scalar field getters
    // =========================================================================

    public T? GetField<T>(string name)
    {
        if (Fields.TryGetValue(name, out var val) && val is T typed)
            return typed;
        return default;
    }

    public float[]? GetVector(string name)
    {
        return Vectors.TryGetValue(name, out var vec) ? vec : null;
    }

    /// <summary>
    /// Create a deep copy of this document.
    /// </summary>
    public Document Clone()
    {
        var clone = new Document
        {
            PrimaryKey = PrimaryKey,
            DocId = DocId,
            Score = Score
        };

        foreach (var (k, v) in Fields)
            clone.Fields[k] = v;

        foreach (var (k, v) in Vectors)
        {
            var copy = new float[v.Length];
            v.AsSpan().CopyTo(copy);
            clone.Vectors[k] = copy;
        }

        return clone;
    }
}
