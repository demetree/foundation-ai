# DocsQ

> A console RAG demo built on Foundation.AI. Drop documents in a folder -- PDF, Word, HTML, Markdown, plain text -- ask questions in natural language, get answers with citations. **No cloud calls, no API keys, no Python.** Runs entirely in-process using a local Phi-4-mini ONNX model.

## What this shows

- **`IMarkItDown`** for converting arbitrary input formats to Markdown
- **`IRagService.IndexFileAsync`** -- the bridge extension that ties MarkItDown into the RAG pipeline (one call: file -> markdown -> chunks -> embeddings -> vector store)
- **`IRagService.QueryStreamAsync`** for streaming token-by-token answers
- **Three pluggable inference backends** behind the same `IInferenceProvider` interface:
  - `OnnxInferenceProvider` for Microsoft Phi-4-mini-instruct (cpu-int4 ONNX)
  - `LlamaSharpInferenceProvider` for Qwen3-8B (GGUF Q4_K_M)
  - `OpenAiInferenceProvider` against an Ollama HTTP endpoint (qwen3:8b)
- **`OnnxEmbeddingProvider`** for chunk embeddings (same regardless of inference backend)
- **`ZvecVectorStore`** for in-process embedded vector storage
- DI composition that mirrors the patterns used in BMC.Server and Scheduler.Server

The whole thing is one [`Program.cs`](Program.cs) file, around 250 lines.

## Run it

```bash
cd examples/DocsQ
dotnet run                                    # default backend: onnx (Phi-4-mini)

# or pick a different backend:
$env:DOCSQ_BACKEND = "ollama";     dotnet run # Ollama-hosted Qwen3:8b -- best quality
$env:DOCSQ_BACKEND = "llamasharp"; dotnet run # LLamaSharp + Qwen3:8b GGUF -- self-contained
```

## Picking a backend

| Backend | Download | Setup | Quality | Speed (CPU) | Self-contained? |
|---|---|---|---|---|---|
| **`onnx`** (default) | ~4.7 GB Phi-4-mini ONNX | None beyond `dotnet run` | Mediocre. Repetition loops on simple Q&A. | ~5-10 tok/s | Yes |
| **`llamasharp`** | ~5.0 GB Qwen3-8B GGUF | None beyond `dotnet run` | Good. | ~3-6 tok/s | Yes |
| **`ollama`** | ~5 GB (managed by Ollama) | Install Ollama; `ollama pull qwen3:8b` | Best. Coherent, well-structured, auto-cites. | ~2-3 tok/s | No |

**The differences are real and obvious.** A query that produces a repetition loop on the ONNX path produces a clean, structured answer with citations on the Ollama path. Try the same question on each backend to see for yourself -- the model matters far more than any single Foundation.AI tuning knob.

The downloads happen on first run via `OnnxModelDownloader.EnsureModelExistsAsync` (for `onnx`) or an inline `HttpClient` (for `llamasharp`); already-present files are skipped on subsequent runs.

## Putting models on a different drive

All paths are configurable via environment variables, so you can park models on whichever drive has space:

```powershell
# ONNX backend
$env:DOCSQ_EMBED_DIR       = "D:\AI-Models\all-MiniLM-L6-v2"
$env:DOCSQ_INFER_DIR       = "D:\AI-Models\Phi-4-mini-instruct-onnx"

# LLamaSharp backend
$env:DOCSQ_LLAMASHARP_GGUF = "D:\AI-Models\Qwen3-8B-Q4_K_M.gguf"

# Ollama backend
$env:DOCSQ_OLLAMA_ENDPOINT = "http://localhost:11434/v1/chat/completions"
$env:DOCSQ_OLLAMA_MODEL    = "qwen3:8b"
```

The relevant downloader creates the directory if it doesn't exist; for Ollama, you just need to have pulled the model.

## Try these questions

The included [`sample-docs/`](sample-docs/) folder has five short Markdown files spanning Foundation.AI architecture, embedded-LLM concepts, .NET 10 features, baroque counterpoint, and search architecture. Try:

| Question | Should pull from |
|---|---|
| `What is GGUF and how does it differ from ONNX?` | embedded-llms-primer.md |
| `Why was the .sln file format replaced in .NET 10?` | dotnet-10-highlights.md |
| `What is the cantus firmus?` | baroque-counterpoint.md |
| `When does keyword search beat semantic search?` | semantic-vs-keyword-search.md |
| `What's the difference between IRagService and Microsoft.Extensions.AI?` | foundation-ai-architecture.md + dotnet-10-highlights.md |

