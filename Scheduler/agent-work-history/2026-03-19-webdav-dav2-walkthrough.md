# WebDAV Server Improvements — Walkthrough

## Summary

Upgraded the Scheduler.WebDAV server from **DAV:1** to **DAV:1,2** compliance with LOCK/UNLOCK support, fixed several bugs, and improved code quality.

---

## Changes Made

### New Files (6)

| File | Purpose |
|------|---------|
| [WebDavLock.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Services/WebDavLock.cs) | Lock data model (token, docId, owner, expiry, scope) |
| [WebDavLockDatabase.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Services/WebDavLockDatabase.cs) | `DexterDatabase` subclass — CRUD, conflict detection, If-header validation, expired lock cleanup |
| [LockHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/LockHandler.cs) | LOCK handler — acquires/refreshes locks, parses lockinfo XML, returns lockdiscovery XML |
| [UnlockHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/UnlockHandler.cs) | UNLOCK handler — releases locks by token |
| [PropPatchHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/PropPatchHandler.cs) | No-op PROPPATCH — returns 207 success for client compatibility |
| [MimeTypes.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/MimeTypes.cs) | Shared MIME type utility extracted from PutHandler |

### Modified Files (8)

| File | Change |
|------|--------|
| [Program.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Program.cs) | `WebDavLockDatabase` DI registration + 60s cleanup timer |
| [WebDavMiddleware.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Middleware/WebDavMiddleware.cs) | LOCK/UNLOCK/PROPPATCH routing |
| [OptionsHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/OptionsHandler.cs) | DAV:1,2 + expanded Allow header |
| [GetHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/GetHandler.cs) | ETag fix + If-None-Match/If-Modified-Since 304 support |
| [HeadHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/HeadHandler.cs) | ETag fix (id+ticks) |
| [PutHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/PutHandler.cs) | MimeTypes delegation |
| [PathResolver.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/PathResolver.cs) | New `ResolveDocumentAsync` helper |
| [DavXmlBuilder.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Xml/DavXmlBuilder.cs) | Lock XML: `SupportedLockProperty`, `LockDiscoveryResponse`, `LockDiscoveryProperty` |
| [Scheduler.WebDAV.csproj](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Scheduler.WebDAV.csproj) | `Foundation.IndexedDB` project reference |

---

## Key Design Decisions

### Lock Storage: Foundation.IndexedDB (SQLite)
- Locks persist in `./Data/WebDavLocks.sqlite` alongside the server binary
- WAL mode for concurrent reads during PROPFIND while locks are being acquired
- Auto-cleanup timer every 60s removes expired locks
- Single-server scope confirmed by user — no need for SQL Server lock table

### ETag Formula
```diff
-doc.objectGuid:N     // Immutable — never changes on update!
+doc.id-doc.uploadedDate.Ticks:X   // Changes every time the file is updated
```

---

## Deferred Items

- **PUT streaming** — still buffers to `byte[]`; needs `IFileStorageService` streaming API for proper fix
- **COPY SQL-level** — still materializes binary in memory; would need raw SQL `INSERT...SELECT`
- **PROPFIND supportedlock** — XML builder is ready, but wiring into PROPFIND responses deferred to keep this changeset focused

---

## Verification

- **Build**: `dotnet build` → **0 errors**, 112 pre-existing platform warnings
