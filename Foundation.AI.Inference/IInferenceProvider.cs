namespace Foundation.AI.Inference;

/// <summary>
/// Generates text responses from prompts or conversations.
///
/// <para><b>Purpose:</b>
/// Provides a unified interface over LLM backends — local (LLamaSharp, Ollama)
/// and cloud (OpenAI, Azure OpenAI). Supports single-shot generation,
/// streaming output, and multi-turn chat conversations.</para>
///
/// <para><b>Thread safety:</b>
/// Implementations must be safe for concurrent calls. Cloud providers use
/// async HTTP; local providers manage their own thread pools.</para>
/// </summary>
public interface IInferenceProvider : IAsyncDisposable
{
    /// <summary>
    /// A human-readable name for this provider (e.g., "openai:gpt-4o", "ollama:llama3").
    /// Useful for logging and diagnostics.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Generate a complete response from a prompt.
    /// </summary>
    /// <param name="prompt">The user prompt.</param>
    /// <param name="options">Generation parameters (temperature, max tokens, system prompt, etc.).</param>
    /// <returns>The complete generated response.</returns>
    Task<InferenceResponse> GenerateAsync(string prompt,
        InferenceOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Generate a response token-by-token for streaming UX.
    /// Each yielded string is a token or small chunk of text.
    /// </summary>
    IAsyncEnumerable<string> GenerateStreamAsync(string prompt,
        InferenceOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Multi-turn chat conversation.
    /// <para>Supports system, user, and assistant messages for maintaining
    /// conversation history and context.</para>
    /// </summary>
    Task<InferenceResponse> ChatAsync(IReadOnlyList<ChatMessage> messages,
        InferenceOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Streaming multi-turn chat conversation.
    /// </summary>
    IAsyncEnumerable<string> ChatStreamAsync(IReadOnlyList<ChatMessage> messages,
        InferenceOptions? options = null,
        CancellationToken ct = default);
}
