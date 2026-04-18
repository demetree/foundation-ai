using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.Inference.LlamaSharp;

/// <summary>
/// Extension methods for registering the LLamaSharp local inference provider with DI.
/// </summary>
public static class LlamaSharpInferenceExtensions
{
    /// <summary>
    /// Register <see cref="LlamaSharpInferenceProvider"/> as the singleton
    /// <see cref="IInferenceProvider"/> implementation.
    ///
    /// <para><b>Example (CPU-only, Qwen3):</b>
    /// <code>
    /// services.AddLlamaSharpInference(c => {
    ///     c.ModelPath = @"C:\models\Qwen3-8B-Q4_K_M.gguf";
    ///     c.GpuLayerCount = 0;
    ///     c.DefaultPromptTemplate = ChatTemplates.Qwen3;
    /// });
    /// </code></para>
    ///
    /// <para><b>Example (hybrid GPU, Phi-4-mini):</b>
    /// <code>
    /// services.AddLlamaSharpInference(c => {
    ///     c.ModelPath = @"C:\models\phi-4-mini-instruct-Q4_K_M.gguf";
    ///     c.GpuLayerCount = 24;
    ///     c.DefaultPromptTemplate = ChatTemplates.Phi4Mini;
    /// });
    /// </code></para>
    ///
    /// <para>The consuming application must also reference exactly one
    /// <c>LLamaSharp.Backend.*</c> NuGet package (Cpu, Cuda12, or Vulkan) so the native
    /// llama.cpp binaries end up in the output directory at runtime.</para>
    /// </summary>
    public static IServiceCollection AddLlamaSharpInference(
        this IServiceCollection services,
        Action<LlamaSharpInferenceConfig> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        var config = new LlamaSharpInferenceConfig();
        configure(config);

        services.AddSingleton(config);
        services.AddSingleton<IInferenceProvider, LlamaSharpInferenceProvider>();
        return services;
    }
}
