using System.Runtime.InteropServices;
using System.Text;
using BitNet.Interop.Handles;
using BitNet.Interop.Native;

namespace BitNet.Interop;

/// <summary>
/// High-level API for loading and running inference with BitNet/llama models.
/// Provides a clean, IDisposable interface over the native llama.cpp library.
/// </summary>
public sealed class BitNetModel : IDisposable
{
    private readonly LlamaModelHandle _model;
    private readonly LlamaContextHandle _context;
    private bool _disposed;

    // ========================================================================
    // Backend lifecycle (static)
    // ========================================================================

    private static bool _backendInitialized;
    private static readonly object _backendLock = new();

    /// <summary>
    /// Initialize the llama backend. Must be called once before any model loading.
    /// Thread-safe — subsequent calls are no-ops.
    /// </summary>
    public static void InitBackend()
    {
        lock (_backendLock)
        {
            if (!_backendInitialized)
            {
                NativeMethods.BackendInit();
                _backendInitialized = true;
            }
        }
    }

    /// <summary>
    /// Free the llama backend. Call once at application shutdown.
    /// </summary>
    public static void FreeBackend()
    {
        lock (_backendLock)
        {
            if (_backendInitialized)
            {
                NativeMethods.BackendFree();
                _backendInitialized = false;
            }
        }
    }

    /// <summary>
    /// Get system information about the hardware and supported features.
    /// </summary>
    public static string SystemInfo
    {
        get
        {
            var ptr = NativeMethods.PrintSystemInfo();
            return Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
        }
    }

    /// <summary>
    /// Whether the runtime supports memory-mapped file I/O.
    /// </summary>
    public static bool SupportsMmap => NativeMethods.SupportsMmap();

    /// <summary>
    /// Whether the runtime supports mlock.
    /// </summary>
    public static bool SupportsMlock => NativeMethods.SupportsMlock();

    /// <summary>
    /// Whether the runtime supports GPU offloading.
    /// </summary>
    public static bool SupportsGpuOffload => NativeMethods.SupportsGpuOffload();

    // ========================================================================
    // Constructor / Disposal
    // ========================================================================

    /// <summary>
    /// Load a model and create an inference context.
    /// </summary>
    /// <param name="modelPath">Path to the GGUF model file.</param>
    /// <param name="modelParams">Optional model loading parameters.</param>
    /// <param name="contextParams">Optional context creation parameters.</param>
    /// <exception cref="InvalidOperationException">If model loading or context creation fails.</exception>
    public BitNetModel(string modelPath, BitNetModelParams? modelParams = null, BitNetContextParams? contextParams = null)
    {
        InitBackend();

        var mp = (modelParams ?? new BitNetModelParams()).ToNative();
        var cp = (contextParams ?? new BitNetContextParams()).ToNative();

        _model = LlamaModelHandle.LoadFromFile(modelPath, mp);
        _context = LlamaContextHandle.Create(_model, cp);
    }

    /// <summary>
    /// Dispose the model and context, freeing all native resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _model.Dispose();
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    // ========================================================================
    // Model properties
    // ========================================================================

    /// <summary>The native model handle (for advanced/direct P/Invoke use).</summary>
    public IntPtr ModelHandle => _model.DangerousGetHandle();

    /// <summary>The native context handle (for advanced/direct P/Invoke use).</summary>
    public IntPtr ContextHandle => _context.DangerousGetHandle();

    /// <summary>Number of tokens in the model's vocabulary.</summary>
    public int VocabSize => NativeMethods.NVocab(ModelHandle);

    /// <summary>Embedding dimension of the model.</summary>
    public int EmbeddingSize => NativeMethods.NEmbd(ModelHandle);

    /// <summary>Training context length of the model.</summary>
    public int TrainingContextLength => NativeMethods.NCtxTrain(ModelHandle);

    /// <summary>Active context window size.</summary>
    public uint ContextSize => NativeMethods.NCtx(ContextHandle);

