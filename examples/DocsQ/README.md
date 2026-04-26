# DocsQ

> A console RAG demo built on Foundation.AI. Drop documents in a folder -- PDF, Word, HTML, Markdown, plain text -- ask questions in natural language, get answers with citations. **No cloud calls, no API keys, no Python.** Runs entirely in-process using a local Phi-4-mini ONNX model.

## What this shows

- **`IMarkItDown`** for converting arbitrary input formats to Markdown
- **`IRagService.IndexFileAsync`** -- the bridge extension that ties MarkItDown into the RAG pipeline (one call: file -> markdown -> chunks -> embeddings -> vector store)
- **`IRagService.QueryStreamAsync`** for streaming token-by-token answers
- **`OnnxInferenceProvider`** running Microsoft's Phi-4-mini-instruct (cpu-int4) for the LLM
- **`OnnxEmbeddingProvider`** for chunk embeddings
- **`ZvecVectorStore`** for in-process embedded vector storage
- DI composition that mirrors the patterns used in BMC.Server and Scheduler.Server

The whole thing is one [`Program.cs`](Program.cs) file, around 150 lines.

## Run it

```bash
cd examples/DocsQ
dotnet run
```

### **Heads-up about first-run download size**

This sample needs **two** ONNX models:

- `Xenova/all-MiniLM-L6-v2` -- the embedder, ~80 MB.
- `microsoft/Phi-4-mini-instruct-onnx` (cpu-int4 variant) -- the LLM, **~2-3 GB**.

Both auto-download on first run via `OnnxModelDownloader.EnsureModelExistsAsync`, which hits the HuggingFace tree API to discover the file list and skips already-present files on subsequent runs. Allow 5-15 minutes for the initial download on a typical home connection. Files land in `./ai-models/` (gitignored).

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

```bash
dotnet run -- C:\path\to\your\documents
```

Any folder with `.pdf`, `.docx`, `.pptx`, `.xlsx`, `.html`, `.md`, `.txt`, `.csv`, `.json`, or `.xml` files works. MarkItDown handles the format conversion in every case.

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
