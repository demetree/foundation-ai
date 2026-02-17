# Zvec — Architecture

> **Version**: 1.0 | **Status**: Active Development | **Last Updated**: February 2026

---

# Executive Summary

**Zvec** is a pure C# vector database engine for approximate nearest neighbor (ANN) search, embedded directly into Foundation applications. It provides document storage with vector similarity search, scalar filtering, and durable persistence — all without native dependencies.

## Core Capabilities

| Capability | Description |
|------------|-------------|
| **Vector Search** | HNSW, IVF, and Flat (brute-force) index algorithms |
| **Quantization** | FP16, INT8, INT4 compression for reduced memory footprint |
| **Scalar Filtering** | SQL-like filter expressions evaluated during search |
| **Persistence** | Durable storage via ZoneTree (LSM-tree) with index snapshots |
| **Pure C#** | No native interop, P/Invoke, or external binaries |

---

# 1. System Architecture

## 1.1 Layer Diagram

Zvec follows a strict three-layer architecture. Each layer has a single responsibility and communicates only with its immediate neighbor.

```mermaid
graph TD
    subgraph SDK["SDK Layer (Zvec)"]
        ZC[ZvecCollection]
        ZD[ZvecDoc]
        CS[CollectionSchema]
        IP[IndexParams / QueryParams]
        EN[Enums]
    end

    subgraph Engine["Engine Layer (Zvec.Engine)"]
        subgraph Core["Core"]
            COL[Collection]
            DOC[Document]
            SCH[Schema]
        end
        subgraph Index["Index"]
            HNSW[HnswIndex]
            IVF[IvfIndex]
            FLAT[FlatIndex]
            PERS[IndexPersistence]
        end
        subgraph Math["Math"]
            DIST[DistanceFunction]
            QUANT[Quantization]
        end
        subgraph Filter["Filter"]
            FILT[FilterEngine]
        end
        subgraph Storage["Storage"]
            SE[StorageEngine]
            ZT[ZoneTreeStorageEngine]
        end
    end

    subgraph Bench["Benchmarks (Zvec.Bench)"]
        BM[Program.cs]
    end

    subgraph Test["Tests (Zvec.Test)"]
        TS[Program.cs]
    end

    SDK --> Engine
    BM --> SDK
    TS --> SDK
    COL --> Index
    COL --> Storage
    COL --> Filter
    Index --> Math
```

## 1.2 Project Structure

| Project | Role | Dependencies |
|---------|------|-------------|
| **Zvec** | Public SDK — user-facing API surface | Zvec.Engine |
| **Zvec.Engine** | Core engine — algorithms, storage, persistence | ZoneTree (NuGet) |
| **Zvec.Test** | Integration test suite | Zvec |
| **Zvec.Bench** | Performance benchmarks | Zvec |

## 1.3 SDK → Engine Boundary

The SDK layer provides a simplified, opinionated API. Internally it delegates to the engine:

| SDK Class | Engine Class | Purpose |
|-----------|-------------|---------|
| `ZvecCollection` | `Collection` | Collection lifecycle and operations |
| `ZvecDoc` | `Document` | Document read/write |
| `CollectionSchema` | `SchemaBuilder` → `CollectionSchemaDefinition` | Schema definition |
| `HnswIndexParams` | `IndexConfig` | Index configuration |
| `QueryResult` | `IReadOnlyList<Document>` | Query results |

---

# 2. Data Flow

## 2.1 Insert Flow

```mermaid
sequenceDiagram
    participant App as Application
    participant SDK as ZvecCollection
    participant Col as Collection
    participant Store as ZoneTreeStorageEngine
    participant Idx as Vector Index

    App->>SDK: Insert(docs)
    SDK->>Col: Insert(engineDocs)
    Col->>Col: Assign docId (atomic increment)
    Col->>Store: PutDocument(docId, doc)
    Store->>Store: Serialize to JSON → ZoneTree
    Col->>Idx: Add(docId, vector)
    Idx->>Idx: Build graph connections (HNSW) or assign cluster (IVF)
```

## 2.2 Query Flow

