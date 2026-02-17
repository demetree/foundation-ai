# Pure C# Vector Database — Task Checklist

## Phase 1: Foundation & Scaffolding
- [x] Create `Zvec.Engine` project with dependencies (ZoneTree, LZ4, RoaringBitmaps)
- [x] Implement SIMD distance functions (Euclidean, Cosine, InnerProduct) with AVX2/SSE fallback
- [x] Implement distance function factory with MetricType enum
- [x] Create internal `Document` model (managed, Dictionary-based)
- [x] Create `IStorageEngine` abstraction + InMemory implementation
- [x] Basic collection lifecycle (create, open, close, destroy)

## Phase 2: Flat Index (Proof of Pipeline)
- [x] Implement `IVectorIndex` interface
- [x] Implement `FlatIndex` — brute-force search using SIMD distance
- [x] End-to-end insert → flush → query pipeline working

## Phase 3: SDK Refactor  ✅ COMPLETE
- [x] Refactor `ZvecDoc` from IntPtr to managed fields
- [x] Refactor `CollectionSchema` from IntPtr to managed schema
- [x] Refactor `IndexParams` / `QueryParams` to POCOs
- [x] Refactor `ZvecCollection` to call Engine instead of NativeMethods
- [x] Refactor `QueryResult` to wrap managed results
- [x] Validate existing `Zvec.Test/Program.cs` passes against managed engine

## Phase 4: Segment & Persistence  ✅ COMPLETE
- [x] Implement ZoneTree-backed persistent storage engine
- [x] Implement collection metadata persistence (JSON)
- [x] Implement Collection.Open (load from disk + rebuild indexes)
- [x] Fix numeric type roundtrip in ZvecDoc (IConvertible)
- [x] Persistence roundtrip test passing

## Phase 5: Filter Engine  ✅ COMPLETE
- [x] Implement filter expression tokenizer
- [x] Implement recursive-descent parser (AND/OR/NOT, comparisons, parens)
- [x] Implement filter evaluator against Document
- [x] Wire into Query path (filtered vector search with 4x oversampling)
- [x] Wire into DeleteByFilter path

## Phase 6: HNSW Index  ✅ COMPLETE
- [x] Implement multi-layer navigable graph structure
- [x] Implement greedy beam search
- [x] Implement concurrent insert with level assignment
- [x] Implement connection pruning
- [x] Wire into Collection engine (replaces Flat fallback)
- [ ] Validate HNSW search quality (recall metrics)

## Phase 7: IVF Index  ✅ COMPLETE
- [x] Implement k-means++ initialization and Lloyd's algorithm
- [x] Implement inverted file index with cluster partitioning
- [x] Implement multi-probe search with configurable nprobe
- [x] Wire into Collection engine (CreateIndex + Optimize trains clusters)

## Phase 8: Quantization  ✅ COMPLETE
- [x] FP16 quantization using .NET Half type
- [x] INT8 uniform scalar quantization with min/max calibration
- [x] INT4 packed nibble quantization
- [x] Batch calibration and memory stats calculator

## Phase 9: Testing & Polish  ✅ COMPLETE
- [x] Benchmark suite (10K vectors, insert/query/fetch/quantization)
- [x] True HNSW deletion (graph repair, neighbor reconnection, entry point reassignment)
- [x] Index persistence (binary HNSW, JSON IVF — saved on Flush, loaded on Open)
- [x] Quantization roundtrip verified (FP16/INT8/INT4)
- [x] NuGet-compatible project structure

## Code Review Fixes  ← COMPLETE
- [x] #1 Drop FP32 after calibration for true memory savings
- [x] #2 Fix dead traversal code in SearchLayer deleted-node handling
- [x] #3 Restore quantized vectors in HNSW LoadSnapshot
- [x] #4 Persist quantized lists in IVF snapshot
- [x] #5 Remove redundant add+remove in DeleteByPks
- [x] #6 Remove unused PruneConnections parameter
- [x] #7 Make DocumentCount O(1) with counter field
