# LDraw Import Tool

Standalone console app that parses LDraw files, copies them to a server-accessible data folder, and imports colour/part metadata into the BMC database.

## User Review Required

> [!IMPORTANT]
> The `FABRIC` finish type (20 LDraw colours) needs a new `ColourFinish` seed entry.

> [!IMPORTANT]
> LDraw data files will be copied to a configurable path (`LDraw:DataPath` in appsettings, falls back to `LDRAW_DATA_PATH` env var, defaults to `./ldraw-data`). The BMC server can reference this same path at runtime for geometry file access.

## Proposed Changes

### Database Fix

#### [MODIFY] [BmcDatabaseGenerator.cs](file:///d:/source/repos/scheduler/BmcDatabaseGenerator/BmcDatabaseGenerator.cs)
- Add `FABRIC` seed entry to `ColourFinish` (sequence 10)

---

### Import Service (in BMC.LDraw)

#### [NEW] [LDrawImportService.cs](file:///d:/source/repos/scheduler/BMC.LDraw/Services/LDrawImportService.cs)

Pure service class taking `BMCContext` — does the actual import logic:

- **`ImportColoursAsync(string ldConfigPath)`** — parse → resolve `ColourFinish` FKs → upsert `BrickColour` rows
- **`ImportPartHeadersAsync(string partsDir)`** — enumerate .dat files → resolve `PartType` + `BrickCategory` FKs → upsert `BrickPart` rows in batches
- **`CopyDataFiles(string sourcePath, string destPath)`** — copies LDConfig.ldr and parts/ to the server data folder

---

### Console App

#### [NEW] [BMC.LDraw.Import/](file:///d:/source/repos/scheduler/BMC.LDraw.Import/)

Standalone .NET 10 console app. References `BMC.LDraw` and `BmcDatabase`.

| File | Purpose |
|------|---------|
| `BMC.LDraw.Import.csproj` | Console app project |
| `Program.cs` | Entry point with CLI args |
| `appsettings.json` | DB connection string + LDraw paths |

**Usage:**
```
BMC.LDraw.Import --source d:\ldraw --import colours parts --copy-data
```

**CLI Arguments:**
| Arg | Description |
|-----|-------------|
| `--source` | Path to LDraw library (e.g. `d:\ldraw`) |
| `--import` | What to import: `colours`, `parts`, or both |
| `--copy-data` | Copy LDraw files to the configured data path |

---

### Server Configuration

#### [MODIFY] [appsettings.json](file:///d:/source/repos/scheduler/BMC/BMC.Server/appsettings.json)
- Add `LDraw` section with `DataPath` pointing to the copied data directory

```json
"LDraw": {
    "DataPath": "./ldraw-data"
}
```

## Verification Plan

### Build
- `dotnet build BMC.LDraw.Import/BMC.LDraw.Import.csproj`

### Functional Test
- Run the console app against `d:\ldraw`
- Verify 322 colours and 23K+ parts imported
- Verify data files copied to target directory
