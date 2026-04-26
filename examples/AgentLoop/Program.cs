using System.Diagnostics;
using Foundation.AI.Embed;
using Foundation.AI.Examples.AgentLoop;
using Foundation.AI.Inference;
using Foundation.AI.Inference.LlamaSharp;
using Foundation.AI.Inference.Onnx;
using Foundation.AI.VectorStore;
using Foundation.AI.VectorStore.Zvec;
using Microsoft.Extensions.DependencyInjection;

// AgentLoop -- Multi-hop tool-calling research agent
//
// A console sample showing how Foundation.AI's IInferenceProvider tool-calling
// surface looks in practice. The agent has 5 tools over a small markdown KB
// and is asked questions that genuinely require multiple hops (browse, read,
// compare, search, finalize) to answer.
//
// Same three-backend toggle as DocsQ (AGENTLOOP_BACKEND=onnx | llamasharp | ollama).
// The default is 'ollama' here rather than 'onnx' because multi-hop reasoning
// is exactly where Phi-4-mini-cpu-int4 falls down -- if you start on the
// default ONNX backend the agent will likely fail interestingly within 3-4
// hops. That's a feature for the demo, but not a great first impression.

const string VectorDir = "./vectors";

string backend = (Environment.GetEnvironmentVariable("AGENTLOOP_BACKEND") ?? "ollama").ToLowerInvariant();
if (backend != "onnx" && backend != "llamasharp" && backend != "ollama")
{
    Console.Error.WriteLine($"Unknown AGENTLOOP_BACKEND value '{backend}'. Use 'onnx', 'llamasharp', or 'ollama'.");
    return 1;
}

string EmbedDir       = Environment.GetEnvironmentVariable("AGENTLOOP_EMBED_DIR")       ?? "./ai-models/all-MiniLM-L6-v2";
string OnnxInferDir   = Environment.GetEnvironmentVariable("AGENTLOOP_INFER_DIR")       ?? "./ai-models/Phi-4-mini-instruct-onnx";
string GgufPath       = Environment.GetEnvironmentVariable("AGENTLOOP_LLAMASHARP_GGUF") ?? "./ai-models/Qwen3-8B-GGUF/Qwen3-8B-Q4_K_M.gguf";
string OllamaEndpoint = Environment.GetEnvironmentVariable("AGENTLOOP_OLLAMA_ENDPOINT") ?? "http://localhost:11434/v1/chat/completions";
string OllamaModel    = Environment.GetEnvironmentVariable("AGENTLOOP_OLLAMA_MODEL")    ?? "qwen3:8b";

string corpusDir = args.Length > 0 ? args[0] : "./sample-docs";
if (!Directory.Exists(corpusDir))
{
    Console.Error.WriteLine($"Corpus directory not found: {Path.GetFullPath(corpusDir)}");
    Console.Error.WriteLine("Pass a folder path as the first argument, or run from a directory containing 'sample-docs/'.");
    return 1;
}

Console.WriteLine($"AgentLoop backend: {backend}");

// ─── Step 1: ensure model files are on disk ────────────────────────────────
//
// Embedder: shared across all backends. Download via OnnxModelDownloader.
// Inference model: backend-specific.
//
// Note: AgentLoop deliberately does NOT include an inline GGUF downloader
// like DocsQ does -- if you want the LLamaSharp backend, run DocsQ first to
// get the GGUF (or set AGENTLOOP_LLAMASHARP_GGUF to an existing path). This
// keeps Program.cs focused on the agent loop instead of model plumbing.
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
        if (!File.Exists(GgufPath))
        {
            Console.Error.WriteLine($"GGUF not found at {GgufPath}.");
            Console.Error.WriteLine("Either:");
            Console.Error.WriteLine("  - Run DocsQ first with DOCSQ_BACKEND=llamasharp (it downloads the GGUF), or");
            Console.Error.WriteLine("  - Set AGENTLOOP_LLAMASHARP_GGUF to point at an existing GGUF file.");
            return 1;
        }
        break;

    case "ollama":
        Console.WriteLine($"[AgentLoop] Using Ollama at {OllamaEndpoint} with model '{OllamaModel}'.");
        Console.WriteLine($"[AgentLoop] If you see a connection error, run: ollama pull {OllamaModel}");
        break;
}

