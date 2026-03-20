# File Manager Performance: In-Memory Cache Layer

## Problem

The file manager data access layer has several compounding performance issues that cause sluggish navigation:

### Audit Findings

| Issue | Impact | Location |
|-------|--------|----------|
| **N+1 tags** | 1 HTTP + DB query **per document** to load tags | `loadDocumentTags()` in component, `GetTagsForDocument` endpoint |
| **N+1 thumbnails** | 1 download request **per image** on every folder nav | `loadThumbnails()` in component |
| **17 LEFT JOINs** | Every listing/search query joins 17 entity tables | `GetDocumentsInFolderAsync` + `SearchDocumentsAsync` |
| **Full folder reload** | ALL folders re-fetched on every navigation | `loadFolders()` called on init + refresh |
| **Binary in GetById** | `GetDocumentByIdAsync` loads `fileDataData` for all uses | Used for downloads AND metadata lookups |
| **No caching** | Every action triggers fresh DB round-trips | No server or client-side caching |
| **Thumbnails load full binary** | `GetThumbnail` endpoint loads entire document binary from DB to generate 80px PNG | `FileManagerController.GetThumbnail` → `GetDocumentByIdAsync` |

### Download Endpoint Audit

| Call Site | Trigger | Binary Loaded? | User-Initiated? | Verdict |
|-----------|---------|----------------|------------------|---------|
| `selectDocument` (image/PDF) | Click on document | ✅ Full binary | ✅ Yes | ✅ OK — preview on user click |
| `selectDocument` (text) | Click on text file | ✅ Full binary | ✅ Yes | ✅ OK — preview on user click |
| `downloadFile` | Double-click / button | ✅ Full binary | ✅ Yes | ✅ Correct |
| `loadThumbnails` | **Every folder navigation** | ✅ Full binary **on server** | ❌ Automatic | 🔴 **N full binary DB loads per nav** |

> [!CAUTION]
> The N+1 tag loading alone generates **50+ HTTP requests** for a folder with 50 documents. The thumbnail endpoint loads the **entire document binary** from the DB to generate an 80px thumbnail — for every image, on every folder navigation. Combined with the 17 JOINs per listing, this creates severe latency.

## Proposed Architecture

### Server-Side: `FileManagerCacheService` (tenant-isolated)

A scoped service with a per-tenant `ConcurrentDictionary` holding the full file system tree in memory:

```
Dictionary<Guid, TenantFileSystemCache>
  ├── folders: Dictionary<int, FolderMetadata>
  ├── documents: Dictionary<int, DocumentMetadata>  // NO binary
  ├── documentTags: Dictionary<int, List<int>>       // docId → tagIds
  ├── tags: Dictionary<int, TagMetadata>
  └── lastRefreshed: DateTime
```

**Key design decisions:**
- Cache is **metadata-only** — binary `fileDataData` is never cached
- Cache is **per-tenant** with automatic isolation via `tenantGuid`
- Cache is populated on first access, then maintained via invalidation
- Periodic background refresh every ~5 minutes catches external changes
- Write operations (upload/delete/move/tag) invalidate affected cache entries immediately

---

### Proposed Changes

### Server-Side Cache Service

#### [NEW] [FileManagerCacheService.cs](file:///g:/source/repos/Scheduler/SchedulerServices/FileManagerCacheService.cs)

Per-tenant in-memory cache holding folders, document metadata (no binary), tags, document-tag mappings, and **generated thumbnails**. Provides:
- `GetOrLoadFoldersAsync(tenantGuid)` — returns cached folders, loads on miss
- `GetOrLoadDocumentsInFolderAsync(folderId, tenantGuid)` — cached doc metadata
- `GetOrLoadTagMappingsAsync(tenantGuid)` — ALL doc-tag mappings in one query
- `GetOrLoadThumbnailAsync(documentId, tenantGuid)` — cached generated thumbnail bytes
- `InvalidateDocuments(tenantGuid, folderId?)` — targeted invalidation
- `InvalidateFolders(tenantGuid)` — folder tree invalidation
- Background timer for periodic full refresh

> [!TIP]
> Thumbnail cache avoids repeated full-binary loads. Thumbnails are tiny (~few KB) so caching all of them per tenant is cheap. Invalidation happens on document upload/delete.

---

### Batch Endpoint Fixes (eliminate N+1)

#### [MODIFY] [FileManagerController.cs](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Server/Controllers/FileManagerController.cs)

- Add `GET api/FileManager/Documents/Tags/Batch?documentIds=1,2,3` — returns all tag mappings for a list of documents in one query
- Modify `GetDocumentsInFolder` to use cache service instead of direct DB
- Modify `GetFolders` to use cache service

#### [MODIFY] [SqlFileStorageService.cs](file:///g:/source/repos/Scheduler/SchedulerServices/SqlFileStorageService.cs)

- Add `GetDocumentMetadataByIdAsync()` — like `GetDocumentByIdAsync()` but excludes binary
- Reduce `.Include()` chains: project entity names via `.Select()` instead of 17 LEFT JOINs
- Add `GetTagMappingsForFolderAsync(folderId, tenantGuid)` — batch tag query
- Modify `GetThumbnail` endpoint to use cache service (avoid full binary load from DB on every request)

---

### Client-Side Fixes

#### [MODIFY] [file-manager.component.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/file-manager/file-manager.component.ts)

- Replace N+1 `loadDocumentTags()` with single batch API call
- Cache `allFolders` locally — don't re-fetch on folder navigation
- Debounce `loadThumbnails()` — only fetch visible thumbnails

#### [MODIFY] [file-manager.service.ts](file:///g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/services/file-manager.service.ts)

- Add `getTagsForDocuments(documentIds: number[])` batch method
- Add client-side folder cache with TTL

---

### Cache Invalidation Flow

#### Write Operations → Cache:
- **Upload/Delete/Move document** → `InvalidateDocuments(tenantGuid, folderId)`
- **Create/Update/Delete folder** → `InvalidateFolders(tenantGuid)`
- **Add/Remove tag** → `InvalidateTagMappings(tenantGuid)`
- **SignalR events** → trigger invalidation for other connected clients

## Verification Plan

### Before/After Measurement
- Count HTTP requests in browser Network tab for folder navigation (before vs after)
- Measure response time for `GetDocumentsInFolder` with/without cache
- Verify N+1 tags eliminated (should be 1 request instead of N)

### Functional Testing
- Navigate folders — verify cached results match DB
- Upload/delete/move documents — verify cache invalidation
- Tag operations — verify batch endpoint returns correct mappings
- Multi-tab — verify SignalR triggers cache refresh
