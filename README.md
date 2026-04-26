# Foundation.AI

> A pragmatic .NET 10 stack for embedded LLM inference, embeddings, vector search, RAG, and document processing — built local-first, cloud-optional, and free of any single-provider lock-in.

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

---

## Why this exists

The .NET AI ecosystem in 2026 has filled in a lot of gaps, but two specific shapes remain under-served.

**One: embedded AI inside a .NET application.** A desktop app, a worker service, an MCP server, a game on the .NET runtime. Where you want an LLM, embeddings, RAG, and document conversion to all run *in the same process* — without a Python sidecar, without a Postgres+pgvector deployment, and (optionally) without a cloud LLM dependency.

**Two: one coherent stack rather than five libraries to assemble.** The pieces exist — `Microsoft.ML.OnnxRuntimeGenAI`, `LLamaSharp`, `pgvector`, `LangChain.NET` (or hand-rolled chunkers), Microsoft's Python `markitdown`, somebody else's HuggingFace downloader. You can stitch them together. Many people do. Foundation.AI is what the result looks like when one developer needed exactly this shape for two products and got tired of re-stitching.

This project is not trying to be the universal answer. It exists because the maintainer was building two products — a healthcare scheduling system and a Steam game — that needed exactly this shape, and there was nothing off-the-shelf that fit.

## Is this for you?

**You will probably benefit if:**

- You build .NET applications and want to add AI features without operating a separate Python service.
- You ship desktop / single-binary / worker-service / game / regulated-industry workloads where "talk to OpenAI from production" is not on the menu.
- You want **embedded** vector search — no Postgres, no Pinecone, no separate process.
- You want RAG that works in 50 lines of code rather than 500, and document conversion in the same package.
- You want one `IInferenceProvider` interface fronting OpenAI, Azure OpenAI, Ollama, ONNX GenAI, LLamaSharp/GGUF, and BitNet — so you can swap backends without touching application code.

**You should probably look elsewhere if:**

- You are starting fresh on `Microsoft.Extensions.AI` and it's covering your needs — there is no compelling reason to switch.
- You need cloud-native vector search at scale (>100 QPS over millions of vectors). Pinecone, Weaviate, Qdrant, or pgvector are better tools for that job.
- You need the polish and SLAs of a managed product. This is a one-maintainer project shipping in evenings.
- You are building production agentic systems with strict observability/telemetry requirements — those are not yet in this stack.

### How does this compare to `Microsoft.Extensions.AI`?

`Microsoft.Extensions.AI` is the obvious-default for **provider-neutral abstractions** in new .NET AI work. It does that job well. What it does *not* ship is concrete implementations of everything below the abstraction — the embedded vector database, the chunker, the document-conversion pipeline, the model downloader, the RAG orchestrator. You assemble those yourself.

