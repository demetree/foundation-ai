# CodeGenerationCore — Overview

CodeGenerationCore is a **.NET 10** class library that extends CodeGenerationCommon with generators for producing application source code from database definitions.

---

## Contents

| File | Size | Output |
|------|------|--------|
| `WebAPICodeGenerator.cs` | ~419 KB | Auto-generated `DataControllers/` — full CRUD controllers with filtering, pagination, security |
| `WebAPIBaseClassCodeGenerator.cs` | ~279 KB | Base controller classes |
| `AngularComponentGenerator.cs` | ~250 KB | Auto-generated Angular CRUD components (`*-data-components/`) |
| `AngularServiceGenerator.cs` | ~108 KB | Auto-generated Angular HTTP services (`*-data-services/`) |
| `EntityExtensionGenerator.cs` | ~96 KB | Entity partial class extensions |
| `AngularAutomationUtility.cs` | ~13 KB | Helpers for Angular module registration automation |
| `CodeGeneratorUtility.cs` | ~6 KB | Shared generator utilities |
| `DeploymentUtility.cs` | ~6 KB | File deployment helpers (copy generated files to target directories) |

---

## What It Generates

For each entity defined in a `DatabaseGenerator` subclass, this library produces:

| Output | Target Folder |
|--------|--------------|
| C# WebAPI Controller | `*/DataControllers/` |
| C# Entity Extension | `*/EntityExtensions/` |
| Angular Service (TypeScript) | `*/scheduler-data-services/` (or equivalent) |
| Angular Component (TypeScript/HTML/SCSS) | `*/scheduler-data-components/` (or equivalent) |

> [!CAUTION]
> All output folders are **auto-generated** and will be overwritten. Never manually edit files in these folders.

---

## Usage

These generators are invoked by tool projects:
- `SchedulerTools` — generates Scheduler code
- `AlertingTools` — generates Alerting code
- `FoundationCoreTools` — generates Foundation code
