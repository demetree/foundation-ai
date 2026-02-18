using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foundation.AI.Embed;

/// <summary>
/// Configuration for the OpenAI embedding provider.
/// Compatible with OpenAI, Azure OpenAI, and Ollama (OpenAI-compatible endpoints).
/// </summary>
public sealed class OpenAiEmbeddingConfig
{
    /// <summary>
    /// API key for authentication.
    /// For OpenAI: starts with "sk-".
    /// For Azure OpenAI: the deployment key from Azure portal.
    /// For Ollama: any non-empty string (e.g., "ollama").
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// API endpoint URL.
    /// Default: OpenAI's production embeddings endpoint.
    /// For Ollama: "http://localhost:11434/v1/embeddings"
    /// For Azure OpenAI: https://{resource}.openai.azure.com/openai/deployments/{deployment}/embeddings?api-version=2024-06-01
    /// </summary>
    public string Endpoint { get; set; } = "https://api.openai.com/v1/embeddings";

    /// <summary>
    /// Model name (e.g., "text-embedding-3-small", "nomic-embed-text").
    /// Ignored for Azure OpenAI (model is specified in the deployment).
    /// </summary>
    public string Model { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// Explicit embedding dimension override. When set (&gt; 0), overrides the
    /// auto-detected dimension based on model name. Required for Ollama and custom models.
    /// Common values: 384 (MiniLM), 768 (nomic-embed-text), 1536 (OpenAI small).
    /// </summary>
    public int Dimension { get; set; }

    /// <summary>
    /// Whether to use the Azure OpenAI authentication header (api-key) instead of
    /// the standard OpenAI bearer token.
    /// </summary>
    public bool UseAzureAuth { get; set; }

    /// <summary>
    /// HTTP request timeout. Default: 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Embedding provider using the OpenAI (or Azure OpenAI) embeddings API.
///
/// <para><b>When to use:</b>
/// Cloud fallback when local ONNX models are unavailable, or when you need
/// access to higher-quality models (text-embedding-3-large = 3072d) without
/// local GPU resources.</para>
///
/// <para><b>Cost awareness:</b>
/// text-embedding-3-small: ~$0.02 per 1M tokens.
/// text-embedding-3-large: ~$0.13 per 1M tokens.
/// Batch calls are more efficient than individual calls.</para>
///
/// <para><b>Thread safety:</b>
/// HttpClient is thread-safe. This provider can be used as a singleton.</para>
/// </summary>
public sealed class OpenAiEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiEmbeddingConfig _config;
    private readonly int _dimension;
    private readonly string _modelName;

    public int Dimension => _dimension;
    public string ModelName => _modelName;

    public OpenAiEmbeddingProvider(OpenAiEmbeddingConfig config)
    {
        _config = config;

        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new ArgumentException("ApiKey must be specified.", nameof(config));

        _httpClient = new HttpClient { Timeout = config.Timeout };

        if (config.UseAzureAuth)
            _httpClient.DefaultRequestHeaders.Add("api-key", config.ApiKey);
        else
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ApiKey);

        // Dimension: use explicit config if set, otherwise auto-detect from known models
        _dimension = config.Dimension > 0
            ? config.Dimension
            : config.Model switch
            {
                "text-embedding-3-small" => 1536,
                "text-embedding-3-large" => 3072,
                "text-embedding-ada-002" => 1536,
                _ => 1536 // default assumption
            };

        _modelName = $"openai:{config.Model}";
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        var results = await EmbedBatchAsync([text], ct);
        return results[0];
    }

    public async Task<float[][]> EmbedBatchAsync(IReadOnlyList<string> texts,
        CancellationToken ct = default)
    {
        if (texts.Count == 0)
            return [];

        var request = new EmbeddingRequest
        {
            Model = _config.Model,
            Input = texts.ToList()
        };

        var response = await _httpClient.PostAsJsonAsync(
            _config.Endpoint, request, JsonOptions, ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(
            JsonOptions, ct);

        if (result?.Data == null || result.Data.Count != texts.Count)
            throw new InvalidOperationException(
                $"Expected {texts.Count} embeddings, got {result?.Data?.Count ?? 0}");

        // Sort by index to ensure correct order
        var sorted = result.Data.OrderBy(d => d.Index).ToList();
        var embeddings = new float[texts.Count][];
        for (int i = 0; i < texts.Count; i++)
            embeddings[i] = sorted[i].Embedding;

        return embeddings;
    }

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }

    // ─── JSON Contracts ─────────────────────────────────────────────

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private sealed class EmbeddingRequest
    {
        public string Model { get; set; } = "";
        public List<string> Input { get; set; } = [];
    }

    private sealed class EmbeddingResponse
    {
        public List<EmbeddingData> Data { get; set; } = [];
    }

    private sealed class EmbeddingData
    {
        public int Index { get; set; }
        public float[] Embedding { get; set; } = [];
    }
}