    /// <summary>Number of layers in the model.</summary>
    public int LayerCount => NativeMethods.NLayer(ModelHandle);

    /// <summary>Number of attention heads.</summary>
    public int HeadCount => NativeMethods.NHead(ModelHandle);

    /// <summary>Total size of model tensors in bytes.</summary>
    public ulong ModelSizeBytes => NativeMethods.ModelSize(ModelHandle);

    /// <summary>Total number of parameters in the model.</summary>
    public ulong ParameterCount => NativeMethods.ModelNParams(ModelHandle);

    /// <summary>Whether the model has an encoder component.</summary>
    public bool HasEncoder => NativeMethods.ModelHasEncoder(ModelHandle);

    /// <summary>Whether the model has a decoder component.</summary>
    public bool HasDecoder => NativeMethods.ModelHasDecoder(ModelHandle);

    /// <summary>Whether the model is recurrent (Mamba, RWKV, etc.).</summary>
    public bool IsRecurrent => NativeMethods.ModelIsRecurrent(ModelHandle);

    /// <summary>BOS (beginning-of-sentence) token id.</summary>
    public int BosToken => NativeMethods.TokenBos(ModelHandle);

    /// <summary>EOS (end-of-sentence) token id.</summary>
    public int EosToken => NativeMethods.TokenEos(ModelHandle);

    /// <summary>Newline token id.</summary>
    public int NlToken => NativeMethods.TokenNl(ModelHandle);

    /// <summary>
    /// Get a text description of the model type.
    /// </summary>
    public unsafe string Description
    {
        get
        {
            ThrowIfDisposed();
            Span<byte> buf = stackalloc byte[256];
            fixed (byte* ptr = buf)
            {
                var len = NativeMethods.ModelDesc(ModelHandle, ptr, 256);
                return len > 0 ? Encoding.UTF8.GetString(buf[..len]) : string.Empty;
            }
        }
    }

    /// <summary>
    /// Get a model metadata value by key.
    /// </summary>
    public unsafe string? GetMetadata(string key)
    {
        ThrowIfDisposed();
        Span<byte> buf = stackalloc byte[512];
        fixed (byte* ptr = buf)
        {
            var len = NativeMethods.ModelMetaValStr(ModelHandle, key, ptr, 512);
            return len > 0 ? Encoding.UTF8.GetString(buf[..len]) : null;
        }
    }

    // ========================================================================
    // Tokenization
    // ========================================================================

    /// <summary>
    /// Tokenize text into token ids.
    /// </summary>
    /// <param name="text">The text to tokenize.</param>
    /// <param name="addSpecial">Add BOS/EOS tokens if the model is configured to do so.</param>
    /// <param name="parseSpecial">Allow tokenizing special/control tokens.</param>
    /// <returns>Array of token ids.</returns>
    public unsafe int[] Tokenize(string text, bool addSpecial = true, bool parseSpecial = false)
    {
        ThrowIfDisposed();

        var textLen = Encoding.UTF8.GetByteCount(text);

        // First call to get the required size (returns negative count if buffer too small)
        var nTokensMax = textLen + (addSpecial ? 2 : 0);
        var tokens = new int[nTokensMax];

        fixed (int* tokensPtr = tokens)
        {
            var nTokens = NativeMethods.Tokenize(ModelHandle, text, textLen, tokensPtr, nTokensMax, addSpecial, parseSpecial);

            if (nTokens < 0)
            {
                // Need more space — retry with the returned required size
                nTokensMax = -nTokens;
                tokens = new int[nTokensMax];
                fixed (int* retryPtr = tokens)
                {
                    nTokens = NativeMethods.Tokenize(ModelHandle, text, textLen, retryPtr, nTokensMax, addSpecial, parseSpecial);
                    if (nTokens < 0)
                        throw new InvalidOperationException("Tokenization failed.");
                }
            }

            return tokens[..nTokens];
        }
    }

