# Set Explorer Improvements — Walkthrough

## Summary

Replaced server-side pagination with a precomputed in-memory endpoint + CDK virtual scrolling on the client. The full dataset (~20K sets) is loaded once, cached in IndexedDB for 24 hours, and filtered/sorted entirely client-side.

## Server-Side Changes

### [NEW] [SetExplorerService.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/SetExplorerService.cs)
`BackgroundService` that loads all active, non-deleted LEGO sets at startup into a lean DTO list (`SetExplorerItemDTO`). Sorted newest-first (year desc, partCount desc). Cached in memory for sub-millisecond API response.

### [NEW] [SetExplorerController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/SetExplorerController.cs)
Read-only `GET /api/set-explorer` endpoint. Returns the precomputed list or 503 if still computing. No database round-trip.

### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Program.cs)
Registered `SetExplorerService` as singleton + hosted service. Added `SetExplorerController` to the controller list.

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Server/Program.cs)

---

## Client-Side Changes

### [NEW] [set-explorer-api.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/set-explorer-api.service.ts)
Extends `SecureEndpointBase`. Uses `IndexedDBCacheService.getOrFetch()` with store `'set-explorer'` and 24-hour TTL. First visit fetches from server; subsequent visits load instantly from IndexedDB.

### [MODIFY] [set-explorer.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.ts)
Full rewrite:
- Loads entire dataset once via `SetExplorerApiService`
- Client-side filter pipeline: search (name/number/theme), theme dropdown, year range
- Client-side sorting: defaults to **newest-first** (year desc)
- Chunks filtered results into `CardRow[]` for the virtual-scroll viewport
- Responsive column calculation via `@HostListener('window:resize')`
- Extracts theme list locally from loaded data (no separate API call)

### [MODIFY] [set-explorer.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.html)
Replaced `*ngFor` grid + pagination with `cdk-virtual-scroll-viewport` containing `*cdkVirtualFor` card rows. Badge now shows filtered count vs total.

### [MODIFY] [set-explorer.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-explorer/set-explorer.component.scss)
Added `.card-viewport` and `.card-row` styles. Removed pagination styles. All original card, skeleton, and empty-state styles preserved.

---

## Build Verification

| Build | Result |
|-------|--------|
| Server (`dotnet build`) | ✅ 0 errors, 0 warnings |
| Client (`ng build --production`) | ✅ Bundle generated (25s) |
