# Fix Code Generator Revive Method

## Problem

The `ReviveLegoSet` method uses wrong property names because `AngularServiceGenerator.cs` generates the Revive method by iterating `tablesThatLinkHere` (unique table names), while all other generated code iterates `foreignKeysThatLinkHere` (per-FK).

When a table like `LegoSetSubset` has **two FKs** back to `LegoSet` (`parentLegoSetId` and `childLegoSetId`), the table name `LegoSetSubset` appears once in `tablesThatLinkHere`, producing a single `_legoSetSubsets` cache. But the class fields correctly produce `_legoSetSubsetParentLegoSets` and `_legoSetSubsetChildLegoSets`.

## Root Cause

| Area | Iterates | Naming | Correct? |
|------|----------|--------|----------|
| Class fields (line 1841) | `foreignKeysThatLinkHere` | FK-aware | ✅ |
| Observables (line 1912) | `foreignKeysThatLinkHere` | FK-aware | ✅ |
| `clearAllLazyCaches()` (line 2046) | `foreignKeysThatLinkHere` | FK-aware | ✅ |
| `Get{X}For{Entity}()` (line 558) | `foreignKeysThatLinkHere` | FK-aware | ✅ |
| **Revive caches (line 660)** | **`tablesThatLinkHere`** | **Table-only** | ❌ |
| **Revive observables (line 684)** | **`tablesThatLinkHere`** | **Table-only** | ❌ |

## Proposed Changes

#### [MODIFY] [AngularServiceGenerator.cs](file:///g:/source/repos/Scheduler/CodeGenerationCore/AngularServiceGenerator.cs)

Replace the two `foreach` loops in the Revive method section (lines 660-704) to iterate `foreignKeysThatLinkHere` instead of `tablesThatLinkHere`, using the same FK-aware naming pattern as class fields (lines 1841-1886) and observables (lines 1912-1996).

## Verification Plan

### Automated Tests
- Build `CodeGenerationCore` project to confirm compilation