```mermaid
sequenceDiagram
    participant App as Application
    participant SDK as ZvecCollection
    participant Col as Collection
    participant Idx as Vector Index
    participant Filt as FilterEngine
    participant Store as ZoneTreeStorageEngine

    App->>SDK: Query(field, vector, topk, filter)
    SDK->>Col: Query(...)
    Col->>Idx: Search(vector, topk, deletedFilter)
    Idx->>Idx: ANN search (greedy beam / cluster scan)
    Idx-->>Col: SearchHit[] (docId, distance)
    Col->>Store: GetDocument(docId) for each hit
    Col->>Filt: Evaluate filter on each document
    Filt-->>Col: pass/fail
    Col-->>SDK: Filtered Document[]
    SDK-->>App: QueryResult (ZvecDoc[])
```

## 2.3 Persistence Flow

```mermaid
sequenceDiagram
    participant App as Application
    participant Col as Collection
    participant Store as ZoneTreeStorageEngine
    participant Pers as IndexPersistence
    participant Disk as File System

    App->>Col: Flush()
    Col->>Store: Flush() (ZoneTree WAL + compaction)
    Col->>Col: Save metadata (NextDocId, schema)
    Col->>Pers: SaveHnsw(index, path) / SaveIvf(index, path)
    Pers->>Disk: Binary file (HNSW) / JSON file (IVF)

    Note over App,Disk: On reopen...

    App->>Col: Open(path)
    Col->>Store: Open ZoneTree
    Col->>Col: Load metadata
    Col->>Pers: LoadHnsw(path) / LoadIvf(path)
    Pers->>Disk: Read snapshot files
    Pers-->>Col: Restored indexes (with quantization state)
```

---

# 3. Index Architecture

## 3.1 Index Comparison

| Feature | HNSW | IVF | Flat |
|---------|------|-----|------|
| **Algorithm** | Navigable small-world graph | Inverted file with k-means clustering | Brute-force linear scan |
| **Build time** | O(n log n) | O(n × k × iterations) | O(1) |
| **Query time** | O(log n) | O(nprobe × n/nlist) | O(n) |
| **Memory** | High (graph + vectors) | Medium (centroids + lists) | Low (vectors only) |
| **Accuracy** | Very high (tunable via ef) | Good (tunable via nprobe) | Exact (100%) |
| **Best for** | General purpose, < 10M vectors | Large datasets, batch workloads | Small datasets, ground truth |
| **Quantization** | ✅ ADC with recalibration | ✅ ADC with per-cluster lists | ❌ |
| **Deletion** | ✅ True delete with graph repair | ✅ List removal | ✅ List removal |
| **Persistence** | Binary snapshot | JSON snapshot | Rebuild from storage |

## 3.2 HNSW Graph Structure

```mermaid
graph TD
    subgraph Layer2["Layer 2 (sparse)"]
        L2A((A))
        L2B((B))
        L2A --- L2B
    end

    subgraph Layer1["Layer 1 (medium)"]
        L1A((A))
        L1B((B))
        L1C((C))
        L1D((D))
        L1A --- L1B
        L1A --- L1C
        L1B --- L1D
        L1C --- L1D
    end

    subgraph Layer0["Layer 0 (dense — all nodes)"]
        L0A((A))
        L0B((B))
        L0C((C))
        L0D((D))
        L0E((E))
        L0F((F))
        L0A --- L0B
        L0A --- L0C
        L0B --- L0D
        L0C --- L0D
        L0D --- L0E
        L0E --- L0F
        L0C --- L0F
    end

    L2A -.-> L1A
    L2B -.-> L1B
    L1A -.-> L0A
    L1B -.-> L0B
    L1C -.-> L0C
    L1D -.-> L0D
```

Search starts at the top layer and greedily descends, narrowing the search space at each level.

## 3.3 IVF Partition Structure

```mermaid
graph LR
    subgraph Centroids["K-Means Centroids"]
        C1((C₁))
        C2((C₂))
        C3((C₃))
    end

    subgraph Lists["Inverted Lists"]
        L1["List₁: doc₁, doc₅, doc₈"]
        L2["List₂: doc₂, doc₃, doc₇"]
        L3["List₃: doc₄, doc₆, doc₉"]
    end

    C1 --> L1
    C2 --> L2
    C3 --> L3

    Q[/"Query vector"/] --> C1
    Q --> C2
    style Q fill:#ff9,stroke:#333
```

At query time, only the nearest `nprobe` clusters are scanned instead of the full dataset.

