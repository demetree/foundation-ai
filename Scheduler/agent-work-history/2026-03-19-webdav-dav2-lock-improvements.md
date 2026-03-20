# WebDAV Server DAV:2 Upgrade & Improvements

**Date:** 2026-03-19

## Summary

Upgraded the Scheduler.WebDAV server from DAV:1 to DAV:1,2 compliance with LOCK/UNLOCK support backed by Foundation.IndexedDB (SQLite), fixed ETag correctness, added conditional request support, a no-op PROPPATCH handler, and improved code quality through deduplication.

## Changes Made

### New Files
- `Services/WebDavLock.cs` — Lock data model (token, docId, owner, scope, expiry)
- `Services/WebDavLockDatabase.cs` — DexterDatabase subclass with lock CRUD, conflict detection, If-header validation, and expired lock cleanup
- `Handlers/LockHandler.cs` — LOCK handler (RFC 4918 §9.10): acquires/refreshes locks, parses lockinfo XML
- `Handlers/UnlockHandler.cs` — UNLOCK handler (RFC 4918 §9.11): releases locks by token
- `Handlers/PropPatchHandler.cs` — No-op PROPPATCH returning 207 success for client compatibility
- `Handlers/MimeTypes.cs` — Shared MIME type utility extracted from PutHandler

### Modified Files
- `Program.cs` — WebDavLockDatabase DI registration + 60-second cleanup timer; Foundation.IndexedDB imports
- `Middleware/WebDavMiddleware.cs` — Added LOCK/UNLOCK/PROPPATCH routing
- `Handlers/OptionsHandler.cs` — DAV:1,2 header + expanded Allow list
- `Handlers/GetHandler.cs` — ETag fix (id+ticks), If-None-Match/If-Modified-Since 304 support
- `Handlers/HeadHandler.cs` — ETag fix (id+ticks)
- `Handlers/PutHandler.cs` — Delegated GuessMimeType to shared MimeTypes.FromFileName
- `Handlers/PathResolver.cs` — Added ResolveDocumentAsync helper (deduplicates 7-handler pattern)
- `Xml/DavXmlBuilder.cs` — Added SupportedLockProperty, LockDiscoveryResponse, LockDiscoveryProperty, BuildActiveLockElement
- `Scheduler.WebDAV.csproj` — Added Foundation.IndexedDB project reference

## Key Decisions

- **Lock storage**: Foundation.IndexedDB (SQLite) chosen over in-memory ConcurrentDictionary for persistence across restarts, and over SQL Server because locks are transient/operational, not business data. Single-server scope confirmed.
- **ETag formula**: Changed from immutable `objectGuid` to `{id}-{uploadedDate.Ticks:X}` which rotates on every file update — the old ETag never changed, breaking conditional PUT detection for WebDAV clients.
- **PROPPATCH**: Implemented as a no-op (accepts any property, returns 207 success) because the underlying Document model doesn't support arbitrary WebDAV dead properties. This unblocks macOS Finder and LibreOffice workflows.
- **Deferred**: PUT streaming (needs IFileStorageService streaming API), COPY SQL-level copy, PROPFIND supportedlock wiring.

## Testing / Verification

- `dotnet build Scheduler.WebDAV/Scheduler.WebDAV.csproj` — **0 errors**, 112 pre-existing platform warnings
- WebDAV improvements are compile-verified; functional testing (LOCK/UNLOCK via curl, Office edit, 304 responses) will be done at deploy time
