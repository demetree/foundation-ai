# WebDAV Server Improvements

## LOCK/UNLOCK Support (DAV:2)
- [x] Create `WebDavLockDatabase` extending `DexterDatabase` with lock store schema
- [x] Create `LockHandler.cs` and `UnlockHandler.cs`
- [x] Add `If` header validation to lock operations
- [x] Update OptionsHandler to advertise DAV:2 and LOCK/UNLOCK methods
- [x] Wire lock database into DI in `Program.cs`
- [x] Add lock cleanup timer for expired locks

## Bug Fixes
- [x] Fix ETag — now uses `{id}-{uploadedDate.Ticks:X}` which changes on file update
- [ ] Fix PUT handler buffering entire request body into memory (deferred — needs IFileStorageService streaming API)
- [ ] Fix COPY handler loading full binary into memory (deferred — needs SQL-level copy)

## Compatibility
- [x] Add PROPPATCH no-op handler (returns 207 success)
- [x] Add `If-None-Match` / `If-Modified-Since` conditional support (304 responses)
- [ ] Add `supportedlock` property to PROPFIND responses (XML builder ready, wiring deferred)

## Performance / Code Quality
- [x] Extract duplicate document resolution into `PathResolver.ResolveDocumentAsync`
- [x] Share `GuessMimeType` — extracted to `MimeTypes.cs` utility