---

# 4. Storage Model

## 4.1 ZoneTree (LSM-Tree)

Zvec uses [ZoneTree](https://github.com/koculu/ZoneTree) as its durable key-value store. ZoneTree is a .NET LSM-tree implementation providing:

- **Write-optimized**: Append-only writes with write-ahead log (WAL)
- **Sorted keys**: Long docId keys enable efficient range scans
- **Compaction**: Background merge of sorted runs
- **ACID**: Crash-safe with WAL recovery

## 4.2 Document Encoding

Documents are stored as JSON-serialized byte arrays keyed by `long docId`:

```
Key:   long docId (8 bytes)
Value: UTF-8 JSON bytes
       {
         "PrimaryKey": "item_42",
         "Fields": { "category": "electronics", "price": 29.99 },
         "Vectors": { "embedding": [0.1, 0.2, ...] }
       }
```

## 4.3 On-Disk Layout

```
<collection-path>/
├── metadata.json           # Schema, NextDocId, index configs
├── data/                   # ZoneTree files
│   ├── *.wal               # Write-ahead log
│   └── *.zonedata          # Sorted run segments
├── index_<field>.hnsw      # Binary HNSW snapshot
└── index_<field>.ivf       # JSON IVF snapshot
```

---

# 5. Persistence Formats

## 5.1 HNSW Binary Format

```
┌──────────────────────────────┐
│ Magic: "HNSW" (4 bytes)     │
│ Version: 1 (int32)          │
│ MaxLevel (int32)            │
│ EntryPointIdx (int32)       │
│ NodeCount (int32)           │
├──────────────────────────────┤
│ For each node:              │
│   DocId (int64)             │
│   IsDeleted (byte)          │
│   VectorLength (int32)      │
│   Vector (float32[])        │
│   Level (int32)             │
│   For each layer 0..Level:  │
│     NeighborCount (int32)   │
│     Neighbors (int32[])     │
│   QVecLength (int32)        │
│   QVec (byte[]) if present  │
└──────────────────────────────┘
```

## 5.2 IVF JSON Format

```json
{
  "Trained": true,
  "Dimension": 128,
  "Centroids": [[0.1, 0.2, ...], ...],
  "Lists": [
    [{ "DocId": 1, "Vector": [...], "QVec": "base64..." }, ...]
  ],
  "QCalibrated": true,
  "Int8CalMin": -1.5, "Int8CalMax": 2.3,
  "Int4CalMin": -1.5, "Int4CalMax": 2.3
}
```

---

# 6. Quantization Pipeline

```mermaid
flowchart LR
    subgraph Calibration["1. Calibration"]
        Scan["Scan all vectors"]
        MinMax["Compute global min/max"]
        Scan --> MinMax
    end

    subgraph Compress["2. Compression"]
        FP32["FP32 vector"]
        INT8["INT8 bytes"]
        FP32 --> INT8
    end

    subgraph Search["3. ADC Search"]
        Query["Query vector (FP32)"]
        Decomp["Decompress INT8 → FP32"]
        Dist["Compute distance"]
        Query --> Dist
        Decomp --> Dist
    end

    Calibration --> Compress --> Search
```

| Type | Bytes/Component | Bins | Error | Use Case |
|------|----------------|------|-------|----------|
| FP16 | 2 | 65,536 | Very low | Memory savings with minimal quality loss |
| INT8 | 1 | 256 | Low | Good balance of compression and accuracy |
| INT4 | 0.5 | 16 | Moderate | Maximum compression, lower accuracy |

---

# 7. Relationship to Other Projects

```mermaid
graph TD
    Zvec["Foundation.AI / Zvec<br/>(this project)"]
    Foundation["Foundation<br/>(Platform Admin)"]
    Scheduler["Scheduler<br/>(Application)"]
    Alerting["Alerting<br/>(Incident Management)"]

    Foundation -.-> |"future: semantic search"| Zvec
    Scheduler -.-> |"future: embedding search"| Zvec
    Alerting -.-> |"future: similar incidents"| Zvec
```

Zvec is an **independent, embeddable library** — it has no dependencies on the Foundation platform and can be used standalone. Future Foundation applications may embed Zvec for semantic search, document similarity, or recommendation features.

---

*Documentation generated by AI assistant — February 2026*
