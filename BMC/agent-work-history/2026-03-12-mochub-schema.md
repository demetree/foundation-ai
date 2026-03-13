# MOCHub Database Schema — Phase 1

**Date:** 2026-03-12

## Summary

Designed and implemented the MOCHub database schema — a GitHub-inspired collaborative MOC publishing platform with version control, forking, and collaboration. Added new tables and columns to `BmcDatabaseGenerator.cs` to support the full MOCHub feature set.

## Changes Made

- **`BmcDatabaseGenerator/BmcDatabaseGenerator.cs`** — Enhanced `PublishedMoc` table and added 3 new tables:
  - Added 7 new columns to `PublishedMoc`: `visibility` (Public/Private/Unlisted), `forkCount`, `forkedFromMocId` (self-referencing FK), `licenseName`, `readmeMarkdown`, `slug` (unique per tenant), `defaultBranchName`
  - **`MocVersion`** — Version snapshots (commits) storing full MPD text content per version, with part count deltas and author tracking
  - **`MocFork`** — Fork lineage graph with FKs to source and forked MOCs, plus the version forked from
  - **`MocCollaborator`** — Shared access control (Read/Write/Admin) with invitation tracking
  - 3 new `ActivityEventType` seeds: `CommittedVersion`, `ForkedMoc`, `AddedCollaborator`

## Key Decisions

- **Full MPD snapshots** per version (not diffs) — MPD files are small (~50-200KB), so full snapshots are simple and reliable
- **No `currentVersionId` FK** on PublishedMoc — the current version is derived by querying `MAX(versionNumber)` to avoid circular FK between PublishedMoc ↔ MocVersion
- **No branching/PR model** — deemed overkill for LEGO MOCs. Linear version history + forking + collaborators covers all real-world AFOL workflows
- **`defaultBranchName`** field reserved for potential future branching, but not actively used
- **Slug-based URLs** for clean `/mochub/:username/:slug` routing

## Testing / Verification

- `dotnet build BmcDatabaseGenerator/BmcDatabaseGenerator.csproj` — **0 errors, 0 warnings**
- User manually rescaffolded after schema changes to regenerate all code-gen output

## Phase 2 — Server Services & Controller

### Changes Made

- **`BMC.Server/Services/MocVersioningService.cs`** [NEW] — Core version control logic:
  - `CreateSnapshotAsync` — generates MPD via `ModelExportService`, computes part count deltas, stores as `MocVersion`
  - `ComputeDiffSummaryAsync` — parses LDraw type 1 lines from two snapshots, diffs added/removed parts
  - `ForkMocAsync` — deep clones Project, PlacedBricks, ModelDocument; creates new PublishedMoc + MocFork + initial version
  - `GenerateSlug` — URL-friendly slug generation with uniqueness enforcement

- **`BMC.Server/Controllers/MocHubController.cs`** [NEW] — 10 endpoints:
  - Public: `explore`, `explore/search`, `moc/{id}`, `moc/{id}/versions`, `moc/{id}/versions/{v}/diff`, `moc/{id}/forks`
  - Authenticated: `publish`, `moc/{id}/commit`, `moc/{id}/fork`, `moc/{id}` (PUT)

- **`BMC.Server/Program.cs`** — Registered `MocVersioningService` (scoped) and `MocHubController` in controller list

- **`BmcDatabaseGenerator.cs`** — Added `AddVersionControl()` to `MocVersion` and `MocCollaborator` tables

### Testing / Verification

- `dotnet build BMC/BMC.Server/BMC.Server.csproj` — **0 errors** (exit code 0)
