# Foundation.IndexedDB — Bug Fixes

Five concrete bugs identified during code review. All are small, surgical fixes with no API-breaking changes.

## Proposed Changes

### IDBFactory

#### [MODIFY] [IDBFactory.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/IDBFactory.cs)

**Bug 1 — `basePath` parameter silently ignored:**
The constructor accepts `basePath` but always overwrites it with the entry assembly location.

```diff
 public IDBFactory(string basePath = null)
 {
-    _basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";
+    _basePath = basePath ?? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";
 }
```

**Bug 2 — `DeleteDatabase` doesn't clean up WAL/SHM sidecar files:**
SQLite in WAL mode creates `*.sqlite-wal` and `*.sqlite-shm` files. Deleting only the main `.sqlite` file leaves orphans on disk.

```diff
 if (System.IO.File.Exists(dbPath))
 {
     try
     {
         System.IO.File.Delete(dbPath);
+
+        // Clean up WAL mode sidecar files
+        string walPath = dbPath + "-wal";
+        string shmPath = dbPath + "-shm";
+        if (System.IO.File.Exists(walPath)) System.IO.File.Delete(walPath);
+        if (System.IO.File.Exists(shmPath)) System.IO.File.Delete(shmPath);
     }
```

---

### Dexter Layer

#### [MODIFY] [DexterCollection.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/Dexter/DexterCollection.cs)

**Bug 3 — Empty `catch` block that just rethrows:**
The `First()` method has `catch (Exception ex) { throw; }` which adds no value and clutters the code.

```diff
 public async Task<T> First()
 {
-    try
-    {
         if (!(_query is IDBKeyRange))
         {
             return await _index.GetAsync<T>(_query).ConfigureAwait(false);
         }
         else
         {
             var results = await _index.GetAllAsync<T>(_query, 1).ConfigureAwait(false);
             return results.FirstOrDefault();
         }
-    }
-    catch (Exception ex)
-    {
-        throw;
-    }
 }
```

#### [MODIFY] [DexterDatabase.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/Dexter/DexterDatabase.cs)

**Bug 4 — Double-stripping `&` prefix on unique indexes:**
In `DefineStores`, the `&` prefix is already stripped at line 165 when adding to `indexPaths`. But the index creation loop at line 205 checks for `&` again and strips again — this second check will **never be true**, making the `isUnique` flag always `false`.

```diff
 foreach (var indexPath in indexPaths)
 {
-    bool isUnique = indexPath.StartsWith("&");
-    string actualPath = isUnique ? indexPath.Substring(1) : indexPath;
-    await currentStore.CreateIndexAsync(actualPath, actualPath, new IDBObjectStore.IndexOptions { Unique = isUnique }).ConfigureAwait(false);
+    await currentStore.CreateIndexAsync(indexPath, indexPath, new IDBObjectStore.IndexOptions { Unique = false }).ConfigureAwait(false);
 }
```

And propagate the uniqueness info from parsing:

```diff
-else if (part.StartsWith("&"))
+else if (part.StartsWith("&"))
 {
-    indexPaths.Add(part.Substring(1)); // Unique index
+    indexPaths.Add(part.Substring(1)); // Unique index - set to unique below
 }
```

We need to preserve the unique flag. The cleanest fix is to use a list of tuples:

```diff
-List<string> indexPaths = new List<string>();
+List<(string path, bool unique)> indexPaths = new List<(string, bool)>();
```

Then store the parsed info correctly:
- `&` fields: `indexPaths.Add((part.Substring(1), true))`
- normal fields: `indexPaths.Add((part, false))`

And use them in the creation loop:
```diff
-foreach (var indexPath in indexPaths)
+foreach (var (indexPath, isUnique) in indexPaths)
 {
     await currentStore.CreateIndexAsync(indexPath, indexPath,
-        new IDBObjectStore.IndexOptions { Unique = indexPath.StartsWith("&") }).ConfigureAwait(false);
+        new IDBObjectStore.IndexOptions { Unique = isUnique }).ConfigureAwait(false);
 }
```

---

### Utility

#### [MODIFY] [SqliteWALInterceptor.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/Utility/SqliteWALInterceptor.cs)

**Bug 5 — Typo:** `diagnsostics` → `diagnostics`

```diff
-// Log the error to the diagnsostics; do not rethrow...
+// Log the error to the diagnostics; do not rethrow...
```

## Verification Plan

### Automated Tests
- Run: `dotnet build g:\source\repos\Scheduler\Foundation.IndexedDB\Foundation.IndexedDB.csproj` — must succeed with no errors.

### Manual Verification
- Since there are no existing test projects and these are small, isolated fixes, a successful build confirms no regressions.
- The unique index bug (#4) can be verified by inspecting the `DefineStores` logic to confirm uniqueness info now flows correctly from parse → create.