// ─── Step 2: DI ───────────────────────────────────────────────────────────
var services = new ServiceCollection();
services.AddOnnxEmbedding(c =>
{
    c.ModelPath = Path.Combine(EmbedDir, "model.onnx");
    c.ModelName = "all-MiniLM-L6-v2";
});
services.AddVectorStore(_ => new ZvecVectorStore(new ZvecVectorStoreConfig { BasePath = VectorDir }));

switch (backend)
{
    case "onnx":
        services.AddSingleton<IInferenceProvider>(_ =>
            new OnnxInferenceProvider(OnnxInferDir, "phi-4-mini-instruct"));
        break;

    case "llamasharp":
        services.AddLlamaSharpInference(c =>
        {
            c.ModelPath             = GgufPath;
            c.ModelName             = "qwen3-8b-q4-k-m";
            c.GpuLayerCount         = 0;
            c.ContextSize           = 8192;
            c.DefaultPromptTemplate = ChatTemplates.Qwen3;
        });
        break;

    case "ollama":
        services.AddOpenAiInference(c =>
        {
            c.Endpoint = OllamaEndpoint;
            c.Model    = OllamaModel;
            c.ApiKey   = "ollama";
        });
        break;
}

await using var sp = services.BuildServiceProvider();

var embedder  = sp.GetRequiredService<IEmbeddingProvider>();
var store     = sp.GetRequiredService<IVectorStore>();
var inference = sp.GetRequiredService<IInferenceProvider>();

// ─── Step 3: warm both models ─────────────────────────────────────────────
Console.WriteLine($"Warming up embedder + LLM ({inference.ModelName})...");
var warmup = Stopwatch.StartNew();
await embedder.EmbedAsync("warmup");
await inference.GenerateAsync("hi", new InferenceOptions { MaxTokens = 1 });
warmup.Stop();
Console.WriteLine($"Warm in {warmup.ElapsedMilliseconds} ms.");

// ─── Step 4: index the KB (chunk + embed + store) ─────────────────────────
var tools = new KbTools(embedder, store, inference);
Console.WriteLine($"\nIndexing corpus from {Path.GetFullPath(corpusDir)}...");
var indexSw = Stopwatch.StartNew();
await tools.IndexCorpusAsync(corpusDir);
indexSw.Stop();
Console.WriteLine($"Indexed in {indexSw.ElapsedMilliseconds} ms.\n");

// ─── Step 5: REPL ─────────────────────────────────────────────────────────
bool isOnnxPhi4 = backend == "onnx";
var runner = new AgentRunner(inference, tools, isOnnxPhi4, verbose: true);

Console.WriteLine("Ask the agent a multi-hop question (or 'quit' to exit). Three to try:");
Console.WriteLine("  - Compare what each document says about model selection.");
Console.WriteLine("  - Which two of the included documents would be most useful preparation for understanding RAG?");
Console.WriteLine("  - Find any sentence in the corpus that mentions both 'cantus firmus' and 'composer'.");
Console.WriteLine();

while (true)
{
    Console.Write("> ");
    string? question = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(question)) continue;
    if (question.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
        question.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    var sw = Stopwatch.StartNew();
    KbContext result = await runner.RunAsync(question);
    sw.Stop();

    Console.WriteLine();
    Console.WriteLine(new string('=', 78));
    Console.WriteLine($"FINAL ({result.TerminalStatus} in {sw.ElapsedMilliseconds} ms)");
    Console.WriteLine(new string('=', 78));
    Console.WriteLine(result.FinalAnswer ?? "(no answer)");
    Console.WriteLine();
}

return 0;
