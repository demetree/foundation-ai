using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace Foundation.AI.Embed;

/// <summary>
/// Embedding provider using ONNX Runtime for local model inference.
///
/// <para><b>Supported models:</b>
/// Any HuggingFace sentence-transformer model exported to ONNX format.
/// Common models: all-MiniLM-L6-v2 (384d), BGE-small-en-v1.5 (384d),
/// all-e5-small-v2 (384d), BGE-M3 (1024d).</para>
///
/// <para><b>Required files:</b>
/// <list type="bullet">
/// <item><c>model.onnx</c> — The ONNX model file</item>
/// <item><c>vocab.json</c> — BPE vocabulary (from HuggingFace)</item>
/// <item><c>merges.txt</c> — BPE merge rules (from HuggingFace)</item>
/// </list>
/// All files should be in the same directory.</para>
///
/// <para><b>GPU acceleration:</b>
/// When <see cref="OnnxEmbeddingConfig.UseCuda"/> is true, uses CUDA Execution Provider
/// for GPU-accelerated inference. Requires the Microsoft.ML.OnnxRuntime.Gpu NuGet package.
/// Falls back automatically to CPU if CUDA is unavailable.</para>
///
/// <para><b>Thread safety:</b>
/// ONNX Runtime InferenceSession is thread-safe for concurrent Run() calls.
/// The tokenizer is also thread-safe. This provider can be used as a singleton.</para>
/// </summary>
public sealed class OnnxEmbeddingProvider : IEmbeddingProvider
{
    private readonly InferenceSession _session;
    private readonly Tokenizer? _tokenizer;
    private readonly OnnxEmbeddingConfig _config;
    private readonly int _dimension;
    private readonly string _modelName;

    /// <summary>
    /// When true, the tokenizer is for a BERT-family model that requires
    /// [CLS] at the start and [SEP] at the end of input sequences.
    /// </summary>
    private readonly bool _addBertSpecialTokens;
    private readonly long _clsTokenId;
    private readonly long _sepTokenId;

    public int Dimension => _dimension;
    public string ModelName => _modelName;

    public OnnxEmbeddingProvider(OnnxEmbeddingConfig config)
    {
        _config = config;

        if (string.IsNullOrWhiteSpace(config.ModelPath))
            throw new ArgumentException("ModelPath must be specified.", nameof(config));

        if (!File.Exists(config.ModelPath))
            throw new FileNotFoundException($"ONNX model not found: {config.ModelPath}");

        // Create ONNX session with optional CUDA
        var sessionOptions = new SessionOptions();
        if (config.UseCuda)
        {
            try
            {
                sessionOptions.AppendExecutionProvider_CUDA(config.GpuDeviceId);
            }
            catch
            {
                // CUDA not available — fall back to CPU silently
            }
        }

        _session = new InferenceSession(config.ModelPath, sessionOptions);

        // Determine output dimension from model metadata
        var outputMeta = _session.OutputMetadata.First();
        var outputShape = outputMeta.Value.Dimensions;
        // Shape is typically [batch_size, dimension] — dimension is the last axis
        _dimension = outputShape.Length > 1 ? outputShape[^1] : outputShape[0];

        // ── Load tokenizer ───────────────────────────────────────────────────
        // Try multiple strategies in priority order. The tokenizer must match
        // the model architecture — BERT needs WordPiece, GPT-2 needs BPE, etc.
        var modelDir = Path.GetDirectoryName(config.ModelPath)!;

        // Strategy 1: vocab.txt (WordPiece — BERT family models like all-MiniLM-L6-v2)
        {
            var vocabTxtPath = Path.Combine(modelDir, "vocab.txt");
            if (File.Exists(vocabTxtPath))
            {
                try
                {
                    _tokenizer = WordPieceTokenizer.Create(vocabTxtPath);
                    _addBertSpecialTokens = true;
                    // Read [CLS] and [SEP] token IDs from vocab (line number = token ID)
                    var vocabLines = File.ReadAllLines(vocabTxtPath);
                    _clsTokenId = Array.IndexOf(vocabLines, "[CLS]");
                    _sepTokenId = Array.IndexOf(vocabLines, "[SEP]");
                    if (_clsTokenId < 0) _clsTokenId = 101; // BERT default
                    if (_sepTokenId < 0) _sepTokenId = 102; // BERT default
                    Console.WriteLine($"[Foundation.AI.Embed] Loaded WordPiece tokenizer from vocab.txt ([CLS]={_clsTokenId}, [SEP]={_sepTokenId})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Foundation.AI.Embed] Failed to load WordPiece tokenizer ({ex.Message}), trying BPE fallback...");
                }
            }
        }

        // Strategy 2: vocab.json + merges.txt (BPE — GPT-2 family models)
        if (_tokenizer == null)
        {
            var vocabJsonPath = Path.Combine(modelDir, "vocab.json");
            var mergesPath = Path.Combine(modelDir, "merges.txt");
            if (File.Exists(vocabJsonPath) && File.Exists(mergesPath))
            {
                using var vocabStream = File.OpenRead(vocabJsonPath);
                using var mergesStream = File.OpenRead(mergesPath);
                _tokenizer = BpeTokenizer.Create(vocabStream, mergesStream);
                Console.WriteLine($"[Foundation.AI.Embed] Loaded BPE tokenizer from vocab.json + merges.txt");
            }
        }

        if (_tokenizer == null)
        {
            Console.WriteLine("[Foundation.AI.Embed] WARNING: No tokenizer files found — using whitespace fallback. " +
                              "Embeddings will be LOW QUALITY. Provide tokenizer.json, vocab.txt, or vocab.json + merges.txt.");
        }

        _modelName = config.ModelName
            ?? Path.GetFileNameWithoutExtension(config.ModelPath);
    }

