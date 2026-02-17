# Zvec Documentation & Comment Quality

Two-part effort: (1) architectural docs for the `docs/` folder, and (2) in-code comment improvements for junior readers.

## Current State

**SDK layer** (`Zvec/`): Has XML doc comments on all public methods, but they're mechanical ("Insert documents into the collection") — no guidance on *when* or *why*.

**Engine layer** (`Zvec.Engine/`): Minimal comments. Key algorithms like HNSW graph construction, IVF k-means, scalar quantization, and the filter expression parser have no theory docs. A junior developer would need to read academic papers to understand the code.

## Proposed Changes

### Part 1: Architectural Documentation

Following the pattern of [Foundation architecture.md](file:///g:/source/repos/Scheduler/Foundation/docs/architecture.md) and [Alerting ARCHITECTURE.md](file:///g:/source/repos/Scheduler/Alerting/docs/ARCHITECTURE.md):

#### [NEW] [architecture.md](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/docs/architecture.md)

Contents:
- **Executive summary** and system overview
- **Layer diagram** (SDK → Engine → Storage) with mermaid
- **Project structure** table (4 projects, their roles)
- **Data flow** — insert, query, persistence lifecycle
- **Storage model** — ZoneTree, WAL, document encoding
- **Index architecture** — HNSW, IVF, Flat comparison table
- **Persistence model** — binary HNSW format, JSON IVF format, metadata
- **Quantization pipeline** — calibration → compress → ADC search flow

#### [NEW] [key-concepts.md](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/docs/key-concepts.md)

Contents:
- **Vector search theory** — embeddings, similarity, ANN vs exact
- **HNSW** — skip-list analogy, graph layers, greedy routing, parameters (M, efConstruction, ef)
- **IVF** — Voronoi partitioning, k-means training, nprobe tradeoff
- **Distance metrics** — L2, cosine, inner product — when to use each
- **Quantization** — FP16 lossless, INT8/INT4 uniform scalar, calibration, error characteristics
- **Filter expressions** — grammar, supported operators, examples
- **Collections & schemas** — fields, vectors, primary keys, document lifecycle

---

### Part 2: In-Code Comment Improvements

#### Engine Layer (high impact — these are the algorithm files)

##### [MODIFY] [HnswIndex.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Index/HnswIndex.cs)
- Class-level: HNSW algorithm overview, paper reference, complexity characteristics
- `Add()`: Layer selection probability, entry point update logic
- `SearchLayer()`: Greedy beam search explanation
- `PruneConnections()`: Why M limits matter for graph quality
- `Remove()`: Graph repair algorithm explanation
- `Recalibrate()`: Global vs per-vector calibration
- Parameters: Document what M, efConstruction, ef control

##### [MODIFY] [IvfIndex.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Index/IvfIndex.cs)
- Class-level: IVF algorithm overview, Voronoi cell analogy
- `Train()`: K-means algorithm steps, convergence behavior
- `Search()`: nprobe accuracy/speed tradeoff
- `KMeans()`: Mini-batch k-means vs standard, convergence criteria

##### [MODIFY] [Quantization.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Math/Quantization.cs)
- Class-level: Why quantization matters (memory reduction, bandwidth)
- Each section: FP16 (lossless within Half range), INT8 (uniform 256 bins), INT4 (packed nibbles, 16 bins)
- Calibration: Why global calibration matters, min/max tracking
- Error characteristics for each type

##### [MODIFY] [DistanceFunction.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Math/DistanceFunction.cs)
- Class-level: Distance function selection guidance
- Each metric: Mathematical definition, when to use, "lower is better" semantics
- SIMD: Current status and future optimization notes

##### [MODIFY] [FilterEngine.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Filter/FilterEngine.cs)
- Class-level: Grammar specification (EBNF or similar)
- Lexer: Token types and recognized patterns
- Parser: Recursive descent structure, precedence handling
- Evaluator: Type coercion rules, null handling

##### [MODIFY] [Collection.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Core/Collection.cs)
- Class-level: Collection as the orchestration layer
- `Open()`: Persistence loading strategy (metadata → storage → indexes)
- `Flush()`: What gets persisted and in what order
- `Optimize()`: When to call and what it does (IVF training, HNSW recalibration)

##### [MODIFY] [IndexPersistence.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Index/IndexPersistence.cs)
- HNSW binary format specification (magic, version, node layout)
- IVF JSON format documentation
- Snapshot model explanation

##### [MODIFY] [ZoneTreeStorageEngine.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Storage/ZoneTreeStorageEngine.cs)
- Class-level: Why ZoneTree (LSM-tree, write-optimized, sorted KV)
- Document encoding format (JSON in value bytes)
- WAL and compaction behavior

#### SDK Layer (lower impact — mostly parameter guidance)

##### [MODIFY] [IndexParams.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec/IndexParams.cs)
- Each param class: Practical guidance on parameter selection
- `HnswIndexParams`: What M/efConstruction do, recommended ranges
- `IvfIndexParams`: nlist/nprobe sizing guidance

##### [MODIFY] [QueryParams.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec/QueryParams.cs)
- `HnswQueryParams`: ef tradeoff (higher = better recall, slower)
- `IvfQueryParams`: nprobe tradeoff

## Verification Plan

### Automated Tests
```
dotnet build Zvec.sln
```
(Comment-only changes — no functional changes, so existing tests remain valid)
