# SchedulerTools — Overview

SchedulerTools is a **.NET 10** console application that orchestrates code generation for the Scheduler module.

---

## Contents

| File | Size | Purpose |
|------|------|---------|
| `Program.cs` | ~50 KB | Orchestrates all code generators for the Scheduler database definition |

---

## What It Produces

When run, SchedulerTools generates:

| Output | Generator | Target |
|--------|-----------|--------|
| SQL database scripts | `DatabaseGenerator` | `SchedulerDatabaseGenerator/Scripts/` |
| C# DataControllers | `WebAPICodeGenerator` | `Scheduler.Server/DataControllers/` |
| C# Entity Extensions | `EntityExtensionGenerator` | `SchedulerDatabase/EntityExtensions/` |
| Angular data services | `AngularServiceGenerator` | `Scheduler.Client/src/app/scheduler-data-services/` |
| Angular data components | `AngularComponentGenerator` | `Scheduler.Client/src/app/scheduler-data-components/` |

---

## Usage

```powershell
cd SchedulerTools
dotnet run
```

Follow the interactive menu to run specific generators or run all at once.

> [!IMPORTANT]
> After running code generation, you must:
> 1. Apply any new SQL scripts to your database
> 2. Run EF Core Power Tools to regenerate `SchedulerDatabase/Database/`
> 3. Register any new controllers in `Scheduler.Server/Program.cs`
> 4. Register any new Angular components in `app.module.ts`

---

## Dependencies

- `CodeGenerationCore` — all code generators
- `CodeGenerationCommon` — `DatabaseGenerator` base
- `SchedulerDatabaseGenerator` — Scheduler schema definition
