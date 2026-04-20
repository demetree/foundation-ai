using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace Foundation.AI.Inference.Onnx;

public class OnnxInferenceProvider : IInferenceProvider
{
    private readonly string _modelPath;
    private readonly string _modelName;
    private Model _model;
    private Tokenizer _tokenizer;
    private readonly object _initLock = new();
    private bool _initialized;

    public string ModelName => _modelName;

    public OnnxInferenceProvider(string modelPath, string modelName = "onnx-genai-local")
    {
        _modelPath = modelPath;
        _modelName = modelName;
    }

    // Deferred so DI construction succeeds even before model files are on disk.
    // OnnxModelDownloadWorker downloads the files and then calls Preload() to
    // load the ONNX session into RAM before the first user request arrives.
    private void EnsureInitialized()
    {
        if (_initialized) return;
        lock (_initLock)
        {
            if (_initialized) return;
            _model = new Model(_modelPath);
            _tokenizer = new Tokenizer(_model);
            _initialized = true;
        }
    }

    public void Preload() => EnsureInitialized();

    public Task<InferenceResponse> GenerateAsync(string prompt, InferenceOptions? options = null, CancellationToken ct = default)
    {
        EnsureInitialized();
        options ??= new InferenceOptions();

        string fullPrompt = prompt;
        // Only prepend SystemPrompt if using raw GenerateAsync (ChatAsync does this natively)
        if (!string.IsNullOrWhiteSpace(options.SystemPrompt) && !prompt.Contains("<|system|>") && !prompt.Contains("<|im_start|>system"))
        {
            fullPrompt = $"{options.SystemPrompt}\n{prompt}";
        }

        using var sequences = _tokenizer.Encode(fullPrompt);
        int inputTokenCount = sequences[0].Length;

        using var generatorParams = new GeneratorParams(_model);
        // ONNX GenAI's max_length is TOTAL length (input + output). InferenceOptions.MaxTokens is
        // documented as "tokens to generate in the response" — honour that by adding input length.
        generatorParams.SetSearchOption("max_length", inputTokenCount + options.MaxTokens);
        generatorParams.SetSearchOption("temperature", options.Temperature);
        generatorParams.SetSearchOption("top_p", options.TopP);
        if (options.RepetitionPenalty > 1.0f)
            generatorParams.SetSearchOption("repetition_penalty", options.RepetitionPenalty);

        using var generator = new Generator(_model, generatorParams);
        generator.AppendTokenSequences(sequences);

        var resultBuilder = new StringBuilder();

        while (!generator.IsDone())
        {
            ct.ThrowIfCancellationRequested();
            generator.GenerateNextToken();
            var tokenIds = generator.GetSequence(0);
            int lastTokenId = tokenIds[tokenIds.Length - 1];
            resultBuilder.Append(_tokenizer.Decode(new[] { lastTokenId }));
        }

        return Task.FromResult(new InferenceResponse(resultBuilder.ToString()));
    }

    public async IAsyncEnumerable<string> GenerateStreamAsync(string prompt, InferenceOptions? options = null, [EnumeratorCancellation] CancellationToken ct = default)
    {
        EnsureInitialized();
        options ??= new InferenceOptions();

        string fullPrompt = prompt;
        if (!string.IsNullOrWhiteSpace(options.SystemPrompt) && !prompt.Contains("<|system|>") && !prompt.Contains("<|im_start|>system"))
        {
            fullPrompt = $"{options.SystemPrompt}\n{prompt}";
        }

        using var sequences = _tokenizer.Encode(fullPrompt);
        int inputTokenCount = sequences[0].Length;

        using var generatorParams = new GeneratorParams(_model);
        generatorParams.SetSearchOption("max_length", inputTokenCount + options.MaxTokens);
        generatorParams.SetSearchOption("temperature", options.Temperature);
        generatorParams.SetSearchOption("top_p", options.TopP);
        if (options.RepetitionPenalty > 1.0f)
            generatorParams.SetSearchOption("repetition_penalty", options.RepetitionPenalty);

        using var generator = new Generator(_model, generatorParams);
        generator.AppendTokenSequences(sequences);

        while (!generator.IsDone())
        {
            ct.ThrowIfCancellationRequested();
            generator.GenerateNextToken();
            ct.ThrowIfCancellationRequested();
            
            var tokenIds = generator.GetSequence(0);
            int lastTokenId = tokenIds[tokenIds.Length - 1];
            string tokenStr = _tokenizer.Decode(new[] { lastTokenId });
            yield return tokenStr;
            await Task.Yield(); 
        }
    }

    public async Task<InferenceResponse> ChatAsync(IReadOnlyList<ChatMessage> messages, InferenceOptions? options = null, CancellationToken ct = default)
    {
        options ??= new InferenceOptions();
        string prompt = ChatTemplates.Render(messages, options.PromptTemplate, options.Tools);
        var response = await GenerateAsync(prompt, options, ct);
        return PostProcessForToolCalls(response, options);
    }

    /// <summary>
    /// When the Phi-4-mini template was used and tools were offered, scan the raw model output
    /// for an embedded tool-call JSON blob and, if present, lift it into
    /// <see cref="InferenceResponse.FunctionCalls"/> with <c>FinishReason = "tool_calls"</c>.
    /// This matches the shape <see cref="OpenAiInferenceProvider"/> already returns, so agent
    /// loops don't need a code path per provider.
    /// </summary>
    private static InferenceResponse PostProcessForToolCalls(InferenceResponse response, InferenceOptions options)
    {
        if (!ChatTemplates.IsPhi4Mini(options.PromptTemplate)) return response;
        if (options.Tools is null || options.Tools.Count == 0) return response;

        var extraction = Phi4MiniToolCallExtractor.Extract(response.Content);
        if (extraction.Calls.Count == 0) return response;

        return new InferenceResponse(
            Content: extraction.Prose,
            TokensUsed: response.TokensUsed,
            FinishReason: "tool_calls",
            FunctionCalls: extraction.Calls);
    }

    public IAsyncEnumerable<string> ChatStreamAsync(IReadOnlyList<ChatMessage> messages, InferenceOptions? options = null, CancellationToken ct = default)
    {
        options ??= new InferenceOptions();
        string prompt = ChatTemplates.Render(messages, options.PromptTemplate, options.Tools);
        return GenerateStreamAsync(prompt, options, ct);
    }

    public ValueTask DisposeAsync()
    {
        _tokenizer?.Dispose();
        _model?.Dispose();
        return ValueTask.CompletedTask;
    }
}
