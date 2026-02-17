namespace Foundation.AI.Embed;

/// <summary>
/// Embedding provider that tries a primary (local) provider first,
/// falling back to a secondary (cloud) provider on failure.
///
/// <para><b>Typical usage:</b>
/// Primary = OnnxEmbeddingProvider (local GPU/CPU).
/// Fallback = OpenAiEmbeddingProvider (cloud API).
/// If the local model is unavailable or fails, the cloud provider is used seamlessly.</para>
///
/// <para><b>Important:</b>
/// Both providers MUST produce embeddings of the same dimension.
/// Mixing dimensions (e.g., 384 from ONNX and 1536 from OpenAI)
/// would make vectors incomparable in the same collection.</para>
/// </summary>
public sealed class FallbackEmbeddingProvider : IEmbeddingProvider
{
    private readonly IEmbeddingProvider _primary;
    private readonly IEmbeddingProvider _fallback;

    public int Dimension => _primary.Dimension;
    public string ModelName => $"{_primary.ModelName} (fallback: {_fallback.ModelName})";

    public FallbackEmbeddingProvider(IEmbeddingProvider primary, IEmbeddingProvider fallback)
    {
        if (primary.Dimension != fallback.Dimension)
            throw new ArgumentException(
                $"Dimension mismatch: primary={primary.Dimension}, fallback={fallback.Dimension}. " +
                "Both providers must produce embeddings of the same dimension.");

        _primary = primary;
        _fallback = fallback;
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        try
        {
            return await _primary.EmbedAsync(text, ct);
        }
        catch
        {
            return await _fallback.EmbedAsync(text, ct);
        }
    }

    public async Task<float[][]> EmbedBatchAsync(IReadOnlyList<string> texts,
        CancellationToken ct = default)
    {
        try
        {
            return await _primary.EmbedBatchAsync(texts, ct);
        }
        catch
        {
            return await _fallback.EmbedBatchAsync(texts, ct);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _primary.DisposeAsync();
        await _fallback.DisposeAsync();
    }
}
