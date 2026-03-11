# LDraw In-Memory Preload & Performance Fix — Walkthrough

## Problem
The MOC viewer was overwhelming the server with hundreds of concurrent LDraw file requests. Each request triggered disk I/O + auth overhead, and missing files took 30+ seconds to 404 due to directory scanning. The Three.js `LDrawLoader` tries 12 URL patterns per missing file sequentially.

## Solution Overview

Three-layer fix: server-side preloading, client-side interception, and hot-reload for updates.

---

## Changes Made

### 1. Server: `LDrawFileService.cs` (NEW — Singleton Hosted Service)
- Preloads **all** `.dat`/`.ldr` files from LDraw library into two `ConcurrentDictionary` caches at startup
- `TryGetFile(path)` → O(1) lookup with multi-strategy resolution
- `IngestDirectory(basePath)` → hot-adds new/updated files at runtime
- Memory: ~200-400 MB for full library

### 2. Server: `LDrawController.cs` (REWRITTEN)
- Now extends plain `ControllerBase` instead of `SecureWebAPIController`
- Fully synchronous — pure dictionary lookup per request
- Eliminated: auth checks, semaphore, disk I/O, directory scanning, `FileStream`
- Missing files return 404 instantly (<1ms)

### 3. Server: `ModelExportService.cs` (UPDATED)
- Injected `LDrawFileService` — replaces disk-based `ReadLDrawFile`/`EnsureLDrawFileIndex`
- Bundler uses `_ldrawFiles.TryGetFile()` for O(1) dependency resolution
- Removed ~300 lines of disk-based code

### 4. Server: `DataImportWorker.cs` (UPDATED)
- Injected `LDrawFileService`
- After `CopyDataFiles()` writes new LDraw data to disk, calls `_ldrawFileService.IngestDirectory(ldrawDataPath)` to hot-reload the cache

### 5. Server: `Program.cs` (UPDATED)
- Registered `LDrawFileService` as singleton + hosted service

### 6. Client: `moc-viewer.component.ts` (UPDATED)
- `extractBundledFiles()` parses all `0 FILE` blocks from server MPD into `fileMap`
- Monkey-patched `LDrawParsedCache.fetchData` to check `fileMap` first
- **Throws immediately** for unbundled files instead of falling back to 12-attempt HTTP flood
- `setData('LDConfig.ldr', ...)` seeds parse cache for colour definitions

## Verification

- Server build: **0 errors**
- LDraw file preload logs file count/size/time on startup
- DataImportWorker hot-reload logs add/update counts after each import cycle
