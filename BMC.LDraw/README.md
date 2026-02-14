# BMC.LDraw

Pure C# parser library for the [LDraw](https://www.ldraw.org/) file format — the open standard for LEGO CAD models.

## Purpose

Reads LDraw format files and outputs strongly-typed POCOs. Has zero database dependencies — a downstream importer service maps these objects to EF Core entities.

## Capabilities

| Parser | Input | Output |
|--------|-------|--------|
| `ColourConfigParser` | `LDConfig.ldr` | `List<LDrawColour>` — all ~215 standard colours |
| `PartHeaderParser` | `.dat` file | `LDrawPartHeader` — title, author, category, keywords |
| `ModelParser` | `.ldr` / `.mpd` file | `LDrawModel` — steps with placed part references |

## Usage

```csharp
// Parse all colours
var colours = ColourConfigParser.ParseFile(@"d:\ldraw\LDConfig.ldr");

// Parse a single part header
var header = PartHeaderParser.ParseFile(@"d:\ldraw\parts\3001.dat");

// Parse a model
var model = ModelParser.ParseFile(@"d:\ldraw\models\car.ldr");
```
