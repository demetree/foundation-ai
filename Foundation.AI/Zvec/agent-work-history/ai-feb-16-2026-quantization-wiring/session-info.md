# Session Information

- **Conversation ID:** e9093069-2cf0-43ab-9021-8eea7d84a2df
- **Date:** 2026-02-16
- **Time:** 20:32 NST (UTC-03:30)

## Summary

Wired the Quantization module (FP16/INT8/INT4) into HNSW and IVF index pipelines using Asymmetric Distance Computation (ADC). Vectors are stored quantized; queries decompress on-the-fly for distance computation.

## Key Changes

- **HnswIndex**: Accepts `QuantizeType` in constructor, stores `QuantizedVector` byte[] per node, auto-calibrates from first vector, `DistanceToNode` ADC method in all search paths, `Recalibrate()` for global min/max
- **IvfIndex**: Parallel `_qLists` with quantized byte arrays, ADC search in cluster traversal, calibration during `Train()`
- **Collection**: Passes `QuantizeType` from `IndexConfig` to index constructors, `Optimize()` triggers recalibration
- **Benchmark**: Added end-to-end quantized HNSW (INT8) benchmark — 1800µs/query, full recall

## Benchmark Results (10K × 128D, Release)

- FP32 HNSW Query: 1,505 µs/query (top-10)
- INT8 HNSW Query: 1,800 µs/query (top-10, quantized ADC, full recall)
- Insert: 1,800 docs/sec
- Fetch: 50 µs/fetch
- Persistence reopen: 102 ms

## Files Modified

- `Zvec.Engine/Index/HnswIndex.cs` — quantization fields, ADC search, calibration, Recalibrate()
- `Zvec.Engine/Index/IvfIndex.cs` — quantize constructor, parallel _qLists, ADC search, calibration helpers
- `Zvec.Engine/Core/Collection.cs` — pass QuantizeType to constructors, recalibrate in Optimize
- `Zvec.Bench/Program.cs` — INT8 HNSW end-to-end benchmark
