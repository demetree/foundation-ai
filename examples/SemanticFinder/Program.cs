using System.Diagnostics;
using Foundation.AI.Embed;
using Foundation.AI.Inference.Onnx;
using Foundation.AI.VectorStore;
using Foundation.AI.VectorStore.Zvec;

// SemanticFinder
//
// A 100-line, no-LLM-required demo of Foundation.AI's embedding + vector-store
// stack. Indexes a folder of text files using a small ONNX embedding model,
// then lets you query by meaning rather than keyword.
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
const string ModelDir = "./ai-models/all-MiniLM-L6-v2";
const string VectorDir = "./vectors";

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

// 2. Build the embedding provider and the vector store. Both are IAsyncDisposable
// so we await-using them to release the ONNX session and flush the Zvec WAL.
await using var embedder = new OnnxEmbeddingProvider(new OnnxEmbeddingConfig
{
    ModelPath = Path.Combine(ModelDir, "model.onnx"),
    ModelName = "all-MiniLM-L6-v2"
});
await using var store = new ZvecVectorStore(new ZvecVectorStoreConfig
{
    BasePath = VectorDir
});

// Warmup the embedder. The OnnxEmbeddingProvider lazy-initialises on first
// call, which means embedder.Dimension is 0 until then. A throwaway embed
// resolves both the dimension and pays the ONNX session cold-start cost up
// front instead of folding it into the first user query latency. (BMC and
// Scheduler do the same thing in their hosted EmbeddingModelDownloadWorker.)
await embedder.EmbedAsync("warmup");

// 3. Rebuild the collection from scratch every run -- cheap at this scale and
// keeps the demo deterministic. For a real ingestion pipeline you'd hash the
// inputs and only re-embed what changed (see BMC.RagIngestionJob for the pattern).
if (await store.CollectionExistsAsync(CollectionName))
    await store.DeleteCollectionAsync(CollectionName);

await store.CreateCollectionAsync(CollectionName, embedder.Dimension);

string[] files = Directory.EnumerateFiles(dataDir, "*", SearchOption.AllDirectories)
    .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
             || f.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
    .OrderBy(f => f)
    .ToArray();

if (files.Length == 0)
{
    Console.Error.WriteLine($"No .txt or .md files found under {Path.GetFullPath(dataDir)}.");
    return 1;
}

Console.WriteLine($"Indexing {files.Length} files using {embedder.ModelName} ({embedder.Dimension}-dim)...");
var sw = Stopwatch.StartNew();

string[] contents = await Task.WhenAll(files.Select(f => File.ReadAllTextAsync(f)));

// EmbedBatchAsync is the throughput path -- one ONNX session run for the
// whole batch instead of N round-trips. On CPU at this corpus size you
// won't notice; on CUDA the difference is 10-50x.
float[][] vectors = await embedder.EmbedBatchAsync(contents);

var docs = new VectorDocument[files.Length];
for (int i = 0; i < files.Length; i++)
{
    string preview = contents[i].Length > 100
        ? contents[i][..100].Replace("\n", " ").Replace("\r", "") + "..."
        : contents[i].Replace("\n", " ").Replace("\r", "");

    docs[i] = new VectorDocument(
        Id: files[i],
        Vector: vectors[i],
        Metadata: new Dictionary<string, object>
        {
            ["file"] = Path.GetFileName(files[i]),
            ["preview"] = preview
        });
}

await store.UpsertBatchAsync(CollectionName, docs);
await store.FlushAsync(CollectionName);
sw.Stop();
Console.WriteLine($"Indexed in {sw.ElapsedMilliseconds} ms.\n");

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
        string file = r.Metadata?.GetValueOrDefault("file") as string ?? Path.GetFileName(r.Id);
        string preview = r.Metadata?.GetValueOrDefault("preview") as string ?? "";
        Console.WriteLine($"  {rank}. [{r.Score:F4}]  {file}");
        Console.WriteLine($"       {preview}");
        rank++;
    }
    Console.WriteLine();
}

return 0;
