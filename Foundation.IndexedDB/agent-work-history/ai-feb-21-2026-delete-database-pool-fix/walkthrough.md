# Walkthrough: SQLite Connection Pool Fix in DeleteDatabase

## What Changed

Fixed a vulnerability in `IDBFactory.DeleteDatabase` where `File.Delete` would throw `IOException` on Windows because EF Core's SQLite connection pool still held open file handles to the `.sqlite`, `-wal`, and `-shm` files.

### [IDBFactory.cs](file:///g:/source/repos/Scheduler/Foundation.IndexedDB/IDBFactory.cs)

render_diffs(file:///g:/source/repos/Scheduler/Foundation.IndexedDB/IDBFactory.cs)

**Key change:** Before deleting files, we now create a temporary `SqliteConnection` with the same connection string and call `SqliteConnection.ClearPool()` to drain all pooled connections and release file handles.

## Verification

- **Build:** `dotnet build Foundation.IndexedDB.csproj` — ✅ 0 errors, no new warnings
