# Rebrickable-Led Auto-Bootstrap Import Service

The `DataImportWorker` already supports Rebrickable-first importing with FK-ordered CSV loading, but it requires `DataImport:Enabled = true` in config. A fresh BMC.Server deployment currently starts with empty tables and no catalog data unless someone manually enables the import and waits for it.

**Goal**: Make BMC.Server self-bootstrapping — on first startup with an empty database, automatically run a full Rebrickable + LDraw import without requiring config changes.

## Proposed Changes

### Config & Registration

#### [MODIFY] [appsettings.json](file:///g:/source/repos/Scheduler/BMC/BMC.Server/appsettings.json)
- Change `DataImport:Enabled` default to `true` (always run the worker)
- Add `DataImport:AutoBootstrap` setting (default `true`) — triggers immediate full import when BrickPart table is empty

#### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Program.cs)
- Remove the `if (Enabled)` guard — always register `DataImportWorker` as a hosted service
- The worker itself will check config and database state to decide what to do

---

### Worker Logic

#### [MODIFY] [DataImportWorker.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/DataImportWorker.cs)

**First-run auto-bootstrap detection:**
1. On startup, before the normal hourly loop, check if `AutoBootstrap` is enabled (default: `true`)
2. Query `BrickParts.CountAsync()` — if 0 (or below a threshold like 100), this is a fresh deployment
3. If empty: immediately run a **full import cycle** (all Rebrickable CSVs downloaded regardless of timestamps, then LDraw)
4. Log prominently: `"[DataImportWorker] Empty database detected — running first-time bootstrap import"`
5. After bootstrap completes, continue into the normal hourly check loop

**Import order stays Rebrickable-first (correct):**
1. Rebrickable themes → colours → categories → parts → relationships → elements → sets → minifigs → inventories
2. LDraw Official (colours from LDConfig.ldr, part headers from parts/*.dat, copy data files)
3. LDraw Unofficial (optional)

**Force-download on first run:**
- When `state.Timestamps` is empty (no state file), the existing logic already downloads everything. So first-run bootstrap is essentially "delete state file if exists + run cycle". But we add explicit `isFirstRun` flag for clear logging.

## Verification Plan

### Automated Tests
- Build `BMC.Server` to ensure no compilation errors
- Verify the worker starts and logs "ready to receive" when DataImport tables exist
- Verify the worker logs "bootstrap" when tables are empty

### Manual Verification
- Start BMC.Server with `DataImport:Enabled = true` and an empty BrickPart table → confirm it auto-downloads and imports
