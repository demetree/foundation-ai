using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.Inference;

/// <summary>
/// Extension methods for registering Foundation.AI.Inference services.
/// </summary>
public static class InferenceServiceExtensions
{
    /// <summary>
    /// Register an <see cref="IInferenceProvider"/> implementation as a singleton.
    /// </summary>
    public static IServiceCollection AddInferenceProvider<TProvider>(
        this IServiceCollection services)
        where TProvider : class, IInferenceProvider
    {
        services.AddSingleton<IInferenceProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Register an <see cref="IInferenceProvider"/> implementation with a factory.
    /// </summary>
    public static IServiceCollection AddInferenceProvider(
        this IServiceCollection services,
        Func<IServiceProvider, IInferenceProvider> factory)
    {
        services.AddSingleton(factory);
        return services;
    }

    /// <summary>
    /// Register an OpenAI-compatible inference provider with configuration.
    /// Works with OpenAI, Azure OpenAI, and Ollama.
    ///
    /// <para><b>OpenAI:</b>
    /// <code>
    /// services.AddOpenAiInference(c => {
    ///     c.ApiKey = "sk-...";
    ///     c.Model = "gpt-4o-mini";
    /// });
    /// </code></para>
    ///
    /// <para><b>Ollama (local):</b>
    /// <code>
    /// services.AddOpenAiInference(c => {
    ///     c.Endpoint = "http://localhost:11434/v1/chat/completions";
    ///     c.Model = "llama3";
    ///     c.ApiKey = "ollama";
    /// });
    /// </code></para>
    /// </summary>
    public static IServiceCollection AddOpenAiInference(
        this IServiceCollection services,
        Action<OpenAiInferenceConfig> configure)
    {
        var config = new OpenAiInferenceConfig();
        configure(config);

        services.AddSingleton(config);
        services.AddSingleton<IInferenceProvider, OpenAiInferenceProvider>();
        return services;
    }
}
