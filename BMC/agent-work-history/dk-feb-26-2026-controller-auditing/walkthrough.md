# BMC Controller Auditing — Walkthrough

## Goal
Close the auditing gap in BMC's custom controllers so that all user activity is logged and measurable through the Foundation operational management system.

## Strategy

All custom controllers already inherit from `SecureWebAPIController`, which provides:
- `StartAuditEventClock()` — starts the timer at the top of each action
- `CreateAuditEventAsync(AuditType, message, primaryKey?, exception?)` — logs the audit event

Each action was mapped to an appropriate `AuditType`:

| AuditType | Used For |
|-----------|----------|
| `ReadEntity` | Single-entity reads, renders |
| `ReadList` | List/grid data endpoints |
| `CreateEntity` | Inserts (add part, import set) |
| `UpdateEntity` | Updates (profile, links, themes) |
| `DeleteEntity` | Soft-deletes (remove part, remove image) |
| `Search` | Search/typeahead endpoints |
| `LoadPage` | Page-level data loads (dashboards) |
| `Miscellaneous` | Admin/utility actions |
| `Error` | Exception logging |

## High-Volume Strategy
For `LDrawController` and `PartRendererController`, auditing is **only on cache misses** (actual disk reads / renders) to avoid flooding the auditor with repetitive cache-hit events.

## Changes by Controller

### [ManualGeneratorController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/ManualGeneratorController.cs)
- **Bug fix**: Entity name `"PartRenderer"` → `"ManualGenerator"`
- 3 actions audited: AnalyseUpload, GenerateUpload, DownloadManual

### [MinifigGalleryController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/MinifigGalleryController.cs)
- 1 action: GetMinifigGalleryData (LoadPage)

### [SetExplorerController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/SetExplorerController.cs)
- 1 action: GetSetExplorerData (LoadPage)

### [PartsUniverseController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartsUniverseController.cs)
- 2 actions: GetPartsUniverseData (LoadPage), RefreshPartsUniverseData (Miscellaneous + Error)

### [AiController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/AiController.cs)
- 4 actions: SearchParts, SearchSets (Search), Chat (Miscellaneous + Error), IndexData (Miscellaneous + Error)

### [LDrawController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/LDrawController.cs)
- 1 audit call on **disk reads only** (cache misses)

### [PartsCatalogController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartsCatalogController.cs)
- 4 actions: GetCatalogParts, GetAllCatalogParts, GetCatalogCategories, GetCatalogPartTypes (ReadList)

### [CollectionController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/CollectionController.cs)
- 8 actions: GetMyCollections, GetCollectionParts, AddPart, RemovePart, ImportSet, GetImportedSets, GetWishlist, SearchSets

### [ProfileController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/ProfileController.cs)
- 16 actions: GetMyProfile, UpdateMyProfile, UploadAvatar/Banner, GetAvatar/Banner, DeleteAvatar/Banner, GetMyLinks, SaveMyLinks, GetLinkTypes, GetMyPreferredThemes, SaveMyPreferredThemes, GetPublicProfile

### [PartRendererController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/PartRendererController.cs)
- 15 actions across render, turntable, exploded, step, batch, upload, and search endpoints
- Render endpoints audit **cache misses only** to avoid performance impact

## Build Verification

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Issues Fixed During Implementation
1. **Entity name bug** in `ManualGeneratorController` constructor (`"PartRenderer"` → `"ManualGenerator"`)
2. **Missing imports** — `using System.Threading.Tasks` added to MinifigGalleryController and SetExplorerController
3. **Overload mismatch** — 3 error-logging calls needed `(type, msg, null, ex)` instead of `(type, msg, ex)`
