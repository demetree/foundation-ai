# LDraw File Cache — Walkthrough

## What Was Done

Added persistent IndexedDB caching for LDraw geometry files (`.dat`, `.ldr`) so they're only fetched from the server once — ever. Subsequent loads (even across browser sessions) are served instantly.

## Approach: THREE.Cache Integration

Rather than intercepting HTTP requests manually, the solution hooks into Three.js's built-in `THREE.Cache` system:

1. **Enable `Cache.enabled = true`** — makes `FileLoader` check `Cache.get()` before fetching
2. **Hydrate from IndexedDB** — pre-populates `Cache.files` on startup
3. **Write-through hook** — monkey-patches `Cache.add()` to persist new entries to IndexedDB (fire-and-forget)

This is transparent to all consumers — any `LDrawLoader` → `FileLoader` request automatically benefits.

## Files Changed

| File | Change |
|------|--------|
| [ldraw-file-cache.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/ldraw-file-cache.service.ts) | **[NEW]** Dexie-backed service, patches `THREE.Cache.add` |
| [catalog-part-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/catalog-part-detail/catalog-part-detail.component.ts) | Inject + call `initialise()` before model load |
| [ldraw-thumbnail.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/ldraw-thumbnail.service.ts) | Inject + call `initialise()` in `processQueue()` |

## Verification

- ✅ `ng build --configuration production` — clean compile (only pre-existing CSS warning)
