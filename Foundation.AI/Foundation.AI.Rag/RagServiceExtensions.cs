using Foundation.AI.Embed;
using Foundation.AI.Inference;
using Foundation.AI.VectorStore;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.Rag;

/// <summary>
/// Extension methods for registering Foundation.AI.Rag services.
/// </summary>
public static class RagServiceExtensions
{
    /// <summary>
    /// Register the RAG service with default configuration.
    /// Requires <see cref="IEmbeddingProvider"/>, <see cref="IVectorStore"/>,
    /// and <see cref="IInferenceProvider"/> to already be registered.
    ///
    /// <para><b>Usage:</b>
    /// <code>
    /// services.AddVectorStore(sp => new ZvecVectorStore(config));
    /// services.AddOnnxEmbedding(c => { c.ModelPath = "..."; });
    /// services.AddOpenAiInference(c => { c.ApiKey = "..."; });
    /// services.AddRag();  // ties them all together
    /// </code></para>
    /// </summary>
    public static IServiceCollection AddRag(this IServiceCollection services,
        Action<TextChunker>? configureChunker = null)
    {
        var chunker = new TextChunker();
        configureChunker?.Invoke(chunker);

        services.AddSingleton<IDocumentChunker>(chunker);
        services.AddSingleton<IRagService, RagService>();
        return services;
    }
}
