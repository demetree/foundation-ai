using Foundation.AI.VectorStore;

namespace Foundation.AI.Rag;

/// <summary>
/// Retrieval-Augmented Generation service.
///
/// <para><b>Purpose:</b>
/// Orchestrates the full RAG pipeline: document indexing (chunk → embed → store)
/// and query answering (embed query → retrieve relevant chunks → generate answer).
/// Ties together <c>IEmbeddingProvider</c>, <c>IVectorStore</c>, and
/// <c>IInferenceProvider</c>.</para>
///
/// <para><b>Usage pattern:</b>
/// <code>
/// // Index documents
/// await rag.IndexDocumentAsync("policies", "doc-1", longDocumentText);
///
/// // Query
/// var response = await rag.QueryAsync("What is the vacation policy?",
///     new RagOptions { Collection = "policies" });
/// Console.WriteLine(response.Answer);
/// foreach (var source in response.Sources)
///     Console.WriteLine($"  [{source.Score:F2}] {source.DocId}: {source.Excerpt}");
/// </code></para>
/// </summary>
public interface IRagService
{
    /// <summary>
    /// Answer a question using relevant context retrieved from the vector store.
    /// </summary>
    /// <param name="question">Natural language question.</param>
    /// <param name="options">RAG options: collection, topK, filter, system prompt.</param>
    /// <returns>Generated answer with source attribution.</returns>
    Task<RagResponse> QueryAsync(string question,
        RagOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Stream the answer token-by-token for responsive UX.
    /// Sources are not available until the full response is generated.
    /// </summary>
    IAsyncEnumerable<string> QueryStreamAsync(string question,
        RagOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Index a single document: chunk it, embed all chunks, store in vector store.
    /// </summary>
    /// <param name="collection">Vector collection name.</param>
    /// <param name="docId">Unique document identifier (used as prefix for chunk IDs).</param>
    /// <param name="text">Full document text to chunk and index.</param>
    /// <param name="metadata">Optional metadata applied to all chunks.</param>
    Task IndexDocumentAsync(string collection, string docId, string text,
        Dictionary<string, object>? metadata = null,
        CancellationToken ct = default);

    /// <summary>
    /// Index multiple documents in a batch for efficiency.
    /// </summary>
    Task IndexBatchAsync(string collection, IReadOnlyList<RagDocument> documents,
        CancellationToken ct = default);

    /// <summary>
    /// Remove a previously indexed document and all its chunks from the vector store.
    /// </summary>
    Task RemoveDocumentAsync(string collection, string docId,
        CancellationToken ct = default);
}

/// <summary>RAG query options.</summary>
public class RagOptions
{
    /// <summary>Vector collection to search. Default: "default".</summary>
    public string Collection { get; set; } = "default";

    /// <summary>Number of top relevant chunks to retrieve. Default: 5.</summary>
    public int TopK { get; set; } = 5;

    /// <summary>Optional metadata filter expression (provider-specific syntax).</summary>
    public string? Filter { get; set; }

    /// <summary>
    /// System prompt for the LLM. Defines AI behavior and response format.
    /// Default provides a standard RAG instruction.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Minimum relevance score (0.0-1.0). Chunks below this threshold are excluded.
    /// Default: 0.0 (include all).
    /// </summary>
    public float MinRelevanceScore { get; set; }

    /// <summary>Temperature for LLM generation. Default: 0.3 (factual).</summary>
    public float Temperature { get; set; } = 0.3f;

    /// <summary>Max tokens for the generated answer. Default: 1024.</summary>
    public int MaxTokens { get; set; } = 1024;
}

/// <summary>A document to index for RAG retrieval.</summary>
public record RagDocument(string Id, string Text, Dictionary<string, object>? Metadata = null);

/// <summary>Complete RAG response: answer text with source attribution.</summary>
public record RagResponse(string Answer, IReadOnlyList<RagSource> Sources);

/// <summary>A source chunk that contributed to the RAG answer.</summary>
public record RagSource(string DocId, string Excerpt, float Score);
