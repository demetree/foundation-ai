# BMC Controller Auditing Gap Analysis

## Executive Summary

**Every custom BMC controller has zero audit calls.** The base class `SecureWebAPIController` provides ready-made helpers (`StartAuditEventClock()` + `CreateAuditEventAsync()`), and each controller already passes `module="BMC"` and a sensible `entityName` in its constructor — but nobody actually _calls_ the audit methods.

Result: User Activity Insights in Foundation's operational dashboard shows **no BMC custom activity whatsoever**.

> [!CAUTION]
> `ManualGeneratorController` sets `entityName = "PartRenderer"` instead of `"ManualGenerator"`. This is a bug — it will pollute the PartRenderer entity's audit logs.

## Auditing Pattern

```csharp
// At the START of the action
StartAuditEventClock();

// ... do work ...

// At the END (success)
await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, "Rendered part 3001.dat at 512x512");

// On error
await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Render failed", primaryKey, ex);
```

The base class auto-fills: `user`, `session`, `source`, `userAgent`, `module`, `moduleEntity`, `resource`, `hostSystem`, `threadId`, `startTime` → `stopTime` (duration).

---

## Controller-by-Controller Gap Analysis

### 1. AiController — Entity: `AI`

| Action | HTTP | AuditType | Message Template |
|--------|------|-----------|------------------|
| `SearchParts` | GET | `Search` | `"AI semantic search: parts — query='{q}', topK={k}"` |
| `SearchSets` | GET | `Search` | `"AI semantic search: sets — query='{q}', topK={k}"` |
| `Chat` | POST | `Miscellaneous` | `"AI RAG chat — prompt='{truncated}'"` |
| `IndexData` | POST | `Miscellaneous` | `"AI index triggered"` |

---

### 2. CollectionController — Entity: `Collection`

| Action | HTTP | AuditType | Message Template |
|--------|------|-----------|------------------|
| `GetMyCollections` | GET | `ReadList` | `"Get my collections"` |
| `GetCollectionParts` | GET | `ReadList` | `"Get collection parts — id={id}"` |
| `AddPart` | POST | `CreateEntity` | `"Add part to collection — id={id}, part={partNum}, colour={colourId}"` |
| `RemovePart` | DELETE | `DeleteEntity` | `"Remove part from collection — id={id}, partId={partId}"` |
| `ImportSet` | POST | `CreateEntity` | `"Import set to collection — id={id}, set={setNumber}"` |
| `GetImportedSets` | GET | `ReadList` | `"Get imported sets — id={id}"` |
| `GetWishlist` | GET | `ReadList` | `"Get wishlist — id={id}"` |
| `SearchSets` | GET | `Search` | `"Collection set search — q='{q}'"` |

---

### 3. LDrawController — Entity: `LDraw`

| Action | HTTP | AuditType | Message Template |
|--------|------|-----------|------------------|
| `GetLDrawFile` | GET | `ReadEntity` | `"LDraw file served — path='{path}'"` |
| `GetLDrawFileByPath` | GET | `ReadEntity` | `"LDraw file served — path='{path}'"` |

> [!NOTE]
> LDraw serves many files per part render (sub-files). Consider auditing at **coarser granularity** — e.g. only audit cache misses (disk reads), not cache hits, to avoid flood.

---

### 4. ManualGeneratorController — Entity: `ManualGenerator` *(currently mislabeled as `PartRenderer`)*

| Action | HTTP | AuditType | Message Template |
|--------|------|-----------|------------------|
| `AnalyseUpload` | POST | `ReadEntity` | `"Manual upload analysed — filename='{name}', steps={count}"` |
| `GenerateUpload` | POST | `CreateEntity` | `"Manual generation started — filename='{name}', id={genId}"` |
| `DownloadManual` | GET | `ReadEntity` | `"Manual downloaded — id='{id}'"` |

---

### 5. PartRendererController — Entity: `PartRenderer`

| Action | HTTP | AuditType | Message Template |
|--------|------|-----------|------------------|
| `Render` | GET | `ReadEntity` | `"Part rendered — part='{partNum}', colour={c}, size={w}x{h}, renderer={r}"` |
| `Turntable` | GET | `ReadEntity` | `"Turntable GIF — part='{partNum}', frames={n}"` |
| `Exploded` | GET | `ReadEntity` | `"Exploded view — part='{partNum}'"` |
| `StepCount` | GET | `ReadEntity` | `"Step count — part='{partNum}'"` |
| `RenderStep` | GET | `ReadEntity` | `"Step render — part='{partNum}', step={s}"` |
| `BatchRender` | POST | `ReadEntity` | `"Batch render — part='{partNum}', sizes={count}"` |
| `RenderUpload` | POST | `ReadEntity` | `"Upload render — filename='{name}'"` |
| `Search` | GET | `Search` | `"Part renderer search — q='{q}'"` |
| `Colours` | GET | `ReadList` | `"Part colours — part='{partNum}'"` |
| `AllColours` | GET | `ReadList` | `"All colours loaded"` |

