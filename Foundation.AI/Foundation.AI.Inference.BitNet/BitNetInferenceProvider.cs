using System.Runtime.CompilerServices;
using BitNet.Interop;

namespace Foundation.AI.Inference.BitNet;

/// <summary>
/// Local inference provider using Microsoft BitNet 1-bit LLM models.
///
/// <para><b>Key features:</b>
/// <list type="bullet">
/// <item>Fully local CPU inference — no API keys, no network required</item>
/// <item>1-bit quantized models for extreme efficiency</item>
/// <item>Streaming token-by-token generation</item>
/// <item>Chat template support for multi-turn conversations</item>
/// </list></para>
///
/// <para><b>Thread safety:</b>
/// The underlying llama.cpp context is not thread-safe for concurrent decode calls.
/// Concurrent requests are serialized via a SemaphoreSlim.</para>
/// </summary>
public sealed class BitNetInferenceProvider : IInferenceProvider
{
    private readonly BitNetModel _model;
    private readonly BitNetInferenceConfig _config;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    public string ModelName { get; }

    public BitNetInferenceProvider(BitNetInferenceConfig config)
    {
        _config = config;

        if (string.IsNullOrWhiteSpace(config.ModelPath))
            throw new ArgumentException("ModelPath must be specified.", nameof(config));

        var threads = config.Threads > 0 ? config.Threads : Environment.ProcessorCount;

        _model = new BitNetModel(config.ModelPath,
            new BitNetModelParams
            {
                GpuLayers = config.GpuLayers,
                UseMmap = config.UseMmap,
            },
            new BitNetContextParams
            {
                ContextSize = config.ContextSize,
                Threads = threads,
                BatchSize = config.BatchSize,
            });

        // Build a descriptive model name for logging
        var desc = _model.Description;
        ModelName = string.IsNullOrEmpty(desc)
            ? $"bitnet:{Path.GetFileNameWithoutExtension(config.ModelPath)}"
            : $"bitnet:{desc}";
    }

    // ─── IInferenceProvider ────────────────────────────────────────────

    public async Task<InferenceResponse> GenerateAsync(string prompt,
        InferenceOptions? options = null, CancellationToken ct = default)
    {
        options ??= new InferenceOptions();

        await _gate.WaitAsync(ct);
        try
        {
            ct.ThrowIfCancellationRequested();

            var effectivePrompt = BuildPromptWithSystem(prompt, options.SystemPrompt);
            var (temp, topK, topP, maxTokens) = MapOptions(options);

            var text = await Task.Run(() =>
                _model.Generate(effectivePrompt, maxTokens, temp, topK, topP), ct);

            var finishReason = text.Length > 0 ? "stop" : "length";

            return new InferenceResponse(
                Content: text,
                TokensUsed: null, // llama.cpp doesn't return exact usage in Generate
                FinishReason: finishReason);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async IAsyncEnumerable<string> GenerateStreamAsync(string prompt,
        InferenceOptions? options = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        options ??= new InferenceOptions();

        await _gate.WaitAsync(ct);
        try
        {
            var effectivePrompt = BuildPromptWithSystem(prompt, options.SystemPrompt);
            var (temp, topK, topP, maxTokens) = MapOptions(options);

            // GenerateTokens is synchronous + yields. We run it on a thread pool thread
            // and pipe tokens through a Channel-like pattern using async enumeration.
            var pieces = _model.GenerateTokens(effectivePrompt, maxTokens, temp, topK, topP);

            foreach (var piece in pieces)
            {
                ct.ThrowIfCancellationRequested();
                yield return piece;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<InferenceResponse> ChatAsync(IReadOnlyList<ChatMessage> messages,
        InferenceOptions? options = null, CancellationToken ct = default)
    {
        options ??= new InferenceOptions();
        var formattedPrompt = ApplyTemplate(messages, options);

        await _gate.WaitAsync(ct);
        try
        {
            ct.ThrowIfCancellationRequested();
            var (temp, topK, topP, maxTokens) = MapOptions(options);

            var text = await Task.Run(() =>
                _model.Generate(formattedPrompt, maxTokens, temp, topK, topP), ct);

            return new InferenceResponse(
                Content: text,
                FinishReason: "stop");
        }
        finally
        {
            _gate.Release();
        }
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(IReadOnlyList<ChatMessage> messages,
        InferenceOptions? options = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        options ??= new InferenceOptions();
        var formattedPrompt = ApplyTemplate(messages, options);

        await _gate.WaitAsync(ct);
        try
        {
            var (temp, topK, topP, maxTokens) = MapOptions(options);
            var pieces = _model.GenerateTokens(formattedPrompt, maxTokens, temp, topK, topP);

            foreach (var piece in pieces)
            {
                ct.ThrowIfCancellationRequested();
                yield return piece;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _model.Dispose();
            _gate.Dispose();
            _disposed = true;
        }
        return ValueTask.CompletedTask;
    }

    // ─── Private Helpers ───────────────────────────────────────────────

    private static string BuildPromptWithSystem(string prompt, string? systemPrompt)
    {
        if (string.IsNullOrWhiteSpace(systemPrompt))
            return prompt;

        return $"{systemPrompt}\n\n{prompt}";
    }

    private static (float temp, int topK, float topP, int maxTokens) MapOptions(InferenceOptions options)
    {
        var temp = options.Temperature;
        var topK = 40; // BitNet default; InferenceOptions doesn't expose top_k
        var topP = options.TopP;
        var maxTokens = options.MaxTokens;
        return (temp, topK, topP, maxTokens);
    }

    private string ApplyTemplate(IReadOnlyList<ChatMessage> messages, InferenceOptions options)
    {
        // Map Foundation.AI.Inference.ChatMessage → BitNet.Interop.BitNetChatMessage
        var bitnetMessages = new List<BitNetChatMessage>(messages.Count + 1);

        // Inject system prompt from options if not already present
        if (!string.IsNullOrWhiteSpace(options.SystemPrompt) &&
            !messages.Any(m => m.Role == "system"))
        {
            bitnetMessages.Add(new BitNetChatMessage("system", options.SystemPrompt));
        }

        foreach (var msg in messages)
            bitnetMessages.Add(new BitNetChatMessage(msg.Role, msg.Content));

        return _model.ApplyChatTemplate(bitnetMessages);
    }
}
