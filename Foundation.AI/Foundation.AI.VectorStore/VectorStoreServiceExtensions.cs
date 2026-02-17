using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.VectorStore;

/// <summary>
/// Extension methods for registering Foundation.AI.VectorStore services.
/// </summary>
public static class VectorStoreServiceExtensions
{
    /// <summary>
    /// Register an <see cref="IVectorStore"/> implementation as a singleton.
    ///
    /// <para><b>Usage:</b>
    /// <code>
    /// services.AddVectorStore&lt;ZvecVectorStore&gt;();
    /// </code></para>
    /// </summary>
    public static IServiceCollection AddVectorStore<TStore>(
        this IServiceCollection services)
        where TStore : class, IVectorStore
    {
        services.AddSingleton<IVectorStore, TStore>();
        return services;
    }

    /// <summary>
    /// Register an <see cref="IVectorStore"/> implementation with a factory.
    ///
    /// <para><b>Usage:</b>
    /// <code>
    /// services.AddVectorStore(sp => new ZvecVectorStore(new ZvecVectorStoreConfig
    /// {
    ///     BasePath = "./ai-data/vectors"
    /// }));
    /// </code></para>
    /// </summary>
    public static IServiceCollection AddVectorStore(
        this IServiceCollection services,
        Func<IServiceProvider, IVectorStore> factory)
    {
        services.AddSingleton(factory);
        return services;
    }
}
