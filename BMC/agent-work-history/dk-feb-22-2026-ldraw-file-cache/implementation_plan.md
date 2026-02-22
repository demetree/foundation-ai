# IndexedDB Caching for LDraw File Data

## Problem

Both `catalog-part-detail` (interactive 3D viewer) and `ldraw-thumbnail.service.ts` (batch thumbnail renderer) fetch LDraw geometry files from the server API (`api/ldraw/file/<path>`) on every page load. These files are static text (`.dat`, `.ldr`) that never change, but each part load can trigger 10-50+ HTTP requests for the main file plus all its sub-parts.

The thumbnail service caches the *rendered PNGs* in IndexedDB, but the raw LDraw source files are re-fetched every time. The detail viewer has **no caching at all**.

## Proposed Changes

### LDraw File Cache Service

#### [NEW] [ldraw-file-cache.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/ldraw-file-cache.service.ts)

A shared Dexie-backed IndexedDB cache for LDraw file content that intercepts `LDrawLoader` HTTP requests:

- **IndexedDB store**: `bmc-ldraw-files-v1` with schema `{ filePath, content, cachedAt }`
- **`createCachingManager(authToken)`**: Returns a `THREE.LoadingManager` with a custom `urlModifier` that:
  1. Before fetch: checks IndexedDB for the file path
  2. If cached: returns the content as a blob URL (instant, no network)
  3. If not cached: lets the fetch proceed, then stores the response text in IndexedDB for next time
- Both consumers (detail viewer and thumbnail service) use this manager when constructing their `LDrawLoader`

**Key design decisions:**
- Cache at the file level (not model level) because sub-parts are shared across many models — caching `3001.dat` benefits every model that uses a 2x4 brick
- No cache expiration — LDraw geometry is immutable. DB version bump invalidates if needed
- Fire-and-forget writes — don't block rendering on IndexedDB persistence

> [!IMPORTANT]  
> The Three.js `LDrawLoader` uses `FileLoader` internally which respects `LoadingManager`. However, `urlModifier` only transforms URLs — it can't intercept the actual fetch. The correct approach is to patch the loader's `FileLoader` fetch path via a **Service Worker** or by overriding `THREE.FileLoader.prototype.load`. 
>
> The cleanest viable approach: create a wrapper that **pre-fetches** files using a cached fetch utility and provides them to `LDrawLoader` via blob URLs in the `urlModifier`.

### Integration Points

#### [MODIFY] [catalog-part-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.ts)

- Inject `LDrawFileCacheService`
- In `loadLDrawModel()`, use the cache service to install a caching `LoadingManager` on the `LDrawLoader`

#### [MODIFY] [ldraw-thumbnail.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/ldraw-thumbnail.service.ts)

- Inject `LDrawFileCacheService`  
- In `ensureInitialized()`, use the cache service's loading manager

## Verification Plan

### Automated Tests
- `ng build --configuration production` — ensure clean compile

### Manual Verification
- Open part detail page → verify 3D model loads
- Navigate back and re-open same part → observe instant load from cache (no network requests in DevTools)
- Open a different part that shares sub-files → observe partial cache hits
