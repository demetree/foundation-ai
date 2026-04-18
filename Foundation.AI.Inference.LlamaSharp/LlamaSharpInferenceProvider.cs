using System.Runtime.CompilerServices;
using System.Text;
using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Sampling;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.AI.Inference.LlamaSharp;

/// <summary>
/// Local inference provider backed by LLamaSharp (llama.cpp wrapper). Loads any GGUF
/// model — Qwen3, Phi-4, Llama-3, etc. — and speaks the same <see cref="IInferenceProvider"/>
/// shape as the OpenAI / ONNX / BitNet providers so agent code can swap between them
/// without branches.
///
/// <para><b>Threading:</b> llama.cpp's context is not safe for concurrent decode on a single
/// executor. All calls are serialized through a <see cref="SemaphoreSlim"/>. For high
/// concurrency, run multiple provider instances with separate contexts.</para>
///
/// <para><b>KV-cache reuse:</b> v1 uses <see cref="StatelessExecutor"/> — each call re-prefills
/// the full prompt. This matches the provider abstraction (every <c>ChatAsync</c> call carries
/// the whole history, so the provider can't know which KV state to continue). A later revision
/// can add prompt-prefix caching keyed by the leading token stream.</para>
///
/// <para><b>Backend selection:</b> The native backend package (CPU vs CUDA vs Vulkan) is
/// chosen by the consuming application's NuGet references, not by this provider.
/// <see cref="LlamaSharpInferenceConfig.GpuLayerCount"/> only decides how much to offload
/// at runtime — when CUDA isn't available, LLamaSharp logs a warning and decodes on CPU.</para>
/// </summary>
public sealed class LlamaSharpInferenceProvider : IInferenceProvider
{
    private readonly LLamaWeights _weights;
    private readonly ModelParams _modelParams;
    private readonly StatelessExecutor _executor;
    private readonly LlamaSharpInferenceConfig _config;
    private readonly ILogger<LlamaSharpInferenceProvider> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    public string ModelName { get; }

