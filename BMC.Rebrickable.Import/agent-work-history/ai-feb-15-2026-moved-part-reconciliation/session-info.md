# Session Information

- **Conversation ID:** d67b0fbf-9243-46da-b32d-fb84c8a1674b
- **Date:** 2026-02-15
- **Time:** 11:09 NST (UTC-3:30)
- **Duration:** ~15 minutes

## Summary

Implemented moved part reconciliation in the BMC Rebrickable Import pipeline. Parts with LDraw `~Moved to X` titles are now resolved to their actual destination parts during import, and a standalone `reconcile` import target was added to fix already-imported data.

## Files Modified

- `BMC.Rebrickable.Import/RebrickableImportService.cs` — Added `BuildMovedPartMapAsync()`, `ReconcileMovedPartsAsync()`, `ReconcileResult` class; integrated moved part resolution into `ImportElementsAsync()` and `ImportInventoryPartsAsync()`
- `BMC.Rebrickable.Import/Program.cs` — Added `reconcile` import target and help text

## Related Sessions

- **Parts Catalog Performance Tuning** (daba13f4) — Identified moved parts as a data quality issue during catalog browsing
