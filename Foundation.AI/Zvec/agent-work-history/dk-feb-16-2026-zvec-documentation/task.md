# Zvec Documentation & Comment Quality

## Architectural Documentation
- [x] Create `docs/architecture.md` — system overview, layer diagram, data flow, persistence model, project structure
- [x] Create `docs/key-concepts.md` — vector search theory (HNSW, IVF, quantization), distance metrics, schemas, filters

## Comment Quality Improvements (Engine Layer)
- [x] `HnswIndex.cs` — algorithm theory (NSW graph, layer selection, greedy search), parameter explanations (M, efConstruction, ef)
- [x] `IvfIndex.cs` — IVF theory (Voronoi partitioning, nprobe tradeoffs), k-means algorithm, quantized search path
- [x] `Quantization.cs` — quantization theory (uniform scalar, packed nibble), calibration explanation, error characteristics
- [x] `DistanceFunction.cs` — distance metric theory (L2, cosine, IP), when to use each, SIMD optimization notes
- [x] `FilterEngine.cs` — filter expression grammar, recursive descent parser theory, AST evaluation
- [x] `Collection.cs` — collection lifecycle, persistence model, index management
- [x] `IndexPersistence.cs` — binary format specification (HNSW), JSON format (IVF), snapshot model
- [x] `ZoneTreeStorageEngine.cs` — storage abstraction, ZoneTree integration, WAL explanation

## Comment Quality Improvements (SDK Layer)
- [x] `IndexParams.cs` — parameter guidance for each index type
- [x] `QueryParams.cs` — query parameter effects on recall vs speed
- [x] `Enums.cs` — data type descriptions
- [ ] `CollectionSchema.cs` — schema design guidance (already has adequate docs)

## Verification
- [ ] Build passes
