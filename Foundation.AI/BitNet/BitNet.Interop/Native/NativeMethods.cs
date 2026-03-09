using System.Runtime.InteropServices;

namespace BitNet.Interop.Native;

/// <summary>
/// Raw P/Invoke bindings to the llama.cpp C API.
/// All functions map 1:1 to their C counterparts in llama.h.
/// </summary>
public static unsafe partial class NativeMethods
{
    private const string LibraryName = "llama";

    // ========================================================================
    // Backend lifecycle
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_backend_init")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void BackendInit();

    [LibraryImport(LibraryName, EntryPoint = "llama_numa_init")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void NumaInit(GgmlNumaStrategy numa);

    [LibraryImport(LibraryName, EntryPoint = "llama_backend_free")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void BackendFree();

    // ========================================================================
    // Default parameters
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_model_default_params")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial LlamaModelParams ModelDefaultParams();

    [LibraryImport(LibraryName, EntryPoint = "llama_context_default_params")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial LlamaContextParams ContextDefaultParams();

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_chain_default_params")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial LlamaSamplerChainParams SamplerChainDefaultParams();

    // ========================================================================
    // Model loading / freeing
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_load_model_from_file", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr LoadModelFromFile(string pathModel, LlamaModelParams @params);

    [LibraryImport(LibraryName, EntryPoint = "llama_free_model")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void FreeModel(IntPtr model);

    // ========================================================================
    // Context creation / freeing
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_new_context_with_model")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr NewContextWithModel(IntPtr model, LlamaContextParams @params);

    [LibraryImport(LibraryName, EntryPoint = "llama_free")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void Free(IntPtr ctx);

    // ========================================================================
    // System queries
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_time_us")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial long TimeUs();

    [LibraryImport(LibraryName, EntryPoint = "llama_max_devices")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial nuint MaxDevices();

    [LibraryImport(LibraryName, EntryPoint = "llama_supports_mmap")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool SupportsMmap();

    [LibraryImport(LibraryName, EntryPoint = "llama_supports_mlock")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool SupportsMlock();

    [LibraryImport(LibraryName, EntryPoint = "llama_supports_gpu_offload")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool SupportsGpuOffload();

    [LibraryImport(LibraryName, EntryPoint = "llama_print_system_info")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr PrintSystemInfo();

    // ========================================================================
    // Context queries
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_n_ctx")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial uint NCtx(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_n_batch")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial uint NBatch(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_n_ubatch")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial uint NUBatch(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_n_seq_max")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial uint NSeqMax(IntPtr ctx);

    // ========================================================================
    // Model queries
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_n_vocab")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int NVocab(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_n_ctx_train")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int NCtxTrain(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_n_embd")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int NEmbd(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_n_layer")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int NLayer(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_n_head")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int NHead(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_model_desc", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int ModelDesc(IntPtr model, byte* buf, nuint bufSize);

    [LibraryImport(LibraryName, EntryPoint = "llama_model_size")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial ulong ModelSize(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_model_n_params")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial ulong ModelNParams(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_model_has_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ModelHasEncoder(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_model_has_decoder")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ModelHasDecoder(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_model_is_recurrent")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ModelIsRecurrent(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_get_model")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr GetModel(IntPtr ctx);

    // ========================================================================
    // Model metadata
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_model_meta_val_str", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int ModelMetaValStr(IntPtr model, string key, byte* buf, nuint bufSize);

    [LibraryImport(LibraryName, EntryPoint = "llama_model_meta_count")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int ModelMetaCount(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_model_meta_key_by_index")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int ModelMetaKeyByIndex(IntPtr model, int i, byte* buf, nuint bufSize);

    [LibraryImport(LibraryName, EntryPoint = "llama_model_meta_val_str_by_index")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int ModelMetaValStrByIndex(IntPtr model, int i, byte* buf, nuint bufSize);

    // ========================================================================
    // Tokenization
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_tokenize", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int Tokenize(IntPtr model, string text, int textLen, int* tokens, int nTokensMax,
        [MarshalAs(UnmanagedType.U1)] bool addSpecial, [MarshalAs(UnmanagedType.U1)] bool parseSpecial);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_to_piece")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int TokenToPiece(IntPtr model, int token, byte* buf, int length, int lstrip,
        [MarshalAs(UnmanagedType.U1)] bool special);

    [LibraryImport(LibraryName, EntryPoint = "llama_detokenize")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int Detokenize(IntPtr model, int* tokens, int nTokens, byte* text, int textLenMax,
        [MarshalAs(UnmanagedType.U1)] bool removeSpecial, [MarshalAs(UnmanagedType.U1)] bool unparseSpecial);

    // ========================================================================
    // Special tokens
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_token_bos")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int TokenBos(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_eos")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int TokenEos(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_eot")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int TokenEot(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_cls")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int TokenCls(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_sep")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int TokenSep(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_nl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int TokenNl(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_pad")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int TokenPad(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_is_eog")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool TokenIsEog(IntPtr model, int token);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_is_control")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool TokenIsControl(IntPtr model, int token);

    [LibraryImport(LibraryName, EntryPoint = "llama_add_bos_token")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool AddBosToken(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_add_eos_token")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool AddEosToken(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_get_text")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr TokenGetText(IntPtr model, int token);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_get_score")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial float TokenGetScore(IntPtr model, int token);

    [LibraryImport(LibraryName, EntryPoint = "llama_token_get_attr")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial LlamaTokenAttr TokenGetAttr(IntPtr model, int token);

    // ========================================================================
    // Batch operations
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_batch_init")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial LlamaBatch BatchInit(int nTokens, int embd, int nSeqMax);

    [LibraryImport(LibraryName, EntryPoint = "llama_batch_free")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void BatchFree(LlamaBatch batch);

    [LibraryImport(LibraryName, EntryPoint = "llama_batch_get_one")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial LlamaBatch BatchGetOne(int* tokens, int nTokens, int pos0, int seqId);

    // ========================================================================
    // Decode / Encode
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_decode")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int Decode(IntPtr ctx, LlamaBatch batch);

    [LibraryImport(LibraryName, EntryPoint = "llama_encode")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int Encode(IntPtr ctx, LlamaBatch batch);

    [LibraryImport(LibraryName, EntryPoint = "llama_synchronize")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void Synchronize(IntPtr ctx);

    // ========================================================================
    // Thread configuration
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_set_n_threads")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void SetNThreads(IntPtr ctx, int nThreads, int nThreadsBatch);

    [LibraryImport(LibraryName, EntryPoint = "llama_n_threads")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int NThreads(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_n_threads_batch")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int NThreadsBatch(IntPtr ctx);

    // ========================================================================
    // Logits / Embeddings
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_get_logits")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial float* GetLogits(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_get_logits_ith")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial float* GetLogitsIth(IntPtr ctx, int i);

    [LibraryImport(LibraryName, EntryPoint = "llama_get_embeddings")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial float* GetEmbeddings(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_get_embeddings_ith")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial float* GetEmbeddingsIth(IntPtr ctx, int i);

    [LibraryImport(LibraryName, EntryPoint = "llama_get_embeddings_seq")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial float* GetEmbeddingsSeq(IntPtr ctx, int seqId);

    [LibraryImport(LibraryName, EntryPoint = "llama_set_embeddings")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void SetEmbeddings(IntPtr ctx, [MarshalAs(UnmanagedType.U1)] bool embeddings);

    [LibraryImport(LibraryName, EntryPoint = "llama_set_causal_attn")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void SetCausalAttn(IntPtr ctx, [MarshalAs(UnmanagedType.U1)] bool causalAttn);

    // ========================================================================
    // KV cache
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_kv_cache_clear")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void KvCacheClear(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_kv_cache_seq_rm")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool KvCacheSeqRm(IntPtr ctx, int seqId, int p0, int p1);

    [LibraryImport(LibraryName, EntryPoint = "llama_kv_cache_seq_cp")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void KvCacheSeqCp(IntPtr ctx, int seqIdSrc, int seqIdDst, int p0, int p1);

    [LibraryImport(LibraryName, EntryPoint = "llama_kv_cache_seq_keep")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void KvCacheSeqKeep(IntPtr ctx, int seqId);

    [LibraryImport(LibraryName, EntryPoint = "llama_kv_cache_seq_add")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void KvCacheSeqAdd(IntPtr ctx, int seqId, int p0, int p1, int delta);

    [LibraryImport(LibraryName, EntryPoint = "llama_kv_cache_seq_div")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void KvCacheSeqDiv(IntPtr ctx, int seqId, int p0, int p1, int d);

    [LibraryImport(LibraryName, EntryPoint = "llama_kv_cache_seq_pos_max")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int KvCacheSeqPosMax(IntPtr ctx, int seqId);

    [LibraryImport(LibraryName, EntryPoint = "llama_kv_cache_defrag")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void KvCacheDefrag(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_kv_cache_update")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void KvCacheUpdate(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_get_kv_cache_token_count")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int GetKvCacheTokenCount(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_get_kv_cache_used_cells")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int GetKvCacheUsedCells(IntPtr ctx);

    // ========================================================================
    // Sampling
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_chain_init")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerChainInit(LlamaSamplerChainParams @params);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_chain_add")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void SamplerChainAdd(IntPtr chain, IntPtr smpl);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_chain_get")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerChainGet(IntPtr chain, int i);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_chain_n")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int SamplerChainN(IntPtr chain);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_chain_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerChainRemove(IntPtr chain, int i);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_free")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void SamplerFree(IntPtr smpl);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_accept")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void SamplerAccept(IntPtr smpl, int token);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_reset")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void SamplerReset(IntPtr smpl);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_name")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerName(IntPtr smpl);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_sample")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int SamplerSample(IntPtr smpl, IntPtr ctx, int idx);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_get_seed")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial uint SamplerGetSeed(IntPtr smpl);

    // Sampler init functions

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_greedy")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitGreedy();

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_dist")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitDist(uint seed);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_softmax")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitSoftmax();

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_top_k")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitTopK(int k);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_top_p")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitTopP(float p, nuint minKeep);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_min_p")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitMinP(float p, nuint minKeep);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_tail_free")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitTailFree(float z, nuint minKeep);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_typical")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitTypical(float p, nuint minKeep);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_temp")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitTemp(float t);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_temp_ext")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitTempExt(float t, float delta, float exponent);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_mirostat")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitMirostat(int nVocab, uint seed, float tau, float eta, int m);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_mirostat_v2")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitMirostatV2(uint seed, float tau, float eta);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_grammar", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitGrammar(IntPtr model, string grammarStr, string grammarRoot);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_penalties")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitPenalties(int nVocab, int specialEosId, int linefeedId,
        int penaltyLastN, float penaltyRepeat, float penaltyFreq, float penaltyPresent,
        [MarshalAs(UnmanagedType.U1)] bool penalizeNl, [MarshalAs(UnmanagedType.U1)] bool ignoreEos);

    [LibraryImport(LibraryName, EntryPoint = "llama_sampler_init_logit_bias")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr SamplerInitLogitBias(int nVocab, int nLogitBias, LlamaLogitBias* logitBias);

    // ========================================================================
    // Chat template
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_chat_apply_template", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int ChatApplyTemplate(IntPtr model, string? tmpl, LlamaChatMessage* chat, nuint nMsg,
        [MarshalAs(UnmanagedType.U1)] bool addAss, byte* buf, int length);

    // ========================================================================
    // LoRA adapters
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_lora_adapter_init", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr LoraAdapterInit(IntPtr model, string pathLora);

    [LibraryImport(LibraryName, EntryPoint = "llama_lora_adapter_set")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int LoraAdapterSet(IntPtr ctx, IntPtr adapter, float scale);

    [LibraryImport(LibraryName, EntryPoint = "llama_lora_adapter_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int LoraAdapterRemove(IntPtr ctx, IntPtr adapter);

    [LibraryImport(LibraryName, EntryPoint = "llama_lora_adapter_clear")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void LoraAdapterClear(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_lora_adapter_free")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void LoraAdapterFree(IntPtr adapter);

    // ========================================================================
    // Performance
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_perf_context")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial LlamaPerfContextData PerfContext(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_perf_context_print")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PerfContextPrint(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_perf_context_reset")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PerfContextReset(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_perf_sampler")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial LlamaPerfSamplerData PerfSampler(IntPtr chain);

    [LibraryImport(LibraryName, EntryPoint = "llama_perf_sampler_print")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PerfSamplerPrint(IntPtr chain);

    [LibraryImport(LibraryName, EntryPoint = "llama_perf_sampler_reset")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PerfSamplerReset(IntPtr chain);

    // ========================================================================
    // State / Session  (advanced)
    // ========================================================================

    [LibraryImport(LibraryName, EntryPoint = "llama_state_get_size")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial nuint StateGetSize(IntPtr ctx);

    [LibraryImport(LibraryName, EntryPoint = "llama_state_get_data")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial nuint StateGetData(IntPtr ctx, byte* dst, nuint size);

    [LibraryImport(LibraryName, EntryPoint = "llama_state_set_data")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial nuint StateSetData(IntPtr ctx, byte* src, nuint size);

    [LibraryImport(LibraryName, EntryPoint = "llama_state_load_file", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool StateLoadFile(IntPtr ctx, string pathSession, int* tokensOut,
        nuint nTokenCapacity, nuint* nTokenCountOut);

    [LibraryImport(LibraryName, EntryPoint = "llama_state_save_file", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool StateSaveFile(IntPtr ctx, string pathSession, int* tokens, nuint nTokenCount);
}
