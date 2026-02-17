namespace Foundation.AI.Embed;

/// <summary>
/// Configuration for the ONNX embedding provider.
/// </summary>
public sealed class OnnxEmbeddingConfig
{
    /// <summary>
    /// Path to the ONNX model file (e.g., "./ai-models/all-MiniLM-L6-v2.onnx").
    /// </summary>
    public string ModelPath { get; set; } = "";

    /// <summary>
    /// Path to the tokenizer file (tokenizer.json from HuggingFace).
    /// If not specified, defaults to "{ModelPath directory}/tokenizer.json".
    /// </summary>
    public string? TokenizerPath { get; set; }

    /// <summary>
    /// Human-readable model name for logging. Defaults to the model filename.
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// Use CUDA GPU acceleration if available.
    /// Requires Microsoft.ML.OnnxRuntime.Gpu NuGet package and NVIDIA driver.
    /// Falls back to CPU if CUDA is unavailable.
    /// </summary>
    public bool UseCuda { get; set; }

    /// <summary>
    /// CUDA device ID (0 = first GPU). Only used when <see cref="UseCuda"/> is true.
    /// </summary>
    public int GpuDeviceId { get; set; }

    /// <summary>
    /// Maximum tokens per input text. Longer inputs are truncated.
    /// Model-dependent — typical values: 128 (MiniLM), 512 (BGE).
    /// Default: 512.
    /// </summary>
    public int MaxTokenLength { get; set; } = 512;

    /// <summary>
    /// Whether to L2-normalize output embeddings.
    /// Many embedding models produce normalized output by default,
    /// but this option ensures it regardless.
    /// Default: true.
    /// </summary>
    public bool NormalizeOutput { get; set; } = true;
}
