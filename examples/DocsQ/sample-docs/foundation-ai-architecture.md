# Foundation.AI Architecture Overview

Foundation.AI is a .NET 10 stack for embedding language-model capabilities directly into applications -- desktop, server, or worker -- with no Python runtime, no mandatory cloud dependency, and no external vector database. Every major capability is exposed behind a small interface, with concrete providers selected at composition time via dependency injection.

## Module map

The codebase is organised into five composable layers:

- **Inference** -- `IInferenceProvider` with implementations for OpenAI-compatible endpoints (used for both real OpenAI and any local server such as Ollama or LM Studio), ONNX GenAI for in-process autoregressive generation, LLamaSharp for GGUF-format quantised models, and Microsoft Research's BitNet for 1-bit quantised inference.
- **Embeddings** -- `IEmbeddingProvider` with ONNX (local) and OpenAI-compatible (network) implementations. The ONNX path is what most apps reach for first because it requires no service to be running.
- **Vector storage** -- `IVectorStore` with the Zvec implementation as the default. Zvec is an embedded, file-based vector database -- no Postgres, no Pinecone, no separate process. It supports HNSW, IVF, and flat indexes with optional FP16, INT8, or INT4 quantisation.
- **RAG orchestration** -- `IRagService` ties the previous three together. Indexing chunks documents, embeds them, and stores them; queries embed the question, retrieve top-K chunks, and synthesise an answer through the inference provider.
- **Document conversion** -- MarkItDown converts arbitrary input formats (PDF, Office, HTML, ZIP, EPUB, plain text) to clean Markdown suitable for chunking. It is conceptually inspired by Microsoft's Python library of the same name but is an independent .NET implementation.

## Design principles

The codebase favours pragmatic, opinionated defaults over framework ceremony. Each provider is a small singleton with constructor-injected configuration. Cold-start latency is acknowledged as a real cost and the conventions encourage warming the embedder and inference model up front -- the project ships hosted-service wrappers (`OnnxModelDownloadWorker`, `EmbeddingModelDownloadWorker`) that take care of "fetch the model from HuggingFace, then preload it into RAM" for ASP.NET applications.

The interfaces are deliberately small. `IEmbeddingProvider` has just `EmbedAsync` and `EmbedBatchAsync`. `IInferenceProvider` adds streaming variants to `GenerateAsync` and `ChatAsync`. `IVectorStore` is mostly CRUD plus a single `SearchAsync`. `IRagService` exposes `IndexDocumentAsync`, `IndexBatchAsync`, `QueryAsync`, and `QueryStreamAsync`. The aim is for a competent .NET developer to get from "I want to add semantic search to my app" to "it works" in an afternoon.

## What it explicitly is not

Foundation.AI is not a model-training framework. There are no training loops, no fine-tuning utilities, no gradient computation. It is also not an agent orchestration framework -- there is no built-in tool-use scheduler or planner; multi-step reasoning is left to the application layer, where every project's needs are different.
