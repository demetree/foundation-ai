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
    private readonly Model _model;
    private readonly Tokenizer _tokenizer;
    private readonly string _modelName;

    public string ModelName => _modelName;

    public OnnxInferenceProvider(string modelPath, string modelName = "onnx-genai-local")
    {
        _modelName = modelName;
        _model = new Model(modelPath);
        _tokenizer = new Tokenizer(_model);
    }

    public Task<InferenceResponse> GenerateAsync(string prompt, InferenceOptions? options = null, CancellationToken ct = default)
    {
        options ??= new InferenceOptions();
        
        string fullPrompt = prompt;
        // Only prepend SystemPrompt if using raw GenerateAsync (ChatAsync does this natively)
        if (!string.IsNullOrWhiteSpace(options.SystemPrompt) && !prompt.Contains("<|system|>") && !prompt.Contains("<|im_start|>system"))
        {
            fullPrompt = $"{options.SystemPrompt}\n{prompt}";
        }

        using var generatorParams = new GeneratorParams(_model);
        generatorParams.SetSearchOption("max_length", options.MaxTokens);
        generatorParams.SetSearchOption("temperature", options.Temperature);
        generatorParams.SetSearchOption("top_p", options.TopP);

        using var generator = new Generator(_model, generatorParams);
        
        using var sequences = _tokenizer.Encode(fullPrompt);
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
        options ??= new InferenceOptions();
        
        string fullPrompt = prompt;
        if (!string.IsNullOrWhiteSpace(options.SystemPrompt) && !prompt.Contains("<|system|>") && !prompt.Contains("<|im_start|>system"))
        {
            fullPrompt = $"{options.SystemPrompt}\n{prompt}";
        }

        using var generatorParams = new GeneratorParams(_model);
        generatorParams.SetSearchOption("max_length", options.MaxTokens);
        generatorParams.SetSearchOption("temperature", options.Temperature);
        generatorParams.SetSearchOption("top_p", options.TopP);

        using var generator = new Generator(_model, generatorParams);
        using var sequences = _tokenizer.Encode(fullPrompt);
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

    public Task<InferenceResponse> ChatAsync(IReadOnlyList<ChatMessage> messages, InferenceOptions? options = null, CancellationToken ct = default)
    {
        options ??= new InferenceOptions();
        string prompt = FormatChatPrompt(messages, options.PromptTemplate);
        return GenerateAsync(prompt, options, ct);
    }

    public IAsyncEnumerable<string> ChatStreamAsync(IReadOnlyList<ChatMessage> messages, InferenceOptions? options = null, CancellationToken ct = default)
    {
        options ??= new InferenceOptions();
        string prompt = FormatChatPrompt(messages, options.PromptTemplate);
        return GenerateStreamAsync(prompt, options, ct);
    }

    private string FormatChatPrompt(IReadOnlyList<ChatMessage> messages, string template)
    {
        var sb = new StringBuilder();
        
        if (string.Equals(template, "ChatML", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var msg in messages)
            {
                sb.AppendLine($"<|im_start|>{msg.Role}\n{msg.Content}<|im_end|>");
            }
            sb.AppendLine("<|im_start|>assistant");
        }
        else // Default to Phi-3 format
        {
            foreach (var msg in messages)
            {
                // Phi-3 format: <|system|>\n...<|end|>\n
                sb.AppendLine($"<|{msg.Role}|>\n{msg.Content}<|end|>");
            }
            sb.AppendLine("<|assistant|>");
        }
        
        return sb.ToString();
    }

    public ValueTask DisposeAsync()
    {
        _tokenizer?.Dispose();
        _model?.Dispose();
        return ValueTask.CompletedTask;
    }
}