> [!NOTE] 
> `Render` is the hottest endpoint. Audit only cache **misses** to avoid flood, or use `LoadPage` type for cache hits.

---

### 6. PartsCatalogController — Entity: `PartsCatalog`

| Action | HTTP | AuditType | Message Template |
|--------|------|-----------|------------------|
| `GetCatalogParts` | GET | `ReadList` | `"Catalog browse — page={p}, search='{q}'"` |
| `GetAllCatalogParts` | GET | `ReadList` | `"Full catalog loaded (IndexedDB sync)"` |
| `GetCatalogCategories` | GET | `ReadList` | `"Catalog categories loaded"` |
| `GetCatalogPartTypes` | GET | `ReadList` | `"Catalog part types loaded"` |

---

### 7. PartsUniverseController — Entity: `PartsUniverse`

| Action | HTTP | AuditType | Message Template |
|--------|------|-----------|------------------|
| `GetPartsUniverseData` | GET | `LoadPage` | `"Parts Universe data loaded"` |
| `RefreshPartsUniverseData` | POST | `Miscellaneous` | `"Parts Universe data refresh triggered"` |

---

### 8. MinifigGalleryController — Entity: `MinifigGallery`

| Action | HTTP | AuditType | Message Template |
|--------|------|-----------|------------------|
| `GetMinifigGalleryData` | GET | `LoadPage` | `"Minifig Gallery data loaded"` |

---

### 9. SetExplorerController — Entity: `SetExplorer`

| Action | HTTP | AuditType | Message Template |
|--------|------|-----------|------------------|
| `GetSetExplorerData` | GET | `LoadPage` | `"Set Explorer data loaded"` |

---

### 10. ProfileController — Entity: `Profile`

| Action | HTTP | AuditType | Message Template |
|--------|------|-----------|------------------|
| `GetMyProfile` | GET | `ReadEntity` | `"Get my profile"` |
| `UpdateMyProfile` | PUT | `UpdateEntity` | `"Profile updated"` |
| `UploadAvatar` | POST | `UpdateEntity` | `"Avatar uploaded"` |
| `UploadBanner` | POST | `UpdateEntity` | `"Banner uploaded"` |
| `GetAvatar` | GET | `ReadEntity` | `"Avatar served"` |
| `GetBanner` | GET | `ReadEntity` | `"Banner served"` |
| `DeleteAvatar` | DELETE | `DeleteEntity` | `"Avatar deleted"` |
| `DeleteBanner` | DELETE | `DeleteEntity` | `"Banner deleted"` |
| `GetMyLinks` | GET | `ReadList` | `"Get my links"` |
| `SaveMyLinks` | PUT | `UpdateEntity` | `"Links updated"` |
| `GetMyPreferredThemes` | GET | `ReadList` | `"Get preferred themes"` |
| `SaveMyPreferredThemes` | PUT | `UpdateEntity` | `"Preferred themes updated"` |
| `GetPublicProfile` | GET | `ReadEntity` | `"Public profile viewed — slug='{slug}'"` |

---

## Bug Fix

#### [MODIFY] [ManualGeneratorController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/ManualGeneratorController.cs)

Change constructor from `base("BMC", "PartRenderer")` → `base("BMC", "ManualGenerator")`

## Proposed Changes Summary

| Controller | File | Entity | Actions to Audit |
|-----------|------|--------|-----------------|
| AiController | [AiController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/AiController.cs) | AI | 4 |
| CollectionController | [CollectionController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/CollectionController.cs) | Collection | 8 |
| LDrawController | [LDrawController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/LDrawController.cs) | LDraw | 2 |
| ManualGeneratorController | [ManualGeneratorController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/ManualGeneratorController.cs) | ManualGenerator | 3 + bug fix |
| PartRendererController | [PartRendererController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartRendererController.cs) | PartRenderer | 10 |
| PartsCatalogController | [PartsCatalogController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartsCatalogController.cs) | PartsCatalog | 4 |
| PartsUniverseController | [PartsUniverseController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartsUniverseController.cs) | PartsUniverse | 2 |
| MinifigGalleryController | [MinifigGalleryController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/MinifigGalleryController.cs) | MinifigGallery | 1 |
| SetExplorerController | [SetExplorerController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/SetExplorerController.cs) | SetExplorer | 1 |
| ProfileController | [ProfileController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/ProfileController.cs) | Profile | 13 |
| **Total** | | **10 entities** | **48 actions** |

## High-Volume Endpoint Strategy

For endpoints that get called at very high frequency (LDraw sub-files, Part Renderer cache hits), we should audit **selectively** — e.g. only on cache misses, or only for the top-level request. The base class has built-in **flood protection** (`_auditFloodCheckerCache`) that suppresses duplicate events within the same request, but high-volume reads could still overwhelm the auditor.

## Verification Plan

- Build `.sln` → zero errors
- Sign in → browse Parts Catalog, render a part, open Set Explorer
- Check Foundation User Activity Insights → verify events appear under `BMC` module with correct entities and timing
