using System.Diagnostics;
using Foundation.AI.Embed;
using Foundation.AI.Inference;
using Foundation.AI.Inference.LlamaSharp;
using Foundation.AI.Inference.Onnx;
using Foundation.AI.MarkItDown;
using Foundation.AI.Rag;
using Foundation.AI.VectorStore;
using Foundation.AI.VectorStore.Zvec;
using Microsoft.Extensions.DependencyInjection;

// DocsQ -- "Talk to your documents"
//
// A console RAG demo built on Foundation.AI. Indexes every file in a folder
// (PDF, DOCX, HTML, MD, TXT) using MarkItDown for format conversion, then lets
// you ask questions in natural language. Answers stream token by token from a
// local LLM with no cloud calls; each answer includes citations to the source
// chunks the model relied on.
//
// Three inference backends, picked by the DOCSQ_BACKEND env var:
//
//   onnx        (default) -- Phi-4-mini-instruct cpu-int4 ONNX (~4.7 GB).
//                Smallest viable model, no extra software. Fast download,
//                mediocre answers, occasional repetition on simple questions.
//
//   llamasharp -- Qwen3-8B-Q4_K_M GGUF via LLamaSharp (~5.0 GB). Self-contained
//                (no external service); needs LLamaSharp.Backend.Cpu (already
//                referenced) or a GPU backend NuGet for acceleration.
//
//   ollama     -- Qwen3:8b via OpenAI-compatible HTTP at http://localhost:11434.
//                Requires Ollama installed and `ollama pull qwen3:8b`. This is
//                what BMC.Server uses in production (see HunterInferenceFactory)
//                -- Ollama manages the model and you can swap it out at runtime.
//
// Same RAG pipeline, same Foundation.AI abstractions -- only the
// IInferenceProvider implementation changes.

const string Collection = "docsq";
const string VectorDir  = "./vectors";

// ─── Backend selection ──────────────────────────────────────────────────────
string backend = (Environment.GetEnvironmentVariable("DOCSQ_BACKEND")
                  ?? "onnx").ToLowerInvariant();
if (backend != "onnx" && backend != "llamasharp" && backend != "ollama")
{
    Console.Error.WriteLine($"Unknown DOCSQ_BACKEND value '{backend}'. Use 'onnx', 'llamasharp', or 'ollama'.");
    return 1;
}

// ─── Embedder location (same for both backends) ─────────────────────────────
string EmbedDir = Environment.GetEnvironmentVariable("DOCSQ_EMBED_DIR")
                  ?? "./ai-models/all-MiniLM-L6-v2";

// ─── Inference model location ───────────────────────────────────────────────
// ONNX: a directory containing model.onnx + tokenizer + genai_config.json
// LLamaSharp: a single .gguf file
string OnnxInferDir = Environment.GetEnvironmentVariable("DOCSQ_INFER_DIR")
                      ?? "./ai-models/Phi-4-mini-instruct-onnx";
string GgufPath = Environment.GetEnvironmentVariable("DOCSQ_LLAMASHARP_GGUF")
                  ?? "./ai-models/Qwen3-8B-GGUF/Qwen3-8B-Q4_K_M.gguf";

// Ollama coordinates (only used when DOCSQ_BACKEND=ollama). Defaults match
// Ollama's out-of-the-box configuration; override either if you've changed
// the listen address or want to point at a different model.
string OllamaEndpoint = Environment.GetEnvironmentVariable("DOCSQ_OLLAMA_ENDPOINT")
                       ?? "http://localhost:11434/v1/chat/completions";
string OllamaModel = Environment.GetEnvironmentVariable("DOCSQ_OLLAMA_MODEL")
                     ?? "qwen3:8b";

string docsDir = args.Length > 0 ? args[0] : "./sample-docs";
if (!Directory.Exists(docsDir))
{
    Console.Error.WriteLine($"Documents directory not found: {Path.GetFullPath(docsDir)}");
    Console.Error.WriteLine("Pass a folder path as the first argument, or run from a directory containing 'sample-docs/'.");
    return 1;
}

Console.WriteLine($"DocsQ backend: {backend}");

