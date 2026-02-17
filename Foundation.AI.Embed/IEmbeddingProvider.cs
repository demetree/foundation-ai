namespace Foundation.AI.Embed;

/// <summary>
/// Generates vector embeddings from text input.
///
/// <para><b>Purpose:</b>
/// Converts natural language text into dense float vectors (embeddings)
/// that capture semantic meaning. These vectors are then stored in an
/// <c>IVectorStore</c> for similarity search.</para>
///
/// <para><b>Provider model:</b>
/// Different providers use different backends (local ONNX model, OpenAI API, etc.)
/// but all produce float[] embeddings of a fixed dimension. The dimension
/// depends on the model — e.g., all-MiniLM-L6 = 384, BGE-small = 384,
/// text-embedding-3-small = 1536.</para>
///
/// <para><b>Thread safety:</b>
/// Implementations must be safe for concurrent calls. ONNX Runtime manages
/// its own thread pool; cloud providers use async HTTP.</para>
/// </summary>
public interface IEmbeddingProvider : IAsyncDisposable
{
    /// <summary>
    /// The dimensionality of embeddings produced by this provider.
    /// All vectors from a given provider have this fixed length.
    /// Must match the vector collection dimension in the vector store.
    /// </summary>
    int Dimension { get; }

    /// <summary>
    /// A human-readable name for this provider (e.g., "onnx:all-MiniLM-L6-v2", "openai:text-embedding-3-small").
    /// Useful for logging and diagnostics.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Embed a single text string.
    /// </summary>
    /// <param name="text">Input text to embed. Will be truncated if it exceeds the model's max token length.</param>
    /// <returns>A float array of length <see cref="Dimension"/>.</returns>
    Task<float[]> EmbedAsync(string text, CancellationToken ct = default);

    /// <summary>
    /// Embed multiple texts in a single batch for throughput.
    /// GPU-accelerated providers see significant speedup from batching
    /// (e.g., 10-50× vs sequential calls for ONNX + CUDA).
    /// </summary>
    /// <param name="texts">Texts to embed. Each produces one embedding.</param>
    /// <returns>Array of embeddings, one per input text, in the same order.</returns>
    Task<float[][]> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default);
}
