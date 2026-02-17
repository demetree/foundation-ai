using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.Vision;

/// <summary>
/// Extension methods for registering Foundation.AI.Vision services.
/// </summary>
public static class VisionServiceExtensions
{
    /// <summary>
    /// Register an <see cref="IVisionProvider"/> implementation as a singleton.
    /// </summary>
    public static IServiceCollection AddVisionProvider<TProvider>(
        this IServiceCollection services)
        where TProvider : class, IVisionProvider
    {
        services.AddSingleton<IVisionProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Register an <see cref="IVisionProvider"/> implementation with a factory.
    /// </summary>
    public static IServiceCollection AddVisionProvider(
        this IServiceCollection services,
        Func<IServiceProvider, IVisionProvider> factory)
    {
        services.AddSingleton(factory);
        return services;
    }

    /// <summary>
    /// Register an OpenAI-compatible vision provider with configuration.
    /// Works with OpenAI (GPT-4o), Azure OpenAI, and Ollama (LLaVA, etc.).
    ///
    /// <para><b>OpenAI:</b>
    /// <code>
    /// services.AddOpenAiVision(c => {
    ///     c.ApiKey = "sk-...";
    ///     c.Model = "gpt-4o";
    /// });
    /// </code></para>
    ///
    /// <para><b>Ollama (local LLaVA):</b>
    /// <code>
    /// services.AddOpenAiVision(c => {
    ///     c.Endpoint = "http://localhost:11434/v1/chat/completions";
    ///     c.Model = "llava";
    ///     c.ApiKey = "ollama";
    /// });
    /// </code></para>
    /// </summary>
    public static IServiceCollection AddOpenAiVision(
        this IServiceCollection services,
        Action<OpenAiVisionConfig> configure)
    {
        var config = new OpenAiVisionConfig();
        configure(config);

        services.AddSingleton(config);
        services.AddSingleton<IVisionProvider, OpenAiVisionProvider>();
        return services;
    }
}