// ─── Step 1: ensure all needed model files are on disk ──────────────────────
//
// Embedder: hits the HuggingFace tree API, downloads any missing files,
// skips files whose local size already matches.
//
// ONNX inference: same pattern, but for the Phi-4-mini repo + subfolder.
//
// LLamaSharp inference: a single GGUF file -- hand-rolled HttpClient download
// at the bottom of this file (the OnnxModelDownloader downloads ENTIRE repos,
// and the Qwen3-8B-GGUF repo has six quantisation variants totalling ~30 GB
// that we don't want).
//
// First-run download size by backend:
//   onnx       -> ~80 MB embedder + ~4.7 GB Phi-4-mini    = ~4.8 GB
//   llamasharp -> ~80 MB embedder + ~5.0 GB Qwen3-8B GGUF = ~5.1 GB
//   ollama     -> ~80 MB embedder; the LLM is managed by Ollama itself
//                 (`ollama pull qwen3:8b` -- ~5 GB, in Ollama's own cache)
Directory.CreateDirectory(EmbedDir);
var hfDownloader = new OnnxModelDownloader();
await hfDownloader.EnsureModelExistsAsync(EmbedDir, new OnnxModelConfig
{
    RepoId = "Xenova/all-MiniLM-L6-v2",
    Branch = "main"
});

switch (backend)
{
    case "onnx":
        Directory.CreateDirectory(OnnxInferDir);
        await hfDownloader.EnsureModelExistsAsync(OnnxInferDir, new OnnxModelConfig
        {
            RepoId = "microsoft/Phi-4-mini-instruct-onnx",
            Branch = "main",
            Subfolder = "cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4"
        });
        break;

    case "llamasharp":
        await EnsureGgufAsync(
            targetPath: GgufPath,
            url: "https://huggingface.co/Qwen/Qwen3-8B-GGUF/resolve/main/Qwen3-8B-Q4_K_M.gguf");
        break;

    case "ollama":
        // Nothing to download here -- Ollama manages models on the user's behalf.
        // User must have: (1) installed Ollama (https://ollama.com),
        //                 (2) pulled the model: `ollama pull qwen3:8b`,
        //                 (3) Ollama serving on the configured endpoint.
        Console.WriteLine($"[DocsQ] Using Ollama at {OllamaEndpoint} with model '{OllamaModel}'.");
        Console.WriteLine($"[DocsQ] If you see a connection error below, run: ollama pull {OllamaModel}");
        break;
}

// ─── Step 2: wire up DI ─────────────────────────────────────────────────────
var services = new ServiceCollection();
services.AddOnnxEmbedding(c =>
{
    c.ModelPath = Path.Combine(EmbedDir, "model.onnx");
    c.ModelName = "all-MiniLM-L6-v2";
});
services.AddVectorStore(_ => new ZvecVectorStore(new ZvecVectorStoreConfig
{
    BasePath = VectorDir
}));
switch (backend)
{
    case "onnx":
        services.AddSingleton<IInferenceProvider>(_ =>
            new OnnxInferenceProvider(OnnxInferDir, "phi-4-mini-instruct"));
        break;

    case "llamasharp":
        services.AddLlamaSharpInference(c =>
        {
            c.ModelPath = GgufPath;
            c.ModelName = "qwen3-8b-q4-k-m";
            c.GpuLayerCount = 0; // CPU only -- raise if you swap in a GPU backend
            c.ContextSize = 8192;
            c.DefaultPromptTemplate = ChatTemplates.Qwen3;
        });
        break;

    case "ollama":
        services.AddOpenAiInference(c =>
        {
            c.Endpoint = OllamaEndpoint;
            c.Model = OllamaModel;
            c.ApiKey = "ollama"; // Ollama ignores the key but the client wants non-empty
        });
        break;
}
services.AddMarkItDown();
services.AddRag();

await using var sp = services.BuildServiceProvider();

var embedder   = sp.GetRequiredService<IEmbeddingProvider>();
var store      = sp.GetRequiredService<IVectorStore>();
var inference  = sp.GetRequiredService<IInferenceProvider>();
var markItDown = sp.GetRequiredService<IMarkItDown>();
var rag        = sp.GetRequiredService<IRagService>();

// ─── Step 3: warm both models (pay cold-start once, not per-query) ─────────
Console.WriteLine($"Warming up embedder + LLM ({inference.ModelName})...");
var warmup = Stopwatch.StartNew();
await embedder.EmbedAsync("warmup");
await inference.GenerateAsync("hi", new InferenceOptions { MaxTokens = 1 });
warmup.Stop();
Console.WriteLine($"Warm in {warmup.ElapsedMilliseconds} ms.");

// ─── Step 4: rebuild the collection from scratch ───────────────────────────
if (await store.CollectionExistsAsync(Collection))
    await store.DeleteCollectionAsync(Collection);

await store.CreateCollectionAsync(Collection, embedder.Dimension);

string[] files = Directory.EnumerateFiles(docsDir, "*", SearchOption.AllDirectories)
    .Where(IsSupported)
    .OrderBy(f => f)
    .ToArray();

