namespace BitNet.Interop.Native;

/// <summary>
/// Vocabulary type used by the model.
/// </summary>
public enum LlamaVocabType
{
    None = 0,
    Spm  = 1,  // SentencePiece
    Bpe  = 2,  // Byte Pair Encoding
    Wpm  = 3,  // WordPiece
    Ugm  = 4,  // Unigram (T5)
    Rwkv = 5,  // RWKV greedy tokenization
}

/// <summary>
/// Pre-tokenization types.
/// </summary>
public enum LlamaVocabPreType
{
    Default       = 0,
    Llama3        = 1,
    DeepSeekLlm   = 2,
    DeepSeekCoder  = 3,
    Falcon        = 4,
    Mpt           = 5,
    StarCoder     = 6,
    Gpt2          = 7,
    Refact        = 8,
    Command       = 9,
    StableLm2     = 10,
    Qwen2         = 11,
    Olmo          = 12,
    Viking        = 13,
    Jais          = 14,
    Smaug         = 15,
    Poro          = 16,
    Chatglm3      = 17,
    Chatglm4      = 18,
    Tekken        = 19,
    Smollm        = 20,
    CodeShell     = 21,
    Bloom         = 22,
    Gpt3Finnish   = 23,
    Exaone        = 24,
    Chameleon     = 25,
    Falcon3       = 27,
    FalconE       = 28,
}

/// <summary>
/// RoPE scaling type.
/// </summary>
public enum LlamaRopeType
{
    None = -1,
    Norm = 0,
    Neox = 2,
}

/// <summary>
/// Model file types / quantization types.
/// </summary>
public enum LlamaFtype
{
    AllF32              = 0,
    MostlyF16           = 1,
    MostlyQ4_0          = 2,
    MostlyQ4_1          = 3,
    MostlyQ8_0          = 7,
    MostlyQ5_0          = 8,
    MostlyQ5_1          = 9,
    MostlyQ2K           = 10,
    MostlyQ3KS          = 11,
    MostlyQ3KM          = 12,
    MostlyQ3KL          = 13,
    MostlyQ4KS          = 14,
    MostlyQ4KM          = 15,
    MostlyQ5KS          = 16,
    MostlyQ5KM          = 17,
    MostlyQ6K           = 18,
    MostlyIQ2XXS        = 19,
    MostlyIQ2XS         = 20,
    MostlyQ2KS          = 21,
    MostlyIQ3XS         = 22,
    MostlyIQ3XXS        = 23,
    MostlyIQ1S          = 24,
    MostlyIQ4NL         = 25,
    MostlyIQ3S          = 26,
    MostlyIQ3M          = 27,
    MostlyIQ2S          = 28,
    MostlyIQ2M          = 29,
    MostlyIQ4XS         = 30,
    MostlyIQ1M          = 31,
    MostlyBF16          = 32,
    MostlyQ4_0_4_4      = 33,
    MostlyQ4_0_4_8      = 34,
    MostlyQ4_0_8_8      = 35,
    MostlyTQ1_0         = 36,
    MostlyTQ2_0         = 37,
    MostlyTL1           = 38,
    MostlyTL2           = 39,
    MostlyI2S           = 40,
    Guessed             = 1024,
}

/// <summary>
/// RoPE scaling type for context parameters.
/// </summary>
public enum LlamaRopeScalingType
{
    Unspecified = -1,
    None        = 0,
    Linear      = 1,
    Yarn        = 2,
}

/// <summary>
/// Pooling type for embedding models.
/// </summary>
public enum LlamaPoolingType
{
    Unspecified = -1,
    None = 0,
    Mean = 1,
    Cls  = 2,
    Last = 3,
    Rank = 4,
}

/// <summary>
/// Attention type.
/// </summary>
public enum LlamaAttentionType
{
    Unspecified = -1,
    Causal      = 0,
    NonCausal   = 1,
}

/// <summary>
/// How to split model across multiple GPUs.
/// </summary>
public enum LlamaSplitMode
{
    None  = 0,
    Layer = 1,
    Row   = 2,
}

/// <summary>
/// Token attributes (flags).
/// </summary>
[Flags]
public enum LlamaTokenAttr
{
    Undefined    = 0,
    Unknown      = 1 << 0,
    Unused       = 1 << 1,
    Normal       = 1 << 2,
    Control      = 1 << 3,
    UserDefined  = 1 << 4,
    Byte         = 1 << 5,
    Normalized   = 1 << 6,
    Lstrip       = 1 << 7,
    Rstrip       = 1 << 8,
    SingleWord   = 1 << 9,
}

/// <summary>
/// GGML data types (subset used for KV cache type_k/type_v).
/// </summary>
public enum GgmlType
{
    F32   = 0,
    F16   = 1,
    Q4_0  = 2,
    Q4_1  = 3,
    Q5_0  = 6,
    Q5_1  = 7,
    Q8_0  = 8,
    Q8_1  = 9,
    Q2K   = 10,
    Q3K   = 11,
    Q4K   = 12,
    Q5K   = 13,
    Q6K   = 14,
    Q8K   = 15,
    IQ2XXS = 16,
    IQ2XS  = 17,
    IQ3XXS = 18,
    IQ1S   = 19,
    IQ4NL  = 20,
    IQ3S   = 21,
    IQ2S   = 22,
    IQ4XS  = 23,
    I8     = 24,
    I16    = 25,
    I32    = 26,
    I64    = 27,
    F64    = 28,
    IQ1M   = 29,
    BF16   = 30,
}

/// <summary>
/// NUMA strategy.
/// </summary>
public enum GgmlNumaStrategy
{
    Disabled  = 0,
    Distribute = 1,
    Isolate    = 2,
    NumaCtl    = 3,
    Mirror     = 4,
}
