using System.Runtime.CompilerServices;
using System.Text;
using Foundation.AI.Embed;
using Foundation.AI.Inference;
using Foundation.AI.VectorStore;

namespace Foundation.AI.Rag;

/// <summary>
/// Default RAG service implementation.
///
/// <para><b>Pipeline:</b>
/// <list type="number">
/// <item>Index: Document → Chunk → Embed each chunk → Store in VectorStore</item>
/// <item>Query: Embed question → Search VectorStore → Build prompt → LLM Generate</item>
/// </list></para>
///
/// <para><b>Dependencies:</b>
/// <list type="bullet">
/// <item><see cref="IEmbeddingProvider"/> — converts text to vectors</item>
/// <item><see cref="IVectorStore"/> — stores and retrieves vectors</item>
/// <item><see cref="IInferenceProvider"/> — generates answers from context</item>
/// <item><see cref="IDocumentChunker"/> — splits documents into chunks</item>
/// </list></para>
/// </summary>
public sealed class RagService : IRagService
{
    private readonly IEmbeddingProvider _embedder;
    private readonly IVectorStore _vectorStore;
    private readonly IInferenceProvider _inference;
    private readonly IDocumentChunker _chunker;

    private const string DefaultSystemPrompt =
        """
        You are a helpful assistant. Answer the user's question based ONLY on the provided context.
        If the context does not contain enough information to answer, say so clearly.
        Do not make up information. Cite relevant details from the context in your answer.
        """;

    public RagService(
        IEmbeddingProvider embedder,
        IVectorStore vectorStore,
        IInferenceProvider inference,
        IDocumentChunker chunker)
    {
        _embedder = embedder;
        _vectorStore = vectorStore;
        _inference = inference;
        _chunker = chunker;
    }

    public async Task<RagResponse> QueryAsync(string question,
        RagOptions? options = null, CancellationToken ct = default)
    {
        options ??= new RagOptions();

        // 1. Embed the question
        var queryVector = await _embedder.EmbedAsync(question, ct);

        // 2. Search for relevant chunks
        var searchResults = await _vectorStore.SearchAsync(
            options.Collection, queryVector, options.TopK, options.Filter);

        // 3. Filter by minimum relevance score
        var relevant = searchResults
            .Where(r => r.Score >= options.MinRelevanceScore)
            .ToList();

        if (relevant.Count == 0)
        {
            return new RagResponse(
                "I couldn't find any relevant information to answer your question.",
                []);
        }

        // 4. Build sources and context
        var sources = BuildSources(relevant);
        var prompt = BuildRagPrompt(question, sources);

        // 5. Generate answer
        var systemPrompt = options.SystemPrompt ?? DefaultSystemPrompt;
        var response = await _inference.GenerateAsync(prompt, new InferenceOptions
        {
            SystemPrompt = systemPrompt,
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens
        }, ct);

        return new RagResponse(response.Content, sources);
    }

    public async IAsyncEnumerable<string> QueryStreamAsync(string question,
        RagOptions? options = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        options ??= new RagOptions();

        // 1. Embed the question
        var queryVector = await _embedder.EmbedAsync(question, ct);

        // 2. Search for relevant chunks
        var searchResults = await _vectorStore.SearchAsync(
            options.Collection, queryVector, options.TopK, options.Filter);

        // 3. Filter by minimum relevance score
        var relevant = searchResults
            .Where(r => r.Score >= options.MinRelevanceScore)
            .ToList();

        if (relevant.Count == 0)
        {
            yield return "I couldn't find any relevant information to answer your question.";
            yield break;
        }

        // 4. Build context and prompt
        var sources = BuildSources(relevant);
        var prompt = BuildRagPrompt(question, sources);

        // 5. Stream the answer
        var systemPrompt = options.SystemPrompt ?? DefaultSystemPrompt;
        await foreach (var token in _inference.GenerateStreamAsync(prompt, new InferenceOptions
        {
            SystemPrompt = systemPrompt,
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens
        }, ct))
        {
            yield return token;
        }
    }

    public async Task IndexDocumentAsync(string collection, string docId, string text,
        Dictionary<string, object>? metadata = null, CancellationToken ct = default)
    {
        // Ensure collection exists
        if (!await _vectorStore.CollectionExistsAsync(collection))
            await _vectorStore.CreateCollectionAsync(collection, _embedder.Dimension);

        // Chunk the document
        var chunks = _chunker.Chunk(text, docId);

        if (chunks.Count == 0)
            return;

        // Embed all chunks in a single batch
        var texts = chunks.Select(c => c.Text).ToList();
        var embeddings = await _embedder.EmbedBatchAsync(texts, ct);

        // Build vector documents with metadata
        var docs = new List<VectorDocument>(chunks.Count);
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunkMeta = new Dictionary<string, object>
            {
                ["doc_id"] = chunks[i].DocId,
                ["chunk_index"] = chunks[i].Index,
                ["text"] = chunks[i].Text
            };

            // Merge user-provided metadata
            if (metadata != null)
            {
                foreach (var kv in metadata)
                    chunkMeta[kv.Key] = kv.Value;
            }

            docs.Add(new VectorDocument(chunks[i].ChunkId, embeddings[i], chunkMeta));
        }

        // Store in vector store
        await _vectorStore.UpsertBatchAsync(collection, docs);
    }

    public async Task IndexBatchAsync(string collection, IReadOnlyList<RagDocument> documents,
        CancellationToken ct = default)
    {
        foreach (var doc in documents)
        {
            await IndexDocumentAsync(collection, doc.Id, doc.Text, doc.Metadata, ct);
        }
    }

    public async Task RemoveDocumentAsync(string collection, string docId,
        CancellationToken ct = default)
    {
        // Search for all chunks belonging to this document
        // Convention: chunk IDs are "{docId}::chunk-{index}"
        // We need to delete all chunks with this prefix
        // Since IVectorStore doesn't have a prefix-delete, we search by metadata
        var queryVector = await _embedder.EmbedAsync(docId, ct);
        var results = await _vectorStore.SearchAsync(collection, queryVector, topK: 1000);

        foreach (var result in results)
        {
            if (result.Metadata?.TryGetValue("doc_id", out var metaDocId) == true &&
                metaDocId?.ToString() == docId)
            {
                await _vectorStore.DeleteAsync(collection, result.Id);
            }
        }
    }

    // ─── Helpers ───────────────────────────────────────────────

    private static List<RagSource> BuildSources(IReadOnlyList<VectorSearchResult> results)
    {
        var sources = new List<RagSource>(results.Count);
        foreach (var result in results)
        {
            var docId = result.Metadata?.TryGetValue("doc_id", out var id) == true
                ? id?.ToString() ?? result.Id
                : result.Id;
            var excerpt = result.Metadata?.TryGetValue("text", out var text) == true
                ? text?.ToString() ?? ""
                : "";

            sources.Add(new RagSource(docId, excerpt, result.Score));
        }
        return sources;
    }

    private static string BuildRagPrompt(string question, IReadOnlyList<RagSource> sources)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Context (retrieved from knowledge base):");
        sb.AppendLine();

        for (int i = 0; i < sources.Count; i++)
        {
            sb.AppendLine($"[{i + 1}] (relevance: {sources[i].Score:F2}, source: {sources[i].DocId})");
            sb.AppendLine(sources[i].Excerpt);
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"Question: {question}");

        return sb.ToString();
    }
}
