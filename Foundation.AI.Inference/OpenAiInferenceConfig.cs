namespace Foundation.AI.Inference;

/// <summary>
/// Configuration for the OpenAI inference provider.
/// Compatible with OpenAI, Azure OpenAI, and Ollama (OpenAI-compatible endpoints).
/// </summary>
public sealed class OpenAiInferenceConfig
{
    /// <summary>
    /// API key for authentication.
    /// For Ollama, any non-empty string works (e.g., "ollama").
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// Chat completions endpoint.
    /// Default: OpenAI production endpoint.
    /// For Ollama: "http://localhost:11434/v1/chat/completions"
    /// For Azure: "https://{resource}.openai.azure.com/openai/deployments/{deployment}/chat/completions?api-version=2024-06-01"
    /// </summary>
    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";

    /// <summary>
    /// Model name (e.g., "gpt-4o", "gpt-4o-mini", "llama3", "mistral").
    /// Ignored for Azure OpenAI (model is specified in the deployment).
    /// </summary>
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Use Azure OpenAI authentication header (api-key) instead of Bearer token.
    /// </summary>
    public bool UseAzureAuth { get; set; }

    /// <summary>
    /// HTTP request timeout. Default: 120 seconds (LLM generation can be slow).
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(120);
}