    /// <summary>
    /// Convert a single token id to its text representation.
    /// </summary>
    public unsafe string TokenToText(int token, bool special = false)
    {
        ThrowIfDisposed();
        Span<byte> buf = stackalloc byte[256];
        fixed (byte* ptr = buf)
        {
            var len = NativeMethods.TokenToPiece(ModelHandle, token, ptr, 256, 0, special);
            return len > 0 ? Encoding.UTF8.GetString(buf[..len]) : string.Empty;
        }
    }

    /// <summary>
    /// Detokenize an array of token ids back to text.
    /// </summary>
    public unsafe string Detokenize(ReadOnlySpan<int> tokens, bool removeSpecial = true, bool unparseSpecial = false)
    {
        ThrowIfDisposed();

        var bufSize = tokens.Length * 32; // heuristic
        var buf = new byte[bufSize];

        fixed (int* tokensPtr = tokens)
        fixed (byte* bufPtr = buf)
        {
            var len = NativeMethods.Detokenize(ModelHandle, tokensPtr, tokens.Length, bufPtr, bufSize, removeSpecial, unparseSpecial);

            if (len < 0)
            {
                // Need more space
                bufSize = -len;
                buf = new byte[bufSize];
                fixed (byte* retryPtr = buf)
                {
                    len = NativeMethods.Detokenize(ModelHandle, tokensPtr, tokens.Length, retryPtr, bufSize, removeSpecial, unparseSpecial);
                    if (len < 0)
                        throw new InvalidOperationException("Detokenization failed.");
                }
            }

            return Encoding.UTF8.GetString(buf, 0, len);
        }
    }

    /// <summary>
    /// Check if a token is an end-of-generation token (EOS, EOT, etc.).
    /// </summary>
    public bool IsEndOfGeneration(int token) => NativeMethods.TokenIsEog(ModelHandle, token);

    // ========================================================================
    // Inference — one-shot generation
    // ========================================================================

