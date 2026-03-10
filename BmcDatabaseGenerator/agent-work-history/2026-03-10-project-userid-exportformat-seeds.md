# Schema Updates — userId on Project, ExportFormat Seed Data

**Date:** 2026-03-10

## Summary

Added `userId` field to the `Project` table for explicit user ownership, and added BrickLink Studio (`.io`) and STL (`.stl`) to the `ExportFormat` seed data.

## Changes Made

- **Modified** `BmcDatabaseGenerator.cs`:
  - Added `userId` (nullable int) to `Project` table after `AddMultiTenantSupport()` — cross-database reference to `SecurityUser.id`
  - Added `ExportFormat` seed: BrickLink Studio (`.io`, sequence 8, guid `ef100001-...-000000000008`)
  - Added `ExportFormat` seed: STL (`.stl`, sequence 9, guid `ef100001-...-000000000009`)

## Key Decisions

- `userId` is nullable to support legacy projects that predate the field
- Uses `AddIntField` rather than `AddForeignKeyField` since `SecurityUser` is in a separate database (cross-DB FKs not supported by SQL Server)

## Testing / Verification

- `dotnet build BmcDatabaseGenerator/BmcDatabaseGenerator.csproj` — ✅ 0 errors, 0 warnings
- User rescaffolded with EF Core Power Tools — confirmed `Project.userId` (int?) on generated entity
