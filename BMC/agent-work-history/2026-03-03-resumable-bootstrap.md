# Resumable Bootstrap for DataImportWorker

**Date:** 2026-03-03

## Summary

Hardened the `DataImportWorker` bootstrap process so it can recover from interruptions (e.g., killing the dev server mid-import) without requiring a full database delete and recreation. The bootstrap now tracks per-step completion and resumes from where it left off.

## Changes Made

- **`BMC/BMC.Server/Services/DataImportWorker.cs`**
  - Replaced `IsDatabaseEmptyAsync` (naive `BrickParts.Count() < 100` check) with a `BootstrapComplete` flag in the persisted state file — only set `true` after all 14 import steps complete
  - Added `CompletedSteps` (HashSet) to `ImportState` for per-step completion tracking
  - Added `RunTrackedStepAsync` helper that skips already-completed steps during bootstrap and saves state after each step (checkpoint pattern)
  - Updated `CheckAndImportRebrickableAsync` — each of the 12 Rebrickable import steps is individually tracked
  - Updated `CheckAndImportLDrawAsync` — LDraw Official and Unofficial tracked as separate steps with bootstrap-aware skip logic
  - Added resume-aware download logic: during bootstrap resume, cached CSV files are reused rather than re-downloaded
  - Added `isBootstrap` parameter throughout the call chain to distinguish bootstrap from normal hourly cycles
  - Normal hourly cycle behavior is unchanged (no step gating, no per-step saves)

## Key Decisions

- **State-file-based detection over DB row counting** — the old `BrickParts.Count() < 100` check was unreliable because partial imports could exceed the threshold. The `BootstrapComplete` flag is explicit and unambiguous.
- **Per-step granularity** — each of the 14 import steps (12 Rebrickable + 2 LDraw) is tracked individually, giving good resume precision without over-engineering.
- **No DB schema changes** — all tracking is via the existing `import-state.json` file, keeping the change self-contained.
- **Idempotent imports are key** — both `RebrickableImportService` and `LDrawImportService` use `BulkInsertOrUpdate`, so re-running an interrupted step is safe. This allowed us to keep the design simple.

## Testing / Verification

- `dotnet build BMC\BMC.Server\BMC.Server.csproj` — **0 errors**, 115 pre-existing warnings (unrelated)
- Manual testing flow: delete `import-state.json` → start server → kill mid-import → restart → verify `BOOTSTRAP RESUME` logs show skipped steps and remaining steps run
