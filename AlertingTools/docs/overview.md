# AlertingTools — Overview

AlertingTools is a **.NET 10** console application that orchestrates code generation for the Alerting module.

---

## Contents

| File | Size | Purpose |
|------|------|---------|
| `Program.cs` | ~9 KB | Orchestrates code generators for the Alerting database definition |

---

## What It Produces

| Output | Target |
|--------|--------|
| SQL database scripts | `DatabaseGenerators/AlertingDatabaseGenerator/Scripts/` |
| C# DataControllers | `Alerting/Alerting.Server/DataControllers/` |
| C# Entity Extensions | `AlertingDatabase/EntityExtensions/` |
| Angular data services | `Alerting/Alerting.Client/src/app/alerting-data-services/` |
| Angular data components | `Alerting/Alerting.Client/src/app/alerting-data-components/` |

---

## Usage

```powershell
cd AlertingTools
dotnet run
```

---

## Dependencies

- `CodeGenerationCore` — code generators
- `CodeGenerationCommon` — `DatabaseGenerator` base
- `DatabaseGenerators/AlertingDatabaseGenerator` — Alerting schema definition

---

## Related Documentation

For the full Alerting system architecture, see [Alerting/docs/ARCHITECTURE.md](file:///d:/source/repos/scheduler/Alerting/docs/ARCHITECTURE.md).
