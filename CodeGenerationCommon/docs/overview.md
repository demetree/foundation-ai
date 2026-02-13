# CodeGenerationCommon — Overview

CodeGenerationCommon is a **.NET Standard 2.1** class library providing the lowest-level base classes for the Foundation code generation system.

---

## Contents

| File | Size | Purpose |
|------|------|---------|
| `DatabaseGenerator.cs` | ~400 KB | Core database schema definition DSL — defines table structures, columns, data types, relationships, and constraints using a fluent C# API |
| `CodeGenerationBase.cs` | ~11 KB | Base class for all code generators, providing shared infrastructure (file I/O, template rendering, naming conventions) |
| `Utility.cs` | ~1.5 KB | Small utility helpers for code generation |

---

## Key Concept: DatabaseGenerator

`DatabaseGenerator.cs` is the heart of the Foundation code generation system. It provides a fluent API for defining database schemas in C#:

- Table definitions with column types, constraints, and defaults
- Foreign key relationships
- Index definitions
- Change history table auto-generation
- SQL script output

Each application (Scheduler, Alerting, Security, Auditor, Telemetry) has its own **DatabaseGenerator subclass** (in a separate project) that defines its specific tables using this base API.

---

## Dependency Chain

```
Application Tools (SchedulerTools, AlertingTools, FoundationCoreTools)
    └── CodeGenerationCore (Angular/WebAPI generators)
        └── CodeGenerationCommon (this project — base classes + DatabaseGenerator)
```
