# Foundation.IndexedDB Bug Fixes — Walkthrough

## Changes Made

Five bugs were fixed across 4 files:

| # | Bug | File | Severity |
|---|-----|------|----------|
| 1 | `basePath` parameter silently ignored | [IDBFactory.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/IDBFactory.cs#L17-L19) | Medium |
| 2 | WAL/SHM sidecar files not cleaned up on delete | [IDBFactory.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/IDBFactory.cs#L126-L138) | Medium |
| 3 | Empty catch/rethrow in `First()` | [DexterCollection.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/Dexter/DexterCollection.cs#L29-L40) | Low |
| 4 | **Unique indexes never actually unique** | [DexterDatabase.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/Dexter/DexterDatabase.cs#L138-L206) | **High** |
| 5 | Typo in comment | [SqliteWALInterceptor.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/Utility/SqliteWALInterceptor.cs#L107) | Low |

## Diffs

render_diffs(file:///g:/source/repos/Scheduler/Foundation.IndexedDB/IDBFactory.cs)
render_diffs(file:///g:/source/repos/Scheduler/Foundation.IndexedDB/Dexter/DexterDatabase.cs)
render_diffs(file:///g:/source/repos/Scheduler/Foundation.IndexedDB/Dexter/DexterCollection.cs)
render_diffs(file:///g:/source/repos/Scheduler/Foundation.IndexedDB/Utility/SqliteWALInterceptor.cs)

## Verification

Build passed with **0 errors**:

```
dotnet build Foundation.IndexedDB.csproj
    0 Error(s)
    113 Warning(s)  ← all pre-existing
```
