# SemanticFinder

> A small console app that demonstrates **semantic search over a folder of documents** using Foundation.AI's embedding + vector-store stack. **No LLM required**, no cloud calls, no Postgres or Pinecone — runs entirely in-process from a single executable.

## What this shows

- `IEmbeddingProvider` (ONNX, all-MiniLM-L6-v2, ~80 MB on CPU)
- `IVectorStore` (Zvec, embedded, file-based, no external service)
- `IMarkItDown` for converting any input format (PDF, Office, HTML, etc.) to clean text before embedding
- Batch ingestion (`EmbedBatchAsync` + `UpsertBatchAsync`)
- The "auto-download model on first run" pattern via `OnnxModelDownloader.EnsureModelExistsAsync`

The whole thing is one [`Program.cs`](Program.cs) file, ~150 lines including comments and prompts.

## Run it

```bash
cd examples/SemanticFinder
dotnet run
```

On the **first run**, the embedding model downloads from HuggingFace (`Xenova/all-MiniLM-L6-v2`, ~80 MB). That goes to `./ai-models/` and is reused on every subsequent run.

The included [`sample-data/`](sample-data/) folder has 16 short text files spanning five semantic clusters (camelids, local AI inference, container tech, classical composers, databases).

## Try these queries

The whole point of semantic search is that **none of these phrases appear verbatim in the documents** — but the system finds the right ones anyway:

| Query | Should find |
|---|---|
| `South American pack animals` | llama, alpaca, vicuna |
| `running large language models on a laptop` | ollama, llama-cpp, onnx-runtime |
| `polyphonic baroque composer` | bach (then mozart, beethoven) |
| `managing containerized workloads at scale` | kubernetes (then containerd, docker) |
| `in-memory key-value store` | redis (specifically, not postgres or sqlite) |
| `ACID-compliant relational database` | postgres (specifically) |

Type `quit` or `exit` to stop, or just close the window.

## Use it on your own files

```powershell
dotnet run -- C:\path\to\your\documents
```

The folder can contain **any mix of supported formats** — `.pdf`, `.docx`, `.pptx`, `.xlsx`, `.html`, `.md`, `.txt`, `.csv`, `.json`, or `.xml`. MarkItDown handles the format conversion transparently before each file is embedded; your application code is exactly the same whether the folder is plain markdown or 200 mixed PDFs and Word docs.

The whole corpus is re-indexed on every run — fine for hundreds of files; for thousands you'd want incremental ingestion (see `BMC.RagIngestionJob` in the parent project for that pattern). Files that fail conversion are skipped with a one-line warning and the rest still index.

## What this demo does *not* show

- **No LLM** — this is pure embedding + vector search. For the "ask a chatbot questions about your PDFs" experience, see the upcoming `examples/DocsQ` sample which adds RAG + inference.
- **No persistence across runs** — the collection is dropped and rebuilt every launch. Real apps would persist and only re-embed changed files.
- **No multi-tenancy, ACL, or filtering** — you can pass a `filter` string to `SearchAsync` for metadata predicates; not exercised here for clarity.

## Adapting this to your project

If you copy this `Program.cs` into your own project, the only thing that changes is the project references → switch from `<ProjectReference>` to `<PackageReference>`:

```xml
<PackageReference Include="Foundation.AI.Embed" Version="..." />
<PackageReference Include="Foundation.AI.VectorStore" Version="..." />
<PackageReference Include="Foundation.AI.VectorStore.Zvec" Version="..." />
<PackageReference Include="Foundation.AI.Inference.Onnx" Version="..." />
<PackageReference Include="Foundation.AI.MarkItDown" Version="..." />
<PackageReference Include="Foundation.AI.MarkItDown.Pdf" Version="..." />
<PackageReference Include="Foundation.AI.MarkItDown.Office" Version="..." />
```

Everything else — the API calls, the configuration, the patterns — works identically.

## License

Same as Foundation.AI: [Apache-2.0](../../LICENSE).
