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

    /// <summary>
    /// The template format to use for formatting chat messages.
    /// Default: "Phi3". Supported: "Phi3", "ChatML", "Phi4Mini".
    /// Only "Phi4Mini" honours <see cref="Tools"/> — the Phi-3 and ChatML templates ignore tool schemas
    /// because those model families don't understand the injected format.
    /// </summary>
    public string PromptTemplate { get; set; } = "Phi3";

    /// <summary>
    /// Definitions of tools/functions the model may call during generation.
    /// </summary>
    public IReadOnlyList<ToolSchema>? Tools { get; set; }
}

/// <summary>
/// Defines a tool (function) that the LLM is capable of calling.
/// </summary>
/// <param name="Name">Tool identifier (must match backend method name, e.g., "CreateEvent").</param>
/// <param name="Description">Human-readable explanation of when and how to use the tool.</param>
/// <param name="ParametersSchema">JSON schema representing the input arguments for the tool.</param>
public record ToolSchema(string Name, string Description, string ParametersSchema);

/// <summary>
/// Represents an invocation of a tool requested by the model.
/// </summary>
/// <param name="Id">A unique ID for this invocation instance.</param>
/// <param name="Name">The name of the tool to be invoked.</param>
/// <param name="Arguments">JSON string containing the tool parameters.</param>
public record FunctionCall(string Id, string Name, string Arguments);

/// <summary>
/// A message in a multi-turn chat conversation.
/// </summary>
/// <param name="Role">
/// Message role: "system" (instructions), "user" (human input), "assistant" (model output), or "tool" (tool execution result).
/// </param>
/// <param name="Content">The message text content or the raw result of a tool execution.</param>
/// <param name="ToolCallId">If this is a "tool" message, the ID linking this response to the function call.</param>
/// <param name="ToolName">If this is a "tool" message, the name of the tool that was executed.</param>
/// <param name="FunctionCalls">If this is an "assistant" message, any function calls made by the model in this turn.</param>
public record ChatMessage(
    string Role, 
    string Content, 
    string? ToolCallId = null, 
    string? ToolName = null, 
    IReadOnlyList<FunctionCall>? FunctionCalls = null)
{
    /// <summary>Create a system instruction message.</summary>
    public static ChatMessage System(string content) => new("system", content);

    /// <summary>Create a user message.</summary>
    public static ChatMessage User(string content) => new("user", content);

    /// <summary>Create an assistant response message.</summary>
    public static ChatMessage Assistant(string content, IReadOnlyList<FunctionCall>? functionCalls = null) 
        => new("assistant", content, FunctionCalls: functionCalls);

    /// <summary>Create a tool execution result message.</summary>
    public static ChatMessage Tool(string content, string toolCallId, string toolName) 
        => new("tool", content, toolCallId, toolName);
}

/// <summary>
/// Response from a text generation or chat call.
/// </summary>
/// <param name="Content">The generated text content.</param>
/// <param name="TokensUsed">Total tokens consumed (prompt + completion), if reported by the provider.</param>
/// <param name="FinishReason">Why generation stopped: "stop", "length", "tool_calls", or null if unknown.</param>
/// <param name="FunctionCalls">Any tool invocations requested by the model during this turn.</param>
public record InferenceResponse(
    string Content,
    int? TokensUsed = null,
    string? FinishReason = null,
    IReadOnlyList<FunctionCall>? FunctionCalls = null);
