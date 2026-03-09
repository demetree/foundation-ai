using BitNet.Interop.Native;

namespace BitNet.Interop;

/// <summary>
/// High-level options for loading a BitNet/llama model.
/// </summary>
public class BitNetModelParams
{
    /// <summary>
    /// Number of layers to offload to GPU. Default 0 (CPU only).
    /// Set to -1 to offload all layers.
    /// </summary>
    public int GpuLayers { get; set; } = 0;

    /// <summary>
    /// How to split the model across multiple GPUs.
    /// </summary>
    public LlamaSplitMode SplitMode { get; set; } = LlamaSplitMode.None;

    /// <summary>
    /// Main GPU to use (when split mode is active).
    /// </summary>
    public int MainGpu { get; set; } = 0;

    /// <summary>
    /// Use memory-mapped file I/O for loading the model.
    /// </summary>
    public bool UseMmap { get; set; } = true;

    /// <summary>
    /// Force the system to keep the model in RAM (mlock).
    /// </summary>
    public bool UseMlock { get; set; } = false;

    /// <summary>
    /// Only load the vocabulary, not the weights (for inspection).
    /// </summary>
    public bool VocabOnly { get; set; } = false;

    /// <summary>
    /// Validate model tensor data on load.
    /// </summary>
    public bool CheckTensors { get; set; } = false;

    /// <summary>
    /// Converts this options object to the native struct, applying defaults from llama.cpp first.
    /// </summary>
    internal LlamaModelParams ToNative()
    {
        var p = NativeMethods.ModelDefaultParams();
        p.NGpuLayers = GpuLayers;
        p.SplitMode = SplitMode;
        p.MainGpu = MainGpu;
        p.UseMmap = (byte)(UseMmap ? 1 : 0);
        p.UseMlock = (byte)(UseMlock ? 1 : 0);
        p.VocabOnly = (byte)(VocabOnly ? 1 : 0);
        p.CheckTensors = (byte)(CheckTensors ? 1 : 0);
        return p;
    }
}
