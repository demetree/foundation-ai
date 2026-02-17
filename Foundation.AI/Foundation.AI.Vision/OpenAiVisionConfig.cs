namespace Foundation.AI.Vision;

/// <summary>
/// Configuration for the OpenAI Vision provider.
/// Compatible with OpenAI (GPT-4o), Azure OpenAI, and Ollama multimodal models.
/// </summary>
public sealed class OpenAiVisionConfig
{
    /// <summary>API key for authentication. For Ollama, any non-empty string works.</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// Chat completions endpoint (vision uses the same endpoint as chat).
    /// Default: OpenAI production endpoint.
    /// For Ollama: "http://localhost:11434/v1/chat/completions"
    /// </summary>
    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";

    /// <summary>
    /// Model name. Must support vision (e.g., "gpt-4o", "gpt-4o-mini", "llava").
    /// </summary>
    public string Model { get; set; } = "gpt-4o";

    /// <summary>Use Azure OpenAI authentication header (api-key) instead of Bearer token.</summary>
    public bool UseAzureAuth { get; set; }

    /// <summary>HTTP request timeout. Default: 120 seconds.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(120);
}
