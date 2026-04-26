using System.Diagnostics;
using Foundation.AI.Embed;
using Foundation.AI.Inference;
using Foundation.AI.Inference.Onnx;
using Foundation.AI.MarkItDown;
using Foundation.AI.Rag;
using Foundation.AI.VectorStore;
using Foundation.AI.VectorStore.Zvec;
using Microsoft.Extensions.DependencyInjection;

// DocsQ -- "Talk to your documents"
//
// A ~150-line console RAG demo built on Foundation.AI. Indexes every file in
// a folder (PDF, DOCX, HTML, MD, TXT) using MarkItDown for format conversion,
// then lets you ask questions in natural language. Answers stream token by
// token from a local Phi-4-mini ONNX model with no cloud calls; each answer
// includes citations to the source chunks the model relied on.
//
// Stack:
//   - MarkItDown            : convert any input format -> markdown
//   - Foundation.AI.Embed   : ONNX all-MiniLM-L6-v2 (~80 MB)
//   - Foundation.AI.VectorStore.Zvec : in-process embedded vector DB
//   - Foundation.AI.Inference.Onnx   : Phi-4-mini-instruct (~2-3 GB on first run!)
//   - Foundation.AI.Rag     : ties chunking + retrieval + synthesis together

const string Collection      = "docsq";
const string VectorDir       = "./vectors";

// Model directories. Default to ./ai-models/ alongside the sample, but allow
// override via env var so users with a small system drive can park the
// 4.7 GB Phi-4-mini somewhere else (e.g. an external SSD) without editing code:
//   set DOCSQ_INFER_DIR=D:\AI-Models\Phi-4-mini-instruct-onnx
string EmbedDir = Environment.GetEnvironmentVariable("DOCSQ_EMBED_DIR")
                  ?? "./ai-models/all-MiniLM-L6-v2";
string InferDir = Environment.GetEnvironmentVariable("DOCSQ_INFER_DIR")
                  ?? "./ai-models/Phi-4-mini-instruct-onnx";

string docsDir = args.Length > 0 ? args[0] : "./sample-docs";
if (!Directory.Exists(docsDir))
{
    Console.Error.WriteLine($"Documents directory not found: {Path.GetFullPath(docsDir)}");
    Console.Error.WriteLine("Pass a folder path as the first argument, or run from a directory containing 'sample-docs/'.");
    return 1;
}

// 1. Make sure both ONNX models are on disk. EnsureModelExistsAsync hits the
// HuggingFace tree API to discover the file list, downloads anything missing,
// and skips files whose local size already matches.
//
// First run will download ~80 MB (embedder) plus ~4.7 GB (Phi-4-mini int4) --
// allow ~10-20 minutes on a typical home connection, and have ~5 GB free
// on the drive where this sample is checked out. Subsequent runs are instant.
Directory.CreateDirectory(EmbedDir);
Directory.CreateDirectory(InferDir);

var downloader = new OnnxModelDownloader();
await downloader.EnsureModelExistsAsync(EmbedDir, new OnnxModelConfig
{
    RepoId = "Xenova/all-MiniLM-L6-v2",
    Branch = "main"
});
await downloader.EnsureModelExistsAsync(InferDir, new OnnxModelConfig
{
    RepoId = "microsoft/Phi-4-mini-instruct-onnx",
    Branch = "main",
    // CPU int4 build -- works without a GPU. Swap to
    // "directml/directml-int4-awq-block-128" if you have a DirectX-12 GPU
    // and want roughly an order of magnitude faster generation.
    Subfolder = "cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4"
});

// 2. Wire up DI. Foundation.AI is built around IEmbeddingProvider +
// IVectorStore + IInferenceProvider as separate concerns; AddRag() ties them
// together. AddMarkItDown() registers the converter chain that handles PDF,
// Office, HTML, ZIP, EPUB, plain text, etc.
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
services.AddSingleton<IInferenceProvider>(_ =>
    new OnnxInferenceProvider(InferDir, "phi-4-mini-instruct"));
services.AddMarkItDown();
services.AddRag();

await using var sp = services.BuildServiceProvider();

var embedder    = sp.GetRequiredService<IEmbeddingProvider>();
var store       = sp.GetRequiredService<IVectorStore>();
var inference   = sp.GetRequiredService<IInferenceProvider>();
var markItDown  = sp.GetRequiredService<IMarkItDown>();
var rag         = sp.GetRequiredService<IRagService>();

// 3. Pay both models' cold-start costs up front. Without these, the first
// indexed file blocks on the embedder's ONNX session creation, and the first
// query blocks on Phi-4's session + KV cache setup. Doing them now means the
// timing numbers we print later are honest.
Console.WriteLine("Warming up embedder + LLM...");
var warmup = Stopwatch.StartNew();
await embedder.EmbedAsync("warmup");
await inference.GenerateAsync("hi", new InferenceOptions { MaxTokens = 1 });
warmup.Stop();
Console.WriteLine($"Warm in {warmup.ElapsedMilliseconds} ms.");

// 4. Rebuild the collection from scratch. Cheap at this scale; for a real
// app you would hash file contents and only re-index what changed (BMC's
// BmcRagIngestionJob is the reference for that pattern).
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
    // IndexFileAsync is the bridge extension in Foundation.AI.MarkItDown:
    // converts the file to markdown, chunks it, embeds the chunks, and
    // upserts them into the vector store under the given doc id.
    await rag.IndexFileAsync(markItDown, Collection, Path.GetFileName(file), file);
    perFile.Stop();
    Console.WriteLine($"  + {Path.GetFileName(file),-40} {perFile.ElapsedMilliseconds,5} ms");
}
indexSw.Stop();
Console.WriteLine($"Indexed in {indexSw.ElapsedMilliseconds} ms total.\n");

// 5. Interactive REPL. Each query streams tokens as they come, then prints
// the source chunks the retriever actually pulled in. Sources are only
// available on the non-streaming QueryAsync path -- so we run both: stream
// for the answer text, then a second QueryAsync for the source list.
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
        Temperature = 0.3f
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

    // Show citations after the streaming answer. Re-uses the same retrieval
    // path -- the second call is cheap because the embedder caches nothing
    // user-visible and the vector store query is sub-millisecond at this scale.
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
