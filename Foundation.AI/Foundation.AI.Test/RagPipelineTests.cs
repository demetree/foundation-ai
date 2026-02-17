using Foundation.AI.Embed;
using Foundation.AI.Inference;
using Foundation.AI.Rag;
using Foundation.AI.VectorStore;
using Foundation.AI.VectorStore.Zvec;

namespace Foundation.AI.Test;

/// <summary>
/// Integration test for the complete RAG pipeline.
/// Uses a mock embedding provider (deterministic vectors) and mock inference provider
/// to test the full flow: index → query → response with sources.
/// </summary>
public class RagPipelineTests : IAsyncLifetime
{
    private readonly string _testPath;
    private IVectorStore _store = null!;
    private RagService _rag = null!;
    private MockEmbeddingProvider _embedder = null!;
    private MockInferenceProvider _inference = null!;

    public RagPipelineTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), $"rag_test_{Guid.NewGuid():N}");
    }

    public Task InitializeAsync()
    {
        _store = new ZvecVectorStore(new ZvecVectorStoreConfig { BasePath = _testPath });
        _embedder = new MockEmbeddingProvider();
        _inference = new MockInferenceProvider();

        _rag = new RagService(_embedder, _store, _inference,
            new TextChunker { ChunkSize = 100, ChunkOverlap = 20 });

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _store.DisposeAsync();
        if (Directory.Exists(_testPath))
            Directory.Delete(_testPath, recursive: true);
    }

    [Fact]
    public async Task IndexAndQuery_ShouldReturnRelevantSources()
    {
        // Index a document
        await _rag.IndexDocumentAsync("test", "doc-1",
            "Cats are domestic animals. They like to sleep and play.");

        // Query
        var response = await _rag.QueryAsync("Tell me about cats",
            new RagOptions { Collection = "test", TopK = 3 });

        Assert.NotNull(response);
        Assert.NotEmpty(response.Answer);
        Assert.NotEmpty(response.Sources);
        Assert.Contains(response.Sources, s => s.DocId.StartsWith("doc-1"));
    }

    [Fact]
    public async Task IndexBatch_ShouldIndexAllDocuments()
    {
        var docs = new List<RagDocument>
        {
            new("doc-a", "The quick brown fox jumps over the lazy dog."),
            new("doc-b", "A stitch in time saves nine. Early to bed, early to rise."),
        };

        await _rag.IndexBatchAsync("test-batch", docs);

        // Query should find results
        var response = await _rag.QueryAsync("fox",
            new RagOptions { Collection = "test-batch" });

        Assert.NotEmpty(response.Sources);
    }

    // ─── Mock Providers ─────────────────────────────────────────

    /// <summary>
    /// Mock embedding provider that produces deterministic vectors.
    /// Hashes the input text to create a reproducible embedding.
    /// </summary>
    private sealed class MockEmbeddingProvider : IEmbeddingProvider
    {
        public int Dimension => 4;
        public string ModelName => "mock:test";

        public Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
        {
            return Task.FromResult(HashToVector(text));
        }

        public Task<float[][]> EmbedBatchAsync(IReadOnlyList<string> texts,
            CancellationToken ct = default)
        {
            return Task.FromResult(texts.Select(HashToVector).ToArray());
        }

        private float[] HashToVector(string text)
        {
            // Simple deterministic hash → vector
            int hash = text.GetHashCode();
            var vec = new float[4];
            for (int i = 0; i < 4; i++)
            {
                vec[i] = ((hash >> (i * 8)) & 0xFF) / 255f;
            }
            // L2 normalize
            float norm = MathF.Sqrt(vec.Sum(v => v * v));
            if (norm > 0) for (int i = 0; i < 4; i++) vec[i] /= norm;
            return vec;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    /// <summary>
    /// Mock inference provider that echoes back the context it receives.
    /// </summary>
    private sealed class MockInferenceProvider : IInferenceProvider
    {
        public string ModelName => "mock:test";

        public Task<InferenceResponse> GenerateAsync(string prompt,
            InferenceOptions? options = null, CancellationToken ct = default)
        {
            return Task.FromResult(new InferenceResponse(
                Content: $"Based on the context: {prompt[..Math.Min(200, prompt.Length)]}...",
                TokensUsed: 50));
        }

        public async IAsyncEnumerable<string> GenerateStreamAsync(string prompt,
            InferenceOptions? options = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            yield return "Streamed ";
            await Task.Delay(1, ct);
            yield return "response.";
        }

        public Task<InferenceResponse> ChatAsync(IReadOnlyList<ChatMessage> messages,
            InferenceOptions? options = null, CancellationToken ct = default)
        {
            var lastMsg = messages.LastOrDefault()?.Content ?? "";
            return Task.FromResult(new InferenceResponse(
                Content: $"Chat response to: {lastMsg}", TokensUsed: 30));
        }

        public async IAsyncEnumerable<string> ChatStreamAsync(IReadOnlyList<ChatMessage> messages,
            InferenceOptions? options = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            yield return "Chat ";
            await Task.Delay(1, ct);
            yield return "stream.";
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
