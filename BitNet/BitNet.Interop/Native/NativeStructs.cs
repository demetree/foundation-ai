using System.Runtime.InteropServices;

namespace BitNet.Interop.Native;

/// <summary>
/// Token data containing id, logit, and probability.
/// Maps to llama_token_data in llama.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct LlamaTokenData
{
    /// <summary>Token id.</summary>
    public int Id;

    /// <summary>Log-odds of the token.</summary>
    public float Logit;

    /// <summary>Probability of the token.</summary>
    public float P;
}

/// <summary>
/// Array of token data passed to samplers.
/// Maps to llama_token_data_array in llama.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LlamaTokenDataArray
{
    public LlamaTokenData* Data;
    public nuint Size;
    public long Selected;
    public byte Sorted; // bool
}

/// <summary>
/// Input batch for llama_decode / llama_encode.
/// Maps to llama_batch in llama.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LlamaBatch
{
    public int NTokens;

    public int* Token;
    public float* Embd;
    public int* Pos;
    public int* NSeqId;
    public int** SeqId;
    public sbyte* Logits;

    // Helpers for smooth API transition
    public int AllPos0;
    public int AllPos1;
    public int AllSeqId;
}

/// <summary>
/// Model loading parameters.
/// Maps to llama_model_params in llama.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LlamaModelParams
{
    /// <summary>Number of layers to store in VRAM.</summary>
    public int NGpuLayers;

    /// <summary>How to split the model across multiple GPUs.</summary>
    public LlamaSplitMode SplitMode;

    /// <summary>Main GPU to use.</summary>
    public int MainGpu;

    /// <summary>Proportion of the model to offload to each GPU.</summary>
    public float* TensorSplit;

    /// <summary>Comma separated list of RPC servers.</summary>
    public IntPtr RpcServers; // const char*

    /// <summary>Progress callback during loading.</summary>
    public IntPtr ProgressCallback;

    /// <summary>User data for progress callback.</summary>
    public IntPtr ProgressCallbackUserData;

    /// <summary>Override key-value pairs of the model metadata.</summary>
    public IntPtr KvOverrides;

    /// <summary>Only load the vocabulary, no weights.</summary>
    public byte VocabOnly; // bool

    /// <summary>Use mmap if possible.</summary>
    public byte UseMmap; // bool

    /// <summary>Force system to keep model in RAM.</summary>
    public byte UseMlock; // bool

    /// <summary>Validate model tensor data.</summary>
    public byte CheckTensors; // bool
}

/// <summary>
/// Context creation parameters.
/// Maps to llama_context_params in llama.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct LlamaContextParams
{
    public uint NCtx;
    public uint NBatch;
    public uint NUBatch;
    public uint NSeqMax;
    public int NThreads;
    public int NThreadsBatch;

    public LlamaRopeScalingType RopeScalingType;
    public LlamaPoolingType PoolingType;
    public LlamaAttentionType AttentionType;

    public float RopeFreqBase;
    public float RopeFreqScale;
    public float YarnExtFactor;
    public float YarnAttnFactor;
    public float YarnBetaFast;
    public float YarnBetaSlow;
    public uint YarnOrigCtx;
    public float DefragThold;

    public IntPtr CbEval;
    public IntPtr CbEvalUserData;

    public GgmlType TypeK;
    public GgmlType TypeV;

    public byte LogitsAll;  // bool
    public byte Embeddings; // bool
    public byte OffloadKqv; // bool
    public byte FlashAttn;  // bool
    public byte NoPerf;     // bool

    public IntPtr AbortCallback;
    public IntPtr AbortCallbackData;
}

/// <summary>
/// Sampler chain parameters.
/// Maps to llama_sampler_chain_params in llama.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct LlamaSamplerChainParams
{
    /// <summary>Whether to measure performance timings.</summary>
    public byte NoPerf; // bool
}

/// <summary>
/// Chat message for template formatting.
/// Maps to llama_chat_message in llama.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct LlamaChatMessage
{
    public IntPtr Role;    // const char*
    public IntPtr Content; // const char*
}

/// <summary>
/// Performance data for a context.
/// Maps to llama_perf_context_data in llama.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct LlamaPerfContextData
{
    public double TStartMs;
    public double TLoadMs;
    public double TPEvalMs;
    public double TEvalMs;
    public int NPEval;
    public int NEval;
}

/// <summary>
/// Performance data for a sampler.
/// Maps to llama_perf_sampler_data in llama.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct LlamaPerfSamplerData
{
    public double TSampleMs;
    public int NSample;
}

/// <summary>
/// Token logit bias.
/// Maps to llama_logit_bias in llama.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct LlamaLogitBias
{
    public int Token;
    public float Bias;
}
