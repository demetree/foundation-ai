using Foundation.AI.Embed;
using Foundation.AI.Inference;
using Foundation.AI.Rag;
using Foundation.AI.VectorStore;
using Foundation.AI.Vision;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI;

/// <summary>
/// Unified entry point for configuring all Foundation.AI services.
///
/// <para><b>Usage:</b>
/// <code>
/// services.AddFoundationAI(ai =&gt; {
///     // Vector storage
///     ai.Services.AddVectorStore(sp =&gt; new ZvecVectorStore(config));
///
///     // Embeddings (local ONNX)
///     ai.Services.AddOnnxEmbedding(c =&gt; {
///         c.ModelPath = "./ai-models/bge-small.onnx";
///         c.UseCuda = true;
///     });
///
///     // LLM inference (Ollama local)
///     ai.Services.AddOpenAiInference(c =&gt; {
///         c.Endpoint = "http://localhost:11434/v1/chat/completions";
///         c.Model = "llama3";
///         c.ApiKey = "ollama";
///     });
///
///     // Vision (GPT-4o or Ollama LLaVA)
///     ai.Services.AddOpenAiVision(c =&gt; {
///         c.ApiKey = "sk-...";
///         c.Model = "gpt-4o";
///     });
///
///     // RAG orchestration (wires embed + vectorstore + inference)
///     ai.Services.AddRag(chunker =&gt; {
///         chunker.ChunkSize = 1000;
///         chunker.ChunkOverlap = 200;
///     });
/// });
/// </code></para>
/// </summary>
public static class FoundationAIServiceExtensions
{
    /// <summary>
    /// Add Foundation.AI services to the application.
    /// Use the builder to configure specific AI capabilities.
    /// </summary>
    public static IServiceCollection AddFoundationAI(
        this IServiceCollection services,
        Action<FoundationAIBuilder> configure)
    {
        var builder = new FoundationAIBuilder(services);
        configure(builder);
        return services;
    }
}

/// <summary>
/// Builder for configuring Foundation.AI services.
/// Exposes the <see cref="IServiceCollection"/> for registering individual providers.
/// </summary>
public sealed class FoundationAIBuilder
{
    /// <summary>
    /// The service collection to register AI services with.
    /// Use extension methods from individual packages:
    /// <list type="bullet">
    /// <item><c>AddVectorStore()</c> — from Foundation.AI.VectorStore</item>
    /// <item><c>AddOnnxEmbedding()</c> / <c>AddEmbeddingProvider()</c> — from Foundation.AI.Embed</item>
    /// <item><c>AddOpenAiInference()</c> / <c>AddInferenceProvider()</c> — from Foundation.AI.Inference</item>
    /// <item><c>AddBitNetInference()</c> — from Foundation.AI.Inference.BitNet (local 1-bit LLM)</item>
    /// <item><c>AddOpenAiVision()</c> / <c>AddVisionProvider()</c> — from Foundation.AI.Vision</item>
    /// <item><c>AddRag()</c> — from Foundation.AI.Rag</item>
    /// </list>
    /// </summary>
    public IServiceCollection Services { get; }

    internal FoundationAIBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
