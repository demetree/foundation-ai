# Minifig Gallery → CDK Virtual Scroll + IndexedDB Caching

The current minifig-gallery uses server-side pagination (48/page, 2 API calls per page turn), a `*ngFor` grid, and a search bar. This plan mirrors the set-explorer treatment.

## User Review Required

> [!IMPORTANT]
> **Year field is derived, not stored.** `LegoMinifig` has no `year` column. The new server endpoint will join through `LegoSetMinifig` → `LegoSet` to compute `MAX(legoSet.year)` per minifig as the DTO's `Year` field. Minifigs not linked to any set will get `Year = 0` and sort to the end.

---

## Proposed Changes

### Server — New Precomputed Endpoint

#### [NEW] [MinifigGalleryService.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/MinifigGalleryService.cs)

`BackgroundService` that queries all active, non-deleted minifigs at startup, joins through `LegoSetMinifigs` → `LegoSet` to derive max year, projects to a lean DTO, sorts by year desc then name asc, and holds in memory.

```csharp
public class MinifigGalleryItemDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FigNumber { get; set; }
    public int PartCount { get; set; }
    public string ImageUrl { get; set; }
    public int Year { get; set; }        // MAX(legoSet.year), 0 if unlinked
}
```

#### [NEW] [MinifigGalleryController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/MinifigGalleryController.cs)

`GET /api/minifig-gallery` → returns the precomputed list.

#### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Program.cs)

Register `MinifigGalleryService` as hosted service + singleton accessor.

---

### Client — New API Service

#### [NEW] [minifig-gallery-api.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/minifig-gallery-api.service.ts)

Fetches from `api/minifig-gallery`, wraps with `IndexedDBCacheService.getOrFetch('minifig-gallery', {}, …, 1440)`. Returns `Observable<MinifigGalleryItem[]>`.

---

### Client — Component Rewrite

#### [MODIFY] [minifig-gallery.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/minifig-gallery/minifig-gallery.component.ts)

- Load all minifigs once via `MinifigGalleryApiService`
- **Default sort: year desc, name asc**
- Client-side search filter (replaces server `anyStringContains`)
- Client-side sort pipeline (year, name, partCount, figNumber)
- Chunk into `CardRow[]` for CDK virtual scroll
- Responsive column calculation with `@HostListener('window:resize')`
- Remove all server-paged state

#### [MODIFY] [minifig-gallery.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/minifig-gallery/minifig-gallery.component.html)

- Replace `*ngFor` grid with `cdk-virtual-scroll-viewport` + `*cdkVirtualFor`
- Add sort controls, show `year` badge on cards
- Remove pagination, update result count to show filtered/total

#### [MODIFY] [minifig-gallery.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/minifig-gallery/minifig-gallery.component.scss)

- Add viewport and card-row styles, remove `.pagination` styles

---

## Verification Plan

### Automated Tests
- `dotnet build BMC.Server` → 0 errors
- `npx ng build --configuration production` → bundle generated
