# Set Explorer — Full-Dataset Virtual Scroll + Smart Default Sort

The current set-explorer uses server-side pagination (36 per page) with client-side sorting. This means sorting only reorders the current page of data, making it useless across 20K+ sets. The default sort-by-name surfaces decades-old sets first, which isn't what users want when they land on the page.

## User Review Required

> [!IMPORTANT]
> **Payload size**: The lean DTO for ~20K sets will be roughly **2–3 MB** of JSON. This is cached in **IndexedDB** via the existing `IndexedDBCacheService` with a 24-hour TTL, so subsequent visits load instantly from local storage. Only the first visit (or after TTL expiry) hits the server.

> [!IMPORTANT]
> **Default sort**: The plan defaults to **newest-first** (`year desc`, then `partCount desc`). This surfaces recent, part-rich sets at the top — the sets users are most likely to care about. The "relevance" sort you mentioned (factoring themes, user preferences, etc.) is deferred to a Phase 2 as it requires persisting user preferences and more complex scoring logic. Let me know if you'd like that moved into this phase or if newest-first is a solid starting default.

---

## Proposed Changes

### Server — Custom Lightweight Endpoint

We follow the same `PartsUniverseController` + `PartsUniverseService` pattern already in the project: a service that precomputes a lean DTO list at startup, and a controller that returns it from memory.

---

#### [NEW] [SetExplorerService.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/SetExplorerService.cs)

An `IHostedService` singleton that loads all active, non-deleted `LegoSets` with their `LegoTheme` nav property into a lean DTO list sorted by year descending (newest first), then by part count descending as a tiebreaker. The DTO excludes URLs, guids, and audit fields — only the fields the card grid needs:

```csharp
public class SetExplorerItemDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SetNumber { get; set; }
    public int Year { get; set; }
    public int PartCount { get; set; }
    public string ImageUrl { get; set; }
    public int? ThemeId { get; set; }
    public string ThemeName { get; set; }
}
```

The service materializes the list once at startup and exposes a `GetCachedSets()` method. A `RefreshAsync()` method allows manual re-computation. Estimated payload: ~150 bytes per set × 20K sets ≈ **3 MB** JSON.

---

#### [NEW] [SetExplorerController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/SetExplorerController.cs)

A single `GET /api/set-explorer` endpoint that returns the precomputed list. Modelled on `PartsUniverseController`:
- Returns 503 if data is still loading
- Read-only, BMC Reader role required
- No pagination — the client gets the full dataset

---

#### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Program.cs)

Register `SetExplorerService` as a singleton + `IHostedService` (mirrors the `PartsUniverseService` registration at lines 119-120). Add the controller to the explicit controller list (mirrors line 251).

---

### Client — New API Service

#### [NEW] [set-explorer-api.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/set-explorer-api.service.ts)

A lightweight injectable service that extends `SecureEndpointBase`. Exposes a single method:

```typescript
getExploreSets(): Observable<SetExplorerItem[]>
```

The `SetExplorerItem` interface mirrors the server DTO (id, name, setNumber, year, partCount, imageUrl, themeId, themeName).

**IndexedDB caching**: Internally uses `IndexedDBCacheService.getOrFetch()` with store name `'set-explorer'` and a **24-hour TTL**. On first load the data is fetched from `GET /api/set-explorer` and persisted to IndexedDB. Subsequent visits return the cached data instantly, with automatic refresh when the TTL expires.

---

### Client — Component Rewrite

#### [MODIFY] [set-explorer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.ts)

Major rewrite:

| Before | After |
|---|---|
| Server-paged data (36/page) | Single fetch of full dataset |
| Sorting only works on current page | True global sort across all sets |
| Default sort: name A→Z  | Default sort: year desc → partCount desc (newest, biggest sets first) |
| `*ngFor` grid | `cdk-virtual-scroll-viewport` with custom grid strategy |
| `forkJoin` of list + count calls | Single call to new endpoint |
| Year range: sent as exact match to API, then client-filtered | Fully client-side year range filter |
| Pagination controls | Removed — smooth infinite scroll replaces paging |

Key logic:
1. **One-time load**: on `ngOnInit`, fetch all sets from `GET /api/set-explorer` into `allSets: SetExplorerItem[]`
2. **Filter pipeline**: `allSets` → search/theme/year filter → `filteredSets: SetExplorerItem[]`
3. **Sort**: apply active sort to `filteredSets` (year, name, partCount with asc/desc toggle). Default: year desc, partCount desc.
4. **Virtual scroll**: the `cdk-virtual-scroll-viewport` renders only the visible rows from `filteredSets`
5. **Theme list** is extracted locally from the loaded data (distinct theme names), eliminating the separate theme-list API call
6. **Query param sync** is preserved for deep-linking from LEGO Universe dashboard

---

#### [MODIFY] [set-explorer.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.html)

- Replace the `*ngFor` card grid with a `cdk-virtual-scroll-viewport` containing a CSS grid of cards
- Remove the `pagination-bar` section entirely
- Update the results text from "Page X of Y" to "Showing Z of N sets"
- Replace `<select>` theme dropdown with a searchable theme list extracted from loaded data
- Skeleton loading remains but is simplified (single spinner or minimal skeleton while the dataset loads)

---

#### [MODIFY] [set-explorer.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.scss)

- Add `.virtual-viewport` styling with `height: calc(100vh - <header_height>px)` to fill the available screen
- Remove `.pagination-bar` and `.page-btn` styles (no longer needed)
- Keep all existing card, filter bar, sort bar, skeleton, and empty state styles

---

## Verification Plan

### Automated Tests

**Server build**:
```
dotnet build g:\source\repos\Scheduler\BMC\BMC.Server\BMC.Server.csproj
```

**Client build**:
```
cd g:\source\repos\Scheduler\BMC\BMC.Client
npx ng build --configuration production
```

Both must succeed with no errors.

### Manual Verification

I'll ask you to run the application and manually verify:

1. Navigate to **LEGO Universe → Set Explorer**
2. Confirm sets load with newest sets (2024–2026) appearing first
3. Confirm smooth scroll through 20K+ sets without pagination buttons
4. Toggle sort by **Year**, **Name**, and **Parts** — verify sorting applies globally (not just to a visible slice)
5. Apply a **theme filter** and confirm instant re-filter without re-fetching
6. Search for a specific set name — confirm it filters correctly
7. Click on a set card — confirm navigation to set detail still works
8. Test a deep link from the LEGO Universe dashboard (e.g. clicking a year on the timeline chart)
