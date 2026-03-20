# IDBDatabase Close Method for Connection Pool Cleanup

**Date:** 2026-03-20

## Summary

Added a `Close()` method to `IDBDatabase` that properly clears the SQLite connection pool before disposing, preventing file handle leaks that blocked file deletion.

## Changes Made

- **`IDBDatabase.cs`** — Added `Close()` method that retrieves the connection string, disposes the `DbContext` and semaphore, then calls `SqliteConnection.ClearPool()` on a temporary connection. `Dispose()` now delegates to `Close()`. Added `using Microsoft.Data.Sqlite` import.

## Key Decisions

- `IDBFactory.DeleteDatabase()` already had the correct pool-clearing pattern. `Close()` reuses this approach at the instance level so consumers like `ChunkBufferService` can release file handles without deleting the entire database.
- Named `Close()` (not just fixing `Dispose()`) to be explicit — callers who need file handle release should call `Close()`, while `Dispose()` delegates to it for backward compatibility.

## Testing / Verification

- `dotnet build --no-restore` succeeded on the Scheduler.Server project (which depends on Foundation.IndexedDB)
- ChunkBufferService verified to use `Close()` for session cleanup