The model will stream the answer token by token, then print citations showing which chunks the retriever pulled and what their relevance scores were.

## Use it on your own documents

This is the headline reason DocsQ exists. Drop a folder with **any mix of supported formats** on the command line and DocsQ will index everything via MarkItDown, then let you ask questions about it:

```powershell
dotnet run -- C:\path\to\your\documents

# Or with a different backend:
$env:DOCSQ_BACKEND = "ollama"; dotnet run -- C:\path\to\your\documents
```

Supported formats: `.pdf`, `.docx`, `.pptx`, `.xlsx`, `.html` / `.htm`, `.md`, `.txt`, `.csv`, `.json`, `.xml`. MarkItDown handles the format conversion transparently — your application code is exactly the same whether the folder contains 100 PDFs, a mix of Word and HTML, or just plain markdown.

## A note on answer quality (with real numbers)

The default `onnx` backend uses Phi-4-mini cpu-int4 -- a 3.8-billion-parameter model quantised to 4 bits per weight. It is the *smallest viable* choice, not the *best*. The model dictates answer quality far more than anything else; the same RAG pipeline produces dramatically different output depending on which backend you pick. Below are real outputs from this sample, against the included `sample-docs/`:

**Question: "What is the cantus firmus?"**

*ONNX / Phi-4-mini:* starts with multiple-choice format, then enters a repetition loop ("The cantus firmus can be played or sung by itself or can be the basis for the counterpoint lines in a composition." × ~20 times) and only stops because it hits the `MaxTokens` ceiling. The retrieval was perfect; the model failed to produce a coherent answer.

*Ollama / Qwen3-8B:* "The cantus firmus is a fixed melody used as the foundation in counterpoint exercises. It is the primary melodic line around which other voices are constructed in contrapuntal techniques. This concept is central to the five species of counterpoint, which involve varying rhythmic relationships between the cantus firmus and other voices. Composers like Mozart, Beethoven, and Brahms studied these exercises, and the practice remains relevant in modern conservatories." -- complete with an auto-generated citation hint.

The Phi-4-mini limits in practice:

- **Occasional repetition loops** on simple lookup questions. The `RepetitionPenalty = 1.1f` setting in `Program.cs` reduces them but doesn't eliminate them.
- **Slow on CPU** -- 5-10 tokens per second on a typical laptop. 15-45 seconds for a complete answer.
- **Limited multi-step reasoning** -- single-shot Q&A over retrieved chunks works; chained reasoning is hit-or-miss.

`DOCSQ_BACKEND=ollama` (or `llamasharp`) cures all three. The trade-off is the install / disk cost listed in the table above.

## Run it faster on a GPU

The default model variant is `cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4` -- pure CPU, works on every machine but slow (a few tokens per second on a typical laptop). If you have a DirectX-12 GPU on Windows, change the `Subfolder` in [Program.cs](Program.cs) to:

```csharp
Subfolder = "directml/directml-int4-awq-block-128"
```

Delete `./ai-models/Phi-4-mini-instruct-onnx/` so the new variant downloads, and re-run. Token generation will be roughly an order of magnitude faster.

## Adapting this to your project

Same as SemanticFinder: copy `Program.cs` into your own console app, swap the `<ProjectReference>` entries in the csproj for `<PackageReference>`s, and you have a working RAG pipeline against your own documents.

## What this demo does *not* show

- **No incremental ingestion.** The whole collection is rebuilt every run. For a real app, see `BMC.RagIngestionJob` for the hash-based delta-detection pattern.
- **No conversation memory.** Each query is independent -- no chat history is carried across the REPL. `IInferenceProvider.ChatAsync` accepts message history if you want to add this.
- **No streaming citations.** The streaming path returns just the answer text; sources are fetched separately after the stream completes. This is a Foundation.AI API constraint, not a UX choice -- it would be straightforward to add a streaming-with-trailer variant if there's demand.
- **No filtering or multi-tenancy.** `RagOptions.Filter` accepts a metadata expression; not exercised here.

## License

Same as Foundation.AI: [Apache-2.0](../../LICENSE).
