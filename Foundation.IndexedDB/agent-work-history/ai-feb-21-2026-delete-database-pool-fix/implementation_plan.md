# Fix: SQLite Connection Pool File Locking in DeleteDatabase

## Problem

`IDBFactory.DeleteDatabase` performs a raw `File.Delete` on the SQLite database file without first draining the EF Core / `Microsoft.Data.Sqlite` connection pool. On Windows, this will throw an `IOException` ("The process cannot access the file…") when pooled connections still hold open file handles to the `.sqlite`, `-wal`, and `-shm` files.

## Root Cause

EF Core's SQLite provider uses `Microsoft.Data.Sqlite`, which **pools connections by default**. Even after calling `IDBContext.Dispose()` (via `IDBDatabase.Dispose()`), the underlying `SqliteConnection` is returned to the pool — **not closed**. The OS file handle stays open.

`DeleteDatabase` has **no awareness** of whether an `IDBDatabase` was previously opened for the same name, and makes no attempt to clear the pool before deleting files.

## Audit Findings

### Issue 1 — Connection pool not cleared before file deletion (Critical)

In [IDBFactory.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/IDBFactory.cs), `DeleteDatabase` (line 119) does:

```csharp
System.IO.File.Delete(dbPath);  // Will fail if pool holds a handle
```

The fix is to call `SqliteConnection.ClearPool()` (or `ClearAllPools()`) targeting the connection string for this database **before** attempting `File.Delete`. This is the official Microsoft-recommended approach for releasing SQLite file locks.

### Issue 2 — No open database tracking

`IDBFactory` creates `IDBDatabase` instances in `OpenAsync` but never tracks them. If a caller opens a database and then calls `DeleteDatabase` on the same name without disposing it first, the factory cannot warn or auto-dispose. This is a design consideration rather than a bug — the fix below handles it defensively via pool clearing.

### Issue 3 — WAL/SHM deletion also vulnerable

The sidecar file deletion (lines 134-135) suffers from the same issue — the `-wal` and `-shm` files are also locked by the pooled connection. Clearing the pool before any deletion resolves all three files at once.

## Proposed Changes

### Foundation.IndexedDB

#### [MODIFY] [IDBFactory.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/IDBFactory.cs)

Add `Microsoft.Data.Sqlite` import and update `DeleteDatabase` to:

1. Build the same connection string used by `OpenAsync`
2. Call `SqliteConnection.ClearPool()` with a temporary connection for that connection string to drain all pooled handles
3. Proceed with file deletion only after the pool is cleared

```diff
+using Microsoft.Data.Sqlite;

 public IDBRequest<object> DeleteDatabase(string name)
 {
     IDBRequest<object> request = new IDBRequest<object>();

     string dbPath = Path.Combine(_basePath, "IndexedDB", $"{name}.sqlite");

     if (System.IO.File.Exists(dbPath))
     {
         try
         {
+            //
+            // Clear the SQLite connection pool for this database before deleting files.
+            //
+            // EF Core's SQLite provider pools connections by default, so even after disposing 
+            // the IDBContext, the underlying file handle may still be held by a pooled connection.
+            // Creating a temporary connection with the same connection string and calling ClearPool 
+            // forces all pooled connections for this file to be fully closed and released.
+            //
+            string connectionString = $"Data Source={dbPath}";
+            using (SqliteConnection tempConnection = new SqliteConnection(connectionString))
+            {
+                SqliteConnection.ClearPool(tempConnection);
+            }

             System.IO.File.Delete(dbPath);

             // Clean up WAL mode sidecar files
             string walPath = dbPath + "-wal";
             string shmPath = dbPath + "-shm";
-            if (System.IO.File.Exists(walPath)) System.IO.File.Delete(walPath);
-            if (System.IO.File.Exists(shmPath)) System.IO.File.Delete(shmPath);
+            if (System.IO.File.Exists(walPath) == true)
+            {
+                System.IO.File.Delete(walPath);
+            }
+
+            if (System.IO.File.Exists(shmPath) == true)
+            {
+                System.IO.File.Delete(shmPath);
+            }
         }
```

> [!NOTE] 
> The sidecar `if` blocks are also updated to follow the project's style guidelines (explicit `== true`, always use braces).

## Verification Plan

### Automated Tests
- Run: `dotnet build g:\source\repos\Scheduler\Foundation.IndexedDB\Foundation.IndexedDB.csproj` — must succeed with no errors.
- There are no existing test projects for Foundation.IndexedDB, so a build-only check confirms no compilation regressions.

### Manual Verification
- The fix can be verified by inspecting the code to confirm `SqliteConnection.ClearPool` is called before any `File.Delete` calls, using the same connection string pattern (`Data Source={dbPath}`) as `OpenAsync`.
