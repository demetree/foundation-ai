# File Manager Performance Audit — Walkthrough

## Audit Findings

| Issue | Calls Per Navigation | Fix |
|-------|---------------------:|-----|
| N+1 tag loading | **N** HTTP + DB queries | Single batch endpoint |
| N+1 thumbnail binary loads | **N** full binary reads from DB | Cache-first thumbnails |
| 17 LEFT JOINs per listing | 1 heavy query | Server-side SELECT projection |
| Redundant folder re-fetch | 1 full-table load | Cached with 5-min TTL |
| No caching anywhere | Every click → DB | Per-tenant in-memory cache |

## Changes Made

### New: [FileManagerCacheService.cs](file:///g:/source/repos/Scheduler/SchedulerServices/FileManagerCacheService.cs)
- Per-tenant `ConcurrentDictionary<Guid, TenantFileSystemCache>` holding folders, document metadata (no binary), tags, tag mappings, and generated thumbnails
- Thread-safe lazy loading with `SemaphoreSlim` + double-check locking
- 5-minute TTL with background refresh timer
- Targeted invalidation: `InvalidateFolders()`, `InvalidateDocuments()`, `InvalidateTagMappings()`, `InvalidateThumbnail()`

### Modified: [FileManagerController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FileManagerController.cs)

**Read endpoints → cache:**
- `GetFolders` → `_cache.GetFoldersAsync()`
- `GetFolder` → `_cache.GetFolderByIdAsync()`
- `GetDocumentsInFolder` → `_cache.GetDocumentsInFolderAsync()`
- `GetTags` → `_cache.GetTagsAsync()`
- `GetThumbnail` → cache-first, only load binary on miss

**Write endpoints → invalidation (12 total):**
- Upload, chunked upload, delete, move, restore, new version, metadata update → `InvalidateDocuments()`
- Delete + new version → also `InvalidateThumbnail()`
- Cascade delete → also `InvalidateDocuments()` + `InvalidateFolders()`
- Create/update/delete tag, add/remove tag from doc → `InvalidateTagMappings()`

**New endpoint:**
- `GET api/FileManager/Documents/Tags/Batch?documentIds=1,2,3` — returns all tag mappings in one request

### Modified: [Program.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Program.cs)
- Registered `FileManagerCacheService` as singleton

### Modified: [file-manager.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/file-manager.service.ts)
- Added `getTagsForDocumentsBatch()` method

### Modified: [file-manager.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/file-manager.component.ts)
- Replaced N+1 `loadDocumentTags()` (`forkJoin` of N requests) with single batch call

## Verification

- Server build: **0 errors, 0 warnings** ✅

## Expected Performance Impact

| Metric | Before | After |
|--------|--------|-------|
| HTTP requests per folder nav (50 docs) | **~53** (1 folder + 1 listing + 50 tags + N thumbnails) | **~3** (1 folder + 1 listing + 1 batch tags) |
| DB queries per listing | 1 + 17 JOINs | 0 (cached) |
| Thumbnail: DB binary loads | N per nav | 0 after first visit |
| Tag loading | N separate queries | 1 batch from cache |