    public LlamaSharpInferenceProvider(
        LlamaSharpInferenceConfig config,
        ILogger<LlamaSharpInferenceProvider>? logger = null)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(config.ModelPath))
            throw new ArgumentException("ModelPath must be specified.", nameof(config));
        if (!File.Exists(config.ModelPath))
            throw new FileNotFoundException($"GGUF model not found at '{config.ModelPath}'.", config.ModelPath);

        _config = config;
        _logger = logger ?? NullLogger<LlamaSharpInferenceProvider>.Instance;

        RouteNativeLogs(_logger);

        _modelParams = new ModelParams(config.ModelPath)
        {
            ContextSize = config.ContextSize,
            GpuLayerCount = config.GpuLayerCount,
            Threads = config.Threads > 0 ? config.Threads : Environment.ProcessorCount,
            BatchSize = config.BatchSize,
        };

        _logger.LogInformation("Loading GGUF model: {Path}", config.ModelPath);
        _weights = LLamaWeights.LoadFromFileAsync(_modelParams).GetAwaiter().GetResult();
        _executor = new StatelessExecutor(_weights, _modelParams);

        ModelName = !string.IsNullOrWhiteSpace(config.ModelName)
            ? config.ModelName!
            : $"llamasharp:{Path.GetFileNameWithoutExtension(config.ModelPath)}";

        _logger.LogInformation("LLamaSharp provider ready: {ModelName}", ModelName);
    }

    // ─── IInferenceProvider ────────────────────────────────────────────

    public async Task<InferenceResponse> GenerateAsync(string prompt,
        InferenceOptions? options = null, CancellationToken ct = default)
    {
        options ??= new InferenceOptions();
        var effectivePrompt = BuildPromptWithSystem(prompt, options.SystemPrompt);

        await _gate.WaitAsync(ct);
        try
        {
            var (text, tokenCount) = await RunInferenceAsync(effectivePrompt, options, ct).ConfigureAwait(false);
            return new InferenceResponse(
                Content: text,
                TokensUsed: tokenCount,
                FinishReason: "stop");
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
        var effectivePrompt = BuildPromptWithSystem(prompt, options.SystemPrompt);

        await _gate.WaitAsync(ct);
        try
        {
            var inferenceParams = BuildInferenceParams(options);
            await foreach (var token in _executor.InferAsync(effectivePrompt, inferenceParams, ct).ConfigureAwait(false))
            {
                ct.ThrowIfCancellationRequested();
                yield return token;
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
        var template = ResolveTemplate(options);
        var prompt = ChatTemplates.Render(messages, template, options.Tools);

        await _gate.WaitAsync(ct);
        try
        {
            var (text, tokenCount) = await RunInferenceAsync(prompt, options, ct).ConfigureAwait(false);
            var baseResponse = new InferenceResponse(text, tokenCount, FinishReason: "stop");
            return PostProcessForToolCalls(baseResponse, template, options);
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
        var template = ResolveTemplate(options);
        var prompt = ChatTemplates.Render(messages, template, options.Tools);

        await _gate.WaitAsync(ct);
        try
        {
            var inferenceParams = BuildInferenceParams(options);
            await foreach (var token in _executor.InferAsync(prompt, inferenceParams, ct).ConfigureAwait(false))
            {
                ct.ThrowIfCancellationRequested();
                yield return token;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed) return ValueTask.CompletedTask;
        _disposed = true;
        _weights.Dispose();
        _gate.Dispose();
        return ValueTask.CompletedTask;
    }

    // ─── Private Helpers ───────────────────────────────────────────────

    private async Task<(string Text, int TokenCount)> RunInferenceAsync(
        string prompt, InferenceOptions options, CancellationToken ct)
    {
        var inferenceParams = BuildInferenceParams(options);
        var sb = new StringBuilder();
        int tokenCount = 0;

        await foreach (var token in _executor.InferAsync(prompt, inferenceParams, ct).ConfigureAwait(false))
        {
            sb.Append(token);
            tokenCount++;
        }
        return (sb.ToString(), tokenCount);
    }

    private static InferenceParams BuildInferenceParams(InferenceOptions options)
    {
        // Stop at every model's end-of-turn sentinel — cheap defensive list covering the
        // families this provider is most commonly pointed at. Caller stop sequences are
        // additive.
        var antiPrompts = new List<string>
        {
            "<|im_end|>",       // ChatML / Qwen3
            "<|endoftext|>",    // GPT-2 tokenizer families, Qwen3 fallback
            "<|end|>",          // Phi-3 / Phi-4-mini
            "<|eot_id|>",       // Llama-3
        };
        if (options.StopSequences is { Count: > 0 })
        {
            foreach (var s in options.StopSequences)
            {
                if (!string.IsNullOrEmpty(s)) antiPrompts.Add(s);
            }
        }

        return new InferenceParams
        {
            MaxTokens = options.MaxTokens,
            AntiPrompts = antiPrompts,
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = options.Temperature,
                TopP = options.TopP,
                RepeatPenalty = options.RepetitionPenalty,
            },
        };
    }

    private string ResolveTemplate(InferenceOptions options)
    {
        // InferenceOptions defaults PromptTemplate to "Phi3". That default only makes sense
        // for Phi-3 ONNX models; for a LlamaSharp provider the model family is decided by the
        // GGUF we loaded. If the caller didn't override, use the config default.
        if (string.IsNullOrEmpty(options.PromptTemplate) ||
            string.Equals(options.PromptTemplate, ChatTemplates.Phi3, StringComparison.OrdinalIgnoreCase))
        {
            return _config.DefaultPromptTemplate;
        }
        return options.PromptTemplate;
    }

    /// <summary>
    /// When the rendered template was tool-aware and tools were offered, lift any embedded
    /// tool-call spans out of the raw output and into <see cref="InferenceResponse.FunctionCalls"/>,
    /// matching the shape <c>OpenAiInferenceProvider</c> already returns.
    /// </summary>
    private static InferenceResponse PostProcessForToolCalls(
        InferenceResponse response, string template, InferenceOptions options)
    {
        if (options.Tools is null || options.Tools.Count == 0) return response;

        ToolCallExtractionResult? extraction = null;

        if (ChatTemplates.IsQwen3(template))
            extraction = Qwen3ToolCallExtractor.Extract(response.Content);
        else if (ChatTemplates.IsPhi4Mini(template))
            extraction = Phi4MiniToolCallExtractor.Extract(response.Content);

        if (extraction is null || extraction.Calls.Count == 0) return response;

        return new InferenceResponse(
            Content: extraction.Prose,
            TokensUsed: response.TokensUsed,
            FinishReason: "tool_calls",
            FunctionCalls: extraction.Calls);
    }

    private static string BuildPromptWithSystem(string prompt, string? systemPrompt) =>
        string.IsNullOrWhiteSpace(systemPrompt) ? prompt : $"{systemPrompt}\n\n{prompt}";

    // ─── Native log routing ────────────────────────────────────────────

    private static int s_logsRouted;

    /// <summary>
    /// Route llama.cpp's native <c>stderr</c>-bound log stream through the managed
    /// <see cref="ILogger"/>. Runs once per process — subsequent calls are no-ops.
    /// </summary>
    private static void RouteNativeLogs(ILogger logger)
    {
        if (Interlocked.Exchange(ref s_logsRouted, 1) != 0) return;

        try
        {
            NativeLogConfig.llama_log_set((level, message) =>
            {
                var trimmed = message?.TrimEnd('\r', '\n');
                if (string.IsNullOrEmpty(trimmed)) return;

                var severity = level switch
                {
                    LLamaLogLevel.Error => LogLevel.Error,
                    LLamaLogLevel.Warning => LogLevel.Warning,
                    LLamaLogLevel.Info => LogLevel.Information,
                    LLamaLogLevel.Debug => LogLevel.Debug,
                    _ => LogLevel.Trace,
                };
                logger.Log(severity, "[llama.cpp] {Message}", trimmed);
            });
        }
        catch (Exception ex)
        {
            // Logging hookup must never break inference — surface once at Debug and move on.
            logger.LogDebug(ex, "Native log routing unavailable; llama.cpp will use its default stderr sink.");
        }
    }
}
