using Foundation.AI.Embed;
using Foundation.AI.Rag;
using Foundation.AI.VectorStore;
using Microsoft.Extensions.Logging;

namespace BMC.AI;

/// <summary>
/// Service interface for BMC AI-powered features.
/// </summary>
public interface IBmcAiService
{
    /// <summary>
    /// Semantic search across all brick parts.
    /// Returns relevant parts ranked by similarity to the query.
    /// </summary>
    Task<IReadOnlyList<BmcSearchResult>> SearchPartsAsync(string query, int topK = 10, CancellationToken ct = default);

    /// <summary>
    /// Semantic search across all LEGO sets.
    /// Returns relevant sets ranked by similarity to the query.
    /// </summary>
    Task<IReadOnlyList<BmcSearchResult>> SearchSetsAsync(string query, int topK = 10, CancellationToken ct = default);

    /// <summary>
    /// RAG-powered chat: answer questions using the full LEGO knowledge base.
    /// Retrieves relevant parts and sets, then generates a natural language answer.
    /// </summary>
    Task<BmcChatResponse> ChatAsync(string question, CancellationToken ct = default);

    /// <summary>
    /// Streaming version of ChatAsync — returns answer tokens as they're generated.
    /// </summary>
    IAsyncEnumerable<string> ChatStreamAsync(string question, CancellationToken ct = default);
}

/// <summary>
/// Main BMC AI service implementation.
/// Wraps Foundation.AI components with BMC-specific logic and system prompts.
/// </summary>
public class BmcAiService : IBmcAiService
{
    private readonly IEmbeddingProvider _embed;
    private readonly IVectorStore _vectorStore;
    private readonly IRagService _rag;
    private readonly ILogger<BmcAiService> _logger;

    private const string SystemPrompt =
        """
        You are the BMC Building Assistant — an expert on LEGO parts, sets, Technic mechanisms, and building techniques.

        You have access to a comprehensive database of every LEGO part and set ever made. When answering:
        - Be specific: cite part numbers, set numbers, and exact names
        - For mechanical questions, explain gear ratios, axle compatibility, and connection types
        - For set questions, mention the year, theme, and approximate part count
        - If multiple parts match, list the best options with pros/cons
        - Keep answers concise but informative
        - If you don't know something, say so rather than guessing

        Use the provided context to ground your answers in real data.
        """;

    public BmcAiService(
        IEmbeddingProvider embed,
        IVectorStore vectorStore,
        IRagService rag,
        ILogger<BmcAiService> logger)
    {
        _embed = embed;
        _vectorStore = vectorStore;
        _rag = rag;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BmcSearchResult>> SearchPartsAsync(
        string query, int topK = 10, CancellationToken ct = default)
    {
        return await SemanticSearchAsync(BmcSearchIndex.PartsCollection, query, topK, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BmcSearchResult>> SearchSetsAsync(
        string query, int topK = 10, CancellationToken ct = default)
    {
        return await SemanticSearchAsync(BmcSearchIndex.SetsCollection, query, topK, ct);
    }

    /// <inheritdoc />
    public async Task<BmcChatResponse> ChatAsync(string question, CancellationToken ct = default)
    {
        _logger.LogInformation("BMC Chat: {Question}", question);

        // Search both parts and sets for maximum context
        var response = await _rag.QueryAsync(question, new RagOptions
        {
            Collection = BmcSearchIndex.PartsCollection,
            TopK = 8,
            SystemPrompt = SystemPrompt,
            Temperature = 0.3f,
            MaxTokens = 1024
        }, ct);

        return new BmcChatResponse
        {
            Answer = response.Answer,
            Sources = response.Sources.Select(s => new BmcChatSource
            {
                DocId = s.DocId,
                Excerpt = s.Excerpt,
                Score = s.Score
            }).ToList()
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> ChatStreamAsync(
        string question,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        _logger.LogInformation("BMC Chat (streaming): {Question}", question);

        if (!await _vectorStore.CollectionExistsAsync(BmcSearchIndex.PartsCollection, ct))
        {
            _logger.LogWarning("Collection '{Collection}' does not exist — chat unavailable until indexing is run", BmcSearchIndex.PartsCollection);
            yield return "The AI knowledge base hasn't been indexed yet. Please run indexing from the admin panel first (POST /api/ai/index).";
            yield break;
        }

        await foreach (var token in _rag.QueryStreamAsync(question, new RagOptions
        {
            Collection = BmcSearchIndex.PartsCollection,
            TopK = 8,
            SystemPrompt = SystemPrompt,
            Temperature = 0.3f,
            MaxTokens = 1024
        }, ct))
        {
            yield return token;
        }
    }

    // ───────────────────────────────────────────────────────────
    //  Internal helpers
    // ───────────────────────────────────────────────────────────

    private async Task<IReadOnlyList<BmcSearchResult>> SemanticSearchAsync(
        string collection, string query, int topK, CancellationToken ct)
    {
        _logger.LogInformation("Semantic search in '{Collection}': {Query}", collection, query);

        if (!await _vectorStore.CollectionExistsAsync(collection, ct))
        {
            _logger.LogWarning("Collection '{Collection}' does not exist — returning empty results", collection);
            return Array.Empty<BmcSearchResult>();
        }

        var queryVector = await _embed.EmbedAsync(query, ct);
        var results = await _vectorStore.SearchAsync(collection, queryVector, topK, ct: ct);

        return results.Select(r => new BmcSearchResult
        {
            Id = r.Id,
            Score = r.Score,
            Name = r.Metadata?.GetValueOrDefault("name")?.ToString() ?? "",
            Type = r.Metadata?.GetValueOrDefault("type")?.ToString() ?? "",
            Category = r.Metadata?.GetValueOrDefault("category")?.ToString() ?? "",
            Year = r.Metadata?.GetValueOrDefault("year")?.ToString() ?? ""
        }).ToList();
    }
}

// ───────────────────────────────────────────────────────────
//  Response models
// ───────────────────────────────────────────────────────────

/// <summary>
/// A single semantic search result from the BMC knowledge base.
/// </summary>
public class BmcSearchResult
{
    /// <summary>Internal doc ID (e.g. "part-1234" or "set-5678").</summary>
    public string Id { get; set; } = "";

    /// <summary>Similarity score (higher = more relevant).</summary>
    public float Score { get; set; }

    /// <summary>Human-readable name of the part or set.</summary>
    public string Name { get; set; } = "";

    /// <summary>Result type: "part" or "set".</summary>
    public string Type { get; set; } = "";

    /// <summary>Category for parts, theme for sets.</summary>
    public string Category { get; set; } = "";

    /// <summary>Release year (for sets only).</summary>
    public string Year { get; set; } = "";
}

/// <summary>
/// Response from the BMC Building Assistant chat.
/// </summary>
public class BmcChatResponse
{
    /// <summary>The generated answer text.</summary>
    public string Answer { get; set; } = "";

    /// <summary>Source documents that contributed to the answer.</summary>
    public List<BmcChatSource> Sources { get; set; } = new();
}

/// <summary>
/// A source reference from the RAG pipeline.
/// </summary>
public class BmcChatSource
{
    public string DocId { get; set; } = "";
    public string Excerpt { get; set; } = "";
    public float Score { get; set; }
}
