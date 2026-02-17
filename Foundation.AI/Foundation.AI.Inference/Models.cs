namespace Foundation.AI.Inference;

/// <summary>
/// Options for controlling LLM text generation behavior.
/// </summary>
public class InferenceOptions
{
    /// <summary>
    /// Controls randomness. Lower = more deterministic, higher = more creative.
    /// Range: 0.0 (greedy) to 2.0 (very random). Default: 0.7.
    /// </summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>
    /// Maximum number of tokens to generate in the response.
    /// Default: 1024. Set higher for long-form content.
    /// </summary>
    public int MaxTokens { get; set; } = 1024;

    /// <summary>
    /// Nucleus sampling: only sample from tokens whose cumulative probability
    /// exceeds this threshold. Range: 0.0 to 1.0. Default: 0.9.
    /// </summary>
    public float TopP { get; set; } = 0.9f;

    /// <summary>
    /// System prompt that defines the AI's role and behavior.
    /// Prepended to the conversation as a system message.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Sequences that cause the model to stop generating.
    /// </summary>
    public IReadOnlyList<string>? StopSequences { get; set; }
}

/// <summary>
/// A message in a multi-turn chat conversation.
/// </summary>
/// <param name="Role">
/// Message role: "system" (instructions), "user" (human input), or "assistant" (model output).
/// </param>
/// <param name="Content">The message text content.</param>
public record ChatMessage(string Role, string Content)
{
    /// <summary>Create a system instruction message.</summary>
    public static ChatMessage System(string content) => new("system", content);

    /// <summary>Create a user message.</summary>
    public static ChatMessage User(string content) => new("user", content);

    /// <summary>Create an assistant response message (for conversation history).</summary>
    public static ChatMessage Assistant(string content) => new("assistant", content);
}

/// <summary>
/// Response from a text generation or chat call.
/// </summary>
/// <param name="Content">The generated text content.</param>
/// <param name="TokensUsed">Total tokens consumed (prompt + completion), if reported by the provider.</param>
/// <param name="FinishReason">Why generation stopped: "stop", "length", or null if unknown.</param>
public record InferenceResponse(
    string Content,
    int? TokensUsed = null,
    string? FinishReason = null);
