# Zvec Review Fixes — Walkthrough

## Changes Made

### Fix 1: IVF Quantization Persistence
Previously, `IvfSnapshot` only saved FP32 vectors — quantized lists, calibration, and `_qCalibrated` were lost on reopen.

- [IvfIndex.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Index/IvfIndex.cs) — `GetSnapshot()` now serializes `_qLists` (per-entry `QVec` bytes) and calibration state; `LoadSnapshot()` restores them
- [IndexPersistence.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Index/IndexPersistence.cs) — `IvfSnapshot` extended with `Int8CalMin/Max`, `Int4CalMin/Max`, `QCalibrated`, `Dimension`; `IvfEntry` extended with `byte[]? QVec`

render_diffs(file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Index/IvfIndex.cs)

---

### Fix 4: Remove Redundant `AllDocIds()` Scan
`Collection.Open` iterated `AllDocIds()` twice when indexes loaded from disk. The second pass only tracked max docId, but `NextDocId` is already persisted in metadata.

- [Collection.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Core/Collection.cs) — Removed the `else` block (7 lines)

render_diffs(file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Core/Collection.cs)

---

### Fix 5: `Recalibrate()` Double Enumeration
Lazy LINQ chain was enumerated twice (`.Any()` then `.Select()`). Materialized with `.ToList()`.

- [HnswIndex.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Index/HnswIndex.cs) — 2-line change

render_diffs(file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Index/HnswIndex.cs)

---

### Bonus: Solution File Cleanup
- [Zvec.sln](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.sln) — Removed deleted `Zvec.Interop`, added missing `Zvec.Bench`

## Verification

✅ **Build**: 0 warnings, 0 errors across all 4 projects
✅ **Zvec.Test**: All 13 tests passed including persistence roundtrip
