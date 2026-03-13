# Walkthrough: Seed MOC From Steps

## What Was Built

An admin-only testing tool that takes an existing BMC project and publishes it to MOCHub where **each building step becomes a separate version**. This creates realistic test data for the version diff viewer, 3D diff preview, and version management UI.

## Changes Made

### Server — [MocHubController.cs](file:///g:/source/repos/Scheduler/BMC/BMC.Server/Controllers/MocHubController.cs)

`POST /api/mochub/admin/seed-from-steps` endpoint:
- **Auth**: Admin-only (`DoesUserHaveAdminPrivilegeSecurityCheckAsync`)
- **Input**: `{ projectId: int }`
- **Process**:
  1. Loads the project's `ModelDocument → ModelSubFile → ModelBuildStep → ModelStepPart` hierarchy
  2. Identifies the main model's distinct step numbers
  3. Creates a `PublishedMoc` tagged `[TEST]` with `Unlisted` visibility
  4. For each step N, builds a cumulative LDraw MPD (all parts from steps 1..N) and creates a `MocVersion` with diff stats
- **Output**: `{ publishedMocId, slug, versionCount, totalParts }`

Also added the `AppendPartLine` helper: reconstructs LDraw Type 1 lines from `ModelStepPart` data (position + 3×3 transform matrix + part filename).

### Client — mochub-explore component

| File | Change |
|------|--------|
| [TS](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/mochub-explore/mochub-explore.component.ts) | `isAdmin` getter (→ `isBMCAdministrator`), seed form state, `seedMocFromSteps()` method |
| [HTML](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/mochub-explore/mochub-explore.component.html) | Admin-only toggle button + inline seed form with project ID input, result display, error state |
| [SCSS](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/mochub-explore/mochub-explore.component.scss) | Purple-themed seed form styles matching the dark UI aesthetic |

## Build Verification

| Target | Status |
|--------|--------|
| Server (`dotnet build`) | ✅ Clean (exit code 0) |
| Client (`ng build --configuration production`) | ✅ Clean — only pre-existing `.form-floating>~label` CSS warning from third-party dependency |
