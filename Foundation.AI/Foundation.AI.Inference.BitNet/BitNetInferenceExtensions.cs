using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.Inference.BitNet;

/// <summary>
/// Extension methods for registering BitNet local inference.
/// </summary>
public static class BitNetInferenceExtensions
{
    /// <summary>
    /// Register a BitNet local inference provider.
    ///
    /// <para><b>Example:</b>
    /// <code>
    /// services.AddBitNetInference(c => {
    ///     c.ModelPath = "./models/BitNet-b1.58-2B-4T/ggml-model-i2_s.gguf";
    ///     c.Threads = 8;
    /// });
    /// </code></para>
    ///
    /// <para>Requires the native <c>llama.dll</c> to be built from the BitNet project
    /// and placed in the application's runtime directory.</para>
    /// </summary>
    public static IServiceCollection AddBitNetInference(
        this IServiceCollection services,
        Action<BitNetInferenceConfig> configure)
    {
        var config = new BitNetInferenceConfig();
        configure(config);

        services.AddSingleton(config);
        services.AddSingleton<IInferenceProvider, BitNetInferenceProvider>();
        return services;
    }
}
