# Foundation.IndexedDB — Review Fixes

All bugs, cleanup items, and style violations identified in the review, plus the user-identified tuple violation in `DexterDatabase.cs`.

## Proposed Changes

### Bug Fixes

---

#### [MODIFY] [IDBObjectStore.cs](file:///g:/source/Compactica/Foundation.IndexedDB/IDBObjectStore.cs)

**Bug 1 — `AddListAsync` serializes entire list instead of individual value (line 229):**

```diff
-string valueJson = JsonSerializer.Serialize(valueList, IDBCommon.JsonOptions);
+string valueJson = JsonSerializer.Serialize(value, IDBCommon.JsonOptions);
```

**Bug 3 — `DeleteIndexAsync` is synchronous despite `Async` name (lines 482–504):**
Convert to a proper async method returning `Task`, using `await` instead of `.Wait()`.

**Style fix — Missing braces on conditional (line 635):**
```diff
-if (conditions.Count == 0)
-    return q;
+if (conditions.Count == 0)
+{
+    return q;
+}
```

**Style fix — Replace `var` with explicit types (lines 608, 619, 628, 638).**

**Style fix — Explicit boolean check (line 130):**
```diff
-if (value == null)
+if (value == null)  // already correct, but check !_writeModeTransactionFinalized patterns
```

---

#### [MODIFY] [IDBIndex.cs](file:///g:/source/Compactica/Foundation.IndexedDB/IDBIndex.cs)

**Bug 2 — `GetAllAsync` (non-generic) passes `count.HasValue` instead of `count.Value` (lines 199–208):**
Also deduplicate the double `if (count.HasValue)` check.

```diff
 if (count.HasValue)
 {
     sql += $" LIMIT {{{parameters.Count}}}";
+    parameters.Add(count.Value);
 }
-
-// Now add count if present
-if (count.HasValue)
-{
-    parameters.Add(count.HasValue);
-}
```

**Style fix — Replace `var` with explicit types (lines 396–397).**

---

#### [MODIFY] [IDBCursor.cs](file:///g:/source/Compactica/Foundation.IndexedDB/IDBCursor.cs)

**Bug 4 — Implement `IAsyncDisposable` alongside `IDisposable`:**
Add `DisposeAsync` method that properly awaits async cleanup with the semaphore.

---

#### [MODIFY] [IDBTransaction.cs](file:///g:/source/Compactica/Foundation.IndexedDB/IDBTransaction.cs)

**Style fix — Explicit boolean checks (lines 125–127):**

```diff
-if (_mode == TransactionMode.ReadWrite && 
-    !_writeModeTransactionFinalized && 
+if (_mode == TransactionMode.ReadWrite && 
+    _writeModeTransactionFinalized == false && 
     _sqliteTransaction != null)
```

---

### Cleanup

---

#### [MODIFY] [DexterDatabase.cs](file:///g:/source/Compactica/Foundation.IndexedDB/Dexter/DexterDatabase.cs)

Remove the unused `_storesToCreate` field (line 105) which also uses a tuple in violation of the "no tuples" guideline.

---

#### [MODIFY] [IDBContext.cs](file:///g:/source/Compactica/Foundation.IndexedDB/IDBContext.cs)

Remove double semicolons on lines 25 and 36. Fix opening brace style for C# lambda on line 31.

---

## Verification Plan

### Automated Build
```powershell
dotnet build g:\source\Compactica\Foundation.IndexedDB\Foundation.IndexedDB.csproj
dotnet build g:\source\Compactica\IndexedDBTest\IndexedDBTest.csproj
```
Both must compile with 0 errors.
