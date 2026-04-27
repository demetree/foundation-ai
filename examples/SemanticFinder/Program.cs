using System.Diagnostics;
using Foundation.AI.Embed;
using Foundation.AI.Inference.Onnx;
using Foundation.AI.MarkItDown;
using Foundation.AI.VectorStore;
using Foundation.AI.VectorStore.Zvec;
using Microsoft.Extensions.DependencyInjection;

// SemanticFinder
//
// A no-LLM-required demo of Foundation.AI's embedding + vector-store stack.
// Indexes a folder of documents and lets you query by meaning rather than
// keyword. Now wired through MarkItDown so the folder can contain mixed
// formats -- PDF, Office, HTML, JSON, CSV, etc. -- not just plain text.
//
// First-run UX: the embedding model (Xenova/all-MiniLM-L6-v2, ~80 MB) is
// auto-downloaded from HuggingFace if not already present. After that every
// run is instant.
//
// Try queries like:
//   > South American pack animals
//   > running large language models on a laptop
//   > polyphonic baroque composer
// None of those phrases appear in the documents. Semantic search finds them
// anyway -- that is the whole point.

const string CollectionName = "semantic-finder";
const string ModelDir       = "./ai-models/all-MiniLM-L6-v2";
const string VectorDir      = "./vectors";

string dataDir = args.Length > 0 ? args[0] : "./sample-data";
if (!Directory.Exists(dataDir))
{
    Console.Error.WriteLine($"Data directory not found: {Path.GetFullPath(dataDir)}");
    Console.Error.WriteLine("Pass a folder path as the first argument, or run from a directory containing 'sample-data/'.");
    return 1;
}

// 1. Make sure the embedding model is on disk. EnsureModelExistsAsync hits
// the HuggingFace tree API to discover the file list, downloads anything
// missing, and skips files whose local size already matches.
Directory.CreateDirectory(ModelDir);
var downloader = new OnnxModelDownloader();
await downloader.EnsureModelExistsAsync(ModelDir, new OnnxModelConfig
{
    RepoId = "Xenova/all-MiniLM-L6-v2",
    Branch = "main"
});

// 2. Build the embedding provider, vector store, and MarkItDown converter.
// MarkItDown gets a tiny DI bootstrap because it needs an IEnumerable<IDocumentConverter>
// behind the scenes -- AddMarkItDown() registers every built-in converter (plain
// text, CSV, JSON, XML, HTML, ZIP, EPUB, Jupyter notebooks) plus, because we
// reference Foundation.AI.MarkItDown.Pdf / .Office / .Media, the format-
// specific PDF, DOCX/PPTX/XLSX, and audio-metadata converters too.
await using var embedder = new OnnxEmbeddingProvider(new OnnxEmbeddingConfig
{
    ModelPath = Path.Combine(ModelDir, "model.onnx"),
    ModelName = "all-MiniLM-L6-v2"
});
await using var store = new ZvecVectorStore(new ZvecVectorStoreConfig
{
    BasePath = VectorDir
});

var markItDownServices = new ServiceCollection().AddMarkItDown().BuildServiceProvider();
var markItDown = markItDownServices.GetRequiredService<IMarkItDown>();

// Warmup the embedder. The OnnxEmbeddingProvider lazy-initialises on first
// call, which means embedder.Dimension is 0 until then. A throwaway embed
// resolves both the dimension and pays the ONNX session cold-start cost up
// front instead of folding it into the first user query latency.
await embedder.EmbedAsync("warmup");

// 3. Rebuild the collection from scratch every run -- cheap at this scale and
// keeps the demo deterministic. For a real ingestion pipeline you'd hash the
// inputs and only re-embed what changed (see BMC.RagIngestionJob for the pattern).
if (await store.CollectionExistsAsync(CollectionName))
    await store.DeleteCollectionAsync(CollectionName);

await store.CreateCollectionAsync(CollectionName, embedder.Dimension);

string[] files = Directory.EnumerateFiles(dataDir, "*", SearchOption.AllDirectories)
    .Where(IsSupported)
    .OrderBy(f => f)
    .ToArray();

if (files.Length == 0)
{
    Console.Error.WriteLine($"No supported files found under {Path.GetFullPath(dataDir)}.");
    Console.Error.WriteLine("Drop .pdf, .docx, .pptx, .xlsx, .html, .md, .txt, .csv, .json, or .xml files in there and re-run.");
    return 1;
}

Console.WriteLine($"Indexing {files.Length} files using {embedder.ModelName} ({embedder.Dimension}-dim)...");
var sw = Stopwatch.StartNew();

// Convert every file to clean Markdown via MarkItDown. For .md / .txt this is
// effectively a pass-through; for .pdf / .docx / etc. it's where the heavy
// lifting happens. Sequential rather than parallel because per-file work is
// tiny at this scale and parallel I/O over the same disk doesn't speed it up.
string[] contents = new string[files.Length];
for (int i = 0; i < files.Length; i++)
{
    try
    {
        var result = await markItDown.ConvertFileAsync(files[i]);
        contents[i] = result.Markdown;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ! skipping {Path.GetFileName(files[i])}: {ex.Message}");
        contents[i] = "";
    }
}

// Drop any files that failed to convert before we send them to the embedder.
var indexed = files.Zip(contents)
    .Where(p => !string.IsNullOrWhiteSpace(p.Second))
    .ToArray();

// EmbedBatchAsync is the throughput path -- one ONNX session run for the
// whole batch instead of N round-trips. On CPU at this corpus size you
// won't notice; on CUDA the difference is 10-50x.
float[][] vectors = await embedder.EmbedBatchAsync(indexed.Select(p => p.Second).ToArray());

var docs = new VectorDocument[indexed.Length];
for (int i = 0; i < indexed.Length; i++)
{
    string text = indexed[i].Second;
    string preview = text.Length > 100
        ? text[..100].Replace("\n", " ").Replace("\r", "") + "..."
        : text.Replace("\n", " ").Replace("\r", "");

    docs[i] = new VectorDocument(
        Id: indexed[i].First,
        Vector: vectors[i],
        Metadata: new Dictionary<string, object>
        {
            ["file"] = Path.GetFileName(indexed[i].First),
            ["preview"] = preview
        });
}

await store.UpsertBatchAsync(CollectionName, docs);
await store.FlushAsync(CollectionName);
sw.Stop();
Console.WriteLine($"Indexed {indexed.Length} of {files.Length} files in {sw.ElapsedMilliseconds} ms.\n");

// 4. Interactive REPL. Each query becomes one embedding + one vector search.
Console.WriteLine("Type a query (or 'quit' to exit). A few that show this off:");
Console.WriteLine("  - South American pack animals");
Console.WriteLine("  - running large language models on a laptop");
Console.WriteLine("  - polyphonic baroque composer");
Console.WriteLine("  - in-memory key-value store");
Console.WriteLine();

while (true)
{
    Console.Write("> ");
    string? query = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(query)) continue;
    if (query.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
        query.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    float[] queryVector = await embedder.EmbedAsync(query);
    var results = await store.SearchAsync(CollectionName, queryVector, topK: 5);

    Console.WriteLine();
    int rank = 1;
    foreach (var r in results)
    {
        string file    = r.Metadata?.GetValueOrDefault("file")    as string ?? Path.GetFileName(r.Id);
        string preview = r.Metadata?.GetValueOrDefault("preview") as string ?? "";
        Console.WriteLine($"  {rank}. [{r.Score:F4}]  {file}");
        Console.WriteLine($"       {preview}");
        rank++;
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