if (files.Length == 0)
{
    Console.Error.WriteLine($"No supported files found under {Path.GetFullPath(docsDir)}.");
    Console.Error.WriteLine("Drop .pdf, .docx, .html, .md, or .txt files in there and re-run.");
    return 1;
}

Console.WriteLine($"\nIndexing {files.Length} document(s) from {Path.GetFullPath(docsDir)}...");
var indexSw = Stopwatch.StartNew();
foreach (var file in files)
{
    var perFile = Stopwatch.StartNew();
    await rag.IndexFileAsync(markItDown, Collection, Path.GetFileName(file), file);
    perFile.Stop();
    Console.WriteLine($"  + {Path.GetFileName(file),-40} {perFile.ElapsedMilliseconds,5} ms");
}
indexSw.Stop();
Console.WriteLine($"Indexed in {indexSw.ElapsedMilliseconds} ms total.\n");

// ─── Step 5: REPL (stream answer, then list source citations) ──────────────
Console.WriteLine("Ask a question, or type 'quit' to exit.\n");

while (true)
{
    Console.Write("> ");
    string? question = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(question)) continue;
    if (question.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
        question.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    var ragOptions = new RagOptions
    {
        Collection = Collection,
        TopK = 4,
        MaxTokens = 512,
        Temperature = 0.3f,
        // Small penalty breaks Phi-4-mini's repetition loops without hurting
        // answer fluency. Qwen3-8B doesn't need it but it's harmless to leave
        // on for both backends.
        RepetitionPenalty = 1.1f
    };

    Console.WriteLine();
    var queryStart = Stopwatch.StartNew();
    int tokens = 0;
    await foreach (var token in rag.QueryStreamAsync(question, ragOptions))
    {
        Console.Write(token);
        tokens++;
    }
    queryStart.Stop();

    var withSources = await rag.QueryAsync(question, ragOptions);
    Console.WriteLine($"\n\n  ({tokens} tokens in {queryStart.ElapsedMilliseconds} ms)");
    Console.WriteLine("  Sources:");
    foreach (var s in withSources.Sources)
    {
        string excerpt = s.Excerpt.Length > 100
            ? s.Excerpt[..100].Replace("\n", " ").Replace("\r", "") + "..."
            : s.Excerpt.Replace("\n", " ").Replace("\r", "");
        Console.WriteLine($"    [{s.Score:F3}] {s.DocId}: {excerpt}");
    }
    Console.WriteLine();
}

return 0;

static bool IsSupported(string path)
{
    string ext = Path.GetExtension(path).ToLowerInvariant();
    return ext is ".pdf" or ".docx" or ".pptx" or ".xlsx"
        or ".html" or ".htm" or ".md" or ".txt"
        or ".csv" or ".json" or ".xml";
}

// ─── Inline single-file HuggingFace download ────────────────────────────────
//
// OnnxModelDownloader downloads entire repos via the HF tree API. For Qwen3-
// 8B-GGUF that would pull six quantisation variants (~30 GB) when we only
// want one file (~5 GB). This helper grabs a single URL with a percent-
// progress indicator and a generous timeout. No retry / resume; if the
// network drops mid-download, delete the partial file and re-run.
static async Task EnsureGgufAsync(string targetPath, string url)
{
    if (File.Exists(targetPath))
    {
        Console.WriteLine($"[DocsQ] GGUF already present at {targetPath}, skipping download.");
        return;
    }

    string? dir = Path.GetDirectoryName(targetPath);
    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

    Console.WriteLine($"[DocsQ] Downloading {Path.GetFileName(targetPath)} from {url}");
    Console.WriteLine($"[DocsQ] Target: {Path.GetFullPath(targetPath)}");

    using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(60) };
    using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();
    long? totalBytes = response.Content.Headers.ContentLength;

    await using var inStream = await response.Content.ReadAsStreamAsync();
    await using var outStream = File.Create(targetPath);

    var buffer = new byte[1 << 17]; // 128 KB
    long copied = 0;
    int read;
    int lastReportedPercent = -1;

    while ((read = await inStream.ReadAsync(buffer)) > 0)
    {
        await outStream.WriteAsync(buffer.AsMemory(0, read));
        copied += read;

        if (totalBytes is { } total && total > 0)
        {
            int percent = (int)(100 * copied / total);
            if (percent != lastReportedPercent && percent % 2 == 0)
            {
                Console.Write($"\r[DocsQ] {percent,3}% ({copied / (1024 * 1024)} MB / {total / (1024 * 1024)} MB)   ");
                lastReportedPercent = percent;
            }
        }
    }

    Console.WriteLine();
    Console.WriteLine($"[DocsQ] Downloaded {copied / (1024 * 1024)} MB.");
}
