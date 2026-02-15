# Moved Part Reconciliation

- [x] Add `BuildMovedPartMapAsync()` method to `RebrickableImportService`
  - Query BrickPart rows with `~Moved to` titles
  - Parse destination part IDs and follow chains
  - Return `Dictionary<int, int>` mapping moved → destination
- [x] Integrate moved part map into `ImportInventoryPartsAsync()`
- [x] Integrate moved part map into `ImportElementsAsync()`
- [x] Add `ReconcileMovedPartsAsync()` method for fixing existing data
- [x] Add `reconcile` import target in `Program.cs`
- [x] Build and verify — 0 errors