    /// <summary>
    /// Generate text from a prompt using one-shot generation.
    /// </summary>
    /// <param name="prompt">The input prompt text.</param>
    /// <param name="maxTokens">Maximum number of tokens to generate.</param>
    /// <param name="temperature">Sampling temperature (0 = greedy).</param>
    /// <param name="topK">Top-K sampling parameter (0 = disabled).</param>
    /// <param name="topP">Top-P (nucleus) sampling parameter (1.0 = disabled).</param>
    /// <param name="seed">Random seed for sampling (null = random).</param>
    /// <returns>The generated text.</returns>
    public string Generate(string prompt, int maxTokens = 128, float temperature = 0.8f,
                           int topK = 40, float topP = 0.95f, uint? seed = null)
    {
        var sb = new StringBuilder();
        foreach (var piece in GenerateTokens(prompt, maxTokens, temperature, topK, topP, seed))
        {
            sb.Append(piece);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Generate tokens one by one from a prompt, yielding each piece as it's produced.
    /// Ideal for streaming output to a UI or console.
    /// </summary>
    public IEnumerable<string> GenerateTokens(string prompt, int maxTokens = 128, float temperature = 0.8f,
                                               int topK = 40, float topP = 0.95f, uint? seed = null)
    {
        ThrowIfDisposed();

        // Tokenize the prompt
        var tokens = Tokenize(prompt, addSpecial: true, parseSpecial: false);

        // Clear KV cache for a fresh generation
        NativeMethods.KvCacheClear(ContextHandle);

        // Create sampler chain
        var sparams = NativeMethods.SamplerChainDefaultParams();
        using var samplerHandle = LlamaSamplerHandle.CreateChain(sparams);
        var samplerPtr = samplerHandle.DangerousGetHandle();

        // Add sampling stages
        if (topK > 0)
            NativeMethods.SamplerChainAdd(samplerPtr, NativeMethods.SamplerInitTopK(topK));

        if (topP < 1.0f)
            NativeMethods.SamplerChainAdd(samplerPtr, NativeMethods.SamplerInitTopP(topP, 1));

        if (temperature > 0)
            NativeMethods.SamplerChainAdd(samplerPtr, NativeMethods.SamplerInitTemp(temperature));
        else
            NativeMethods.SamplerChainAdd(samplerPtr, NativeMethods.SamplerInitGreedy());

        // Final sampling stage
        var actualSeed = seed ?? (uint)Random.Shared.Next();
        if (temperature > 0)
            NativeMethods.SamplerChainAdd(samplerPtr, NativeMethods.SamplerInitDist(actualSeed));

        // Process the prompt (unsafe helper — can't use fixed in an iterator)
        DecodeTokenBatch(tokens, 0);

        // Generate tokens
        var curPos = tokens.Length;
        for (int i = 0; i < maxTokens; i++)
        {
            // Sample the next token
            var newTokenId = NativeMethods.SamplerSample(samplerPtr, ContextHandle, -1);

            // Check for end of generation
            if (IsEndOfGeneration(newTokenId))
                yield break;

            // Convert token to text
            var piece = TokenToText(newTokenId);

            // Accept the token (updates sampler internal state)
            NativeMethods.SamplerAccept(samplerPtr, newTokenId);

            // Decode the new token (unsafe helper)
            DecodeSingleToken(newTokenId, curPos);

            curPos++;

            yield return piece;
        }
    }

    /// <summary>
    /// Decode a batch of tokens starting at the given position. (unsafe helper)
    /// </summary>
    private unsafe void DecodeTokenBatch(int[] tokens, int pos0)
    {
        fixed (int* tokensPtr = tokens)
        {
            var batch = NativeMethods.BatchGetOne(tokensPtr, tokens.Length, pos0, 0);
            var result = NativeMethods.Decode(ContextHandle, batch);
            if (result != 0)
                throw new InvalidOperationException($"Failed to decode token batch. Error code: {result}");
        }
    }

    /// <summary>
    /// Decode a single token at the given position. (unsafe helper)
    /// </summary>
    private unsafe void DecodeSingleToken(int tokenId, int pos)
    {
        var tokenArr = new[] { tokenId };
        fixed (int* tokenPtr = tokenArr)
        {
            var batch = NativeMethods.BatchGetOne(tokenPtr, 1, pos, 0);
            var result = NativeMethods.Decode(ContextHandle, batch);
            if (result != 0)
                throw new InvalidOperationException($"Failed to decode token. Error code: {result}");
        }
    }

    // ========================================================================
    // Chat support
    // ========================================================================

    /// <summary>
    /// Apply the model's chat template to a list of messages.
    /// </summary>
    /// <param name="messages">The conversation messages.</param>
    /// <param name="addAssistantPrompt">Whether to add the assistant prompt token at the end.</param>
    /// <param name="customTemplate">Optional custom Jinja-like template (null = use model default).</param>
    /// <returns>The formatted prompt string.</returns>
    public unsafe string ApplyChatTemplate(IReadOnlyList<BitNetChatMessage> messages, bool addAssistantPrompt = true,
                                            string? customTemplate = null)
    {
        ThrowIfDisposed();

        var nativeMessages = new LlamaChatMessage[messages.Count];
        var pinnedRoles = new GCHandle[messages.Count];
        var pinnedContents = new GCHandle[messages.Count];

        try
        {
            for (int i = 0; i < messages.Count; i++)
            {
                var roleBytes = Encoding.UTF8.GetBytes(messages[i].Role + '\0');
                var contentBytes = Encoding.UTF8.GetBytes(messages[i].Content + '\0');

                pinnedRoles[i] = GCHandle.Alloc(roleBytes, GCHandleType.Pinned);
                pinnedContents[i] = GCHandle.Alloc(contentBytes, GCHandleType.Pinned);

                nativeMessages[i] = new LlamaChatMessage
                {
                    Role = pinnedRoles[i].AddrOfPinnedObject(),
                    Content = pinnedContents[i].AddrOfPinnedObject()
                };
            }

            fixed (LlamaChatMessage* msgsPtr = nativeMessages)
            {
                // First call to get required buffer size
                var requiredSize = NativeMethods.ChatApplyTemplate(
                    ModelHandle, customTemplate, msgsPtr, (nuint)messages.Count, addAssistantPrompt, null, 0);

                if (requiredSize <= 0)
                    throw new InvalidOperationException("Failed to apply chat template.");

                var buf = new byte[requiredSize + 1];
                fixed (byte* bufPtr = buf)
                {
                    var written = NativeMethods.ChatApplyTemplate(
                        ModelHandle, customTemplate, msgsPtr, (nuint)messages.Count, addAssistantPrompt, bufPtr, requiredSize + 1);

                    return Encoding.UTF8.GetString(buf, 0, written);
                }
            }
        }
        finally
        {
            for (int i = 0; i < messages.Count; i++)
            {
                if (pinnedRoles[i].IsAllocated) pinnedRoles[i].Free();
                if (pinnedContents[i].IsAllocated) pinnedContents[i].Free();
            }
        }
    }

    /// <summary>
    /// Chat with the model using a list of messages. 
    /// Applies the chat template and generates a response.
    /// </summary>
    /// <param name="messages">The conversation messages.</param>
    /// <param name="maxTokens">Maximum tokens to generate.</param>
    /// <param name="temperature">Sampling temperature.</param>
    /// <param name="topK">Top-K sampling parameter.</param>
    /// <param name="topP">Top-P sampling parameter.</param>
    /// <param name="seed">Random seed.</param>
    /// <returns>The assistant's response text.</returns>
    public string Chat(IReadOnlyList<BitNetChatMessage> messages, int maxTokens = 512, float temperature = 0.8f,
                       int topK = 40, float topP = 0.95f, uint? seed = null)
    {
        var formattedPrompt = ApplyChatTemplate(messages);
        return Generate(formattedPrompt, maxTokens, temperature, topK, topP, seed);
    }

    /// <summary>
    /// Chat with streaming output.
    /// </summary>
    public IEnumerable<string> ChatStreaming(IReadOnlyList<BitNetChatMessage> messages, int maxTokens = 512,
                                             float temperature = 0.8f, int topK = 40, float topP = 0.95f, uint? seed = null)
    {
        var formattedPrompt = ApplyChatTemplate(messages);
        return GenerateTokens(formattedPrompt, maxTokens, temperature, topK, topP, seed);
    }

    // ========================================================================
    // KV Cache management
    // ========================================================================

    /// <summary>
    /// Clear the entire KV cache.
    /// </summary>
    public void ClearKvCache()
    {
        ThrowIfDisposed();
        NativeMethods.KvCacheClear(ContextHandle);
    }

    /// <summary>
    /// Get the number of tokens currently in the KV cache.
    /// </summary>
    public int KvCacheTokenCount => NativeMethods.GetKvCacheTokenCount(ContextHandle);

    /// <summary>
    /// Get the number of used KV cache cells.
    /// </summary>
    public int KvCacheUsedCells => NativeMethods.GetKvCacheUsedCells(ContextHandle);

    // ========================================================================
    // Performance
    // ========================================================================

    /// <summary>
    /// Get performance data for the context.
    /// </summary>
    public LlamaPerfContextData GetPerformanceData()
    {
        ThrowIfDisposed();
        return NativeMethods.PerfContext(ContextHandle);
    }

    /// <summary>
    /// Print performance data to stderr (native llama.cpp format).
    /// </summary>
    public void PrintPerformanceData()
    {
        ThrowIfDisposed();
        NativeMethods.PerfContextPrint(ContextHandle);
    }

    /// <summary>
    /// Reset performance counters.
    /// </summary>
    public void ResetPerformanceData()
    {
        ThrowIfDisposed();
        NativeMethods.PerfContextReset(ContextHandle);
    }
}
