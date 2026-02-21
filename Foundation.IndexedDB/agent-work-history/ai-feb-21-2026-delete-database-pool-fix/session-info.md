# Session Information

- **Conversation ID:** c8ef0779-0833-45dd-82af-61a72abea6d8
- **Date:** 2026-02-21
- **Time:** 18:36 NST (UTC-03:30)
- **Duration:** ~15 minutes

## Summary

Audited and fixed a SQLite connection pool vulnerability in `IDBFactory.DeleteDatabase` where `File.Delete` would throw `IOException` on Windows because EF Core's connection pool still held open file handles. The fix calls `SqliteConnection.ClearPool()` before file deletion.

## Files Modified

- `Foundation.IndexedDB/IDBFactory.cs` — Added `SqliteConnection.ClearPool()` call before `File.Delete` in `DeleteDatabase`

## Related Sessions

- `ai-feb-15-2026-indexeddb-bug-fixes` — Previous session that fixed WAL/SHM sidecar cleanup and other bugs, but did not address the connection pool locking issue
