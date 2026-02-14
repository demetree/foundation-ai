# BMC.LDraw — Library & Import Tool Walkthrough

## What Was Built

### 1. BMC.LDraw Parser Library
Standalone .NET 10 library (zero DB dependencies) that parses LDraw format files into POCOs.

```
BMC.LDraw/
├── Models/    — LDrawColour, LDrawPartHeader, LDrawSubfileReference, LDrawModel/LDrawStep
└── Parsers/   — ColourConfigParser, PartHeaderParser, ModelParser
```

**Parser test results (11/11 passed):**
- 322 colours parsed across 10 finish types (Solid, Transparent, Chrome, Pearlescent, Metal, Rubber, Glitter, Speckle, Milky, Fabric)
- Part header parsing correct (Brick 2x4: title, author, category, part type)
- Model parsing correct (car.ldr: 8 steps, 61 parts)

---

### 2. BMC.LDraw.Import Console Tool
CLI tool that imports LDraw data into the BMC database.

```
BMC.LDraw.Import/
├── Program.cs               — CLI arg parsing + config
├── LDrawImportService.cs    — Upsert logic + data file copying
└── appsettings.json         — DB connection + LDraw data path
```

**Usage:**
```bash
# Import everything and copy data files
BMC.LDraw.Import --source d:\ldraw --import colours parts --copy-data

# Import just colours
BMC.LDraw.Import --source d:\ldraw --import colours
```

**Key features:**
- Upsert logic — idempotent, safe to re-run
- FK resolution: `ColourFinish`, `PartType`, `BrickCategory` by name match
- Batch saves (500 parts per batch) for performance
- Data file copy: LDConfig.ldr, parts/*.dat, primitives/

---

### 3. Schema Addition
- Added `Fabric` finish type to `ColourFinish` seed data in [BmcDatabaseGenerator.cs](file:///d:/source/repos/scheduler/BmcDatabaseGenerator/BmcDatabaseGenerator.cs)
- Added `LDraw:DataPath` to BMC.Server [appsettings.json](file:///d:/source/repos/scheduler/BMC/BMC.Server/appsettings.json)

## Files Changed

| File | Change |
|------|--------|
| [BMC.LDraw.csproj](file:///d:/source/repos/scheduler/BMC.LDraw/BMC.LDraw.csproj) | New parser library project |
| [BMC.LDraw.Import.csproj](file:///d:/source/repos/scheduler/BMC.LDraw.Import/BMC.LDraw.Import.csproj) | New console import tool |
| [BmcDatabaseGenerator.cs](file:///d:/source/repos/scheduler/BmcDatabaseGenerator/BmcDatabaseGenerator.cs) | Added Fabric ColourFinish seed |
| [appsettings.json](file:///d:/source/repos/scheduler/BMC/BMC.Server/appsettings.json) | Added LDraw:DataPath config |

## Build Verification
All three projects compile successfully with `dotnet build`.

## Next Steps
1. Run the import tool against a live BMC database
2. Add BMC.LDraw reference to BMC.Server for runtime geometry access
3. Build LDraw geometry renderer for the web UI
