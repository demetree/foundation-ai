# Upload Crash Fix, Chunk Cleanup & Sidebar Redesign

**Date:** 2026-03-20

## Summary

Three related improvements to the file manager: fixed a client-side crash when uploading multiple large files sequentially, fixed server-side chunk buffer cleanup that was silently failing, and redesigned the sidebar to reduce visual clutter.

## Changes Made

### Upload Crash Fix (NgZone)

- **`file-manager.component.ts`** — Injected `NgZone` and wrapped all `uploadDocumentChunked()` calls and progress callbacks in `ngZone.run()`. The chunked upload service uses raw `fetch()` which runs outside Angular's zone, causing state changes between sequential file uploads to be invisible to change detection. Applied to both `uploadFiles()` (standalone) and `uploadBatch()` (folder import) code paths.

### Chunk Buffer Cleanup

- **`Foundation.IndexedDB/IDBDatabase.cs`** — Added `Close()` method that disposes the context, then clears the SQLite connection pool via `SqliteConnection.ClearPool()`. `Dispose()` now delegates to `Close()`. Without pool clearing, EF Core's SQLite provider keeps file handles alive after dispose.
- **`ChunkBufferService.cs`** — Changed `CleanupSession()` to call `db.IndexedDB.Close()` instead of `db.Dispose()`. Added `CleanupOrphanedSessions()` called from the constructor to purge stale session directories on server startup.

### Sidebar Redesign

- **`file-manager.component.ts`** — Added `sidebarFavoritesOpen`, `sidebarTagsOpen`, `sidebarLinksOpen` boolean properties (all default `false`).
- **`file-manager.component.html`** — Replaced 6 stacked always-visible sidebar sections with collapsible accordion sections (Favorites, Tags, Entity Links) and a compact horizontal icon toolbar (Trash, Activity).
- **`file-manager.component.scss`** — Added `.fm-sidebar-accordion`, `.fm-accordion-header/chevron/title/badge/body`, `.fm-quick-nav`, `.fm-quick-nav-btn/badge` styles with chevron rotation animation.

## Key Decisions

- **NgZone wrapping** chosen over migrating `fetch()` to `HttpClient` because the chunked upload logic requires streaming progress callbacks that `HttpClient` doesn't support as cleanly.
- **`Close()` vs modifying `Dispose()`** — Added a separate `Close()` method to be explicit about connection pool clearing. `Dispose()` delegates to it for backward compatibility.
- **Accordion default collapsed** — All sidebar sections start collapsed to keep the initial view clean. Count badges on collapsed headers provide context without expanding.
- **Icon toolbar for Trash/Activity** — These are single-click toggle actions, not lists, so icon buttons are more appropriate than full sections with headers.

## Testing / Verification

- Two-file folder upload (2×500MB) completed successfully without crash after NgZone fix
- Angular build verified clean (only pre-existing volunteer-group template warning)
- Server-side `dotnet build --no-restore` succeeded for chunk cleanup changes
