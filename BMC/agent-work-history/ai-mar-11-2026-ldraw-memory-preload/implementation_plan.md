# Optimize MOC Viewer 3D Loading Performance

## Problem

A 1600-part model generates **thousands of individual HTTP requests** because:

1. Three.js `LDrawLoader.fetchData()` uses **trial-and-error** — for each sub-file reference, it tries up to 12 URLs (6 directory prefixes × lowercase retry). Each 404 is a wasted round-trip.
2. LDraw parts are **hierarchical** — a single part like `3001.dat` references `stud.dat`, `3-4cyli.dat`, etc., which reference further primitives. A 1600-part model can trigger 5,000+ HTTP requests.
3. Browser limits concurrent connections (~6 per origin), creating severe queuing.

## Proposed Solution: Server-Side Dependency Bundling

> [!TIP]
> Instead of the client fetching each sub-file individually, the server **pre-resolves all dependencies** and embeds them as `0 FILE` blocks in the MPD text. The LDrawLoader sees inline sub-files and skips all external fetches.

### How It Works

```mermaid
graph LR
    A[Client requests MPD] --> B[Server: GenerateViewerMpdAsync]
    B --> C[Parse all type-1 references recursively]
    C --> D[Read each .dat file from LDraw library]
    D --> E[Embed as '0 FILE xxx.dat' blocks in MPD]
    E --> F[Return self-contained MPD ~2-5MB]
    F --> G[LDrawLoader.parse: no external fetches needed]
```

**Before**: 1 MPD request + 3,000-5,000 sub-file requests (many 404s) = minutes of loading
**After**: 1 MPD request (larger payload, ~2-5 MB) = seconds of loading

---

### [MODIFY] [ModelExportService.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/ModelExportService.cs)

#### New method: `BundleLDrawDependenciesAsync`

After `GenerateViewerMpdAsync` produces the base MPD:

1. Scan all type-1 lines in the MPD for `.dat` part references
2. For each unique part filename, read the `.dat` file from the LDraw data directory
3. Recursively scan those files for their own sub-file references (primitives like `stud.dat`, `3-4cyli.dat`)
4. Append all resolved files as `0 FILE filename.dat` / `0 NOFILE` blocks at the end of the MPD
5. Skip any files already embedded (custom parts from .io), skip files that can't be found

#### Key design decisions:
- **LDConfig.ldr** should also be embedded (colour definitions)
- Use the existing `LDrawController._fileIndex` pattern for O(1) filename resolution
- Cache the bundled result per-project (optional, for repeated loads)
- Add a size guard — if the bundled MPD exceeds ~20 MB, fall back to unbundled mode

---

### [MODIFY] [moc-viewer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/moc-viewer/moc-viewer.component.ts)

- The existing `loader.setPartsLibraryPath()` call stays as a fallback for any files the bundler missed
- No major changes needed — the LDrawLoader's `parse()` method already handles inline `0 FILE` blocks natively (see lines 703-721 of LDrawLoader.js)

## Verification Plan

### Manual Verification
1. Load the 1600-part model — should complete in seconds instead of minutes
2. Open browser DevTools Network tab — verify significantly reduced request count
3. Verify the 3D model renders correctly (no missing parts)
4. Test with the small 3-part model to verify no regressions
