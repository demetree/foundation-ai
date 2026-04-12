using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foundation.AI.Inference;

/// <summary>
/// Inference provider using the OpenAI Chat Completions API.
///
/// <para><b>Compatible with:</b>
/// <list type="bullet">
/// <item><b>OpenAI</b> — GPT-4o, GPT-4o-mini, etc.</item>
/// <item><b>Azure OpenAI</b> — Same models via Azure deployment</item>
/// <item><b>Ollama</b> — Llama 3, Mistral, Phi-3, Qwen, etc. via OpenAI-compatible API</item>
/// <item><b>Any OpenAI-compatible endpoint</b> — LM Studio, vLLM, etc.</item>
/// </list></para>
///
/// <para><b>Features:</b>
/// <list type="bullet">
/// <item>Streaming via Server-Sent Events (SSE)</item>
/// <item>Tool/function calling (send tool definitions, parse tool_calls from responses)</item>
/// <item>Multi-turn chat with system, user, assistant, and tool messages</item>
/// </list></para>
/// </summary>
public sealed class OpenAiInferenceProvider : IInferenceProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiInferenceConfig _config;

    public string ModelName => $"openai:{_config.Model}";

    public OpenAiInferenceProvider(OpenAiInferenceConfig config)
    {
        _config = config;

        _httpClient = new HttpClient { Timeout = config.Timeout };

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            if (config.UseAzureAuth)
                _httpClient.DefaultRequestHeaders.Add("api-key", config.ApiKey);
            else
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ApiKey);
        }
    }

    public async Task<InferenceResponse> GenerateAsync(string prompt,
        InferenceOptions? options = null, CancellationToken ct = default)
    {
        var messages = BuildMessages(prompt, options?.SystemPrompt);
        return await ChatAsync(messages, options, ct);
    }

    public IAsyncEnumerable<string> GenerateStreamAsync(string prompt,
        InferenceOptions? options = null, CancellationToken ct = default)
    {
        var messages = BuildMessages(prompt, options?.SystemPrompt);
        return ChatStreamAsync(messages, options, ct);
    }

    public async Task<InferenceResponse> ChatAsync(IReadOnlyList<ChatMessage> messages,
        InferenceOptions? options = null, CancellationToken ct = default)
    {
        options ??= new InferenceOptions();
        var request = BuildRequest(messages, options, stream: false);

        var response = await _httpClient.PostAsJsonAsync(
            _config.Endpoint, request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(
            JsonOptions, ct);

        var choice = result?.Choices?.FirstOrDefault();
        var msg = choice?.Message;

        // Parse tool calls from the response if present
        IReadOnlyList<FunctionCall>? functionCalls = null;
        if (msg?.ToolCalls is { Count: > 0 })
        {
            functionCalls = msg.ToolCalls
                .Where(tc => tc.Type == "function" && tc.Function != null)
                .Select(tc => new FunctionCall(
                    Id: tc.Id ?? Guid.NewGuid().ToString(),
                    Name: tc.Function!.Name ?? "",
                    Arguments: tc.Function.Arguments ?? "{}"))
                .ToList();

            if (functionCalls.Count == 0)
                functionCalls = null;
        }

        var finishReason = choice?.FinishReason;
        // Normalize: OpenAI uses "tool_calls" as finish reason when tools are invoked
        if (finishReason == "tool_calls" && functionCalls == null)
            finishReason = "stop";

        return new InferenceResponse(
            Content: msg?.Content ?? "",
            TokensUsed: result?.Usage?.TotalTokens,
            FinishReason: finishReason,
            FunctionCalls: functionCalls);
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(IReadOnlyList<ChatMessage> messages,
        InferenceOptions? options = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        options ??= new InferenceOptions();
        var request = BuildRequest(messages, options, stream: true);

        var jsonBody = JsonSerializer.Serialize(request, JsonOptions);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _config.Endpoint)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(
            httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!line.StartsWith("data: "))
                continue;

            var data = line["data: ".Length..];

            if (data == "[DONE]")
                yield break;

            string? delta = null;
            try
            {
                var chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(data, JsonOptions);
                delta = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
            }
            catch (JsonException)
            {
                // Skip malformed chunks
            }

            if (!string.IsNullOrEmpty(delta))
                yield return delta;
        }
    }

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }

    // ─── Request Building ───────────────────────────────────────────

    private static IReadOnlyList<ChatMessage> BuildMessages(string prompt, string? systemPrompt)
    {
        var messages = new List<ChatMessage>();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(ChatMessage.System(systemPrompt));
        messages.Add(ChatMessage.User(prompt));
        return messages;
    }

    private ChatCompletionRequest BuildRequest(IReadOnlyList<ChatMessage> messages,
        InferenceOptions options, bool stream)
    {
        var apiMessages = new List<ApiMessage>();

        // Prepend system prompt if specified in options and not already in messages
        if (!string.IsNullOrWhiteSpace(options.SystemPrompt) &&
            !messages.Any(m => m.Role == "system"))
        {
            apiMessages.Add(new ApiMessage { Role = "system", Content = options.SystemPrompt });
        }

        foreach (var msg in messages)
        {
            var apiMsg = new ApiMessage { Role = msg.Role, Content = msg.Content };

            // Tool result messages need tool_call_id
            if (msg.Role == "tool" && msg.ToolCallId != null)
            {
                apiMsg.ToolCallId = msg.ToolCallId;
            }

            // Assistant messages with function calls need tool_calls array
            if (msg.Role == "assistant" && msg.FunctionCalls is { Count: > 0 })
            {
                apiMsg.ToolCalls = msg.FunctionCalls
                    .Select(fc => new ApiToolCall
                    {
                        Id = fc.Id,
                        Type = "function",
                        Function = new ApiToolCallFunction
                        {
                            Name = fc.Name,
                            Arguments = fc.Arguments
                        }
                    })
                    .ToList();
            }

            apiMessages.Add(apiMsg);
        }

        var request = new ChatCompletionRequest
        {
            Model = _config.Model,
            Messages = apiMessages,
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens,
            TopP = options.TopP,
            Stream = stream
        };

        if (options.StopSequences is { Count: > 0 })
            request.Stop = options.StopSequences.ToList();

        // Add tool definitions if provided
        if (options.Tools is { Count: > 0 })
        {
            request.Tools = options.Tools
                .Select(t => new ApiToolDefinition
                {
                    Type = "function",
                    Function = new ApiToolFunction
                    {
                        Name = t.Name,
                        Description = t.Description,
                        Parameters = JsonSerializer.Deserialize<JsonElement>(t.ParametersSchema)
                    }
                })
                .ToList();
        }

        return request;
    }

    // ─── JSON Contracts ─────────────────────────────────────────────

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // ── Request types ──

    private sealed class ChatCompletionRequest
    {
        public string Model { get; set; } = "";
        public List<ApiMessage> Messages { get; set; } = [];
        public float? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public float? TopP { get; set; }
        public bool? Stream { get; set; }
        public List<string>? Stop { get; set; }
        public List<ApiToolDefinition>? Tools { get; set; }
    }

    private sealed class ApiMessage
    {
        public string Role { get; set; } = "";
        public string? Content { get; set; }
        public string? ToolCallId { get; set; }
        public List<ApiToolCall>? ToolCalls { get; set; }
    }

    private sealed class ApiToolDefinition
    {
        public string Type { get; set; } = "function";
        public ApiToolFunction? Function { get; set; }
    }

    private sealed class ApiToolFunction
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public JsonElement? Parameters { get; set; }
    }

    private sealed class ApiToolCall
    {
        public string? Id { get; set; }
        public string Type { get; set; } = "function";
        public ApiToolCallFunction? Function { get; set; }
    }

    private sealed class ApiToolCallFunction
    {
        public string? Name { get; set; }
        public string? Arguments { get; set; }
    }

    // ── Response types (non-streaming) ──

    private sealed class ChatCompletionResponse
    {
        public List<ChatChoice>? Choices { get; set; }
        public UsageInfo? Usage { get; set; }
    }

    private sealed class ChatChoice
    {
        public ResponseMessage? Message { get; set; }
        public string? FinishReason { get; set; }
    }

    private sealed class ResponseMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
        public List<ApiToolCall>? ToolCalls { get; set; }
    }

    private sealed class UsageInfo
    {
        public int? TotalTokens { get; set; }
    }

    // ─�� Response types (streaming) ──

    private sealed class ChatCompletionChunk
    {
        public List<StreamChoice>? Choices { get; set; }
    }

    private sealed class StreamChoice
    {
        public StreamDelta? Delta { get; set; }
    }

    private sealed class StreamDelta
    {
        public string? Content { get; set; }
    }
}
