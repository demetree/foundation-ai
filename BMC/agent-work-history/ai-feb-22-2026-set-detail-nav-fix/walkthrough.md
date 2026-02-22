# Set-Detail Navigation Fix — Walkthrough

## Changes Made

### 1. Fixed `ReviveLegoSet` Subset Cache Names
**File:** [lego-set.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/bmc-data-services/lego-set.service.ts)

The code generator produced a `ReviveLegoSet` method with a single `_legoSetSubsets` cache, but the `LegoSetData` class defines **two** subset relationships:
- `_legoSetSubsetParentLegoSets` — "what subsets does this set contain?"
- `_legoSetSubsetChildLegoSets` — "which sets contain this set?"

The revival method now correctly initializes both pairs of caches, observables, loaders, and count queries.

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/bmc-data-services/lego-set.service.ts)

### 2. Fixed Subset Getter Direction
**File:** [set-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.ts)

Changed from `LegoSetSubsetChildLegoSets` (= "which sets contain me?") to `LegoSetSubsetParentLegoSets` (= "show my subsets").

### 3. Added Minifig Diagnostic Logging
Added `console.log` in the minifig loading path to surface why zero records are returned despite data existing (confirmed by inverse query from minifig gallery working).

render_diffs(file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.ts)

## Verification
- **Production build:** ✅ Passed cleanly (`ng build --configuration production`)

## Outstanding
- **Minifig zero-results:** The console log will help diagnose this at runtime. If the API call returns 0 results despite data existing in the junction table, check whether a stale `shareReplay` cache is replaying an old empty result (try hard-refreshing the page), or whether the `active` flag on junction records is incorrectly set after the database rebuild.
- **Code generator:** The `ReviveLegoSet` template should be updated to handle dual-key subset relationships to prevent regression on regeneration.
