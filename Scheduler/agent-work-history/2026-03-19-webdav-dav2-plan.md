# WebDAV Server Improvements

Upgrade the Scheduler.WebDAV server from DAV:1 to DAV:2 with LOCK/UNLOCK support, fix correctness bugs, and improve performance and client compatibility.

## User Review Required

> [!IMPORTANT]
> The LOCK/UNLOCK implementation uses `Foundation.IndexedDB` (SQLite-backed) for lock persistence. Locks survive server restarts but are inherently per-server — if you scale to multiple WebDAV instances, locks would need to move to the shared SQL Server database instead. For single-server deployment (current), SQLite is lightweight and ideal.

> [!WARNING]
> **ETag bug**: The current ETag uses `objectGuid` which is immutable — it never changes when a file is updated. This means WebDAV clients cannot detect file changes via ETag, breaking conditional PUT (overwrite detection). The fix changes ETag to a hash of `id + uploadedDate`, which rotates on every update.

---

## Proposed Changes

### LOCK/UNLOCK Support (Foundation.IndexedDB)

#### [NEW] [WebDavLockDatabase.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Services/WebDavLockDatabase.cs)

`DexterDatabase` subclass with a single `Locks` table:

```csharp
public class WebDavLock
{
    public string LockToken { get; set; }    // opaque URI token (urn:uuid:...)
    public int DocumentId { get; set; }       // locked document
    public string Owner { get; set; }         // lock owner (username or href)
    public Guid TenantGuid { get; set; }      // tenant isolation
    public int Depth { get; set; }            // 0 or infinity
    public string LockScope { get; set; }     // "exclusive" or "shared"
    public DateTime ExpiresAt { get; set; }   // auto-cleanup threshold
    public DateTime CreatedAt { get; set; }
}
```

Schema DSL: `"++id, &lockToken, documentId, tenantGuid"`

Stored in a `.sqlite` file alongside the WebDAV server binary (e.g., `./Data/webdav-locks.sqlite`).

#### [NEW] [LockHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/LockHandler.cs)

Handles `LOCK` requests (RFC 4918 §9.10):
- Parses the `lockinfo` XML body for scope, type, and owner
- Resolves path to a document via `PathResolver`
- Checks for conflicting locks (exclusive lock already held by another user)
- Creates a `WebDavLock` record with a `urn:uuid:{Guid}` token
- Returns 200 with `Lock-Token` header and `DAV:prop/lockdiscovery` XML body
- Supports lock refresh via `If` header (no body, extends timeout)

#### [NEW] [UnlockHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/UnlockHandler.cs)

Handles `UNLOCK` requests (RFC 4918 §9.11):
- Extracts `Lock-Token` header
- Validates the token belongs to the requesting user and tenant
- Deletes the lock record
- Returns 204 No Content

#### [MODIFY] [WebDavMiddleware.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Middleware/WebDavMiddleware.cs)

- Add `LOCK` and `UNLOCK` cases to the method switch
- Add `PROPPATCH` case (no-op handler)

#### [MODIFY] [OptionsHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/OptionsHandler.cs)

- Change `DAV` header from `"1"` to `"1, 2"`
- Add `LOCK`, `UNLOCK`, `PROPPATCH` to `Allow` header

#### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Program.cs)

- Register `WebDavLockDatabase` as singleton via `IDBFactory.OpenAsync()`
- Add project reference to `Foundation.IndexedDB`
- Start a cleanup timer for expired locks (runs every 60 seconds)

---

### Bug Fixes

#### [MODIFY] [GetHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/GetHandler.cs) + [HeadHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/HeadHandler.cs)

**ETag fix**: Replace `objectGuid:N` with a hash of `{id}-{uploadedDate:O}` which changes on every update:

```diff
-context.Response.Headers["ETag"] = $"\"{doc.objectGuid:N}\"";
+context.Response.Headers["ETag"] = $"\"{doc.id}-{doc.uploadedDate.Ticks:X}\"";
```

**Conditional request support** (GET/HEAD only):
- Check `If-None-Match` against current ETag → return 304
- Check `If-Modified-Since` against `uploadedDate` → return 304

#### [MODIFY] [PutHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/PutHandler.cs)

**Stream to temp file instead of `byte[]`**: Use the existing `ChunkBufferService` pattern — stream `Request.Body` to a temp file, then read from the temp file for DB storage. This prevents OOM on large uploads.

Also add `If` header lock token validation: if the document is locked, the PUT must include the correct lock token.

#### [MODIFY] [CopyHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/CopyHandler.cs)

The COPY handler currently loads the full source document binary into memory. For the copy, use a direct SQL `INSERT ... SELECT` approach via the `SchedulerContext` to copy the binary at the database level without materializing it in server memory.

---

### Compatibility

#### [NEW] [PropPatchHandler.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/PropPatchHandler.cs)

No-op PROPPATCH handler — accepts any XML body and returns a 207 Multi-Status response indicating all requested properties were "set" (without actually changing anything). This satisfies clients that send PROPPATCH after PUT (e.g., macOS Finder sets `getlastmodified`).

#### [MODIFY] [DavXmlBuilder.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Xml/DavXmlBuilder.cs)

Add `supportedlock` property to file/collection responses so clients know locking is available:

```xml
<D:supportedlock>
  <D:lockentry>
    <D:lockscope><D:exclusive/></D:lockscope>
    <D:locktype><D:write/></D:locktype>
  </D:lockentry>
</D:supportedlock>
```

Add `lockdiscovery` XML builder method for LOCK responses.

---

### Performance / Code Quality

#### [MODIFY] [PathResolver.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/PathResolver.cs)

Add a `ResolveDocumentAsync` helper that combines the repeated pattern of:
1. Resolve path → get folder ID + document name
2. Fetch documents in folder
3. Match by `fileName` or `name` (case-insensitive)

This pattern is currently duplicated in GET, HEAD, PUT, DELETE, MOVE, COPY, and PROPFIND (7 handlers).

#### [NEW] [MimeTypes.cs](file:///g:/source/repos/Scheduler/Scheduler.WebDAV/Handlers/MimeTypes.cs)

Extract `GuessMimeType` from `PutHandler` into a shared static utility class so it can be reused.

---

## Verification Plan

### Build
- `dotnet build Scheduler.WebDAV/Scheduler.WebDAV.csproj` — must compile with 0 errors

### Manual Testing
1. **LOCK/UNLOCK**: Use `curl` to LOCK a document, verify the lock token, then UNLOCK it
2. **Office edit**: Map the WebDAV drive in Windows Explorer, open a .docx file in Word, edit and save — should work without "read-only" warnings
3. **Conditional GET**: `curl -H "If-None-Match: \"<etag>\"" ...` → verify 304
4. **Large PUT**: Upload a 100MB+ file and verify no OOM (temp file approach)
5. **PROPPATCH**: macOS Finder copy-to-WebDAV workflow — verify no 405 errors
