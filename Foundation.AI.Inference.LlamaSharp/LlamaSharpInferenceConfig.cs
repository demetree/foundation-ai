namespace Foundation.AI.Inference.LlamaSharp;

/// <summary>
/// Configuration for the LLamaSharp local inference provider.
///
/// <para>This provider loads a GGUF file through LLamaSharp (a llama.cpp wrapper).
/// The native backend (<c>LLamaSharp.Backend.Cpu</c> or <c>.Cuda12</c> / <c>.Vulkan</c>)
/// is chosen by the consuming application's NuGet references — this config only
/// decides how many layers to hand to the GPU at runtime.</para>
/// </summary>
public class LlamaSharpInferenceConfig
{
    /// <summary>
    /// Absolute path to the GGUF model file (e.g. <c>C:\models\Qwen3-8B-Q4_K_M.gguf</c>).
    /// </summary>
    public string ModelPath { get; set; } = string.Empty;

    /// <summary>
    /// Context window size in tokens. Default: 8192.
    /// Larger contexts cost more VRAM/RAM for the KV cache but allow longer conversations.
    /// </summary>
    public uint ContextSize { get; set; } = 8192;

    /// <summary>
    /// Number of CPU threads used for inference. Default: 0 (auto = <c>Environment.ProcessorCount</c>).
    /// </summary>
    public int Threads { get; set; } = 0;

    /// <summary>
    /// Number of transformer layers to offload to the GPU.
    /// <list type="bullet">
    /// <item><c>0</c> — CPU only (use with <c>LLamaSharp.Backend.Cpu</c>).</item>
    /// <item><c>&gt;0</c> — hybrid or full GPU offload (use with <c>LLamaSharp.Backend.Cuda12</c> / <c>.Vulkan</c>).</item>
    /// <item><c>-1</c> — offload every layer if the backend supports it.</item>
    /// </list>
    /// If CUDA is not available at runtime, LLamaSharp logs a warning and falls back to CPU.
    /// </summary>
    public int GpuLayerCount { get; set; } = 0;

    /// <summary>
    /// Batch size for prompt processing. Default: 512.
    /// </summary>
    public uint BatchSize { get; set; } = 512;

    /// <summary>
    /// Default prompt template when <c>InferenceOptions.PromptTemplate</c> is not set by the caller.
    /// Default: <see cref="ChatTemplates.Qwen3"/>. Providers that want a different default
    /// (e.g. a Phi-4 or Llama-3 GGUF) override this via <c>configure</c> at registration time.
    /// </summary>
    public string DefaultPromptTemplate { get; set; } = ChatTemplates.Qwen3;

    /// <summary>
    /// Friendly model name surfaced through <see cref="IInferenceProvider.ModelName"/>.
    /// When unset, the provider derives one from the GGUF filename.
    /// </summary>
    public string? ModelName { get; set; }
}
