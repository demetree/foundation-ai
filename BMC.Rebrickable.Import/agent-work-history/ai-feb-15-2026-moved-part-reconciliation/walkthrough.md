# Moved Part Reconciliation — Walkthrough

## Problem
Rebrickable set inventories reference LDraw parts that have been "moved" — e.g. part `10` with title `~Moved to 10a`. The import pipeline was linking sets to these placeholder parts, causing the "My Catalog" UI to display unhelpful descriptions.

## Changes Made

### [RebrickableImportService.cs](file:///d:/source/repos/scheduler/BMC.Rebrickable.Import/RebrickableImportService.cs)

**New: `BuildMovedPartMapAsync()`** — Queries all `BrickPart` rows where `ldrawTitle` starts with `~Moved to `, parses the destination part ID, resolves it against the database, and follows chains (A→B→C becomes A→C). Returns a `Dictionary<int, int>` mapping moved part IDs to their final destination.

**Modified: `ImportElementsAsync()`** — After resolving a part ID from `partLookup`, checks the moved part map and redirects to the actual destination. Logs reconciliation count.

**Modified: `ImportInventoryPartsAsync()`** — Same pattern — redirects moved part references during inventory import.

**New: `ReconcileMovedPartsAsync()`** — Standalone method that scans existing `LegoSetPart` and `BrickElement` rows for moved part references and fixes them in-place. Idempotent — safe to re-run.

**New: `ReconcileResult`** — Counter class for the reconcile operation.

### [Program.cs](file:///d:/source/repos/scheduler/BMC.Rebrickable.Import/Program.cs)

Added `reconcile` as a new import target. This runs `ReconcileMovedPartsAsync()` to fix already-imported data without requiring a full re-import.

## How to Run

Fix existing data:
```
BMC.Rebrickable.Import --source <csv-folder> --import reconcile
```

Full import (now includes reconciliation automatically):
```
BMC.Rebrickable.Import --download --import all
```

## Verification

Build passes with **0 errors**. To validate the data fix, run this SQL before and after:
```sql
SELECT COUNT(*) FROM bmc.LegoSetPart lsp
JOIN bmc.BrickPart bp ON lsp.brickPartId = bp.id
WHERE bp.ldrawTitle LIKE '~Moved to%'
```
