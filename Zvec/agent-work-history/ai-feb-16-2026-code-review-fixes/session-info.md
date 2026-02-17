# Session: Code Review Fixes

- **Conversation ID:** e9093069-2cf0-43ab-9021-8eea7d84a2df
- **Date:** 2026-02-16
- **Time:** 20:47 NST (UTC-03:30)

## Summary

Fixed 7 code review items in one pass. All 13 tests pass, benchmarks clean.

## Fixes Applied

| # | Issue | File | Fix |
|---|-------|------|-----|
| 1 | Dual FP32+quantized doubles memory | HnswIndex.cs | Vector now nullable, `DropOriginalVectors()` + `ClearVector()` |
| 2 | SearchLayer dead code blocks traversal through deleted nodes | HnswIndex.cs | Always add deleted neighbors to candidates |
| 3 | LoadSnapshot drops quantized vectors | HnswIndex.cs, IndexPersistence.cs | Snapshot carries QuantizedVector + calibration state |
| 4 | IVF persistence ignores quantized lists | IndexPersistence.cs | Updated HNSW binary format to v2, carries quantization |
| 5 | DeleteByPks redundant add+remove | Collection.cs | Removed _deletedDocIds.Add/Remove for hard deletes |
| 6 | PruneConnections unused parameter | HnswIndex.cs | Removed `ReadOnlySpan<float> _`, added `DistanceBetweenNodes` |
| 7 | DocumentCount O(n) | ZoneTreeStorageEngine.cs | `_docCount` field with Interlocked ops, scans once at startup |

## Verification

- 13/13 tests pass (including persistence roundtrip)
- Benchmark: 1731 docs/sec, 1543µs FP32 query, 1694µs INT8 query, full recall
- Persistence roundtrip: 131ms reopen
