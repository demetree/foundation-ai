# File Manager Performance: In-Memory Cache Layer

## Server-Side
- [x] Create `FileManagerCacheService` — per-tenant in-memory cache for folders, doc metadata, tags, thumbnails
- [x] Add batch tag endpoint `GET api/FileManager/Documents/Tags/Batch`
- [x] Wire `GetDocumentsInFolder`, `GetFolders`, `GetFolder`, `GetTags` to use cache
- [x] Modify `GetThumbnail` endpoint to use cached thumbnails (cache-first strategy)
- [x] Add cache invalidation to all 7 document write endpoints
- [x] Add cache invalidation to all 5 tag write endpoints
- [x] Register `FileManagerCacheService` as Singleton in DI

## Client-Side
- [x] Add `getTagsForDocumentsBatch(ids)` method to `file-manager.service.ts`
- [x] Replace N+1 `loadDocumentTags()` with single batch call

## Verification
- [x] Server build compiles with 0 errors
