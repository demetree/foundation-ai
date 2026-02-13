# SchedulerDatabaseGenerator — Overview

SchedulerDatabaseGenerator is a **.NET Standard 2.1** class library that defines the complete Scheduler database schema using the Foundation code generation DSL.

---

## Contents

| File | Size | Purpose |
|------|------|---------|
| `SchedulerDatabaseGenerator.cs` | ~205 KB | Complete schema definition for all Scheduler database tables |
| `Scripts/` | 8 files | Generated SQL scripts ready for execution |

---

## Purpose

This is the **single source of truth** for the Scheduler database schema. The class file defines every table, column, data type, constraint, foreign key, and index using CodeGenerationCommon's `DatabaseGenerator` fluent API.

When changes are needed:
1. Modify `SchedulerDatabaseGenerator.cs`
2. Run `SchedulerTools` to regenerate SQL scripts and source code
3. Apply scripts to SQL Server
4. Run EF Core Power Tools to update `SchedulerDatabase`

---

## Domain Description

The Scheduler system is a purpose-built, multi-tenant resource scheduling system designed for construction operations but flexible for any domain. Key capabilities:
- Scheduled events with arbitrary duration and resource/crew assignment
- Persistent crew definitions for one-click team assignment
- Partial-assignment support for shift hand-offs
- Resource availability management (blackout periods, shifts)
- Full change history and multi-tenant data isolation

---

## Dependencies

- `CodeGenerationCommon` — `DatabaseGenerator` base class

## Referenced By

- `SchedulerTools` — invokes this generator to produce scripts and code