    public Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        var embedding = RunInference([text])[0];
        return Task.FromResult(embedding);
    }

    public Task<float[][]> EmbedBatchAsync(IReadOnlyList<string> texts,
        CancellationToken ct = default)
    {
        if (texts.Count == 0)
            return Task.FromResult(Array.Empty<float[]>());

        var embeddings = RunInference(texts);
        return Task.FromResult(embeddings);
    }

    /// <summary>
    /// Run ONNX inference on a batch of texts.
    ///
    /// <para><b>Pipeline:</b>
    /// 1. Tokenize each text into input_ids and attention_mask
    /// 2. Pad sequences to uniform length within the batch
    /// 3. Create ONNX tensors and run the model
    /// 4. Extract embeddings from output (mean pooling over token embeddings)
    /// 5. Optionally L2-normalize the output</para>
    /// </summary>
    private float[][] RunInference(IReadOnlyList<string> texts)
    {
        int batchSize = texts.Count;
        int maxLen = _config.MaxTokenLength;

        // Tokenize each text using the BPE tokenizer
        var allInputIds = new long[batchSize][];
        var allAttentionMasks = new long[batchSize][];
        int actualMaxLen = 0;

        for (int i = 0; i < batchSize; i++)
        {
            long[] tokenIds;

            if (_tokenizer != null)
            {
                // Uncased BERT models require lowercased input
                string inputText = _addBertSpecialTokens ? texts[i].ToLowerInvariant() : texts[i];
                var encoded = _tokenizer.EncodeToTokens(inputText, out _);

                if (_addBertSpecialTokens)
                {
                    // BERT models: [CLS] + content tokens + [SEP]
                    int maxContentTokens = maxLen - 2;
                    int contentCount = Math.Min(encoded.Count, maxContentTokens);
                    int tokenCount = contentCount + 2;
                    tokenIds = new long[tokenCount];
                    tokenIds[0] = _clsTokenId;
                    for (int j = 0; j < contentCount; j++)
                        tokenIds[j + 1] = encoded[j].Id;
                    tokenIds[tokenCount - 1] = _sepTokenId;
                }
                else
                {
                    int tokenCount = Math.Min(encoded.Count, maxLen);
                    tokenIds = new long[tokenCount];
                    for (int j = 0; j < tokenCount; j++)
                        tokenIds[j] = encoded[j].Id;
                }
            }
            else
            {
                // Last-resort fallback: split on whitespace, assign sequential IDs.
                // Produces meaningless embeddings — only useful for basic smoke testing.
                var words = texts[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int tokenCount = Math.Min(words.Length, maxLen);
                tokenIds = new long[tokenCount];
                for (int j = 0; j < tokenCount; j++)
                    tokenIds[j] = j + 1; // avoid 0 as it's typically padding
            }

            allInputIds[i] = tokenIds;
            allAttentionMasks[i] = new long[tokenIds.Length];
            Array.Fill(allAttentionMasks[i], 1L);
            actualMaxLen = Math.Max(actualMaxLen, tokenIds.Length);
        }

        // Pad to uniform length
        var inputIdsTensor = new DenseTensor<long>(new[] { batchSize, actualMaxLen });
        var attentionMaskTensor = new DenseTensor<long>(new[] { batchSize, actualMaxLen });
        var tokenTypeTensor = new DenseTensor<long>(new[] { batchSize, actualMaxLen });

        for (int i = 0; i < batchSize; i++)
        {
            for (int j = 0; j < allInputIds[i].Length; j++)
            {
                inputIdsTensor[i, j] = allInputIds[i][j];
                attentionMaskTensor[i, j] = allAttentionMasks[i][j];
                // token_type_ids stays 0 (single-segment input)
            }
            // Remaining positions stay 0 (padding)
        }

        // Build ONNX inputs — model may use different input names
        var inputs = new List<NamedOnnxValue>();
        var inputNames = _session.InputMetadata.Keys.ToList();

        foreach (var name in inputNames)
        {
            if (name.Contains("input_id", StringComparison.OrdinalIgnoreCase))
                inputs.Add(NamedOnnxValue.CreateFromTensor(name, inputIdsTensor));
            else if (name.Contains("attention", StringComparison.OrdinalIgnoreCase))
                inputs.Add(NamedOnnxValue.CreateFromTensor(name, attentionMaskTensor));
            else if (name.Contains("token_type", StringComparison.OrdinalIgnoreCase))
                inputs.Add(NamedOnnxValue.CreateFromTensor(name, tokenTypeTensor));
        }

        // Run model
        using var results = _session.Run(inputs);
        var output = results.First();
        var outputTensor = output.AsTensor<float>();

        // Extract embeddings — handle both [batch, dim] and [batch, seq, dim] shapes
        var embeddings = new float[batchSize][];
        var shape = outputTensor.Dimensions.ToArray();

        if (shape.Length == 2)
        {
            // Direct [batch, dim] output (sentence-transformers with pooling built in)
            for (int i = 0; i < batchSize; i++)
            {
                embeddings[i] = new float[_dimension];
                for (int d = 0; d < _dimension; d++)
                    embeddings[i][d] = outputTensor[i, d];
            }
        }
        else if (shape.Length == 3)
        {
            // [batch, seq_len, dim] output — apply mean pooling over token positions
            // Mean pooling: average token embeddings, weighted by attention mask
            for (int i = 0; i < batchSize; i++)
            {
                embeddings[i] = new float[_dimension];
                int tokenCount = allInputIds[i].Length;

                for (int d = 0; d < _dimension; d++)
                {
                    float sum = 0;
                    for (int s = 0; s < tokenCount; s++)
                        sum += outputTensor[i, s, d];
                    embeddings[i][d] = sum / tokenCount;
                }
            }
        }

        // Optionally normalize
        if (_config.NormalizeOutput)
        {
            for (int i = 0; i < batchSize; i++)
                L2Normalize(embeddings[i]);
        }

        return embeddings;
    }

    /// <summary>In-place L2 normalization: v = v / ||v||</summary>
    private static void L2Normalize(float[] vector)
    {
        float sumSq = 0;
        for (int i = 0; i < vector.Length; i++)
            sumSq += vector[i] * vector[i];

        if (sumSq <= 0) return;

        float invNorm = 1f / MathF.Sqrt(sumSq);
        for (int i = 0; i < vector.Length; i++)
            vector[i] *= invNorm;
    }

    public ValueTask DisposeAsync()
    {
        _session.Dispose();
        return ValueTask.CompletedTask;
    }
}
