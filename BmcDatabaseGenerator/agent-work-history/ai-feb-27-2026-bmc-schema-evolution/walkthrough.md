# Auto-Bootstrap DataImportWorker — Walkthrough

## Goal

Make BMC.Server self-bootstrapping: on first startup with an empty database, automatically download and import all Rebrickable + LDraw data without requiring manual configuration.

## Changes Made

### [appsettings.json](file:///g:/source/repos/Scheduler/BMC/BMC.Server/appsettings.json)

```diff
 "DataImport": {
-    "Enabled": false,
+    "Enabled": true,
+    "AutoBootstrap": true,
     "IntervalMinutes": 60,
```

### [Program.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Program.cs)

Removed the conditional `if (Enabled)` guard — worker is now always registered. It handles its own enabled/disabled logic internally.

### [DataImportWorker.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Services/DataImportWorker.cs)

**New startup flow:**
1. Check `DataImport:Enabled` internally — graceful no-op when disabled
2. Check `DataImport:AutoBootstrap` (default `true`)
3. Query `BrickParts.Count()` — if < 100, this is a fresh deployment
4. If empty → immediate full import cycle (Rebrickable CSVs → LDraw data)
5. Enter normal hourly check loop (delay-first to prevent double-import)

**New method:** `IsDatabaseEmptyAsync()` — checks BrickParts count with threshold of 100.

**Loop timing change:** moved `Task.Delay` to top of loop (delay-first), so after bootstrap the worker waits the full interval before its first hourly check.

## Verification

| Check | Result |
|---|---|
| `dotnet build BMC.Server` | ✅ Build succeeded |
