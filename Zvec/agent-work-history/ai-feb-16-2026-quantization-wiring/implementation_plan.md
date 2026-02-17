# Wiring Quantization into Indexes

Wire the existing `Quantization` module into the HNSW and IVF index pipelines so that vectors are stored in compressed form, reducing memory usage by 50–88%.

## Proposed Changes

### Strategy: Asymmetric Distance Computation (ADC)

The standard approach for quantized search is **asymmetric distance computation**: store vectors quantized, but decompress to FP32 on-the-fly during distance calculations. This preserves search quality while cutting memory.

- **Storage**: Vectors stored as quantized bytes (FP16/INT8/INT4)
- **Search**: Query vector stays FP32; stored vectors are decompressed to FP32 for distance calc
- **Full vector**: The original FP32 vector is discarded from the index after quantization (it remains in ZoneTree storage for exact retrieval)

---

### HNSW Index

#### [MODIFY] [HnswIndex.cs](file:///g:/source/repos/zvec/dotnet/Zvec.Engine/Index/HnswIndex.cs)

- Add `QuantizeType` parameter to constructor
- In `HnswNode`: store quantized data (byte[]) alongside or instead of float[]
- Add a `Decompress` helper to reconstruct FP32 from quantized on-the-fly during distance calls
- Calibration: compute global INT8/INT4 calibration from first N vectors, or recalibrate on Optimize

Key design: keep `HnswNode.Vector` as `float[]` for FP32/unquantized mode. For quantized mode, add `QuantizedVector` byte[] and calibration data, and make `Vector` a lazy decompress property.

#### [MODIFY] [Collection.cs](file:///g:/source/repos/zvec/dotnet/Zvec.Engine/Core/Collection.cs)

- Pass `QuantizeType` from `IndexConfig` to index constructors
- `CreateHnswIndex`: forward quantize setting
- `CreateIvfIndex`: forward quantize setting

---

### IVF Index

#### [MODIFY] [IvfIndex.cs](file:///g:/source/repos/zvec/dotnet/Zvec.Engine/Index/IvfIndex.cs)

- Add `QuantizeType` + calibration to constructor
- Store inverted list vectors as quantized bytes
- Decompress during search distance computation
- Centroids always stored as FP32 (partition accuracy matters)

---

### Persistence Updates

#### [MODIFY] [IndexPersistence.cs](file:///g:/source/repos/zvec/dotnet/Zvec.Engine/Index/IndexPersistence.cs)

- Serialize quantize type and calibration data in HNSW binary header
- Serialize quantized vectors in nodes (use byte arrays instead of float arrays when quantized)

---

## Verification Plan

### Automated Tests

Run existing tests to verify no regressions:
```
dotnet run --project Zvec.Test\Zvec.Test.csproj
```

Run benchmark with quantization enabled:
```
dotnet run --project Zvec.Bench\Zvec.Bench.csproj -c Release
```

### New Verification: Quantized Index Test

Add a test in the benchmark that creates an HNSW index with `QuantizeType.Int8`, inserts vectors, queries, and verifies results are reasonable (recall > 50% vs brute-force at 10K scale).
