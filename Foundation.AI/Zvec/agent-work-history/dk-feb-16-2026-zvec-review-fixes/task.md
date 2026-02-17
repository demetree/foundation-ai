# Zvec Review Fixes

- [x] Plan fixes (1) IVF quantization persistence, (4) Collection.Open double iteration, (5) Recalibrate double enumeration
- [x] Fix 1: IVF quantization persistence in `IvfSnapshot`, `IvfIndex.GetSnapshot`, `IvfIndex.LoadSnapshot`, `IndexPersistence` serializer context
- [x] Fix 4: Collection.Open single-pass `AllDocIds()` — metadata already persists `NextDocId`, skip the second pass
- [x] Fix 5: `HnswIndex.Recalibrate()` — materialize LINQ chain to avoid double enumeration
- [x] Bonus: Fixed `.sln` — removed deleted Zvec.Interop, added missing Zvec.Bench
- [/] Verify: Build solution ✅, run `Zvec.Test`
