using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.Embed;

/// <summary>
/// Extension methods for registering Foundation.AI.Embed services.
/// </summary>
public static class EmbedServiceExtensions
{
    /// <summary>
    /// Register an <see cref="IEmbeddingProvider"/> implementation as a singleton.
    ///
    /// <para><b>Usage:</b>
    /// <code>
    /// services.AddEmbeddingProvider&lt;OnnxEmbeddingProvider&gt;();
    /// </code></para>
    /// </summary>
    public static IServiceCollection AddEmbeddingProvider<TProvider>(
        this IServiceCollection services)
        where TProvider : class, IEmbeddingProvider
    {
        services.AddSingleton<IEmbeddingProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Register an <see cref="IEmbeddingProvider"/> implementation with a factory.
    ///
    /// <para><b>Usage:</b>
    /// <code>
    /// services.AddEmbeddingProvider(sp => new OnnxEmbeddingProvider(new OnnxEmbeddingConfig
    /// {
    ///     ModelPath = "./ai-models/all-MiniLM-L6-v2.onnx",
    ///     UseCuda = true
    /// }));
    /// </code></para>
    /// </summary>
    public static IServiceCollection AddEmbeddingProvider(
        this IServiceCollection services,
        Func<IServiceProvider, IEmbeddingProvider> factory)
    {
        services.AddSingleton(factory);
        return services;
    }

    /// <summary>
    /// Register an ONNX embedding provider with configuration.
    ///
    /// <para><b>Usage:</b>
    /// <code>
    /// services.AddOnnxEmbedding(config =>
    /// {
    ///     config.ModelPath = "./ai-models/all-MiniLM-L6-v2.onnx";
    ///     config.UseCuda = true;
    ///     config.GpuDeviceId = 0;
    /// });
    /// </code></para>
    /// </summary>
    public static IServiceCollection AddOnnxEmbedding(
        this IServiceCollection services,
        Action<OnnxEmbeddingConfig> configure)
    {
        var config = new OnnxEmbeddingConfig();
        configure(config);

        services.AddSingleton(config);
        services.AddSingleton<IEmbeddingProvider, OnnxEmbeddingProvider>();
        return services;
    }
}
