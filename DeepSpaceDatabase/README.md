# DeepSpaceDatabase

Runtime database layer for the DeepSpace storage management module.

This project provides the EF Core context, entity classes, migration utilities, and SQLite lifecycle management for DeepSpace's metadata database.

## Architecture

Follows the RollerOps `LocalDatabaseStructure` pattern:

1. **`DeepSpaceDatabaseManager`** — Orchestrates database lifecycle (create, migrate, validate, tune)
2. **`DeepSpaceMigration`** — Schema creation from `DeepspaceDatabaseGenerator` + in-place migration
3. **`SqliteWALInterceptor`** — WAL mode + PRAGMA tuning for concurrency/performance
4. **`DeepSpaceContextCustom`** — Handwritten partial class extending the generated EF context

## Primary Database Target

**SQLite** — DeepSpace is designed to run self-contained with no SQL Server or PostgreSQL dependency.

## Entity Scaffolding

Entity classes and `DeepSpaceContext.cs` are scaffolded by FoundationCoreTools from `DeepspaceDatabaseGenerator` output. Only `DeepSpaceContextCustom.cs` is handwritten.
