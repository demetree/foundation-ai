namespace Foundation.AI.Inference.BitNet;

/// <summary>
/// Configuration for the BitNet local inference provider.
/// </summary>
public class BitNetInferenceConfig
{
    /// <summary>
    /// Path to the BitNet GGUF model file.
    /// </summary>
    public string ModelPath { get; set; } = string.Empty;

    /// <summary>
    /// Context window size in tokens. Default: 2048.
    /// </summary>
    public uint ContextSize { get; set; } = 2048;

    /// <summary>
    /// Number of CPU threads to use for inference.
    /// Default: 0 (auto-detect = Environment.ProcessorCount).
    /// </summary>
    public int Threads { get; set; } = 0;

    /// <summary>
    /// Number of layers to offload to GPU. Default: 0 (CPU only).
    /// Set to -1 to offload all layers.
    /// </summary>
    public int GpuLayers { get; set; } = 0;

    /// <summary>
    /// Maximum batch size for prompt processing. Default: 2048.
    /// </summary>
    public uint BatchSize { get; set; } = 2048;

    /// <summary>
    /// Use memory-mapped file I/O for loading the model. Default: true.
    /// </summary>
    public bool UseMmap { get; set; } = true;
}
