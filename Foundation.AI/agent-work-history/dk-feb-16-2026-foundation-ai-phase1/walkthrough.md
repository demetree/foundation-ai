# Foundation.AI — Phase 1 Walkthrough

## What Was Built

5 new projects under `Foundation.AI/`, establishing the core AI platform:

### Projects Created

| Project | Purpose | Files |
|---------|---------|-------|
| **[Foundation.AI.VectorStore](file:///g:/source/repos/Scheduler/Foundation.AI/Foundation.AI.VectorStore)** | Provider-agnostic vector storage interface | `IVectorStore`, `Models`, `VectorStoreOptions`, DI extensions |
| **[Foundation.AI.VectorStore.Zvec](file:///g:/source/repos/Scheduler/Foundation.AI/Foundation.AI.VectorStore.Zvec)** | Zvec engine provider | `ZvecVectorStore` (collection lifecycle, type mapping) |
| **[Foundation.AI.Embed](file:///g:/source/repos/Scheduler/Foundation.AI/Foundation.AI.Embed)** | Embedding generation service | `IEmbeddingProvider`, ONNX + OpenAI + Fallback providers, DI extensions |
| **[Foundation.AI.Test](file:///g:/source/repos/Scheduler/Foundation.AI/Foundation.AI.Test)** | Integration tests | 5 tests covering full vector pipeline |

### Key Interfaces

```csharp
// Store vectors
IVectorStore store = new ZvecVectorStore(config);
await store.CreateCollectionAsync("projects", 384);
await store.UpsertAsync("projects", "proj-42", embedding);
var results = await store.SearchAsync("projects", queryVector, topK: 10);

// Generate embeddings
IEmbeddingProvider embedder = new OnnxEmbeddingProvider(onnxConfig);
float[] embedding = await embedder.EmbedAsync("some text");

// Register via DI
services.AddVectorStore(sp => new ZvecVectorStore(config));
services.AddOnnxEmbedding(c => { c.ModelPath = "./model.onnx"; c.UseCuda = true; });
```

### Embedding Providers

| Provider | Backend | Use Case |
|----------|---------|----------|
| [OnnxEmbeddingProvider](file:///g:/source/repos/Scheduler/Foundation.AI/Foundation.AI.Embed/OnnxEmbeddingProvider.cs) | ONNX Runtime (CPU + CUDA) | Local inference, GPU-accelerated |
| [OpenAiEmbeddingProvider](file:///g:/source/repos/Scheduler/Foundation.AI/Foundation.AI.Embed/OpenAiEmbeddingProvider.cs) | OpenAI / Azure OpenAI / **Ollama** | Cloud fallback or local Ollama |
| [FallbackEmbeddingProvider](file:///g:/source/repos/Scheduler/Foundation.AI/Foundation.AI.Embed/FallbackEmbeddingProvider.cs) | Primary → Fallback | Auto-failover with dimension validation |

## Verification

**Build:** 0 errors across full `Scheduler.sln`

**Tests:** 5/5 passed (1.4s)
```
✅ CreateCollection_ShouldCreateDirectory
✅ UpsertAndSearch_ShouldReturnSimilarVectors
✅ UpsertBatch_ShouldInsertMultipleDocuments
✅ Delete_ShouldRemoveDocument
✅ DeleteCollection_ShouldRemoveAllData
```

## Next Steps (Phase 2)
- `Foundation.AI.Inference` — LLM via LLamaSharp + OpenAI/Ollama fallback
- `Foundation.AI.Rag` — RAG orchestration (chunking → embedding → retrieval → generation)
- `SqliteVecVectorStore` — Vector search co-located with EF Core databases
