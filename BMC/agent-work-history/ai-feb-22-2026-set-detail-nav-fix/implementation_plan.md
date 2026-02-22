# Fix Set-Detail Minifig & Subset Navigation

The set-detail component shows **zero minifigs** and **non-functional subset navigation** because of two bugs in the data layer.

## Root Causes

### Bug 1: `ReviveLegoSet` — Mismatched Subset Cache Names

The `ReviveLegoSet` method initializes caches with **wrong property names**:

```diff
 // ReviveLegoSet (line 1243)
-(revived as any)._legoSetSubsets = null;
-(revived as any)._legoSetSubsetsPromise = null;
-(revived as any)._legoSetSubsetsSubject = new BehaviorSubject(null);
+// Need TWO pairs → ParentLegoSets and ChildLegoSets
+(revived as any)._legoSetSubsetParentLegoSets = null;
+(revived as any)._legoSetSubsetParentLegoSetsPromise = null;
+(revived as any)._legoSetSubsetParentLegoSetsSubject = new BehaviorSubject(null);
+(revived as any)._legoSetSubsetChildLegoSets = null;
+(revived as any)._legoSetSubsetChildLegoSetsPromise = null;
+(revived as any)._legoSetSubsetChildLegoSetsSubject = new BehaviorSubject(null);
```

Same issue with the observable re-attachment (lines 1299-1311). The Revive creates `LegoSetSubsets$` / `loadLegoSetSubsets()` but the class defines `LegoSetSubsetParentLegoSets$` + `LegoSetSubsetChildLegoSets$` / `loadLegoSetSubsetParentLegoSets()` + `loadLegoSetSubsetChildLegoSets()`. Since these names don't match, the getters on the revived object find `null` private caches, call the prototype `loadX()` which writes to the correctly-named fields, but the observables never emit because they reference the wrong subject.

### Bug 2: Subset Getter in `set-detail.component.ts`

The component uses the wrong getter:

```diff
-this.subsets = await this.set.LegoSetSubsetChildLegoSets;
+this.subsets = await this.set.LegoSetSubsetParentLegoSets;
```

`ChildLegoSets` queries `childLegoSetId = this.id` ("which parent sets contain me?"), but we want `ParentLegoSets` which queries `parentLegoSetId = this.id` ("show me this set's subsets").

### Bug 3 (Likely): Minifig data returning empty

The code path for minifigs looks correct — the `ReviveLegoSet` method's minifig names match the class definitions. But the user reports zero minifigs even though data exists (confirmed by inverse query working from minifig gallery). Most likely explanation: the `LegoSetMinifigService.listCache` has a poisoned cache entry (from during/after the database rebuild) that still replays an empty array via `shareReplay`. A page reload should fix this, but if not, the issue may be server-side (the controller returns `active=true` records, and the rebuild may not set `active=true` on junction records). I'll add a diagnostic console log to confirm.

## Proposed Changes

### Data Layer

#### [MODIFY] [lego-set.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/bmc-data-services/lego-set.service.ts)

Fix `ReviveLegoSet` method (lines 1243-1311):
- Replace `_legoSetSubsets` with two pairs: `_legoSetSubsetParentLegoSets` and `_legoSetSubsetChildLegoSets`
- Replace `LegoSetSubsets$` observable with `LegoSetSubsetParentLegoSets$` and `LegoSetSubsetChildLegoSets$`
- Replace `loadLegoSetSubsets()` calls with `loadLegoSetSubsetParentLegoSets()` and `loadLegoSetSubsetChildLegoSets()`
- Replace `LegoSetSubsetsCount$` with both `LegoSetSubsetParentLegoSetsCount$` and `LegoSetSubsetChildLegoSetsCount$`

---

### Set Detail Component

#### [MODIFY] [set-detail.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/set-detail/set-detail.component.ts)

- Change subset loading from `LegoSetSubsetChildLegoSets` → `LegoSetSubsetParentLegoSets`
- Add temporary diagnostic `console.log` in the minifig loading path to surface any silent failures

## Verification Plan

### Automated Tests
- `ng build --configuration production` — confirm clean compilation

### Manual Verification
- Navigate to set 76463-1 and verify minifig count appears
- Verify minifig tab shows cards with names/images
- Verify clicking a minifig card navigates to minifig detail
- Verify subset tab loads correctly (if set has subsets)