Foundation.AI is more opinionated and more bundled. The `IInferenceProvider` interface is smaller (chat, completion, streaming, tool-calling — that's it). The vector store is shipped in-process and ready. RAG is a one-method-call away. Document conversion is in the same package. The trade-off is exactly what you'd expect: less ceremony, less flexibility.

The two stacks are not in opposition. They target different sweet spots.

## What's inside

| Module | Purpose |
|---|---|
| `Foundation.AI` | Umbrella facade — `AddFoundationAI()` + the builder pattern |
| `Foundation.AI.Inference` | Unified `IInferenceProvider` interface — chat, completion, streaming, tool-calling |
| `Foundation.AI.Inference.Onnx` | ONNX GenAI runtime — Phi-4-mini, Phi-3, etc. (DirectML on Windows, CPU elsewhere). Includes a HuggingFace model downloader. |
| `Foundation.AI.Inference.LlamaSharp` | GGUF inference via LLamaSharp. Pluggable CPU / CUDA / Vulkan backend. |
| `Foundation.AI.Inference.BitNet` | 1-bit quantised inference via Microsoft Research's BitNet llama.cpp fork |
| `Foundation.AI.Embed` | Embedding pipeline with ONNX models + batching, plus an OpenAI-compatible (works against Ollama too) provider |
| `Foundation.AI.VectorStore` | `IVectorStore` abstraction |
| `Foundation.AI.VectorStore.Zvec` | Zvec — .NET port of [`alibaba/zvec`](https://github.com/alibaba/zvec); in-process embedded vector database, no external service required |
| `Foundation.AI.Rag` | RAG: chunking, retrieval, synthesis, streaming, optional `RepetitionPenalty` for small models |
| `Foundation.AI.Vision` | Vision/image abstractions |
| `Foundation.AI.MarkItDown[.Pdf / .Office / .Media]` | Convert PDF, Office (DOCX/PPTX/XLSX), HTML, ZIP, EPUB, plain text, audio metadata, and ~50 other formats to clean Markdown |
| `Foundation.AI.Experiment` | Autonomous LLM experiment runner — C# port and generalisation of [`karpathy/autoresearch`](https://github.com/karpathy/autoresearch) |

## Examples

Three runnable console samples under [`examples/`](examples/), in increasing order of stack depth. Each one is one folder, one `dotnet run` from a clean clone, and one focused thing it proves.

| Sample | What it shows | Cost |
|---|---|---|
| **[SemanticFinder](examples/SemanticFinder/)** | Semantic search over a folder of text files. **No LLM required.** Index 16 sample files and ask "what's a South American pack animal?" — it returns llamas and alpacas without those words appearing in the query. | ~80 MB embedder download |
| **[DocsQ](examples/DocsQ/)** | Full RAG over PDFs / Office / Markdown / HTML. Three swappable inference backends — Phi-4-mini ONNX (small, mediocre answers), Qwen3-8B GGUF via LLamaSharp (good), Qwen3:8b via Ollama (best). Side-by-side answer-quality comparison documented in its README. | ~80 MB + 4.7-5 GB depending on backend |
| **[AgentLoop](examples/AgentLoop/)** | Multi-hop tool-calling research agent. Five tools (browse, read, search, compare, finalize), 8-hop budget. Demonstrates exactly where small-model multi-hop reasoning falls down — Phi-4-mini will visibly fail, Qwen3-8B will succeed. | ~80 MB + LLM (any of the three backends) |

The samples are deliberately small — `SemanticFinder` is one 100-line `Program.cs`, `DocsQ` is ~250 lines, `AgentLoop` is ~140 lines of agent loop + ~340 lines of tool definitions. Copy them into your own project, swap `ProjectReference` for `PackageReference`, and you have a working starting point.

## Quick start

```bash
dotnet add package Foundation.AI
dotnet add package Foundation.AI.Embed
dotnet add package Foundation.AI.VectorStore.Zvec
dotnet add package Foundation.AI.Inference.Onnx       # or LLamaSharp, or use OpenAI-compat against Ollama
dotnet add package Foundation.AI.Rag
dotnet add package Foundation.AI.MarkItDown
dotnet add package Foundation.AI.MarkItDown.Pdf       # add format-specific converters as needed
```

```csharp
using Foundation.AI;
using Foundation.AI.Embed;
using Foundation.AI.Inference;
using Foundation.AI.Inference.Onnx;
using Foundation.AI.MarkItDown;
using Foundation.AI.Rag;
using Foundation.AI.VectorStore;
using Foundation.AI.VectorStore.Zvec;
using Microsoft.Extensions.DependencyInjection;

// 1. Compose the stack via DI.
var services = new ServiceCollection().AddFoundationAI(ai =>
{
    ai.Services.AddOnnxEmbedding(c =>
    {
        c.ModelPath = "./models/all-MiniLM-L6-v2/model.onnx";
        c.ModelName = "all-MiniLM-L6-v2";
    });
    ai.Services.AddVectorStore(_ => new ZvecVectorStore(new ZvecVectorStoreConfig
    {
        BasePath = "./vectors"
    }));
    ai.Services.AddSingleton<IInferenceProvider>(_ =>
        new OnnxInferenceProvider("./models/phi-4-mini-instruct-onnx", "phi-4-mini"));
    ai.Services.AddRag();
    ai.Services.AddMarkItDown();
});
await using var sp = services.BuildServiceProvider();

var rag        = sp.GetRequiredService<IRagService>();
var markItDown = sp.GetRequiredService<IMarkItDown>();

// 2. Index a folder of documents. MarkItDown handles the format conversion;
// IndexFileAsync chunks the result, embeds the chunks, and stores them.
foreach (string file in Directory.EnumerateFiles("./docs"))
{
    await rag.IndexFileAsync(markItDown, "default", Path.GetFileName(file), file);
}

// 3. Ask questions. Sources are attached to every response.
RagResponse response = await rag.QueryAsync(
    "What does the deployment guide say about Vulkan backends?",
    new RagOptions { Collection = "default", TopK = 4 });

Console.WriteLine(response.Answer);
foreach (RagSource s in response.Sources)
    Console.WriteLine($"  [{s.Score:F2}] {s.DocId}: {s.Excerpt[..Math.Min(100, s.Excerpt.Length)]}...");
```

For runnable end-to-end code with sample data, model auto-download, and the three-backend toggle, see **[examples/DocsQ](examples/DocsQ/)**.

## Backend selection — LLamaSharp on modern GPUs

LLamaSharp ships native backends as separate NuGet packages. **Pick exactly one** in your application project:

- `LLamaSharp.Backend.Cpu` — works everywhere
- `LLamaSharp.Backend.Cuda12` — NVIDIA up through Ada Lovelace (sm_89)
- `LLamaSharp.Backend.Vulkan` — **required for RTX 50-series (Blackwell, sm_120)** and works well on AMD/Intel GPUs too

The `Cuda12` backend at version 0.26.0 silently falls back to CPU on Blackwell because no sm_120 kernel ships in that build. If you have a 50-series card and your tokens-per-second look CPU-shaped, that's why — switch to Vulkan.

## A note on answer quality

The most important thing this project teaches, perhaps slightly inadvertently: **the model dictates answer quality far more than any framework choice.** [DocsQ's README](examples/DocsQ/) documents this concretely — the same RAG pipeline produces a clean coherent answer on Qwen3-8B and a repetition loop on Phi-4-mini-cpu-int4 for the same question over the same documents. If your demo isn't impressing people, the answer is almost always "use a bigger model", not "tune the framework".

This is honest data, not a sales pitch. Foundation.AI gives you a uniform interface across many backends precisely so you can make this trade-off explicit and reversible.

## Inspirations & credits

This project stands on the shoulders of work done by other people. Where Foundation.AI is a port or close adaptation of an upstream project, the upstream is named here and reproduced in [NOTICE](NOTICE):

- **`Foundation.AI.VectorStore.Zvec`** — .NET port of [`alibaba/zvec`](https://github.com/alibaba/zvec), an in-process embedded vector database originally written in C++ by Alibaba Group. Apache-2.0.
- **`Foundation.AI.Experiment`** — C# port and generalisation of [Andrej Karpathy's `autoresearch`](https://github.com/karpathy/autoresearch). The original is a 5-minute-budget train-and-evaluate loop for ML research; this version replaces the runner, agent, and metric parser with interfaces so you can drive any iterative-improvement task that has a measurable outcome. MIT.
- **`Foundation.AI.MarkItDown`** — conceptually inspired by [Microsoft's `markitdown`](https://github.com/microsoft/markitdown) (Python). Independent .NET implementation, not a direct port. MIT.
- **`Foundation.AI.Inference.BitNet`** — built on Microsoft Research's [BitNet](https://github.com/microsoft/BitNet) llama.cpp fork (which itself forks [`ggerganov/llama.cpp`](https://github.com/ggerganov/llama.cpp) and uses lookup-table kernels pioneered in [Microsoft Research's T-MAC](https://github.com/microsoft/T-MAC)). MIT.

See [NOTICE](NOTICE) for full attribution and third-party license text.

## Status

**Pre-1.0.** APIs may shift. Used in two unreleased products by the maintainer: a healthcare scheduling system (server-side, Linux) and a Steam-bound game (Windows desktop, BMC.Studio). Battle-tested for those two shapes; coverage of less-trodden paths (ARM64, macOS, large-scale distributed) is best-effort.

What's stable enough to depend on: `IEmbeddingProvider`, `IVectorStore`, `IInferenceProvider`, `IRagService`, `IMarkItDown`. Concrete provider implementations behind those interfaces are stable too.

What's most likely to change: `Foundation.AI.Experiment` (still settling), `Foundation.AI.Vision` (placeholder shape — current implementations are minimal), the chunker configuration surface, additions to `RagOptions`.

## Contributing

Issues and PRs welcome. For larger changes please open an issue first to discuss. See [CONTRIBUTING.md](CONTRIBUTING.md) for setup, scope guidelines, and the small list of conventions the codebase follows.

## License

[Apache License 2.0](LICENSE) — Copyright 2026 Demetree Kallergis.
