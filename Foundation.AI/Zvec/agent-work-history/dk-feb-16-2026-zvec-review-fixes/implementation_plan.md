# Zvec Review Fixes

Addressing three issues found during round 3 code review. Issue #3 (O(n) DocumentCount) was already fixed — `ZoneTreeStorageEngine` now maintains an `_docCount` field.

## Proposed Changes

### Fix 1: IVF Quantization Persistence

The `IvfSnapshot` currently only stores FP32 vectors. After reopen, quantized lists (`_qLists`), calibration, and `_qCalibrated` are lost — forcing a full `Train()` to restore quantized search.

#### [MODIFY] [IvfIndex.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Index/IvfIndex.cs)

- **`GetSnapshot()`** (line 296): Also serialize `_qLists`, `_int8Cal`, `_int4Cal`, `_qCalibrated`, and `_dimension`
- **`LoadSnapshot()`** (line 323): Restore quantized lists, calibration state, and dimension

#### [MODIFY] [IndexPersistence.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Index/IndexPersistence.cs)

- **`IvfSnapshot`** (line 279): Add `QLists`, `Int8CalMin`, `Int8CalMax`, `Int4CalMin`, `Int4CalMax`, `QCalibrated`, `Dimension` properties
- **`IvfEntry`** (line 286): Add optional `QVec` property for quantized bytes
- Add `IvfQEntry` DTO for serializing `(docId, byte[])` tuples, or add `byte[]? QVec` to `IvfEntry`

---

### Fix 4: Collection.Open Double AllDocIds Iteration

When indexes load from disk, `Collection.Open` iterates `AllDocIds()` twice — once for index loading, once for max docId. But `NextDocId` is already persisted in metadata and restored on line 131. The second pass (lines 170-176) is redundant.

#### [MODIFY] [Collection.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Core/Collection.cs)

- Remove the `else` block (lines 168-176) that iterates `AllDocIds()` just to track max docId — it's already set from `metadata.NextDocId` on line 131

---

### Fix 5: HnswIndex.Recalibrate Double Enumeration

`Recalibrate()` uses a lazy LINQ chain (`withVectors`) that's evaluated twice: once by `.Any()` and again by `.Select()`. Materializing it once prevents the double scan.

#### [MODIFY] [HnswIndex.cs](file:///g:/source/repos/Scheduler/Foundation.AI/Zvec/Zvec.Engine/Index/HnswIndex.cs)

- Line 565-568: Replace the lazy LINQ + `.Any()` + `.Select()` pattern with a single materialized list:
```csharp
var withVectors = _nodes.Where(n => !n.IsDeleted && n.Vector != null).ToList();
if (withVectors.Count == 0) return;
var allVectors = withVectors.Select(n => n.Vector!);
```

## Verification Plan

### Automated Tests

Build and run the existing test suite:
```
cd g:\source\repos\Scheduler\Foundation.AI\Zvec
dotnet build Zvec.sln
dotnet run --project Zvec.Test
```

The test includes a persistence roundtrip (insert → flush → close → reopen → query → fetch) which validates that indexes survive restart.
