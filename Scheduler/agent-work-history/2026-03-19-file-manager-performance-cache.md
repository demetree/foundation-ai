# File Manager Performance Audit & In-Memory Cache

**Date:** 2026-03-19

## Summary

Conducted a performance audit of the file manager data access layer and implemented a per-tenant in-memory cache to eliminate N+1 queries, redundant binary data loading, and excessive database round-trips. The audit identified six major performance issues; all were addressed with a new `FileManagerCacheService` and client-side batch loading.

## Changes Made

- **Created `SchedulerServices/FileManagerCacheService.cs`** — Singleton per-tenant in-memory cache holding folders, document metadata (no binary), tag definitions, document-tag mappings, and generated thumbnails. Features: `ConcurrentDictionary` per tenant, `SemaphoreSlim` double-check locking, 5-minute TTL with background refresh timer, targeted invalidation methods.

- **Modified `Scheduler.Server/Controllers/FileManagerController.cs`** — Injected `FileManagerCacheService`. Switched read endpoints (`GetFolders`, `GetFolder`, `GetDocumentsInFolder`, `GetTags`) to use cache. Added cache-first thumbnail strategy to `GetThumbnail` endpoint (avoids repeated full binary DB loads). Added `_cache.InvalidateDocuments/Folders/TagMappings/Thumbnail` calls to all 12 write endpoints (upload, chunked upload, delete, move, restore, new version, metadata update, create/update/delete tag, add/remove tag). Added new batch tag endpoint `GET api/FileManager/Documents/Tags/Batch`.

- **Modified `Scheduler.Server/Program.cs`** — Registered `FileManagerCacheService` as singleton in DI.

- **Modified `Scheduler.Client/src/app/services/file-manager.service.ts`** — Added `getTagsForDocumentsBatch()` method for batch tag loading.

- **Modified `Scheduler.Client/src/app/components/file-manager/file-manager.component.ts`** — Replaced N+1 `loadDocumentTags()` (which fired N separate HTTP requests via `forkJoin`) with a single batch API call.

## Key Decisions

- **Cache is metadata-only** — binary `fileDataData` is never cached in memory to avoid excessive RAM usage.
- **Thumbnails are cached post-generation** — the 80px PNG thumbnail bytes are tiny (~few KB each) and cached after first generation, avoiding repeated full-binary DB loads.
- **Per-tenant isolation** — each tenant gets its own `TenantFileSystemCache` instance, keyed by `tenantGuid`.
- **Targeted invalidation over full flush** — write operations invalidate only the affected cache partition (folders, documents, tags, or specific thumbnails), not the entire tenant cache.
- **Download endpoint confirmed safe** — audited all client-side `downloadDocument` calls; only user-initiated clicks (preview, download) trigger binary loads. The problematic path was `loadThumbnails` which loads full binary on the server side, now cached.

## Testing / Verification

- Server build: **0 errors, 0 warnings** ✅
- Expected reduction: folder navigation drops from ~53 HTTP requests (for 50 docs) to ~3
- Thumbnail binary loads from DB: N per navigation → 0 after first visit (cached)
- Tag loading: N separate per-doc queries → 1 batch request
