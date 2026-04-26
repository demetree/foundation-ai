# Foundation.AI

> A pragmatic .NET 10 stack for embedded LLM inference, embeddings, vector search, RAG, and document processing — built local-first, cloud-optional, and free of any single-provider lock-in.

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

---

## Why this exists

The .NET AI ecosystem in 2026 is split between thin SDK wrappers (one provider each, no abstraction) and Python-port-shaped frameworks that bring their architecture along for the ride. Foundation.AI was extracted from a healthcare scheduling product where these limitations bit hard:

- We needed **the same interface** over OpenAI, Ollama, and on-device ONNX/GGUF models so the deployment target could move without code changes.
- We needed **embedded vector storage** because shipping Postgres+pgvector or Pinecone alongside a desktop app is a non-starter.
- We needed **document → markdown** for 50+ formats so a RAG pipeline could ingest whatever a user dropped on it.
- And we needed all of it to be embeddable inside a single-binary Windows desktop app with no Python runtime.

This is what fell out.

## What's inside

| Module | Purpose |
|---|---|
| `Foundation.AI` | Umbrella facade — DI extensions, sensible defaults |
| `Foundation.AI.Inference` | Unified `IInferenceProvider` interface — chat, completion, streaming, tool-calling |
| `Foundation.AI.Inference.Onnx` | ONNX GenAI runtime (DirectML on Windows, CPU elsewhere) |
| `Foundation.AI.Inference.LlamaSharp` | GGUF inference via LLamaSharp (pluggable CPU / CUDA / Vulkan backend) |
| `Foundation.AI.Inference.BitNet` | 1-bit quantized inference via Microsoft Research's BitNet llama.cpp fork |
| `Foundation.AI.Embed` | Embedding pipeline with ONNX models + batching |
| `Foundation.AI.VectorStore` | `IVectorStore` abstraction |
| `Foundation.AI.VectorStore.Zvec` | Zvec — .NET port of [`alibaba/zvec`](https://github.com/alibaba/zvec); in-process embedded vector database, no external service required |
| `Foundation.AI.Rag` | Production RAG: chunking, retrieval, synthesis, streaming |
| `Foundation.AI.Vision` | Vision/image abstractions |
| `Foundation.AI.MarkItDown[.Pdf / .Office / .Media]` | Convert 50+ document formats to markdown |
| `Foundation.AI.Experiment` | Autonomous LLM experiment runner — C# port and generalization of [`karpathy/autoresearch`](https://github.com/karpathy/autoresearch) |

## Quick start

```bash
dotnet add package Foundation.AI
dotnet add package Foundation.AI.Inference.Onnx
dotnet add package Foundation.AI.VectorStore.Zvec
```

```csharp
// Local inference — runs on CPU, DirectML, or CUDA depending on what you wire up
var services = new ServiceCollection()
    .AddFoundationAi()
    .AddOnnxInference(opts => opts.ModelPath = "models/phi-4-mini-cpu-int4")
    .BuildServiceProvider();

var inference = services.GetRequiredService<IInferenceProvider>();
var response = await inference.ChatAsync(new[] {
    new ChatMessage(ChatRole.User, "Summarize the structure of a bach fugue in two sentences.")
});
```

```csharp
// RAG over a folder of documents
var rag = services.GetRequiredService<IRagEngine>();
await rag.IngestFolderAsync("./docs");
var answer = await rag.AskAsync("What does the deployment guide say about Vulkan backends?");
```

> **Examples:** see [`examples/`](examples/) for runnable end-to-end programs (local inference, RAG over PDFs, document conversion CLI).

## Backend selection — LLamaSharp on modern GPUs

LLamaSharp ships native backends as separate NuGet packages. **Pick exactly one** in your application project:

- `LLamaSharp.Backend.Cpu` — works everywhere
- `LLamaSharp.Backend.Cuda12` — NVIDIA up through Ada Lovelace (sm_89)
- `LLamaSharp.Backend.Vulkan` — **required for RTX 50-series (Blackwell, sm_120)** and works well on AMD/Intel GPUs too

The `Cuda12` backend at version 0.26.0 silently falls back to CPU on Blackwell because no sm_120 kernel ships in that build. If you have a 50-series card and your tokens-per-second look CPU-shaped, that's why — switch to Vulkan.

## Inspirations & credits

This project stands on the shoulders of work done by other people. Where Foundation.AI is a port or close adaptation of an upstream project, the upstream is named here and reproduced in [NOTICE](NOTICE):

- **`Foundation.AI.VectorStore.Zvec`** — .NET port of [`alibaba/zvec`](https://github.com/alibaba/zvec), an in-process embedded vector database originally written in C++ by Alibaba Group. Apache-2.0.
- **`Foundation.AI.Experiment`** — C# port and generalization of [Andrej Karpathy's `autoresearch`](https://github.com/karpathy/autoresearch). The original is a 5-minute-budget train-and-evaluate loop for ML research; this version replaces the runner, agent, and metric parser with interfaces so you can drive any iterative-improvement task that has a measurable outcome. MIT.
- **`Foundation.AI.MarkItDown`** — conceptually inspired by [Microsoft's `markitdown`](https://github.com/microsoft/markitdown) (Python). This is an independent .NET implementation, not a port. MIT.
- **`Foundation.AI.Inference.BitNet`** — built on Microsoft Research's [BitNet](https://github.com/microsoft/BitNet) llama.cpp fork. MIT.

See [NOTICE](NOTICE) for full attribution and third-party license text.

## Status

**Pre-1.0.** APIs may shift. Used in production by a closed-source healthcare product; battle-tested on Windows desktop and Linux server. Coverage of less-trodden paths (ARM64, macOS) is best-effort — issues and PRs welcome.

## Contributing

Issues and PRs welcome. For larger changes please open an issue first to discuss. See [CONTRIBUTING.md](CONTRIBUTING.md) (TODO).

## License

[Apache License 2.0](LICENSE) — Copyright 2026 Demetree Kallergis.
