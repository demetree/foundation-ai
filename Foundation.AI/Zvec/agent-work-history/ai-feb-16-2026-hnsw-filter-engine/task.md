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

## Phase 4: Segment & Persistence
- [ ] Implement segment model (writing segment + persisted segments)
- [ ] Implement VersionManager for MVCC
- [ ] Implement Flush (writing segment → ZoneTree)
- [ ] Implement Optimize (compact segments, rebuild indexes)
- [ ] DeleteStore with RoaringBitmap

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

## Phase 7: IVF Index
- [ ] Implement k-means clustering
- [ ] Implement inverted file index
- [ ] Implement multi-probe search

## Phase 8: Quantization
- [ ] FP16 quantization using Half type
- [ ] INT8 quantization
- [ ] INT4 quantization

## Phase 9: Testing & Polish
- [ ] Benchmark suite (insert, query, memory)
- [ ] Edge cases and error handling
- [ ] NuGet packaging
