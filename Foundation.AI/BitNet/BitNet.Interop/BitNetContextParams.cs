using BitNet.Interop.Native;

namespace BitNet.Interop;

/// <summary>
/// High-level options for creating a BitNet inference context.
/// </summary>
public class BitNetContextParams
{
    /// <summary>
    /// Context window size in tokens. 0 = use model default.
    /// </summary>
    public uint ContextSize { get; set; } = 2048;

    /// <summary>
    /// Number of threads for single-token generation. 0 = auto-detect.
    /// </summary>
    public int Threads { get; set; } = 0;

    /// <summary>
    /// Number of threads for batch/prompt processing. 0 = same as Threads.
    /// </summary>
    public int ThreadsBatch { get; set; } = 0;

    /// <summary>
    /// Logical maximum batch size that can be submitted to decode.
    /// </summary>
    public uint BatchSize { get; set; } = 2048;

    /// <summary>
    /// Physical maximum batch size (micro-batch).
    /// </summary>
    public uint MicroBatchSize { get; set; } = 512;

    /// <summary>
    /// Enable Flash Attention (experimental, if supported).
    /// </summary>
    public bool FlashAttention { get; set; } = false;

    /// <summary>
    /// Whether to extract embeddings (together with logits).
    /// </summary>
    public bool Embeddings { get; set; } = false;

    /// <summary>
    /// Whether to offload KQV operations to GPU.
    /// </summary>
    public bool OffloadKqv { get; set; } = true;

    /// <summary>
    /// Data type for K cache. Default F16.
    /// </summary>
    public GgmlType TypeK { get; set; } = GgmlType.F16;

    /// <summary>
    /// Data type for V cache. Default F16.
    /// </summary>
    public GgmlType TypeV { get; set; } = GgmlType.F16;

    /// <summary>
    /// Converts this options object to the native struct, applying defaults from llama.cpp first.
    /// </summary>
    internal LlamaContextParams ToNative()
    {
        var p = NativeMethods.ContextDefaultParams();
        p.NCtx = ContextSize;
        p.NBatch = BatchSize;
        p.NUBatch = MicroBatchSize;

        if (Threads > 0)
        {
            p.NThreads = Threads;
            p.NThreadsBatch = ThreadsBatch > 0 ? ThreadsBatch : Threads;
        }

        p.FlashAttn = (byte)(FlashAttention ? 1 : 0);
        p.Embeddings = (byte)(Embeddings ? 1 : 0);
        p.OffloadKqv = (byte)(OffloadKqv ? 1 : 0);
        p.TypeK = TypeK;
        p.TypeV = TypeV;
        return p;
    }
}
